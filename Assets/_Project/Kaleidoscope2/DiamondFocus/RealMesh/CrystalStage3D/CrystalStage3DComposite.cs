using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.RealMesh.CrystalStage3D
{
    public sealed class CrystalStage3DComposite
    {
        private const string CompositeShaderName = "Kaleidoscope2/DiamondComposite";
        private static readonly int BlurredBackgroundTexId = Shader.PropertyToID("_BlurredBackgroundTex");
        private static readonly int CrystalTexId = Shader.PropertyToID("_CrystalTex");
        private static readonly int BackgroundBlurAmountId = Shader.PropertyToID("_BackgroundBlurAmount");

        private Material compositeMaterial;
        private RenderTexture outputTexture;
        private string diagnosticsLabel = "composite not created";

        public Texture OutputTexture
        {
            get { return outputTexture; }
        }

        public int RuntimeObjectCount
        {
            get { return (compositeMaterial != null ? 1 : 0) + (outputTexture != null ? 1 : 0); }
        }

        public string DiagnosticsLabel
        {
            get { return diagnosticsLabel; }
        }

        public bool Composite(RenderTexture sourceTexture, RenderTexture stageTexture)
        {
            if (sourceTexture == null || stageTexture == null)
            {
                diagnosticsLabel = "missing source or stage texture";
                return false;
            }

            PrepareOutput(sourceTexture.width, sourceTexture.height);
            if (outputTexture == null)
            {
                diagnosticsLabel = "missing output texture";
                return false;
            }

            Material material = EnsureCompositeMaterial();
            if (material == null)
            {
                Graphics.Blit(sourceTexture, outputTexture);
                diagnosticsLabel = "fallback kept clean background because alpha composite shader is missing";
                return true;
            }

            material.SetTexture(BlurredBackgroundTexId, sourceTexture);
            material.SetTexture(CrystalTexId, stageTexture);
            material.SetFloat(BackgroundBlurAmountId, 0f);
            Graphics.Blit(sourceTexture, outputTexture, material);
            diagnosticsLabel = "stage render texture composited over final kaleidoscope output";
            return true;
        }

        public void Shutdown()
        {
            ReleaseOutput();
            DestroyRuntimeObject(compositeMaterial);
            compositeMaterial = null;
            diagnosticsLabel = "composite not created";
        }

        private void PrepareOutput(int width, int height)
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            if (outputTexture != null && outputTexture.width == width && outputTexture.height == height)
            {
                return;
            }

            ReleaseOutput();
            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 0)
            {
                msaaSamples = 1,
                sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear,
                useMipMap = false,
                autoGenerateMips = false
            };

            outputTexture = new RenderTexture(descriptor)
            {
                name = "Kaleidoscope2_CrystalStage3D_CompositeOutput",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            outputTexture.Create();
        }

        private Material EnsureCompositeMaterial()
        {
            if (compositeMaterial != null)
            {
                return compositeMaterial;
            }

            Shader shader = Shader.Find(CompositeShaderName);
            if (shader == null)
            {
                return null;
            }

            compositeMaterial = new Material(shader)
            {
                name = "Kaleidoscope2_CrystalStage3D_CompositeMaterial",
                hideFlags = HideFlags.HideAndDontSave
            };
            return compositeMaterial;
        }

        private void ReleaseOutput()
        {
            if (outputTexture == null)
            {
                return;
            }

            if (outputTexture.IsCreated())
            {
                outputTexture.Release();
            }

            DestroyRuntimeObject(outputTexture);
            outputTexture = null;
        }

        private static void DestroyRuntimeObject(Object instance)
        {
            if (instance == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(instance);
            }
            else
            {
                Object.DestroyImmediate(instance);
            }
        }
    }
}
