using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus
{
    public static class DiamondInputRouter
    {
        public static Vector2 NormalizeDirectionInput(bool left, bool right, bool up, bool down)
        {
            Vector2 direction = Vector2.zero;

            if (left)
            {
                direction.x -= 1f;
            }

            if (right)
            {
                direction.x += 1f;
            }

            if (up)
            {
                direction.y += 1f;
            }

            if (down)
            {
                direction.y -= 1f;
            }

            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        public static Vector2 NormalizeNumpadRotationInput(
            bool downLeft,
            bool down,
            bool downRight,
            bool left,
            bool right,
            bool upLeft,
            bool up,
            bool upRight)
        {
            Vector2 direction = Vector2.zero;

            if (downLeft)
            {
                direction += new Vector2(-1f, -1f);
            }

            if (down)
            {
                direction.y -= 1f;
            }

            if (downRight)
            {
                direction += new Vector2(1f, -1f);
            }

            if (left)
            {
                direction.x -= 1f;
            }

            if (right)
            {
                direction.x += 1f;
            }

            if (upLeft)
            {
                direction += new Vector2(-1f, 1f);
            }

            if (up)
            {
                direction.y += 1f;
            }

            if (upRight)
            {
                direction += new Vector2(1f, 1f);
            }

            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        public static void DispatchDirection(KaleidoscopeDirector director, Vector2 direction)
        {
            if (director != null)
            {
                director.Dispatch(KaleidoscopeCommand.SetDiamondRotationDirection(direction));
            }
        }

        public static void DispatchSpeedDelta(KaleidoscopeDirector director, float delta)
        {
            if (director == null || Mathf.Abs(delta) <= 0.0001f)
            {
                return;
            }

            if (delta > 0f)
            {
                director.Dispatch(KaleidoscopeCommand.IncreaseDiamondRotationSpeed(delta));
                return;
            }

            director.Dispatch(KaleidoscopeCommand.DecreaseDiamondRotationSpeed(-delta));
        }
    }
}
