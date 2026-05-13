using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kaleidoscope2.Core
{
    [DisallowMultipleComponent]
    public sealed class KaleidoscopeDirector : MonoBehaviour
    {
        [SerializeField] private KaleidoscopeState state = new KaleidoscopeState();
        [SerializeField] private bool tickModules = true;
        [SerializeField] private bool keepRunningInBackground = true;

        private readonly List<IKaleidoscopeModule> modules = new List<IKaleidoscopeModule>();
        private readonly List<KaleidoscopeModuleStatus> moduleStatuses = new List<KaleidoscopeModuleStatus>();
        private IKaleidoscopeSourceProvider sourceProvider;
        private readonly List<IKaleidoscopeTextureProcessor> textureProcessors = new List<IKaleidoscopeTextureProcessor>();
        private Texture finalOutputTexture;

        public event Action<KaleidoscopeCommand> CommandDispatched;

        public KaleidoscopeState State
        {
            get
            {
                EnsureState();
                return state;
            }
        }

        public IReadOnlyList<IKaleidoscopeModule> RegisteredModules
        {
            get { return modules; }
        }

        public Texture FinalOutputTexture
        {
            get { return finalOutputTexture; }
        }

        private void Awake()
        {
            EnsureState();
            if (keepRunningInBackground)
            {
                Application.runInBackground = true;
            }

            // Requirement: the render output should be clean (no on-screen diagnostics by default).
            state.SetDiagnosticsVisible(false);
        }

        private void Update()
        {
            if (!tickModules)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            state.TickImageReanimation(deltaTime);

            for (int index = 0; index < modules.Count; index++)
            {
                IKaleidoscopeModule module = modules[index];

                if (module != null && module.IsActive)
                {
                    module.Tick(deltaTime);
                }
            }

            RefreshRenderPipeline();
            RefreshModuleStatuses();
        }

        public bool RegisterModule(IKaleidoscopeModule module)
        {
            EnsureState();

            if (module == null)
            {
                state.ReportWarning("[Director] Ignored null module registration.");
                return false;
            }

            if (IsRegistered(module))
            {
                return false;
            }

            modules.Add(module);
            module.Initialize(state);
            RegisterRenderEndpoint(module);
            RefreshModuleStatuses();
            return true;
        }

        public void ClearRegisteredModules()
        {
            modules.Clear();
            sourceProvider = null;
            textureProcessors.Clear();
            finalOutputTexture = null;
            RefreshModuleStatuses();
        }

        public void ActivateAllModules()
        {
            for (int index = 0; index < modules.Count; index++)
            {
                if (modules[index] != null)
                {
                    modules[index].Activate();
                }
            }

            RefreshModuleStatuses();
        }

        public void DeactivateAllModules()
        {
            for (int index = 0; index < modules.Count; index++)
            {
                if (modules[index] != null)
                {
                    modules[index].Deactivate();
                }
            }

            RefreshModuleStatuses();
        }

        public void Dispatch(KaleidoscopeCommand command)
        {
            EnsureState();

            if (command == null)
            {
                state.ReportWarning("[Director] Ignored null command.");
                return;
            }

            bool handledByState = ApplyStateCommand(command);
            bool handledByModule = RouteCommandToModules(command);

            if (!handledByState && !handledByModule)
            {
                state.ReportWarning("[Director] Command was not handled: " + command.Type);
            }

            RefreshModuleStatuses();

            Action<KaleidoscopeCommand> handler = CommandDispatched;
            if (handler != null)
            {
                handler(command);
            }
        }

        public void ValidateSystem()
        {
            EnsureState();

            state.ClearMissingReferences();

            if (modules.Count == 0)
            {
                state.ReportWarning("[Director] No modules registered.");
            }

            for (int index = 0; index < modules.Count; index++)
            {
                IKaleidoscopeModule module = modules[index];

                if (module == null)
                {
                    state.ReportMissingReference("Director.RegisteredModules[" + index + "]");
                    continue;
                }

                module.Validate();
            }

            RefreshModuleStatuses();
        }

        private bool ApplyStateCommand(KaleidoscopeCommand command)
        {
            switch (command.Type)
            {
                case KaleidoscopeCommandType.SetSourceMode:
                    state.SetSourceMode(command.SourceModeValue);
                    return true;

                case KaleidoscopeCommandType.SetVisualMode:
                    state.SetVisualMode(command.VisualModeValue);
                    return true;

                case KaleidoscopeCommandType.SetMirrorCount:
                    state.MirrorSettings.SetMirrorCount(command.IntValue);
                    return true;

                case KaleidoscopeCommandType.SetMirrorRotation:
                    state.MirrorSettings.SetRotation(command.FloatValue);
                    return true;

                case KaleidoscopeCommandType.SetMirrorRotationSpeed:
                    state.MirrorSettings.SetRotationSpeed(command.FloatValue);
                    return true;

                case KaleidoscopeCommandType.SetMirrorZoom:
                    state.MirrorSettings.SetZoom(command.FloatValue);
                    return true;

                case KaleidoscopeCommandType.SetMirrorCenterOffset:
                    state.MirrorSettings.SetCenterOffset(command.Vector2Value);
                    return true;

                case KaleidoscopeCommandType.ToggleMirrorGuides:
                    state.MirrorSettings.SetGuidesVisible(!state.MirrorSettings.GuidesVisible);
                    return true;

                case KaleidoscopeCommandType.SetMirrorGuidesVisible:
                    state.MirrorSettings.SetGuidesVisible(command.BoolValue);
                    return true;

                case KaleidoscopeCommandType.SetMirrorRotationSpeedUnits:
                    state.MirrorSettings.SetRotationSpeed(command.FloatValue);
                    return true;

                case KaleidoscopeCommandType.SetMirrorForwardSpeedUnits:
                    state.MirrorSettings.SetForwardSpeedUnits(command.FloatValue);
                    return true;

                case KaleidoscopeCommandType.SetTunnelEnabled:
                    state.SetTunnelEnabled(command.BoolValue);
                    return true;

                case KaleidoscopeCommandType.SetTunnelBend:
                    state.TunnelSettings.SetBend(command.Vector2Value);
                    return true;

                case KaleidoscopeCommandType.SetTunnelHoseOpeningUnits:
                    state.TunnelSettings.SetHoseOpeningUnits(command.FloatValue);
                    return true;

                case KaleidoscopeCommandType.SetTunnelHoseWallCurvatureUnits:
                    state.TunnelSettings.SetHoseWallCurvatureUnits(command.FloatValue);
                    return true;

                case KaleidoscopeCommandType.ResetTunnelHoseProfile:
                    state.TunnelSettings.ResetHoseProfile();
                    return true;

                case KaleidoscopeCommandType.ToggleTunnelHoseChromaticAberration:
                    state.TunnelSettings.ToggleHoseChromaticAberration();
                    return true;

                case KaleidoscopeCommandType.SetTunnelHoseChromaticAberration:
                    state.TunnelSettings.SetHoseChromaticAberrationEnabled(command.BoolValue);
                    return true;

                case KaleidoscopeCommandType.SetFiveDFlightSpeedUnits:
                    state.FiveDSettings.SetFlightSpeedUnits(command.FloatValue);
                    return true;

                case KaleidoscopeCommandType.SetSevenDStrategy:
                    state.SevenDSettings.SetStrategyIndex(command.IntValue);
                    return true;

                case KaleidoscopeCommandType.CycleSevenDStrategy:
                    state.SevenDSettings.CycleStrategy(command.IntValue);
                    return true;

                case KaleidoscopeCommandType.SetVisualMotionFlightSpeedUnits:
                    {
                        VisualMotionSettings motionSettings = state.GetVisualMotionSettings(command.VisualModeValue);
                        if (motionSettings != null)
                        {
                            motionSettings.SetFlightSpeedUnits(command.FloatValue);
                            return true;
                        }

                        return false;
                    }

                case KaleidoscopeCommandType.SetVisualMotionImageOffset:
                    {
                        VisualMotionSettings motionSettings = state.GetVisualMotionSettings(command.VisualModeValue);
                        if (motionSettings != null)
                        {
                            motionSettings.SetImageOffset(command.Vector2Value);
                            return true;
                        }

                        return false;
                    }

                case KaleidoscopeCommandType.ResetVisualMotion:
                    state.ResetVisualMotion(command.VisualModeValue);
                    return true;

                case KaleidoscopeCommandType.StartImageReanimation:
                    state.BeginImageReanimation(ImageReanimationState.DefaultDurationSeconds);
                    return true;

                case KaleidoscopeCommandType.SetVisualMotionImageVelocity:
                    {
                        VisualMotionSettings motionSettings = state.GetVisualMotionSettings(command.VisualModeValue);
                        if (motionSettings != null)
                        {
                            motionSettings.SetImageOffsetVelocity(command.Vector2Value);
                            return true;
                        }

                        return false;
                    }

                case KaleidoscopeCommandType.ToggleVisualMotionImageInertia:
                    {
                        VisualMotionSettings motionSettings = state.GetVisualMotionSettings(command.VisualModeValue);
                        if (motionSettings != null)
                        {
                            motionSettings.ToggleImageShiftInertia();
                            return true;
                        }

                        return false;
                    }

                case KaleidoscopeCommandType.SetVisualMotionImageInertia:
                    {
                        VisualMotionSettings motionSettings = state.GetVisualMotionSettings(command.VisualModeValue);
                        if (motionSettings != null)
                        {
                            motionSettings.SetImageShiftInertiaEnabled(command.BoolValue);
                            return true;
                        }

                        return false;
                    }

                case KaleidoscopeCommandType.SetRecordingStatus:
                    state.SetRecordingStatus(command.RecordingStatusValue);
                    return true;

                case KaleidoscopeCommandType.SetActivePreset:
                    state.SetActivePreset(command.StringValue);
                    return true;

                case KaleidoscopeCommandType.SetQualityLevel:
                    state.SetQualityLevel(command.QualityLevelValue);
                    return true;

                case KaleidoscopeCommandType.ValidateSystem:
                    ValidateSystem();
                    return true;

                case KaleidoscopeCommandType.ClearDiagnostics:
                    state.ClearWarnings();
                    state.ClearErrors();
                    state.ClearMissingReferences();
                    return true;

                case KaleidoscopeCommandType.SetDiagnosticsVisible:
                    state.SetDiagnosticsVisible(command.BoolValue);
                    return true;

                case KaleidoscopeCommandType.SetControlMenuVisible:
                    state.SetControlMenuVisible(command.BoolValue);
                    return true;

                case KaleidoscopeCommandType.ToggleControlMenu:
                    state.SetControlMenuVisible(!state.ControlMenuVisible);
                    return true;

                case KaleidoscopeCommandType.ToggleHotkeysHelp:
                    state.ToggleHotkeysHelpVisible();
                    return true;

                case KaleidoscopeCommandType.SetHotkeysHelpVisible:
                    state.SetHotkeysHelpVisible(command.BoolValue);
                    return true;

                case KaleidoscopeCommandType.SetDiamondFocusEnabled:
                    state.DiamondFocusSettings.SetEnabled(command.BoolValue);
                    return true;

                case KaleidoscopeCommandType.ToggleDiamondFocus:
                    state.DiamondFocusSettings.ToggleEnabled();
                    return true;

                case KaleidoscopeCommandType.SetImageFilePath:
                    state.SetImageFilePath(command.StringValue);
                    return true;

                case KaleidoscopeCommandType.SetImageFolderPath:
                    state.SetImageFolderPath(command.StringValue);
                    return true;

                case KaleidoscopeCommandType.SetAudioFilePath:
                    state.SetAudioFilePath(command.StringValue);
                    return true;

                case KaleidoscopeCommandType.SetAudioFolderPath:
                    state.SetAudioFolderPath(command.StringValue);
                    return true;
            }

            return false;
        }

        private bool RouteCommandToModules(KaleidoscopeCommand command)
        {
            bool handled = false;

            for (int index = 0; index < modules.Count; index++)
            {
                IKaleidoscopeModule module = modules[index];

                if (module == null)
                {
                    continue;
                }

                try
                {
                    if (module.CanHandle(command))
                    {
                        module.HandleCommand(command);
                        handled = true;
                    }
                }
                catch (Exception exception)
                {
                    string message = "[Director] Module command failure in " + module.ModuleId + ": " + exception.Message;
                    state.ReportError(message);
                    Debug.LogException(exception, this);
                }
            }

            return handled;
        }

        private void RefreshModuleStatuses()
        {
            EnsureState();
            moduleStatuses.Clear();

            for (int index = 0; index < modules.Count; index++)
            {
                IKaleidoscopeModule module = modules[index];

                if (module != null)
                {
                    moduleStatuses.Add(module.GetStatus());
                }
            }

            state.SetModuleStatuses(moduleStatuses);
        }

        private void RegisterRenderEndpoint(IKaleidoscopeModule module)
        {
            IKaleidoscopeSourceProvider provider = module as IKaleidoscopeSourceProvider;
            if (provider != null)
            {
                if (sourceProvider != null && !ReferenceEquals(sourceProvider, provider))
                {
                    state.ReportWarning("[Director] Multiple source providers registered. Keeping the first provider.");
                }
                else
                {
                    sourceProvider = provider;
                }
            }

            IKaleidoscopeTextureProcessor processor = module as IKaleidoscopeTextureProcessor;
            if (processor != null)
            {
                for (int index = 0; index < textureProcessors.Count; index++)
                {
                    if (ReferenceEquals(textureProcessors[index], processor))
                    {
                        return;
                    }
                }

                textureProcessors.Add(processor);
            }
        }

        public void RefreshRenderPipeline()
        {
            Texture sourceTexture = sourceProvider != null ? sourceProvider.SourceTexture : null;

            if (textureProcessors.Count > 0)
            {
                Texture processed = sourceTexture;

                for (int index = 0; index < textureProcessors.Count; index++)
                {
                    IKaleidoscopeTextureProcessor processor = textureProcessors[index];
                    if (processor == null)
                    {
                        continue;
                    }

                    processed = processor.Process(processed, state);
                }

                finalOutputTexture = processed;
                return;
            }

            finalOutputTexture = sourceTexture;
        }

        private bool IsRegistered(IKaleidoscopeModule module)
        {
            for (int index = 0; index < modules.Count; index++)
            {
                if (ReferenceEquals(modules[index], module))
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureState()
        {
            if (state == null)
            {
                state = new KaleidoscopeState();
            }

            state.EnsureInitialized();
        }
    }
}
