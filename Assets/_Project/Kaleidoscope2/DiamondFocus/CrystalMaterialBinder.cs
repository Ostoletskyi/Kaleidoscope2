using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus
{
    public sealed class CrystalMaterialBinder
    {
        private const float BaselineRefractionStrength = 0.074f;
        private const float BaselineDispersionStrength = 1f;
        private const float BaselineReflectionStrength = 0.72f;
        private const float BaselineFresnelPower = 3.2f;
        private const float BaselineInternalBrightness = 1f;
        private const float BaselineNoiseDistortionStrength = 0.08f;
        private const float BaselineEdgeHighlight = 1.35f;
        private const float BaselineChromaticAberration = 0.009f;
        private const float BaselineFacetContrast = 1.45f;
        private const float BaselineInternalGlow = 0.62f;
        private const float BaselineBloomBoost = 0.44f;

        public CrystalModeProfile LastProfile { get; private set; }
        public float FocusAmount { get; private set; }

        public void ConfigureCrystalMaterial(
            Material material,
            DiamondFocusSettings settings,
            Texture kaleidoscopeTexture,
            bool kaleidoscopeTextureValid,
            Color fallbackWarningColor)
        {
            if (material == null || settings == null)
            {
                return;
            }

            FocusAmount = settings.NormalizedRotationSpeed;
            LastProfile = CrystalModeLibrary.Resolve(
                settings.MaterialMode,
                settings.RandomSeed,
                settings.EnableRandomVariants,
                settings.RefractionCoefficient);

            CrystalOpticsProfile optics = LastProfile.Optics;
            CrystalSurfaceProfile surface = LastProfile.Surface;
            int textureWidth = kaleidoscopeTexture != null ? Mathf.Max(1, kaleidoscopeTexture.width) : 1;
            int textureHeight = kaleidoscopeTexture != null ? Mathf.Max(1, kaleidoscopeTexture.height) : 1;

            material.SetTexture(DiamondOpticalShaderIds.KaleidoscopeTex, kaleidoscopeTexture);
            material.SetFloat(DiamondOpticalShaderIds.KaleidoscopeTexValid, kaleidoscopeTextureValid ? 1f : 0f);
            material.SetColor(DiamondOpticalShaderIds.FallbackWarningColor, fallbackWarningColor);
            material.SetFloat(DiamondOpticalShaderIds.Transparency, settings.Transparency);
            material.SetFloat(DiamondOpticalShaderIds.RefractionStrength, ScaleFloat(optics.RefractionStrength, settings.RefractionStrength, BaselineRefractionStrength, 0f, 0.085f));
            material.SetFloat(DiamondOpticalShaderIds.DispersionStrength, ScaleFloat(optics.DispersionStrength, settings.DispersionStrength, BaselineDispersionStrength, 0f, 1.55f));
            material.SetFloat(DiamondOpticalShaderIds.ReflectionStrength, ScaleFloat(optics.ReflectionStrength, settings.ReflectionStrength, BaselineReflectionStrength, 0f, 1f));
            material.SetFloat(DiamondOpticalShaderIds.FresnelPower, ScaleFloat(optics.FresnelPower, settings.FresnelPower, BaselineFresnelPower, 0.6f, 9f));
            material.SetFloat(DiamondOpticalShaderIds.InternalBrightness, ScaleFloat(optics.InternalBrightness, settings.InternalBrightness, BaselineInternalBrightness, 0f, 3f));
            material.SetFloat(DiamondOpticalShaderIds.NoiseDistortionStrength, ScaleFloat(optics.NoiseDistortionStrength, settings.NoiseDistortionStrength, BaselineNoiseDistortionStrength, 0f, 0.35f));
            material.SetFloat(DiamondOpticalShaderIds.EdgeHighlight, ScaleFloat(optics.EdgeHighlight, settings.EdgeHighlight, BaselineEdgeHighlight, 0f, 2.5f));
            material.SetFloat(DiamondOpticalShaderIds.ChromaticAberrationAmount, ScaleFloat(optics.ChromaticAberrationAmount, settings.ChromaticAberrationAmount, BaselineChromaticAberration, 0f, 0.02f));
            material.SetFloat(DiamondOpticalShaderIds.FacetContrast, ScaleFloat(optics.FacetContrast, settings.FacetContrast, BaselineFacetContrast, 0f, 2.4f));
            material.SetFloat(DiamondOpticalShaderIds.InternalGlow, ScaleFloat(optics.InternalGlow, settings.InternalGlow, BaselineInternalGlow, 0f, 1.8f));
            material.SetFloat(DiamondOpticalShaderIds.BloomBoost, ScaleFloat(optics.BloomBoost, settings.BloomBoost, BaselineBloomBoost, 0f, 3f));
            material.SetFloat(DiamondOpticalShaderIds.FocusAmount, FocusAmount);
            material.SetFloat(DiamondOpticalShaderIds.Shape, settings.ShapeIndex);
            material.SetFloat(DiamondOpticalShaderIds.CrystalMaterialMode, settings.MaterialModeIndex);
            material.SetColor(DiamondOpticalShaderIds.GeneratedColor, surface.Tint);
            material.SetFloat(DiamondOpticalShaderIds.GeneratedMaterialKind, ResolveLegacyKind(surface.Family));
            material.SetFloat(DiamondOpticalShaderIds.OpticalIOR, optics.OpticalIOR);
            material.SetFloat(DiamondOpticalShaderIds.OpticalCaustics, Mathf.Clamp(optics.HighEnergyCaustics * 0.55f + settings.OpticalCaustics * 0.25f, 0f, 2f));
            material.SetFloat(DiamondOpticalShaderIds.OpticalDispersion, Mathf.Clamp(optics.SpectralDispersion, 0f, 2f));
            material.SetFloat(DiamondOpticalShaderIds.TotalInternalReflection, Mathf.Clamp(optics.TotalInternalReturn, 0f, 2f));
            material.SetFloat(DiamondOpticalShaderIds.DiamondLikeRefraction, optics.DiamondLikeRefraction);
            material.SetFloat(DiamondOpticalShaderIds.SpectralDispersion, optics.SpectralDispersion);
            material.SetFloat(DiamondOpticalShaderIds.HighEnergyCaustics, optics.HighEnergyCaustics);
            material.SetFloat(DiamondOpticalShaderIds.MultiBounceInternalReflections, optics.MultiBounceInternalReflections);
            material.SetFloat(DiamondOpticalShaderIds.CinematicCrystalOptics, optics.CinematicCrystalOptics);
            material.SetFloat(DiamondOpticalShaderIds.PhysicallyBasedRefraction, optics.PhysicallyBasedRefraction);
            material.SetFloat(DiamondOpticalShaderIds.DeepVolumetricLightScattering, optics.DeepVolumetricLightScattering);
            material.SetFloat(DiamondOpticalShaderIds.CrystalSolidity, optics.CrystalSolidity);
            material.SetFloat(DiamondOpticalShaderIds.BlueWhitePlasmaEnergy, ResolvePlasmaEnergy(surface.Family, optics));
            material.SetFloat(DiamondOpticalShaderIds.DirectTransmission, optics.DirectTransmission);
            material.SetFloat(DiamondOpticalShaderIds.TotalInternalReturn, optics.TotalInternalReturn);
            material.SetFloat(DiamondOpticalShaderIds.SpectralFireIntensity, optics.SpectralFireIntensity);
            material.SetFloat(DiamondOpticalShaderIds.FacetDepthContrast, optics.FacetDepthContrast);
            material.SetFloat(DiamondOpticalShaderIds.CrystalDebugMode, (int)settings.DebugMode);
            material.SetVector(DiamondOpticalShaderIds.Rotation, settings.RotationEuler);
            material.SetFloat(DiamondOpticalShaderIds.DiamondScale, settings.ScreenScale);
            material.SetVector(DiamondOpticalShaderIds.TexelSize, new Vector4(1f / textureWidth, 1f / textureHeight, textureWidth, textureHeight));
            material.SetColor(DiamondOpticalShaderIds.SurfaceTint, surface.Tint);
            material.SetFloat(DiamondOpticalShaderIds.SurfaceTintStrength, surface.TintStrength);
            material.SetFloat(DiamondOpticalShaderIds.Metallic, surface.Metallic);
            material.SetFloat(DiamondOpticalShaderIds.Roughness, surface.Roughness);
            material.SetFloat(DiamondOpticalShaderIds.ScratchStrength, surface.ScratchStrength);
            material.SetFloat(DiamondOpticalShaderIds.Iridescence, surface.Iridescence);
            material.SetFloat(DiamondOpticalShaderIds.SurfacePatternStrength, surface.PatternStrength);
            material.SetFloat(DiamondOpticalShaderIds.Clarity, surface.Clarity);
            material.SetFloat(DiamondOpticalShaderIds.CrystalSurfaceFamily, (int)surface.Family);
            material.SetFloat(DiamondOpticalShaderIds.SampleMipBias, optics.SampleMipBias);
            material.SetFloat(DiamondOpticalShaderIds.DirectedLightIntensity, settings.DirectedLightIntensity);
            ConfigureCrystalLightRigMaterial(material, settings);
        }

        private static void ConfigureCrystalLightRigMaterial(Material material, DiamondFocusSettings settings)
        {
            CrystalLightRigSettings lightRig = settings.CrystalLightRigSettings;
            bool rigEnabled = lightRig.RigEnabled;
            float normalizedIntensity = lightRig.LightIntensity / CrystalLightRigSettings.LightIntensityMax;
            float rotation01 = settings.NormalizedRotationSpeed;
            float fallbackGlint = normalizedIntensity * lightRig.GlintIntensity * (0.72f + rotation01 * 1.22f);
            float fallbackRim = normalizedIntensity * lightRig.RimIntensity * (0.78f + rotation01 * 0.62f);
            float fallbackSpectral = normalizedIntensity * lightRig.SpectralIntensity * (0.52f + rotation01 * 1.5f);
            bool hasLiveDiagnostics = lightRig.ActiveOrbitLights > 0;
            float glint = rigEnabled ? (hasLiveDiagnostics ? lightRig.ResolvedGlintIntensity : fallbackGlint) : 0f;
            float rim = rigEnabled ? (hasLiveDiagnostics ? lightRig.ResolvedRimIntensity : fallbackRim) : 0f;
            float spectral = rigEnabled ? (hasLiveDiagnostics ? lightRig.ResolvedSpectralIntensity : fallbackSpectral) : 0f;
            float phase = Mathf.Abs(lightRig.OrbitPhaseRadians) > 0.0001f
                ? lightRig.OrbitPhaseRadians
                : settings.RotationEuler.y * Mathf.Deg2Rad;

            material.SetFloat(DiamondOpticalShaderIds.CrystalLightRigEnabled, rigEnabled ? 1f : 0f);
            material.SetFloat(DiamondOpticalShaderIds.CrystalRigLightIntensity, rigEnabled ? lightRig.LightIntensity : 0f);
            material.SetFloat(DiamondOpticalShaderIds.CrystalRigGlintIntensity, glint);
            material.SetFloat(DiamondOpticalShaderIds.CrystalRigRimIntensity, rim);
            material.SetFloat(DiamondOpticalShaderIds.CrystalRigSpectralIntensity, spectral);
            material.SetFloat(DiamondOpticalShaderIds.CrystalRigPulse, rigEnabled ? lightRig.PulseAmount * rotation01 : 0f);
            material.SetFloat(DiamondOpticalShaderIds.CrystalRigOrbitPhase, phase);
        }

        private static float ScaleFloat(float profileValue, float inspectorValue, float baseline, float min, float max)
        {
            float scale = baseline > 0.0001f ? Mathf.Max(0f, inspectorValue) / baseline : 1f;
            return Mathf.Clamp(profileValue * scale, min, max);
        }

        private static float ResolveLegacyKind(CrystalSurfaceFamily family)
        {
            switch (family)
            {
                case CrystalSurfaceFamily.Metal:
                    return 1f;
                case CrystalSurfaceFamily.Plastic:
                    return 2f;
                case CrystalSurfaceFamily.Gemstone:
                    return 3f;
                default:
                    return 0f;
            }
        }

        private static float ResolvePlasmaEnergy(CrystalSurfaceFamily family, CrystalOpticsProfile optics)
        {
            switch (family)
            {
                case CrystalSurfaceFamily.Metal:
                    return 0.18f;
                case CrystalSurfaceFamily.Plastic:
                    return 0.34f + optics.SpectralFireIntensity * 0.12f;
                case CrystalSurfaceFamily.Gemstone:
                    return 0.42f + optics.SpectralFireIntensity * 0.16f;
                default:
                    return 0.72f + optics.SpectralFireIntensity * 0.22f;
            }
        }
    }
}
