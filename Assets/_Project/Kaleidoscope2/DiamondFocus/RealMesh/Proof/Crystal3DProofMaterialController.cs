using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.RealMesh.Proof
{
    [DisallowMultipleComponent]
    public sealed class Crystal3DProofMaterialController : MonoBehaviour
    {
        [SerializeField] private MeshRenderer targetRenderer;
        [SerializeField] private Material transparentBlueGlassMaterial;
        [SerializeField] private Material solidDebugMaterial;
        [SerializeField] private bool useSolidDebugMaterial;

        public bool UseSolidDebugMaterial
        {
            get { return useSolidDebugMaterial; }
            set
            {
                useSolidDebugMaterial = value;
                ApplyMaterial();
            }
        }

        private void Reset()
        {
            ResolveRenderer();
            ApplyMaterial();
        }

        private void Awake()
        {
            ResolveRenderer();
            ApplyMaterial();
        }

        private void OnValidate()
        {
            ResolveRenderer();
            ApplyMaterial();
        }

        public void Configure(Material glassMaterial, Material debugMaterial)
        {
            transparentBlueGlassMaterial = glassMaterial;
            solidDebugMaterial = debugMaterial;
            ApplyMaterial();
        }

        private void ResolveRenderer()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<MeshRenderer>();
            }
        }

        private void ApplyMaterial()
        {
            if (targetRenderer == null)
            {
                return;
            }

            Material material = useSolidDebugMaterial ? solidDebugMaterial : transparentBlueGlassMaterial;
            if (material != null)
            {
                targetRenderer.sharedMaterial = material;
            }
        }
    }
}
