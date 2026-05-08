using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Diagnostics
{
    // Collects runtime diagnostics data into KaleidoscopeState.Diagnostics.
    // Does not render any on-screen text (output must remain clean).
    [DisallowMultipleComponent]
    public sealed class DiagnosticsModule : KaleidoscopeModuleBase
    {
        [SerializeField] private float smoothing = 0.15f;

        private float smoothedDelta;

        public override string ModuleId
        {
            get { return "Diagnostics"; }
        }

        protected override void OnInitialized()
        {
            smoothedDelta = 0f;
        }

        public override void Tick(float deltaTime)
        {
            if (State == null)
            {
                return;
            }

            // Exponential moving average FPS.
            float dt = Mathf.Max(0.00001f, Time.unscaledDeltaTime);
            if (smoothedDelta <= 0f)
            {
                smoothedDelta = dt;
            }
            else
            {
                float t = Mathf.Clamp01(1f - Mathf.Exp(-deltaTime / Mathf.Max(0.01f, smoothing)));
                smoothedDelta = Mathf.Lerp(smoothedDelta, dt, t);
            }

            State.SetFramesPerSecond(1f / smoothedDelta);
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            float fps = State != null ? State.Diagnostics.FramesPerSecond : 0f;
            return CreateStatus("FPS " + fps.ToString("0.0"));
        }
    }
}
