using System;
using UnityEngine;

namespace Kaleidoscope2.Tunnel
{
    [Serializable]
    public sealed class TunnelBendSettings
    {
        [SerializeField] private float bendSpeed = 0.65f;
        [SerializeField] private float bendLimit = 1.0f;
        [SerializeField] private float bendReturnSpeed = 0.35f;
        [SerializeField] private float bendSmoothTime = 0.18f;
        [SerializeField] private float foldStrength = 0.45f;
        [SerializeField] private float foldShadowStrength = 0.55f;
        [SerializeField] private float darknessDepth = 0.85f;
        [SerializeField] private float endLightVisibility = 0.25f;

        public float BendSpeed
        {
            get { return bendSpeed; }
        }

        public float BendLimit
        {
            get { return Mathf.Max(0.01f, bendLimit); }
        }

        public float BendReturnSpeed
        {
            get { return Mathf.Max(0.01f, bendReturnSpeed); }
        }

        public float BendSmoothTime
        {
            get { return Mathf.Max(0.01f, bendSmoothTime); }
        }

        public float FoldStrength
        {
            get { return Mathf.Max(0f, foldStrength); }
        }

        public float FoldShadowStrength
        {
            get { return Mathf.Max(0f, foldShadowStrength); }
        }

        public float DarknessDepth
        {
            get { return Mathf.Clamp01(darknessDepth); }
        }

        public float EndLightVisibility
        {
            get { return Mathf.Clamp01(endLightVisibility); }
        }
    }
}
