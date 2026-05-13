using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.SixD
{
    public static class OpticalLookShaderIds
    {
        public static readonly int FocusStrength = Shader.PropertyToID("_FocusStrength");
        public static readonly int FocusFalloff = Shader.PropertyToID("_FocusFalloff");
        public static readonly int ChromaticAmount = Shader.PropertyToID("_ChromaticAmount");
        public static readonly int DistortionStrength = Shader.PropertyToID("_DistortionStrength");
        public static readonly int MotionBreathing = Shader.PropertyToID("_MotionBreathing");
        public static readonly int TimeValue = Shader.PropertyToID("_TimeValue");
        public static readonly int TexelSize = Shader.PropertyToID("_InputTexelSize");
        public static readonly int MotionOffset = Shader.PropertyToID("_MotionOffset");
        public static readonly int FlightTime = Shader.PropertyToID("_FlightTime");
        public static readonly int MotionShake = Shader.PropertyToID("_MotionShake");
        public static readonly int ImageReanimationBlend = Shader.PropertyToID("_ImageReanimationBlend");
    }

    [DisallowMultipleComponent]
    public sealed class OpticalLookModule : SixDTextureProcessorBase
    {
        public override string ModuleId
        {
            get { return "OpticalLook"; }
        }

        protected override string ShaderName
        {
            get { return "Kaleidoscope2/SixDOpticalLook"; }
        }

        protected override bool IsStageEnabled(SixDSettings settings)
        {
            return settings.OpticalLookEnabled;
        }

        protected override void ConfigureMaterial(Material activeMaterial, KaleidoscopeState runtimeState, SixDSettings settings, Texture sourceTexture)
        {
            activeMaterial.SetFloat(OpticalLookShaderIds.FocusStrength, settings.FocusStrength);
            activeMaterial.SetFloat(OpticalLookShaderIds.FocusFalloff, settings.FocusFalloff);
            activeMaterial.SetFloat(OpticalLookShaderIds.ChromaticAmount, settings.ChromaticAmount);
            activeMaterial.SetFloat(OpticalLookShaderIds.DistortionStrength, settings.DistortionStrength);
            activeMaterial.SetFloat(OpticalLookShaderIds.MotionBreathing, settings.MotionBreathing);
            activeMaterial.SetFloat(OpticalLookShaderIds.TimeValue, TimeValue);
            activeMaterial.SetVector(OpticalLookShaderIds.TexelSize, new Vector4(1f / sourceTexture.width, 1f / sourceTexture.height, sourceTexture.width, sourceTexture.height));
            activeMaterial.SetVector(OpticalLookShaderIds.MotionOffset, MotionOffset);
            activeMaterial.SetFloat(OpticalLookShaderIds.FlightTime, MotionFlightTime);
            activeMaterial.SetFloat(OpticalLookShaderIds.MotionShake, MotionShake);
            activeMaterial.SetFloat(OpticalLookShaderIds.ImageReanimationBlend, runtimeState.ImageReanimationBlend);
        }
    }
}
