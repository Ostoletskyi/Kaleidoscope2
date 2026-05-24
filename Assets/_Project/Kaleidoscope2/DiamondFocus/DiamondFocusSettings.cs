using System;
using UnityEngine;

namespace Kaleidoscope2.Core
{
    public enum DiamondFocusShape
    {
        ClassicDiamond = 0,
        FacetedCube = 1,
        DiscoBall = 2,
        TetrahedralCrystal = 3,
        RhombicCrystal = 4,
        OvalRingGem = 5,
        RadialShardCrystal = 6,
        MandalaCrystal = 7,
        StarDiamond = 8,
        PolygonCrystal = 9
    }

    public enum DiamondCrystalMaterialMode
    {
        AbsoluteMirror = 0,
        Diamond = 1,
        LegacyGlow = 2,
        LegacyOpticalPhysics = 3,
        LegacyGeneratedMaterial = 4,
        Emerald = 5,
        Topaz = 6,
        Ruby = 7,
        Sapphire = 8,
        Amethyst = 9,
        Aquamarine = 10,
        Garnet = 11,
        FuturisticPlastic = 12,
        Mercury = 13,
        StainlessSteel = 14,
        Chrome = 15,
        CastIron = 16,
        PolishedBrass = 17
    }

    public enum DiamondCrystalDebugMode
    {
        FinalCrystalComposite = 0,
        RawKaleidoscopeTex = 1,
        RefractionOnly = 2,
        ReflectionOnly = 3,
        DispersionOnly = 4,
        SurfaceNormalOnly = 5,
        CrystalOff = 6,
        ArtifactStressTest = 7
    }

    public enum DiamondGeneratedMaterialKind
    {
        Wood = 0,
        Metal = 1,
        Plastic = 2,
        Stone = 3
    }

    public enum PremiumCrystalEffectToggle
    {
        HiddenReflectionBackground = 0,
        MirrorFacets = 1,
        InternalReflections = 2,
        Dispersion = 3,
        RefractionDistortion = 4,
        OpalIridescence = 5,
        FacetHighlights = 6,
        ShapeMorphing = 7,
        DebugOpticalDiagnostics = 8
    }

    public enum PremiumCrystalOpticsParameter
    {
        Brightness = 0,
        Contrast = 1,
        BloomGlow = 2,
        FacetHighlights = 3,
        RefractionStrength = 4,
        ReflectionStrength = 5,
        InternalReflections = 6,
        BackgroundDistortion = 7,
        DirectTransparency = 8,
        PrismDispersion = 9,
        ChromaticAberration = 10,
        RainbowEdge = 11,
        SpectralSplit = 12,
        CrystalDepth = 13,
        Caustics = 14
    }

    public enum PremiumCrystalFactoryPreset
    {
        DiamondPalace = 0,
        BlueIce = 1,
        GoldenPrism = 2,
        RubyNight = 3,
        EmeraldDepth = 4,
        OpalDream = 5,
        CosmicGlass = 6,
        DarkLuxury = 7,
        AbsoluteMirror = 8
    }

    [Serializable]
    public sealed class DiamondFocusSettings
    {
        public const int ShapeCount = 10;
        public const int GeneratedMaterialKindCount = 4;
        public const float RefractionIndexMin = 0f;
        public const float RefractionIndexMax = 10f;
        public const float DirectedLightIntensityMin = -10f;
        public const float DirectedLightIntensityMax = 10f;
        public const float PremiumCrystalScalePercentMin = 20f;
        public const float PremiumCrystalScalePercentMax = 300f;
        public const float PremiumCrystalScalePercentDefault = 100f;
        public const float PremiumCrystalWheelScaleStepPercentMin = 1f;
        public const float PremiumCrystalWheelScaleStepPercentMax = 50f;
        public const float PremiumCrystalWheelScaleStepPercentDefault = 10f;
        public const float PremiumOpticsBrightnessDefault = 1f;
        public const float PremiumOpticsContrastDefault = 1.1f;
        public const float PremiumOpticsBloomGlowDefault = 0.8f;
        public const float PremiumOpticsFacetHighlightsDefault = 1.2f;
        public const float PremiumOpticsRefractionStrengthDefault = 1f;
        public const float PremiumOpticsReflectionStrengthDefault = 1.1f;
        public const float PremiumOpticsInternalReflectionsDefault = 1.25f;
        public const float PremiumOpticsBackgroundDistortionDefault = 0.9f;
        public const float PremiumOpticsDirectTransparencyDefault = 0.08f;
        public const float PremiumOpticsPrismDispersionDefault = 1f;
        public const float PremiumOpticsChromaticAberrationDefault = 0.45f;
        public const float PremiumOpticsRainbowEdgeDefault = 0.85f;
        public const float PremiumOpticsSpectralSplitDefault = 0.65f;
        public const float PremiumOpticsCrystalDepthDefault = 1f;
        public const float OldCrystalBrightnessMin = CrystalLightRigSettings.LightIntensityMin;
        public const float OldCrystalBrightnessMax = CrystalLightRigSettings.LightIntensityMax;
        public const float PremiumCrystalBrightnessMin = OldCrystalBrightnessMin * 0.6f;
        public const float PremiumCrystalBrightnessMax = OldCrystalBrightnessMax * 0.6f;

        private static readonly DiamondCrystalMaterialMode[] MaterialModeSequence =
        {
            DiamondCrystalMaterialMode.AbsoluteMirror,
            DiamondCrystalMaterialMode.Diamond,
            DiamondCrystalMaterialMode.Emerald,
            DiamondCrystalMaterialMode.Topaz,
            DiamondCrystalMaterialMode.Ruby,
            DiamondCrystalMaterialMode.Sapphire,
            DiamondCrystalMaterialMode.Amethyst,
            DiamondCrystalMaterialMode.Aquamarine,
            DiamondCrystalMaterialMode.Garnet,
            DiamondCrystalMaterialMode.FuturisticPlastic,
            DiamondCrystalMaterialMode.Mercury,
            DiamondCrystalMaterialMode.StainlessSteel,
            DiamondCrystalMaterialMode.Chrome,
            DiamondCrystalMaterialMode.CastIron,
            DiamondCrystalMaterialMode.PolishedBrass
        };

        [SerializeField] private bool enabled;
        [SerializeField] private DiamondFocusShape shape = DiamondFocusShape.ClassicDiamond;
        [SerializeField] private DiamondFocusShape shapeTransitionFromShape = DiamondFocusShape.ClassicDiamond;
        [SerializeField] private DiamondFocusShape shapeTransitionToShape = DiamondFocusShape.ClassicDiamond;
        [SerializeField] private bool shapeTransitionActive;
        [SerializeField] private float shapeTransitionElapsed;
        [SerializeField, Range(0.1f, 5f)] private float shapeTransitionDuration = 2f;
        [SerializeField] private DiamondCrystalMaterialMode materialMode = DiamondCrystalMaterialMode.Diamond;
        [SerializeField] private DiamondGeneratedMaterialKind generatedMaterialKind = DiamondGeneratedMaterialKind.Wood;
        [SerializeField] private Color generatedMaterialColor = new Color(0.85f, 0.96f, 1f, 1f);
        [SerializeField, InspectorName("RandomSeed")] private int generatedMaterialSeed = 1;
        [SerializeField] private Vector2 targetRotationDirection = new Vector2(0.35f, 0.75f);
        [SerializeField] private Vector2 currentRotationDirection = new Vector2(0.35f, 0.75f);
        [SerializeField] private Vector3 rotationEuler = Vector3.zero;
        [SerializeField] private Vector3 rotationVelocity = new Vector3(10f, 17f, 4f);
        [SerializeField] private float currentRotationSpeed = 20f;
        [SerializeField] private float targetRotationSpeed = 20f;

        [Header("Rotation")]
        [SerializeField] private float minRotationSpeed = 0f;
        [SerializeField] private float defaultRotationSpeed = 20f;
        [SerializeField] private float maxRotationSpeed = 240f;
        [SerializeField] private float speedAcceleration = 90f;
        [SerializeField] private float directionAcceleration = 3.5f;

        [Header("Optics")]
        [SerializeField, Range(0f, 1f)] private float transparency;
        [SerializeField, Range(0f, 0.12f)] private float refractionStrength = 0.074f;
        [SerializeField, Range(0f, 2f)] private float dispersionStrength = 1f;
        [SerializeField, Range(0f, 1f)] private float reflectionStrength = 0.72f;
        [SerializeField, Range(0.5f, 8f)] private float fresnelPower = 3.2f;
        [SerializeField, Range(0f, 3f)] private float internalBrightness = 1f;
        [SerializeField, Range(0f, 1f)] private float noiseDistortionStrength = 0.08f;
        [SerializeField, Range(0f, 2f)] private float edgeHighlight = 1.35f;
        [SerializeField, Range(0f, 0.04f)] private float chromaticAberrationBase = 0.009f;
        [SerializeField, Range(0f, 0.04f)] private float chromaticAberrationExtra = 0.016f;
        [SerializeField, Range(0f, 2f)] private float facetContrast = 1.45f;
        [SerializeField, Range(0f, 1.5f)] private float internalGlow = 0.62f;
        [SerializeField, Range(0f, 3f)] private float bloomBoostBase = 0.44f;
        [SerializeField, Range(0f, 3f)] private float bloomBoostExtra = 1.25f;
        [SerializeField, Range(0.1f, 1f)] private float screenScale = 0.38f;

        [Header("Cinematic Crystal Optics")]
        [SerializeField, Range(0f, 2f)] private float diamondLikeRefraction = 1.8f;
        [SerializeField, Range(0f, 3f)] private float spectralDispersion = 2.25f;
        [SerializeField, Range(0f, 3f)] private float highEnergyCaustics = 1.85f;
        [SerializeField, Range(0f, 3f)] private float multiBounceInternalReflections = 1.75f;
        [SerializeField, Range(0f, 2f)] private float cinematicCrystalOptics = 1.45f;
        [SerializeField, Range(0f, 2f)] private float physicallyBasedRefraction = 1.55f;
        [SerializeField, Range(0f, 3f)] private float deepVolumetricLightScattering = 1.35f;
        [SerializeField, Range(0f, 1f)] private float crystalSolidity = 1f;
        [SerializeField, Range(0f, 3f)] private float blueWhitePlasmaEnergy = 2.05f;
        [SerializeField, Range(0f, 1f)] private float directTransmission = 0.06f;
        [SerializeField, Range(0f, 3f)] private float totalInternalReturn = 1.65f;
        [SerializeField, Range(0f, 3f)] private float spectralFireIntensity = 1.85f;
        [SerializeField, Range(0f, 2f)] private float facetDepthContrast = 1.22f;

        [Header("Optical Material Mode")]
        [SerializeField, InspectorName("RefractionCoefficient"), Range(0f, 10f)] private float opticalIOR = 2.42f;
        [SerializeField, InspectorName("DirectedLightIntensity"), Range(-10f, 10f)] private float directedLightIntensity;
        [SerializeField, Range(0f, 2f)] private float opticalCaustics = 0.8f;
        [SerializeField, Range(0f, 2f)] private float opticalDispersion = 1.45f;
        [SerializeField, Range(0f, 2f)] private float totalInternalReflection = 1.45f;

        [Header("Crystal Light Rig")]
        [SerializeField] private CrystalLightRigSettings crystalLightRigSettings = new CrystalLightRigSettings();

        [Header("Crystal Presentation")]
        [SerializeField, InspectorName("Crystal Simulation")] private CrystalRenderMode crystalSimulationMode = CrystalRenderMode.Billboard2D;
        [SerializeField, Range(PremiumCrystalScalePercentMin, PremiumCrystalScalePercentMax)] private float premiumCrystalScalePercent = PremiumCrystalScalePercentDefault;
        [SerializeField] private bool premiumCrystalWheelScaleEnabled = true;
        [SerializeField, Range(PremiumCrystalWheelScaleStepPercentMin, PremiumCrystalWheelScaleStepPercentMax)] private float premiumCrystalWheelScaleStepPercent = PremiumCrystalWheelScaleStepPercentDefault;

        [Header("Premium3D Menu Optics")]
        [SerializeField, Range(0.10f, 3f)] private float premiumOpticsBrightness = PremiumOpticsBrightnessDefault;
        [SerializeField, Range(0.20f, 3f)] private float premiumOpticsContrast = PremiumOpticsContrastDefault;
        [SerializeField, Range(0f, 5f)] private float premiumOpticsBloomGlow = PremiumOpticsBloomGlowDefault;
        [SerializeField, Range(0f, 6f)] private float premiumOpticsFacetHighlights = PremiumOpticsFacetHighlightsDefault;
        [SerializeField, Range(0f, 5f)] private float premiumOpticsRefractionStrength = PremiumOpticsRefractionStrengthDefault;
        [SerializeField, Range(0f, 5f)] private float premiumOpticsReflectionStrength = PremiumOpticsReflectionStrengthDefault;
        [SerializeField, Range(0f, 6f)] private float premiumOpticsInternalReflections = PremiumOpticsInternalReflectionsDefault;
        [SerializeField, Range(0f, 5f)] private float premiumOpticsBackgroundDistortion = PremiumOpticsBackgroundDistortionDefault;
        [SerializeField, Range(0f, 1f)] private float premiumOpticsDirectTransparency = PremiumOpticsDirectTransparencyDefault;
        [SerializeField, Range(0f, 5f)] private float premiumOpticsPrismDispersion = PremiumOpticsPrismDispersionDefault;
        [SerializeField, Range(0f, 3f)] private float premiumOpticsChromaticAberration = PremiumOpticsChromaticAberrationDefault;
        [SerializeField, Range(0f, 5f)] private float premiumOpticsRainbowEdge = PremiumOpticsRainbowEdgeDefault;
        [SerializeField, Range(0f, 4f)] private float premiumOpticsSpectralSplit = PremiumOpticsSpectralSplitDefault;
        [SerializeField, Range(0.20f, 4f)] private float premiumOpticsCrystalDepth = PremiumOpticsCrystalDepthDefault;
        [SerializeField, Range(0f, 1f)] private float premiumOpticsCaustics;

        [Header("Premium3D Effect Toggles")]
        [SerializeField] private bool premiumHiddenReflectionBackgroundEnabled = true;
        [SerializeField] private bool premiumMirrorFacetsEnabled = true;
        [SerializeField] private bool premiumInternalReflectionsEnabled = true;
        [SerializeField] private bool premiumDispersionEnabled = true;
        [SerializeField] private bool premiumRefractionDistortionEnabled = true;
        [SerializeField] private bool premiumOpalIridescenceEnabled = true;
        [SerializeField] private bool premiumFacetHighlightsEnabled = true;
        [SerializeField] private bool premiumShapeMorphingEnabled = true;
        [SerializeField] private bool premiumDebugOpticalDiagnosticsEnabled;

        [Header("Debug")]
        [SerializeField, InspectorName("DebugView")] private DiamondCrystalDebugMode debugMode = DiamondCrystalDebugMode.FinalCrystalComposite;

        [Header("Runtime Safety")]
        [SerializeField] private bool enableRandomVariants = true;
        [SerializeField] private bool preserveClassicMode = true;
        [SerializeField, InspectorName("CurrentShapeName")] private string currentShapeName = "Classic Diamond";
        [SerializeField, InspectorName("CurrentModeName")] private string currentModeName = "High-Purity Diamond";
        [SerializeField, InspectorName("KaleidoscopeTex Binding Status")] private string kaleidoscopeTexBindingStatus = "Unbound";

        public bool Enabled { get { return enabled; } }
        public DiamondFocusShape Shape { get { return shape; } }
        public int ShapeIndex { get { return Mathf.Clamp((int)shape, 0, ShapeCount - 1); } }
        public DiamondFocusShape ShapeTransitionFromShape { get { return shapeTransitionFromShape; } }
        public DiamondFocusShape ShapeTransitionToShape { get { return shapeTransitionToShape; } }
        public bool ShapeTransitionActive { get { return shapeTransitionActive; } }
        public float ShapeTransitionDuration { get { return Mathf.Max(0.1f, shapeTransitionDuration); } }
        public static int MaterialModeCount { get { return MaterialModeSequence.Length; } }
        public float ShapeTransitionProgress
        {
            get
            {
                return shapeTransitionActive
                    ? Mathf.Clamp01(shapeTransitionElapsed / ShapeTransitionDuration)
                    : 1f;
            }
        }
        public float ShapeTransitionSmoothProgress
        {
            get
            {
                float value = ShapeTransitionProgress;
                return value * value * value * (value * (value * 6f - 15f) + 10f);
            }
        }
        public DiamondCrystalMaterialMode MaterialMode { get { return materialMode; } }
        public int MaterialModeIndex { get { return GetMaterialModeIndex(materialMode); } }
        public DiamondGeneratedMaterialKind GeneratedMaterialKind { get { return generatedMaterialKind; } }
        public int GeneratedMaterialKindIndex { get { return Mathf.Clamp((int)generatedMaterialKind, 0, GeneratedMaterialKindCount - 1); } }
        public Color GeneratedMaterialColor { get { return generatedMaterialColor; } }
        public int GeneratedMaterialSeed { get { return RandomSeed; } }
        public int RandomSeed { get { return Mathf.Max(1, generatedMaterialSeed); } }
        public Vector2 TargetRotationDirection { get { return targetRotationDirection; } }
        public Vector2 ResolvedTargetRotationDirection { get { return NormalizeOrDefault(targetRotationDirection); } }
        public Vector2 CurrentRotationDirection { get { return NormalizeOrDefault(currentRotationDirection); } }
        public Vector3 RotationEuler { get { return rotationEuler; } }
        public Vector3 RotationVelocity { get { return rotationVelocity; } }
        public float CurrentRotationSpeed { get { return Mathf.Clamp(currentRotationSpeed, MinRotationSpeed, MaxRotationSpeed); } }
        public float TargetRotationSpeed { get { return Mathf.Clamp(targetRotationSpeed, MinRotationSpeed, MaxRotationSpeed); } }
        public float MinRotationSpeed { get { return Mathf.Max(0f, minRotationSpeed); } }
        public float DefaultRotationSpeed { get { return Mathf.Clamp(defaultRotationSpeed, MinRotationSpeed, MaxRotationSpeed); } }
        public float MaxRotationSpeed { get { return Mathf.Max(1f, maxRotationSpeed); } }
        public float SpeedAcceleration { get { return Mathf.Max(1f, speedAcceleration); } }
        public float DirectionAcceleration { get { return Mathf.Max(0.01f, directionAcceleration); } }
        public float Transparency { get { return Mathf.Clamp01(transparency); } }
        public float RefractionStrength { get { return Mathf.Clamp(refractionStrength, 0f, 0.12f); } }
        public float DispersionStrength { get { return Mathf.Max(0f, dispersionStrength); } }
        public float ReflectionStrength { get { return Mathf.Clamp01(reflectionStrength); } }
        public float FresnelPower { get { return Mathf.Clamp(fresnelPower, 0.5f, 8f); } }
        public float InternalBrightness { get { return Mathf.Max(0f, internalBrightness); } }
        public float NoiseDistortionStrength { get { return Mathf.Clamp01(noiseDistortionStrength); } }
        public float EdgeHighlight { get { return Mathf.Max(0f, edgeHighlight); } }
        public float ChromaticAberrationAmount { get { return chromaticAberrationBase + NormalizedRotationSpeed * chromaticAberrationExtra; } }
        public float FacetContrast { get { return Mathf.Max(0f, facetContrast); } }
        public float InternalGlow { get { return Mathf.Max(0f, internalGlow); } }
        public float BloomBoost { get { return Mathf.Max(0f, bloomBoostBase + NormalizedRotationSpeed * bloomBoostExtra); } }
        public float ScreenScale { get { return Mathf.Clamp(screenScale, 0.1f, 1f); } }
        public float DiamondLikeRefraction { get { return Mathf.Max(0f, diamondLikeRefraction); } }
        public float SpectralDispersion { get { return Mathf.Max(0f, spectralDispersion); } }
        public float HighEnergyCaustics { get { return Mathf.Max(0f, highEnergyCaustics); } }
        public float MultiBounceInternalReflections { get { return Mathf.Max(0f, multiBounceInternalReflections); } }
        public float CinematicCrystalOptics { get { return Mathf.Max(0f, cinematicCrystalOptics); } }
        public float PhysicallyBasedRefraction { get { return Mathf.Max(0f, physicallyBasedRefraction); } }
        public float DeepVolumetricLightScattering { get { return Mathf.Max(0f, deepVolumetricLightScattering); } }
        public float CrystalSolidity { get { return Mathf.Clamp01(crystalSolidity); } }
        public float BlueWhitePlasmaEnergy { get { return Mathf.Max(0f, blueWhitePlasmaEnergy); } }
        public float DirectTransmission { get { return Mathf.Clamp01(directTransmission); } }
        public float TotalInternalReturn { get { return Mathf.Max(0f, totalInternalReturn); } }
        public float SpectralFireIntensity { get { return Mathf.Max(0f, spectralFireIntensity); } }
        public float FacetDepthContrast { get { return Mathf.Max(0f, facetDepthContrast); } }
        public float OpticalIOR { get { return RefractionCoefficient; } }
        public float RefractionIndex { get { return RefractionCoefficient; } }
        public float RefractionCoefficient { get { return Mathf.Clamp(opticalIOR, RefractionIndexMin, RefractionIndexMax); } }
        public float DirectedLightIntensity { get { return Mathf.Clamp(directedLightIntensity, DirectedLightIntensityMin, DirectedLightIntensityMax); } }
        public CrystalLightRigSettings CrystalLightRigSettings
        {
            get
            {
                if (crystalLightRigSettings == null)
                {
                    crystalLightRigSettings = new CrystalLightRigSettings();
                }

                return crystalLightRigSettings;
            }
        }
        public CrystalRenderMode CrystalSimulationMode { get { return crystalSimulationMode; } }
        public string CrystalSimulationModeLabel { get { return CrystalSharedSettings.GetRenderModeLabel(crystalSimulationMode); } }
        public float PremiumCrystalScalePercent { get { return Mathf.Clamp(premiumCrystalScalePercent, PremiumCrystalScalePercentMin, PremiumCrystalScalePercentMax); } }
        public float PremiumCrystalScaleMultiplier { get { return PremiumCrystalScalePercent / 100f; } }
        public bool PremiumCrystalWheelScaleEnabled { get { return premiumCrystalWheelScaleEnabled; } }
        public float PremiumCrystalWheelScaleStepPercent { get { return Mathf.Clamp(premiumCrystalWheelScaleStepPercent, PremiumCrystalWheelScaleStepPercentMin, PremiumCrystalWheelScaleStepPercentMax); } }
        public float PremiumOpticsBrightness { get { return Mathf.Clamp(premiumOpticsBrightness, 0.10f, 3f); } }
        public float PremiumOpticsContrast { get { return Mathf.Clamp(premiumOpticsContrast, 0.20f, 3f); } }
        public float PremiumOpticsBloomGlow { get { return Mathf.Clamp(premiumOpticsBloomGlow, 0f, 5f); } }
        public float PremiumOpticsFacetHighlights { get { return Mathf.Clamp(premiumOpticsFacetHighlights, 0f, 6f); } }
        public float PremiumOpticsRefractionStrength { get { return Mathf.Clamp(premiumOpticsRefractionStrength, 0f, 5f); } }
        public float PremiumOpticsReflectionStrength { get { return Mathf.Clamp(premiumOpticsReflectionStrength, 0f, 5f); } }
        public float PremiumOpticsInternalReflections { get { return Mathf.Clamp(premiumOpticsInternalReflections, 0f, 6f); } }
        public float PremiumOpticsBackgroundDistortion { get { return Mathf.Clamp(premiumOpticsBackgroundDistortion, 0f, 5f); } }
        public float PremiumOpticsDirectTransparency { get { return Mathf.Clamp01(premiumOpticsDirectTransparency); } }
        public float PremiumOpticsPrismDispersion { get { return Mathf.Clamp(premiumOpticsPrismDispersion, 0f, 5f); } }
        public float PremiumOpticsChromaticAberration { get { return Mathf.Clamp(premiumOpticsChromaticAberration, 0f, 3f); } }
        public float PremiumOpticsRainbowEdge { get { return Mathf.Clamp(premiumOpticsRainbowEdge, 0f, 5f); } }
        public float PremiumOpticsSpectralSplit { get { return Mathf.Clamp(premiumOpticsSpectralSplit, 0f, 4f); } }
        public float PremiumOpticsCrystalDepth { get { return Mathf.Clamp(premiumOpticsCrystalDepth, 0.20f, 4f); } }
        public float PremiumOpticsCaustics { get { return Mathf.Clamp01(premiumOpticsCaustics); } }
        public float ActiveCrystalBrightnessMin { get { return IsPremiumCrystalSimulation ? PremiumCrystalBrightnessMin : OldCrystalBrightnessMin; } }
        public float ActiveCrystalBrightnessMax { get { return IsPremiumCrystalSimulation ? PremiumCrystalBrightnessMax : OldCrystalBrightnessMax; } }
        public bool IsPremiumCrystalSimulation { get { return crystalSimulationMode == CrystalRenderMode.RealMesh3D; } }
        public string PremiumCrystalControlStatus
        {
            get
            {
                return "crystal visible " + (enabled && IsPremiumCrystalSimulation ? "true" : "false")
                    + ", active shape " + ShapeLabel
                    + ", active material " + MaterialModeLabel
                    + ", shape transition " + (ShapeTransitionActive ? "active " + ShapeTransitionSmoothProgress.ToString("0.00") : "inactive")
                    + ", current crystal scale percent " + PremiumCrystalScalePercent.ToString("0")
                    + ", wheel scale " + (PremiumCrystalWheelScaleEnabled ? "enabled" : "disabled")
                    + ", wheel step " + PremiumCrystalWheelScaleStepPercent.ToString("0") + "%"
                    + ", direct transparency " + PremiumOpticsDirectTransparency.ToString("0.00")
                    + ", refraction " + PremiumOpticsRefractionStrength.ToString("0.00")
                    + ", reflection " + PremiumOpticsReflectionStrength.ToString("0.00")
                    + ", dispersion " + PremiumOpticsPrismDispersion.ToString("0.00")
                    + ", internal reflections " + PremiumOpticsInternalReflections.ToString("0.00")
                    + ", depth " + PremiumOpticsCrystalDepth.ToString("0.00")
                    + ", old brightness min/max " + OldCrystalBrightnessMin.ToString("0.00") + "/" + OldCrystalBrightnessMax.ToString("0.00")
                    + ", new brightness min/max " + PremiumCrystalBrightnessMin.ToString("0.00") + "/" + PremiumCrystalBrightnessMax.ToString("0.00");
            }
        }
        public bool PremiumHiddenReflectionBackgroundEnabled { get { return premiumHiddenReflectionBackgroundEnabled; } }
        public bool PremiumMirrorFacetsEnabled { get { return premiumMirrorFacetsEnabled; } }
        public bool PremiumInternalReflectionsEnabled { get { return premiumInternalReflectionsEnabled; } }
        public bool PremiumDispersionEnabled { get { return premiumDispersionEnabled; } }
        public bool PremiumRefractionDistortionEnabled { get { return premiumRefractionDistortionEnabled; } }
        public bool PremiumOpalIridescenceEnabled { get { return premiumOpalIridescenceEnabled; } }
        public bool PremiumFacetHighlightsEnabled { get { return premiumFacetHighlightsEnabled; } }
        public bool PremiumShapeMorphingEnabled { get { return premiumShapeMorphingEnabled; } }
        public bool PremiumDebugOpticalDiagnosticsEnabled { get { return premiumDebugOpticalDiagnosticsEnabled; } }
        public string PremiumCrystalEffectStatus
        {
            get
            {
                return "hidden reflection " + FormatEnabled(premiumHiddenReflectionBackgroundEnabled)
                    + ", mirror facets " + FormatEnabled(premiumMirrorFacetsEnabled)
                    + ", internal reflections " + FormatEnabled(premiumInternalReflectionsEnabled)
                    + ", dispersion " + FormatEnabled(premiumDispersionEnabled)
                    + ", refraction distortion " + FormatEnabled(premiumRefractionDistortionEnabled)
                    + ", opal iridescence " + FormatEnabled(premiumOpalIridescenceEnabled)
                    + ", facet highlights " + FormatEnabled(premiumFacetHighlightsEnabled)
                    + ", shape morphing " + FormatEnabled(premiumShapeMorphingEnabled)
                    + ", optical diagnostics " + FormatEnabled(premiumDebugOpticalDiagnosticsEnabled);
            }
        }
        public float OpticalCaustics { get { return Mathf.Max(0f, opticalCaustics); } }
        public float OpticalDispersion { get { return Mathf.Max(0f, opticalDispersion); } }
        public float TotalInternalReflection { get { return Mathf.Max(0f, totalInternalReflection); } }
        public float NormalizedRotationSpeed { get { return MaxRotationSpeed > 0.0001f ? Mathf.Clamp01(CurrentRotationSpeed / MaxRotationSpeed) : 0f; } }
        public string ShapeLabel { get { return GetShapeLabel(shape); } }
        public string MaterialModeLabel { get { return GetMaterialModeLabel(materialMode); } }
        public string GeneratedMaterialLabel { get { return GetGeneratedMaterialKindLabel(generatedMaterialKind); } }
        public DiamondCrystalDebugMode DebugMode { get { return debugMode; } }
        public DiamondCrystalDebugMode DebugView { get { return debugMode; } }
        public bool EnableRandomVariants { get { return enableRandomVariants; } }
        public bool PreserveClassicMode { get { return preserveClassicMode; } }
        public string CurrentShapeName { get { return currentShapeName; } }
        public string CurrentModeName { get { return currentModeName; } }
        public string KaleidoscopeTexBindingStatus { get { return kaleidoscopeTexBindingStatus; } }

        public void SetEnabled(bool value)
        {
            enabled = value;
            if (enabled)
            {
                EnsureDefaultSpin();
                RegenerateModeVariant();
                ClampCrystalLightRigIntensityForCurrentMode();
            }
        }

        public void ToggleEnabled()
        {
            SetEnabled(!enabled);
        }

        public void SetShape(DiamondFocusShape value)
        {
            shape = value;
            shapeTransitionFromShape = value;
            shapeTransitionToShape = value;
            shapeTransitionActive = false;
            shapeTransitionElapsed = 0f;
            RefreshInspectorLabels();
        }

        public void BeginShapeTransition(DiamondFocusShape value)
        {
            DiamondFocusShape target = (DiamondFocusShape)Mathf.Clamp((int)value, 0, ShapeCount - 1);

            if (!premiumShapeMorphingEnabled)
            {
                SetShape(target);
                return;
            }

            if (shapeTransitionActive && target == shapeTransitionFromShape)
            {
                DiamondFocusShape previousFrom = shapeTransitionFromShape;
                shapeTransitionFromShape = shapeTransitionToShape;
                shapeTransitionToShape = previousFrom;
                shapeTransitionElapsed = Mathf.Clamp(ShapeTransitionDuration - shapeTransitionElapsed, 0f, ShapeTransitionDuration);
                shape = target;
                RefreshInspectorLabels();
                return;
            }

            DiamondFocusShape from = shapeTransitionActive ? shapeTransitionToShape : shape;

            shape = target;
            shapeTransitionFromShape = from;
            shapeTransitionToShape = target;
            shapeTransitionElapsed = 0f;
            shapeTransitionActive = from != target;
            RefreshInspectorLabels();
        }

        public void TickShapeTransition(float deltaTime)
        {
            if (!shapeTransitionActive)
            {
                return;
            }

            shapeTransitionElapsed = Mathf.Min(ShapeTransitionDuration, shapeTransitionElapsed + Mathf.Max(0f, deltaTime));
            if (shapeTransitionElapsed >= ShapeTransitionDuration)
            {
                shapeTransitionActive = false;
                shapeTransitionFromShape = shapeTransitionToShape;
                shape = shapeTransitionToShape;
            }
        }

        public void SetShapeIndex(int index)
        {
            int wrapped = index % ShapeCount;
            if (wrapped < 0)
            {
                wrapped += ShapeCount;
            }

            shape = (DiamondFocusShape)wrapped;
        }

        public void CycleShape(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            SetShapeIndex(ShapeIndex + (direction > 0 ? 1 : -1));
        }

        public void SetMaterialMode(DiamondCrystalMaterialMode value)
        {
            materialMode = NormalizeMaterialMode(value);
            RegenerateModeVariant();
            RefreshInspectorLabels();
        }

        public void SetMaterialModeIndex(int index)
        {
            int count = MaterialModeCount;
            int wrapped = count > 0 ? index % count : 0;
            if (wrapped < 0)
            {
                wrapped += count;
            }

            SetMaterialMode(MaterialModeSequence[wrapped]);
        }

        public void CycleMaterialMode(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            SetMaterialModeIndex(MaterialModeIndex + (direction > 0 ? 1 : -1));
        }

        public void SetTargetRotationDirection(Vector2 value)
        {
            targetRotationDirection = value.sqrMagnitude > 0.0001f ? value.normalized : Vector2.zero;
        }

        public void SetCurrentRotationDirection(Vector2 value)
        {
            currentRotationDirection = NormalizeOrDefault(value);
        }

        public void SetCurrentRotationSpeed(float value)
        {
            float speed = Mathf.Clamp(value, MinRotationSpeed, MaxRotationSpeed);
            Vector3 direction = rotationVelocity.sqrMagnitude > 0.0001f
                ? rotationVelocity.normalized
                : BuildRotationAxis(CurrentRotationDirection);
            SetRotationVelocity(direction * speed);
        }

        public void SetTargetRotationSpeed(float value)
        {
            targetRotationSpeed = Mathf.Clamp(value, MinRotationSpeed, MaxRotationSpeed);
        }

        public void AdjustTargetRotationSpeed(float delta)
        {
            SetTargetRotationSpeed(TargetRotationSpeed + delta);
            SetCurrentRotationSpeed(CurrentRotationSpeed + delta);
        }

        public void SetRefractionIndex(float value)
        {
            SetRefractionCoefficient(value);
        }

        public void AdjustRefractionIndex(float delta)
        {
            SetRefractionCoefficient(RefractionCoefficient + delta);
        }

        public void SetRefractionCoefficient(float value)
        {
            opticalIOR = Mathf.Clamp(value, RefractionIndexMin, RefractionIndexMax);
        }

        public void AdjustDirectedLightIntensity(float delta)
        {
            SetDirectedLightIntensity(DirectedLightIntensity + delta);
        }

        public void SetDirectedLightIntensity(float value)
        {
            directedLightIntensity = Mathf.Clamp(value, DirectedLightIntensityMin, DirectedLightIntensityMax);
        }

        public void SetCrystalSimulationMode(CrystalRenderMode value)
        {
            crystalSimulationMode = value == CrystalRenderMode.RealMesh3D
                ? CrystalRenderMode.RealMesh3D
                : CrystalRenderMode.Billboard2D;
            ClampCrystalLightRigIntensityForCurrentMode();
        }

        public void ToggleCrystalSimulationMode()
        {
            SetCrystalSimulationMode(crystalSimulationMode == CrystalRenderMode.RealMesh3D
                ? CrystalRenderMode.Billboard2D
                : CrystalRenderMode.RealMesh3D);
        }

        public void SetPremiumCrystalScalePercent(float value)
        {
            premiumCrystalScalePercent = Mathf.Clamp(value, PremiumCrystalScalePercentMin, PremiumCrystalScalePercentMax);
        }

        public void AdjustPremiumCrystalScalePercent(float deltaPercent)
        {
            SetPremiumCrystalScalePercent(PremiumCrystalScalePercent + deltaPercent);
        }

        public void SetPremiumCrystalWheelScaleEnabled(bool value)
        {
            premiumCrystalWheelScaleEnabled = value;
        }

        public void SetPremiumCrystalWheelScaleStepPercent(float value)
        {
            premiumCrystalWheelScaleStepPercent = Mathf.Clamp(value, PremiumCrystalWheelScaleStepPercentMin, PremiumCrystalWheelScaleStepPercentMax);
        }

        public void SetPremiumCrystalOptic(PremiumCrystalOpticsParameter parameter, float value)
        {
            switch (parameter)
            {
                case PremiumCrystalOpticsParameter.Brightness:
                    premiumOpticsBrightness = Mathf.Clamp(value, 0.10f, 3f);
                    break;
                case PremiumCrystalOpticsParameter.Contrast:
                    premiumOpticsContrast = Mathf.Clamp(value, 0.20f, 3f);
                    break;
                case PremiumCrystalOpticsParameter.BloomGlow:
                    premiumOpticsBloomGlow = Mathf.Clamp(value, 0f, 5f);
                    break;
                case PremiumCrystalOpticsParameter.FacetHighlights:
                    premiumOpticsFacetHighlights = Mathf.Clamp(value, 0f, 6f);
                    break;
                case PremiumCrystalOpticsParameter.RefractionStrength:
                    premiumOpticsRefractionStrength = Mathf.Clamp(value, 0f, 5f);
                    break;
                case PremiumCrystalOpticsParameter.ReflectionStrength:
                    premiumOpticsReflectionStrength = Mathf.Clamp(value, 0f, 5f);
                    break;
                case PremiumCrystalOpticsParameter.InternalReflections:
                    premiumOpticsInternalReflections = Mathf.Clamp(value, 0f, 6f);
                    break;
                case PremiumCrystalOpticsParameter.BackgroundDistortion:
                    premiumOpticsBackgroundDistortion = Mathf.Clamp(value, 0f, 5f);
                    break;
                case PremiumCrystalOpticsParameter.DirectTransparency:
                    premiumOpticsDirectTransparency = Mathf.Clamp01(value);
                    break;
                case PremiumCrystalOpticsParameter.PrismDispersion:
                    premiumOpticsPrismDispersion = Mathf.Clamp(value, 0f, 5f);
                    break;
                case PremiumCrystalOpticsParameter.ChromaticAberration:
                    premiumOpticsChromaticAberration = Mathf.Clamp(value, 0f, 3f);
                    break;
                case PremiumCrystalOpticsParameter.RainbowEdge:
                    premiumOpticsRainbowEdge = Mathf.Clamp(value, 0f, 5f);
                    break;
                case PremiumCrystalOpticsParameter.SpectralSplit:
                    premiumOpticsSpectralSplit = Mathf.Clamp(value, 0f, 4f);
                    break;
                case PremiumCrystalOpticsParameter.CrystalDepth:
                    premiumOpticsCrystalDepth = Mathf.Clamp(value, 0.20f, 4f);
                    break;
                case PremiumCrystalOpticsParameter.Caustics:
                    premiumOpticsCaustics = Mathf.Clamp01(value);
                    break;
            }
        }

        public float GetPremiumCrystalOptic(PremiumCrystalOpticsParameter parameter)
        {
            switch (parameter)
            {
                case PremiumCrystalOpticsParameter.Brightness:
                    return PremiumOpticsBrightness;
                case PremiumCrystalOpticsParameter.Contrast:
                    return PremiumOpticsContrast;
                case PremiumCrystalOpticsParameter.BloomGlow:
                    return PremiumOpticsBloomGlow;
                case PremiumCrystalOpticsParameter.FacetHighlights:
                    return PremiumOpticsFacetHighlights;
                case PremiumCrystalOpticsParameter.RefractionStrength:
                    return PremiumOpticsRefractionStrength;
                case PremiumCrystalOpticsParameter.ReflectionStrength:
                    return PremiumOpticsReflectionStrength;
                case PremiumCrystalOpticsParameter.InternalReflections:
                    return PremiumOpticsInternalReflections;
                case PremiumCrystalOpticsParameter.BackgroundDistortion:
                    return PremiumOpticsBackgroundDistortion;
                case PremiumCrystalOpticsParameter.DirectTransparency:
                    return PremiumOpticsDirectTransparency;
                case PremiumCrystalOpticsParameter.PrismDispersion:
                    return PremiumOpticsPrismDispersion;
                case PremiumCrystalOpticsParameter.ChromaticAberration:
                    return PremiumOpticsChromaticAberration;
                case PremiumCrystalOpticsParameter.RainbowEdge:
                    return PremiumOpticsRainbowEdge;
                case PremiumCrystalOpticsParameter.SpectralSplit:
                    return PremiumOpticsSpectralSplit;
                case PremiumCrystalOpticsParameter.CrystalDepth:
                    return PremiumOpticsCrystalDepth;
                case PremiumCrystalOpticsParameter.Caustics:
                    return PremiumOpticsCaustics;
                default:
                    return 0f;
            }
        }

        public void TogglePremiumCrystalEffect(PremiumCrystalEffectToggle effect)
        {
            SetPremiumCrystalEffectEnabled(effect, !GetPremiumCrystalEffectEnabled(effect));
        }

        public void SetPremiumCrystalEffectEnabled(PremiumCrystalEffectToggle effect, bool value)
        {
            switch (effect)
            {
                case PremiumCrystalEffectToggle.HiddenReflectionBackground:
                    premiumHiddenReflectionBackgroundEnabled = value;
                    break;
                case PremiumCrystalEffectToggle.MirrorFacets:
                    premiumMirrorFacetsEnabled = value;
                    break;
                case PremiumCrystalEffectToggle.InternalReflections:
                    premiumInternalReflectionsEnabled = value;
                    break;
                case PremiumCrystalEffectToggle.Dispersion:
                    premiumDispersionEnabled = value;
                    break;
                case PremiumCrystalEffectToggle.RefractionDistortion:
                    premiumRefractionDistortionEnabled = value;
                    break;
                case PremiumCrystalEffectToggle.OpalIridescence:
                    premiumOpalIridescenceEnabled = value;
                    break;
                case PremiumCrystalEffectToggle.FacetHighlights:
                    premiumFacetHighlightsEnabled = value;
                    break;
                case PremiumCrystalEffectToggle.ShapeMorphing:
                    premiumShapeMorphingEnabled = value;
                    if (!value && shapeTransitionActive)
                    {
                        SetShape(shapeTransitionToShape);
                    }
                    break;
                case PremiumCrystalEffectToggle.DebugOpticalDiagnostics:
                    premiumDebugOpticalDiagnosticsEnabled = value;
                    debugMode = value
                        ? DiamondCrystalDebugMode.ReflectionOnly
                        : DiamondCrystalDebugMode.FinalCrystalComposite;
                    break;
            }
        }

        public bool GetPremiumCrystalEffectEnabled(PremiumCrystalEffectToggle effect)
        {
            switch (effect)
            {
                case PremiumCrystalEffectToggle.HiddenReflectionBackground:
                    return premiumHiddenReflectionBackgroundEnabled;
                case PremiumCrystalEffectToggle.MirrorFacets:
                    return premiumMirrorFacetsEnabled;
                case PremiumCrystalEffectToggle.InternalReflections:
                    return premiumInternalReflectionsEnabled;
                case PremiumCrystalEffectToggle.Dispersion:
                    return premiumDispersionEnabled;
                case PremiumCrystalEffectToggle.RefractionDistortion:
                    return premiumRefractionDistortionEnabled;
                case PremiumCrystalEffectToggle.OpalIridescence:
                    return premiumOpalIridescenceEnabled;
                case PremiumCrystalEffectToggle.FacetHighlights:
                    return premiumFacetHighlightsEnabled;
                case PremiumCrystalEffectToggle.ShapeMorphing:
                    return premiumShapeMorphingEnabled;
                case PremiumCrystalEffectToggle.DebugOpticalDiagnostics:
                    return premiumDebugOpticalDiagnosticsEnabled;
                default:
                    return false;
            }
        }

        public void ResetPremiumCrystalOpticalControls()
        {
            premiumOpticsBrightness = PremiumOpticsBrightnessDefault;
            premiumOpticsContrast = PremiumOpticsContrastDefault;
            premiumOpticsBloomGlow = PremiumOpticsBloomGlowDefault;
            premiumOpticsFacetHighlights = PremiumOpticsFacetHighlightsDefault;
            premiumOpticsRefractionStrength = PremiumOpticsRefractionStrengthDefault;
            premiumOpticsReflectionStrength = PremiumOpticsReflectionStrengthDefault;
            premiumOpticsInternalReflections = PremiumOpticsInternalReflectionsDefault;
            premiumOpticsBackgroundDistortion = PremiumOpticsBackgroundDistortionDefault;
            premiumOpticsDirectTransparency = PremiumOpticsDirectTransparencyDefault;
            premiumOpticsPrismDispersion = PremiumOpticsPrismDispersionDefault;
            premiumOpticsChromaticAberration = PremiumOpticsChromaticAberrationDefault;
            premiumOpticsRainbowEdge = PremiumOpticsRainbowEdgeDefault;
            premiumOpticsSpectralSplit = PremiumOpticsSpectralSplitDefault;
            premiumOpticsCrystalDepth = PremiumOpticsCrystalDepthDefault;
            premiumOpticsCaustics = 0f;
            premiumHiddenReflectionBackgroundEnabled = true;
            premiumMirrorFacetsEnabled = true;
            premiumInternalReflectionsEnabled = true;
            premiumDispersionEnabled = true;
            premiumRefractionDistortionEnabled = true;
            premiumOpalIridescenceEnabled = true;
            premiumFacetHighlightsEnabled = true;
            premiumShapeMorphingEnabled = true;
            premiumDebugOpticalDiagnosticsEnabled = false;
            debugMode = DiamondCrystalDebugMode.FinalCrystalComposite;
        }

        public void ApplyPremiumCrystalPreset(PremiumCrystalFactoryPreset preset)
        {
            SetEnabled(true);
            SetCrystalSimulationMode(CrystalRenderMode.RealMesh3D);
            CrystalLightRigSettings.SetRigEnabled(true);
            ResetPremiumCrystalOpticalControls();
            SetPremiumCrystalScalePercent(PremiumCrystalScalePercentDefault);

            switch (preset)
            {
                case PremiumCrystalFactoryPreset.BlueIce:
                    BeginShapeTransition(DiamondFocusShape.PolygonCrystal);
                    SetMaterialMode(DiamondCrystalMaterialMode.Sapphire);
                    SetPremiumCrystalScalePercent(118f);
                    ApplyPremiumCrystalOptics(1.28f, 1.35f, 1.45f, 2.5f, 2.35f, 1.95f, 1.75f, 1.55f, 0.08f, 2.2f, 0.95f, 1.45f, 1.35f, 1.35f, 0.35f);
                    break;

                case PremiumCrystalFactoryPreset.GoldenPrism:
                    BeginShapeTransition(DiamondFocusShape.RadialShardCrystal);
                    SetMaterialMode(DiamondCrystalMaterialMode.Topaz);
                    SetPremiumCrystalScalePercent(126f);
                    ApplyPremiumCrystalOptics(1.38f, 1.28f, 2.25f, 3.6f, 3.55f, 2.15f, 2.6f, 2.85f, 0.06f, 4.25f, 1.6f, 3.65f, 3.35f, 1.7f, 0.65f);
                    break;

                case PremiumCrystalFactoryPreset.RubyNight:
                    BeginShapeTransition(DiamondFocusShape.RhombicCrystal);
                    SetMaterialMode(DiamondCrystalMaterialMode.Ruby);
                    SetPremiumCrystalScalePercent(115f);
                    ApplyPremiumCrystalOptics(0.82f, 1.85f, 1.35f, 2.65f, 1.65f, 2.75f, 2.7f, 1.4f, 0.08f, 1.55f, 0.55f, 1.2f, 1.0f, 1.8f, 0.35f);
                    break;

                case PremiumCrystalFactoryPreset.EmeraldDepth:
                    BeginShapeTransition(DiamondFocusShape.OvalRingGem);
                    SetMaterialMode(DiamondCrystalMaterialMode.Emerald);
                    SetPremiumCrystalScalePercent(124f);
                    ApplyPremiumCrystalOptics(1.08f, 1.55f, 1.5f, 2.05f, 2.25f, 2.15f, 4.2f, 2.1f, 0.12f, 1.75f, 0.8f, 1.1f, 1.7f, 2.65f, 0.45f);
                    break;

                case PremiumCrystalFactoryPreset.OpalDream:
                    BeginShapeTransition(DiamondFocusShape.MandalaCrystal);
                    SetMaterialMode(DiamondCrystalMaterialMode.FuturisticPlastic);
                    premiumOpalIridescenceEnabled = true;
                    SetPremiumCrystalScalePercent(132f);
                    ApplyPremiumCrystalOptics(1.18f, 0.95f, 2.8f, 2.7f, 1.85f, 1.75f, 2.65f, 1.95f, 0.12f, 4.1f, 1.85f, 4.2f, 3.55f, 1.65f, 0.55f);
                    break;

                case PremiumCrystalFactoryPreset.CosmicGlass:
                    BeginShapeTransition(DiamondFocusShape.MandalaCrystal);
                    SetMaterialMode(DiamondCrystalMaterialMode.Amethyst);
                    SetPremiumCrystalScalePercent(145f);
                    ApplyPremiumCrystalOptics(1.42f, 1.2f, 3.2f, 3.8f, 3.25f, 2.45f, 3.75f, 3.35f, 0.08f, 4.7f, 2.2f, 4.65f, 3.85f, 2.05f, 0.8f);
                    break;

                case PremiumCrystalFactoryPreset.DarkLuxury:
                    BeginShapeTransition(DiamondFocusShape.StarDiamond);
                    SetMaterialMode(DiamondCrystalMaterialMode.Garnet);
                    SetPremiumCrystalScalePercent(112f);
                    ApplyPremiumCrystalOptics(0.7f, 2.15f, 1.15f, 2.45f, 1.25f, 3.45f, 2.5f, 1.2f, 0.05f, 1.2f, 0.45f, 0.8f, 0.8f, 1.75f, 0.25f);
                    premiumHiddenReflectionBackgroundEnabled = true;
                    premiumMirrorFacetsEnabled = true;
                    break;

                case PremiumCrystalFactoryPreset.AbsoluteMirror:
                    BeginShapeTransition(DiamondFocusShape.PolygonCrystal);
                    SetMaterialMode(DiamondCrystalMaterialMode.AbsoluteMirror);
                    SetPremiumCrystalScalePercent(120f);
                    ApplyPremiumCrystalOptics(1.12f, 1.9f, 1.85f, 4.3f, 0.95f, 5f, 3.2f, 1.75f, 0.0f, 2.8f, 1.1f, 2.1f, 1.95f, 1.7f, 0.35f);
                    premiumHiddenReflectionBackgroundEnabled = true;
                    premiumMirrorFacetsEnabled = true;
                    premiumRefractionDistortionEnabled = true;
                    premiumDispersionEnabled = true;
                    break;

                default:
                    BeginShapeTransition(DiamondFocusShape.StarDiamond);
                    SetMaterialMode(DiamondCrystalMaterialMode.Diamond);
                    SetPremiumCrystalScalePercent(118f);
                    ApplyPremiumCrystalOptics(1.45f, 1.38f, 2.1f, 3.45f, 2.85f, 2.65f, 2.95f, 2.15f, 0.06f, 3.25f, 1.25f, 2.85f, 2.45f, 1.85f, 0.55f);
                    break;
            }

            ClampCrystalLightRigIntensityForCurrentMode();
            RefreshInspectorLabels();
        }

        private void ApplyPremiumCrystalOptics(
            float brightness,
            float contrast,
            float bloomGlow,
            float facetHighlights,
            float refractionStrength,
            float reflectionStrength,
            float internalReflections,
            float backgroundDistortion,
            float directTransparency,
            float prismDispersion,
            float chromaticAberration,
            float rainbowEdge,
            float spectralSplit,
            float crystalDepth,
            float caustics)
        {
            SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.Brightness, brightness);
            SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.Contrast, contrast);
            SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.BloomGlow, bloomGlow);
            SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.FacetHighlights, facetHighlights);
            SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.RefractionStrength, refractionStrength);
            SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.ReflectionStrength, reflectionStrength);
            SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.InternalReflections, internalReflections);
            SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.BackgroundDistortion, backgroundDistortion);
            SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.DirectTransparency, directTransparency);
            SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.PrismDispersion, prismDispersion);
            SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.ChromaticAberration, chromaticAberration);
            SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.RainbowEdge, rainbowEdge);
            SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.SpectralSplit, spectralSplit);
            SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.CrystalDepth, crystalDepth);
            SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.Caustics, caustics);
        }

        public void SetCrystalLightRigIntensityForCurrentMode(float value)
        {
            CrystalLightRigSettings.SetLightIntensity(ClampCrystalLightRigIntensityForCurrentMode(value));
        }

        public void AdjustCrystalLightRigIntensityForCurrentMode(float delta)
        {
            float current = ClampCrystalLightRigIntensityForCurrentMode(CrystalLightRigSettings.LightIntensity);
            CrystalLightRigSettings.SetLightIntensity(ClampCrystalLightRigIntensityForCurrentMode(current + delta));
        }

        public void ClampCrystalLightRigIntensityForCurrentMode()
        {
            CrystalLightRigSettings.SetLightIntensity(ClampCrystalLightRigIntensityForCurrentMode(CrystalLightRigSettings.LightIntensity));
        }

        public void SetDebugMode(DiamondCrystalDebugMode value)
        {
            debugMode = value;
        }

        public void CycleDebugMode(int direction)
        {
            const int debugModeCount = 8;
            int next = ((int)debugMode + direction) % debugModeCount;
            if (next < 0)
            {
                next += debugModeCount;
            }

            debugMode = (DiamondCrystalDebugMode)next;
        }

        public void SetKaleidoscopeTexBindingStatus(bool bound)
        {
            kaleidoscopeTexBindingStatus = bound ? "Bound" : "Missing / fallback";
        }

        public void RefreshInspectorLabels()
        {
            currentShapeName = ShapeLabel;
            currentModeName = MaterialModeLabel;
        }

        public void SetRotationVelocity(Vector3 value)
        {
            rotationVelocity = Vector3.ClampMagnitude(value, MaxRotationSpeed);
            currentRotationSpeed = rotationVelocity.magnitude;

            Vector2 planar = new Vector2(rotationVelocity.y, rotationVelocity.x);
            currentRotationDirection = planar.sqrMagnitude > 0.0001f ? planar.normalized : Vector2.zero;
            targetRotationSpeed = currentRotationSpeed;
        }

        public void AddRotationVelocity(Vector3 delta)
        {
            SetRotationVelocity(rotationVelocity + delta);
        }

        public void SetRotationEuler(Vector3 value)
        {
            rotationEuler = new Vector3(TrimLongAngle(value.x), TrimLongAngle(value.y), TrimLongAngle(value.z));
        }

        public Vector3 BuildRotationAxis(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return rotationVelocity.sqrMagnitude > 0.0001f
                    ? rotationVelocity.normalized
                    : new Vector3(0.45f, 0.78f, 0.2f).normalized;
            }

            direction.Normalize();
            return new Vector3(direction.y, direction.x, direction.x * direction.y * 0.45f).normalized;
        }

        public static string GetPremiumCrystalEffectLabel(PremiumCrystalEffectToggle value)
        {
            switch (value)
            {
                case PremiumCrystalEffectToggle.HiddenReflectionBackground:
                    return "Hidden reflection background";
                case PremiumCrystalEffectToggle.MirrorFacets:
                    return "Mirror facets";
                case PremiumCrystalEffectToggle.InternalReflections:
                    return "Internal reflections";
                case PremiumCrystalEffectToggle.Dispersion:
                    return "Dispersion";
                case PremiumCrystalEffectToggle.RefractionDistortion:
                    return "Refraction distortion";
                case PremiumCrystalEffectToggle.OpalIridescence:
                    return "Opal iridescence";
                case PremiumCrystalEffectToggle.FacetHighlights:
                    return "Facet highlights";
                case PremiumCrystalEffectToggle.ShapeMorphing:
                    return "Shape morphing";
                case PremiumCrystalEffectToggle.DebugOpticalDiagnostics:
                    return "Debug optical diagnostics";
                default:
                    return "Unknown Premium3D effect";
            }
        }

        public static string GetPremiumCrystalFactoryPresetLabel(PremiumCrystalFactoryPreset value)
        {
            switch (value)
            {
                case PremiumCrystalFactoryPreset.BlueIce:
                    return "Blue Ice";
                case PremiumCrystalFactoryPreset.GoldenPrism:
                    return "Golden Prism";
                case PremiumCrystalFactoryPreset.RubyNight:
                    return "Ruby Night";
                case PremiumCrystalFactoryPreset.EmeraldDepth:
                    return "Emerald Depth";
                case PremiumCrystalFactoryPreset.OpalDream:
                    return "Opal Dream";
                case PremiumCrystalFactoryPreset.CosmicGlass:
                    return "Cosmic Glass";
                case PremiumCrystalFactoryPreset.DarkLuxury:
                    return "Dark Luxury";
                case PremiumCrystalFactoryPreset.AbsoluteMirror:
                    return "Absolute Mirror";
                default:
                    return "Diamond Palace";
            }
        }

        private static string FormatEnabled(bool value)
        {
            return value ? "on" : "off";
        }

        public static string GetShapeLabel(DiamondFocusShape value)
        {
            switch (value)
            {
                case DiamondFocusShape.FacetedCube:
                    return "Faceted Cube";
                case DiamondFocusShape.DiscoBall:
                    return "Disco Ball";
                case DiamondFocusShape.TetrahedralCrystal:
                    return "Triangular Crystal";
                case DiamondFocusShape.RhombicCrystal:
                    return "Rhombic Crystal";
                case DiamondFocusShape.OvalRingGem:
                    return "Oval Ring Gem";
                case DiamondFocusShape.RadialShardCrystal:
                    return "Radial Shard Crystal";
                case DiamondFocusShape.MandalaCrystal:
                    return "Mandala Crystal";
                case DiamondFocusShape.StarDiamond:
                    return "Star Diamond";
                case DiamondFocusShape.PolygonCrystal:
                    return "Polygon Crystal";
                default:
                    return "Classic Diamond";
            }
        }

        public static string GetMaterialModeLabel(DiamondCrystalMaterialMode value)
        {
            switch (value)
            {
                case DiamondCrystalMaterialMode.Emerald:
                    return "Emerald";
                case DiamondCrystalMaterialMode.Topaz:
                    return "Topaz";
                case DiamondCrystalMaterialMode.Ruby:
                    return "Ruby";
                case DiamondCrystalMaterialMode.Sapphire:
                    return "Sapphire";
                case DiamondCrystalMaterialMode.Amethyst:
                    return "Amethyst";
                case DiamondCrystalMaterialMode.Aquamarine:
                    return "Aquamarine";
                case DiamondCrystalMaterialMode.Garnet:
                    return "Garnet";
                case DiamondCrystalMaterialMode.FuturisticPlastic:
                    return "Futuristic Plastic";
                case DiamondCrystalMaterialMode.Mercury:
                    return "Mercury";
                case DiamondCrystalMaterialMode.StainlessSteel:
                    return "Stainless Steel";
                case DiamondCrystalMaterialMode.Chrome:
                    return "Chrome";
                case DiamondCrystalMaterialMode.CastIron:
                    return "Cast Iron";
                case DiamondCrystalMaterialMode.PolishedBrass:
                    return "Polished Brass";
                case DiamondCrystalMaterialMode.AbsoluteMirror:
                    return "Absolute Mirror + Prism";
                case DiamondCrystalMaterialMode.LegacyGlow:
                    return "Legacy Glow";
                case DiamondCrystalMaterialMode.LegacyOpticalPhysics:
                    return "Legacy Optical IOR";
                case DiamondCrystalMaterialMode.LegacyGeneratedMaterial:
                    return "Legacy Generated Material";
                default:
                    return "High-Purity Diamond";
            }
        }

        public static string GetGeneratedMaterialKindLabel(DiamondGeneratedMaterialKind value)
        {
            switch (value)
            {
                case DiamondGeneratedMaterialKind.Metal:
                    return "Metal";
                case DiamondGeneratedMaterialKind.Plastic:
                    return "Plastic";
                case DiamondGeneratedMaterialKind.Stone:
                    return "Stone";
                default:
                    return "Wood";
            }
        }

        private static Vector2 NormalizeOrDefault(Vector2 value)
        {
            return value.sqrMagnitude > 0.0001f ? value.normalized : new Vector2(0.35f, 0.75f).normalized;
        }

        private float ClampCrystalLightRigIntensityForCurrentMode(float value)
        {
            return Mathf.Clamp(value, ActiveCrystalBrightnessMin, ActiveCrystalBrightnessMax);
        }

        public static DiamondCrystalMaterialMode GetModeAtIndex(int index)
        {
            int count = MaterialModeCount;
            if (count <= 0)
            {
                return DiamondCrystalMaterialMode.Diamond;
            }

            int wrapped = index % count;
            if (wrapped < 0)
            {
                wrapped += count;
            }

            return MaterialModeSequence[wrapped];
        }

        private static int GetMaterialModeIndex(DiamondCrystalMaterialMode value)
        {
            DiamondCrystalMaterialMode normalized = NormalizeMaterialMode(value);
            for (int index = 0; index < MaterialModeSequence.Length; index++)
            {
                if (MaterialModeSequence[index] == normalized)
                {
                    return index;
                }
            }

            return 1;
        }

        private static DiamondCrystalMaterialMode NormalizeMaterialMode(DiamondCrystalMaterialMode value)
        {
            switch (value)
            {
                case DiamondCrystalMaterialMode.LegacyGlow:
                    return DiamondCrystalMaterialMode.FuturisticPlastic;
                case DiamondCrystalMaterialMode.LegacyOpticalPhysics:
                case DiamondCrystalMaterialMode.LegacyGeneratedMaterial:
                    return DiamondCrystalMaterialMode.Diamond;
                default:
                    for (int index = 0; index < MaterialModeSequence.Length; index++)
                    {
                        if (MaterialModeSequence[index] == value)
                        {
                            return value;
                        }
                    }

                    return DiamondCrystalMaterialMode.Diamond;
            }
        }

        private void RegenerateModeVariant()
        {
            if (enableRandomVariants)
            {
                generatedMaterialSeed = generatedMaterialSeed >= int.MaxValue - 1 ? 1 : generatedMaterialSeed + 1;
            }

            if (materialMode == DiamondCrystalMaterialMode.FuturisticPlastic)
            {
                generatedMaterialColor = new Color(0.68f, 0.9f, 1f, 1f);
                return;
            }

            if (materialMode == DiamondCrystalMaterialMode.AbsoluteMirror)
            {
                generatedMaterialColor = new Color(0.92f, 0.96f, 1f, 1f);
                return;
            }

            if (materialMode == DiamondCrystalMaterialMode.PolishedBrass)
            {
                generatedMaterialColor = new Color(1f, 0.78f, 0.38f, 1f);
                return;
            }

            if (materialMode == DiamondCrystalMaterialMode.Diamond)
            {
                generatedMaterialColor = new Color(0.9f, 0.98f, 1f, 1f);
            }
        }

        private void EnsureDefaultSpin()
        {
            if (rotationVelocity.magnitude >= 0.1f)
            {
                SetRotationVelocity(rotationVelocity);
                return;
            }

            SetRotationVelocity(new Vector3(DefaultRotationSpeed * 0.45f, DefaultRotationSpeed * 0.78f, DefaultRotationSpeed * 0.2f));
        }

        private static float TrimLongAngle(float value)
        {
            const float limit = 100000f;
            if (value > limit || value < -limit)
            {
                return Mathf.Repeat(value + limit, limit * 2f) - limit;
            }

            return value;
        }
    }
}
