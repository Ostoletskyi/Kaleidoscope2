using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.RealMesh.CrystalStage3D
{
    public sealed class CrystalStage3DLightRig
    {
        private struct LightSlot
        {
            public readonly string Name;
            public readonly LightType Type;
            public readonly Color Color;
            public readonly float Radius;
            public readonly float Height;
            public readonly float Phase;
            public readonly float IntensityScale;

            public LightSlot(string name, LightType type, Color color, float radius, float height, float phase, float intensityScale)
            {
                Name = name;
                Type = type;
                Color = color;
                Radius = radius;
                Height = height;
                Phase = phase;
                IntensityScale = intensityScale;
            }
        }

        private static readonly LightSlot[] Slots =
        {
            new LightSlot("CrystalStage3D_Key_WarmWhite", LightType.Point, new Color(1f, 0.94f, 0.82f, 1f), 3.1f, 2.45f, 0.25f, 0.45f),
            new LightSlot("CrystalStage3D_Rim_CoolBlue", LightType.Point, new Color(0.42f, 0.68f, 1f, 1f), 3f, 2.15f, 2.6f, 0.34f),
            new LightSlot("CrystalStage3D_Rim_Cyan", LightType.Point, new Color(0.22f, 1f, 0.94f, 1f), 3f, -2.05f, 4.2f, 0.32f),
            new LightSlot("CrystalStage3D_Glint_01", LightType.Point, new Color(0.62f, 0.86f, 1f, 1f), 2.7f, 2.1f, 0f, 0.2f),
            new LightSlot("CrystalStage3D_Glint_02", LightType.Point, new Color(0.35f, 1f, 0.96f, 1f), 2.75f, -2f, 1.05f, 0.18f),
            new LightSlot("CrystalStage3D_Glint_03", LightType.Point, new Color(1f, 0.92f, 0.78f, 1f), 2.65f, 1.8f, 2.1f, 0.18f),
            new LightSlot("CrystalStage3D_Glint_04", LightType.Point, new Color(1f, 0.34f, 0.28f, 1f), 2.85f, 2.05f, 3.15f, 0.14f),
            new LightSlot("CrystalStage3D_Glint_05", LightType.Point, new Color(0.74f, 0.42f, 1f, 1f), 2.72f, -1.9f, 4.2f, 0.16f),
            new LightSlot("CrystalStage3D_Glint_06", LightType.Point, new Color(0.68f, 0.95f, 1f, 1f), 2.8f, 1.95f, 5.25f, 0.18f),
            new LightSlot("CrystalStage3D_Spectral_Violet", LightType.Point, new Color(0.7f, 0.36f, 1f, 1f), 3.15f, 2.2f, 1.7f, 0.12f),
            new LightSlot("CrystalStage3D_Spectral_Red", LightType.Point, new Color(1f, 0.25f, 0.2f, 1f), 3.05f, -2.15f, 5.1f, 0.1f)
        };

        private GameObject root;
        private Light[] lights;
        private float orbitPhase;
        private string diagnosticsLabel = "light rig not created";

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

        public string DiagnosticsLabel
        {
            get { return diagnosticsLabel; }
        }

        public void Ensure(Transform parent, int layer)
        {
            if (root == null)
            {
                root = new GameObject("CrystalStage3D_LightRig")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                root.transform.SetParent(parent, false);
            }

            int safeLayer = Mathf.Clamp(layer, 0, 31);
            root.layer = safeLayer;
            if (lights != null && lights.Length == Slots.Length)
            {
                return;
            }

            ShutdownLightsOnly();
            lights = new Light[Slots.Length];
            int cullingMask = 1 << safeLayer;
            for (int index = 0; index < Slots.Length; index++)
            {
                LightSlot slot = Slots[index];
                GameObject lightObject = new GameObject(slot.Name)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                lightObject.transform.SetParent(root.transform, false);
                lightObject.layer = safeLayer;

                Light light = lightObject.AddComponent<Light>();
                light.type = slot.Type;
                light.color = slot.Color;
                light.range = 5.5f;
                light.shadows = LightShadows.None;
                light.bounceIntensity = 0f;
                light.cullingMask = cullingMask;
                lights[index] = light;
            }
        }

        public void Tick(float deltaTime, float intensity, float rotation01, float crystalScale, int activeLightLimit, bool visible)
        {
            if (lights == null)
            {
                ActiveLightCount = 0;
                diagnosticsLabel = "light rig has no lights";
                return;
            }

            if (!visible)
            {
                SetVisible(false);
                return;
            }

            if (root != null && !root.activeSelf)
            {
                root.SetActive(true);
            }

            float safeIntensity = Mathf.Clamp(intensity, 0f, 20f);
            float scale = Mathf.Max(1f, crystalScale);
            int lightLimit = Mathf.Clamp(activeLightLimit, CrystalLightRigSettings.ActiveLightCountMin, CrystalLightRigSettings.ActiveLightCountMax);
            orbitPhase += Mathf.Max(0f, deltaTime) * Mathf.Lerp(0.25f, 1.1f, Mathf.Clamp01(rotation01));
            ActiveLightCount = 0;

            for (int index = 0; index < lights.Length; index++)
            {
                Light light = lights[index];
                if (light == null)
                {
                    continue;
                }

                LightSlot slot = Slots[index];
                bool slotActive = index < lightLimit;
                float phase = orbitPhase + slot.Phase;
                Vector3 position = new Vector3(
                    Mathf.Cos(phase) * slot.Radius * scale,
                    slot.Height * scale,
                    Mathf.Sin(phase) * slot.Radius * scale);
                light.transform.localPosition = position;
                light.range = 5.5f * scale;
                Vector3 direction = -position;
                if (direction.sqrMagnitude > 0.0001f)
                {
                    light.transform.localRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                }

                float pulse = 1f + Mathf.Sin((orbitPhase + slot.Phase) * 3.3f) * 0.08f * rotation01;
                light.intensity = safeIntensity * slot.IntensityScale * pulse;
                light.enabled = slotActive && light.intensity > 0.0001f;
                if (light.enabled)
                {
                    ActiveLightCount++;
                }
            }

            diagnosticsLabel = "active lights " + ActiveLightCount.ToString()
                + " / " + lightLimit.ToString()
                + ", intensity " + safeIntensity.ToString("0.00");
        }

        public void SetVisible(bool visible)
        {
            if (root != null && root.activeSelf != visible)
            {
                root.SetActive(visible);
            }

            if (!visible)
            {
                ActiveLightCount = 0;
            }

            if (lights == null)
            {
                return;
            }

            for (int index = 0; index < lights.Length; index++)
            {
                if (lights[index] != null && !visible)
                {
                    lights[index].enabled = false;
                    lights[index].intensity = 0f;
                }
            }
        }

        public void Shutdown()
        {
            ShutdownLightsOnly();
            DestroyRuntimeObject(root);
            root = null;
            ActiveLightCount = 0;
            diagnosticsLabel = "light rig not created";
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

                DestroyRuntimeObject(lights[index].gameObject);
            }

            lights = null;
            ActiveLightCount = 0;
        }

        private static void DestroyRuntimeObject(Object instance)
        {
            if (instance == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(instance);
            }
            else
            {
                Object.DestroyImmediate(instance);
            }
        }
    }
}
