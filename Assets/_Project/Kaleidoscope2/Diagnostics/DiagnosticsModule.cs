using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Diagnostics
{
    [DisallowMultipleComponent]
    public sealed class DiagnosticsModule : KaleidoscopeModuleBase
    {
        [SerializeField] private float smoothing = 0.1f;

        private float smoothedDeltaTime;

        public override string ModuleId
        {
            get { return "Diagnostics"; }
        }

        public override void Tick(float deltaTime)
        {
            if (State == null || deltaTime <= 0f)
            {
                return;
            }

            smoothedDeltaTime += (deltaTime - smoothedDeltaTime) * Mathf.Clamp01(smoothing);
            State.SetFramesPerSecond(smoothedDeltaTime > 0f ? 1f / smoothedDeltaTime : 0f);
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            return command != null
                && (command.Type == KaleidoscopeCommandType.ValidateSystem
                    || command.Type == KaleidoscopeCommandType.ClearDiagnostics);
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            float fps = State != null ? State.Diagnostics.FramesPerSecond : 0f;
            string visibility = State != null && State.Diagnostics.HudVisible ? "visible" : "hidden";
            return CreateStatus("FPS " + fps.ToString("0.0") + ". Debug HUD " + visibility + ".");
        }
    }
}
