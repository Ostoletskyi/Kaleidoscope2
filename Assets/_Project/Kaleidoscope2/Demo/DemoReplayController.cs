using System;
using System.Collections.Generic;
using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Demo
{
    [DisallowMultipleComponent]
    public sealed class DemoReplayController : KaleidoscopeModuleBase
    {
        private KaleidoscopeDirector director;
        private SettingsRestoreService restoreService;
        private InputRecorder recorder;
        private VisualSessionUiController sessionUi;
        private List<RecordedSemanticAction> sequence;
        private int sequenceIndex;
        private float nextDispatchIn;
        private string status = "Replay Demo ready after at least one visual control action.";

        public override string ModuleId { get { return "DemoReplay"; } }
        public bool IsRunning { get { return restoreService != null && restoreService.IsActive(TemporarySessionKind.ReplayDemo); } }

        public void Configure(KaleidoscopeDirector owner, SettingsRestoreService restore, InputRecorder inputRecorder, VisualSessionUiController presentationUi)
        {
            director = owner;
            restoreService = restore;
            recorder = inputRecorder;
            sessionUi = presentationUi;
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            return command != null
                && (command.Type == KaleidoscopeCommandType.StartReplayDemo
                    || command.Type == KaleidoscopeCommandType.CancelTemporarySession);
        }

        public override void HandleCommand(KaleidoscopeCommand command)
        {
            if (command.Type == KaleidoscopeCommandType.StartReplayDemo)
            {
                StartReplay();
            }
            else if (IsRunning)
            {
                StopReplay("Replay Demo cancelled and prior settings restored.");
            }
        }

        public override void Tick(float deltaTime)
        {
            if (!IsRunning || director == null || sequence == null || sequence.Count == 0)
            {
                return;
            }

            nextDispatchIn -= Mathf.Max(0f, deltaTime);
            try
            {
                while (nextDispatchIn <= 0f && IsRunning)
                {
                    RecordedSemanticAction action = sequence[sequenceIndex];
                    director.Dispatch(action.Command, KaleidoscopeCommandOrigin.Replay);
                    sequenceIndex = (sequenceIndex + 1) % sequence.Count;
                    nextDispatchIn += Mathf.Max(0.01f, sequence[sequenceIndex].DeltaSeconds);
                }
            }
            catch (Exception exception)
            {
                FailAndRestore("Replay Demo failed during playback; prior settings restored.", exception);
            }
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            return CreateStatus(status);
        }

        private void StartReplay()
        {
            if (recorder == null || recorder.Count == 0)
            {
                status = "Replay Demo unavailable: interact with visual controls first.";
                return;
            }

            if (director == null || restoreService == null || !restoreService.TryBeginSession(TemporarySessionKind.ReplayDemo))
            {
                status = "Replay Demo unavailable while another temporary session is active.";
                return;
            }

            sequence = recorder.BuildPlaybackSequence();
            sequenceIndex = 0;
            nextDispatchIn = sequence.Count > 0 ? Mathf.Max(0.01f, sequence[0].DeltaSeconds) : 0f;
            try
            {
                sessionUi?.BeginPresentation(TemporarySessionKind.ReplayDemo, "REPLAY DEMO", KaleidoscopeCommandOrigin.Replay);
                director.Dispatch(KaleidoscopeCommand.SetDemoImageContent(DemoContentCatalog.ReplayProfileId), KaleidoscopeCommandOrigin.Replay);
                director.Dispatch(KaleidoscopeCommand.SetSourceMode(KaleidoscopeSourceMode.ImageTexture), KaleidoscopeCommandOrigin.Replay);
                status = "Replay Demo looping 500 semantic actions. Escape or middle mouse stops.";
            }
            catch (Exception exception)
            {
                FailAndRestore("Replay Demo failed to start; prior settings restored.", exception);
            }
        }

        private void StopReplay(string finalStatus)
        {
            if (!IsRunning)
            {
                return;
            }

            restoreService.RestoreAndEnd(TemporarySessionKind.ReplayDemo);
            sessionUi?.EndPresentation(TemporarySessionKind.ReplayDemo);
            sequence = null;
            sequenceIndex = 0;
            nextDispatchIn = 0f;
            status = finalStatus;
        }

        private void FailAndRestore(string finalStatus, Exception exception)
        {
            ReportWarning(finalStatus + " " + exception.Message);
            if (restoreService != null)
            {
                restoreService.RestoreAndEnd(TemporarySessionKind.ReplayDemo);
            }

            sessionUi?.EndPresentation(TemporarySessionKind.ReplayDemo);
            sequence = null;
            sequenceIndex = 0;
            nextDispatchIn = 0f;
            status = finalStatus;
        }
    }
}
