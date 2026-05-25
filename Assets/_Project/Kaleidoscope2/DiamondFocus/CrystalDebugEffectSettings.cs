using System;
using UnityEngine;

namespace Kaleidoscope2.Core
{
    public enum CrystalDebugEffectType
    {
        None = 0,
        PerfectMirrorBoost = 1,
        SeaFrostedBrokenBottleGlass = 2,
        Negative = 3,
        Halo = 4,
        AncientStone = 5,
        FacetChromaticAberration = 6,
        GlimmerLensFlare = 7,
        RainbowPrismFire = 8,
        MirageAtmosphericHeatHaze = 9
    }

    [Serializable]
    public sealed class CrystalDebugEffectSettings
    {
        [SerializeField] private CrystalDebugEffectType selectedEffect = CrystalDebugEffectType.None;
        [SerializeField, Range(0f, 1f)] private float blendAmount = 1f;

        public CrystalDebugEffectType SelectedEffect
        {
            get { return CrystalDebugEffectLibrary.Normalize(selectedEffect); }
        }

        public float BlendAmount
        {
            get { return Mathf.Clamp01(blendAmount); }
        }

        public string DisplayName
        {
            get { return CrystalDebugEffectLibrary.GetDisplayName(SelectedEffect); }
        }

        public void SetEffect(CrystalDebugEffectType value)
        {
            selectedEffect = CrystalDebugEffectLibrary.Normalize(value);
        }

        public void CycleEffect(int direction)
        {
            selectedEffect = CrystalDebugEffectLibrary.Cycle(selectedEffect, direction);
        }

        public void SetBlendAmount(float value)
        {
            blendAmount = Mathf.Clamp01(value);
        }

        public void CopyFrom(CrystalDebugEffectSettings source)
        {
            if (source == null)
            {
                selectedEffect = CrystalDebugEffectType.None;
                blendAmount = 1f;
                return;
            }

            selectedEffect = source.SelectedEffect;
            blendAmount = source.BlendAmount;
        }
    }

    public static class CrystalDebugEffectLibrary
    {
        public const int Count = 10;

        public static CrystalDebugEffectType Normalize(CrystalDebugEffectType value)
        {
            int index = (int)value;
            return index >= 0 && index < Count ? value : CrystalDebugEffectType.None;
        }

        public static CrystalDebugEffectType Cycle(CrystalDebugEffectType value, int direction)
        {
            int step = direction > 0 ? 1 : direction < 0 ? -1 : 0;
            int next = ((int)Normalize(value) + step) % Count;
            if (next < 0)
            {
                next += Count;
            }

            return (CrystalDebugEffectType)next;
        }

        public static string GetDisplayName(CrystalDebugEffectType value)
        {
            switch (Normalize(value))
            {
                case CrystalDebugEffectType.PerfectMirrorBoost:
                    return "Perfect Mirror Boost";
                case CrystalDebugEffectType.SeaFrostedBrokenBottleGlass:
                    return "Sea-Frosted Broken Bottle Glass";
                case CrystalDebugEffectType.Negative:
                    return "Negative";
                case CrystalDebugEffectType.Halo:
                    return "Halo";
                case CrystalDebugEffectType.AncientStone:
                    return "Ancient Stone";
                case CrystalDebugEffectType.FacetChromaticAberration:
                    return "Facet Chromatic Aberration";
                case CrystalDebugEffectType.GlimmerLensFlare:
                    return "Glimmer + Lens Flare";
                case CrystalDebugEffectType.RainbowPrismFire:
                    return "Rainbow / Prism Fire";
                case CrystalDebugEffectType.MirageAtmosphericHeatHaze:
                    return "Mirage / Atmospheric Heat Haze";
                default:
                    return "None";
            }
        }
    }
}
