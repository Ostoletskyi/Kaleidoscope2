namespace Kaleidoscope2.Menu
{
    internal enum KaelisMenuSection
    {
        None,
        Modes,
        Optics,
        Presets,
        Settings,
        About,
        Demo,
        MeditationSetup,
        ReplaySetup,
        Benchmark,
        Exit
    }

    internal enum KaelisMenuPanelCommand
    {
        None,
        CloseSection,
        ApplyClassicMode,
        ApplyPremium3DMode,
        ApplyTunnelMode,
        ApplyFiveDMode,
        ShowDiagnostics,
        SetSecondDisplayOutput,
        TestSecondDisplayOutput,
        SetRecordingEnabled,
        SelectRecordingOutputFolder,
        ClearRecordingOutputFolder,
        ApplySelectedPreset,
        OpenAboutPanel,
        OpenMeditationPanel,
        StartMeditationMode,
        ToggleCrystalSplitComfort,
        OpenReplayPanel,
        StartReplayDemo,
        OpenBenchmarkPanel,
        StartBenchmarkDemo,
        CancelTemporarySession,
        SaveBenchmarkResult,
        CycleLanguage,
        SelectPremiumCrystalShape,
        SelectPremiumCrystalOpticalMode,
        SelectFactoryPresetCard,
        ApplyExperimentalPresetCard,
        SetAutoSaveSettings,
        ReservedAction,
        CancelExit,
        SaveAndExit,
        ExitWithoutSaving
    }

    internal enum KaelisMenuBindingStatus
    {
        RealBinding,
        PartialBinding,
        Reserved
    }

    internal sealed class KaelisProductionOptions
    {
        public bool OutputToSecondDisplay { get; set; }
        public bool SecondDisplayAvailable { get; set; }
        public bool CreateVideoClip { get; set; }
        public string RecordingOutputFolder { get; set; }
        public bool AutoStartRecordingWithExperience { get; set; } = true;
        public bool IncludeAudioIfAvailable { get; set; } = true;
        public bool IsRecording { get; set; }
        public bool RecordingBackendAvailable { get; set; }

        public bool HasRecordingOutputFolder
        {
            get { return !string.IsNullOrWhiteSpace(RecordingOutputFolder); }
        }
    }

    internal sealed class KaelisSettingsMenuOptions
    {
        public bool AutoSaveSettings { get; set; }
    }
}
