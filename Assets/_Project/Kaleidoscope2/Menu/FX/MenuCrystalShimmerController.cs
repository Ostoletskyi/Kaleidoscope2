using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu.FX
{
    [DisallowMultipleComponent]
    public sealed class MenuCrystalShimmerController : MaskableGraphic
    {
        private Material runtimeMaterial;
        private float shimmerIntensity = 0.64f;
        private float sparkleIntensity = 0.70f;
        private float sparkleSpeed = 0.45f;
        private Color coolTint = new Color(0.55f, 0.95f, 1f, 1f);
        private Color warmTint = new Color(1f, 0.72f, 0.34f, 1f);

        public override Texture mainTexture
        {
            get { return s_WhiteTexture; }
        }

        internal void AssignMaterial(Material materialInstance)
        {
            runtimeMaterial = materialInstance;
            material = runtimeMaterial;
            color = Color.white;
            raycastTarget = false;
            SetMaterialDirty();
            UpdateMaterialProperties();
        }

        internal void Configure(float newShimmerIntensity, float newSparkleIntensity, float newSparkleSpeed, Color newCoolTint, Color newWarmTint)
        {
            shimmerIntensity = Mathf.Clamp(newShimmerIntensity, 0f, 2f);
            sparkleIntensity = Mathf.Clamp(newSparkleIntensity, 0f, 2f);
            sparkleSpeed = Mathf.Clamp(newSparkleSpeed, 0.01f, 2.5f);
            coolTint = newCoolTint;
            warmTint = newWarmTint;
            UpdateMaterialProperties();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            raycastTarget = false;
            UpdateMaterialProperties();
        }

        private void Update()
        {
            UpdateMaterialProperties();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Rect rect = rectTransform.rect;
            int vertexIndex = vh.currentVertCount;
            Color32 vertexColor = color;

            vh.AddVert(new Vector3(rect.xMin, rect.yMin, 0f), vertexColor, new Vector2(0f, 0f));
            vh.AddVert(new Vector3(rect.xMin, rect.yMax, 0f), vertexColor, new Vector2(0f, 1f));
            vh.AddVert(new Vector3(rect.xMax, rect.yMax, 0f), vertexColor, new Vector2(1f, 1f));
            vh.AddVert(new Vector3(rect.xMax, rect.yMin, 0f), vertexColor, new Vector2(1f, 0f));
            vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
            vh.AddTriangle(vertexIndex, vertexIndex + 2, vertexIndex + 3);
        }

        private void UpdateMaterialProperties()
        {
            if (runtimeMaterial == null)
            {
                return;
            }

            float time = Application.isPlaying ? Time.unscaledTime : 0f;
            runtimeMaterial.SetFloat(MenuAtmosphereShaderIds.MenuTime, time);
            runtimeMaterial.SetVector(
                MenuAtmosphereShaderIds.ShimmerParams,
                new Vector4(shimmerIntensity, sparkleIntensity, sparkleSpeed, 0.12f));
            runtimeMaterial.SetColor(MenuAtmosphereShaderIds.ShimmerTintA, coolTint);
            runtimeMaterial.SetColor(MenuAtmosphereShaderIds.ShimmerTintB, warmTint);
        }
    }
}
