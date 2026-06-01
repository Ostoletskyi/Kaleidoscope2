using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.RealMesh
{
    public sealed class RealCrystalMaterialBinder
    {
        private const string ShaderName = "Kaleidoscope2/RealCrystalOptics";
        private static readonly int KaleidoscopeTexId = Shader.PropertyToID("_KaleidoscopeTex");
        private static readonly int TintId = Shader.PropertyToID("_Tint");
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
        private static readonly int MetallicId = Shader.PropertyToID("_Metallic");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");
        private static readonly int TransparencyId = Shader.PropertyToID("_Transparency");
        private static readonly int RefractionStrengthId = Shader.PropertyToID("_RefractionStrength");
        private static readonly int FresnelPowerId = Shader.PropertyToID("_FresnelPower");
        private static readonly int ReflectionStrengthId = Shader.PropertyToID("_ReflectionStrength");
        private static readonly int InternalBrightnessId = Shader.PropertyToID("_InternalBrightness");
        private static readonly int MinimumTransmissionId = Shader.PropertyToID("_MinimumTransmission");
        private static readonly int SpecularStrengthId = Shader.PropertyToID("_SpecularStrength");

        private readonly Shader assignedShader;
        private Material material;
        private Material geometryValidationMaterial;
        private string diagnosticsLabel = "optical material not bound";

        public RealCrystalMaterialBinder(Shader shader)
        {
            assignedShader = shader;
        }

        public Material Material
        {
            get { return material; }
        }

        public string DiagnosticsLabel
        {
            get { return diagnosticsLabel; }
        }

        public Material BindGeometryValidationMaterial()
        {
            Material activeMaterial = EnsureGeometryValidationMaterial();
            if (activeMaterial == null)
            {
                diagnosticsLabel = "missing geometry validation material";
                return null;
            }

            diagnosticsLabel = "solid blue geometry validation material";
            return activeMaterial;
        }

        public Material Bind(
            RenderTexture sourceTexture,
            CrystalMaterialMode materialMode,
            float intensity,
            CrystalSharedSettings settings)
        {
            Material activeMaterial = EnsureMaterial();
            if (activeMaterial == null)
            {
                diagnosticsLabel = "missing RealCrystalOptics shader";
                return null;
            }

            Color tint;
            float metallic;
            float smoothness;
            ResolveMaterial(materialMode, out tint, out metallic, out smoothness);
            float alpha = settings != null ? settings.RealMeshAlpha : 0.58f;
            float transparency = settings != null ? settings.Transparency : 0f;
            float refractionStrength = settings != null ? settings.RefractionStrength : 0.04f;
            float fresnelPower = settings != null ? settings.FresnelPower : 3.2f;
            float reflectionStrength = settings != null ? settings.ReflectionStrength : 0.6f;
            float internalBrightness = settings != null ? settings.InternalBrightness : 1f;
            float minimumTransmission = settings != null ? settings.MinimumTransmission : 0.1f;
            float specularStrength = settings != null ? settings.SpecularStrength : 0.75f;

            activeMaterial.SetTexture(KaleidoscopeTexId, sourceTexture);
            activeMaterial.SetColor(TintId, tint);
            activeMaterial.SetFloat(IntensityId, Mathf.Clamp(intensity, 0f, 20f));
            activeMaterial.SetFloat(AlphaId, Mathf.Clamp01(alpha));
            activeMaterial.SetFloat(MetallicId, metallic);
            activeMaterial.SetFloat(SmoothnessId, smoothness);
            activeMaterial.SetFloat(TransparencyId, Mathf.Clamp01(transparency));
            activeMaterial.SetFloat(RefractionStrengthId, Mathf.Clamp(refractionStrength, 0f, 0.12f));
            activeMaterial.SetFloat(FresnelPowerId, Mathf.Clamp(fresnelPower, 0.5f, 8f));
            activeMaterial.SetFloat(ReflectionStrengthId, Mathf.Clamp01(reflectionStrength));
            activeMaterial.SetFloat(InternalBrightnessId, Mathf.Clamp(internalBrightness, 0f, 3f));
            activeMaterial.SetFloat(MinimumTransmissionId, Mathf.Clamp01(minimumTransmission));
            activeMaterial.SetFloat(SpecularStrengthId, Mathf.Clamp01(specularStrength));
            activeMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            diagnosticsLabel = "optics "
                + (settings != null ? settings.OpticalMaterialStatus : "defaults")
                + ", specular " + specularStrength.ToString("0.00");
            return activeMaterial;
        }

        public void Shutdown()
        {
            DestroyMaterial(material);
            DestroyMaterial(geometryValidationMaterial);
            material = null;
            geometryValidationMaterial = null;
            diagnosticsLabel = "optical material not bound";
        }

        private Material EnsureGeometryValidationMaterial()
        {
            if (geometryValidationMaterial != null)
            {
                return geometryValidationMaterial;
            }

            Shader shader = Shader.Find("Standard");
            if (shader == null)
            {
                return null;
            }

            geometryValidationMaterial = new Material(shader)
            {
                name = "Kaleidoscope2_RealMesh_SolidGeometryValidationMaterial",
                hideFlags = HideFlags.HideAndDontSave
            };
            geometryValidationMaterial.SetColor("_Color", new Color(0.18f, 0.62f, 1f, 0.46f));
            geometryValidationMaterial.SetFloat("_Mode", 3f);
            geometryValidationMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            geometryValidationMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            geometryValidationMaterial.SetInt("_ZWrite", 0);
            geometryValidationMaterial.DisableKeyword("_ALPHATEST_ON");
            geometryValidationMaterial.EnableKeyword("_ALPHABLEND_ON");
            geometryValidationMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            geometryValidationMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            geometryValidationMaterial.SetFloat("_Metallic", 0f);
            geometryValidationMaterial.SetFloat("_Glossiness", 0.35f);
            return geometryValidationMaterial;
        }

        private static void DestroyMaterial(Material target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target);
            }
            else
            {
                Object.DestroyImmediate(target);
            }
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
                name = "Kaleidoscope2_CrystalPresentation_RealMeshMaterial",
                hideFlags = HideFlags.HideAndDontSave
            };
            return material;
        }

        private static void ResolveMaterial(
            CrystalMaterialMode mode,
            out Color tint,
            out float metallic,
            out float smoothness)
        {
            switch (mode)
            {
                case CrystalMaterialMode.AbsoluteMirror:
                    tint = new Color(0.9f, 0.97f, 1f, 1f);
                    metallic = 0.25f;
                    smoothness = 0.98f;
                    break;
                case CrystalMaterialMode.Gemstone:
                    tint = new Color(0.72f, 0.98f, 0.9f, 1f);
                    metallic = 0.02f;
                    smoothness = 0.9f;
                    break;
                case CrystalMaterialMode.FuturisticPlastic:
                    tint = new Color(0.86f, 0.94f, 1f, 1f);
                    metallic = 0.02f;
                    smoothness = 0.93f;
                    break;
                case CrystalMaterialMode.Metal:
                    tint = new Color(0.92f, 0.91f, 0.86f, 1f);
                    metallic = 0.92f;
                    smoothness = 0.94f;
                    break;
                default:
                    tint = new Color(0.9f, 0.98f, 1f, 1f);
                    metallic = 0.02f;
                    smoothness = 0.96f;
                    break;
            }
        }
    }
}
