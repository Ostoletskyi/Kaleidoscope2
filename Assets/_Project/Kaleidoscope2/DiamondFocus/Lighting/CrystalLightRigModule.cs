using Kaleidoscope2.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.Lighting
{
    [DisallowMultipleComponent]
    public sealed class CrystalLightRigModule : KaleidoscopeModuleBase
    {
        private enum LightRole
        {
            Key,
            Rim,
            Glint,
            Spectral
        }

        private struct LightSlot
        {
            public readonly string Name;
            public readonly LightRole Role;
            public readonly Color Color;
            public readonly float Radius;
            public readonly float Height;
            public readonly float Phase;
            public readonly float Range;
            public readonly float IntensityScale;
            public readonly float PulseScale;
            public readonly bool CountsAsOrbitLight;

            public LightSlot(
                string name,
                LightRole role,
                Color color,
                float radius,
                float height,
                float phase,
                float range,
                float intensityScale,
                float pulseScale,
                bool countsAsOrbitLight)
            {
                Name = name;
                Role = role;
                Color = color;
                Radius = radius;
                Height = height;
                Phase = phase;
                Range = range;
                IntensityScale = intensityScale;
                PulseScale = pulseScale;
                CountsAsOrbitLight = countsAsOrbitLight;
            }
        }

        private static readonly LightSlot[] Slots =
        {
            new LightSlot("Crystal Soft Key Warm White", LightRole.Key, new Color(1f, 0.93f, 0.82f, 1f), 2.9f, 1.15f, 0.15f, 5.8f, 0.16f, 0.12f, false),
            new LightSlot("Crystal Rim Cool Blue", LightRole.Rim, new Color(0.45f, 0.68f, 1f, 1f), 2.45f, 0.28f, 2.35f, 4.8f, 0.12f, 0.24f, false),
            new LightSlot("Crystal Rim Cyan", LightRole.Rim, new Color(0.35f, 1f, 0.96f, 1f), 2.5f, -0.12f, 4.25f, 4.8f, 0.11f, 0.24f, false),
            new LightSlot("Crystal Orbit Glint 01 Blue", LightRole.Glint, new Color(0.52f, 0.8f, 1f, 1f), 2.15f, 0.62f, 0f, 3.5f, 0.08f, 0.42f, true),
            new LightSlot("Crystal Orbit Glint 02 Cyan", LightRole.Glint, new Color(0.3f, 1f, 0.95f, 1f), 2.25f, -0.42f, 1.05f, 3.4f, 0.075f, 0.48f, true),
            new LightSlot("Crystal Orbit Glint 03 Warm", LightRole.Glint, new Color(1f, 0.92f, 0.78f, 1f), 2.05f, 0.22f, 2.1f, 3.3f, 0.07f, 0.38f, true),
            new LightSlot("Crystal Orbit Glint 04 Red", LightRole.Glint, new Color(1f, 0.38f, 0.32f, 1f), 2.35f, 0.82f, 3.15f, 3.25f, 0.055f, 0.36f, true),
            new LightSlot("Crystal Orbit Glint 05 Violet", LightRole.Glint, new Color(0.78f, 0.48f, 1f, 1f), 2.18f, -0.22f, 4.2f, 3.3f, 0.065f, 0.44f, true),
            new LightSlot("Crystal Orbit Glint 06 Ice", LightRole.Glint, new Color(0.72f, 0.94f, 1f, 1f), 2.32f, 0.44f, 5.25f, 3.45f, 0.08f, 0.46f, true),
            new LightSlot("Crystal Spectral Accent Red", LightRole.Spectral, new Color(1f, 0.22f, 0.18f, 1f), 2.65f, 0.1f, 0.72f, 3.6f, 0.045f, 0.62f, true),
            new LightSlot("Crystal Spectral Accent Violet", LightRole.Spectral, new Color(0.68f, 0.36f, 1f, 1f), 2.7f, 0.72f, 2.9f, 3.6f, 0.045f, 0.62f, true),
            new LightSlot("Crystal Spectral Accent Cyan", LightRole.Spectral, new Color(0.22f, 0.95f, 1f, 1f), 2.72f, -0.52f, 4.85f, 3.6f, 0.045f, 0.62f, true)
        };

        [Header("Target")]
        [SerializeField] private Transform crystalTarget;
        [SerializeField, Range(0, 31)] private int crystalLayer = 31;

        private readonly List<CrystalOrbitLight> orbitLights = new List<CrystalOrbitLight>(Slots.Length);
        private GameObject rigRoot;
        private float elapsedSeconds;
        private float orbitPhaseRadians;

        public override string ModuleId
        {
            get { return "CrystalLightRig"; }
        }

        public override void Tick(float deltaTime)
        {
            DiamondFocusSettings diamondSettings = State != null ? State.DiamondFocusSettings : null;
            CrystalLightRigSettings settings = diamondSettings != null ? diamondSettings.CrystalLightRigSettings : null;
            if (settings == null)
            {
                SetAllLightsEnabled(false);
                return;
            }

            EnsureRig();

            elapsedSeconds += Mathf.Max(0f, deltaTime);
            orbitPhaseRadians += Mathf.Max(0f, deltaTime) * settings.OrbitSpeed * Mathf.Deg2Rad;
            if (orbitPhaseRadians > Mathf.PI * 4096f)
            {
                orbitPhaseRadians = Mathf.Repeat(orbitPhaseRadians, Mathf.PI * 2f);
            }

            bool enabled = settings.RigEnabled && diamondSettings.Enabled;
            float rotation01 = diamondSettings.NormalizedRotationSpeed;
            float pulseWave = Mathf.Sin(elapsedSeconds * settings.PulseSpeed * Mathf.PI * 2f) * 0.5f + 0.5f;
            float rotationPulse = settings.PulseAmount * rotation01 * pulseWave;
            float normalizedIntensity = settings.LightIntensity / CrystalLightRigSettings.LightIntensityMax;
            float resolvedGlint = enabled ? normalizedIntensity * settings.GlintIntensity * (0.72f + rotation01 * 1.22f) * (1f + rotationPulse) : 0f;
            float resolvedRim = enabled ? normalizedIntensity * settings.RimIntensity * (0.78f + rotation01 * 0.62f) * (1f + rotationPulse * 0.6f) : 0f;
            float resolvedSpectral = enabled ? normalizedIntensity * settings.SpectralIntensity * (0.52f + rotation01 * 1.5f) * (1f + rotationPulse) : 0f;
            int activeOrbitLights = 0;
            int activeLightLimit = settings.ActiveLightCountLimit;
            Vector3 center = crystalTarget != null ? crystalTarget.position : transform.position;
            int count = Mathf.Min(orbitLights.Count, Slots.Length);

            for (int index = 0; index < count; index++)
            {
                LightSlot slot = Slots[index];
                CrystalOrbitLight orbitLight = orbitLights[index];
                float intensity = ResolveSlotIntensity(slot.Role, settings, rotation01, rotationPulse);
                bool lightEnabled = enabled && index < activeLightLimit && intensity > 0.0001f;
                orbitLight.Tick(center, orbitPhaseRadians, intensity, rotationPulse, lightEnabled);
                if (lightEnabled && slot.CountsAsOrbitLight)
                {
                    activeOrbitLights++;
                }
            }

            settings.SetDiagnostics(activeOrbitLights, resolvedGlint, resolvedRim, resolvedSpectral, orbitPhaseRadians);
        }

        public override void Validate()
        {
            if (crystalLayer < 0 || crystalLayer > 31)
            {
                ReportWarning("Crystal layer must be in the Unity layer range 0..31.");
            }
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            DiamondFocusSettings diamondSettings = State != null ? State.DiamondFocusSettings : null;
            CrystalLightRigSettings settings = diamondSettings != null ? diamondSettings.CrystalLightRigSettings : null;
            if (settings == null)
            {
                return CreateStatus("Waiting for crystal light rig settings.");
            }

            return CreateStatus("Light rig "
                + (settings.RigEnabled ? "enabled" : "disabled")
                + ", active light limit " + settings.ActiveLightCountLimit
                + ", active orbit lights " + settings.ActiveOrbitLights
                + ", light intensity " + settings.LightIntensity.ToString("0.00") + " / 20"
                + ", glint intensity " + settings.ResolvedGlintIntensity.ToString("0.00")
                + ", spectral intensity " + settings.ResolvedSpectralIntensity.ToString("0.00") + ".");
        }

        protected override void OnActivated()
        {
            SetAllLightsEnabled(true);
        }

        protected override void OnDeactivated()
        {
            SetAllLightsEnabled(false);
        }

        private void OnDestroy()
        {
            DestroyRuntimeObject(rigRoot);
            rigRoot = null;
            orbitLights.Clear();
        }

        private void EnsureRig()
        {
            if (rigRoot == null)
            {
                rigRoot = new GameObject("Kaleidoscope2_CrystalLightRig_Runtime")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                rigRoot.transform.SetParent(transform, false);
            }

            if (orbitLights.Count == Slots.Length)
            {
                return;
            }

            for (int index = 0; index < orbitLights.Count; index++)
            {
                if (orbitLights[index] != null)
                {
                    DestroyRuntimeObject(orbitLights[index].gameObject);
                }
            }

            orbitLights.Clear();
            int cullingMask = 1 << Mathf.Clamp(crystalLayer, 0, 31);
            for (int index = 0; index < Slots.Length; index++)
            {
                LightSlot slot = Slots[index];
                GameObject lightObject = new GameObject(slot.Name)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                lightObject.transform.SetParent(rigRoot.transform, false);
                CrystalOrbitLight orbitLight = lightObject.AddComponent<CrystalOrbitLight>();
                orbitLight.Configure(
                    slot.Name,
                    slot.Color,
                    slot.Radius,
                    slot.Height,
                    slot.Phase,
                    slot.Range,
                    slot.IntensityScale,
                    slot.PulseScale,
                    slot.CountsAsOrbitLight,
                    cullingMask);
                orbitLights.Add(orbitLight);
            }
        }

        private static float ResolveSlotIntensity(
            LightRole role,
            CrystalLightRigSettings settings,
            float rotation01,
            float rotationPulse)
        {
            float baseIntensity = settings.LightIntensity;
            switch (role)
            {
                case LightRole.Key:
                    return baseIntensity * (0.82f + rotation01 * 0.18f);
                case LightRole.Rim:
                    return baseIntensity * settings.RimIntensity * (0.62f + rotation01 * 0.55f) * (1f + rotationPulse * 0.75f);
                case LightRole.Spectral:
                    return baseIntensity * settings.SpectralIntensity * (0.35f + rotation01 * 1.05f) * (1f + rotationPulse);
                default:
                    return baseIntensity * settings.GlintIntensity * (0.4f + rotation01 * 1.25f) * (1f + rotationPulse);
            }
        }

        private void SetAllLightsEnabled(bool enabled)
        {
            for (int index = 0; index < orbitLights.Count; index++)
            {
                if (orbitLights[index] != null)
                {
                    orbitLights[index].SetEnabled(enabled);
                }
            }
        }

        private void DestroyRuntimeObject(Object instance)
        {
            if (instance == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(instance);
            }
            else
            {
                DestroyImmediate(instance);
            }
        }
    }
}
