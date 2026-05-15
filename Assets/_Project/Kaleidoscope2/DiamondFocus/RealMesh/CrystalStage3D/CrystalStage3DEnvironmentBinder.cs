using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.RealMesh.CrystalStage3D
{
    public sealed class CrystalStage3DEnvironmentBinder
    {
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private GameObject root;
        private Mesh backgroundMesh;
        private Mesh sideMesh;
        private Material backgroundMaterial;
        private MeshRenderer backgroundRenderer;
        private MeshRenderer[] environmentRenderers;
        private MeshFilter[] environmentFilters;
        private string diagnosticsLabel = "environment not created";

        public int RuntimeObjectCount
        {
            get
            {
                int count = root != null ? 1 : 0;
                if (backgroundRenderer != null)
                {
                    count++;
                }

                if (environmentRenderers != null)
                {
                    for (int index = 0; index < environmentRenderers.Length; index++)
                    {
                        if (environmentRenderers[index] != null)
                        {
                            count++;
                        }
                    }
                }

                return count;
            }
        }

        public string DiagnosticsLabel
        {
            get { return diagnosticsLabel; }
        }

        public bool IsActive
        {
            get { return root != null && root.activeInHierarchy; }
        }

        public void Ensure(Transform parent, int layer)
        {
            if (root == null)
            {
                root = new GameObject("CrystalStage3D_Environment")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                root.transform.SetParent(parent, false);
            }

            root.layer = Mathf.Clamp(layer, 0, 31);
            EnsureMaterial();
            EnsurePanels(layer);
        }

        public void Bind(RenderTexture sourceTexture, CrystalStage3DSettings settings, bool visible)
        {
            if (root == null || backgroundMaterial == null || settings == null)
            {
                diagnosticsLabel = "environment missing runtime objects";
                return;
            }

            bool showEnvironment = visible && settings.ShowsBackground && sourceTexture != null;
            if (root.activeSelf != showEnvironment)
            {
                root.SetActive(showEnvironment);
            }

            backgroundMaterial.SetTexture(MainTexId, sourceTexture);
            backgroundMaterial.SetColor(ColorId, new Color(1f, 1f, 1f, settings.BackgroundAlpha));

            float scale = settings.BackgroundScale;
            if (backgroundRenderer != null)
            {
                Transform backgroundTransform = backgroundRenderer.transform;
                backgroundTransform.localPosition = new Vector3(0f, 0f, settings.BackgroundDistance);
                backgroundTransform.localRotation = Quaternion.identity;
                backgroundTransform.localScale = new Vector3(scale, scale, 1f);
            }

            ApplySidePanelTransform(0, new Vector3(-scale * 0.58f, 0f, settings.BackgroundDistance * 0.58f), Quaternion.Euler(0f, 58f, 0f), scale * 0.82f);
            ApplySidePanelTransform(1, new Vector3(scale * 0.58f, 0f, settings.BackgroundDistance * 0.58f), Quaternion.Euler(0f, -58f, 0f), scale * 0.82f);
            ApplySidePanelTransform(2, new Vector3(0f, scale * 0.42f, settings.BackgroundDistance * 0.72f), Quaternion.Euler(-52f, 0f, 0f), scale * 0.72f);
            ApplySidePanelTransform(3, new Vector3(0f, -scale * 0.42f, settings.BackgroundDistance * 0.72f), Quaternion.Euler(52f, 0f, 0f), scale * 0.72f);

            diagnosticsLabel = showEnvironment
                ? "environment bound as distant background, not crystal albedo"
                : "environment hidden outside environment debug modes";
        }

        public void SetVisible(bool visible)
        {
            if (root != null && root.activeSelf != visible)
            {
                root.SetActive(visible);
            }

            diagnosticsLabel = visible
                ? "environment visible for environment debug mode"
                : "environment hidden outside environment debug modes";
        }

        public void Shutdown()
        {
            DestroyRuntimeObject(root);
            DestroyRuntimeObject(backgroundMesh);
            DestroyRuntimeObject(sideMesh);
            DestroyRuntimeObject(backgroundMaterial);
            root = null;
            backgroundMesh = null;
            sideMesh = null;
            backgroundMaterial = null;
            backgroundRenderer = null;
            environmentRenderers = null;
            environmentFilters = null;
            diagnosticsLabel = "environment not created";
        }

        private void EnsurePanels(int layer)
        {
            if (backgroundRenderer == null)
            {
                GameObject background = CreatePanel("CrystalStage3D_KaleidoscopeBackground", layer);
                background.transform.SetParent(root.transform, false);
                backgroundRenderer = background.GetComponent<MeshRenderer>();
            }

            if (environmentRenderers != null && environmentRenderers.Length == 4)
            {
                return;
            }

            environmentRenderers = new MeshRenderer[4];
            environmentFilters = new MeshFilter[4];
            for (int index = 0; index < environmentRenderers.Length; index++)
            {
                GameObject panel = CreatePanel("CrystalStage3D_EnvironmentPanel_" + (index + 1).ToString("00"), layer);
                panel.transform.SetParent(root.transform, false);
                environmentFilters[index] = panel.GetComponent<MeshFilter>();
                environmentRenderers[index] = panel.GetComponent<MeshRenderer>();
            }
        }

        private GameObject CreatePanel(string objectName, int layer)
        {
            GameObject panel = new GameObject(objectName)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            panel.layer = Mathf.Clamp(layer, 0, 31);

            MeshFilter filter = panel.AddComponent<MeshFilter>();
            filter.sharedMesh = objectName.Contains("Background")
                ? EnsureBackgroundMesh()
                : EnsureSideMesh();

            MeshRenderer renderer = panel.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = backgroundMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return panel;
        }

        private void ApplySidePanelTransform(int index, Vector3 position, Quaternion rotation, float scale)
        {
            if (environmentRenderers == null || index < 0 || index >= environmentRenderers.Length || environmentRenderers[index] == null)
            {
                return;
            }

            Transform panelTransform = environmentRenderers[index].transform;
            panelTransform.localPosition = position;
            panelTransform.localRotation = rotation;
            panelTransform.localScale = new Vector3(scale, scale, 1f);
        }

        private void EnsureMaterial()
        {
            if (backgroundMaterial != null)
            {
                return;
            }

            Shader shader = Shader.Find("Unlit/Transparent");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Texture");
            }

            if (shader == null)
            {
                diagnosticsLabel = "missing unlit texture shader";
                return;
            }

            backgroundMaterial = new Material(shader)
            {
                name = "Kaleidoscope2_CrystalStage3D_EnvironmentMaterial",
                hideFlags = HideFlags.HideAndDontSave
            };
            backgroundMaterial.SetColor(ColorId, Color.white);
            backgroundMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
        }

        private Mesh EnsureBackgroundMesh()
        {
            if (backgroundMesh != null)
            {
                return backgroundMesh;
            }

            backgroundMesh = CreatePanelMesh("Kaleidoscope2_CrystalStage3D_BackgroundMesh");
            return backgroundMesh;
        }

        private Mesh EnsureSideMesh()
        {
            if (sideMesh != null)
            {
                return sideMesh;
            }

            sideMesh = CreatePanelMesh("Kaleidoscope2_CrystalStage3D_EnvironmentPanelMesh");
            return sideMesh;
        }

        private static Mesh CreatePanelMesh(string meshName)
        {
            Mesh mesh = new Mesh
            {
                name = meshName,
                hideFlags = HideFlags.HideAndDontSave
            };
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f)
            };
            mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f)
            };
            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
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
