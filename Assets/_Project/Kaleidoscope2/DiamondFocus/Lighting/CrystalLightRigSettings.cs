using System;
using UnityEngine;

namespace Kaleidoscope2.Core
{
    [Serializable]
    public sealed class CrystalLightRigSettings
    {
        public const float LightIntensityMin = 0f;
        public const float LightIntensityMax = 20f;
        public const int ActiveLightCountMin = 1;
        public const int ActiveLightCountMax = 8;

        [SerializeField] private bool rigEnabled = true;
        [SerializeField, Range(0f, 120f)] private float orbitSpeed = 12f;
        [SerializeField, Range(LightIntensityMin, LightIntensityMax)] private float lightIntensity = 8f;
        [SerializeField, Range(ActiveLightCountMin, ActiveLightCountMax)] private int activeLightCountLimit = ActiveLightCountMax;
        [SerializeField, Range(0f, 3f)] private float glintIntensity = 1.05f;
        [SerializeField, Range(0f, 3f)] private float rimIntensity = 0.95f;
        [SerializeField, Range(0f, 3f)] private float spectralIntensity = 0.72f;
        [SerializeField, Range(0f, 1f)] private float pulseAmount = 0.28f;
        [SerializeField, Range(0.05f, 8f)] private float pulseSpeed = 1.15f;

        [Header("Diagnostics")]
        [SerializeField, InspectorName("Light rig enabled")] private bool diagnosticsRigEnabled = true;
        [SerializeField] private int activeOrbitLights;
        [SerializeField] private float resolvedGlintIntensity;
        [SerializeField] private float resolvedRimIntensity;
        [SerializeField] private float resolvedSpectralIntensity;
        [SerializeField] private float orbitPhaseRadians;

        public bool RigEnabled { get { return rigEnabled; } }
        public float OrbitSpeed { get { return Mathf.Max(0f, orbitSpeed); } }
        public float LightIntensity { get { return Mathf.Clamp(lightIntensity, LightIntensityMin, LightIntensityMax); } }
        public int ActiveLightCountLimit { get { return Mathf.Clamp(activeLightCountLimit, ActiveLightCountMin, ActiveLightCountMax); } }
        public float GlintIntensity { get { return Mathf.Max(0f, glintIntensity); } }
        public float RimIntensity { get { return Mathf.Max(0f, rimIntensity); } }
        public float SpectralIntensity { get { return Mathf.Max(0f, spectralIntensity); } }
        public float PulseAmount { get { return Mathf.Clamp01(pulseAmount); } }
        public float PulseSpeed { get { return Mathf.Max(0.05f, pulseSpeed); } }
        public int ActiveOrbitLights { get { return Mathf.Max(0, activeOrbitLights); } }
        public float ResolvedGlintIntensity { get { return Mathf.Max(0f, resolvedGlintIntensity); } }
        public float ResolvedRimIntensity { get { return Mathf.Max(0f, resolvedRimIntensity); } }
        public float ResolvedSpectralIntensity { get { return Mathf.Max(0f, resolvedSpectralIntensity); } }
        public float OrbitPhaseRadians { get { return orbitPhaseRadians; } }

        public void SetRigEnabled(bool value)
        {
            rigEnabled = value;
            diagnosticsRigEnabled = value;
        }

        public void ToggleRigEnabled()
        {
            SetRigEnabled(!rigEnabled);
        }

        public void SetLightIntensity(float value)
        {
            lightIntensity = Mathf.Clamp(value, LightIntensityMin, LightIntensityMax);
        }

        public void AdjustLightIntensity(float delta)
        {
            SetLightIntensity(LightIntensity + delta);
        }

        public void SetActiveLightCountLimit(int value)
        {
            activeLightCountLimit = Mathf.Clamp(value, ActiveLightCountMin, ActiveLightCountMax);
        }

        public void SetDiagnostics(
            int activeOrbitLightCount,
            float glint,
            float rim,
            float spectral,
            float orbitPhase)
        {
            diagnosticsRigEnabled = rigEnabled;
            activeOrbitLights = Mathf.Max(0, activeOrbitLightCount);
            resolvedGlintIntensity = Mathf.Max(0f, glint);
            resolvedRimIntensity = Mathf.Max(0f, rim);
            resolvedSpectralIntensity = Mathf.Max(0f, spectral);
            orbitPhaseRadians = orbitPhase;
        }
    }
}
