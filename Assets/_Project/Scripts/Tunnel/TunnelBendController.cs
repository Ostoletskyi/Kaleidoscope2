using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Tunnel
{
    public sealed class TunnelBendController
    {
        public void Reset(KaleidoscopeState state)
        {
            if (state == null)
            {
                return;
            }

            if (state.TunnelBendState != null)
            {
                state.TunnelBendState.Reset();
            }
        }

        public void Tick(KaleidoscopeState state, Vector2 input, float deltaTime)
        {
            if (state == null)
            {
                return;
            }

            if (state.TunnelBendSettings == null || state.TunnelBendState == null)
            {
                state.EnsureInitialized();
            }

            TunnelBendSettings settings = state.TunnelBendSettings;
            TunnelBendState bendState = state.TunnelBendState;

            if (settings == null || bendState == null)
            {
                return;
            }

            deltaTime = Mathf.Max(0f, deltaTime);

            Vector2 offset = bendState.BendOffset;
            Vector2 velocity = bendState.BendVelocity;

            bool hasInput = input.sqrMagnitude > 0.0001f;
            Vector2 target = hasInput
                ? offset + Vector2.ClampMagnitude(input, 1f) * settings.BendSpeed * deltaTime
                : Vector2.zero;

            float smoothTime = hasInput
                ? settings.BendSmoothTime
                : Mathf.Max(0.01f, 1f / settings.BendReturnSpeed);

            Vector2 next = Vector2.SmoothDamp(
                offset,
                target,
                ref velocity,
                smoothTime,
                Mathf.Max(0.01f, settings.BendSpeed * 2f, settings.BendLimit * 8f),
                deltaTime);

            next = Vector2.ClampMagnitude(next, settings.BendLimit);

            bendState.SetBendOffset(next);
            bendState.SetBendVelocity(velocity);
        }
    }
}
