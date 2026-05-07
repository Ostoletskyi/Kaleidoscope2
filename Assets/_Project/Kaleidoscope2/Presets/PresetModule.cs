using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Presets
{
    [DisallowMultipleComponent]
    public sealed class PresetModule : KaleidoscopeModuleBase
    {
        public override string ModuleId
        {
            get { return "Presets"; }
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            return command != null && command.Type == KaleidoscopeCommandType.SetActivePreset;
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            string preset = State != null ? State.ActivePreset : "None";
            return CreateStatus("Placeholder. Active preset " + preset + ". Preset data starts in Stage 09.");
        }
    }
}
