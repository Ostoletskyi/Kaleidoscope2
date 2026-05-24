namespace Kaleidoscope2.Core
{
    public enum PremiumCrystalOpticalMode
    {
        HighPurityDiamond = 0,
        PrismDispersion = 1,
        MirrorFacets = 2,
        InternalReflection = 3,
        AbsoluteMirror = 4,
        AlienArtifactExperimental = 5
    }

    public static class PremiumCrystalOpticalModeLibrary
    {
        public const int Count = 6;

        public static PremiumCrystalOpticalMode Normalize(PremiumCrystalOpticalMode value)
        {
            int index = (int)value;
            return index >= 0 && index < Count ? value : PremiumCrystalOpticalMode.HighPurityDiamond;
        }

        public static PremiumCrystalOpticalMode Cycle(PremiumCrystalOpticalMode value, int direction)
        {
            int step = direction > 0 ? 1 : direction < 0 ? -1 : 0;
            int next = ((int)Normalize(value) + step) % Count;
            if (next < 0)
            {
                next += Count;
            }

            return (PremiumCrystalOpticalMode)next;
        }

        public static string GetLabel(PremiumCrystalOpticalMode value)
        {
            switch (Normalize(value))
            {
                case PremiumCrystalOpticalMode.PrismDispersion:
                    return "Prism Dispersion";
                case PremiumCrystalOpticalMode.MirrorFacets:
                    return "Mirror Facets";
                case PremiumCrystalOpticalMode.InternalReflection:
                    return "Internal Reflection";
                case PremiumCrystalOpticalMode.AbsoluteMirror:
                    return "Absolute Mirror";
                case PremiumCrystalOpticalMode.AlienArtifactExperimental:
                    return "Alien Artifact / Experimental";
                default:
                    return "High-Purity Diamond";
            }
        }

        public static string GetTooltip(PremiumCrystalOpticalMode value)
        {
            return Normalize(value) == PremiumCrystalOpticalMode.AbsoluteMirror
                ? "Solid reflective crystal. Transparency is disabled."
                : GetLabel(value) + " optical profile.";
        }
    }

    internal sealed class PremiumCrystalModeApplier
    {
        public void Apply(DiamondFocusSettings settings, PremiumCrystalOpticalMode requestedMode)
        {
            if (settings == null)
            {
                return;
            }

            PremiumCrystalOpticalMode mode = PremiumCrystalOpticalModeLibrary.Normalize(requestedMode);
            if (mode != PremiumCrystalOpticalMode.AlienArtifactExperimental
                && settings.ActiveExperimentalCrystalPreset != CrystalExperimentPresetType.Normal)
            {
                settings.RestorePreviousCrystalPreset();
            }

            if (mode == PremiumCrystalOpticalMode.AlienArtifactExperimental)
            {
                settings.ApplyExperimentalCrystalPreset(CrystalExperimentPresetType.AlienArtifactCore);
                settings.SetActivePremiumCrystalOpticalMode(mode);
                return;
            }

            DiamondCrystalDebugMode preservedDebugMode = settings.DebugMode;
            settings.ResetPremiumCrystalOpticalControls();
            settings.SetDebugMode(preservedDebugMode);
            settings.SetEnabled(true);
            settings.SetCrystalSimulationMode(CrystalRenderMode.RealMesh3D);
            settings.SetActivePremiumCrystalOpticalMode(mode);

            switch (mode)
            {
                case PremiumCrystalOpticalMode.PrismDispersion:
                    settings.SetMaterialMode(DiamondCrystalMaterialMode.Diamond);
                    settings.SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.PrismDispersion, 4.25f);
                    settings.SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.ChromaticAberration, 1.5f);
                    settings.SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.SpectralSplit, 3.3f);
                    settings.SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.RefractionStrength, 2.8f);
                    break;

                case PremiumCrystalOpticalMode.MirrorFacets:
                    settings.SetMaterialMode(DiamondCrystalMaterialMode.Chrome);
                    settings.SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.DirectTransparency, 0.02f);
                    settings.SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.ReflectionStrength, 4.2f);
                    settings.SetPremiumCrystalEffectEnabled(PremiumCrystalEffectToggle.HiddenReflectionBackground, true);
                    settings.SetPremiumCrystalEffectEnabled(PremiumCrystalEffectToggle.MirrorFacets, true);
                    break;

                case PremiumCrystalOpticalMode.InternalReflection:
                    settings.SetMaterialMode(DiamondCrystalMaterialMode.Diamond);
                    settings.SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.InternalReflections, 5f);
                    settings.SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.CrystalDepth, 2.5f);
                    settings.SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.RefractionStrength, 2.35f);
                    break;

                case PremiumCrystalOpticalMode.AbsoluteMirror:
                    settings.SetMaterialMode(DiamondCrystalMaterialMode.AbsoluteMirror);
                    settings.SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.DirectTransparency, 0f);
                    settings.SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.RefractionStrength, 0f);
                    settings.SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.ReflectionStrength, 5f);
                    settings.SetPremiumCrystalOptic(PremiumCrystalOpticsParameter.InternalReflections, 5f);
                    settings.SetPremiumCrystalEffectEnabled(PremiumCrystalEffectToggle.HiddenReflectionBackground, true);
                    settings.SetPremiumCrystalEffectEnabled(PremiumCrystalEffectToggle.MirrorFacets, true);
                    settings.SetPremiumCrystalEffectEnabled(PremiumCrystalEffectToggle.InternalReflections, true);
                    settings.SetPremiumCrystalEffectEnabled(PremiumCrystalEffectToggle.RefractionDistortion, false);
                    settings.SetPremiumCrystalEffectEnabled(PremiumCrystalEffectToggle.FacetHighlights, true);
                    break;

                default:
                    settings.SetMaterialMode(DiamondCrystalMaterialMode.Diamond);
                    break;
            }
        }
    }
}
