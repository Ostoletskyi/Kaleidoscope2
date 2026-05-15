using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.Lighting
{
    [DisallowMultipleComponent]
    public sealed class CrystalOrbitLight : MonoBehaviour
    {
        private Light cachedLight;
        private float orbitRadius = 2.6f;
        private float orbitHeight = 0.4f;
        private float phaseRadians;
        private float intensityScale = 1f;
        private float pulseScale = 1f;
        private bool countsAsOrbitLight;

        public bool CountsAsOrbitLight
        {
            get { return countsAsOrbitLight; }
        }

        public void Configure(
            string displayName,
            Color color,
            float radius,
            float height,
            float phase,
            float range,
            float slotIntensityScale,
            float slotPulseScale,
            bool countAsOrbitLight,
            int cullingMask)
        {
            name = displayName;
            orbitRadius = Mathf.Max(0.05f, radius);
            orbitHeight = height;
            phaseRadians = phase;
            intensityScale = Mathf.Max(0f, slotIntensityScale);
            pulseScale = Mathf.Max(0f, slotPulseScale);
            countsAsOrbitLight = countAsOrbitLight;

            Light light = EnsureLight();
            light.type = LightType.Point;
            light.color = color;
            light.range = Mathf.Max(0.05f, range);
            light.intensity = 0f;
            light.shadows = LightShadows.None;
            light.bounceIntensity = 0f;
            light.renderMode = LightRenderMode.Auto;
            light.cullingMask = cullingMask;
        }

        public void Tick(Vector3 center, float orbitPhase, float resolvedIntensity, float rotationPulse, bool enabled)
        {
            Light light = EnsureLight();
            bool lightEnabled = enabled && resolvedIntensity > 0.0001f;
            light.enabled = lightEnabled;

            float phase = orbitPhase + phaseRadians;
            Vector3 position = center + new Vector3(
                Mathf.Cos(phase) * orbitRadius,
                orbitHeight,
                Mathf.Sin(phase) * orbitRadius);

            transform.position = position;
            Vector3 lookDirection = center - position;
            if (lookDirection.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            }

            light.intensity = lightEnabled
                ? Mathf.Max(0f, resolvedIntensity * intensityScale * (1f + Mathf.Max(0f, rotationPulse) * pulseScale))
                : 0f;
        }

        public void SetEnabled(bool enabled)
        {
            Light light = EnsureLight();
            light.enabled = enabled;
            if (!enabled)
            {
                light.intensity = 0f;
            }
        }

        private Light EnsureLight()
        {
            if (cachedLight == null)
            {
                cachedLight = GetComponent<Light>();
                if (cachedLight == null)
                {
                    cachedLight = gameObject.AddComponent<Light>();
                }
            }

            return cachedLight;
        }
    }
}
