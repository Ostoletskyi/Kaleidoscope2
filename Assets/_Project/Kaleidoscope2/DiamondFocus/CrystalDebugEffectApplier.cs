using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus
{
    public sealed class CrystalDebugEffectApplier
    {
        private static readonly int EffectTypeId = Shader.PropertyToID("_CrystalDebugEffectType");
        private static readonly int BlendId = Shader.PropertyToID("_CrystalDebugEffectBlend");
        private static readonly int MirrorBoostId = Shader.PropertyToID("_CrystalDebugMirrorBoost");
        private static readonly int FrostId = Shader.PropertyToID("_CrystalDebugFrost");
        private static readonly int NegativeId = Shader.PropertyToID("_CrystalDebugNegative");
        private static readonly int HaloId = Shader.PropertyToID("_CrystalDebugHalo");
        private static readonly int StoneId = Shader.PropertyToID("_CrystalDebugStone");
        private static readonly int ChromaticId = Shader.PropertyToID("_CrystalDebugChromatic");
        private static readonly int GlimmerId = Shader.PropertyToID("_CrystalDebugGlimmer");
        private static readonly int RainbowId = Shader.PropertyToID("_CrystalDebugRainbow");
        private static readonly int MirageId = Shader.PropertyToID("_CrystalDebugMirage");
        private static readonly int RoughnessId = Shader.PropertyToID("_CrystalDebugRoughness");
        private static readonly int ContrastId = Shader.PropertyToID("_CrystalDebugContrast");
        private static readonly int BrightnessId = Shader.PropertyToID("_CrystalDebugBrightness");
        private static readonly int CracksId = Shader.PropertyToID("_CrystalDebugCracks");
        private static readonly int VeinsId = Shader.PropertyToID("_CrystalDebugVeins");
        private static readonly int EdgeGlowId = Shader.PropertyToID("_CrystalDebugEdgeGlow");
        private static readonly int FlareId = Shader.PropertyToID("_CrystalDebugFlare");
        private static readonly int DistortionId = Shader.PropertyToID("_CrystalDebugDistortion");
        private static readonly int SpeedId = Shader.PropertyToID("_CrystalDebugSpeed");
        private static readonly int SpectralSplitId = Shader.PropertyToID("_CrystalDebugSpectralSplit");
        private static readonly int TintId = Shader.PropertyToID("_CrystalDebugTint");
        private static readonly int AbsoluteMirrorGuardId = Shader.PropertyToID("_CrystalDebugAbsoluteMirrorGuard");

        public void Apply(Material material, CrystalDebugEffectSettings settings, bool absoluteMirror)
        {
            if (material == null)
            {
                return;
            }

            CrystalDebugEffectType type = settings != null
                ? settings.SelectedEffect
                : CrystalDebugEffectType.None;
            float blend = settings != null ? settings.BlendAmount : 1f;
            CrystalDebugEffectProfile profile = Resolve(type);

            SetFloat(material, EffectTypeId, (float)type);
            SetFloat(material, BlendId, blend);
            SetFloat(material, MirrorBoostId, profile.MirrorBoost);
            SetFloat(material, FrostId, profile.Frost);
            SetFloat(material, NegativeId, profile.Negative);
            SetFloat(material, HaloId, profile.Halo);
            SetFloat(material, StoneId, profile.Stone);
            SetFloat(material, ChromaticId, profile.Chromatic);
            SetFloat(material, GlimmerId, profile.Glimmer);
            SetFloat(material, RainbowId, profile.Rainbow);
            SetFloat(material, MirageId, profile.Mirage);
            SetFloat(material, RoughnessId, profile.Roughness);
            SetFloat(material, ContrastId, profile.Contrast);
            SetFloat(material, BrightnessId, profile.Brightness);
            SetFloat(material, CracksId, profile.Cracks);
            SetFloat(material, VeinsId, profile.Veins);
            SetFloat(material, EdgeGlowId, profile.EdgeGlow);
            SetFloat(material, FlareId, profile.Flare);
            SetFloat(material, DistortionId, profile.Distortion);
            SetFloat(material, SpeedId, profile.Speed);
            SetFloat(material, SpectralSplitId, profile.SpectralSplit);
            SetColor(material, TintId, profile.Tint);
            SetFloat(material, AbsoluteMirrorGuardId, absoluteMirror ? 1f : 0f);
        }

        private static CrystalDebugEffectProfile Resolve(CrystalDebugEffectType type)
        {
            switch (CrystalDebugEffectLibrary.Normalize(type))
            {
                case CrystalDebugEffectType.PerfectMirrorBoost:
                    return new CrystalDebugEffectProfile
                    {
                        MirrorBoost = 1f,
                        Contrast = 0.48f,
                        Brightness = 0.22f,
                        EdgeGlow = 0.25f,
                        Tint = Color.white
                    };
                case CrystalDebugEffectType.SeaFrostedBrokenBottleGlass:
                    return new CrystalDebugEffectProfile
                    {
                        Frost = 1f,
                        Roughness = 0.88f,
                        Distortion = 0.42f,
                        EdgeGlow = 0.2f,
                        Tint = new Color(0.46f, 0.86f, 0.77f, 1f)
                    };
                case CrystalDebugEffectType.Negative:
                    return new CrystalDebugEffectProfile
                    {
                        Negative = 1f,
                        Contrast = 0.28f,
                        Tint = Color.white
                    };
                case CrystalDebugEffectType.Halo:
                    return new CrystalDebugEffectProfile
                    {
                        Halo = 1f,
                        Brightness = 0.16f,
                        EdgeGlow = 1.25f,
                        Tint = new Color(0.48f, 0.84f, 1f, 1f)
                    };
                case CrystalDebugEffectType.AncientStone:
                    return new CrystalDebugEffectProfile
                    {
                        Stone = 1f,
                        Roughness = 1f,
                        Cracks = 1f,
                        Veins = 0.82f,
                        Contrast = 0.2f,
                        Tint = new Color(0.51f, 0.4f, 0.28f, 1f)
                    };
                case CrystalDebugEffectType.FacetChromaticAberration:
                    return new CrystalDebugEffectProfile
                    {
                        Chromatic = 1.25f,
                        SpectralSplit = 0.78f,
                        EdgeGlow = 0.2f,
                        Tint = Color.white
                    };
                case CrystalDebugEffectType.GlimmerLensFlare:
                    return new CrystalDebugEffectProfile
                    {
                        Glimmer = 1.25f,
                        Flare = 1.1f,
                        EdgeGlow = 0.45f,
                        Brightness = 0.12f,
                        Tint = new Color(1f, 0.92f, 0.72f, 1f)
                    };
                case CrystalDebugEffectType.RainbowPrismFire:
                    return new CrystalDebugEffectProfile
                    {
                        Rainbow = 1.35f,
                        SpectralSplit = 1.4f,
                        Chromatic = 0.55f,
                        EdgeGlow = 0.4f,
                        Tint = Color.white
                    };
                case CrystalDebugEffectType.MirageAtmosphericHeatHaze:
                    return new CrystalDebugEffectProfile
                    {
                        Mirage = 1f,
                        Distortion = 1f,
                        Speed = 1f,
                        Roughness = 0.18f,
                        Tint = new Color(1f, 0.86f, 0.64f, 1f)
                    };
                default:
                    return new CrystalDebugEffectProfile { Tint = Color.white };
            }
        }

        private static void SetFloat(Material material, int propertyId, float value)
        {
            if (material.HasProperty(propertyId))
            {
                material.SetFloat(propertyId, value);
            }
        }

        private static void SetColor(Material material, int propertyId, Color value)
        {
            if (material.HasProperty(propertyId))
            {
                material.SetColor(propertyId, value);
            }
        }

        private struct CrystalDebugEffectProfile
        {
            public float MirrorBoost;
            public float Frost;
            public float Negative;
            public float Halo;
            public float Stone;
            public float Chromatic;
            public float Glimmer;
            public float Rainbow;
            public float Mirage;
            public float Roughness;
            public float Contrast;
            public float Brightness;
            public float Cracks;
            public float Veins;
            public float EdgeGlow;
            public float Flare;
            public float Distortion;
            public float Speed;
            public float SpectralSplit;
            public Color Tint;
        }
    }
}
