using System;
using Kaleidoscope2.AudioReactive;
using Kaleidoscope2.Core;
using Kaleidoscope2.Source;
using UnityEngine;

namespace Kaleidoscope2.Demo
{
    public enum TemporarySessionKind
    {
        None = 0,
        Meditation = 1,
        ReplayDemo = 2,
        BenchmarkDemo = 3
    }

    [Serializable]
    public sealed class SettingsSnapshot
    {
        public string StateJson;
        public SourceRuntimeSnapshot Source;
        public AudioRuntimeSnapshot Audio;
        public bool HasSourceSnapshot;
        public bool HasAudioSnapshot;
    }

    [DisallowMultipleComponent]
    public sealed class SettingsRestoreService : KaleidoscopeModuleBase
    {
        private KaleidoscopeDirector director;
        private SettingsSnapshot activeSnapshot;
        private TemporarySessionKind activeSession;
        private string lastMessage = "No temporary session active.";

        public override string ModuleId { get { return "SettingsRestore"; } }
        public TemporarySessionKind ActiveSession { get { return activeSession; } }
        public bool HasActiveSession { get { return activeSession != TemporarySessionKind.None; } }

        public void Configure(KaleidoscopeDirector owner)
        {
            director = owner;
        }

        public bool TryBeginSession(TemporarySessionKind kind)
        {
            if (kind == TemporarySessionKind.None || HasActiveSession || State == null)
            {
                lastMessage = HasActiveSession
                    ? "Stop " + activeSession + " before starting another temporary session."
                    : "Unable to acquire a temporary session snapshot.";
                return false;
            }

            SettingsSnapshot snapshot;
            if (!TryCapture(out snapshot))
            {
                lastMessage = "Temporary session blocked because settings snapshot capture failed.";
                return false;
            }

            activeSnapshot = snapshot;
            activeSession = kind;
            lastMessage = kind + " snapshot captured.";
            return true;
        }

        public bool RestoreAndEnd(TemporarySessionKind kind)
        {
            if (!HasActiveSession || activeSession != kind)
            {
                return false;
            }

            ComfortSafetyManager comfort = DemoRuntimeLookup.FindModule<ComfortSafetyManager>(director);
            if (comfort != null)
            {
                comfort.ClearTemporaryPolicy();
            }

            bool restored = Restore(activeSnapshot);
            if (!restored && State != null && State.CrystalSplitPresentation != null)
            {
                State.CrystalSplitPresentation.SetEnabled(false);
                State.CrystalSplitPresentation.SetCycle(0f, 0f);
            }

            activeSnapshot = null;
            activeSession = TemporarySessionKind.None;
            lastMessage = restored ? kind + " state restored." : kind + " ended with a restore warning.";
            return restored;
        }

        public new bool IsActive(TemporarySessionKind kind)
        {
            return activeSession == kind;
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            return CreateStatus(lastMessage);
        }

        private bool TryCapture(out SettingsSnapshot snapshot)
        {
            snapshot = null;
            try
            {
                SettingsSnapshot captured = new SettingsSnapshot
                {
                    StateJson = JsonUtility.ToJson(State)
                };

                SourceModule source = DemoRuntimeLookup.FindModule<SourceModule>(director);
                if (source != null)
                {
                    captured.Source = source.CaptureRuntimeSnapshot();
                    captured.HasSourceSnapshot = true;
                }

                AudioReactiveModule audio = DemoRuntimeLookup.FindModule<AudioReactiveModule>(director);
                if (audio != null)
                {
                    captured.Audio = audio.CaptureRuntimeSnapshot();
                    captured.HasAudioSnapshot = true;
                }

                snapshot = captured;
                return !string.IsNullOrWhiteSpace(captured.StateJson);
            }
            catch (Exception exception)
            {
                ReportWarning("Snapshot capture failed: " + exception.Message);
                return false;
            }
        }

        private bool Restore(SettingsSnapshot snapshot)
        {
            if (snapshot == null || State == null || string.IsNullOrWhiteSpace(snapshot.StateJson))
            {
                return false;
            }

            try
            {
                JsonUtility.FromJsonOverwrite(snapshot.StateJson, State);
                State.EnsureInitialized();

                SourceModule source = DemoRuntimeLookup.FindModule<SourceModule>(director);
                if (source != null && snapshot.HasSourceSnapshot)
                {
                    source.RestoreRuntimeSnapshot(snapshot.Source);
                }

                AudioReactiveModule audio = DemoRuntimeLookup.FindModule<AudioReactiveModule>(director);
                if (audio != null && snapshot.HasAudioSnapshot)
                {
                    audio.RestoreRuntimeSnapshot(snapshot.Audio);
                }

                if (director != null)
                {
                    director.Dispatch(KaleidoscopeCommand.SetControlMenuVisible(State.ControlMenuVisible), KaleidoscopeCommandOrigin.Restore);
                    director.Dispatch(KaleidoscopeCommand.SetHotkeysHelpVisible(State.HotkeysHelpVisible), KaleidoscopeCommandOrigin.Restore);
                    director.Dispatch(KaleidoscopeCommand.SetDiagnosticsVisible(State.Diagnostics.HudVisible), KaleidoscopeCommandOrigin.Restore);
                }

                return true;
            }
            catch (Exception exception)
            {
                ReportWarning("Snapshot restore failed: " + exception.Message);
                return false;
            }
        }
    }

    internal static class DemoRuntimeLookup
    {
        public static T FindModule<T>(KaleidoscopeDirector director) where T : class
        {
            if (director == null)
            {
                return null;
            }

            for (int index = 0; index < director.RegisteredModules.Count; index++)
            {
                T module = director.RegisteredModules[index] as T;
                if (module != null)
                {
                    return module;
                }
            }

            return null;
        }
    }
}
