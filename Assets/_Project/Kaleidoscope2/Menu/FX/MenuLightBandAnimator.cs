using UnityEngine;
using UnityEngine.Rendering;

namespace Kaleidoscope2.Menu.FX
{
    [DisallowMultipleComponent]
    public sealed class MenuLightBandAnimator : MonoBehaviour
    {
        private Material material;

        internal void Initialize(Material runtimeMaterial)
        {
            material = runtimeMaterial;
        }

        internal void ApplySettings(MenuAtmosphereFXController settings)
        {
            if (material == null || settings == null)
            {
                return;
            }

            float softness = settings.LightBandSoftness;
            float narrowWidth = Mathf.Clamp(softness * 0.42f, 0.018f, 0.055f);
            float wideWidth = Mathf.Clamp(softness * 1.65f, 0.11f, 0.32f);
            float wideSoftness = Mathf.Clamp(softness * 1.8f, 0.035f, 0.52f);

            material.SetColor(MenuAtmosphereShaderIds.NarrowBandColor, settings.CoolTint);
            material.SetColor(MenuAtmosphereShaderIds.WideBandColor, settings.WarmTint);
            material.SetVector(
                MenuAtmosphereShaderIds.NarrowBandParams,
                new Vector4(settings.NarrowBandSpeed, settings.NarrowBandOpacity, softness, narrowWidth));
            material.SetVector(
                MenuAtmosphereShaderIds.WideBandParams,
                new Vector4(settings.SoftWideBandSpeed, settings.SoftWideBandOpacity, wideSoftness, wideWidth));
            material.SetVector(
                MenuAtmosphereShaderIds.BandDirection,
                new Vector4(settings.NarrowBandDirection.x, settings.NarrowBandDirection.y, settings.SoftWideBandDirection.x, settings.SoftWideBandDirection.y));
            material.SetVector(
                MenuAtmosphereShaderIds.AtmosphereParams,
                new Vector4(settings.CausticAmount, settings.NoiseAmount, settings.BreathingAmount, settings.AtmosphereEnabled ? 1f : 0f));
            material.SetVector(
                MenuAtmosphereShaderIds.CausticParams,
                new Vector4(settings.Drift.x, settings.Drift.y, 0f, 0f));

            material.SetInt(MenuAtmosphereShaderIds.SrcBlend, (int)BlendMode.SrcAlpha);
            material.SetInt(
                MenuAtmosphereShaderIds.DstBlend,
                settings.UsesAdditiveBlend ? (int)BlendMode.One : (int)BlendMode.OneMinusSrcAlpha);
        }

        private void Update()
        {
            if (material == null)
            {
                return;
            }

            material.SetFloat(MenuAtmosphereShaderIds.MenuTime, Time.unscaledTime);
        }
    }
}
