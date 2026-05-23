using UnityEngine;

namespace Kaleidoscope2.Menu
{
    internal sealed class KaelisMenuAnimator : MonoBehaviour
    {
        [SerializeField] private float shimmerSpeed = 0.035f;
        [SerializeField] private float shimmerRange = 16f;

        private RectTransform shimmerLayer;
        private Vector2 baseOffsetMin;
        private Vector2 baseOffsetMax;

        public void BindShimmerLayer(RectTransform layer)
        {
            shimmerLayer = layer;
            if (shimmerLayer == null)
            {
                return;
            }

            baseOffsetMin = shimmerLayer.offsetMin;
            baseOffsetMax = shimmerLayer.offsetMax;
        }

        private void Update()
        {
            if (shimmerLayer == null)
            {
                return;
            }

            float offset = Mathf.Sin(Time.unscaledTime * shimmerSpeed) * shimmerRange;
            shimmerLayer.offsetMin = baseOffsetMin + new Vector2(offset, 0f);
            shimmerLayer.offsetMax = baseOffsetMax + new Vector2(offset, 0f);
        }
    }
}
