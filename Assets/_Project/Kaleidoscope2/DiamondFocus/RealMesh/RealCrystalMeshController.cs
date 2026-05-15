using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.RealMesh
{
    public sealed class RealCrystalMeshController
    {
        private GameObject crystalObject;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh activeMesh;
        private CrystalShape activeShape = (CrystalShape)(-1);

        public Transform Transform
        {
            get { return crystalObject != null ? crystalObject.transform : null; }
        }

        public MeshRenderer Renderer
        {
            get { return meshRenderer; }
        }

        public string ActiveGameObjectName
        {
            get { return crystalObject != null ? crystalObject.name : "none"; }
        }

        public bool IsVisible
        {
            get { return crystalObject != null && crystalObject.activeSelf; }
        }

        public int RuntimeObjectCount
        {
            get { return crystalObject != null ? 1 : 0; }
        }

        public bool HasMeshFilter
        {
            get { return meshFilter != null; }
        }

        public bool HasMeshRenderer
        {
            get { return meshRenderer != null; }
        }

        public int MeshVertexCount
        {
            get { return activeMesh != null ? activeMesh.vertexCount : 0; }
        }

        public int MeshTriangleCount
        {
            get { return activeMesh != null ? activeMesh.triangles.Length / 3 : 0; }
        }

        public bool HasThickness
        {
            get { return RealCrystalShapeLibrary.HasThickness(activeMesh); }
        }

        public bool SideFacesDetected
        {
            get { return RealCrystalShapeLibrary.HasSideFaces(activeMesh); }
        }

        public bool FrontBackSeparated
        {
            get { return RealCrystalShapeLibrary.HasSeparatedFrontBack(activeMesh); }
        }

        public bool HasVolume
        {
            get { return HasThickness && SideFacesDetected && FrontBackSeparated; }
        }

        public Vector3 MeshBoundsSize
        {
            get { return activeMesh != null ? activeMesh.bounds.size : Vector3.zero; }
        }

        public string MeshDiagnosticsLabel
        {
            get
            {
                Vector3 size = MeshBoundsSize;
                return "MeshFilter " + (HasMeshFilter ? "yes" : "no")
                    + ", MeshRenderer " + (HasMeshRenderer ? "yes" : "no")
                    + ", vertices " + MeshVertexCount.ToString()
                    + ", triangles " + MeshTriangleCount.ToString()
                    + ", bounds " + size.x.ToString("0.00") + "x" + size.y.ToString("0.00") + "x" + size.z.ToString("0.00")
                    + ", hasVolume " + (HasVolume ? "true" : "false")
                    + ", sideFacesDetected " + (SideFacesDetected ? "true" : "false");
            }
        }

        public void Ensure(Transform parent, int layer)
        {
            if (crystalObject == null)
            {
                crystalObject = new GameObject("RealCrystalMesh")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                crystalObject.transform.SetParent(parent, false);
                meshFilter = crystalObject.AddComponent<MeshFilter>();
                meshRenderer = crystalObject.AddComponent<MeshRenderer>();
            }

            if (meshFilter == null)
            {
                meshFilter = crystalObject.GetComponent<MeshFilter>();
            }

            if (meshRenderer == null)
            {
                meshRenderer = crystalObject.GetComponent<MeshRenderer>();
            }

            crystalObject.layer = Mathf.Clamp(layer, 0, 31);
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
        }

        public void SetVisible(bool visible)
        {
            if (crystalObject != null && crystalObject.activeSelf != visible)
            {
                crystalObject.SetActive(visible);
            }
        }

        public void Apply(
            CrystalShape shape,
            Material material,
            Vector3 rotation,
            float scale,
            bool visible)
        {
            if (crystalObject == null || meshFilter == null || meshRenderer == null)
            {
                return;
            }

            if (activeMesh == null || activeShape != shape)
            {
                ReleaseMesh();
                activeMesh = RealCrystalShapeLibrary.CreateMesh(shape);
                activeShape = shape;
                meshFilter.sharedMesh = activeMesh;
                if (!HasVolume)
                {
                    Debug.LogWarning("[RealCrystalMeshController] RealMesh3D validation failed: generated crystal mesh is not a proven volumetric object.", crystalObject);
                }
            }

            meshRenderer.sharedMaterial = material;
            crystalObject.transform.localPosition = Vector3.zero;
            crystalObject.transform.localRotation = Quaternion.Euler(rotation);
            float safeScale = Mathf.Max(0.05f, scale);
            crystalObject.transform.localScale = new Vector3(safeScale, safeScale, safeScale);
            SetVisible(visible);
        }

        public void Shutdown()
        {
            ReleaseMesh();
            if (crystalObject != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(crystalObject);
                }
                else
                {
                    Object.DestroyImmediate(crystalObject);
                }
            }

            crystalObject = null;
            meshFilter = null;
            meshRenderer = null;
        }

        private void ReleaseMesh()
        {
            if (activeMesh == null)
            {
                return;
            }

            if (meshFilter != null && meshFilter.sharedMesh == activeMesh)
            {
                meshFilter.sharedMesh = null;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(activeMesh);
            }
            else
            {
                Object.DestroyImmediate(activeMesh);
            }

            activeMesh = null;
        }
    }
}
