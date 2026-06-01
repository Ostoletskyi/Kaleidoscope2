using System;
using System.IO;
using Kaleidoscope2.Core;
using Kaleidoscope2.Menu;
using UnityEngine;

namespace Kaleidoscope2.Settings
{
    [DisallowMultipleComponent]
    public sealed class SettingsPersistenceService : KaleidoscopeModuleBase
    {
        private const string SettingsFileName = "kaelis-settings.json";

        [SerializeField] private bool loadOnInitialize = true;

        private KaleidoscopeDirector director;
        private KaelisSettingsData settingsData;
        private string storagePathOverride;
        private string lastMessage = "Not loaded";
        private bool applyingSettings;

        public KaelisSettingsData Data
        {
            get
            {
                EnsureData();
                return settingsData;
            }
        }

        public bool AutoSaveEnabled
        {
            get
            {
                EnsureData();
                return settingsData.startup != null && settingsData.startup.autoSaveSettings;
            }
        }

        public string SettingsFilePath
        {
            get
            {
                return !string.IsNullOrWhiteSpace(storagePathOverride)
                    ? storagePathOverride
                    : Path.Combine(Application.persistentDataPath, SettingsFileName);
            }
        }

        protected override void OnInitialized()
        {
            Subscribe();
            if (loadOnInitialize)
            {
                LoadSettings();
                ApplySettingsToRuntime();
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        public void Configure(KaleidoscopeDirector owner)
        {
            if (director == owner)
            {
                return;
            }

            Unsubscribe();
            director = owner;
            Subscribe();
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            if (command == null)
            {
                return false;
            }

            return command.Type == KaleidoscopeCommandType.SetSettingsAutoSaveEnabled
                || command.Type == KaleidoscopeCommandType.SaveSettings
                || command.Type == KaleidoscopeCommandType.ResetSettings;
        }

        public override void HandleCommand(KaleidoscopeCommand command)
        {
            if (command == null)
            {
                return;
            }

            switch (command.Type)
            {
                case KaleidoscopeCommandType.SetSettingsAutoSaveEnabled:
                    SetAutoSaveEnabled(command.BoolValue);
                    break;
                case KaleidoscopeCommandType.SaveSettings:
                    SaveSettings();
                    break;
                case KaleidoscopeCommandType.ResetSettings:
                    ResetSettingsAndSave();
                    break;
            }
        }

        public void LoadSettings()
        {
            string path = SettingsFilePath;
            if (!File.Exists(path))
            {
                settingsData = KaelisSettingsData.CreateDefault();
                WriteSettingsDataToDisk();
                lastMessage = "Defaults active; created settings file at " + path;
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                {
                    BackupInvalidSettingsFile(path, "empty");
                    settingsData = KaelisSettingsData.CreateDefault();
                    WriteSettingsDataToDisk();
                    lastMessage = "Defaults active; settings file was empty.";
                    return;
                }

                settingsData = KaelisSettingsData.Sanitize(JsonUtility.FromJson<KaelisSettingsData>(json));
                lastMessage = "Loaded settings from " + path;
            }
            catch (Exception exception)
            {
                BackupInvalidSettingsFile(path, "corrupt");
                settingsData = KaelisSettingsData.CreateDefault();
                WriteSettingsDataToDisk();
                lastMessage = "Defaults active; settings file could not be read.";
                ReportWarning("Settings load failed; using safe defaults. " + exception.Message);
            }
        }

        public bool SaveSettings()
        {
            EnsureData();
            CaptureRuntimeState();

            return WriteSettingsDataToDisk();
        }

        public void ResetSettings()
        {
            settingsData = KaelisSettingsData.CreateDefault();
            ApplySettingsToRuntime();
            lastMessage = "Settings reset to safe defaults.";
        }

        public bool ResetSettingsAndSave()
        {
            ResetSettings();
            return WriteSettingsDataToDisk();
        }

        public bool SetAutoSaveEnabled(bool enabled)
        {
            EnsureData();
            CaptureRuntimeState();
            settingsData.startup.autoSaveSettings = enabled;

            // The auto-save switch itself must persist even when it is being turned off.
            return WriteSettingsDataToDisk();
        }

        public bool SaveIfAutoSaveEnabled()
        {
            if (!AutoSaveEnabled)
            {
                return true;
            }

            CaptureRuntimeState();
            return WriteSettingsDataToDisk();
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            return CreateStatus(lastMessage);
        }

        private bool WriteSettingsDataToDisk()
        {
            EnsureData();

            try
            {
                settingsData = KaelisSettingsData.Sanitize(settingsData);
                string path = SettingsFilePath;
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(path, JsonUtility.ToJson(settingsData, true));
                lastMessage = "Saved settings to " + path;
                return true;
            }
            catch (Exception exception)
            {
                lastMessage = "Settings save failed.";
                ReportWarning("Settings save failed. " + exception.Message);
                return false;
            }
        }

        private void CaptureRuntimeState()
        {
            EnsureData();
            KaleidoscopeState state = State;
            if (state == null)
            {
                return;
            }

            state.EnsureInitialized();
            settingsData.visual.activeVisualMode = (int)state.ActiveVisualMode;
            settingsData.visual.activePreset = state.ActivePreset;
            settingsData.controls.mouseWheelVisualScaleEnabled = state.MouseWheelVisualScaleEnabled;
            settingsData.controls.mouseWheelVisualScaleStepPercent = state.MouseWheelVisualScaleStepPercent;
            settingsData.ui.menuLanguage = KaelisMenuLocalizationService.CurrentLanguage.ToString();

            DiamondFocusSettings diamond = state.DiamondFocusSettings;
            if (diamond == null)
            {
                return;
            }

            settingsData.visual.diamondFocusEnabled = diamond.Enabled;
            settingsData.visual.crystalRenderMode = (int)diamond.CrystalSimulationMode;
            settingsData.visual.classicCrystalScalePercent = diamond.ClassicCrystalScalePercent;
            settingsData.visual.premiumCrystalScalePercent = diamond.PremiumCrystalScalePercent;
            settingsData.visual.classicShape = (int)diamond.Shape;
            settingsData.visual.premiumShape = (int)diamond.PremiumCrystalShape;
            settingsData.visual.premiumOpticalMode = (int)diamond.ActivePremiumCrystalOpticalMode;
            settingsData.visual.debugMode = (int)diamond.DebugMode;
            settingsData.visual.crystalDebugEffect = (int)diamond.CrystalDebugEffects.SelectedEffect;
            settingsData = KaelisSettingsData.Sanitize(settingsData);
        }

        private void ApplySettingsToRuntime()
        {
            EnsureData();
            KaleidoscopeState state = State;
            if (state == null)
            {
                return;
            }

            applyingSettings = true;
            try
            {
                state.EnsureInitialized();
                state.SetVisualMode((KaleidoscopeVisualMode)settingsData.visual.activeVisualMode);
                state.SetActivePreset(settingsData.visual.activePreset);
                state.SetMouseWheelVisualScaleEnabled(settingsData.controls.mouseWheelVisualScaleEnabled);
                state.SetMouseWheelVisualScaleStepPercent(settingsData.controls.mouseWheelVisualScaleStepPercent);

                DiamondFocusSettings diamond = state.DiamondFocusSettings;
                if (diamond != null)
                {
                    diamond.SetEnabled(settingsData.visual.diamondFocusEnabled);
                    diamond.SetCrystalSimulationMode((CrystalRenderMode)settingsData.visual.crystalRenderMode);
                    diamond.SetClassicCrystalScalePercent(settingsData.visual.classicCrystalScalePercent);
                    diamond.SetPremiumCrystalScalePercent(settingsData.visual.premiumCrystalScalePercent);
                    diamond.SetShape((DiamondFocusShape)settingsData.visual.classicShape);
                    diamond.SetPremiumCrystalShape((PremiumCrystalShapeType)settingsData.visual.premiumShape);
                    diamond.SetPremiumCrystalOpticalMode((PremiumCrystalOpticalMode)settingsData.visual.premiumOpticalMode);
                    diamond.SetDebugMode((DiamondCrystalDebugMode)settingsData.visual.debugMode);
                    diamond.SetCrystalDebugEffect((CrystalDebugEffectType)settingsData.visual.crystalDebugEffect);
                }

                KaelisMenuLanguage language;
                if (Enum.TryParse(settingsData.ui.menuLanguage, out language))
                {
                    KaelisMenuLocalizationService.SetLanguage(language, false);
                }
            }
            finally
            {
                applyingSettings = false;
            }
        }

        private void OnCommandDispatched(KaleidoscopeCommandDispatchEvent dispatchEvent)
        {
            if (applyingSettings
                || dispatchEvent.Origin != KaleidoscopeCommandOrigin.User
                || dispatchEvent.Command == null
                || !IsPersistedStateCommand(dispatchEvent.Command.Type))
            {
                return;
            }

            CaptureRuntimeState();
            SaveIfAutoSaveEnabled();
        }

        private void OnLanguageChanged()
        {
            if (applyingSettings)
            {
                return;
            }

            EnsureData();
            settingsData.ui.menuLanguage = KaelisMenuLocalizationService.CurrentLanguage.ToString();
            SaveIfAutoSaveEnabled();
        }

        private static bool IsPersistedStateCommand(KaleidoscopeCommandType type)
        {
            switch (type)
            {
                case KaleidoscopeCommandType.SetVisualMode:
                case KaleidoscopeCommandType.SetDiamondFocusEnabled:
                case KaleidoscopeCommandType.ToggleDiamondFocus:
                case KaleidoscopeCommandType.SetCrystalSimulationMode:
                case KaleidoscopeCommandType.ToggleCrystalSimulationMode:
                case KaleidoscopeCommandType.SetDiamondShape:
                case KaleidoscopeCommandType.NextDiamondShape:
                case KaleidoscopeCommandType.PreviousDiamondShape:
                case KaleidoscopeCommandType.CycleCrystalGeometryForward:
                case KaleidoscopeCommandType.CycleCrystalGeometryBackward:
                case KaleidoscopeCommandType.SetPremiumCrystalShape:
                case KaleidoscopeCommandType.CyclePremiumCrystalShape:
                case KaleidoscopeCommandType.SetPremiumCrystalOpticalMode:
                case KaleidoscopeCommandType.CyclePremiumCrystalOpticalMode:
                case KaleidoscopeCommandType.SetCrystalDebugMode:
                case KaleidoscopeCommandType.CycleCrystalDebugMode:
                case KaleidoscopeCommandType.SetCrystalDebugEffect:
                case KaleidoscopeCommandType.CycleCrystalDebugEffect:
                case KaleidoscopeCommandType.SetPremiumCrystalWheelScaleEnabled:
                case KaleidoscopeCommandType.SetPremiumCrystalWheelScaleStepPercent:
                case KaleidoscopeCommandType.AdjustClassicCrystalScalePercent:
                case KaleidoscopeCommandType.SetClassicCrystalScalePercent:
                case KaleidoscopeCommandType.AdjustPremiumCrystalScalePercent:
                case KaleidoscopeCommandType.SetPremiumCrystalScalePercent:
                case KaleidoscopeCommandType.ApplyPremiumCrystalPreset:
                case KaleidoscopeCommandType.ApplyExperimentalCrystalPreset:
                case KaleidoscopeCommandType.RestorePreviousCrystalPreset:
                    return true;
                default:
                    return false;
            }
        }

        private void EnsureData()
        {
            if (settingsData == null)
            {
                settingsData = KaelisSettingsData.CreateDefault();
            }

            settingsData = KaelisSettingsData.Sanitize(settingsData);
        }

        private void Subscribe()
        {
            if (director != null)
            {
                director.SemanticCommandDispatched -= OnCommandDispatched;
                director.SemanticCommandDispatched += OnCommandDispatched;
            }

            KaelisMenuLocalizationService.LanguageChanged -= OnLanguageChanged;
            KaelisMenuLocalizationService.LanguageChanged += OnLanguageChanged;
        }

        private void Unsubscribe()
        {
            if (director != null)
            {
                director.SemanticCommandDispatched -= OnCommandDispatched;
            }

            KaelisMenuLocalizationService.LanguageChanged -= OnLanguageChanged;
        }

        private void BackupInvalidSettingsFile(string path, string reason)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return;
                }

                string backupPath = path + "." + reason + "." + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".bak";
                File.Copy(path, backupPath, true);
            }
            catch (Exception exception)
            {
                ReportWarning("Settings backup failed. " + exception.Message);
            }
        }

#if UNITY_EDITOR
        public void SetStoragePathForTests(string path)
        {
            storagePathOverride = path;
        }

        public void ClearStoragePathForTests()
        {
            storagePathOverride = null;
        }
#endif
    }
}
