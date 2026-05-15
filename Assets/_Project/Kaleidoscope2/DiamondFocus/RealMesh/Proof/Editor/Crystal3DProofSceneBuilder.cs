#if UNITY_EDITOR
using System.IO;
using Kaleidoscope2.DiamondFocus.RealMesh.Proof;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kaleidoscope2.DiamondFocus.RealMesh.Proof.Editor
{
    public static class Crystal3DProofSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Kaleidoscope2/Scenes/Crystal3D_Proof.unity";
        private const string ProofFolder = "Assets/_Project/Kaleidoscope2/DiamondFocus/RealMesh/Proof";
        private const string MaterialFolder = ProofFolder + "/Materials";
        private const string MeshPath = ProofFolder + "/Crystal3D_ProofMesh.asset";
        private const string GlassMaterialPath = MaterialFolder + "/Crystal3D_Proof_TransparentBlueGlass.mat";
        private const string DebugMaterialPath = MaterialFolder + "/Crystal3D_Proof_UnlitSolidDebug.mat";

        [MenuItem("Kaleidoscope2/Proof/Rebuild Crystal3D Proof Scene")]
        public static void BuildSceneFromMenu()
        {
            BuildScene();
        }

        public static void BuildSceneFromCommandLine()
        {
            BuildScene();
        }

        private static void BuildScene()
        {
            EnsureFolders();
            Mesh mesh = CreateOrReplaceMeshAsset();
            Material glassMaterial = CreateOrReplaceGlassMaterial();
            Material debugMaterial = CreateOrReplaceDebugMaterial();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Crystal3D_Proof";

            GameObject root = new GameObject("Crystal3D_ProofRoot");

            GameObject crystal = new GameObject("Real volumetric faceted crystal mesh");
            crystal.transform.SetParent(root.transform, false);
            MeshFilter meshFilter = crystal.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            MeshRenderer meshRenderer = crystal.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = glassMaterial;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            meshRenderer.receiveShadows = false;
            Crystal3DProofMaterialController materialController = crystal.AddComponent<Crystal3DProofMaterialController>();
            materialController.Configure(glassMaterial, debugMaterial);
            crystal.AddComponent<Crystal3DProofValidator>();

            GameObject cameraObject = new GameObject("Perspective Camera");
            cameraObject.transform.SetParent(root.transform, false);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.015f, 0.018f, 0.024f, 1f);
            camera.fieldOfView = 38f;
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 50f;
            camera.allowHDR = true;
            Crystal3DProofCameraOrbit orbit = cameraObject.AddComponent<Crystal3DProofCameraOrbit>();
            orbit.SetTarget(crystal.transform);

            GameObject directionalLight = new GameObject("Directional Light");
            directionalLight.transform.SetParent(root.transform, false);
            directionalLight.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
            Light sun = directionalLight.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(0.82f, 0.9f, 1f, 1f);
            sun.intensity = 0.65f;
            sun.shadows = LightShadows.Soft;

            CreatePointLight(root.transform, "Crystal3D_PointLight_Key", new Vector3(-2.2f, 1.4f, -1.6f), new Color(1f, 0.9f, 0.74f, 1f), 2.4f);
            CreatePointLight(root.transform, "Crystal3D_PointLight_CyanRim", new Vector3(2.4f, 0.35f, 1.2f), new Color(0.28f, 0.9f, 1f, 1f), 1.8f);
            CreatePointLight(root.transform, "Crystal3D_PointLight_VioletRim", new Vector3(0.2f, -0.85f, 2.5f), new Color(0.72f, 0.42f, 1f, 1f), 1.55f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.055f, 0.062f, 0.075f, 1f);
            RenderSettings.skybox = null;

            Selection.activeGameObject = crystal;
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Vector3 bounds = mesh.bounds.size;
            Debug.Log("[Crystal3DProofSceneBuilder] Created " + ScenePath
                + " vertices " + mesh.vertexCount.ToString()
                + ", triangles " + (mesh.triangles.Length / 3).ToString()
                + ", bounds " + bounds.x.ToString("0.00") + "x" + bounds.y.ToString("0.00") + "x" + bounds.z.ToString("0.00")
                + ", hasVolume " + (Crystal3DProofMeshFactory.HasVolume(mesh) ? "true" : "false")
                + ", bounds.z > 0.5 " + (Crystal3DProofMeshFactory.DepthIsValid(mesh) ? "true" : "false"));
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            Directory.CreateDirectory(ProofFolder);
            Directory.CreateDirectory(MaterialFolder);
        }

        private static Mesh CreateOrReplaceMeshAsset()
        {
            Mesh existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
            if (existingMesh != null)
            {
                AssetDatabase.DeleteAsset(MeshPath);
            }

            Mesh mesh = Crystal3DProofMeshFactory.CreateCrystalMesh();
            AssetDatabase.CreateAsset(mesh, MeshPath);
            return mesh;
        }

        private static Material CreateOrReplaceGlassMaterial()
        {
            Material material = CreateMaterialAsset(GlassMaterialPath, Shader.Find("Standard"));
            material.name = "Crystal3D_Proof_TransparentBlueGlass";
            material.SetColor("_Color", new Color(0.35f, 0.82f, 1f, 0.34f));
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Glossiness", 0.96f);
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return material;
        }

        private static Material CreateOrReplaceDebugMaterial()
        {
            Material material = CreateMaterialAsset(DebugMaterialPath, Shader.Find("Unlit/Color"));
            material.name = "Crystal3D_Proof_UnlitSolidDebug";
            material.SetColor("_Color", new Color(0.1f, 0.52f, 1f, 1f));
            material.renderQueue = -1;
            return material;
        }

        private static Material CreateMaterialAsset(string path, Shader shader)
        {
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material existingMaterial = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existingMaterial != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            Material material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void CreatePointLight(Transform parent, string objectName, Vector3 position, Color color, float intensity)
        {
            GameObject lightObject = new GameObject(objectName);
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = position;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = 6f;
            light.shadows = LightShadows.None;
        }
    }
}
#endif
