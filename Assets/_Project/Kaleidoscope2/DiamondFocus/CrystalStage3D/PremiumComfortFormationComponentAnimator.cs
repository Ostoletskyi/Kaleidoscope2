using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.CrystalStage3D
{
    public struct PremiumComfortFormationComponentPose
    {
        public PremiumComfortFormationComponentPose(
            Vector3 localOffset,
            Vector3 rotationOffset,
            Vector3 scaleMultiplier,
            float alphaMultiplier,
            float intensityMultiplier,
            bool visible)
        {
            LocalOffset = localOffset;
            RotationOffset = rotationOffset;
            ScaleMultiplier = scaleMultiplier;
            AlphaMultiplier = alphaMultiplier;
            IntensityMultiplier = intensityMultiplier;
            Visible = visible;
        }

        public Vector3 LocalOffset { get; private set; }
        public Vector3 RotationOffset { get; private set; }
        public Vector3 ScaleMultiplier { get; private set; }
        public float AlphaMultiplier { get; private set; }
        public float IntensityMultiplier { get; private set; }
        public bool Visible { get; private set; }
    }

    public static class PremiumComfortFormationComponentAnimator
    {
        public static PremiumComfortFormationComponentPose Resolve(
            int index,
            PremiumComfortFormationLayoutResult layout,
            PremiumComfortFormationMorphState morphState)
        {
            int resolvedIndex = Mathf.Clamp(index, 0, PremiumComfortFormationLayout.CopyCount - 1);
            float radius = Mathf.Max(0f, layout.LocalRadius);
            Vector3 targetOffset = PremiumComfortFormationLayout.ResolveOrbitOffset(
                resolvedIndex,
                morphState.OrbitAngleRadians,
                radius);
            Vector3 seedOffset = ResolveSeedOffset(resolvedIndex, radius);
            Vector3 localOffset = Vector3.Lerp(seedOffset, targetOffset, morphState.ComponentSeparation);
            localOffset.z += Mathf.Sin(morphState.OrbitAngleRadians * 0.7f + resolvedIndex * 1.17f)
                * Mathf.Min(0.035f, radius * 0.06f)
                * morphState.SurfaceRedistribution;

            Vector3 orbitRotation = PremiumComfortFormationLayout.ResolveRotationOffset(
                resolvedIndex,
                morphState.OrbitAngleRadians);
            Vector3 foldRotation = ResolveFoldRotation(resolvedIndex) * morphState.SurfaceRedistribution;
            Vector3 rotationOffset = Vector3.Lerp(foldRotation, orbitRotation, morphState.ComponentSeparation);
            float pulse = morphState.SurfaceRedistribution * Mathf.Sin(resolvedIndex * 1.618f + morphState.OrbitAngleRadians);
            float scale = Mathf.Max(PremiumComfortFormationLayout.MinimumChildScale, layout.ChildScale * morphState.ComponentScaleMultiplier);
            Vector3 scaleMultiplier = new Vector3(
                scale * (1f + pulse * 0.035f),
                scale * (1f - pulse * 0.025f),
                scale * (1f + morphState.SurfaceRedistribution * 0.07f));
            float alpha = Mathf.Clamp01(morphState.ComponentAlphaMultiplier);
            float intensity = Mathf.Max(0f, morphState.ComponentIntensityMultiplier);
            bool visible = alpha > 0.002f && scale > 0.001f;
            return new PremiumComfortFormationComponentPose(localOffset, rotationOffset, scaleMultiplier, alpha, intensity, visible);
        }

        private static Vector3 ResolveSeedOffset(int index, float radius)
        {
            float angle = index * Mathf.PI * 2f / PremiumComfortFormationLayout.CopyCount;
            float seedRadius = Mathf.Max(0.018f, Mathf.Min(radius * 0.11f, 0.09f));
            return new Vector3(Mathf.Cos(angle) * seedRadius, Mathf.Sin(angle) * seedRadius, (index % 2 == 0 ? 1f : -1f) * 0.018f);
        }

        private static Vector3 ResolveFoldRotation(int index)
        {
            return new Vector3(
                Mathf.Sin(index * 1.37f) * 8f,
                Mathf.Cos(index * 0.91f) * 6f,
                (index - 2.5f) * 4f);
        }
    }
}
