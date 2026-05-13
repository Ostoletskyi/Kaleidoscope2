using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.SixD
{
    public static class VolumetricIllusionShaderIds
    {
        public static readonly int VolumetricDensity = Shader.PropertyToID("_VolumetricDensity");
        public static readonly int HazeStrength = Shader.PropertyToID("_HazeStrength");
        public static readonly int CenterPull = Shader.PropertyToID("_CenterPull");
        public static readonly int MotionBreathing = Shader.PropertyToID("_MotionBreathing");
        public static readonly int TimeValue = Shader.PropertyToID("_TimeValue");
        public static readonly int TexelSize = Shader.PropertyToID("_InputTexelSize");
        public static readonly int MotionOffset = Shader.PropertyToID("_MotionOffset");
        public static readonly int FlightTime = Shader.PropertyToID("_FlightTime");
        public static readonly int MotionShake = Shader.PropertyToID("_MotionShake");
        public static readonly int ImageReanimationBlend = Shader.PropertyToID("_ImageReanimationBlend");
    }

    [DisallowMultipleComponent]
    public sealed class VolumetricIllusionModule : SixDTextureProcessorBase
    {
        public override string ModuleId
        {
            get { return "VolumetricIllusion"; }
        }

        protected override string ShaderName
        {
            get { return "Kaleidoscope2/SixDVolumetricIllusion"; }
        }

        protected override bool IsStageEnabled(SixDSettings settings)
        {
            return settings.VolumetricIllusionEnabled;
        }

        protected override void ConfigureMaterial(Material activeMaterial, KaleidoscopeState runtimeState, SixDSettings settings, Texture sourceTexture)
        {
            activeMaterial.SetFloat(VolumetricIllusionShaderIds.VolumetricDensity, settings.VolumetricDensity);
            activeMaterial.SetFloat(VolumetricIllusionShaderIds.HazeStrength, settings.HazeStrength);
            activeMaterial.SetFloat(VolumetricIllusionShaderIds.CenterPull, settings.CenterPull);
            activeMaterial.SetFloat(VolumetricIllusionShaderIds.MotionBreathing, settings.MotionBreathing);
            activeMaterial.SetFloat(VolumetricIllusionShaderIds.TimeValue, TimeValue);
            activeMaterial.SetVector(VolumetricIllusionShaderIds.TexelSize, new Vector4(1f / sourceTexture.width, 1f / sourceTexture.height, sourceTexture.width, sourceTexture.height));
            activeMaterial.SetVector(VolumetricIllusionShaderIds.MotionOffset, MotionOffset);
            activeMaterial.SetFloat(VolumetricIllusionShaderIds.FlightTime, MotionFlightTime);
            activeMaterial.SetFloat(VolumetricIllusionShaderIds.MotionShake, MotionShake);
            activeMaterial.SetFloat(VolumetricIllusionShaderIds.ImageReanimationBlend, runtimeState.ImageReanimationBlend);
        }
    }
}
