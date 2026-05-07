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

        private readonly List<IKaleidoscopeModule> modules = new List<IKaleidoscopeModule>();
        private readonly List<KaleidoscopeModuleStatus> moduleStatuses = new List<KaleidoscopeModuleStatus>();

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

        private void Awake()
        {
            EnsureState();
        }

        private void Update()
        {
            if (!tickModules)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            for (int index = 0; index < modules.Count; index++)
            {
                IKaleidoscopeModule module = modules[index];

                if (module != null && module.IsActive)
                {
                    module.Tick(deltaTime);
                }
            }

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
            RefreshModuleStatuses();
            return true;
        }

        public void ClearRegisteredModules()
        {
            modules.Clear();
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

                case KaleidoscopeCommandType.SetMirrorZoom:
                    state.MirrorSettings.SetZoom(command.FloatValue);
                    return true;

                case KaleidoscopeCommandType.SetMirrorCenterOffset:
                    state.MirrorSettings.SetCenterOffset(command.Vector2Value);
                    return true;

                case KaleidoscopeCommandType.SetTunnelEnabled:
                    state.SetTunnelEnabled(command.BoolValue);
                    return true;

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
