using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.AudioReactive
{
    [DisallowMultipleComponent]
    public sealed class AudioReactiveModule : KaleidoscopeModuleBase
    {
        public override string ModuleId
        {
            get { return "AudioReactive"; }
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            return CreateStatus("Placeholder. Audio events will emit commands through Director in Stage 10.");
        }
    }
}
