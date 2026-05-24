using System.IO;
using Kaleidoscope2.Core;
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

        public bool ShowDiagnostics()
        {
            return Dispatch(KaleidoscopeCommand.SetDiagnosticsVisible(true), "Show Diagnostics");
        }

        public bool SetSecondDisplayOutput(bool enabled)
        {
            return Dispatch(KaleidoscopeCommand.SetSecondDisplayOutputEnabled(enabled), enabled ? "Enable Second Display Output" : "Disable Second Display Output");
        }

        public bool TestSecondDisplayOutput()
        {
            return SetSecondDisplayOutput(true);
        }

        public bool TrySetRecordingEnabled(KaelisProductionOptions options)
        {
            Debug.Log("[KAELIS Menu] Recording backend is reserved. Requested create clip: "
                + (options != null && options.CreateVideoClip ? "ON" : "OFF")
                + ", output folder: "
                + (options != null && !string.IsNullOrWhiteSpace(options.RecordingOutputFolder) ? options.RecordingOutputFolder : "none")
                + ".");
            return false;
        }

        public bool TryPrepareRecordingForExperience(KaelisProductionOptions options, bool hasAudioSource)
        {
            Debug.Log("[KAELIS Menu] Would auto-start video recording with experience. Backend reserved. Audio source: "
                + (hasAudioSource ? "available" : "not selected")
                + ", output folder: "
                + (options != null && !string.IsNullOrWhiteSpace(options.RecordingOutputFolder) ? options.RecordingOutputFolder : "none")
                + ".");
            return false;
        }

        public bool TryToggleRecording(KaelisProductionOptions options)
        {
            Debug.Log("[KAELIS Menu] Recording hotkey received, but recording backend is reserved. Intended output folder: "
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
            Debug.LogWarning("[KAELIS Menu] Native folder picker is reserved outside the Unity editor for now: " + title + ".");
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
