using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.PhysicsChamber
{
    [DisallowMultipleComponent]
    public sealed class PhysicsChamberModule : KaleidoscopeModuleBase
    {
        public override string ModuleId
        {
            get { return "PhysicsChamber"; }
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            return CreateStatus("Placeholder. Physical source generation starts in Stage 08.");
        }
    }
}
