using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus
{
    public struct CrystalModeProfile
    {
        public DiamondCrystalMaterialMode Mode;
        public CrystalOpticsProfile Optics;
        public CrystalSurfaceProfile Surface;
    }

    public static class CrystalModeLibrary
    {
        public static CrystalModeProfile Resolve(DiamondCrystalMaterialMode mode, int seed, bool variantsEnabled, float refractionCoefficient)
        {
            CrystalModeProfile profile = CreateBaseProfile(mode);
            if (variantsEnabled)
            {
                ApplyVariant(ref profile, seed);
            }

            profile.Optics = profile.Optics.WithRefractionCoefficient(refractionCoefficient);
            profile.Surface = profile.Surface.Clamp();
            return profile;
        }

        private static CrystalModeProfile CreateBaseProfile(DiamondCrystalMaterialMode mode)
        {
            switch (mode)
            {
                case DiamondCrystalMaterialMode.AbsoluteMirror:
                    return Profile(mode, "Absolute Mirror + Prism", CrystalSurfaceFamily.Mirror, new Color(0.92f, 0.97f, 1f, 1f), 0.08f, 0.05f, 0.02f, 0.98f, 0.018f, 0.95f, 1.25f, 0.02f, 0.05f, 0.22f);
                case DiamondCrystalMaterialMode.Emerald:
                    return Gem(mode, "Emerald", new Color(0.02f, 0.72f, 0.36f, 1f), 1.78f, 0.048f, 0.55f, 0.78f);
                case DiamondCrystalMaterialMode.Topaz:
                    return Gem(mode, "Topaz", new Color(1f, 0.68f, 0.22f, 1f), 1.64f, 0.043f, 0.48f, 0.82f);
                case DiamondCrystalMaterialMode.Ruby:
                    return Gem(mode, "Ruby", new Color(0.95f, 0.03f, 0.16f, 1f), 1.77f, 0.045f, 0.52f, 0.74f);
                case DiamondCrystalMaterialMode.Sapphire:
                    return Gem(mode, "Sapphire", new Color(0.06f, 0.22f, 0.95f, 1f), 1.76f, 0.044f, 0.54f, 0.78f);
                case DiamondCrystalMaterialMode.Amethyst:
                    return Gem(mode, "Amethyst", new Color(0.62f, 0.22f, 0.95f, 1f), 1.58f, 0.041f, 0.44f, 0.8f);
                case DiamondCrystalMaterialMode.Aquamarine:
                    return Gem(mode, "Aquamarine", new Color(0.2f, 0.94f, 1f, 1f), 1.57f, 0.04f, 0.42f, 0.88f);
                case DiamondCrystalMaterialMode.Garnet:
                    return Gem(mode, "Garnet", new Color(0.56f, 0.02f, 0.08f, 1f), 1.81f, 0.039f, 0.5f, 0.68f);
                case DiamondCrystalMaterialMode.FuturisticPlastic:
                    return Profile(mode, "Opal Prism Glass", CrystalSurfaceFamily.Gemstone, new Color(0.86f, 0.94f, 1f, 1f), 0.24f, 0f, 0.1f, 0.72f, 0.042f, 0.62f, 1.47f, 0.06f, 0.82f, 0.72f);
                case DiamondCrystalMaterialMode.Mercury:
                    return Metal(mode, "Liquid Mercury Mirror", new Color(0.88f, 0.96f, 1f, 1f), 0.08f, 1f, 0.02f, 0.48f);
                case DiamondCrystalMaterialMode.StainlessSteel:
                    return Metal(mode, "Brushed Steel Mirror", new Color(0.72f, 0.76f, 0.78f, 1f), 0.14f, 0.94f, 0.18f, 0.28f);
                case DiamondCrystalMaterialMode.Chrome:
                    return Metal(mode, "Chrome Facet Mirror", new Color(0.94f, 0.98f, 1f, 1f), 0.06f, 1f, 0.035f, 0.46f);
                case DiamondCrystalMaterialMode.CastIron:
                    return Metal(mode, "Blackened Iron Facets", new Color(0.2f, 0.21f, 0.21f, 1f), 0.2f, 0.88f, 0.44f, 0.12f);
                case DiamondCrystalMaterialMode.PolishedBrass:
                    return Metal(mode, "Polished Brass Prism", new Color(1f, 0.74f, 0.28f, 1f), 0.26f, 0.96f, 0.08f, 0.36f);
                default:
                    return Profile(DiamondCrystalMaterialMode.Diamond, "High-Purity Diamond", CrystalSurfaceFamily.Diamond, new Color(0.9f, 0.98f, 1f, 1f), 0.06f, 0f, 0.01f, 0.78f, 0.052f, 0.78f, 1.35f, 0.01f, 0.96f, 0.05f);
            }
        }

        private static CrystalModeProfile Gem(
            DiamondCrystalMaterialMode mode,
            string name,
            Color tint,
            float ior,
            float refraction,
            float dispersion,
            float clarity)
        {
            return Profile(mode, name, CrystalSurfaceFamily.Gemstone, tint, 0.42f, 0f, 0.12f, dispersion, refraction, 0.56f, ior, 0.1f, clarity, 0.18f);
        }

        private static CrystalModeProfile Metal(
            DiamondCrystalMaterialMode mode,
            string name,
            Color tint,
            float tintStrength,
            float metallic,
            float roughness,
            float iridescence)
        {
            return Profile(mode, name, CrystalSurfaceFamily.Metal, tint, tintStrength, metallic, roughness, 0.18f, 0.018f, 0.92f, 1.38f, 0.04f, 0.3f, iridescence);
        }

        private static CrystalModeProfile Profile(
            DiamondCrystalMaterialMode mode,
            string name,
            CrystalSurfaceFamily family,
            Color tint,
            float tintStrength,
            float metallic,
            float roughness,
            float dispersion,
            float refraction,
            float reflection,
            float ior,
            float noise,
            float clarity,
            float iridescence)
        {
            CrystalModeProfile profile = new CrystalModeProfile
            {
                Mode = mode,
                Surface = new CrystalSurfaceProfile
                {
                    ModeName = name,
                    Family = family,
                    Tint = tint,
                    TintStrength = tintStrength,
                    Metallic = metallic,
                    Roughness = roughness,
                    ScratchStrength = family == CrystalSurfaceFamily.Metal && roughness > 0.3f ? 0.18f : 0.035f,
                    Iridescence = iridescence,
                    PatternStrength = family == CrystalSurfaceFamily.Gemstone ? 0.08f : 0.025f,
                    Clarity = clarity
                },
                Optics = new CrystalOpticsProfile
                {
                    OpticalIOR = ior,
                    RefractionStrength = refraction,
                    DispersionStrength = dispersion,
                    ReflectionStrength = reflection,
                    FresnelPower = family == CrystalSurfaceFamily.Metal ? 2.4f : 3.4f,
                    InternalBrightness = family == CrystalSurfaceFamily.Metal ? 0.65f : 1.15f,
                    NoiseDistortionStrength = noise,
                    EdgeHighlight = family == CrystalSurfaceFamily.Metal ? 0.85f : 1.3f,
                    ChromaticAberrationAmount = family == CrystalSurfaceFamily.Metal ? 0.002f : 0.006f,
                    FacetContrast = family == CrystalSurfaceFamily.Plastic ? 0.9f : 1.35f,
                    InternalGlow = family == CrystalSurfaceFamily.Metal ? 0.12f : 0.48f,
                    BloomBoost = family == CrystalSurfaceFamily.Metal ? 0.25f : 0.75f,
                    DiamondLikeRefraction = family == CrystalSurfaceFamily.Diamond ? 1.45f : 0.8f,
                    SpectralDispersion = family == CrystalSurfaceFamily.Metal ? 0.2f : 1.15f,
                    HighEnergyCaustics = family == CrystalSurfaceFamily.Diamond ? 1.45f : 0.7f,
                    MultiBounceInternalReflections = reflection,
                    CinematicCrystalOptics = 1.1f,
                    PhysicallyBasedRefraction = family == CrystalSurfaceFamily.Metal ? 0.2f : 0.8f,
                    DeepVolumetricLightScattering = family == CrystalSurfaceFamily.Metal ? 0.25f : 0.9f,
                    CrystalSolidity = family == CrystalSurfaceFamily.Diamond ? 0.86f : 0.95f,
                    DirectTransmission = family == CrystalSurfaceFamily.Metal ? 0.02f : 0.08f,
                    TotalInternalReturn = family == CrystalSurfaceFamily.Metal ? 0.85f : 1.2f,
                    SpectralFireIntensity = family == CrystalSurfaceFamily.Metal ? 0.15f : 1.0f,
                    FacetDepthContrast = family == CrystalSurfaceFamily.Plastic ? 0.72f : 1.1f,
                    SampleMipBias = family == CrystalSurfaceFamily.Metal && roughness > 0.3f ? 0.6f : 0f
                }
            };

            profile.Optics = profile.Optics.Clamp();
            profile.Surface = profile.Surface.Clamp();
            return profile;
        }

        private static void ApplyVariant(ref CrystalModeProfile profile, int seed)
        {
            if (profile.Surface.Family != CrystalSurfaceFamily.Gemstone && profile.Surface.Family != CrystalSurfaceFamily.Plastic)
            {
                return;
            }

            System.Random random = new System.Random(Mathf.Max(1, seed));
            float colorShift = Range(random, -0.045f, 0.045f);
            profile.Surface.Tint = ShiftColor(profile.Surface.Tint, colorShift);
            profile.Surface.PatternStrength = Mathf.Clamp(profile.Surface.PatternStrength + Range(random, -0.025f, 0.035f), 0f, 0.18f);
            profile.Surface.Roughness = Mathf.Clamp01(profile.Surface.Roughness + Range(random, -0.035f, 0.035f));
            profile.Surface.Clarity = Mathf.Clamp01(profile.Surface.Clarity + Range(random, -0.045f, 0.045f));
            profile.Optics.InternalBrightness = Mathf.Clamp(profile.Optics.InternalBrightness + Range(random, -0.14f, 0.16f), 0.3f, 1.65f);
            profile.Optics.RefractionStrength = Mathf.Clamp(profile.Optics.RefractionStrength + Range(random, -0.004f, 0.004f), 0.012f, 0.07f);
        }

        private static float Range(System.Random random, float min, float max)
        {
            return Mathf.Lerp(min, max, (float)random.NextDouble());
        }

        private static Color ShiftColor(Color color, float amount)
        {
            Color.RGBToHSV(color, out float hue, out float saturation, out float value);
            hue = Mathf.Repeat(hue + amount, 1f);
            saturation = Mathf.Clamp01(saturation + Mathf.Abs(amount) * 0.4f);
            value = Mathf.Clamp01(value + amount * 0.2f);
            Color shifted = Color.HSVToRGB(hue, saturation, value);
            shifted.a = 1f;
            return shifted;
        }
    }
}
