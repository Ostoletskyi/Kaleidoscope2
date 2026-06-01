using System.IO;
using Kaleidoscope2.Core;
using Kaleidoscope2.Demo;
using Kaleidoscope2.Settings;
using UnityEngine;

namespace Kaleidoscope2.Menu
{
    internal sealed class KaelisMenuCommandBridge
    {
        private KaleidoscopeDirector director;

        public bool SetRuntimeControlMenuVisible(bool visible)
        {
            return Dispatch(KaleidoscopeCommand.SetControlMenuVisible(visible), visible ? "Open Runtime Controls" : "Close Runtime Controls");
        }

        public bool ApplyImageFolder(string folderPath)
        {
            return Dispatch(KaleidoscopeCommand.SetImageFolderPath(folderPath), "Image Folder");
        }

        public bool ApplyMusicFolder(string folderPath)
        {
            return Dispatch(KaleidoscopeCommand.SetAudioFolderPath(folderPath), "Audio Folder");
        }

        public bool ApplyVisualMode(KaleidoscopeVisualMode visualMode, string label)
        {
            return Dispatch(KaleidoscopeCommand.SetVisualMode(visualMode), label);
        }

        public bool ApplyPremium3DMode()
        {
            bool enabled = Dispatch(KaleidoscopeCommand.SetDiamondFocusEnabled(true), "Enable Premium3D Diamond Focus");
            bool mode = Dispatch(KaleidoscopeCommand.SetCrystalSimulationMode(CrystalRenderMode.RealMesh3D), "Set Premium3D RealMesh");
            bool lights = Dispatch(KaleidoscopeCommand.SetCrystalLightRigEnabled(true), "Enable Premium3D Light Rig");
            return enabled && mode && lights;
        }

        public bool SetPremiumCrystalOptic(PremiumCrystalOpticsParameter parameter, float value)
        {
            return Dispatch(KaleidoscopeCommand.SetPremiumCrystalOptic(parameter, value), "Premium3D Optic " + parameter);
        }

        public bool SetPremiumCrystalEffect(PremiumCrystalEffectToggle effect, bool enabled)
        {
            return Dispatch(KaleidoscopeCommand.SetPremiumCrystalEffectEnabled(effect, enabled), "Premium3D Effect " + effect);
        }

        public bool SetPremiumCrystalShape(PremiumCrystalShapeType shape)
        {
            return Dispatch(KaleidoscopeCommand.SetPremiumCrystalShape(shape), "Premium3D Shape " + shape);
        }

        public bool SetPremiumCrystalOpticalMode(PremiumCrystalOpticalMode mode)
        {
            return Dispatch(KaleidoscopeCommand.SetPremiumCrystalOpticalMode(mode), "Premium3D Optical Mode " + mode);
        }

        public bool SetPremiumCrystalWheelScaleEnabled(bool enabled)
        {
            return Dispatch(KaleidoscopeCommand.SetPremiumCrystalWheelScaleEnabled(enabled), "Premium3D Wheel Scale");
        }

        public bool SetPremiumCrystalWheelScaleStepPercent(float percent)
        {
            return Dispatch(KaleidoscopeCommand.SetPremiumCrystalWheelScaleStepPercent(percent), "Premium3D Wheel Scale Step");
        }

        public bool ApplyPremiumCrystalPreset(PremiumCrystalFactoryPreset preset)
        {
            return Dispatch(KaleidoscopeCommand.ApplyPremiumCrystalPreset(preset), "Premium3D Factory Preset " + preset);
        }

        public bool CycleCrystalDebugMode(int direction)
        {
            return Dispatch(KaleidoscopeCommand.CycleCrystalDebugMode(direction), "Diamond Focus Debug Mode");
        }

        public bool SetCrystalDebugMode(DiamondCrystalDebugMode mode)
        {
            return Dispatch(KaleidoscopeCommand.SetCrystalDebugMode(mode), "Diamond Focus Debug Mode " + mode);
        }

        public bool ApplyExperimentalCrystalPreset(CrystalExperimentPresetType preset)
        {
            return Dispatch(KaleidoscopeCommand.ApplyExperimentalCrystalPreset(preset), "Experimental Crystal Preset " + preset);
        }

        public bool RestorePreviousCrystalPreset()
        {
            return Dispatch(KaleidoscopeCommand.RestorePreviousCrystalPreset(), "Restore Previous Crystal Preset");
        }

        public bool ShowDiagnostics()
        {
            return Dispatch(KaleidoscopeCommand.SetDiagnosticsVisible(true), "Show Diagnostics");
        }

        public bool StartMeditationMode()
        {
            if (!Dispatch(KaleidoscopeCommand.SetMeditationModeEnabled(true), "Meditation Mode"))
            {
                return false;
            }

            SettingsRestoreService sessions = DemoRuntimeLookup.FindModule<SettingsRestoreService>(director);
            return sessions != null && sessions.IsActive(TemporarySessionKind.Meditation);
        }

        public bool ToggleCrystalSplitComfort()
        {
            if (director == null)
            {
                ResolveDirector();
            }

            bool enabled = director == null || !director.State.CrystalSplitPresentation.Enabled;
            return Dispatch(KaleidoscopeCommand.SetCrystalSplitComfortEnabled(enabled), "Split Comfort");
        }

        public bool StartReplayDemo()
        {
            if (!Dispatch(KaleidoscopeCommand.StartReplayDemo(), "Replay Demo"))
            {
                return false;
            }

            SettingsRestoreService sessions = DemoRuntimeLookup.FindModule<SettingsRestoreService>(director);
            return sessions != null && sessions.IsActive(TemporarySessionKind.ReplayDemo);
        }

        public bool StartBenchmarkDemo()
        {
            if (!Dispatch(KaleidoscopeCommand.StartBenchmarkDemo(), "Benchmark Demo"))
            {
                return false;
            }

            SettingsRestoreService sessions = DemoRuntimeLookup.FindModule<SettingsRestoreService>(director);
            return sessions != null && sessions.IsActive(TemporarySessionKind.BenchmarkDemo);
        }

        public bool CancelTemporarySession()
        {
            return Dispatch(KaleidoscopeCommand.CancelTemporarySession(), "Stop Temporary Session");
        }

        public bool SaveBenchmarkResult()
        {
            if (!HasBenchmarkResult())
            {
                return false;
            }

            return Dispatch(KaleidoscopeCommand.SaveBenchmarkResult(), "Save Benchmark Result");
        }

        public bool HasBenchmarkResult()
        {
            if (director == null)
            {
                ResolveDirector();
            }

            BenchmarkResultView resultView = DemoRuntimeLookup.FindModule<BenchmarkResultView>(director);
            return resultView != null && resultView.HasResult;
        }

        public bool SetSecondDisplayOutput(bool enabled)
        {
            return Dispatch(KaleidoscopeCommand.SetSecondDisplayOutputEnabled(enabled), enabled ? "Enable Second Display Output" : "Disable Second Display Output");
        }

        public bool SetSettingsAutoSaveEnabled(bool enabled)
        {
            return Dispatch(KaleidoscopeCommand.SetSettingsAutoSaveEnabled(enabled), enabled ? "Enable Settings Auto Save" : "Disable Settings Auto Save");
        }

        public bool SaveSettings()
        {
            return Dispatch(KaleidoscopeCommand.SaveSettings(), "Save Settings");
        }

        public bool ResetSettings()
        {
            return Dispatch(KaleidoscopeCommand.ResetSettings(), "Reset Settings");
        }

        public bool TryGetSettingsAutoSaveEnabled(out bool enabled)
        {
            enabled = false;
            if (director == null)
            {
                ResolveDirector();
            }

            SettingsPersistenceService settings = DemoRuntimeLookup.FindModule<SettingsPersistenceService>(director);
            if (settings == null)
            {
                return false;
            }

            enabled = settings.AutoSaveEnabled;
            return true;
        }

        public bool TestSecondDisplayOutput()
        {
            return SetSecondDisplayOutput(true);
        }

        public bool TrySetRecordingEnabled(KaelisProductionOptions options)
        {
            Debug.Log("[KAELIS Menu] Showcase Recording service is not implemented. Requested create clip: "
                + (options != null && options.CreateVideoClip ? "ON" : "OFF")
                + ", output folder: "
                + (options != null && !string.IsNullOrWhiteSpace(options.RecordingOutputFolder) ? options.RecordingOutputFolder : "none")
                + ".");
            return false;
        }

        public bool TryPrepareRecordingForExperience(KaelisProductionOptions options, bool hasAudioSource)
        {
            Debug.Log("[KAELIS Menu] Would auto-start video recording with experience. Showcase Recording service is not implemented. Audio source: "
                + (hasAudioSource ? "available" : "not selected")
                + ", output folder: "
                + (options != null && !string.IsNullOrWhiteSpace(options.RecordingOutputFolder) ? options.RecordingOutputFolder : "none")
                + ".");
            return false;
        }

        public bool TryToggleRecording(KaelisProductionOptions options)
        {
            Debug.Log("[KAELIS Menu] Recording hotkey received, but Showcase Recording service is not implemented. Intended output folder: "
                + (options != null && !string.IsNullOrWhiteSpace(options.RecordingOutputFolder) ? options.RecordingOutputFolder : "none")
                + ".");
            return false;
        }

        public bool TryOpenFolderPicker(string title, string currentPath, out string folderPath)
        {
            folderPath = null;
#if UNITY_EDITOR
            string startPath = Directory.Exists(currentPath) ? currentPath : string.Empty;
            string selected = UnityEditor.EditorUtility.OpenFolderPanel(title, startPath, string.Empty);
            if (string.IsNullOrWhiteSpace(selected))
            {
                return false;
            }

            folderPath = selected;
            return true;
#else
            Debug.LogWarning("[KAELIS Menu] Native folder picker service is unavailable outside the Unity editor for now: " + title + ".");
            return false;
#endif
        }

        public void ExitApplication()
        {
            Debug.Log("[KAELIS Menu] Exit Application confirmed.");
#if UNITY_EDITOR
            Debug.Log("[KAELIS Menu] Application.Quit skipped in the Unity editor.");
#else
            Application.Quit();
#endif
        }

        private bool Dispatch(KaleidoscopeCommand command, string label)
        {
            if (director == null)
            {
                ResolveDirector();
            }

            if (director == null)
            {
                Debug.LogWarning("[KAELIS Menu] " + label + " command unavailable: KaleidoscopeDirector was not found.");
                return false;
            }

            director.Dispatch(command);
            Debug.Log("[KAELIS Menu] Dispatched runtime command for " + label + ": " + command.Type + ".");
            return true;
        }

        private void ResolveDirector()
        {
            director = Object.FindObjectOfType<KaleidoscopeDirector>();
        }
    }
}
