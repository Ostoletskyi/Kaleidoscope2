using UnityEngine;

namespace Kaleidoscope2.DiamondFocus
{
    public enum CrystalSurfaceFamily
    {
        Mirror = 0,
        Diamond = 1,
        Gemstone = 2,
        Plastic = 3,
        Metal = 4
    }

    public struct CrystalSurfaceProfile
    {
        public string ModeName;
        public CrystalSurfaceFamily Family;
        public Color Tint;
        public float TintStrength;
        public float Metallic;
        public float Roughness;
        public float ScratchStrength;
        public float Iridescence;
        public float PatternStrength;
        public float Clarity;

        public CrystalSurfaceProfile Clamp()
        {
            CrystalSurfaceProfile result = this;
            if (string.IsNullOrEmpty(result.ModeName))
            {
                result.ModeName = "Crystal";
            }

            result.Tint.a = 1f;
            result.TintStrength = Mathf.Clamp01(result.TintStrength);
            result.Metallic = Mathf.Clamp01(result.Metallic);
            result.Roughness = Mathf.Clamp01(result.Roughness);
            result.ScratchStrength = Mathf.Clamp(result.ScratchStrength, 0f, 0.45f);
            result.Iridescence = Mathf.Clamp01(result.Iridescence);
            result.PatternStrength = Mathf.Clamp(result.PatternStrength, 0f, 0.45f);
            result.Clarity = Mathf.Clamp01(result.Clarity);
            return result;
        }
    }
}
