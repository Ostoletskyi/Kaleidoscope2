using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Recording
{
    [DisallowMultipleComponent]
    public sealed class RecordingModule : KaleidoscopeModuleBase
    {
        [SerializeField] private RenderTexture finalOutputTexture;

        public override string ModuleId
        {
            get { return "Recording"; }
        }

        public RenderTexture FinalOutputTexture
        {
            get { return finalOutputTexture; }
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            return command != null && command.Type == KaleidoscopeCommandType.SetRecordingStatus;
        }

        public override void Validate()
        {
            if (State != null && State.RecordingStatus != KaleidoscopeRecordingStatus.Idle && finalOutputTexture == null)
            {
                ReportMissingReference("FinalOutputTexture");
            }
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            KaleidoscopeRecordingStatus status = State != null ? State.RecordingStatus : KaleidoscopeRecordingStatus.Idle;
            return CreateStatus("Placeholder. Recording status " + status + ". Export starts in Stage 12.");
        }
    }
}
