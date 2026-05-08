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
    }

    [DisallowMultipleComponent]
    public sealed class MirrorModule : KaleidoscopeModuleBase, IKaleidoscopeTextureProcessor
    {
        private const string DefaultShaderName = "Kaleidoscope2/Mirror";

        [Header("Shader")]
        [SerializeField] private Shader mirrorShader;
        [SerializeField] private Material mirrorMaterial;

        private Material runtimeMaterial;
        private RenderTexture outputTexture;

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

            material.SetFloat(MirrorShaderIds.MirrorCount, mirrorCount);
            material.SetFloat(MirrorShaderIds.Rotation, rotationRadians);
            material.SetFloat(MirrorShaderIds.Zoom, zoom);
            material.SetVector(MirrorShaderIds.CenterOffset, centerOffset);

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

            float speed = settings.RotationSpeed;
            if (Mathf.Abs(speed) <= 0.0001f)
            {
                return;
            }

            settings.SetRotation(settings.Rotation + speed * deltaTime);
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
                || command.Type == KaleidoscopeCommandType.SetMirrorCenterOffset;
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
