using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.RealMesh.CrystalStage3D
{
    public sealed class CrystalStage3DRenderer
    {
        private readonly RealCrystalMeshController meshController = new RealCrystalMeshController();

        private Material solidLitMaterial;
        private Material transparentGlassMaterial;
        private Material reflectionEnvironmentMaterial;
        private Material refractionEnvironmentMaterial;
        private Material finalCompositeMaterial;
        private string diagnosticsLabel = "renderer not created";

        public int RuntimeObjectCount
        {
            get
            {
                int count = meshController.RuntimeObjectCount;
                if (solidLitMaterial != null)
                {
                    count++;
                }

                if (transparentGlassMaterial != null)
                {
                    count++;
                }

                if (reflectionEnvironmentMaterial != null)
                {
                    count++;
                }

                if (refractionEnvironmentMaterial != null)
                {
                    count++;
                }

                if (finalCompositeMaterial != null)
                {
                    count++;
                }

                return count;
            }
        }

        public int MeshVertexCount
        {
            get { return meshController.MeshVertexCount; }
        }

        public int MeshTriangleCount
        {
            get { return meshController.MeshTriangleCount; }
        }

        public Vector3 MeshBoundsSize
        {
            get { return meshController.MeshBoundsSize; }
        }

        public Bounds CrystalWorldBounds
        {
            get { return meshController.WorldBounds; }
        }

        public Vector3 CrystalWorldPosition
        {
            get { return meshController.WorldPosition; }
        }

        public Vector3 CrystalLocalPosition
        {
            get { return meshController.LocalPosition; }
        }

        public Vector3 CrystalLocalScale
        {
            get { return meshController.LocalScale; }
        }

        public bool HasVolume
        {
            get { return meshController.HasVolume; }
        }

        public bool SideFacesDetected
        {
            get { return meshController.SideFacesDetected; }
        }

        public string ActiveGameObjectName
        {
            get { return meshController.ActiveGameObjectName; }
        }

        public string DiagnosticsLabel
        {
            get { return diagnosticsLabel + ", " + meshController.MeshDiagnosticsLabel; }
        }

        public void Ensure(Transform parent, int layer)
        {
            meshController.Ensure(parent, layer);
        }

        public bool Render(
            CrystalStage3DSettings stageSettings,
            CrystalSharedSettings sharedSettings,
            CrystalShape shape,
            CrystalMaterialMode materialMode,
            Vector3 rotation,
            float intensity,
            bool visible)
        {
            if (stageSettings == null)
            {
                diagnosticsLabel = "missing stage settings";
                return false;
            }

            Material material = ResolveMaterial(stageSettings.DebugMode);
            if (material == null)
            {
                diagnosticsLabel = "missing stage material";
                return false;
            }

            ConfigureMaterial(material, stageSettings, sharedSettings, materialMode, intensity);
            float scale = stageSettings.CrystalScale;
            Vector3 resolvedRotation = sharedSettings == null || sharedSettings.CrystalRotationEnabled
                ? rotation
                : Vector3.zero;
            meshController.Apply(shape, material, resolvedRotation, scale, visible);
            diagnosticsLabel = "stage crystal material " + stageSettings.DebugModeLabel + ", no direct kaleidoscope albedo";
            return meshController.HasVolume;
        }

        public void SetVisible(bool visible)
        {
            meshController.SetVisible(visible);
        }

        public void Shutdown()
        {
            meshController.Shutdown();
            DestroyMaterial(solidLitMaterial);
            DestroyMaterial(transparentGlassMaterial);
            DestroyMaterial(reflectionEnvironmentMaterial);
            DestroyMaterial(refractionEnvironmentMaterial);
            DestroyMaterial(finalCompositeMaterial);
            solidLitMaterial = null;
            transparentGlassMaterial = null;
            reflectionEnvironmentMaterial = null;
            refractionEnvironmentMaterial = null;
            finalCompositeMaterial = null;
            diagnosticsLabel = "renderer not created";
        }

        private Material ResolveMaterial(CrystalStage3DDebugMode debugMode)
        {
            switch (debugMode)
            {
                case CrystalStage3DDebugMode.SolidLitGeometry:
                    return EnsureMaterial(ref solidLitMaterial, "Kaleidoscope2_CrystalStage3D_SolidLitMaterial");
                case CrystalStage3DDebugMode.TransparentGlassOnly:
                    return EnsureMaterial(ref transparentGlassMaterial, "Kaleidoscope2_CrystalStage3D_TransparentGlassMaterial");
                case CrystalStage3DDebugMode.ReflectionEnvironmentOnly:
                    return EnsureMaterial(ref reflectionEnvironmentMaterial, "Kaleidoscope2_CrystalStage3D_ReflectionEnvironmentMaterial");
                case CrystalStage3DDebugMode.RefractionEnvironmentOnly:
                    return EnsureMaterial(ref refractionEnvironmentMaterial, "Kaleidoscope2_CrystalStage3D_RefractionEnvironmentMaterial");
                default:
                    return EnsureMaterial(ref finalCompositeMaterial, "Kaleidoscope2_CrystalStage3D_FinalGlassMaterial");
            }
        }

        private static Material EnsureMaterial(ref Material material, string materialName)
        {
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Standard");
            if (shader == null)
            {
                return null;
            }

            material = new Material(shader)
            {
                name = materialName,
                hideFlags = HideFlags.HideAndDontSave
            };
            return material;
        }

        private static void ConfigureMaterial(
            Material material,
            CrystalStage3DSettings stageSettings,
            CrystalSharedSettings sharedSettings,
            CrystalMaterialMode materialMode,
            float intensity)
        {
            Color tint;
            float metallic;
            float smoothness;
            ResolveMode(materialMode, out tint, out metallic, out smoothness);

            float intensity01 = Mathf.Clamp01(intensity / 20f);
            float alpha = stageSettings.UsesTransparentGlass
                ? Mathf.Clamp(sharedSettings != null ? sharedSettings.RealMeshAlpha : 0.42f, 0.18f, 0.72f)
                : 1f;

            switch (stageSettings.DebugMode)
            {
                case CrystalStage3DDebugMode.SolidLitGeometry:
                    SetupOpaque(material, new Color(0.18f, 0.62f, 1f, 1f), 0f, 0.55f);
                    return;
                case CrystalStage3DDebugMode.TransparentGlassOnly:
                    SetupTransparent(material, new Color(0.82f, 0.96f, 1f, 0.28f), 0f, 0.98f, 0.08f);
                    return;
                case CrystalStage3DDebugMode.ReflectionEnvironmentOnly:
                    SetupTransparent(material, new Color(0.9f, 0.98f, 1f, 0.34f), 0.12f, 1f, 0.18f + intensity01 * 0.16f);
                    return;
                case CrystalStage3DDebugMode.RefractionEnvironmentOnly:
                    SetupTransparent(material, new Color(0.78f, 0.95f, 1f, 0.24f), 0f, 0.94f, 0.1f);
                    return;
                default:
                    Color finalTint = new Color(tint.r, tint.g, tint.b, alpha);
                    SetupTransparent(material, finalTint, metallic, smoothness, 0.13f + intensity01 * 0.18f);
                    return;
            }
        }

        private static void SetupOpaque(Material material, Color color, float metallic, float smoothness)
        {
            material.SetOverrideTag("RenderType", "Opaque");
            material.SetFloat("_Mode", 0f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = -1;
            ApplyStandardValues(material, color, metallic, smoothness, Color.black);
        }

        private static void SetupTransparent(Material material, Color color, float metallic, float smoothness, float emission)
        {
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            ApplyStandardValues(material, color, metallic, smoothness, new Color(color.r, color.g, color.b, 1f) * emission);
        }

        private static void ApplyStandardValues(Material material, Color color, float metallic, float smoothness, Color emission)
        {
            material.SetColor("_Color", color);
            material.SetFloat("_Metallic", Mathf.Clamp01(metallic));
            material.SetFloat("_Glossiness", Mathf.Clamp01(smoothness));
            if (emission.maxColorComponent > 0.0001f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission);
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
            }
        }

        private static void ResolveMode(
            CrystalMaterialMode mode,
            out Color tint,
            out float metallic,
            out float smoothness)
        {
            switch (mode)
            {
                case CrystalMaterialMode.AbsoluteMirror:
                    tint = new Color(0.9f, 0.97f, 1f, 1f);
                    metallic = 0.16f;
                    smoothness = 1f;
                    break;
                case CrystalMaterialMode.Gemstone:
                    tint = new Color(0.62f, 0.96f, 0.86f, 1f);
                    metallic = 0f;
                    smoothness = 0.92f;
                    break;
                case CrystalMaterialMode.FuturisticPlastic:
                    tint = new Color(0.86f, 0.94f, 1f, 1f);
                    metallic = 0.02f;
                    smoothness = 0.93f;
                    break;
                case CrystalMaterialMode.Metal:
                    tint = new Color(0.92f, 0.91f, 0.86f, 1f);
                    metallic = 0.82f;
                    smoothness = 0.92f;
                    break;
                default:
                    tint = new Color(0.86f, 0.98f, 1f, 1f);
                    metallic = 0f;
                    smoothness = 0.98f;
                    break;
            }
        }

        private static void DestroyMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(material);
            }
            else
            {
                Object.DestroyImmediate(material);
            }
        }
    }
}
