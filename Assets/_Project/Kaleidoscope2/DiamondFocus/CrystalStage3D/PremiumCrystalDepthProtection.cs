using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.CrystalStage3D
{
    public struct PremiumCrystalDepthCorrection
    {
        public PremiumCrystalDepthCorrection(
            bool active,
            float requiredCameraForwardOffset,
            float rawPenetration,
            float effectiveMaxCameraForwardOffset,
            float protectedPlaneDepth,
            float backMostDepth,
            Vector3 protectedPlanePoint,
            Vector3 backMostPoint,
            float safeDepthClearance)
        {
            Active = active;
            RequiredCameraForwardOffset = requiredCameraForwardOffset;
            RawPenetration = rawPenetration;
            EffectiveMaxCameraForwardOffset = effectiveMaxCameraForwardOffset;
            ProtectedPlaneDepth = protectedPlaneDepth;
            BackMostDepth = backMostDepth;
            ProtectedPlanePoint = protectedPlanePoint;
            BackMostPoint = backMostPoint;
            SafeDepthClearance = safeDepthClearance;
        }

        public bool Active { get; private set; }
        public float RequiredCameraForwardOffset { get; private set; }
        public float RawPenetration { get; private set; }
        public float EffectiveMaxCameraForwardOffset { get; private set; }
        public float ProtectedPlaneDepth { get; private set; }
        public float BackMostDepth { get; private set; }
        public Vector3 ProtectedPlanePoint { get; private set; }
        public Vector3 BackMostPoint { get; private set; }
        public float SafeDepthClearance { get; private set; }
    }

    public static class PremiumCrystalDepthProtection
    {
        public const bool EnablePremiumDepthProtection = true;
        public const float SafeDepthClearance = 0.18f;
        public const float MaxCameraForwardOffset = 3.5f;
        public const float DepthCorrectionSmoothTime = 0.18f;

        public static PremiumCrystalDepthCorrection Resolve(
            Bounds worldBounds,
            Vector3 cameraPosition,
            Vector3 cameraForward,
            Vector3 protectedPlanePoint)
        {
            if (!EnablePremiumDepthProtection || worldBounds.size == Vector3.zero || cameraForward.sqrMagnitude <= 0.0001f)
            {
                return new PremiumCrystalDepthCorrection(false, 0f, 0f, MaxCameraForwardOffset, 0f, 0f, protectedPlanePoint, worldBounds.center, SafeDepthClearance);
            }

            Vector3 forward = cameraForward.normalized;
            float protectedPlaneDepth = Vector3.Dot(forward, protectedPlanePoint - cameraPosition) - SafeDepthClearance;
            Vector3 backMostPoint = ResolveBackMostPoint(worldBounds, cameraPosition, forward);
            float backMostDepth = Vector3.Dot(forward, backMostPoint - cameraPosition);
            float rawPenetration = Mathf.Max(0f, backMostDepth - protectedPlaneDepth);
            float effectiveMaxOffset = Mathf.Max(MaxCameraForwardOffset, rawPenetration);
            float requiredOffset = Mathf.Clamp(rawPenetration, 0f, effectiveMaxOffset);
            return new PremiumCrystalDepthCorrection(
                requiredOffset > 0.0001f,
                requiredOffset,
                rawPenetration,
                effectiveMaxOffset,
                protectedPlaneDepth,
                backMostDepth,
                protectedPlanePoint,
                backMostPoint,
                SafeDepthClearance);
        }

        public static string FormatDiagnostics(string targetName, PremiumCrystalDepthCorrection correction, float appliedOffset)
        {
            return targetName
                + " active " + (correction.Active ? "true" : "false")
                + " required " + correction.RequiredCameraForwardOffset.ToString("0.000")
                + " rawPenetration " + correction.RawPenetration.ToString("0.000")
                + " effectiveMax " + correction.EffectiveMaxCameraForwardOffset.ToString("0.000")
                + " applied " + appliedOffset.ToString("0.000")
                + " backDepth " + correction.BackMostDepth.ToString("0.000")
                + " protectedDepth " + correction.ProtectedPlaneDepth.ToString("0.000");
        }

        private static Vector3 ResolveBackMostPoint(Bounds bounds, Vector3 cameraPosition, Vector3 cameraForward)
        {
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;
            float maxDepth = float.NegativeInfinity;
            Vector3 backMostPoint = center;
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 corner = center + Vector3.Scale(extents, new Vector3(x, y, z));
                        float depth = Vector3.Dot(cameraForward, corner - cameraPosition);
                        if (depth > maxDepth)
                        {
                            maxDepth = depth;
                            backMostPoint = corner;
                        }
                    }
                }
            }

            return backMostPoint;
        }
    }
}
