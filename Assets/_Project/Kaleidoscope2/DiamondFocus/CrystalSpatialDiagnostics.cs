using UnityEngine;

namespace Kaleidoscope2.DiamondFocus
{
    public static class CrystalSpatialDiagnostics
    {
        public const float BillboardTargetMinCoverage = 0.22f;
        public const float BillboardTargetMaxCoverage = 0.30f;
        public const float BillboardTargetCoverage = 0.26f;

        public const float RealMeshTargetMinCoverage = 0.35f;
        public const float RealMeshTargetMaxCoverage = 0.45f;
        public const float RealMeshTargetCoverage = 0.40f;

        public static float ClampBillboardCoverage(float value)
        {
            return Mathf.Clamp(value, BillboardTargetMinCoverage, BillboardTargetMaxCoverage);
        }

        public static float ClampRealMeshCoverage(float value)
        {
            return Mathf.Clamp(value, RealMeshTargetMinCoverage, RealMeshTargetMaxCoverage);
        }

        public static float OrthographicHeightCoverage(float worldHeight, float orthographicSize)
        {
            float fullHeight = Mathf.Max(0.0001f, orthographicSize * 2f);
            return Mathf.Clamp01(Mathf.Max(0f, worldHeight) / fullHeight);
        }

        public static float PerspectiveHeightCoverage(float worldHeight, float cameraDistance, float fieldOfView)
        {
            float safeDistance = Mathf.Max(0.0001f, cameraDistance);
            float halfFovRadians = Mathf.Clamp(fieldOfView, 1f, 179f) * 0.5f * Mathf.Deg2Rad;
            float fullHeight = Mathf.Max(0.0001f, Mathf.Tan(halfFovRadians) * safeDistance * 2f);
            return Mathf.Clamp01(Mathf.Max(0f, worldHeight) / fullHeight);
        }

        public static float ViewportHeightCoverage(Camera camera, Bounds worldBounds)
        {
            if (camera == null || worldBounds.size == Vector3.zero)
            {
                return 0f;
            }

            Vector3 center = worldBounds.center;
            Vector3 extents = worldBounds.extents;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            bool hasVisibleCorner = false;

            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 corner = center + Vector3.Scale(extents, new Vector3(x, y, z));
                        Vector3 viewport = camera.WorldToViewportPoint(corner);
                        if (viewport.z <= 0f)
                        {
                            continue;
                        }

                        hasVisibleCorner = true;
                        minY = Mathf.Min(minY, viewport.y);
                        maxY = Mathf.Max(maxY, viewport.y);
                    }
                }
            }

            return hasVisibleCorner ? Mathf.Clamp01(maxY - minY) : 0f;
        }

        public static Bounds TransformBounds(Bounds localBounds, Transform transform)
        {
            if (transform == null)
            {
                return localBounds;
            }

            Vector3 center = localBounds.center;
            Vector3 extents = localBounds.extents;
            Bounds worldBounds = new Bounds(transform.TransformPoint(center), Vector3.zero);

            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 corner = center + Vector3.Scale(extents, new Vector3(x, y, z));
                        worldBounds.Encapsulate(transform.TransformPoint(corner));
                    }
                }
            }

            return worldBounds;
        }

        public static string FormatPercent(float value)
        {
            return (Mathf.Clamp01(value) * 100f).ToString("0.0") + "%";
        }

        public static string FormatVector(Vector3 value)
        {
            return value.x.ToString("0.00") + "," + value.y.ToString("0.00") + "," + value.z.ToString("0.00");
        }

        public static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return "none";
            }

            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }
    }
}
