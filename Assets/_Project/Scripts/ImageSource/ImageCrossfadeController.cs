using UnityEngine;

namespace Kaleidoscope2.ImageSource
{
    public sealed class ImageCrossfadeController
    {
        private static readonly int NextTexId = Shader.PropertyToID("_NextTex");
        private static readonly int BlendId = Shader.PropertyToID("_Blend");
        private const string CrossfadeShaderName = "Kaleidoscope2/ImageCrossfade";

        private Material material;
        private RenderTexture outputTexture;
        private Texture currentTexture;
        private Texture nextTexture;
        private float elapsed;
        private float duration = 2.5f;
        private bool transitioning;

        public Texture OutputTexture
        {
            get { return transitioning && outputTexture != null ? outputTexture : currentTexture; }
        }

        public bool IsTransitioning
        {
            get { return transitioning; }
        }

        public float Blend
        {
            get { return duration > 0.0001f ? Mathf.Clamp01(elapsed / duration) : 1f; }
        }

        public void SetCurrent(Texture texture)
        {
            currentTexture = texture;
            if (!transitioning)
            {
                nextTexture = null;
            }
        }

        public void Begin(Texture current, Texture next, float crossfadeDuration)
        {
            currentTexture = current;
            nextTexture = next;
            duration = Mathf.Max(0.01f, crossfadeDuration);
            elapsed = 0f;
            transitioning = currentTexture != null && nextTexture != null;

            if (!transitioning)
            {
                currentTexture = nextTexture != null ? nextTexture : currentTexture;
            }
        }

        public bool Tick(float deltaTime)
        {
            if (!transitioning)
            {
                return false;
            }

            elapsed += Mathf.Max(0f, deltaTime);
            RenderBlend();
            if (elapsed < duration)
            {
                return false;
            }

            currentTexture = nextTexture;
            nextTexture = null;
            transitioning = false;
            return true;
        }

        public void RenderBlend()
        {
            if (!transitioning || currentTexture == null || nextTexture == null)
            {
                return;
            }

            Material blendMaterial = EnsureMaterial();
            if (blendMaterial == null)
            {
                return;
            }

            EnsureOutputTexture(currentTexture);
            if (outputTexture == null)
            {
                return;
            }

            blendMaterial.SetTexture(NextTexId, nextTexture);
            blendMaterial.SetFloat(BlendId, Blend);
            Graphics.Blit(currentTexture, outputTexture, blendMaterial);
        }

        public void Dispose()
        {
            if (outputTexture != null)
            {
                if (outputTexture.IsCreated())
                {
                    outputTexture.Release();
                }

                if (Application.isPlaying)
                {
                    Object.Destroy(outputTexture);
                }
                else
                {
                    Object.DestroyImmediate(outputTexture);
                }

                outputTexture = null;
            }

            if (material != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(material);
                }
                else
                {
                    Object.DestroyImmediate(material);
                }

                material = null;
            }
        }

        private Material EnsureMaterial()
        {
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find(CrossfadeShaderName);
            if (shader == null)
            {
                Debug.LogWarning("[ImageCrossfade] Missing shader: " + CrossfadeShaderName);
                return null;
            }

            material = new Material(shader)
            {
                name = "Kaleidoscope2_ImageCrossfade_RuntimeMaterial",
                hideFlags = HideFlags.HideAndDontSave
            };
            return material;
        }

        private void EnsureOutputTexture(Texture source)
        {
            int width = source != null ? source.width : 2;
            int height = source != null ? source.height : 2;

            if (outputTexture != null && outputTexture.width == width && outputTexture.height == height)
            {
                return;
            }

            if (outputTexture != null)
            {
                if (outputTexture.IsCreated())
                {
                    outputTexture.Release();
                }

                if (Application.isPlaying)
                {
                    Object.Destroy(outputTexture);
                }
                else
                {
                    Object.DestroyImmediate(outputTexture);
                }
            }

            outputTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
            {
                name = "Kaleidoscope2_ImageCrossfade_Output",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Repeat
            };
            outputTexture.Create();
        }
    }
}
