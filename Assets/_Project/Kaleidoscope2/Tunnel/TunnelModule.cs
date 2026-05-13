using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Tunnel
{
    public static class TunnelShaderIds
    {
        public static readonly int DepthScale = Shader.PropertyToID("_DepthScale");
        public static readonly int CenterDarken = Shader.PropertyToID("_CenterDarken");
        public static readonly int Scroll = Shader.PropertyToID("_Scroll");
        public static readonly int Bend = Shader.PropertyToID("_Bend");
        public static readonly int TunnelHoseEnabled = Shader.PropertyToID("_TunnelHoseEnabled");
        public static readonly int TunnelBendOffset = Shader.PropertyToID("_TunnelBendOffset");
        public static readonly int TunnelFoldStrength = Shader.PropertyToID("_TunnelFoldStrength");
        public static readonly int TunnelFoldShadowStrength = Shader.PropertyToID("_TunnelFoldShadowStrength");
        public static readonly int TunnelDarknessDepth = Shader.PropertyToID("_TunnelDarknessDepth");
        public static readonly int TunnelEndLightVisibility = Shader.PropertyToID("_TunnelEndLightVisibility");
        public static readonly int TunnelDepthFade = Shader.PropertyToID("_TunnelDepthFade");
        public static readonly int TunnelHoseOpening = Shader.PropertyToID("_TunnelHoseOpening");
        public static readonly int TunnelWallCurvature = Shader.PropertyToID("_TunnelWallCurvature");
        public static readonly int TunnelShake = Shader.PropertyToID("_TunnelShake");
        public static readonly int TunnelChromaticAberrationEnabled = Shader.PropertyToID("_TunnelChromaticAberrationEnabled");
        public static readonly int TunnelChromaticAberrationStrength = Shader.PropertyToID("_TunnelChromaticAberrationStrength");
        public static readonly int ModeMotionOffset = Shader.PropertyToID("_ModeMotionOffset");
        public static readonly int ModeMotionShake = Shader.PropertyToID("_ModeMotionShake");
        public static readonly int ImageReanimationBlend = Shader.PropertyToID("_ImageReanimationBlend");
        public static readonly int FiveDEnabled = Shader.PropertyToID("_FiveDEnabled");
        public static readonly int FiveDTime = Shader.PropertyToID("_FiveDTime");
        public static readonly int FiveDShake = Shader.PropertyToID("_FiveDShake");
    }

    [DisallowMultipleComponent]
    public sealed class TunnelModule : KaleidoscopeModuleBase, IKaleidoscopeTextureProcessor
    {
        private const string DefaultShaderName = "Kaleidoscope2/Tunnel";

        public override string ModuleId
        {
            get { return "Tunnel"; }
        }

        [Header("Shader")]
        [SerializeField] private Shader tunnelShader;
        [SerializeField] private Material tunnelMaterial;
        [SerializeField, Range(0.2f, 8f)] private float depthScale = 2.2f;
        [SerializeField, Range(0f, 1f)] private float centerDarken = 0.55f;
        [SerializeField, Range(0f, 6f)] private float scrollSpeed = 0.45f;
        [SerializeField, Range(0.1f, 2f)] private float fiveDShakeDuration = 0.65f;
        [SerializeField, Range(0.1f, 2f)] private float tunnelShakeDuration = 0.55f;
        [SerializeField, Range(0f, 0.03f)] private float hoseChromaticAberrationStrength = 0.0075f;

        private Material runtimeMaterial;
        private RenderTexture outputTexture;
        private float fiveDTime;
        private float modeMotionScroll;
        private float fiveDRecoveryStart;
        private float modeMotionRecoveryStart;
        private bool internalRecoveryActive;
        private float fiveDShakeRemaining;
        private float tunnelShakeRemaining;

        public Texture OutputTexture
        {
            get { return outputTexture; }
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            return command != null
                && (command.Type == KaleidoscopeCommandType.SetTunnelEnabled
                    || command.Type == KaleidoscopeCommandType.TriggerFiveDShake
                    || command.Type == KaleidoscopeCommandType.TriggerTunnelShake
                    || command.Type == KaleidoscopeCommandType.TriggerVisualMotionShake
                    || command.Type == KaleidoscopeCommandType.StartImageReanimation);
        }

        public override void HandleCommand(KaleidoscopeCommand command)
        {
            if (command == null)
            {
                return;
            }

            if (command.Type == KaleidoscopeCommandType.TriggerFiveDShake)
            {
                fiveDShakeRemaining = Mathf.Max(0.01f, fiveDShakeDuration);
            }
            else if (command.Type == KaleidoscopeCommandType.TriggerTunnelShake)
            {
                tunnelShakeRemaining = Mathf.Max(0.01f, tunnelShakeDuration);
            }
            else if (command.Type == KaleidoscopeCommandType.TriggerVisualMotionShake)
            {
                if (command.VisualModeValue == KaleidoscopeVisualMode.Tunnel || command.VisualModeValue == KaleidoscopeVisualMode.Hose)
                {
                    tunnelShakeRemaining = Mathf.Max(0.01f, tunnelShakeDuration);
                }
                else if (command.VisualModeValue == KaleidoscopeVisualMode.FiveD)
                {
                    fiveDShakeRemaining = Mathf.Max(0.01f, fiveDShakeDuration);
                }
            }
            else if (command.Type == KaleidoscopeCommandType.StartImageReanimation)
            {
                fiveDRecoveryStart = fiveDTime;
                modeMotionRecoveryStart = modeMotionScroll;
                internalRecoveryActive = true;
            }
        }

        public override void Tick(float deltaTime)
        {
            if (State == null)
            {
                return;
            }

            deltaTime = Mathf.Max(0f, deltaTime);
            if (State.ActiveVisualMode == KaleidoscopeVisualMode.FiveD && State.FiveDSettings != null)
            {
                fiveDTime += State.FiveDSettings.FlightSpeedUnits * 0.0009f * deltaTime;
            }
            else if (State.ActiveVisualMode == KaleidoscopeVisualMode.Tunnel || State.ActiveVisualMode == KaleidoscopeVisualMode.Hose)
            {
                VisualMotionSettings motionSettings = State.GetVisualMotionSettings(State.ActiveVisualMode);
                if (motionSettings != null)
                {
                    modeMotionScroll += motionSettings.FlightSpeedUnits * 0.0009f * deltaTime;
                    if (modeMotionScroll > 10000f || modeMotionScroll < -10000f)
                    {
                        modeMotionScroll = 0f;
                    }
                }
            }

            if (fiveDShakeRemaining > 0f)
            {
                fiveDShakeRemaining = Mathf.Max(0f, fiveDShakeRemaining - deltaTime);
            }

            if (tunnelShakeRemaining > 0f)
            {
                tunnelShakeRemaining = Mathf.Max(0f, tunnelShakeRemaining - deltaTime);
            }

            if (internalRecoveryActive && State.ImageReanimationActive)
            {
                float blend = State.ImageReanimationBlend;
                fiveDTime = Mathf.Lerp(fiveDRecoveryStart, 0f, blend);
                modeMotionScroll = Mathf.Lerp(modeMotionRecoveryStart, 0f, blend);
            }
            else if (internalRecoveryActive)
            {
                fiveDTime = 0f;
                modeMotionScroll = 0f;
                internalRecoveryActive = false;
            }
        }

        public Texture Process(Texture sourceTexture, KaleidoscopeState runtimeState)
        {
            if (runtimeState == null || !runtimeState.TunnelEnabled)
            {
                return sourceTexture;
            }

            if (sourceTexture == null)
            {
                return null;
            }

            Material material = EnsureMaterial();
            if (material == null)
            {
                return sourceTexture;
            }

            EnsureOutputTexture(runtimeState);
            if (outputTexture == null)
            {
                return sourceTexture;
            }

            Vector2 bend = runtimeState.TunnelSettings != null ? runtimeState.TunnelSettings.Bend : Vector2.zero;
            TunnelSettings tunnelSettings = runtimeState.TunnelSettings;
            TunnelBendSettings bendSettings = runtimeState.TunnelBendSettings;
            TunnelBendState bendState = runtimeState.TunnelBendState;
            bool hoseMode = runtimeState.ActiveVisualMode == KaleidoscopeVisualMode.Hose;
            bool fiveDMode = runtimeState.ActiveVisualMode == KaleidoscopeVisualMode.FiveD;
            VisualMotionSettings modeMotionSettings = !fiveDMode ? runtimeState.GetVisualMotionSettings(runtimeState.ActiveVisualMode) : null;
            Vector2 modeMotionOffset = modeMotionSettings != null ? modeMotionSettings.ImageOffset : Vector2.zero;
            Vector2 hoseBend = hoseMode && bendState != null ? bendState.BendOffset : Vector2.zero;
            float hoseOpening = hoseMode && tunnelSettings != null ? tunnelSettings.HoseOpeningNormalized : 0f;
            float wallCurvature = hoseMode && tunnelSettings != null ? tunnelSettings.HoseWallCurvatureNormalized : 0f;
            float fiveDShake = fiveDShakeDuration > 0.0001f ? Mathf.Clamp01(fiveDShakeRemaining / fiveDShakeDuration) : 0f;
            float tunnelShake = tunnelShakeDuration > 0.0001f ? Mathf.Clamp01(tunnelShakeRemaining / tunnelShakeDuration) : 0f;
            bool hoseChromaticAberration = hoseMode && tunnelSettings != null && tunnelSettings.HoseChromaticAberrationEnabled;
            float bendMagnitude = Mathf.Clamp01(hoseBend.magnitude);
            float foldStrength = bendSettings != null ? bendSettings.FoldStrength * bendMagnitude : bendMagnitude;
            float foldShadowStrength = bendSettings != null ? bendSettings.FoldShadowStrength * bendMagnitude : bendMagnitude;
            float darknessDepth = bendSettings != null ? bendSettings.DarknessDepth : 0.85f;
            float endLightVisibility = bendSettings != null
                ? bendSettings.EndLightVisibility * Mathf.Clamp01(1f - bendMagnitude)
                : Mathf.Clamp01(1f - bendMagnitude);
            float depthFade = Mathf.Lerp(0.95f, 2.2f, bendMagnitude * darknessDepth);

            material.SetFloat(TunnelShaderIds.DepthScale, depthScale);
            material.SetFloat(TunnelShaderIds.CenterDarken, centerDarken);
            material.SetFloat(TunnelShaderIds.Scroll, Time.unscaledTime * scrollSpeed + modeMotionScroll);
            material.SetVector(TunnelShaderIds.Bend, bend);
            material.SetFloat(TunnelShaderIds.TunnelHoseEnabled, hoseMode ? 1f : 0f);
            material.SetFloat(TunnelShaderIds.FiveDEnabled, fiveDMode ? 1f : 0f);
            material.SetFloat(TunnelShaderIds.FiveDTime, fiveDTime);
            material.SetFloat(TunnelShaderIds.FiveDShake, fiveDShake);
            material.SetVector(TunnelShaderIds.TunnelBendOffset, hoseBend);
            material.SetFloat(TunnelShaderIds.TunnelFoldStrength, foldStrength);
            material.SetFloat(TunnelShaderIds.TunnelFoldShadowStrength, foldShadowStrength);
            material.SetFloat(TunnelShaderIds.TunnelDarknessDepth, darknessDepth);
            material.SetFloat(TunnelShaderIds.TunnelEndLightVisibility, endLightVisibility);
            material.SetFloat(TunnelShaderIds.TunnelDepthFade, depthFade);
            material.SetFloat(TunnelShaderIds.TunnelHoseOpening, hoseOpening);
            material.SetFloat(TunnelShaderIds.TunnelWallCurvature, wallCurvature);
            material.SetFloat(TunnelShaderIds.TunnelShake, tunnelShake);
            material.SetFloat(TunnelShaderIds.TunnelChromaticAberrationEnabled, hoseChromaticAberration ? 1f : 0f);
            material.SetFloat(TunnelShaderIds.TunnelChromaticAberrationStrength, hoseChromaticAberrationStrength);
            material.SetVector(TunnelShaderIds.ModeMotionOffset, modeMotionOffset);
            material.SetFloat(TunnelShaderIds.ModeMotionShake, tunnelShake);
            material.SetFloat(TunnelShaderIds.ImageReanimationBlend, runtimeState.ImageReanimationBlend);

            Graphics.Blit(sourceTexture, outputTexture, material);
            return outputTexture;
        }

        public override void Validate()
        {
            // No scene references required. Shader is resolved at runtime.
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            bool enabled = State != null && State.TunnelEnabled;
            Vector2 bend = State != null && State.TunnelSettings != null ? State.TunnelSettings.Bend : Vector2.zero;
            Vector2 hoseBend = State != null && State.TunnelBendState != null ? State.TunnelBendState.BendOffset : Vector2.zero;
            float opening = State != null && State.TunnelSettings != null ? State.TunnelSettings.HoseOpeningUnits : 0f;
            float curvature = State != null && State.TunnelSettings != null ? State.TunnelSettings.HoseWallCurvatureUnits : 0f;
            bool chromaticAberration = State != null && State.TunnelSettings != null && State.TunnelSettings.HoseChromaticAberrationEnabled;
            float flightSpeed = State != null && State.FiveDSettings != null ? State.FiveDSettings.FlightSpeedUnits : 0f;
            VisualMotionSettings modeMotion = State != null ? State.GetVisualMotionSettings(State.ActiveVisualMode) : null;
            float modeFlightSpeed = modeMotion != null ? modeMotion.FlightSpeedUnits : 0f;
            string mode = "3D Tunnel";
            if (State != null && State.ActiveVisualMode == KaleidoscopeVisualMode.Hose)
            {
                mode = "4D Hose";
            }
            else if (State != null && State.ActiveVisualMode == KaleidoscopeVisualMode.FiveD)
            {
                mode = "5D Mobius";
            }

            return CreateStatus(enabled ? mode + ". 3D bend " + bend.ToString("0.00") + ", hose bend " + hoseBend.ToString("0.00") + ", G " + opening.ToString("0") + ", Shch " + curvature.ToString("0") + ", CA " + (chromaticAberration ? "on" : "off") + ", flight " + flightSpeed.ToString("0") + ", mode flight " + modeFlightSpeed.ToString("0") + "." : "Tunnel disabled");
        }

        private void OnDestroy()
        {
            ReleaseOutputTexture();

            if (runtimeMaterial != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(runtimeMaterial);
                }
                else
                {
                    DestroyImmediate(runtimeMaterial);
                }
            }
        }

        private Material EnsureMaterial()
        {
            if (tunnelMaterial != null)
            {
                return tunnelMaterial;
            }

            if (runtimeMaterial != null)
            {
                return runtimeMaterial;
            }

            Shader shader = tunnelShader != null ? tunnelShader : Shader.Find(DefaultShaderName);
            if (shader == null)
            {
                ReportMissingReference("TunnelShader (" + DefaultShaderName + ")");
                return null;
            }

            runtimeMaterial = new Material(shader)
            {
                name = "Kaleidoscope2_Tunnel_RuntimeMaterial",
                hideFlags = HideFlags.HideAndDontSave
            };

            return runtimeMaterial;
        }

        private void EnsureOutputTexture(KaleidoscopeState runtimeState)
        {
            CameraSettings settings = runtimeState.CameraSettings;
            int width = settings != null ? settings.RenderWidth : 1920;
            int height = settings != null ? settings.RenderHeight : 1080;

            if (outputTexture != null && outputTexture.width == width && outputTexture.height == height)
            {
                return;
            }

            ReleaseOutputTexture();

            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 0)
            {
                msaaSamples = 1,
                sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear,
                useMipMap = false,
                autoGenerateMips = false
            };

            outputTexture = new RenderTexture(descriptor)
            {
                name = "Kaleidoscope2_Tunnel_Output",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            outputTexture.Create();
        }

        private void ReleaseOutputTexture()
        {
            if (outputTexture == null)
            {
                return;
            }

            if (outputTexture.IsCreated())
            {
                outputTexture.Release();
            }

            if (Application.isPlaying)
            {
                Destroy(outputTexture);
            }
            else
            {
                DestroyImmediate(outputTexture);
            }

            outputTexture = null;
        }
    }
}
