namespace Kaleidoscope2.Menu
{
    internal enum KaelisMenuSection
    {
        None,
        Modes,
        Optics,
        Presets,
        Settings,
        Exit
    }

    internal enum KaelisMenuPanelCommand
    {
        None,
        CloseSection,
        ApplyClassicMode,
        ApplyTunnelMode,
        ApplyFiveDMode,
        ShowDiagnostics,
        SetSecondDisplayOutput,
        TestSecondDisplayOutput,
        SetRecordingEnabled,
        SelectRecordingOutputFolder,
        ClearRecordingOutputFolder,
        ReservedAction,
        CancelExit,
        SaveAndExit,
        ExitWithoutSaving
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
}
