using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.RealMesh
{
    public sealed class CrystalLightRigModule
    {
        private struct LightSlot
        {
            public readonly string Name;
            public readonly Color Color;
            public readonly float Radius;
            public readonly float Height;
            public readonly float Phase;
            public readonly float IntensityScale;

            public LightSlot(string name, Color color, float radius, float height, float phase, float intensityScale)
            {
                Name = name;
                Color = color;
                Radius = radius;
                Height = height;
                Phase = phase;
                IntensityScale = intensityScale;
            }
        }

        private static readonly LightSlot[] Slots =
        {
            new LightSlot("RealCrystal_Key_WarmWhite", new Color(1f, 0.94f, 0.82f, 1f), 2.8f, 1.3f, 0.2f, 0.35f),
            new LightSlot("RealCrystal_Rim_CoolBlue", new Color(0.45f, 0.68f, 1f, 1f), 2.35f, 0.15f, 2.4f, 0.28f),
            new LightSlot("RealCrystal_Rim_Cyan", new Color(0.25f, 1f, 0.95f, 1f), 2.45f, -0.2f, 4.15f, 0.26f),
            new LightSlot("RealCrystal_Glint_01", new Color(0.62f, 0.85f, 1f, 1f), 2.05f, 0.72f, 0f, 0.18f),
            new LightSlot("RealCrystal_Glint_02", new Color(0.35f, 1f, 0.96f, 1f), 2.14f, -0.48f, 1.05f, 0.16f),
            new LightSlot("RealCrystal_Glint_03", new Color(1f, 0.92f, 0.78f, 1f), 2.08f, 0.32f, 2.1f, 0.15f),
            new LightSlot("RealCrystal_Glint_04", new Color(1f, 0.34f, 0.28f, 1f), 2.22f, 0.88f, 3.15f, 0.12f),
            new LightSlot("RealCrystal_Glint_05", new Color(0.74f, 0.42f, 1f, 1f), 2.12f, -0.24f, 4.2f, 0.14f),
            new LightSlot("RealCrystal_Glint_06", new Color(0.68f, 0.95f, 1f, 1f), 2.24f, 0.46f, 5.25f, 0.16f)
        };

        private GameObject root;
        private Light[] lights;
        private float orbitPhase;

        public int ActiveLightCount { get; private set; }

        public int RuntimeObjectCount
        {
            get
            {
                int count = root != null ? 1 : 0;
                if (lights == null)
                {
                    return count;
                }

                for (int index = 0; index < lights.Length; index++)
                {
                    if (lights[index] != null)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public void Ensure(Transform parent, int layer)
        {
            if (root == null)
            {
                root = new GameObject("RealMeshCrystalLightRig")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                root.transform.SetParent(parent, false);
            }

            if (lights != null && lights.Length == Slots.Length)
            {
                return;
            }

            ShutdownLightsOnly();
            lights = new Light[Slots.Length];
            int cullingMask = 1 << Mathf.Clamp(layer, 0, 31);
            for (int index = 0; index < Slots.Length; index++)
            {
                LightSlot slot = Slots[index];
                GameObject lightObject = new GameObject(slot.Name)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                lightObject.transform.SetParent(root.transform, false);
                lightObject.layer = Mathf.Clamp(layer, 0, 31);
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = slot.Color;
                light.range = 4.2f;
                light.shadows = LightShadows.None;
                light.bounceIntensity = 0f;
                light.cullingMask = cullingMask;
                lights[index] = light;
            }
        }

        public void Tick(Vector3 center, float deltaTime, float intensity, float rotation01, bool visible)
        {
            if (lights == null)
            {
                ActiveLightCount = 0;
                return;
            }

            if (!visible)
            {
                SetEnabled(false);
                return;
            }

            if (root != null && !root.activeSelf)
            {
                root.SetActive(true);
            }

            orbitPhase += Mathf.Max(0f, deltaTime) * Mathf.Lerp(0.18f, 0.72f, rotation01);
            float safeIntensity = Mathf.Clamp(intensity, 0f, 20f);
            ActiveLightCount = 0;

            for (int index = 0; index < lights.Length; index++)
            {
                Light light = lights[index];
                if (light == null)
                {
                    continue;
                }

                LightSlot slot = Slots[index];
                float phase = orbitPhase + slot.Phase;
                Vector3 position = center + new Vector3(
                    Mathf.Cos(phase) * slot.Radius,
                    slot.Height,
                    Mathf.Sin(phase) * slot.Radius);
                light.transform.position = position;
                Vector3 lookDirection = center - position;
                if (lookDirection.sqrMagnitude > 0.0001f)
                {
                    light.transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
                }

                light.enabled = visible && safeIntensity > 0.0001f;
                light.intensity = light.enabled ? safeIntensity * slot.IntensityScale * (0.75f + rotation01 * 0.65f) : 0f;
                if (light.enabled)
                {
                    ActiveLightCount++;
                }
            }
        }

        public void SetEnabled(bool enabled)
        {
            if (root != null && root.activeSelf != enabled)
            {
                root.SetActive(enabled);
            }

            ActiveLightCount = 0;
            if (lights == null)
            {
                return;
            }

            for (int index = 0; index < lights.Length; index++)
            {
                Light light = lights[index];
                if (light == null)
                {
                    continue;
                }

                if (!enabled)
                {
                    light.enabled = false;
                    light.intensity = 0f;
                }
            }
        }

        public void Shutdown()
        {
            ShutdownLightsOnly();
            if (root != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(root);
                }
                else
                {
                    Object.DestroyImmediate(root);
                }
            }

            root = null;
            ActiveLightCount = 0;
        }

        private void ShutdownLightsOnly()
        {
            if (lights == null)
            {
                return;
            }

            for (int index = 0; index < lights.Length; index++)
            {
                if (lights[index] == null)
                {
                    continue;
                }

                GameObject lightObject = lights[index].gameObject;
                if (Application.isPlaying)
                {
                    Object.Destroy(lightObject);
                }
                else
                {
                    Object.DestroyImmediate(lightObject);
                }
            }

            lights = null;
            ActiveLightCount = 0;
        }
    }
}
