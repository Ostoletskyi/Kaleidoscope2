using UnityEngine;

namespace Kaleidoscope2.DiamondFocus
{
    public struct CrystalOpticsProfile
    {
        public float OpticalIOR;
        public float RefractionStrength;
        public float DispersionStrength;
        public float ReflectionStrength;
        public float FresnelPower;
        public float InternalBrightness;
        public float NoiseDistortionStrength;
        public float EdgeHighlight;
        public float ChromaticAberrationAmount;
        public float FacetContrast;
        public float InternalGlow;
        public float BloomBoost;
        public float DiamondLikeRefraction;
        public float SpectralDispersion;
        public float HighEnergyCaustics;
        public float MultiBounceInternalReflections;
        public float CinematicCrystalOptics;
        public float PhysicallyBasedRefraction;
        public float DeepVolumetricLightScattering;
        public float CrystalSolidity;
        public float DirectTransmission;
        public float TotalInternalReturn;
        public float SpectralFireIntensity;
        public float FacetDepthContrast;
        public float SampleMipBias;

        public CrystalOpticsProfile WithRefractionCoefficient(float coefficient)
        {
            float normalized = Mathf.Clamp01(coefficient / 10f);
            CrystalOpticsProfile resolved = this;
            resolved.OpticalIOR = Mathf.Clamp(OpticalIOR + normalized * 0.34f, 1.01f, 2.9f);
            resolved.RefractionStrength = Mathf.Clamp(RefractionStrength * Mathf.Lerp(0.32f, 1f, normalized), 0f, 0.085f);
            resolved.DispersionStrength = Mathf.Clamp(DispersionStrength * Mathf.Lerp(0.35f, 1f, normalized), 0f, 1.55f);
            resolved.PhysicallyBasedRefraction = Mathf.Clamp(PhysicallyBasedRefraction * Mathf.Lerp(0.45f, 1f, normalized), 0f, 1.65f);
            return resolved.Clamp();
        }

        public CrystalOpticsProfile Clamp()
        {
            CrystalOpticsProfile result = this;
            result.OpticalIOR = Mathf.Clamp(result.OpticalIOR, 1.01f, 2.9f);
            result.RefractionStrength = Mathf.Clamp(result.RefractionStrength, 0f, 0.085f);
            result.DispersionStrength = Mathf.Clamp(result.DispersionStrength, 0f, 1.55f);
            result.ReflectionStrength = Mathf.Clamp01(result.ReflectionStrength);
            result.FresnelPower = Mathf.Clamp(result.FresnelPower, 0.6f, 9f);
            result.InternalBrightness = Mathf.Clamp(result.InternalBrightness, 0f, 3f);
            result.NoiseDistortionStrength = Mathf.Clamp(result.NoiseDistortionStrength, 0f, 0.35f);
            result.EdgeHighlight = Mathf.Clamp(result.EdgeHighlight, 0f, 2.5f);
            result.ChromaticAberrationAmount = Mathf.Clamp(result.ChromaticAberrationAmount, 0f, 0.02f);
            result.FacetContrast = Mathf.Clamp(result.FacetContrast, 0f, 2.4f);
            result.InternalGlow = Mathf.Clamp(result.InternalGlow, 0f, 1.8f);
            result.BloomBoost = Mathf.Clamp(result.BloomBoost, 0f, 3f);
            result.DiamondLikeRefraction = Mathf.Clamp(result.DiamondLikeRefraction, 0f, 1.9f);
            result.SpectralDispersion = Mathf.Clamp(result.SpectralDispersion, 0f, 2.4f);
            result.HighEnergyCaustics = Mathf.Clamp(result.HighEnergyCaustics, 0f, 2.5f);
            result.MultiBounceInternalReflections = Mathf.Clamp(result.MultiBounceInternalReflections, 0f, 2.5f);
            result.CinematicCrystalOptics = Mathf.Clamp(result.CinematicCrystalOptics, 0f, 2f);
            result.PhysicallyBasedRefraction = Mathf.Clamp(result.PhysicallyBasedRefraction, 0f, 1.65f);
            result.DeepVolumetricLightScattering = Mathf.Clamp(result.DeepVolumetricLightScattering, 0f, 2.2f);
            result.CrystalSolidity = Mathf.Clamp01(result.CrystalSolidity);
            result.DirectTransmission = Mathf.Clamp01(result.DirectTransmission);
            result.TotalInternalReturn = Mathf.Clamp(result.TotalInternalReturn, 0f, 2.5f);
            result.SpectralFireIntensity = Mathf.Clamp(result.SpectralFireIntensity, 0f, 2.4f);
            result.FacetDepthContrast = Mathf.Clamp(result.FacetDepthContrast, 0f, 2.2f);
            result.SampleMipBias = Mathf.Clamp(result.SampleMipBias, -1f, 2f);
            return result;
        }
    }
}
