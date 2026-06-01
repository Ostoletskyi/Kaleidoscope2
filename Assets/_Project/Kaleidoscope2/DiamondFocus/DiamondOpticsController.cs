using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus
{
    public static class DiamondOpticalShaderIds
    {
        public static readonly int Transparency = Shader.PropertyToID("_Transparency");
        public static readonly int RefractionStrength = Shader.PropertyToID("_RefractionStrength");
        public static readonly int ReflectionStrength = Shader.PropertyToID("_ReflectionStrength");
        public static readonly int FresnelPower = Shader.PropertyToID("_FresnelPower");
        public static readonly int EdgeHighlight = Shader.PropertyToID("_EdgeHighlight");
        public static readonly int ChromaticAberrationAmount = Shader.PropertyToID("_ChromaticAberrationAmount");
        public static readonly int FacetContrast = Shader.PropertyToID("_FacetContrast");
        public static readonly int InternalGlow = Shader.PropertyToID("_InternalGlow");
        public static readonly int BloomBoost = Shader.PropertyToID("_BloomBoost");
        public static readonly int FocusAmount = Shader.PropertyToID("_FocusAmount");
        public static readonly int Shape = Shader.PropertyToID("_Shape");
        public static readonly int Rotation = Shader.PropertyToID("_Rotation");
        public static readonly int DiamondScale = Shader.PropertyToID("_DiamondScale");
        public static readonly int TexelSize = Shader.PropertyToID("_InputTexelSize");
        public static readonly int KaleidoscopeTex = Shader.PropertyToID("_KaleidoscopeTex");
        public static readonly int KaleidoscopeTexValid = Shader.PropertyToID("_KaleidoscopeTexValid");
        public static readonly int FallbackWarningColor = Shader.PropertyToID("_FallbackWarningColor");
        public static readonly int CrystalTex = Shader.PropertyToID("_CrystalTex");
        public static readonly int BlurredBackgroundTex = Shader.PropertyToID("_BlurredBackgroundTex");
        public static readonly int CrystalTexelSize = Shader.PropertyToID("_CrystalTexelSize");
        public static readonly int BackgroundBlurAmount = Shader.PropertyToID("_BackgroundBlurAmount");
        public static readonly int ComfortSplitAmount = Shader.PropertyToID("_ComfortSplitAmount");
        public static readonly int ComfortOrbitAngle = Shader.PropertyToID("_ComfortOrbitAngle");
        public static readonly int BlurDirection = Shader.PropertyToID("_BlurDirection");
        public static readonly int BlurRadius = Shader.PropertyToID("_BlurRadius");
        public static readonly int CrystalMaterialMode = Shader.PropertyToID("_CrystalMaterialMode");
        public static readonly int GeneratedColor = Shader.PropertyToID("_GeneratedColor");
        public static readonly int GeneratedMaterialKind = Shader.PropertyToID("_GeneratedMaterialKind");
        public static readonly int OpticalIOR = Shader.PropertyToID("_OpticalIOR");
        public static readonly int OpticalCaustics = Shader.PropertyToID("_OpticalCaustics");
        public static readonly int OpticalDispersion = Shader.PropertyToID("_OpticalDispersion");
        public static readonly int TotalInternalReflection = Shader.PropertyToID("_TotalInternalReflection");
        public static readonly int DiamondLikeRefraction = Shader.PropertyToID("_DiamondLikeRefraction");
        public static readonly int SpectralDispersion = Shader.PropertyToID("_SpectralDispersion");
        public static readonly int HighEnergyCaustics = Shader.PropertyToID("_HighEnergyCaustics");
        public static readonly int MultiBounceInternalReflections = Shader.PropertyToID("_MultiBounceInternalReflections");
        public static readonly int CinematicCrystalOptics = Shader.PropertyToID("_CinematicCrystalOptics");
        public static readonly int PhysicallyBasedRefraction = Shader.PropertyToID("_PhysicallyBasedRefraction");
        public static readonly int DeepVolumetricLightScattering = Shader.PropertyToID("_DeepVolumetricLightScattering");
        public static readonly int CrystalSolidity = Shader.PropertyToID("_CrystalSolidity");
        public static readonly int BlueWhitePlasmaEnergy = Shader.PropertyToID("_BlueWhitePlasmaEnergy");
        public static readonly int DirectTransmission = Shader.PropertyToID("_DirectTransmission");
        public static readonly int TotalInternalReturn = Shader.PropertyToID("_TotalInternalReturn");
        public static readonly int SpectralFireIntensity = Shader.PropertyToID("_SpectralFireIntensity");
        public static readonly int FacetDepthContrast = Shader.PropertyToID("_FacetDepthContrast");
        public static readonly int DispersionStrength = Shader.PropertyToID("_DispersionStrength");
        public static readonly int InternalBrightness = Shader.PropertyToID("_InternalBrightness");
        public static readonly int NoiseDistortionStrength = Shader.PropertyToID("_NoiseDistortionStrength");
        public static readonly int CrystalDebugMode = Shader.PropertyToID("_CrystalDebugMode");
        public static readonly int SurfaceTint = Shader.PropertyToID("_SurfaceTint");
        public static readonly int SurfaceTintStrength = Shader.PropertyToID("_SurfaceTintStrength");
        public static readonly int Metallic = Shader.PropertyToID("_Metallic");
        public static readonly int Roughness = Shader.PropertyToID("_Roughness");
        public static readonly int ScratchStrength = Shader.PropertyToID("_ScratchStrength");
        public static readonly int Iridescence = Shader.PropertyToID("_Iridescence");
        public static readonly int SurfacePatternStrength = Shader.PropertyToID("_SurfacePatternStrength");
        public static readonly int Clarity = Shader.PropertyToID("_Clarity");
        public static readonly int CrystalSurfaceFamily = Shader.PropertyToID("_CrystalSurfaceFamily");
        public static readonly int SampleMipBias = Shader.PropertyToID("_SampleMipBias");
        public static readonly int DirectedLightIntensity = Shader.PropertyToID("_DirectedLightIntensity");
        public static readonly int CrystalLightRigEnabled = Shader.PropertyToID("_CrystalLightRigEnabled");
        public static readonly int CrystalRigLightIntensity = Shader.PropertyToID("_CrystalRigLightIntensity");
        public static readonly int CrystalRigGlintIntensity = Shader.PropertyToID("_CrystalRigGlintIntensity");
        public static readonly int CrystalRigRimIntensity = Shader.PropertyToID("_CrystalRigRimIntensity");
        public static readonly int CrystalRigSpectralIntensity = Shader.PropertyToID("_CrystalRigSpectralIntensity");
        public static readonly int CrystalRigPulse = Shader.PropertyToID("_CrystalRigPulse");
        public static readonly int CrystalRigOrbitPhase = Shader.PropertyToID("_CrystalRigOrbitPhase");
    }

    public sealed class DiamondOpticsController
    {
        private readonly CrystalMaterialBinder crystalMaterialBinder = new CrystalMaterialBinder();

        public float FocusAmount { get; private set; }
        public float BackgroundBlur { get; private set; }

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

            crystalMaterialBinder.ConfigureCrystalMaterial(material, settings, kaleidoscopeTexture, kaleidoscopeTextureValid, fallbackWarningColor);
            FocusAmount = crystalMaterialBinder.FocusAmount;
        }

        public void ConfigureCompositeMaterial(Material material, DiamondFocusSettings settings, Texture sourceTexture, Texture blurredBackgroundTexture, Texture crystalTexture, float backgroundBlurAmount)
        {
            if (material == null || settings == null || sourceTexture == null || blurredBackgroundTexture == null || crystalTexture == null)
            {
                return;
            }

            FocusAmount = settings.NormalizedRotationSpeed;
            BackgroundBlur = Mathf.Clamp01(backgroundBlurAmount);

            material.SetFloat(DiamondOpticalShaderIds.FocusAmount, FocusAmount);
            material.SetFloat(DiamondOpticalShaderIds.BackgroundBlurAmount, BackgroundBlur);
            material.SetVector(DiamondOpticalShaderIds.TexelSize, new Vector4(1f / sourceTexture.width, 1f / sourceTexture.height, sourceTexture.width, sourceTexture.height));
            material.SetVector(DiamondOpticalShaderIds.CrystalTexelSize, new Vector4(1f / crystalTexture.width, 1f / crystalTexture.height, crystalTexture.width, crystalTexture.height));
            material.SetTexture(DiamondOpticalShaderIds.BlurredBackgroundTex, blurredBackgroundTexture);
            material.SetTexture(DiamondOpticalShaderIds.CrystalTex, crystalTexture);
        }

        public void ConfigureBlurMaterial(Material material, Texture sourceTexture, Vector2 direction, float radius)
        {
            if (material == null || sourceTexture == null)
            {
                return;
            }

            material.SetVector(DiamondOpticalShaderIds.TexelSize, new Vector4(1f / sourceTexture.width, 1f / sourceTexture.height, sourceTexture.width, sourceTexture.height));
            material.SetVector(DiamondOpticalShaderIds.BlurDirection, new Vector4(direction.x, direction.y, 0f, 0f));
            material.SetFloat(DiamondOpticalShaderIds.BlurRadius, Mathf.Max(0f, radius));
        }
    }
}
