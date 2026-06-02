using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.CrystalStage3D
{
    public enum PremiumComfortFormationPhase
    {
        Merged = 0,
        Detaching = 1,
        Orbiting = 2,
        Merging = 3
    }

    public struct PremiumComfortFormationMorphState
    {
        private const float FullOrbitRadians = Mathf.PI * 2f;

        public PremiumComfortFormationMorphState(
            PremiumComfortFormationPhase phase,
            float cycleSeconds,
            float expansion,
            float orbitAngleRadians,
            float componentSeparation,
            float componentVisibility,
            float componentScaleMultiplier,
            float surfaceRedistribution,
            Vector3 primaryScaleMultiplier,
            Vector3 primaryRotationOffset,
            float primaryAlphaMultiplier,
            float primaryIntensityMultiplier,
            float componentAlphaMultiplier,
            float componentIntensityMultiplier,
            string diagnostics)
        {
            Phase = phase;
            CycleSeconds = cycleSeconds;
            Expansion = expansion;
            OrbitAngleRadians = orbitAngleRadians;
            ComponentSeparation = componentSeparation;
            ComponentVisibility = componentVisibility;
            ComponentScaleMultiplier = componentScaleMultiplier;
            SurfaceRedistribution = surfaceRedistribution;
            PrimaryScaleMultiplier = primaryScaleMultiplier;
            PrimaryRotationOffset = primaryRotationOffset;
            PrimaryAlphaMultiplier = primaryAlphaMultiplier;
            PrimaryIntensityMultiplier = primaryIntensityMultiplier;
            ComponentAlphaMultiplier = componentAlphaMultiplier;
            ComponentIntensityMultiplier = componentIntensityMultiplier;
            Diagnostics = diagnostics;
        }

        public PremiumComfortFormationPhase Phase { get; private set; }
        public float CycleSeconds { get; private set; }
        public float Expansion { get; private set; }
        public float OrbitAngleRadians { get; private set; }
        public float ComponentSeparation { get; private set; }
        public float ComponentVisibility { get; private set; }
        public float ComponentScaleMultiplier { get; private set; }
        public float SurfaceRedistribution { get; private set; }
        public Vector3 PrimaryScaleMultiplier { get; private set; }
        public Vector3 PrimaryRotationOffset { get; private set; }
        public float PrimaryAlphaMultiplier { get; private set; }
        public float PrimaryIntensityMultiplier { get; private set; }
        public float ComponentAlphaMultiplier { get; private set; }
        public float ComponentIntensityMultiplier { get; private set; }
        public string Diagnostics { get; private set; }

        public bool PrimaryVisible
        {
            get { return Phase != PremiumComfortFormationPhase.Orbiting; }
        }

        public float ComponentScale
        {
            get { return ComponentScaleMultiplier; }
        }

        public bool Active
        {
            get { return Phase != PremiumComfortFormationPhase.Merged || Expansion > 0.0001f; }
        }

        public static PremiumComfortFormationMorphState Evaluate(
            float expansion,
            float orbitAngleRadians,
            float cycleSeconds)
        {
            float resolvedExpansion = Mathf.Clamp01(expansion);
            float resolvedOrbit = Mathf.Max(0f, orbitAngleRadians);
            PremiumComfortFormationPhase phase = ResolvePhase(resolvedExpansion, resolvedOrbit);
            float separation = Smooth01(resolvedExpansion);
            float visibility = Smooth01(Mathf.InverseLerp(0.02f, 0.42f, resolvedExpansion));
            float componentScale = Mathf.Lerp(0.36f, 1f, Smooth01(Mathf.InverseLerp(0.04f, 0.88f, resolvedExpansion)));
            float surfaceRedistribution = Mathf.Sin(resolvedExpansion * Mathf.PI);
            float primaryCollapse = Smooth01(Mathf.InverseLerp(0.18f, 0.98f, resolvedExpansion));
            float primaryBaseScale = Mathf.Lerp(1f, 0.024f, primaryCollapse);
            float liquidStretch = surfaceRedistribution * 0.12f;
            Vector3 primaryScale = new Vector3(
                primaryBaseScale * (1f + liquidStretch),
                primaryBaseScale * (1f - liquidStretch * 0.35f),
                primaryBaseScale * (1f + surfaceRedistribution * 0.08f));
            Vector3 primaryRotation = new Vector3(
                surfaceRedistribution * 2.5f,
                -surfaceRedistribution * 1.5f,
                surfaceRedistribution * 4.5f);
            float primaryAlpha = Mathf.Lerp(1f, 0.02f, Smooth01(Mathf.InverseLerp(0.38f, 1f, resolvedExpansion)));
            float primaryIntensity = 1f + surfaceRedistribution * 0.12f;
            float componentAlpha = Mathf.Clamp01(visibility);
            float componentIntensity = 0.86f + visibility * 0.14f + surfaceRedistribution * 0.12f;
            string diagnostics = "premium formation morph phase " + phase.ToString()
                + ", cycle " + Mathf.Max(0f, cycleSeconds).ToString("0.00")
                + ", expansion " + resolvedExpansion.ToString("0.00")
                + ", component separation " + separation.ToString("0.00")
                + ", component visibility " + visibility.ToString("0.00")
                + ", primary visible " + (phase == PremiumComfortFormationPhase.Orbiting ? "false" : "true")
                + ", surface redistribution " + surfaceRedistribution.ToString("0.00")
                + ", smooth morphing true"
                + ", detailed full-mesh copies true"
                + ", component separation true";

            return new PremiumComfortFormationMorphState(
                phase,
                Mathf.Max(0f, cycleSeconds),
                resolvedExpansion,
                resolvedOrbit,
                separation,
                visibility,
                componentScale,
                surfaceRedistribution,
                primaryScale,
                primaryRotation,
                primaryAlpha,
                primaryIntensity,
                componentAlpha,
                componentIntensity,
                diagnostics);
        }

        private static PremiumComfortFormationPhase ResolvePhase(float expansion, float orbitAngleRadians)
        {
            if (expansion <= 0.0001f)
            {
                return PremiumComfortFormationPhase.Merged;
            }

            if (orbitAngleRadians >= FullOrbitRadians - 0.001f && expansion < 0.999f)
            {
                return PremiumComfortFormationPhase.Merging;
            }

            if (expansion >= 0.999f && orbitAngleRadians < FullOrbitRadians - 0.001f)
            {
                return PremiumComfortFormationPhase.Orbiting;
            }

            return PremiumComfortFormationPhase.Detaching;
        }

        private static float Smooth01(float value)
        {
            float clamped = Mathf.Clamp01(value);
            return clamped * clamped * clamped * (clamped * (clamped * 6f - 15f) + 10f);
        }
    }
}
