using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Mirror
{
    public static class MirrorShaderIds
    {
        public static readonly int MirrorCount = Shader.PropertyToID("_MirrorCount");
        public static readonly int MirrorCountFrom = Shader.PropertyToID("_MirrorCountFrom");
        public static readonly int MirrorCountTo = Shader.PropertyToID("_MirrorCountTo");
        public static readonly int MirrorTransition = Shader.PropertyToID("_MirrorTransition");
        public static readonly int Rotation = Shader.PropertyToID("_Rotation");
        public static readonly int Zoom = Shader.PropertyToID("_Zoom");
        public static readonly int CenterOffset = Shader.PropertyToID("_CenterOffset");
        public static readonly int MotionOffset = Shader.PropertyToID("_MotionOffset");
        public static readonly int Scroll = Shader.PropertyToID("_Scroll");
        public static readonly int GuidesVisible = Shader.PropertyToID("_GuidesVisible");
        public static readonly int GuideStrength = Shader.PropertyToID("_GuideStrength");
    }

    [DisallowMultipleComponent]
    public sealed class MirrorModule : KaleidoscopeModuleBase, IKaleidoscopeTextureProcessor
    {
        private const string DefaultShaderName = "Kaleidoscope2/Mirror";

        [Header("Shader")]
        [SerializeField] private Shader mirrorShader;
        [SerializeField] private Material mirrorMaterial;
        [SerializeField, Range(0f, 1f)] private float guideStrength = 0.65f;
        [SerializeField, Range(0.1f, 2f)] private float motionShakeDuration = 0.55f;
        [SerializeField, Range(0f, 0.08f)] private float motionShakeStrength = 0.028f;

        private Material runtimeMaterial;
        private RenderTexture outputTexture;
        private float scroll;
        private float scrollRecoveryStart;
        private bool scrollRecoveryActive;
        private float motionShakeRemaining;
        private float motionShakeSeed = 0.37f;

        public override string ModuleId
        {
            get { return "Mirror"; }
        }

        public Texture OutputTexture
        {
            get { return outputTexture; }
        }

        public MirrorSettings Settings
        {
            get { return State != null ? State.MirrorSettings : null; }
        }

        public Texture Process(Texture sourceTexture, KaleidoscopeState runtimeState)
        {
            if (sourceTexture == null || runtimeState == null)
            {
                return sourceTexture;
            }

            Material material = EnsureMaterial(runtimeState);
            if (material == null)
            {
                return sourceTexture;
            }

            EnsureOutputTexture(runtimeState);
            if (outputTexture == null)
            {
                return sourceTexture;
            }

            MirrorSettings settings = runtimeState.MirrorSettings;
            int mirrorCount = settings != null ? settings.MirrorCount : 6;
            MirrorCountTransitionState transitionState = settings != null ? settings.MirrorCountTransition : null;
            int mirrorCountFrom = transitionState != null && transitionState.Active ? transitionState.FromCount : mirrorCount;
            int mirrorCountTo = transitionState != null && transitionState.Active ? transitionState.ToCount : mirrorCount;
            float mirrorTransition = transitionState != null && transitionState.Active ? transitionState.SmoothProgress : 1f;
            float rotationRadians = settings != null ? settings.Rotation * Mathf.Deg2Rad : 0f;
            float zoom = settings != null ? settings.Zoom : 1f;
            Vector2 centerOffset = settings != null ? settings.CenterOffset : Vector2.zero;
            Vector2 motionOffset = GetActiveMotionOffset(runtimeState) + GetShakeOffset();
            float guidesVisible = settings != null && settings.GuidesVisible ? 1f : 0f;

            material.SetFloat(MirrorShaderIds.MirrorCount, mirrorCount);
            material.SetFloat(MirrorShaderIds.MirrorCountFrom, mirrorCountFrom);
            material.SetFloat(MirrorShaderIds.MirrorCountTo, mirrorCountTo);
            material.SetFloat(MirrorShaderIds.MirrorTransition, mirrorTransition);
            material.SetFloat(MirrorShaderIds.Rotation, rotationRadians);
            material.SetFloat(MirrorShaderIds.Zoom, zoom);
            material.SetVector(MirrorShaderIds.CenterOffset, centerOffset);
            material.SetVector(MirrorShaderIds.MotionOffset, motionOffset);
            material.SetFloat(MirrorShaderIds.Scroll, scroll);
            material.SetFloat(MirrorShaderIds.GuidesVisible, guidesVisible);
            material.SetFloat(MirrorShaderIds.GuideStrength, guideStrength);

            Graphics.Blit(sourceTexture, outputTexture, material);
            return outputTexture;
        }

        public override void Tick(float deltaTime)
        {
            if (State == null)
            {
                return;
            }

            MirrorSettings settings = State.MirrorSettings;
            if (settings == null)
            {
                return;
            }

            settings.TickMirrorCountTransition(deltaTime);

            float rotationUnits = settings.RotationSpeed;
            if (Mathf.Abs(rotationUnits) > 0.0001f)
            {
                // Units are treated as degrees/second.
                settings.SetRotation(settings.Rotation + rotationUnits * deltaTime);
            }

            float forwardUnits = settings.ForwardSpeedUnits;
            if (State.ActiveVisualMode == KaleidoscopeVisualMode.Classic)
            {
                VisualMotionSettings motionSettings = State.GetVisualMotionSettings(KaleidoscopeVisualMode.Classic);
                if (motionSettings != null)
                {
                    forwardUnits += motionSettings.FlightSpeedUnits;
                }
            }

            if (Mathf.Abs(forwardUnits) > 0.0001f)
            {
                // A lightweight "forward movement" illusion by scrolling sampling radius.
                // Units are arbitrary; tuned to be noticeable at ~100.
                scroll += forwardUnits * deltaTime * 0.0025f;
                if (scroll > 10000f || scroll < -10000f)
                {
                    scroll = 0f;
                }
            }

            if (scrollRecoveryActive && State.ImageReanimationActive)
            {
                scroll = Mathf.Lerp(scrollRecoveryStart, 0f, State.ImageReanimationBlend);
            }
            else if (scrollRecoveryActive)
            {
                scroll = 0f;
                scrollRecoveryActive = false;
            }

            if (motionShakeRemaining > 0f)
            {
                motionShakeRemaining = Mathf.Max(0f, motionShakeRemaining - Mathf.Max(0f, deltaTime));
            }
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            if (command == null)
            {
                return false;
            }

            return command.Type == KaleidoscopeCommandType.SetMirrorCount
                || command.Type == KaleidoscopeCommandType.CycleTopRowMirrorCountPreset
                || command.Type == KaleidoscopeCommandType.SetMirrorRotation
                || command.Type == KaleidoscopeCommandType.SetMirrorRotationSpeed
                || command.Type == KaleidoscopeCommandType.SetMirrorZoom
                || command.Type == KaleidoscopeCommandType.SetMirrorCenterOffset
                || command.Type == KaleidoscopeCommandType.ToggleMirrorGuides
                || command.Type == KaleidoscopeCommandType.SetMirrorGuidesVisible
                || command.Type == KaleidoscopeCommandType.SetMirrorRotationSpeedUnits
                || command.Type == KaleidoscopeCommandType.SetMirrorForwardSpeedUnits
                || command.Type == KaleidoscopeCommandType.TriggerVisualMotionShake
                || command.Type == KaleidoscopeCommandType.StartImageReanimation;
        }

        public override void HandleCommand(KaleidoscopeCommand command)
        {
            if (command == null)
            {
                return;
            }

            if (command.Type == KaleidoscopeCommandType.TriggerVisualMotionShake && command.VisualModeValue == KaleidoscopeVisualMode.Classic)
            {
                motionShakeRemaining = Mathf.Max(0.01f, motionShakeDuration);
                motionShakeSeed = Time.unscaledTime + 0.37f;
            }
            else if (command.Type == KaleidoscopeCommandType.StartImageReanimation)
            {
                scrollRecoveryStart = scroll;
                scrollRecoveryActive = true;
            }
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            MirrorSettings settings = Settings;
            string message;
            if (settings == null)
            {
                message = "Waiting for runtime state.";
            }
            else if (settings.MirrorCountTransition.Active)
            {
                message = "Mirror transitioning "
                    + settings.MirrorCountTransition.FromCount.ToString()
                    + "->"
                    + settings.MirrorCountTransition.ToCount.ToString()
                    + " segments, progress "
                    + settings.MirrorCountTransition.SmoothProgress.ToString("0.00")
                    + ", zoom "
                    + settings.Zoom.ToString("0.00")
                    + ".";
            }
            else
            {
                message = "Mirror " + settings.MirrorCount.ToString() + " segments, zoom " + settings.Zoom.ToString("0.00") + ".";
            }

            return CreateStatus(message);
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

        private Material EnsureMaterial(KaleidoscopeState runtimeState)
        {
            if (mirrorMaterial != null)
            {
                return mirrorMaterial;
            }

            if (runtimeMaterial != null)
            {
                return runtimeMaterial;
            }

            Shader shader = mirrorShader != null ? mirrorShader : Shader.Find(DefaultShaderName);
            if (shader == null)
            {
                ReportMissingReference("MirrorShader (" + DefaultShaderName + ")");
                return null;
            }

            runtimeMaterial = new Material(shader)
            {
                name = "Kaleidoscope2_Mirror_RuntimeMaterial",
                hideFlags = HideFlags.HideAndDontSave
            };

            return runtimeMaterial;
        }

        private Vector2 GetActiveMotionOffset(KaleidoscopeState runtimeState)
        {
            if (runtimeState == null || runtimeState.ActiveVisualMode != KaleidoscopeVisualMode.Classic)
            {
                return Vector2.zero;
            }

            VisualMotionSettings motionSettings = runtimeState.GetVisualMotionSettings(KaleidoscopeVisualMode.Classic);
            return motionSettings != null ? motionSettings.ImageOffset : Vector2.zero;
        }

        private Vector2 GetShakeOffset()
        {
            if (motionShakeRemaining <= 0f || motionShakeDuration <= 0.0001f)
            {
                return Vector2.zero;
            }

            float normalized = Mathf.Clamp01(motionShakeRemaining / motionShakeDuration);
            float strength = normalized * normalized * motionShakeStrength;
            float phase = Time.unscaledTime * 41f + motionShakeSeed * 13f;
            return new Vector2(Mathf.Sin(phase * 1.31f), Mathf.Cos(phase * 1.73f)) * strength;
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
                name = "Kaleidoscope2_Mirror_Output",
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
