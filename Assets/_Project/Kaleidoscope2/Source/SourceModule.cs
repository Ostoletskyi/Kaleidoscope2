using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Source
{
    [DisallowMultipleComponent]
    public sealed class SourceModule : KaleidoscopeModuleBase
    {
        public override string ModuleId
        {
            get { return "Source"; }
        }

        public KaleidoscopeSourceMode CurrentSourceMode
        {
            get { return State != null ? State.ActiveSourceMode : KaleidoscopeSourceMode.None; }
        }

        public RenderTexture CurrentSourceTexture
        {
            get { return null; }
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            return command != null && command.Type == KaleidoscopeCommandType.SetSourceMode;
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            return CreateStatus("Placeholder. Source texture generation starts in Stage 03.");
        }
    }
}
