using System;
using UnityEngine;

namespace Kaleidoscope2.Core
{
    public enum CrystalExperimentPresetType
    {
        Normal = 0,
        AlienArtifactCore = 1,
        PredatorCrystal = 2,
        SingularityPrism = 3,
        RecursiveEye = 4,
        GhostDiamond = 5,
        PlasmaLattice = 6,
        ObsidianCore = 7,
        BrokenFacetStorm = 8,
        LiquidGlass = 9,
        CelestialReactor = 10
    }

    public sealed class CrystalExperimentPreset
    {
        private static readonly CrystalExperimentPreset[] Presets =
        {
            new CrystalExperimentPreset(
                CrystalExperimentPresetType.AlienArtifactCore,
                "Alien Artifact Core",
                DiamondFocusShape.MandalaCrystal,
                DiamondCrystalMaterialMode.Amethyst,
                DiamondCrystalDebugMode.FinalCrystalComposite,
                132f, 1.18f, 1.75f, 3.6f, 4.7f, 4.1f, 2.6f, 5.2f, 3.8f, 0.03f, 4.4f, 1.8f, 3.7f, 3.4f, 2.8f, 0.9f,
                0.02f, 0.116f, 1.85f, 1.12f, 2.1f, 2.85f, 0.22f, 2.4f, 1.9f, 1.0f, 0.62f,
                0.42f, 3.5f, 3f, 3f, 2.8f, 2.6f, 2.7f, 1f, 3f, 0.02f, 2.7f, 2.8f, 1.65f,
                2.55f, 1.2f, 2.3f, 2.2f, 1.85f, 2.6f, 2.8f, true, true, true, true, true, true, true, true, false, true, 16f, 8),

            new CrystalExperimentPreset(
                CrystalExperimentPresetType.PredatorCrystal,
                "Predator Crystal",
                DiamondFocusShape.RadialShardCrystal,
                DiamondCrystalMaterialMode.Emerald,
                DiamondCrystalDebugMode.FinalCrystalComposite,
                118f, 0.95f, 2.4f, 1.6f, 5.2f, 3.5f, 2.9f, 3.4f, 3.2f, 0.04f, 3.8f, 1.25f, 4.4f, 2.9f, 2.1f, 0.55f,
                0.03f, 0.112f, 1.65f, 1.28f, 2.45f, 2.1f, 0.26f, 2.65f, 1.75f, 1.15f, 0.54f,
                0.36f, 3.1f, 2.7f, 2.2f, 1.95f, 2.2f, 2.1f, 1f, 1.9f, 0.03f, 2.3f, 2.45f, 1.5f,
                1.74f, 1.0f, 2.0f, 1.9f, 1.65f, 2.3f, 2.1f, true, true, true, true, true, false, true, true, false, true, 14f, 7),

            new CrystalExperimentPreset(
                CrystalExperimentPresetType.SingularityPrism,
                "Singularity Prism",
                DiamondFocusShape.PolygonCrystal,
                DiamondCrystalMaterialMode.Diamond,
                DiamondCrystalDebugMode.FinalCrystalComposite,
                112f, 0.82f, 2.15f, 1.25f, 3.9f, 5.4f, 2.45f, 5.6f, 5f, 0.0f, 4.7f, 1.7f, 3.2f, 3.8f, 3.35f, 0.8f,
                0f, 0.12f, 1.9f, 1.46f, 1.7f, 2.35f, 0.34f, 2.1f, 1.35f, 1.7f, 0.34f,
                0.28f, 3.3f, 2.9f, 3f, 2.9f, 2.8f, 2.75f, 1f, 2.4f, 0f, 3f, 2.95f, 1.9f,
                2.42f, 0.35f, 2.2f, 2.6f, 1.9f, 3f, 2.8f, true, true, true, true, true, true, true, true, false, true, 15f, 8),

            new CrystalExperimentPreset(
                CrystalExperimentPresetType.RecursiveEye,
                "Recursive Eye",
                DiamondFocusShape.MandalaCrystal,
                DiamondCrystalMaterialMode.Aquamarine,
                DiamondCrystalDebugMode.FinalCrystalComposite,
                138f, 1.1f, 1.35f, 2.6f, 3.2f, 3.7f, 2.2f, 5.6f, 2.85f, 0.08f, 3.3f, 1.15f, 2.6f, 2.8f, 2.2f, 0.65f,
                0.04f, 0.102f, 1.45f, 0.9f, 1.8f, 2.95f, 0.28f, 1.9f, 1.6f, 1.35f, 0.44f,
                0.5f, 2.6f, 2.5f, 2.85f, 2.15f, 2.45f, 2.35f, 0.96f, 2.2f, 0.04f, 2.5f, 2.3f, 1.55f,
                1.62f, 0.55f, 1.8f, 2f, 1.55f, 2.2f, 2.6f, true, true, true, true, true, true, true, true, false, true, 12f, 6),

            new CrystalExperimentPreset(
                CrystalExperimentPresetType.GhostDiamond,
                "Ghost Diamond",
                DiamondFocusShape.ClassicDiamond,
                DiamondCrystalMaterialMode.Diamond,
                DiamondCrystalDebugMode.FinalCrystalComposite,
                106f, 1.02f, 0.86f, 0.75f, 1.9f, 1.45f, 1.1f, 1.4f, 0.95f, 0.62f, 1.1f, 0.45f, 1.1f, 0.75f, 0.72f, 0.15f,
                0.55f, 0.058f, 0.74f, 0.36f, 4.8f, 0.75f, 0.035f, 1.8f, 0.42f, 0.42f, 0.28f,
                0.58f, 0.95f, 0.75f, 0.95f, 0.65f, 0.75f, 0.85f, 0.2f, 0.7f, 0.24f, 0.92f, 0.9f, 0.55f,
                1.45f, -0.8f, 0.35f, 0.75f, 0.7f, 0.7f, 0.9f, true, true, true, true, true, true, true, true, false, true, 5f, 4),

            new CrystalExperimentPreset(
                CrystalExperimentPresetType.PlasmaLattice,
                "Plasma Lattice",
                DiamondFocusShape.StarDiamond,
                DiamondCrystalMaterialMode.FuturisticPlastic,
                DiamondCrystalDebugMode.FinalCrystalComposite,
                128f, 1.8f, 1.25f, 4.8f, 5.8f, 3.4f, 2.2f, 4.9f, 2.6f, 0.04f, 5f, 2.4f, 5f, 4f, 2.1f, 1f,
                0.02f, 0.11f, 2f, 1.18f, 2.4f, 3f, 0.18f, 2.8f, 2.35f, 1.6f, 0.76f,
                0.92f, 3.8f, 3f, 3f, 2.9f, 2.8f, 2.75f, 1f, 3f, 0.015f, 3f, 3f, 1.9f,
                1.78f, 1.8f, 2.8f, 2.9f, 1.9f, 3f, 2.6f, true, true, true, true, true, true, true, true, false, true, 20f, 8),

            new CrystalExperimentPreset(
                CrystalExperimentPresetType.ObsidianCore,
                "Obsidian Core",
                DiamondFocusShape.RhombicCrystal,
                DiamondCrystalMaterialMode.Garnet,
                DiamondCrystalDebugMode.FinalCrystalComposite,
                116f, 0.48f, 2.65f, 0.55f, 3.4f, 1.15f, 4.6f, 2.8f, 0.85f, 0f, 0.95f, 0.28f, 0.75f, 0.7f, 2.4f, 0.25f,
                0f, 0.052f, 0.58f, 1.5f, 1.25f, 2.8f, 0.045f, 3f, 0.45f, 2.1f, 0.18f,
                0.14f, 1.35f, 1.0f, 1.65f, 1.25f, 1.55f, 1.6f, 1f, 0.7f, 0f, 2.7f, 2.1f, 1.95f,
                1.82f, -1.6f, 0.85f, 1.3f, 1.8f, 0.9f, 2.5f, true, true, true, true, true, false, true, true, false, true, 10f, 5),

            new CrystalExperimentPreset(
                CrystalExperimentPresetType.BrokenFacetStorm,
                "Broken Facet Storm",
                DiamondFocusShape.RadialShardCrystal,
                DiamondCrystalMaterialMode.Chrome,
                DiamondCrystalDebugMode.FinalCrystalComposite,
                142f, 1.34f, 2.25f, 3.7f, 6f, 5f, 3.3f, 5.6f, 5f, 0.02f, 4.8f, 2.6f, 4.4f, 3.8f, 1.5f, 0.85f,
                0.01f, 0.12f, 1.95f, 1.5f, 0.95f, 3f, 0.35f, 3f, 2.2f, 2.2f, 0.8f,
                0.38f, 3.8f, 3f, 3f, 3f, 2.9f, 3f, 1f, 2.7f, 0.01f, 3f, 3f, 2f,
                2.2f, 0.8f, 2.8f, 3f, 2f, 3f, 3f, true, true, true, true, true, true, true, true, false, true, 18f, 8),

            new CrystalExperimentPreset(
                CrystalExperimentPresetType.LiquidGlass,
                "Liquid Glass",
                DiamondFocusShape.OvalRingGem,
                DiamondCrystalMaterialMode.Aquamarine,
                DiamondCrystalDebugMode.FinalCrystalComposite,
                122f, 1.22f, 0.9f, 1.55f, 1.4f, 2.8f, 1.55f, 2.1f, 2.6f, 0.2f, 1.8f, 0.7f, 1.5f, 1.15f, 1.4f, 0.35f,
                0.16f, 0.088f, 0.92f, 0.62f, 2.7f, 1.25f, 0.12f, 1.15f, 0.95f, 0.72f, 0.36f,
                0.4f, 1.55f, 1.4f, 1.55f, 1.3f, 1.75f, 1.55f, 0.68f, 1.2f, 0.1f, 1.45f, 1.35f, 0.96f,
                1.55f, 0.2f, 0.8f, 1.2f, 1.05f, 1.2f, 1.5f, true, true, true, true, true, true, true, true, false, true, 9f, 5),

            new CrystalExperimentPreset(
                CrystalExperimentPresetType.CelestialReactor,
                "Celestial Reactor",
                DiamondFocusShape.StarDiamond,
                DiamondCrystalMaterialMode.Topaz,
                DiamondCrystalDebugMode.FinalCrystalComposite,
                136f, 2.15f, 1.35f, 4.4f, 5.5f, 3.5f, 3.2f, 4.8f, 2.9f, 0.02f, 4.4f, 1.9f, 4.6f, 3.6f, 2.45f, 1f,
                0.01f, 0.108f, 1.8f, 1.14f, 1.6f, 2.75f, 0.2f, 2.6f, 2.3f, 1.8f, 0.7f,
                0.72f, 3.4f, 2.7f, 2.7f, 2.6f, 2.55f, 2.7f, 1f, 3f, 0.01f, 2.8f, 3f, 1.8f,
                2.0f, 2.2f, 2.6f, 2.8f, 1.75f, 2.8f, 2.6f, true, true, true, true, true, true, true, true, false, true, 19f, 8)
        };

        private CrystalExperimentPreset(
            CrystalExperimentPresetType type,
            string label,
            DiamondFocusShape shape,
            DiamondCrystalMaterialMode materialMode,
            DiamondCrystalDebugMode debugMode,
            float premiumScalePercent,
            float brightness,
            float contrast,
            float bloomGlow,
            float facetHighlights,
            float refraction,
            float reflection,
            float internalReflections,
            float backgroundDistortion,
            float directTransparency,
            float prismDispersion,
            float chromaticAberration,
            float rainbowEdge,
            float spectralSplit,
            float crystalDepth,
            float caustics,
            float transparency,
            float refractionStrength,
            float dispersionStrength,
            float reflectionStrength,
            float fresnelPower,
            float internalBrightness,
            float noiseDistortionStrength,
            float edgeHighlight,
            float facetContrast,
            float internalGlow,
            float bloomBoostBase,
            float bloomBoostExtra,
            float screenScale,
            float diamondLikeRefraction,
            float spectralDispersion,
            float highEnergyCaustics,
            float multiBounceInternalReflections,
            float cinematicCrystalOptics,
            float physicallyBasedRefraction,
            float deepVolumetricLightScattering,
            float crystalSolidity,
            float blueWhitePlasmaEnergy,
            float directTransmission,
            float totalInternalReturn,
            float spectralFireIntensity,
            float facetDepthContrast,
            float opticalIor,
            float directedLightIntensity,
            float opticalCaustics,
            float opticalDispersion,
            float totalInternalReflection,
            bool hiddenReflection,
            bool mirrorFacets,
            bool internalReflectionToggle,
            bool dispersionToggle,
            bool refractionDistortion,
            bool opalIridescence,
            bool facetHighlightToggle,
            bool shapeMorphing,
            bool opticalDiagnostics,
            bool lightRigEnabled,
            float lightRigIntensity,
            int activeLightCount)
        {
            Type = type;
            Label = label;
            Shape = shape;
            MaterialMode = materialMode;
            DebugMode = debugMode;
            PremiumScalePercent = premiumScalePercent;
            Brightness = brightness;
            Contrast = contrast;
            BloomGlow = bloomGlow;
            FacetHighlights = facetHighlights;
            Refraction = refraction;
            Reflection = reflection;
            InternalReflections = internalReflections;
            BackgroundDistortion = backgroundDistortion;
            DirectTransparency = directTransparency;
            PrismDispersion = prismDispersion;
            ChromaticAberration = chromaticAberration;
            RainbowEdge = rainbowEdge;
            SpectralSplit = spectralSplit;
            CrystalDepth = crystalDepth;
            Caustics = caustics;
            Transparency = transparency;
            RefractionStrength = refractionStrength;
            DispersionStrength = dispersionStrength;
            ReflectionStrength = reflectionStrength;
            FresnelPower = fresnelPower;
            InternalBrightness = internalBrightness;
            NoiseDistortionStrength = noiseDistortionStrength;
            EdgeHighlight = edgeHighlight;
            FacetContrast = facetContrast;
            InternalGlow = internalGlow;
            BloomBoostBase = bloomBoostBase;
            BloomBoostExtra = bloomBoostExtra;
            ScreenScale = screenScale;
            DiamondLikeRefraction = diamondLikeRefraction;
            SpectralDispersion = spectralDispersion;
            HighEnergyCaustics = highEnergyCaustics;
            MultiBounceInternalReflections = multiBounceInternalReflections;
            CinematicCrystalOptics = cinematicCrystalOptics;
            PhysicallyBasedRefraction = physicallyBasedRefraction;
            DeepVolumetricLightScattering = deepVolumetricLightScattering;
            CrystalSolidity = crystalSolidity;
            BlueWhitePlasmaEnergy = blueWhitePlasmaEnergy;
            DirectTransmission = directTransmission;
            TotalInternalReturn = totalInternalReturn;
            SpectralFireIntensity = spectralFireIntensity;
            FacetDepthContrast = facetDepthContrast;
            OpticalIor = opticalIor;
            DirectedLightIntensity = directedLightIntensity;
            OpticalCaustics = opticalCaustics;
            OpticalDispersion = opticalDispersion;
            TotalInternalReflection = totalInternalReflection;
            HiddenReflection = hiddenReflection;
            MirrorFacets = mirrorFacets;
            InternalReflectionToggle = internalReflectionToggle;
            DispersionToggle = dispersionToggle;
            RefractionDistortion = refractionDistortion;
            OpalIridescence = opalIridescence;
            FacetHighlightToggle = facetHighlightToggle;
            ShapeMorphing = shapeMorphing;
            OpticalDiagnostics = opticalDiagnostics;
            LightRigEnabled = lightRigEnabled;
            LightRigIntensity = lightRigIntensity;
            ActiveLightCount = activeLightCount;
        }

        public CrystalExperimentPresetType Type { get; private set; }
        public string Label { get; private set; }
        public DiamondFocusShape Shape { get; private set; }
        public DiamondCrystalMaterialMode MaterialMode { get; private set; }
        public DiamondCrystalDebugMode DebugMode { get; private set; }
        public float PremiumScalePercent { get; private set; }
        public float Brightness { get; private set; }
        public float Contrast { get; private set; }
        public float BloomGlow { get; private set; }
        public float FacetHighlights { get; private set; }
        public float Refraction { get; private set; }
        public float Reflection { get; private set; }
        public float InternalReflections { get; private set; }
        public float BackgroundDistortion { get; private set; }
        public float DirectTransparency { get; private set; }
        public float PrismDispersion { get; private set; }
        public float ChromaticAberration { get; private set; }
        public float RainbowEdge { get; private set; }
        public float SpectralSplit { get; private set; }
        public float CrystalDepth { get; private set; }
        public float Caustics { get; private set; }
        public float Transparency { get; private set; }
        public float RefractionStrength { get; private set; }
        public float DispersionStrength { get; private set; }
        public float ReflectionStrength { get; private set; }
        public float FresnelPower { get; private set; }
        public float InternalBrightness { get; private set; }
        public float NoiseDistortionStrength { get; private set; }
        public float EdgeHighlight { get; private set; }
        public float FacetContrast { get; private set; }
        public float InternalGlow { get; private set; }
        public float BloomBoostBase { get; private set; }
        public float BloomBoostExtra { get; private set; }
        public float ScreenScale { get; private set; }
        public float DiamondLikeRefraction { get; private set; }
        public float SpectralDispersion { get; private set; }
        public float HighEnergyCaustics { get; private set; }
        public float MultiBounceInternalReflections { get; private set; }
        public float CinematicCrystalOptics { get; private set; }
        public float PhysicallyBasedRefraction { get; private set; }
        public float DeepVolumetricLightScattering { get; private set; }
        public float CrystalSolidity { get; private set; }
        public float BlueWhitePlasmaEnergy { get; private set; }
        public float DirectTransmission { get; private set; }
        public float TotalInternalReturn { get; private set; }
        public float SpectralFireIntensity { get; private set; }
        public float FacetDepthContrast { get; private set; }
        public float OpticalIor { get; private set; }
        public float DirectedLightIntensity { get; private set; }
        public float OpticalCaustics { get; private set; }
        public float OpticalDispersion { get; private set; }
        public float TotalInternalReflection { get; private set; }
        public bool HiddenReflection { get; private set; }
        public bool MirrorFacets { get; private set; }
        public bool InternalReflectionToggle { get; private set; }
        public bool DispersionToggle { get; private set; }
        public bool RefractionDistortion { get; private set; }
        public bool OpalIridescence { get; private set; }
        public bool FacetHighlightToggle { get; private set; }
        public bool ShapeMorphing { get; private set; }
        public bool OpticalDiagnostics { get; private set; }
        public bool LightRigEnabled { get; private set; }
        public float LightRigIntensity { get; private set; }
        public int ActiveLightCount { get; private set; }

        public static int SelectablePresetCount { get { return Presets.Length + 1; } }

        public static bool TryGet(CrystalExperimentPresetType type, out CrystalExperimentPreset preset)
        {
            for (int index = 0; index < Presets.Length; index++)
            {
                if (Presets[index].Type == type)
                {
                    preset = Presets[index];
                    return true;
                }
            }

            preset = null;
            return false;
        }

        public static CrystalExperimentPresetType Cycle(CrystalExperimentPresetType current, int direction)
        {
            int step = direction >= 0 ? 1 : -1;
            int index = Mathf.Clamp((int)current, 0, SelectablePresetCount - 1);
            int next = (index + step) % SelectablePresetCount;
            if (next < 0)
            {
                next += SelectablePresetCount;
            }

            return (CrystalExperimentPresetType)next;
        }

        public static string GetLabel(CrystalExperimentPresetType type)
        {
            if (type == CrystalExperimentPresetType.Normal)
            {
                return "Normal";
            }

            CrystalExperimentPreset preset;
            return TryGet(type, out preset) ? preset.Label : "Unknown Experiment";
        }
    }

    internal sealed class CrystalExperimentPresetApplier
    {
        private CrystalExperimentSnapshot previousState;
        private bool hasPreviousState;

        public void Apply(DiamondFocusSettings settings, CrystalExperimentPresetType type)
        {
            if (settings == null)
            {
                return;
            }

            if (type == CrystalExperimentPresetType.Normal)
            {
                RestorePrevious(settings);
                return;
            }

            CrystalExperimentPreset preset;
            if (!CrystalExperimentPreset.TryGet(type, out preset))
            {
                return;
            }

            if (!hasPreviousState || settings.ActiveExperimentalCrystalPreset == CrystalExperimentPresetType.Normal)
            {
                previousState = settings.CaptureCrystalExperimentState();
                hasPreviousState = true;
            }

            settings.ApplyCrystalExperimentPresetValues(preset);
            settings.SetActiveExperimentalCrystalPreset(type);
        }

        public void RestorePrevious(DiamondFocusSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            if (hasPreviousState && previousState != null)
            {
                settings.RestoreCrystalExperimentState(previousState);
                previousState = null;
                hasPreviousState = false;
            }

            settings.SetActiveExperimentalCrystalPreset(CrystalExperimentPresetType.Normal);
        }
    }

    internal sealed class CrystalExperimentSnapshot
    {
        public bool Enabled;
        public DiamondFocusShape Shape;
        public DiamondFocusShape ShapeTransitionFromShape;
        public DiamondFocusShape ShapeTransitionToShape;
        public bool ShapeTransitionActive;
        public float ShapeTransitionElapsed;
        public DiamondCrystalMaterialMode MaterialMode;
        public DiamondGeneratedMaterialKind GeneratedMaterialKind;
        public Color GeneratedMaterialColor;
        public int GeneratedMaterialSeed;
        public DiamondCrystalDebugMode DebugMode;
        public float PremiumScalePercent;
        public float PremiumBrightness;
        public float PremiumContrast;
        public float PremiumBloomGlow;
        public float PremiumFacetHighlights;
        public float PremiumRefraction;
        public float PremiumReflection;
        public float PremiumInternalReflections;
        public float PremiumBackgroundDistortion;
        public float PremiumDirectTransparency;
        public float PremiumPrismDispersion;
        public float PremiumChromaticAberration;
        public float PremiumRainbowEdge;
        public float PremiumSpectralSplit;
        public float PremiumCrystalDepth;
        public float PremiumCaustics;
        public bool HiddenReflection;
        public bool MirrorFacets;
        public bool InternalReflectionsToggle;
        public bool DispersionToggle;
        public bool RefractionDistortion;
        public bool OpalIridescence;
        public bool FacetHighlightsToggle;
        public bool ShapeMorphing;
        public bool OpticalDiagnostics;
        public float Transparency;
        public float RefractionStrength;
        public float DispersionStrength;
        public float ReflectionStrength;
        public float FresnelPower;
        public float InternalBrightness;
        public float NoiseDistortionStrength;
        public float EdgeHighlight;
        public float ChromaticAberrationBase;
        public float ChromaticAberrationExtra;
        public float FacetContrast;
        public float InternalGlow;
        public float BloomBoostBase;
        public float BloomBoostExtra;
        public float ScreenScale;
        public float DiamondLikeRefraction;
        public float SpectralDispersion;
        public float HighEnergyCaustics;
        public float MultiBounceInternalReflections;
        public float CinematicCrystalOptics;
        public float PhysicallyBasedRefraction;
        public float DeepVolumetricLightScattering;
        public float CrystalSolidity;
        public float BlueWhitePlasmaEnergy;
        public float DirectTransmission;
        public float TotalInternalReturn;
        public float SpectralFireIntensity;
        public float FacetDepthContrast;
        public float OpticalIor;
        public float DirectedLightIntensity;
        public float OpticalCaustics;
        public float OpticalDispersion;
        public float TotalInternalReflection;
        public bool LightRigEnabled;
        public float LightRigIntensity;
        public int ActiveLightCountLimit;
    }
}
