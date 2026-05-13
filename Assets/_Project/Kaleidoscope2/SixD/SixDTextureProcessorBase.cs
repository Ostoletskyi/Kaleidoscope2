using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.SixD
{
    public abstract class SixDTextureProcessorBase : KaleidoscopeModuleBase, IKaleidoscopeTextureProcessor
    {
        [Header("6D Module")]
        [SerializeField] private bool moduleEnabled = true;
        [SerializeField] private Shader shader;
        [SerializeField] private Material material;
        [SerializeField, Range(0.5f, 1f)] private float renderScale = 1f;
        [SerializeField, Range(0.1f, 2f)] private float motionShakeDuration = 0.55f;

        private Material runtimeMaterial;
        private RenderTexture outputTexture;
        private float time;
        private float motionFlightTime;
        private float flightRecoveryStart;
        private bool internalRecoveryActive;
        private float motionShakeRemaining;

        public Texture OutputTexture
        {
            get { return outputTexture; }
        }

        protected float TimeValue
        {
            get { return time; }
        }

        protected float MotionFlightTime
        {
            get { return motionFlightTime; }
        }

        protected Vector2 MotionOffset
        {
            get
            {
                VisualMotionSettings motionSettings = State != null ? State.GetVisualMotionSettings(KaleidoscopeVisualMode.SixD) : null;
                return motionSettings != null ? motionSettings.ImageOffset : Vector2.zero;
            }
        }

        protected float MotionShake
        {
            get
            {
                if (motionShakeRemaining <= 0f || motionShakeDuration <= 0.0001f)
                {
                    return 0f;
                }

                float normalized = Mathf.Clamp01(motionShakeRemaining / motionShakeDuration);
                return normalized * normalized;
            }
        }

        protected abstract string ShaderName { get; }

        protected virtual bool IsStageEnabled(SixDSettings settings)
        {
            return true;
        }

        public void SetModuleEnabled(bool enabled)
        {
            moduleEnabled = enabled;
        }

        public override void Tick(float deltaTime)
        {
            deltaTime = Mathf.Max(0f, deltaTime);
            time += deltaTime;
            if (time > 10000f)
            {
                time = 0f;
            }

            if (State != null && State.ActiveVisualMode == KaleidoscopeVisualMode.SixD)
            {
                VisualMotionSettings motionSettings = State.GetVisualMotionSettings(KaleidoscopeVisualMode.SixD);
                if (motionSettings != null)
                {
                    motionFlightTime += motionSettings.FlightSpeedUnits * 0.0009f * deltaTime;
                    if (motionFlightTime > 10000f || motionFlightTime < -10000f)
                    {
                        motionFlightTime = 0f;
                    }
                }
            }

            if (internalRecoveryActive && State != null && State.ImageReanimationActive)
            {
                motionFlightTime = Mathf.Lerp(flightRecoveryStart, 0f, State.ImageReanimationBlend);
            }
            else if (internalRecoveryActive)
            {
                motionFlightTime = 0f;
                internalRecoveryActive = false;
            }

            if (motionShakeRemaining > 0f)
            {
                motionShakeRemaining = Mathf.Max(0f, motionShakeRemaining - deltaTime);
            }
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            return command != null
                && (command.Type == KaleidoscopeCommandType.TriggerVisualMotionShake
                    || command.Type == KaleidoscopeCommandType.StartImageReanimation);
        }

        public override void HandleCommand(KaleidoscopeCommand command)
        {
            if (command == null)
            {
                return;
            }

            if (command.Type == KaleidoscopeCommandType.TriggerVisualMotionShake
                && command.VisualModeValue == KaleidoscopeVisualMode.SixD)
            {
                motionShakeRemaining = Mathf.Max(0.01f, motionShakeDuration);
            }
            else if (command.Type == KaleidoscopeCommandType.StartImageReanimation)
            {
                flightRecoveryStart = motionFlightTime;
                internalRecoveryActive = true;
            }
        }

        public Texture Process(Texture sourceTexture, KaleidoscopeState runtimeState)
        {
            if (sourceTexture == null || runtimeState == null)
            {
                return sourceTexture;
            }

            SixDSettings settings = runtimeState.SixDSettings;
            if (!moduleEnabled || runtimeState.ActiveVisualMode != KaleidoscopeVisualMode.SixD || settings == null || !IsStageEnabled(settings))
            {
                return sourceTexture;
            }

            Material activeMaterial = EnsureMaterial();
            if (activeMaterial == null)
            {
                return sourceTexture;
            }

            EnsureOutputTexture(runtimeState);
            if (outputTexture == null)
            {
                return sourceTexture;
            }

            ConfigureMaterial(activeMaterial, runtimeState, settings, sourceTexture);
            Graphics.Blit(sourceTexture, outputTexture, activeMaterial);
            return outputTexture;
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            string state = moduleEnabled ? "enabled" : "disabled";
            if (State == null || State.ActiveVisualMode != KaleidoscopeVisualMode.SixD)
            {
                return CreateStatus(state + ", pass-through outside 6D.");
            }

            VisualMotionSettings motionSettings = State != null ? State.GetVisualMotionSettings(KaleidoscopeVisualMode.SixD) : null;
            float flightSpeed = motionSettings != null ? motionSettings.FlightSpeedUnits : 0f;
            return CreateStatus(state + ", active in 6D, flight " + flightSpeed.ToString("0") + ".");
        }

        protected abstract void ConfigureMaterial(Material activeMaterial, KaleidoscopeState runtimeState, SixDSettings settings, Texture sourceTexture);

        protected virtual void OnDestroy()
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
            if (material != null)
            {
                return material;
            }

            if (runtimeMaterial != null)
            {
                return runtimeMaterial;
            }

            Shader resolvedShader = shader != null ? shader : Shader.Find(ShaderName);
            if (resolvedShader == null)
            {
                ReportMissingReference("Shader (" + ShaderName + ")");
                return null;
            }

            runtimeMaterial = new Material(resolvedShader)
            {
                name = ModuleId + "_RuntimeMaterial",
                hideFlags = HideFlags.HideAndDontSave
            };

            return runtimeMaterial;
        }

        private void EnsureOutputTexture(KaleidoscopeState runtimeState)
        {
            CameraSettings cameraSettings = runtimeState.CameraSettings;
            int baseWidth = cameraSettings != null ? cameraSettings.RenderWidth : 1920;
            int baseHeight = cameraSettings != null ? cameraSettings.RenderHeight : 1080;
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
                name = ModuleId + "_Output",
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
