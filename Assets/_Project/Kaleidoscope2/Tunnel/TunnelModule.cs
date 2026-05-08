using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Tunnel
{
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

        private Material runtimeMaterial;
        private RenderTexture outputTexture;

        public Texture OutputTexture
        {
            get { return outputTexture; }
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            return command != null && command.Type == KaleidoscopeCommandType.SetTunnelEnabled;
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

            material.SetFloat("_DepthScale", depthScale);
            material.SetFloat("_CenterDarken", centerDarken);
            material.SetFloat("_Scroll", Time.unscaledTime * scrollSpeed);

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
            return CreateStatus(enabled ? "Tunnel enabled (post-process)" : "Tunnel disabled");
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
