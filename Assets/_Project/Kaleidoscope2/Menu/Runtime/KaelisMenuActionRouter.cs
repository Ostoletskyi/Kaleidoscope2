using System;
using System.IO;
using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Menu
{
    internal sealed class KaelisMenuActionRouter
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".tga" };
        private static readonly string[] AudioExtensions = { ".mp3", ".wav", ".ogg", ".aiff", ".aif" };

        private readonly KaelisMenuSectionController sectionController;
        private readonly KaelisContentSelectionPanel contentSelectionPanel;
        private readonly KaelisMenuCommandBridge commandBridge;
        private readonly Action<string> setStatus;
        private readonly Action<bool> setMainMenuVisible;
        private readonly Action hideStartupMenu;
        private string imageFolderPath;
        private string musicFolderPath;

        public KaelisMenuActionRouter(
            KaelisMenuSectionController sectionController,
            KaelisContentSelectionPanel contentSelectionPanel,
            KaelisMenuCommandBridge commandBridge,
            Action<string> setStatus,
            Action<bool> setMainMenuVisible,
            Action hideStartupMenu)
        {
            this.sectionController = sectionController;
            this.contentSelectionPanel = contentSelectionPanel;
            this.commandBridge = commandBridge;
            this.setStatus = setStatus;
            this.setMainMenuVisible = setMainMenuVisible;
            this.hideStartupMenu = hideStartupMenu;

            if (contentSelectionPanel != null)
            {
                contentSelectionPanel.SetHandlers(
                    SelectImageFolder,
                    ClearImageFolder,
                    SelectMusicFolder,
                    ClearMusicFolder,
                    StartExperience,
                    BackToMainMenu);
            }
        }

        public void TriggerEnterExperience()
        {
            OpenEnterExperienceFlow();
        }

        public void OpenEnterExperienceFlow()
        {
            if (sectionController != null)
            {
                sectionController.Close();
            }

            if (setMainMenuVisible != null)
            {
                setMainMenuVisible(false);
            }

            if (contentSelectionPanel != null)
            {
                contentSelectionPanel.SetVisible(true);
                contentSelectionPanel.SetValidation("Select an image folder to begin. Music is optional.", false);
            }

            SetStatus("SELECT EXPERIENCE CONTENT");
        }

        public void OpenSection(KaelisMenuSection section)
        {
            if (sectionController == null)
            {
                SetStatus("SECTION ROUTER UNAVAILABLE");
                Debug.LogWarning("[KAELIS Menu] Section router unavailable for " + section + ".");
                return;
            }

            sectionController.SetActiveSection(section);
            SetStatus(GetSectionStatus(section));
            Debug.Log("[KAELIS Menu] Opened section: " + section + ".");
        }

        public void HandlePanelCommand(KaelisMenuPanelCommand command)
        {
            switch (command)
            {
                case KaelisMenuPanelCommand.CloseSection:
                case KaelisMenuPanelCommand.CancelExit:
                    CloseActiveSection(command == KaelisMenuPanelCommand.CancelExit ? "EXIT CANCELLED" : "SECTION CLOSED");
                    break;
                case KaelisMenuPanelCommand.ApplyClassicMode:
                    ApplyVisualMode(KaleidoscopeVisualMode.Classic, "CLASSIC 2D");
                    break;
                case KaelisMenuPanelCommand.ApplyPremium3DMode:
                    ApplyPremium3DMode();
                    break;
                case KaelisMenuPanelCommand.ApplyTunnelMode:
                    ApplyVisualMode(KaleidoscopeVisualMode.Tunnel, "4D TUNNEL");
                    break;
                case KaelisMenuPanelCommand.ApplyFiveDMode:
                    ApplyVisualMode(KaleidoscopeVisualMode.FiveD, "5D FLIGHT");
                    break;
                case KaelisMenuPanelCommand.ShowDiagnostics:
                    ShowDiagnostics();
                    break;
                case KaelisMenuPanelCommand.SetSecondDisplayOutput:
                    SetSecondDisplayOutput();
                    break;
                case KaelisMenuPanelCommand.TestSecondDisplayOutput:
                    TestSecondDisplayOutput();
                    break;
                case KaelisMenuPanelCommand.SetRecordingEnabled:
                    SetRecordingEnabled();
                    break;
                case KaelisMenuPanelCommand.SelectRecordingOutputFolder:
                    SelectRecordingOutputFolder();
                    break;
                case KaelisMenuPanelCommand.ClearRecordingOutputFolder:
                    ClearRecordingOutputFolder();
                    break;
                case KaelisMenuPanelCommand.ApplySelectedPreset:
                    ApplySelectedPremiumPreset();
                    break;
                case KaelisMenuPanelCommand.SaveAndExit:
                    SetStatus("SAVE RESERVED - EXITING");
                    Debug.Log("[KAELIS Menu] Save settings before exit is reserved; exiting without changing persistence.");
                    if (commandBridge != null)
                    {
                        commandBridge.ExitApplication();
                    }
                    break;
                case KaelisMenuPanelCommand.ExitWithoutSaving:
                    SetStatus("EXIT WITHOUT SAVING");
                    if (commandBridge != null)
                    {
                        commandBridge.ExitApplication();
                    }
                    break;
                case KaelisMenuPanelCommand.ReservedAction:
                    SetStatus("ACTION RESERVED");
                    Debug.Log("[KAELIS Menu] Reserved menu action selected. No runtime command dispatched.");
                    break;
            }
        }

        public void HandlePremiumCrystalOptic(PremiumCrystalOpticsParameter parameter, float value)
        {
            bool dispatched = commandBridge != null && commandBridge.SetPremiumCrystalOptic(parameter, value);
            SetStatus(dispatched
                ? "OPTIC " + parameter.ToString().ToUpperInvariant() + " " + value.ToString("0.00")
                : "OPTIC COMMAND UNAVAILABLE");
        }

        public void HandlePremiumCrystalEffect(PremiumCrystalEffectToggle effect, bool enabled)
        {
            bool dispatched = commandBridge != null && commandBridge.SetPremiumCrystalEffect(effect, enabled);
            SetStatus(dispatched
                ? "EFFECT " + DiamondFocusSettings.GetPremiumCrystalEffectLabel(effect).ToUpperInvariant() + " " + (enabled ? "ON" : "OFF")
                : "EFFECT COMMAND UNAVAILABLE");
        }

        public void HandlePremiumCrystalShape(PremiumCrystalShapeType shape)
        {
            bool dispatched = commandBridge != null && commandBridge.SetPremiumCrystalShape(shape);
            SetStatus(dispatched
                ? "PREMIUM3D FORM " + PremiumCrystalShapeLibrary.GetLabel(shape).ToUpperInvariant()
                : "PREMIUM3D FORM COMMAND UNAVAILABLE");
        }

        public void HandlePremiumCrystalOpticalMode(PremiumCrystalOpticalMode mode)
        {
            bool dispatched = commandBridge != null && commandBridge.SetPremiumCrystalOpticalMode(mode);
            SetStatus(dispatched
                ? "PREMIUM3D OPTICAL MODE " + PremiumCrystalOpticalModeLibrary.GetLabel(mode).ToUpperInvariant()
                : "PREMIUM3D OPTICAL MODE COMMAND UNAVAILABLE");
        }

        public void HandlePremiumWheelScaleEnabled(bool enabled)
        {
            bool dispatched = commandBridge != null && commandBridge.SetPremiumCrystalWheelScaleEnabled(enabled);
            SetStatus(dispatched ? "WHEEL VISUAL SCALE " + (enabled ? "ON" : "OFF") : "WHEEL COMMAND UNAVAILABLE");
        }

        public void HandlePremiumWheelScaleStep(float stepPercent)
        {
            bool dispatched = commandBridge != null && commandBridge.SetPremiumCrystalWheelScaleStepPercent(stepPercent);
            SetStatus(dispatched ? "WHEEL SCALE STEP " + stepPercent.ToString("0") + "%" : "WHEEL STEP COMMAND UNAVAILABLE");
        }

        public void HandleCrystalDebugMode(int direction)
        {
            bool dispatched = commandBridge != null && commandBridge.CycleCrystalDebugMode(direction);
            SetStatus(dispatched ? "CRYSTAL DEBUG MODE CYCLED" : "DEBUG MODE COMMAND UNAVAILABLE");
        }

        public void HandleExperimentalCrystalPreset(CrystalExperimentPresetType preset)
        {
            bool dispatched = commandBridge != null && commandBridge.ApplyExperimentalCrystalPreset(preset);
            SetStatus(dispatched
                ? "EXPERIMENT " + CrystalExperimentPreset.GetLabel(preset).ToUpperInvariant()
                : "EXPERIMENT COMMAND UNAVAILABLE");
        }

        private void ApplyVisualMode(KaleidoscopeVisualMode visualMode, string label)
        {
            bool dispatched = commandBridge != null && commandBridge.ApplyVisualMode(visualMode, label);
            SetStatus(dispatched ? "MODE " + label : "MODE COMMAND UNAVAILABLE");
        }

        private void ApplyPremium3DMode()
        {
            bool dispatched = commandBridge != null && commandBridge.ApplyPremium3DMode();
            SetStatus(dispatched ? "MODE PREMIUM 3D CRYSTAL" : "PREMIUM 3D COMMAND UNAVAILABLE");
        }

        private void ApplySelectedPremiumPreset()
        {
            if (sectionController == null || !sectionController.TryGetSelectedPremiumPreset(out PremiumCrystalFactoryPreset preset))
            {
                SetStatus("SELECT A FACTORY PRESET FIRST");
                return;
            }

            bool dispatched = commandBridge != null && commandBridge.ApplyPremiumCrystalPreset(preset);
            SetStatus(dispatched
                ? "PRESET " + DiamondFocusSettings.GetPremiumCrystalFactoryPresetLabel(preset).ToUpperInvariant()
                : "PRESET COMMAND UNAVAILABLE");
        }

        private void SelectImageFolder()
        {
            string selectedPath;
            if (commandBridge == null || !commandBridge.TryOpenFolderPicker("Select KAELIS Image Folder", imageFolderPath, out selectedPath))
            {
                SetContentValidation("Image folder picker is unavailable or was cancelled.", true);
                SetStatus("IMAGE FOLDER NOT SELECTED");
                return;
            }

            SetImageFolderCandidate(selectedPath);
        }

        private void SelectMusicFolder()
        {
            string selectedPath;
            if (commandBridge == null || !commandBridge.TryOpenFolderPicker("Select KAELIS Music Folder", musicFolderPath, out selectedPath))
            {
                SetContentValidation("Music folder picker is unavailable or was cancelled.", true);
                SetStatus("MUSIC FOLDER NOT SELECTED");
                return;
            }

            SetMusicFolderCandidate(selectedPath);
        }

        private void ClearImageFolder()
        {
            imageFolderPath = null;
            if (contentSelectionPanel != null)
            {
                contentSelectionPanel.SetImageFolder(null, false);
                contentSelectionPanel.SetValidation("Please select an image folder first.", true);
            }

            SetStatus("IMAGE FOLDER CLEARED");
        }

        private void ClearMusicFolder()
        {
            musicFolderPath = null;
            if (contentSelectionPanel != null)
            {
                contentSelectionPanel.SetMusicFolder(null, false);
                contentSelectionPanel.SetValidation("Music source cleared. Visual-only start remains available after selecting images.", false);
            }

            SetStatus("MUSIC FOLDER CLEARED");
        }

        private void StartExperience()
        {
            if (!IsFolderValid(imageFolderPath, ImageExtensions))
            {
                SetContentValidation("Please select an image folder first.", true);
                SetStatus("IMAGE FOLDER REQUIRED");
                return;
            }

            bool imageApplied = commandBridge != null && commandBridge.ApplyImageFolder(imageFolderPath);
            if (!imageApplied)
            {
                SetContentValidation("Image folder could not be applied to the runtime session.", true);
                SetStatus("IMAGE SOURCE UNAVAILABLE");
                return;
            }

            bool hasMusic = IsFolderValid(musicFolderPath, AudioExtensions);
            if (hasMusic && commandBridge != null)
            {
                commandBridge.ApplyMusicFolder(musicFolderPath);
            }

            string recordingStartStatus = null;
            KaelisProductionOptions productionOptions = sectionController != null ? sectionController.ProductionOptions : null;
            if (productionOptions != null && productionOptions.CreateVideoClip)
            {
                if (!productionOptions.HasRecordingOutputFolder)
                {
                    recordingStartStatus = "SESSION STARTED - OUTPUT FOLDER MISSING";
                    Debug.LogWarning("[KAELIS Menu] Recording was enabled, but no output folder was selected. Starting visual session without recording.");
                }
                else if (commandBridge != null && commandBridge.TryPrepareRecordingForExperience(productionOptions, hasMusic))
                {
                    recordingStartStatus = "SESSION STARTED - RECORDING READY";
                }
                else
                {
                    recordingStartStatus = "SESSION STARTED - RECORDING RESERVED";
                }
            }

            if (commandBridge != null)
            {
                commandBridge.SetRuntimeControlMenuVisible(true);
            }

            SetContentValidation(hasMusic ? "Session started with image and music sources." : "Session started. Audio source not selected.", !hasMusic);
            SetStatus(!string.IsNullOrWhiteSpace(recordingStartStatus) ? recordingStartStatus : (hasMusic ? "SESSION STARTED" : "SESSION STARTED - AUDIO SOURCE NOT SELECTED"));

            if (hideStartupMenu != null)
            {
                hideStartupMenu();
            }
        }

        private void BackToMainMenu()
        {
            if (contentSelectionPanel != null)
            {
                contentSelectionPanel.SetVisible(false);
            }

            if (setMainMenuVisible != null)
            {
                setMainMenuVisible(true);
            }

            SetStatus("SYSTEM READY");
        }

        private void SetImageFolderCandidate(string path)
        {
            bool valid = IsFolderValid(path, ImageExtensions);
            imageFolderPath = valid ? path : null;

            if (contentSelectionPanel != null)
            {
                contentSelectionPanel.SetImageFolder(path, valid);
                contentSelectionPanel.SetValidation(valid ? "Image folder ready." : "Selected image folder has no supported image files.", !valid);
            }

            SetStatus(valid ? "IMAGE FOLDER READY" : "IMAGE FOLDER INVALID");
        }

        private void SetMusicFolderCandidate(string path)
        {
            bool valid = IsFolderValid(path, AudioExtensions);
            musicFolderPath = valid ? path : null;

            if (contentSelectionPanel != null)
            {
                contentSelectionPanel.SetMusicFolder(path, valid);
                contentSelectionPanel.SetValidation(valid ? "Music folder ready. Start Experience will use audio." : "Selected music folder has no supported audio files. Music is optional.", !valid);
            }

            SetStatus(valid ? "MUSIC FOLDER READY" : "MUSIC FOLDER INVALID");
        }

        private void SetContentValidation(string message, bool warning)
        {
            if (contentSelectionPanel != null)
            {
                contentSelectionPanel.SetValidation(message, warning);
            }
        }

        private void ShowDiagnostics()
        {
            bool dispatched = commandBridge != null && commandBridge.ShowDiagnostics();
            SetStatus(dispatched ? "DIAGNOSTICS VISIBLE" : "DIAGNOSTICS UNAVAILABLE");
        }

        public void HandleRecordingHotkey()
        {
            KaelisProductionOptions options = sectionController != null ? sectionController.ProductionOptions : null;
            bool dispatched = commandBridge != null && commandBridge.TryToggleRecording(options);
            if (options != null)
            {
                options.IsRecording = dispatched && !options.IsRecording;
            }

            SetStatus(dispatched ? (options != null && options.IsRecording ? "RECORDING STARTED" : "RECORDING STOPPED") : "RECORDING HOTKEY RESERVED");
        }

        private void SetSecondDisplayOutput()
        {
            if (sectionController == null || sectionController.ProductionOptions == null)
            {
                SetStatus("SECOND DISPLAY UNAVAILABLE");
                return;
            }

            sectionController.RefreshSecondDisplayAvailability();
            KaelisProductionOptions options = sectionController.ProductionOptions;
            if (!options.SecondDisplayAvailable)
            {
                sectionController.SetSecondDisplayOutputState(false);
                SetStatus("NO SECOND DISPLAY DETECTED");
                return;
            }

            bool dispatched = commandBridge != null && commandBridge.SetSecondDisplayOutput(options.OutputToSecondDisplay);
            sectionController.SetSecondDisplayOutputState(dispatched && options.OutputToSecondDisplay);
            SetStatus(dispatched
                ? (options.OutputToSecondDisplay ? "SECOND DISPLAY OUTPUT ENABLED" : "SECOND DISPLAY OUTPUT DISABLED")
                : "SECOND DISPLAY COMMAND UNAVAILABLE");
        }

        private void TestSecondDisplayOutput()
        {
            if (sectionController == null || sectionController.ProductionOptions == null)
            {
                SetStatus("SECOND DISPLAY UNAVAILABLE");
                return;
            }

            sectionController.RefreshSecondDisplayAvailability();
            KaelisProductionOptions options = sectionController.ProductionOptions;
            if (!options.SecondDisplayAvailable)
            {
                SetStatus("NO SECOND DISPLAY DETECTED");
                return;
            }

            bool dispatched = commandBridge != null && commandBridge.TestSecondDisplayOutput();
            sectionController.SetSecondDisplayOutputState(dispatched);
            SetStatus(dispatched ? "SECOND DISPLAY OUTPUT ENABLED" : "SECOND DISPLAY COMMAND UNAVAILABLE");
        }

        private void SetRecordingEnabled()
        {
            KaelisProductionOptions options = sectionController != null ? sectionController.ProductionOptions : null;
            if (options == null)
            {
                SetStatus("RECORDING RESERVED");
                return;
            }

            if (options.CreateVideoClip && commandBridge != null)
            {
                commandBridge.TrySetRecordingEnabled(options);
            }

            SetStatus(sectionController.GetRecordingStatusForStatusBar());
        }

        private void SelectRecordingOutputFolder()
        {
            KaelisProductionOptions options = sectionController != null ? sectionController.ProductionOptions : null;
            string currentPath = options != null ? options.RecordingOutputFolder : null;
            string selectedPath;
            if (commandBridge == null || !commandBridge.TryOpenFolderPicker("Select KAELIS Recording Output Folder", currentPath, out selectedPath))
            {
                SetStatus("RECORDING OUTPUT FOLDER NOT SELECTED");
                return;
            }

            if (sectionController != null)
            {
                sectionController.SetRecordingOutputFolder(selectedPath);
            }

            SetStatus(options != null && options.CreateVideoClip ? sectionController.GetRecordingStatusForStatusBar() : "RECORDING OUTPUT FOLDER READY");
        }

        private void ClearRecordingOutputFolder()
        {
            if (sectionController != null)
            {
                sectionController.ClearRecordingOutputFolder();
                SetStatus(sectionController.GetRecordingStatusForStatusBar());
                return;
            }

            SetStatus("RECORDING OUTPUT FOLDER CLEARED");
        }

        private void CloseActiveSection(string status)
        {
            if (sectionController != null)
            {
                sectionController.Close();
            }

            SetStatus(status);
        }

        private void SetStatus(string value)
        {
            if (setStatus != null)
            {
                setStatus(value);
            }
        }

        private static string GetSectionStatus(KaelisMenuSection section)
        {
            switch (section)
            {
                case KaelisMenuSection.Modes:
                    return "MODES SECTION";
                case KaelisMenuSection.Optics:
                    return "OPTICS SECTION";
                case KaelisMenuSection.Presets:
                    return "PRESETS SECTION";
                case KaelisMenuSection.Settings:
                    return "SETTINGS SECTION";
                case KaelisMenuSection.Exit:
                    return "EXIT CONFIRMATION";
                default:
                    return "SYSTEM READY";
            }
        }

        private static bool IsFolderValid(string folderPath, string[] extensions)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                return false;
            }

            try
            {
                string[] files = Directory.GetFiles(folderPath);
                for (int index = 0; index < files.Length; index++)
                {
                    if (HasExtension(files[index], extensions))
                    {
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[KAELIS Menu] Failed to validate folder '" + folderPath + "': " + exception.Message);
            }

            return false;
        }

        private static bool HasExtension(string path, string[] extensions)
        {
            string extension = Path.GetExtension(path);
            if (string.IsNullOrWhiteSpace(extension))
            {
                return false;
            }

            extension = extension.ToLowerInvariant();
            for (int index = 0; index < extensions.Length; index++)
            {
                if (extension == extensions[index])
                {
                    return true;
                }
            }

            return false;
        }
    }
}
