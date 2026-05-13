using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.SixD
{
    public static class DepthWarpShaderIds
    {
        public static readonly int DepthStrength = Shader.PropertyToID("_DepthStrength");
        public static readonly int ParallaxScale = Shader.PropertyToID("_ParallaxScale");
        public static readonly int CenterPull = Shader.PropertyToID("_CenterPull");
        public static readonly int OpticalCompression = Shader.PropertyToID("_OpticalCompression");
        public static readonly int MotionBreathing = Shader.PropertyToID("_MotionBreathing");
        public static readonly int TimeValue = Shader.PropertyToID("_TimeValue");
        public static readonly int TexelSize = Shader.PropertyToID("_InputTexelSize");
        public static readonly int MotionOffset = Shader.PropertyToID("_MotionOffset");
        public static readonly int FlightTime = Shader.PropertyToID("_FlightTime");
        public static readonly int MotionShake = Shader.PropertyToID("_MotionShake");
        public static readonly int ImageReanimationBlend = Shader.PropertyToID("_ImageReanimationBlend");
    }

    [DisallowMultipleComponent]
    public sealed class DepthWarpModule : SixDTextureProcessorBase
    {
        public override string ModuleId
        {
            get { return "DepthWarp"; }
        }

        protected override string ShaderName
        {
            get { return "Kaleidoscope2/SixDDepthWarp"; }
        }

        protected override bool IsStageEnabled(SixDSettings settings)
        {
            return settings.DepthWarpEnabled;
        }

        protected override void ConfigureMaterial(Material activeMaterial, KaleidoscopeState runtimeState, SixDSettings settings, Texture sourceTexture)
        {
            activeMaterial.SetFloat(DepthWarpShaderIds.DepthStrength, settings.DepthStrength);
            activeMaterial.SetFloat(DepthWarpShaderIds.ParallaxScale, settings.ParallaxScale);
            activeMaterial.SetFloat(DepthWarpShaderIds.CenterPull, settings.CenterPull);
            activeMaterial.SetFloat(DepthWarpShaderIds.OpticalCompression, settings.OpticalCompression);
            activeMaterial.SetFloat(DepthWarpShaderIds.MotionBreathing, settings.MotionBreathing);
            activeMaterial.SetFloat(DepthWarpShaderIds.TimeValue, TimeValue);
            activeMaterial.SetVector(DepthWarpShaderIds.TexelSize, new Vector4(1f / sourceTexture.width, 1f / sourceTexture.height, sourceTexture.width, sourceTexture.height));
            activeMaterial.SetVector(DepthWarpShaderIds.MotionOffset, MotionOffset);
            activeMaterial.SetFloat(DepthWarpShaderIds.FlightTime, MotionFlightTime);
            activeMaterial.SetFloat(DepthWarpShaderIds.MotionShake, MotionShake);
            activeMaterial.SetFloat(DepthWarpShaderIds.ImageReanimationBlend, runtimeState.ImageReanimationBlend);
        }
    }
}
