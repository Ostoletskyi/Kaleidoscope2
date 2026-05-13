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
        public static readonly int SceneTex = Shader.PropertyToID("_SceneTex");
        public static readonly int CrystalTex = Shader.PropertyToID("_CrystalTex");
        public static readonly int BlurredBackgroundTex = Shader.PropertyToID("_BlurredBackgroundTex");
        public static readonly int CrystalTexelSize = Shader.PropertyToID("_CrystalTexelSize");
        public static readonly int BackgroundBlurAmount = Shader.PropertyToID("_BackgroundBlurAmount");
        public static readonly int BlurDirection = Shader.PropertyToID("_BlurDirection");
        public static readonly int BlurRadius = Shader.PropertyToID("_BlurRadius");
        public static readonly int CrystalMaterialMode = Shader.PropertyToID("_CrystalMaterialMode");
        public static readonly int GeneratedColor = Shader.PropertyToID("_GeneratedColor");
        public static readonly int GeneratedMaterialKind = Shader.PropertyToID("_GeneratedMaterialKind");
        public static readonly int OpticalIOR = Shader.PropertyToID("_OpticalIOR");
        public static readonly int OpticalCaustics = Shader.PropertyToID("_OpticalCaustics");
        public static readonly int OpticalDispersion = Shader.PropertyToID("_OpticalDispersion");
        public static readonly int TotalInternalReflection = Shader.PropertyToID("_TotalInternalReflection");
    }

    public sealed class DiamondOpticsController
    {
        public float FocusAmount { get; private set; }
        public float BackgroundBlur { get; private set; }

        public void ConfigureCrystalMaterial(Material material, DiamondFocusSettings settings, Texture sceneTexture)
        {
            if (material == null || settings == null || sceneTexture == null)
            {
                return;
            }

            FocusAmount = settings.NormalizedRotationSpeed;

            material.SetTexture(DiamondOpticalShaderIds.SceneTex, sceneTexture);
            material.SetFloat(DiamondOpticalShaderIds.Transparency, settings.Transparency);
            material.SetFloat(DiamondOpticalShaderIds.RefractionStrength, settings.RefractionStrength);
            material.SetFloat(DiamondOpticalShaderIds.ReflectionStrength, settings.ReflectionStrength);
            material.SetFloat(DiamondOpticalShaderIds.FresnelPower, settings.FresnelPower);
            material.SetFloat(DiamondOpticalShaderIds.EdgeHighlight, settings.EdgeHighlight);
            material.SetFloat(DiamondOpticalShaderIds.ChromaticAberrationAmount, settings.ChromaticAberrationAmount);
            material.SetFloat(DiamondOpticalShaderIds.FacetContrast, settings.FacetContrast);
            material.SetFloat(DiamondOpticalShaderIds.InternalGlow, settings.InternalGlow);
            material.SetFloat(DiamondOpticalShaderIds.BloomBoost, settings.BloomBoost);
            material.SetFloat(DiamondOpticalShaderIds.FocusAmount, FocusAmount);
            material.SetFloat(DiamondOpticalShaderIds.Shape, settings.ShapeIndex);
            material.SetFloat(DiamondOpticalShaderIds.CrystalMaterialMode, settings.MaterialModeIndex);
            material.SetColor(DiamondOpticalShaderIds.GeneratedColor, settings.GeneratedMaterialColor);
            material.SetFloat(DiamondOpticalShaderIds.GeneratedMaterialKind, settings.GeneratedMaterialKindIndex);
            material.SetFloat(DiamondOpticalShaderIds.OpticalIOR, settings.OpticalIOR);
            material.SetFloat(DiamondOpticalShaderIds.OpticalCaustics, settings.OpticalCaustics);
            material.SetFloat(DiamondOpticalShaderIds.OpticalDispersion, settings.OpticalDispersion);
            material.SetFloat(DiamondOpticalShaderIds.TotalInternalReflection, settings.TotalInternalReflection);
            material.SetVector(DiamondOpticalShaderIds.Rotation, settings.RotationEuler);
            material.SetFloat(DiamondOpticalShaderIds.DiamondScale, settings.ScreenScale);
            material.SetVector(DiamondOpticalShaderIds.TexelSize, new Vector4(1f / sceneTexture.width, 1f / sceneTexture.height, sceneTexture.width, sceneTexture.height));
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
