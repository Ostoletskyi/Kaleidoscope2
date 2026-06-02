using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.CrystalStage3D
{
    public struct PremiumComfortFormationLayoutResult
    {
        public PremiumComfortFormationLayoutResult(
            float localRadius,
            float viewportRadius,
            float childScale,
            string diagnostics)
        {
            LocalRadius = localRadius;
            ViewportRadius = viewportRadius;
            ChildScale = childScale;
            Diagnostics = diagnostics;
        }

        public float LocalRadius { get; private set; }
        public float ViewportRadius { get; private set; }
        public float ChildScale { get; private set; }
        public string Diagnostics { get; private set; }

        public static PremiumComfortFormationLayoutResult Inactive
        {
            get
            {
                return new PremiumComfortFormationLayoutResult(
                    0f,
                    0f,
                    PremiumComfortFormationLayout.DesiredChildScale,
                    "comfort formation inactive");
            }
        }
    }

    public static class PremiumComfortFormationLayout
    {
        public const int CopyCount = 6;
        public const float SafeMargin = 0.15f;
        public const float DesiredChildScale = 0.58f;
        public const float MinimumChildScale = 0.06f;
        public const float TargetChildViewportSize = 0.24f;
        public const float MinimumReadableChildViewportSize = 0.18f;
        public const float MaximumLocalOrbitRadius = 2.8f;
        public const float BalancedViewportRadius = 0.23f;
        public const float SeparationMultiplier = 1.12f;
        public const float SafeRadiusUse = 0.98f;

        private const float MinimumValue = 0.0001f;

        public static PremiumComfortFormationLayoutResult Resolve(
            Vector2 viewportSizeAtScaleOne,
            Vector2 viewportCenter,
            float viewWidth,
            float viewHeight,
            float stageScale)
        {
            Vector2 adaptiveLayout = ResolveAdaptiveViewportLayout(viewportSizeAtScaleOne, viewportCenter, SafeMargin);
            float childScale = adaptiveLayout.x;
            Vector2 childHalfExtent = viewportSizeAtScaleOne * childScale * 0.5f;
            float safeViewportRadius = ResolveSafeViewportRadius(viewportCenter, childHalfExtent, SafeMargin);
            float viewportRadius = Mathf.Min(adaptiveLayout.y, safeViewportRadius);
            float safeStageScale = Mathf.Max(MinimumValue, stageScale);
            float worldRadiusByX = viewportRadius * Mathf.Max(MinimumValue, viewWidth) / safeStageScale;
            float worldRadiusByY = viewportRadius * Mathf.Max(MinimumValue, viewHeight) / safeStageScale;
            float localRadius = Mathf.Clamp(Mathf.Min(worldRadiusByX, worldRadiusByY), 0f, MaximumLocalOrbitRadius);

            string diagnostics = "comfort formation SixCopyOrbitFormation"
                + ", copies " + CopyCount.ToString()
                + ", size-aware layout true"
                + ", safe viewport margin " + (SafeMargin * 100f).ToString("0") + "%"
                + ", target child viewport " + TargetChildViewportSize.ToString("0.00")
                + ", viewport radius " + viewportRadius.ToString("0.000")
                + ", local radius " + localRadius.ToString("0.00")
                + ", child scale " + childScale.ToString("0.00");
            return new PremiumComfortFormationLayoutResult(localRadius, viewportRadius, childScale, diagnostics);
        }

        public static float ResolveSafeViewportRadius(Vector2 viewportCenter, Vector2 childHalfExtent, float safeMargin)
        {
            float margin = Mathf.Clamp(safeMargin, 0f, 0.49f);
            float radiusX = Mathf.Min(
                viewportCenter.x - margin - Mathf.Max(0f, childHalfExtent.x),
                1f - margin - viewportCenter.x - Mathf.Max(0f, childHalfExtent.x));
            float radiusY = Mathf.Min(
                viewportCenter.y - margin - Mathf.Max(0f, childHalfExtent.y),
                1f - margin - viewportCenter.y - Mathf.Max(0f, childHalfExtent.y));
            return Mathf.Max(0f, Mathf.Min(radiusX, radiusY));
        }

        public static Vector2 ResolveAdaptiveViewportLayout(Vector2 viewportSizeAtScaleOne, Vector2 viewportCenter, float safeMargin)
        {
            Vector2 sizeAtScaleOne = new Vector2(
                Mathf.Max(0f, Mathf.Abs(viewportSizeAtScaleOne.x)),
                Mathf.Max(0f, Mathf.Abs(viewportSizeAtScaleOne.y)));
            float fitScaleX = sizeAtScaleOne.x > MinimumValue
                ? ResolveSafeHalfAxis(viewportCenter.x, safeMargin) * 2f / sizeAtScaleOne.x
                : DesiredChildScale;
            float fitScaleY = sizeAtScaleOne.y > MinimumValue
                ? ResolveSafeHalfAxis(viewportCenter.y, safeMargin) * 2f / sizeAtScaleOne.y
                : DesiredChildScale;
            float maxDimensionAtScaleOne = Mathf.Max(sizeAtScaleOne.x, sizeAtScaleOne.y);
            float readableScale = maxDimensionAtScaleOne > MinimumValue
                ? TargetChildViewportSize / maxDimensionAtScaleOne
                : DesiredChildScale;
            float childScale = Mathf.Clamp(
                Mathf.Min(DesiredChildScale, readableScale, fitScaleX, fitScaleY),
                MinimumChildScale,
                DesiredChildScale);
            float viewportRadius = 0f;

            for (int iteration = 0; iteration < 3; iteration++)
            {
                Vector2 childHalfExtent = sizeAtScaleOne * childScale * 0.5f;
                float safeRadius = ResolveSafeViewportRadius(viewportCenter, childHalfExtent, safeMargin) * SafeRadiusUse;
                float separationRadius = maxDimensionAtScaleOne * childScale * SeparationMultiplier;
                float childViewportSize = maxDimensionAtScaleOne * childScale;
                float size01 = Mathf.Clamp01(childViewportSize / TargetChildViewportSize);
                float balancedRadius = Mathf.Min(Mathf.Lerp(BalancedViewportRadius * 0.55f, BalancedViewportRadius, size01), safeRadius);
                viewportRadius = Mathf.Clamp(Mathf.Max(balancedRadius, separationRadius), 0f, safeRadius);

                if (separationRadius <= safeRadius || maxDimensionAtScaleOne <= MinimumValue)
                {
                    break;
                }

                float fittedScale = safeRadius / (maxDimensionAtScaleOne * SeparationMultiplier);
                float nextScale = Mathf.Clamp(Mathf.Min(childScale, fittedScale), MinimumChildScale, DesiredChildScale);
                if (Mathf.Abs(nextScale - childScale) < 0.0005f)
                {
                    break;
                }

                childScale = nextScale;
            }

            return new Vector2(childScale, Mathf.Max(0f, viewportRadius));
        }

        public static Vector3 ResolveOrbitOffset(int index, float orbitAngleRadians, float radius)
        {
            float angle = index * Mathf.PI * 2f / CopyCount + orbitAngleRadians;
            float drift = Mathf.Sin(orbitAngleRadians * 1.5f + index * 0.9f) * Mathf.Min(0.025f, radius * 0.04f);
            float resolvedRadius = Mathf.Clamp(radius + drift, 0f, Mathf.Max(0f, radius));
            return new Vector3(Mathf.Cos(angle) * resolvedRadius, Mathf.Sin(angle) * resolvedRadius, 0f);
        }

        public static Vector3 ResolveRotationOffset(int index, float orbitAngleRadians)
        {
            float orbitDegrees = orbitAngleRadians * Mathf.Rad2Deg;
            return new Vector3(
                (index - 2.5f) * 2.2f,
                (index % 3 - 1) * 4.5f,
                orbitDegrees + (index % 2 == 0 ? 8f : -7f));
        }

        private static float ResolveSafeHalfAxis(float center, float safeMargin)
        {
            float margin = Mathf.Clamp(safeMargin, 0f, 0.49f);
            return Mathf.Max(0f, Mathf.Min(center - margin, 1f - margin - center));
        }
    }
}
