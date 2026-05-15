using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.Billboard
{
    public sealed class BillboardCrystalPlane
    {
        private GameObject planeObject;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh mesh;

        public MeshRenderer Renderer
        {
            get { return meshRenderer; }
        }

        public void Ensure(Transform parent, int layer)
        {
            if (planeObject == null)
            {
                planeObject = new GameObject("BillboardCrystalPlane")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                planeObject.transform.SetParent(parent, false);
                meshFilter = planeObject.AddComponent<MeshFilter>();
                meshRenderer = planeObject.AddComponent<MeshRenderer>();
            }

            if (meshFilter == null)
            {
                meshFilter = planeObject.GetComponent<MeshFilter>();
            }

            if (meshRenderer == null)
            {
                meshRenderer = planeObject.GetComponent<MeshRenderer>();
            }

            planeObject.layer = Mathf.Clamp(layer, 0, 31);
            if (mesh == null)
            {
                mesh = BuildQuadMesh();
            }

            meshFilter.sharedMesh = mesh;
        }

        public void SetVisible(bool visible)
        {
            if (planeObject != null && planeObject.activeSelf != visible)
            {
                planeObject.SetActive(visible);
            }
        }

        public void Shutdown()
        {
            if (planeObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(planeObject);
            }
            else
            {
                Object.DestroyImmediate(planeObject);
            }

            planeObject = null;
            meshFilter = null;
            meshRenderer = null;
            if (mesh != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(mesh);
                }
                else
                {
                    Object.DestroyImmediate(mesh);
                }

                mesh = null;
            }
        }

        private static Mesh BuildQuadMesh()
        {
            Mesh mesh = new Mesh
            {
                name = "BillboardCrystalPlaneMesh",
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
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
