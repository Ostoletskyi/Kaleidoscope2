using System;
using UnityEngine;

namespace Kaleidoscope2.Core
{
    public enum CrystalRuntimeControlModule
    {
        PremiumCrystalShape = 0,
        PremiumOpticalMode = 1,
        CrystalDebugMode = 2,
        CrystalDebugEffect = 3
    }

    [Serializable]
    public sealed class CrystalRuntimeControlSettings
    {
        [SerializeField] private CrystalRuntimeControlModule selectedModule = CrystalRuntimeControlModule.CrystalDebugMode;

        public CrystalRuntimeControlModule SelectedModule
        {
            get { return CrystalRuntimeControlLibrary.Normalize(selectedModule); }
        }

        public string DisplayName
        {
            get { return CrystalRuntimeControlLibrary.GetDisplayName(SelectedModule); }
        }

        public void Select(CrystalRuntimeControlModule module)
        {
            selectedModule = CrystalRuntimeControlLibrary.Normalize(module);
        }
    }

    public static class CrystalRuntimeControlLibrary
    {
        private const int Count = 4;

        public static CrystalRuntimeControlModule Normalize(CrystalRuntimeControlModule module)
        {
            int index = (int)module;
            return index >= 0 && index < Count ? module : CrystalRuntimeControlModule.CrystalDebugMode;
        }

        public static string GetDisplayName(CrystalRuntimeControlModule module)
        {
            switch (Normalize(module))
            {
                case CrystalRuntimeControlModule.PremiumCrystalShape:
                    return "Shape";
                case CrystalRuntimeControlModule.PremiumOpticalMode:
                    return "Optical";
                case CrystalRuntimeControlModule.CrystalDebugEffect:
                    return "Debug Effect";
                default:
                    return "Debug Mode";
            }
        }
    }

    public sealed class CrystalRuntimeControlRouter
    {
        public void CycleSelected(DiamondFocusSettings settings, int direction)
        {
            if (settings == null || direction == 0)
            {
                return;
            }

            switch (settings.RuntimeControl.SelectedModule)
            {
                case CrystalRuntimeControlModule.PremiumCrystalShape:
                    settings.CyclePremiumCrystalShape(direction);
                    break;
                case CrystalRuntimeControlModule.PremiumOpticalMode:
                    settings.CyclePremiumCrystalOpticalMode(direction);
                    break;
                case CrystalRuntimeControlModule.CrystalDebugEffect:
                    settings.CycleCrystalDebugEffect(direction);
                    break;
                default:
                    settings.CycleDebugMode(direction);
                    break;
            }
        }
    }
}
