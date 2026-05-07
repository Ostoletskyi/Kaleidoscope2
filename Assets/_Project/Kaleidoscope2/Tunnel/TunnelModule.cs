using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Tunnel
{
    [DisallowMultipleComponent]
    public sealed class TunnelModule : KaleidoscopeModuleBase
    {
        [SerializeField] private RenderTexture finalKaleidoscopeTexture;

        public override string ModuleId
        {
            get { return "Tunnel"; }
        }

        public RenderTexture FinalKaleidoscopeTexture
        {
            get { return finalKaleidoscopeTexture; }
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            return command != null && command.Type == KaleidoscopeCommandType.SetTunnelEnabled;
        }

        public override void Validate()
        {
            if (State != null && State.TunnelEnabled && finalKaleidoscopeTexture == null)
            {
                ReportMissingReference("FinalKaleidoscopeTexture");
            }
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            bool enabled = State != null && State.TunnelEnabled;
            return CreateStatus("Placeholder. Tunnel enabled: " + enabled + ". Rendering starts in Stage 11.");
        }
    }
}
