using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus
{
    public sealed class DiamondRotationController
    {
        public void Tick(DiamondFocusSettings settings, float deltaTime)
        {
            if (settings == null)
            {
                return;
            }

            deltaTime = Mathf.Max(0f, deltaTime);
            Vector2 rotationInput = settings.TargetRotationDirection;
            Vector3 velocity = settings.RotationVelocity;

            if (rotationInput.sqrMagnitude > 0.0001f)
            {
                Vector3 acceleration = settings.BuildRotationAxis(rotationInput) * settings.SpeedAcceleration * deltaTime;
                velocity += acceleration;
            }

            velocity = Vector3.ClampMagnitude(velocity, settings.MaxRotationSpeed);

            Vector3 euler = settings.RotationEuler;
            euler += velocity * deltaTime;

            settings.SetRotationVelocity(velocity);
            settings.SetRotationEuler(euler);
        }
    }
}
