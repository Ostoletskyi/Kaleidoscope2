using System;
using UnityEngine;

namespace Kaleidoscope2.Tunnel
{
    [Serializable]
    public sealed class TunnelBendState
    {
        [SerializeField] private Vector2 bendOffset = Vector2.zero;
        [SerializeField] private Vector2 bendVelocity = Vector2.zero;

        public Vector2 BendOffset
        {
            get { return bendOffset; }
        }

        public Vector2 BendVelocity
        {
            get { return bendVelocity; }
        }

        public void SetBendOffset(Vector2 value)
        {
            bendOffset = value;
        }

        public void SetBendVelocity(Vector2 value)
        {
            bendVelocity = value;
        }

        public void Reset()
        {
            bendOffset = Vector2.zero;
            bendVelocity = Vector2.zero;
        }

        public float GetMagnitude()
        {
            return Mathf.Clamp01(bendOffset.magnitude);
        }
    }
}
