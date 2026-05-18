using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus
{
    public sealed class BillboardCrystalRenderer : ICrystalRenderer
    {
        private const string ShaderName = "Kaleidoscope2/BillboardCrystal";
        private static readonly int KaleidoscopeTexId = Shader.PropertyToID("_KaleidoscopeTex");
        private static readonly int ShapeId = Shader.PropertyToID("_Shape");
        private static readonly int MaterialModeId = Shader.PropertyToID("_MaterialMode");
        private static readonly int RotationId = Shader.PropertyToID("_Rotation");
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int OverlayAmountId = Shader.PropertyToID("_OverlayAmount");
        private static readonly int CrystalRadiusId = Shader.PropertyToID("_CrystalRadius");
        private static readonly int CrystalFeatherId = Shader.PropertyToID("_CrystalFeather");

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
        private string spatialDiagnostics = "Billboard2D not measured";

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
                    + ", runtime objects " + RuntimeObjectCount.ToString()
                    + ", " + spatialDiagnostics;
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
                spatialDiagnostics = activeMaterial == null
                    ? "Billboard2D material missing, effective screen coverage 0.0%"
                    : "Billboard2D inactive, effective screen coverage 0.0%";
                Graphics.Blit(sourceTexture, outputTexture);
                return;
            }

            float screenCoverage = CrystalSpatialDiagnostics.ClampBillboardCoverage(CrystalSpatialDiagnostics.BillboardTargetCoverage);
            activeMaterial.SetTexture(KaleidoscopeTexId, sourceTexture);
            activeMaterial.SetFloat(ShapeId, (float)shape);
            activeMaterial.SetFloat(MaterialModeId, (float)materialMode);
            activeMaterial.SetVector(RotationId, rotation);
            activeMaterial.SetFloat(IntensityId, intensity);
            activeMaterial.SetFloat(OverlayAmountId, visible ? 1f : 0f);
            activeMaterial.SetFloat(CrystalRadiusId, screenCoverage * 0.5f);
            activeMaterial.SetFloat(CrystalFeatherId, screenCoverage * 0.18f);
            spatialDiagnostics = "Billboard2D measured screen coverage "
                + CrystalSpatialDiagnostics.FormatPercent(screenCoverage)
                + ", transform scale " + (root != null ? CrystalSpatialDiagnostics.FormatVector(root.transform.localScale) : "none")
                + ", render bounds " + sourceTexture.width.ToString() + "x" + sourceTexture.height.ToString()
                + ", apparent screen size " + CrystalSpatialDiagnostics.FormatPercent(screenCoverage) + " height"
                + ", composition fullscreen-space masked blit"
                + ", object-space false"
                + ", hierarchy " + (root != null ? CrystalSpatialDiagnostics.GetHierarchyPath(root.transform) : "none");
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
