using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus
{
    public sealed class BillboardCrystalRenderer : ICrystalRenderer
    {
        private const string ShaderName = "Kaleidoscope2/BillboardCrystal";

        private readonly Transform owner;
        private readonly Shader assignedShader;
        private RenderTexture sourceTexture;
        private RenderTexture outputTexture;
        private Material material;
        private GameObject root;
        private CrystalShape shape;
        private CrystalMaterialMode materialMode;
        private Vector3 rotation;
        private float intensity;
        private bool visible;

        public BillboardCrystalRenderer(Transform ownerTransform, Shader shader)
        {
            owner = ownerTransform;
            assignedShader = shader;
        }

        public Texture OutputTexture
        {
            get { return outputTexture != null ? outputTexture : sourceTexture; }
        }

        public bool IsVisible
        {
            get { return visible; }
        }

        public int RuntimeObjectCount
        {
            get
            {
                int count = 0;
                if (outputTexture != null)
                {
                    count++;
                }

                if (root != null)
                {
                    count++;
                }

                if (material != null)
                {
                    count++;
                }

                return count;
            }
        }

        public string LifecycleState
        {
            get
            {
                return (visible ? "visible" : "inactive")
                    + ", root " + ResolveRootState()
                    + ", runtime objects " + RuntimeObjectCount.ToString();
            }
        }

        public void Initialize(CrystalSharedSettings sharedSettings)
        {
            EnsureRoot();
            ApplyRootVisibility(visible);
        }

        public void PrepareOutput(int width, int height, bool linear)
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
                sRGB = linear,
                useMipMap = false,
                autoGenerateMips = false
            };

            outputTexture = new RenderTexture(descriptor)
            {
                name = "Kaleidoscope2_CrystalPresentation_BillboardOutput",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            outputTexture.Create();
        }

        public void SetSourceTexture(RenderTexture texture)
        {
            sourceTexture = texture;
        }

        public void SetShape(CrystalShape value)
        {
            shape = value;
        }

        public void SetMaterialMode(CrystalMaterialMode value)
        {
            materialMode = value;
        }

        public void SetRotation(Vector3 value)
        {
            rotation = value;
        }

        public void SetIntensity(float value)
        {
            intensity = Mathf.Clamp(value, 0f, 20f);
        }

        public void SetVisible(bool value)
        {
            visible = value;
            ApplyRootVisibility(value);
        }

        public void Render()
        {
            if (sourceTexture == null)
            {
                return;
            }

            PrepareOutput(sourceTexture.width, sourceTexture.height, QualitySettings.activeColorSpace == ColorSpace.Linear);
            if (outputTexture == null)
            {
                return;
            }

            Material activeMaterial = EnsureMaterial();
            if (activeMaterial == null || !visible)
            {
                Graphics.Blit(sourceTexture, outputTexture);
                return;
            }

            activeMaterial.SetTexture("_KaleidoscopeTex", sourceTexture);
            activeMaterial.SetFloat("_Shape", (float)shape);
            activeMaterial.SetFloat("_MaterialMode", (float)materialMode);
            activeMaterial.SetVector("_Rotation", rotation);
            activeMaterial.SetFloat("_Intensity", intensity);
            activeMaterial.SetFloat("_OverlayAmount", 0f);
            Graphics.Blit(sourceTexture, outputTexture, activeMaterial);
        }

        public void Shutdown()
        {
            visible = false;
            DestroyRuntimeObject(root);
            root = null;
            ReleaseOutput();
            DestroyRuntimeObject(material);
            material = null;
            sourceTexture = null;
        }

        private void EnsureRoot()
        {
            if (root != null)
            {
                return;
            }

            root = new GameObject("Billboard2DRoot")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            if (owner != null)
            {
                root.transform.SetParent(owner, false);
            }
        }

        private void ApplyRootVisibility(bool value)
        {
            if (root == null)
            {
                return;
            }

            if (root.activeSelf != value)
            {
                root.SetActive(value);
            }
        }

        private string ResolveRootState()
        {
            if (root == null)
            {
                return "not created";
            }

            return root.activeSelf ? "active" : "inactive";
        }

        private Material EnsureMaterial()
        {
            if (material != null)
            {
                return material;
            }

            Shader shader = assignedShader != null ? assignedShader : Shader.Find(ShaderName);
            if (shader == null)
            {
                return null;
            }

            material = new Material(shader)
            {
                name = "Kaleidoscope2_CrystalPresentation_BillboardMaterial",
                hideFlags = HideFlags.HideAndDontSave
            };
            return material;
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
