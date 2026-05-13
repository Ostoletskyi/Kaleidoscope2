using Kaleidoscope2.Core;

namespace Kaleidoscope2.DiamondFocus
{
    public sealed class DiamondMaterialModeController
    {
        public void NextMode(DiamondFocusSettings settings)
        {
            if (settings != null)
            {
                settings.CycleMaterialMode(1);
            }
        }

        public void SetMode(DiamondFocusSettings settings, DiamondCrystalMaterialMode mode)
        {
            if (settings != null)
            {
                settings.SetMaterialMode(mode);
            }
        }

        public string GetModeLabel(DiamondFocusSettings settings)
        {
            if (settings == null)
            {
                return "None";
            }

            return settings.MaterialMode == DiamondCrystalMaterialMode.GeneratedMaterial
                ? settings.MaterialModeLabel + " / " + settings.GeneratedMaterialLabel
                : settings.MaterialModeLabel;
        }
    }
}
