using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.RealMesh.Proof
{
    [DisallowMultipleComponent]
    public sealed class Crystal3DProofValidator : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private bool logOnStart = true;
        [SerializeField] private string validationReport = "Not validated";

        public string ValidationReport
        {
            get { return validationReport; }
        }

        private void Reset()
        {
            ResolveReferences();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void Start()
        {
            if (logOnStart)
            {
                LogValidation();
            }
        }

        [ContextMenu("Log Crystal3D Proof Validation")]
        public void LogValidation()
        {
            ResolveReferences();
            Mesh mesh = meshFilter != null ? meshFilter.sharedMesh : null;
            int vertexCount = mesh != null ? mesh.vertexCount : 0;
            int triangleCount = mesh != null ? mesh.triangles.Length / 3 : 0;
            Vector3 boundsSize = mesh != null ? mesh.bounds.size : Vector3.zero;
            bool hasVolume = Crystal3DProofMeshFactory.HasVolume(mesh);
            bool depthValid = Crystal3DProofMeshFactory.DepthIsValid(mesh);

            validationReport = "[Crystal3DProof] vertices " + vertexCount.ToString()
                + ", triangles " + triangleCount.ToString()
                + ", bounds " + boundsSize.x.ToString("0.00") + "x" + boundsSize.y.ToString("0.00") + "x" + boundsSize.z.ToString("0.00")
                + ", MeshFilter " + (meshFilter != null ? "yes" : "no")
                + ", MeshRenderer " + (meshRenderer != null ? "yes" : "no")
                + ", hasVolume " + (hasVolume ? "true" : "false")
                + ", bounds.z > 0.5 " + (depthValid ? "true" : "false");

            Debug.Log(validationReport, this);
        }

        private void ResolveReferences()
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }

            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }
        }
    }
}
