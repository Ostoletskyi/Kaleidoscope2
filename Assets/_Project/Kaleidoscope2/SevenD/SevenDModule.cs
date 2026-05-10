using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.SevenD
{
    public static class SevenDShaderIds
    {
        public static readonly int Strategy = Shader.PropertyToID("_Strategy");
        public static readonly int EffectStrength = Shader.PropertyToID("_EffectStrength");
        public static readonly int PatternScale = Shader.PropertyToID("_PatternScale");
        public static readonly int MotionSpeed = Shader.PropertyToID("_MotionSpeed");
        public static readonly int SourceBlend = Shader.PropertyToID("_SourceBlend");
        public static readonly int TimeValue = Shader.PropertyToID("_TimeValue");
        public static readonly int TexelSize = Shader.PropertyToID("_InputTexelSize");
        public static readonly int MotionOffset = Shader.PropertyToID("_MotionOffset");
        public static readonly int FlightTime = Shader.PropertyToID("_FlightTime");
        public static readonly int MotionShake = Shader.PropertyToID("_MotionShake");
    }

    [DisallowMultipleComponent]
    public sealed class SevenDModule : KaleidoscopeModuleBase, IKaleidoscopeTextureProcessor
    {
        private const string DefaultShaderName = "Kaleidoscope2/SevenD";

        [Header("Shader")]
        [SerializeField] private Shader sevenDShader;
        [SerializeField] private Material sevenDMaterial;
        [SerializeField, Range(0.5f, 1f)] private float renderScale = 1f;
        [SerializeField, Range(0.1f, 2f)] private float motionShakeDuration = 0.55f;

        private Material runtimeMaterial;
        private RenderTexture outputTexture;
        private float time;
        private float flightTime;
        private float motionShakeRemaining;

        public override string ModuleId
        {
            get { return "SevenD"; }
        }

        public Texture OutputTexture
        {
            get { return outputTexture; }
        }

        public override void Tick(float deltaTime)
        {
            SevenDSettings settings = State != null ? State.SevenDSettings : null;
            float speed = settings != null ? settings.MotionSpeed : 1f;
            deltaTime = Mathf.Max(0f, deltaTime);
            time += deltaTime * speed;

            if (time > 10000f)
            {
                time = 0f;
            }

            if (State != null && State.ActiveVisualMode == KaleidoscopeVisualMode.SevenD)
            {
                VisualMotionSettings motionSettings = State.GetVisualMotionSettings(KaleidoscopeVisualMode.SevenD);
                if (motionSettings != null)
                {
                    flightTime += motionSettings.FlightSpeedUnits * 0.0009f * deltaTime;
                    if (flightTime > 10000f || flightTime < -10000f)
                    {
                        flightTime = 0f;
                    }
                }
            }

            if (motionShakeRemaining > 0f)
            {
                motionShakeRemaining = Mathf.Max(0f, motionShakeRemaining - deltaTime);
            }
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            return command != null && command.Type == KaleidoscopeCommandType.TriggerVisualMotionShake;
        }

        public override void HandleCommand(KaleidoscopeCommand command)
        {
            if (command != null
                && command.Type == KaleidoscopeCommandType.TriggerVisualMotionShake
                && command.VisualModeValue == KaleidoscopeVisualMode.SevenD)
            {
                motionShakeRemaining = Mathf.Max(0.01f, motionShakeDuration);
            }
        }

        public Texture Process(Texture sourceTexture, KaleidoscopeState runtimeState)
        {
            if (sourceTexture == null || runtimeState == null || runtimeState.ActiveVisualMode != KaleidoscopeVisualMode.SevenD)
            {
                return sourceTexture;
            }

            SevenDSettings settings = runtimeState.SevenDSettings;
            if (settings == null)
            {
                return sourceTexture;
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

            material.SetFloat(SevenDShaderIds.Strategy, settings.StrategyIndex);
            material.SetFloat(SevenDShaderIds.EffectStrength, settings.EffectStrength);
            material.SetFloat(SevenDShaderIds.PatternScale, settings.PatternScale);
            material.SetFloat(SevenDShaderIds.MotionSpeed, settings.MotionSpeed);
            material.SetFloat(SevenDShaderIds.SourceBlend, settings.SourceBlend);
            material.SetFloat(SevenDShaderIds.TimeValue, time);
            material.SetVector(SevenDShaderIds.TexelSize, new Vector4(1f / sourceTexture.width, 1f / sourceTexture.height, sourceTexture.width, sourceTexture.height));
            VisualMotionSettings motionSettings = runtimeState.GetVisualMotionSettings(KaleidoscopeVisualMode.SevenD);
            Vector2 motionOffset = motionSettings != null ? motionSettings.ImageOffset : Vector2.zero;
            float motionShake = motionShakeRemaining > 0f && motionShakeDuration > 0.0001f
                ? Mathf.Pow(Mathf.Clamp01(motionShakeRemaining / motionShakeDuration), 2f)
                : 0f;
            material.SetVector(SevenDShaderIds.MotionOffset, motionOffset);
            material.SetFloat(SevenDShaderIds.FlightTime, flightTime);
            material.SetFloat(SevenDShaderIds.MotionShake, motionShake);

            Graphics.Blit(sourceTexture, outputTexture, material);
            return outputTexture;
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            if (State == null || State.ActiveVisualMode != KaleidoscopeVisualMode.SevenD)
            {
                return CreateStatus("Pass-through outside 7D.");
            }

            SevenDSettings settings = State.SevenDSettings;
            string strategy = settings != null ? settings.StrategyLabel : "Unknown";
            VisualMotionSettings motionSettings = State.GetVisualMotionSettings(KaleidoscopeVisualMode.SevenD);
            float modeFlightSpeed = motionSettings != null ? motionSettings.FlightSpeedUnits : 0f;
            return CreateStatus("7D strategy: " + strategy + ", flight " + modeFlightSpeed.ToString("0") + ".");
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
            if (sevenDMaterial != null)
            {
                return sevenDMaterial;
            }

            if (runtimeMaterial != null)
            {
                return runtimeMaterial;
            }

            Shader shader = sevenDShader != null ? sevenDShader : Shader.Find(DefaultShaderName);
            if (shader == null)
            {
                ReportMissingReference("Shader (" + DefaultShaderName + ")");
                return null;
            }

            runtimeMaterial = new Material(shader)
            {
                name = "Kaleidoscope2_SevenD_RuntimeMaterial",
                hideFlags = HideFlags.HideAndDontSave
            };

            return runtimeMaterial;
        }

        private void EnsureOutputTexture(KaleidoscopeState runtimeState)
        {
            CameraSettings settings = runtimeState.CameraSettings;
            int baseWidth = settings != null ? settings.RenderWidth : 1920;
            int baseHeight = settings != null ? settings.RenderHeight : 1080;
            float scale = Mathf.Clamp(renderScale, 0.5f, 1f);
            int width = Mathf.Max(1, Mathf.RoundToInt(baseWidth * scale));
            int height = Mathf.Max(1, Mathf.RoundToInt(baseHeight * scale));

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
                name = "Kaleidoscope2_SevenD_Output",
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
