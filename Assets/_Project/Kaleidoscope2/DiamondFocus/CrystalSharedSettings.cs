using System;
using UnityEngine;

namespace Kaleidoscope2.Core
{
    [Serializable]
    public sealed class CrystalSharedSettings : ISerializationCallbackReceiver
    {
        private const float RealMeshScaleMinimum = 3f;
        private const float RealMeshScaleMaximum = 5f;
        private const float RealMeshScaleDefault = 3.6f;
        private const float CrystalDistanceDefault = 14f;

        [SerializeField] private CrystalRenderMode renderMode = CrystalRenderMode.Billboard2D;
        [SerializeField] private bool visible = true;
        [SerializeField] private CrystalShape shape = CrystalShape.ClassicDiamond;
        [SerializeField] private CrystalShape shapeTransitionFromShape = CrystalShape.ClassicDiamond;
        [SerializeField] private CrystalShape shapeTransitionToShape = CrystalShape.ClassicDiamond;
        [SerializeField] private bool shapeTransitionActive;
        [SerializeField, Range(0f, 1f)] private float shapeTransitionProgress = 1f;
        [SerializeField] private CrystalMaterialMode materialMode = CrystalMaterialMode.Diamond;
        [SerializeField] private DiamondCrystalMaterialMode premiumMaterialMode = DiamondCrystalMaterialMode.Diamond;
        [SerializeField] private string premiumMaterialName = "Diamond";
        [SerializeField] private Color gemBaseColor = new Color(0.92f, 0.98f, 1f, 1f);
        [SerializeField] private Color gemCoreColor = new Color(0.78f, 0.93f, 1f, 1f);
        [SerializeField] private Color gemFireColor = new Color(1f, 0.86f, 0.42f, 1f);
        [SerializeField, Range(0f, 1f)] private float gemTintStrength = 0.16f;
        [SerializeField, Range(0f, 2f)] private float opticalDensity = 0.72f;
        [SerializeField, Range(0f, 2f)] private float facetRefraction = 1.12f;
        [SerializeField, Range(0f, 2f)] private float thicknessRefraction = 1.05f;
        [SerializeField, Range(0f, 2f)] private float internalReflection = 1.18f;
        [SerializeField, Range(0f, 2f)] private float spectralDispersion = 1.18f;
        [SerializeField, Range(0f, 2f)] private float facetFire = 1.08f;
        [SerializeField, Range(0f, 1f)] private float depthAbsorption = 0.42f;
        [SerializeField, Range(0f, 1f)] private float gemClarity = 0.9f;
        [SerializeField, Range(1f, 2.9f)] private float refractiveIndex = 2.417f;
        [SerializeField, Range(0f, 0.08f)] private float physicalDispersion = 0.044f;
        [SerializeField, Range(0f, 2f)] private float absorptionStrength = 0.38f;
        [SerializeField, Range(0f, 2f)] private float fresnelStrength = 1.35f;
        [SerializeField, Range(0f, 2f)] private float backgroundDistortionStrength = 1.15f;
        [SerializeField, Range(0f, 2f)] private float saturationBoost = 1.08f;
        [SerializeField, Range(0f, 2f)] private float contrastBoost = 1.08f;
        [SerializeField, Range(0f, 1f)] private float opalIridescence;
        [SerializeField] private Vector3 rotation;
        [SerializeField, Range(0f, 20f)] private float intensity = 8f;
        [SerializeField, Range(3f, 5f)] private float realMeshScale = 3.6f;
        [SerializeField, Range(4f, 16f)] private float crystalDistanceFromCamera = 14f;
        [SerializeField] private bool realMeshScreenCenter = true;
        [SerializeField] private Vector2 crystalScreenCenterOffset;
        [SerializeField] private bool useSolidGeometryValidationMaterial;
        [SerializeField] private bool cameraOrbitEnabled;
        [SerializeField] private bool crystalRotationEnabled = true;
        [SerializeField, Range(CrystalLightRigSettings.ActiveLightCountMin, CrystalLightRigSettings.ActiveLightCountMax)] private int realMeshLightCount = CrystalLightRigSettings.ActiveLightCountMax;
        [SerializeField] private bool crystalLightRigEnabled = true;
        [SerializeField, Range(DiamondFocusSettings.PremiumCrystalScalePercentMin, DiamondFocusSettings.PremiumCrystalScalePercentMax)] private float premiumCrystalScalePercent = DiamondFocusSettings.PremiumCrystalScalePercentDefault;
        [SerializeField] private bool premiumHiddenReflectionBackgroundEnabled = true;
        [SerializeField] private bool premiumMirrorFacetsEnabled = true;
        [SerializeField] private bool premiumInternalReflectionsEnabled = true;
        [SerializeField] private bool premiumDispersionEnabled = true;
        [SerializeField] private bool premiumRefractionDistortionEnabled = true;
        [SerializeField] private bool premiumOpalIridescenceEnabled = true;
        [SerializeField] private bool premiumFacetHighlightsEnabled = true;
        [SerializeField] private bool premiumShapeMorphingEnabled = true;
        [SerializeField] private bool premiumDebugOpticalDiagnosticsEnabled;
        [SerializeField, Range(0f, 1f)] private float realMeshAlpha = 0.58f;
        [SerializeField, Range(0f, 1f)] private float transparency;
        [SerializeField, Range(0f, 0.12f)] private float refractionStrength = 0.04f;
        [SerializeField, Range(0.5f, 8f)] private float fresnelPower = 3.2f;
        [SerializeField, Range(0f, 1f)] private float reflectionStrength = 0.6f;
        [SerializeField, Range(0f, 3f)] private float internalBrightness = 1f;
        [SerializeField, Range(0f, 1f)] private float minimumTransmission = 0.1f;
        [SerializeField, Range(0f, 1f)] private float specularStrength = 0.75f;
        [SerializeField] private string sourceTextureStatus = "Unbound";
        [NonSerialized] private bool realMeshScaleMigrationPending;
        [NonSerialized] private float realMeshScaleMigrationSource;

        public CrystalRenderMode RenderMode { get { return renderMode; } }
        public bool Visible { get { return visible; } }
        public CrystalShape Shape { get { return shape; } }
        public CrystalShape ShapeTransitionFromShape { get { return shapeTransitionFromShape; } }
        public CrystalShape ShapeTransitionToShape { get { return shapeTransitionToShape; } }
        public bool ShapeTransitionActive { get { return shapeTransitionActive; } }
        public float ShapeTransitionProgress { get { return Mathf.Clamp01(shapeTransitionProgress); } }
        public CrystalMaterialMode MaterialMode { get { return materialMode; } }
        public DiamondCrystalMaterialMode PremiumMaterialMode { get { return premiumMaterialMode; } }
        public string PremiumMaterialName { get { return premiumMaterialName; } }
        public Color GemBaseColor { get { return gemBaseColor; } }
        public Color GemCoreColor { get { return gemCoreColor; } }
        public Color GemFireColor { get { return gemFireColor; } }
        public float GemTintStrength { get { return Mathf.Clamp01(gemTintStrength); } }
        public float OpticalDensity { get { return Mathf.Clamp(opticalDensity, 0f, 2f); } }
        public float FacetRefraction { get { return Mathf.Clamp(facetRefraction, 0f, 2f); } }
        public float ThicknessRefraction { get { return Mathf.Clamp(thicknessRefraction, 0f, 2f); } }
        public float InternalReflection { get { return Mathf.Clamp(internalReflection, 0f, 2f); } }
        public float SpectralDispersion { get { return Mathf.Clamp(spectralDispersion, 0f, 2f); } }
        public float FacetFire { get { return Mathf.Clamp(facetFire, 0f, 2f); } }
        public float DepthAbsorption { get { return Mathf.Clamp01(depthAbsorption); } }
        public float GemClarity { get { return Mathf.Clamp01(gemClarity); } }
        public float RefractiveIndex { get { return Mathf.Clamp(refractiveIndex, 1f, 2.9f); } }
        public float PhysicalDispersion { get { return Mathf.Clamp(physicalDispersion, 0f, 0.08f); } }
        public float AbsorptionStrength { get { return Mathf.Clamp(absorptionStrength, 0f, 2f); } }
        public float FresnelStrength { get { return Mathf.Clamp(fresnelStrength, 0f, 2f); } }
        public float BackgroundDistortionStrength { get { return Mathf.Clamp(backgroundDistortionStrength, 0f, 2f); } }
        public float SaturationBoost { get { return Mathf.Clamp(saturationBoost, 0f, 2f); } }
        public float ContrastBoost { get { return Mathf.Clamp(contrastBoost, 0f, 2f); } }
        public float OpalIridescence { get { return Mathf.Clamp01(opalIridescence); } }
        public Vector3 Rotation { get { return rotation; } }
        public float Intensity { get { return Mathf.Clamp(intensity, 0f, 20f); } }
        public float RealMeshScale { get { return Mathf.Clamp(realMeshScale, RealMeshScaleMinimum, RealMeshScaleMaximum); } }
        public float CrystalDistanceFromCamera { get { return Mathf.Clamp(crystalDistanceFromCamera, 4f, 16f); } }
        public bool RealMeshScreenCenter { get { return realMeshScreenCenter; } }
        public Vector2 CrystalScreenCenterOffset { get { return realMeshScreenCenter ? Vector2.zero : Vector2.ClampMagnitude(crystalScreenCenterOffset, 1.5f); } }
        public bool UseSolidGeometryValidationMaterial { get { return useSolidGeometryValidationMaterial; } }
        public bool CameraOrbitEnabled { get { return cameraOrbitEnabled; } }
        public bool CrystalRotationEnabled { get { return crystalRotationEnabled; } }
        public int RealMeshLightCount { get { return Mathf.Clamp(realMeshLightCount, CrystalLightRigSettings.ActiveLightCountMin, CrystalLightRigSettings.ActiveLightCountMax); } }
        public bool CrystalLightRigEnabled { get { return crystalLightRigEnabled; } }
        public float PremiumCrystalScalePercent { get { return Mathf.Clamp(premiumCrystalScalePercent, DiamondFocusSettings.PremiumCrystalScalePercentMin, DiamondFocusSettings.PremiumCrystalScalePercentMax); } }
        public float PremiumCrystalScaleMultiplier { get { return PremiumCrystalScalePercent / 100f; } }
        public bool PremiumHiddenReflectionBackgroundEnabled { get { return premiumHiddenReflectionBackgroundEnabled; } }
        public bool PremiumMirrorFacetsEnabled { get { return premiumMirrorFacetsEnabled; } }
        public bool PremiumInternalReflectionsEnabled { get { return premiumInternalReflectionsEnabled; } }
        public bool PremiumDispersionEnabled { get { return premiumDispersionEnabled; } }
        public bool PremiumRefractionDistortionEnabled { get { return premiumRefractionDistortionEnabled; } }
        public bool PremiumOpalIridescenceEnabled { get { return premiumOpalIridescenceEnabled; } }
        public bool PremiumFacetHighlightsEnabled { get { return premiumFacetHighlightsEnabled; } }
        public bool PremiumShapeMorphingEnabled { get { return premiumShapeMorphingEnabled; } }
        public bool PremiumDebugOpticalDiagnosticsEnabled { get { return premiumDebugOpticalDiagnosticsEnabled; } }
        public string RealMeshPlacementStatus
        {
            get
            {
                Vector2 offset = CrystalScreenCenterOffset;
                return "scale " + RealMeshScale.ToString("0.00")
                    + ", premium scale " + PremiumCrystalScalePercent.ToString("0") + "%"
                    + ", distance " + CrystalDistanceFromCamera.ToString("0.00")
                    + ", centered " + (RealMeshScreenCenter ? "true" : "false")
                    + ", offset " + offset.x.ToString("0.00") + "," + offset.y.ToString("0.00")
                    + ", camera orbit " + (CameraOrbitEnabled ? "enabled" : "disabled")
                    + ", crystal rotation " + (CrystalRotationEnabled ? "enabled" : "disabled")
                    + ", light rig " + (CrystalLightRigEnabled ? "enabled" : "disabled")
                    + ", light count " + RealMeshLightCount.ToString();
            }
        }
        public float RealMeshAlpha { get { return Mathf.Clamp01(realMeshAlpha); } }
        public float Transparency { get { return Mathf.Clamp01(transparency); } }
        public float RefractionStrength { get { return Mathf.Clamp(refractionStrength, 0f, 0.12f); } }
        public float FresnelPower { get { return Mathf.Clamp(fresnelPower, 0.5f, 8f); } }
        public float ReflectionStrength { get { return Mathf.Clamp01(reflectionStrength); } }
        public float InternalBrightness { get { return Mathf.Clamp(internalBrightness, 0f, 3f); } }
        public float MinimumTransmission { get { return Mathf.Clamp(minimumTransmission, 0f, 1f); } }
        public float SpecularStrength { get { return Mathf.Clamp01(specularStrength); } }
        public string SourceTextureStatus { get { return sourceTextureStatus; } }
        public string OpticalMaterialStatus
        {
            get
            {
                return "premium material " + PremiumMaterialName
                    + ", IOR " + RefractiveIndex.ToString("0.000")
                    + ", fresnel strength " + FresnelStrength.ToString("0.00")
                    + ", fresnel power " + FresnelPower.ToString("0.00")
                    + ", refraction strength " + RefractionStrength.ToString("0.000")
                    + ", facet refraction " + FacetRefraction.ToString("0.00")
                    + ", thickness refraction " + ThicknessRefraction.ToString("0.00")
                    + ", reflection " + ReflectionStrength.ToString("0.00")
                    + ", internal reflection strength " + InternalReflection.ToString("0.00")
                    + ", internal brightness " + InternalBrightness.ToString("0.00")
                    + ", dispersion " + SpectralDispersion.ToString("0.00")
                    + ", physical dispersion " + PhysicalDispersion.ToString("0.000")
                    + ", absorption strength " + AbsorptionStrength.ToString("0.00")
                    + ", background distortion " + BackgroundDistortionStrength.ToString("0.00")
                    + ", facet fire " + FacetFire.ToString("0.00")
                    + ", saturation boost " + SaturationBoost.ToString("0.00")
                    + ", contrast boost " + ContrastBoost.ToString("0.00")
                    + ", opal iridescence " + OpalIridescence.ToString("0.00")
                    + ", min transmission " + MinimumTransmission.ToString("0.00")
                    + ", hidden reflection " + FormatEnabled(PremiumHiddenReflectionBackgroundEnabled)
                    + ", mirror facets " + FormatEnabled(PremiumMirrorFacetsEnabled)
                    + ", internal reflections " + FormatEnabled(PremiumInternalReflectionsEnabled)
                    + ", dispersion toggle " + FormatEnabled(PremiumDispersionEnabled)
                    + ", refraction distortion " + FormatEnabled(PremiumRefractionDistortionEnabled)
                    + ", opal toggle " + FormatEnabled(PremiumOpalIridescenceEnabled)
                    + ", facet highlights " + FormatEnabled(PremiumFacetHighlightsEnabled)
                    + ", shape morphing " + FormatEnabled(PremiumShapeMorphingEnabled)
                    + ", optical diagnostics " + FormatEnabled(PremiumDebugOpticalDiagnosticsEnabled);
            }
        }

        public void SetRenderMode(CrystalRenderMode value)
        {
            renderMode = value == CrystalRenderMode.RealMesh3D
                ? CrystalRenderMode.RealMesh3D
                : CrystalRenderMode.Billboard2D;
        }

        public void ToggleRenderMode()
        {
            SetRenderMode(renderMode == CrystalRenderMode.RealMesh3D
                ? CrystalRenderMode.Billboard2D
                : CrystalRenderMode.RealMesh3D);
        }

        public void SyncFromDiamond(DiamondFocusSettings diamondSettings)
        {
            if (diamondSettings == null)
            {
                visible = false;
                sourceTextureStatus = "Missing diamond settings";
                return;
            }

            renderMode = diamondSettings.CrystalSimulationMode;
            visible = diamondSettings.Enabled;
            shape = FromDiamondShape(diamondSettings.Shape);
            shapeTransitionFromShape = FromDiamondShape(diamondSettings.ShapeTransitionFromShape);
            shapeTransitionToShape = FromDiamondShape(diamondSettings.ShapeTransitionToShape);
            shapeTransitionActive = diamondSettings.ShapeTransitionActive;
            shapeTransitionProgress = diamondSettings.ShapeTransitionActive
                ? diamondSettings.ShapeTransitionSmoothProgress
                : 1f;
            materialMode = FromDiamondMaterialMode(diamondSettings.MaterialMode);
            premiumMaterialMode = diamondSettings.MaterialMode;
            ApplyPremiumMaterialProfile(premiumMaterialMode);
            rotation = diamondSettings.RotationEuler;
            CrystalLightRigSettings lightRigSettings = diamondSettings.CrystalLightRigSettings;
            intensity = Mathf.Clamp(lightRigSettings.LightIntensity, diamondSettings.ActiveCrystalBrightnessMin, diamondSettings.ActiveCrystalBrightnessMax);
            realMeshLightCount = lightRigSettings.ActiveLightCountLimit;
            crystalLightRigEnabled = lightRigSettings.RigEnabled;
            premiumCrystalScalePercent = diamondSettings.PremiumCrystalScalePercent;
            premiumHiddenReflectionBackgroundEnabled = diamondSettings.PremiumHiddenReflectionBackgroundEnabled;
            premiumMirrorFacetsEnabled = diamondSettings.PremiumMirrorFacetsEnabled;
            premiumInternalReflectionsEnabled = diamondSettings.PremiumInternalReflectionsEnabled;
            premiumDispersionEnabled = diamondSettings.PremiumDispersionEnabled;
            premiumRefractionDistortionEnabled = diamondSettings.PremiumRefractionDistortionEnabled;
            premiumOpalIridescenceEnabled = diamondSettings.PremiumOpalIridescenceEnabled;
            premiumFacetHighlightsEnabled = diamondSettings.PremiumFacetHighlightsEnabled;
            premiumShapeMorphingEnabled = diamondSettings.PremiumShapeMorphingEnabled;
            premiumDebugOpticalDiagnosticsEnabled = diamondSettings.PremiumDebugOpticalDiagnosticsEnabled;
            transparency = diamondSettings.Transparency;
            refractionStrength = ResolveSafeRefractionStrength(diamondSettings);
            fresnelPower = diamondSettings.FresnelPower;
            reflectionStrength = diamondSettings.ReflectionStrength;
            internalBrightness = diamondSettings.InternalBrightness;
            minimumTransmission = Mathf.Clamp(0.08f + diamondSettings.DirectTransmission * 0.58f, 0.06f, 0.42f);
            specularStrength = Mathf.Clamp01(0.28f + diamondSettings.ReflectionStrength * 0.68f);
            sourceTextureStatus = diamondSettings.KaleidoscopeTexBindingStatus;
        }

        private void ApplyPremiumMaterialProfile(DiamondCrystalMaterialMode mode)
        {
            switch (mode)
            {
                case DiamondCrystalMaterialMode.Ruby:
                    SetPremiumMaterial("Ruby", new Color(1f, 0.045f, 0.12f, 1f), new Color(0.62f, 0f, 0.035f, 1f), new Color(1f, 0.5f, 0.22f, 1f), 0.68f, 1.28f, 1.14f, 1.2f, 1.42f, 1.08f, 1.12f, 0.68f, 0.78f, refractiveIndexValue: 1.77f, physicalDispersionValue: 0.018f, absorptionStrengthValue: 1.28f, fresnelStrengthValue: 1.18f, backgroundDistortionValue: 1.08f, saturationBoostValue: 1.36f, contrastBoostValue: 1.3f);
                    break;
                case DiamondCrystalMaterialMode.Emerald:
                    SetPremiumMaterial("Emerald", new Color(0.02f, 0.95f, 0.42f, 1f), new Color(0f, 0.46f, 0.22f, 1f), new Color(0.42f, 1f, 0.78f, 1f), 0.6f, 1.16f, 1.06f, 1.14f, 1.26f, 0.9f, 0.94f, 0.58f, 0.82f, refractiveIndexValue: 1.58f, physicalDispersionValue: 0.014f, absorptionStrengthValue: 1.02f, fresnelStrengthValue: 1.08f, backgroundDistortionValue: 1.08f, saturationBoostValue: 1.26f, contrastBoostValue: 1.18f);
                    break;
                case DiamondCrystalMaterialMode.Sapphire:
                    SetPremiumMaterial("Sapphire", new Color(0.035f, 0.2f, 1f, 1f), new Color(0.02f, 0.055f, 0.5f, 1f), new Color(0.42f, 0.72f, 1f, 1f), 0.66f, 1.24f, 1.1f, 1.18f, 1.36f, 1.02f, 1.08f, 0.64f, 0.8f, refractiveIndexValue: 1.76f, physicalDispersionValue: 0.018f, absorptionStrengthValue: 1.14f, fresnelStrengthValue: 1.14f, backgroundDistortionValue: 1.1f, saturationBoostValue: 1.34f, contrastBoostValue: 1.24f);
                    break;
                case DiamondCrystalMaterialMode.Amethyst:
                    SetPremiumMaterial("Amethyst", new Color(0.72f, 0.28f, 1f, 1f), new Color(0.32f, 0.06f, 0.58f, 1f), new Color(1f, 0.56f, 0.92f, 1f), 0.58f, 1.08f, 1.0f, 1.08f, 1.2f, 0.94f, 0.98f, 0.54f, 0.84f, refractiveIndexValue: 1.54f, physicalDispersionValue: 0.013f, absorptionStrengthValue: 0.92f, fresnelStrengthValue: 1.08f, backgroundDistortionValue: 1.02f, saturationBoostValue: 1.2f, contrastBoostValue: 1.16f);
                    break;
                case DiamondCrystalMaterialMode.Topaz:
                    SetPremiumMaterial("Topaz / Citrine", new Color(1f, 0.68f, 0.16f, 1f), new Color(0.82f, 0.36f, 0.02f, 1f), new Color(1f, 0.9f, 0.36f, 1f), 0.56f, 1.0f, 0.96f, 1.02f, 1.08f, 0.8f, 0.94f, 0.48f, 0.88f, refractiveIndexValue: 1.62f, physicalDispersionValue: 0.014f, absorptionStrengthValue: 0.78f, fresnelStrengthValue: 1.04f, backgroundDistortionValue: 1.04f, saturationBoostValue: 1.18f, contrastBoostValue: 1.12f);
                    break;
                case DiamondCrystalMaterialMode.Aquamarine:
                    SetPremiumMaterial("Aquamarine", new Color(0.14f, 0.95f, 1f, 1f), new Color(0.02f, 0.46f, 0.72f, 1f), new Color(0.68f, 1f, 0.96f, 1f), 0.42f, 0.88f, 0.94f, 0.98f, 1.02f, 0.78f, 0.84f, 0.36f, 0.95f, refractiveIndexValue: 1.57f, physicalDispersionValue: 0.014f, absorptionStrengthValue: 0.48f, fresnelStrengthValue: 1.0f, backgroundDistortionValue: 1.08f, saturationBoostValue: 1.1f, contrastBoostValue: 1.08f);
                    break;
                case DiamondCrystalMaterialMode.Garnet:
                    SetPremiumMaterial("Garnet", new Color(0.68f, 0.015f, 0.07f, 1f), new Color(0.3f, 0f, 0.025f, 1f), new Color(1f, 0.32f, 0.2f, 1f), 0.72f, 1.38f, 1.04f, 1.1f, 1.44f, 0.86f, 1.0f, 0.72f, 0.72f, refractiveIndexValue: 1.79f, physicalDispersionValue: 0.024f, absorptionStrengthValue: 1.42f, fresnelStrengthValue: 1.18f, backgroundDistortionValue: 1.02f, saturationBoostValue: 1.28f, contrastBoostValue: 1.34f);
                    break;
                case DiamondCrystalMaterialMode.AbsoluteMirror:
                    SetPremiumMaterial("Mirror Diamond", new Color(0.92f, 0.97f, 1f, 1f), new Color(0.72f, 0.86f, 1f, 1f), new Color(1f, 0.92f, 0.58f, 1f), 0.08f, 0.58f, 0.9f, 0.82f, 1.44f, 1.3f, 1.28f, 0.2f, 0.96f, refractiveIndexValue: 2.417f, physicalDispersionValue: 0.044f, absorptionStrengthValue: 0.18f, fresnelStrengthValue: 1.5f, backgroundDistortionValue: 1.16f, saturationBoostValue: 1.04f, contrastBoostValue: 1.22f);
                    break;
                case DiamondCrystalMaterialMode.FuturisticPlastic:
                    SetPremiumMaterial("Opal Glass", new Color(0.86f, 0.94f, 1f, 1f), new Color(0.96f, 0.82f, 1f, 1f), new Color(1f, 0.74f, 0.36f, 1f), 0.28f, 0.76f, 0.9f, 0.98f, 0.94f, 1.34f, 1.28f, 0.28f, 0.74f, refractiveIndexValue: 1.45f, physicalDispersionValue: 0.03f, absorptionStrengthValue: 0.3f, fresnelStrengthValue: 0.92f, backgroundDistortionValue: 1.16f, saturationBoostValue: 1.12f, contrastBoostValue: 0.94f, opalIridescenceValue: 0.86f);
                    break;
                case DiamondCrystalMaterialMode.Mercury:
                case DiamondCrystalMaterialMode.StainlessSteel:
                case DiamondCrystalMaterialMode.Chrome:
                case DiamondCrystalMaterialMode.CastIron:
                case DiamondCrystalMaterialMode.PolishedBrass:
                    SetPremiumMaterial(DiamondFocusSettings.GetMaterialModeLabel(mode), new Color(0.86f, 0.88f, 0.9f, 1f), new Color(0.46f, 0.48f, 0.5f, 1f), new Color(0.9f, 0.86f, 0.74f, 1f), 0.22f, 0.58f, 0.32f, 0.44f, 0.88f, 0.18f, 0.34f, 0.24f, 0.42f, refractiveIndexValue: 1.62f, physicalDispersionValue: 0.01f, absorptionStrengthValue: 0.34f, fresnelStrengthValue: 0.9f, backgroundDistortionValue: 0.72f, saturationBoostValue: 0.82f, contrastBoostValue: 1.06f);
                    break;
                default:
                    SetPremiumMaterial("Diamond", new Color(0.94f, 0.99f, 1f, 1f), new Color(0.7f, 0.9f, 1f, 1f), new Color(1f, 0.88f, 0.34f, 1f), 0.12f, 0.86f, 1.28f, 1.22f, 1.42f, 1.36f, 1.34f, 0.22f, 0.98f, refractiveIndexValue: 2.417f, physicalDispersionValue: 0.044f, absorptionStrengthValue: 0.22f, fresnelStrengthValue: 1.55f, backgroundDistortionValue: 1.26f, saturationBoostValue: 1.06f, contrastBoostValue: 1.26f);
                    break;
            }
        }

        private void SetPremiumMaterial(
            string name,
            Color baseColor,
            Color coreColor,
            Color fireColor,
            float tintStrength,
            float density,
            float facet,
            float thickness,
            float reflection,
            float dispersion,
            float fire,
            float depthAbsorptionValue,
            float clarity,
            float refractiveIndexValue = 1.76f,
            float physicalDispersionValue = 0.018f,
            float absorptionStrengthValue = 0.7f,
            float fresnelStrengthValue = 1f,
            float backgroundDistortionValue = 1f,
            float saturationBoostValue = 1f,
            float contrastBoostValue = 1f,
            float opalIridescenceValue = 0f)
        {
            premiumMaterialName = name;
            gemBaseColor = baseColor;
            gemCoreColor = coreColor;
            gemFireColor = fireColor;
            gemTintStrength = Mathf.Clamp01(tintStrength);
            opticalDensity = Mathf.Clamp(density, 0f, 2f);
            facetRefraction = Mathf.Clamp(facet, 0f, 2f);
            thicknessRefraction = Mathf.Clamp(thickness, 0f, 2f);
            internalReflection = Mathf.Clamp(reflection, 0f, 2f);
            spectralDispersion = Mathf.Clamp(dispersion, 0f, 2f);
            facetFire = Mathf.Clamp(fire, 0f, 2f);
            depthAbsorption = Mathf.Clamp01(depthAbsorptionValue);
            gemClarity = Mathf.Clamp01(clarity);
            refractiveIndex = Mathf.Clamp(refractiveIndexValue, 1f, 2.9f);
            physicalDispersion = Mathf.Clamp(physicalDispersionValue, 0f, 0.08f);
            absorptionStrength = Mathf.Clamp(absorptionStrengthValue, 0f, 2f);
            fresnelStrength = Mathf.Clamp(fresnelStrengthValue, 0f, 2f);
            backgroundDistortionStrength = Mathf.Clamp(backgroundDistortionValue, 0f, 2f);
            saturationBoost = Mathf.Clamp(saturationBoostValue, 0f, 2f);
            contrastBoost = Mathf.Clamp(contrastBoostValue, 0f, 2f);
            opalIridescence = Mathf.Clamp01(opalIridescenceValue);
        }

        public bool EnsureRealMeshProductionDefaults(out string migrationMessage)
        {
            migrationMessage = string.Empty;
            float previousScale = realMeshScale;
            bool migrated = ApplyRealMeshProductionDefaults(out previousScale);

            if (realMeshScaleMigrationPending)
            {
                previousScale = realMeshScaleMigrationSource;
                migrated = true;
                realMeshScaleMigrationPending = false;
            }

            CreateRealMeshMigrationMessage(migrated, previousScale, out migrationMessage);
            return migrated;
        }

        public void OnBeforeSerialize()
        {
            float previousScale;
            ApplyRealMeshProductionDefaults(out previousScale);
        }

        public void OnAfterDeserialize()
        {
            float previousScale;
            bool migrated = ApplyRealMeshProductionDefaults(out previousScale);
            if (migrated)
            {
                realMeshScaleMigrationPending = true;
                realMeshScaleMigrationSource = previousScale;
            }
        }

        private bool ApplyRealMeshProductionDefaults(out float previousScale)
        {
            previousScale = realMeshScale;
            bool migrated = false;
            if (float.IsNaN(realMeshScale) || float.IsInfinity(realMeshScale) || realMeshScale < RealMeshScaleMinimum)
            {
                realMeshScale = RealMeshScaleDefault;
                migrated = true;
            }
            else
            {
                realMeshScale = Mathf.Clamp(realMeshScale, RealMeshScaleMinimum, RealMeshScaleMaximum);
            }

            if (float.IsNaN(crystalDistanceFromCamera) || float.IsInfinity(crystalDistanceFromCamera) || crystalDistanceFromCamera <= 0f)
            {
                crystalDistanceFromCamera = CrystalDistanceDefault;
            }

            if (realMeshScreenCenter)
            {
                crystalScreenCenterOffset = Vector2.zero;
            }

            premiumCrystalScalePercent = Mathf.Clamp(
                premiumCrystalScalePercent,
                DiamondFocusSettings.PremiumCrystalScalePercentMin,
                DiamondFocusSettings.PremiumCrystalScalePercentMax);

            return migrated;
        }

        private static void CreateRealMeshMigrationMessage(bool migrated, float previousScale, out string migrationMessage)
        {
            migrationMessage = migrated
                ? "Migrated RealMesh3D realMeshScale from "
                    + previousScale.ToString("0.00")
                    + " to production default "
                    + RealMeshScaleDefault.ToString("0.00")
                    + "."
                : string.Empty;
        }

        public void SetSourceTextureStatus(bool bound)
        {
            sourceTextureStatus = bound ? "Bound" : "Missing / fallback";
        }

        public static string GetRenderModeLabel(CrystalRenderMode value)
        {
            return value == CrystalRenderMode.RealMesh3D ? "3D Premium" : "2D Performance";
        }

        private static CrystalShape FromDiamondShape(DiamondFocusShape value)
        {
            switch (value)
            {
                case DiamondFocusShape.FacetedCube:
                    return CrystalShape.FacetedCube;
                case DiamondFocusShape.DiscoBall:
                    return CrystalShape.DiscoBall;
                case DiamondFocusShape.TetrahedralCrystal:
                    return CrystalShape.TetrahedralCrystal;
                case DiamondFocusShape.RhombicCrystal:
                    return CrystalShape.RhombicCrystal;
                case DiamondFocusShape.OvalRingGem:
                    return CrystalShape.OvalRingGem;
                default:
                    return CrystalShape.ClassicDiamond;
            }
        }

        private static CrystalMaterialMode FromDiamondMaterialMode(DiamondCrystalMaterialMode value)
        {
            switch (value)
            {
                case DiamondCrystalMaterialMode.AbsoluteMirror:
                    return CrystalMaterialMode.AbsoluteMirror;
                case DiamondCrystalMaterialMode.FuturisticPlastic:
                    return CrystalMaterialMode.FuturisticPlastic;
                case DiamondCrystalMaterialMode.Mercury:
                case DiamondCrystalMaterialMode.StainlessSteel:
                case DiamondCrystalMaterialMode.Chrome:
                case DiamondCrystalMaterialMode.CastIron:
                case DiamondCrystalMaterialMode.PolishedBrass:
                    return CrystalMaterialMode.Metal;
                case DiamondCrystalMaterialMode.Emerald:
                case DiamondCrystalMaterialMode.Topaz:
                case DiamondCrystalMaterialMode.Ruby:
                case DiamondCrystalMaterialMode.Sapphire:
                case DiamondCrystalMaterialMode.Amethyst:
                case DiamondCrystalMaterialMode.Aquamarine:
                case DiamondCrystalMaterialMode.Garnet:
                    return CrystalMaterialMode.Gemstone;
                default:
                    return CrystalMaterialMode.Diamond;
            }
        }

        private static float ResolveSafeRefractionStrength(DiamondFocusSettings diamondSettings)
        {
            float coefficient01 = Mathf.InverseLerp(0f, 10f, diamondSettings.RefractionCoefficient);
            float mapped = diamondSettings.RefractionStrength * Mathf.Lerp(0.35f, 1f, coefficient01);
            return Mathf.Clamp(mapped, 0f, 0.085f);
        }

        private static string FormatEnabled(bool value)
        {
            return value ? "on" : "off";
        }
    }
}
