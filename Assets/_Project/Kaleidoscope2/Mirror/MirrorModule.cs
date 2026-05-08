using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Mirror
{
    public static class MirrorShaderIds
    {
        public static readonly int MirrorCount = Shader.PropertyToID("_MirrorCount");
        public static readonly int Rotation = Shader.PropertyToID("_Rotation");
        public static readonly int Zoom = Shader.PropertyToID("_Zoom");
        public static readonly int CenterOffset = Shader.PropertyToID("_CenterOffset");
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

        private Material runtimeMaterial;
        private RenderTexture outputTexture;
        private float scroll;

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
            float rotationRadians = settings != null ? settings.Rotation * Mathf.Deg2Rad : 0f;
            float zoom = settings != null ? settings.Zoom : 1f;
            Vector2 centerOffset = settings != null ? settings.CenterOffset : Vector2.zero;
            float guidesVisible = settings != null && settings.GuidesVisible ? 1f : 0f;

            material.SetFloat(MirrorShaderIds.MirrorCount, mirrorCount);
            material.SetFloat(MirrorShaderIds.Rotation, rotationRadians);
            material.SetFloat(MirrorShaderIds.Zoom, zoom);
            material.SetVector(MirrorShaderIds.CenterOffset, centerOffset);
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

            float rotationUnits = settings.RotationSpeed;
            if (Mathf.Abs(rotationUnits) > 0.0001f)
            {
                // Units are treated as degrees/second.
                settings.SetRotation(settings.Rotation + rotationUnits * deltaTime);
            }

            float forwardUnits = settings.ForwardSpeedUnits;
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
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            if (command == null)
            {
                return false;
            }

            return command.Type == KaleidoscopeCommandType.SetMirrorCount
                || command.Type == KaleidoscopeCommandType.SetMirrorRotation
                || command.Type == KaleidoscopeCommandType.SetMirrorRotationSpeed
                || command.Type == KaleidoscopeCommandType.SetMirrorZoom
                || command.Type == KaleidoscopeCommandType.SetMirrorCenterOffset
                || command.Type == KaleidoscopeCommandType.ToggleMirrorGuides
                || command.Type == KaleidoscopeCommandType.SetMirrorGuidesVisible
                || command.Type == KaleidoscopeCommandType.SetMirrorRotationSpeedUnits
                || command.Type == KaleidoscopeCommandType.SetMirrorForwardSpeedUnits;
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            MirrorSettings settings = Settings;
            string message = settings == null
                ? "Waiting for runtime state."
                : "Mirror " + settings.MirrorCount + " segments, zoom " + settings.Zoom.ToString("0.00") + ".";

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
