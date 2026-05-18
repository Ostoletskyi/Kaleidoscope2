using System;
using Kaleidoscope2.Core;
using Kaleidoscope2.DiamondFocus;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.RealMesh.CrystalStage3D
{
    public enum CrystalStage3DDebugMode
    {
        SolidLitGeometry = 0,
        TransparentGlassOnly = 1,
        ReflectionEnvironmentOnly = 2,
        RefractionEnvironmentOnly = 3,
        FinalPremiumComposite = 4
    }

    [Serializable]
    public sealed class CrystalStage3DSettings
    {
        [SerializeField] private CrystalStage3DDebugMode debugMode = CrystalStage3DDebugMode.FinalPremiumComposite;
        [SerializeField, Range(3f, 5f)] private float crystalScale = 3.6f;
        [SerializeField] private bool realMeshScreenCenter = true;
        [SerializeField] private Vector2 crystalScreenCenterOffset;
        [SerializeField, Range(20f, 60f)] private float cameraFieldOfView = 34f;
        [SerializeField, Range(4f, 64f)] private float cameraDistance = 14f;
        [SerializeField, Range(CrystalSpatialDiagnostics.RealMeshTargetMinCoverage, CrystalSpatialDiagnostics.RealMeshTargetMaxCoverage)] private float targetFrameHeight = CrystalSpatialDiagnostics.RealMeshTargetCoverage;
        [SerializeField, Range(-1f, 1f)] private float cameraHeight = 0.05f;
        [SerializeField, Range(0f, 0.8f)] private float cameraOrbitRadius = 0.22f;
        [SerializeField, Range(0f, 1.5f)] private float cameraOrbitSpeed = 0.18f;
        [SerializeField] private bool cameraOrbitEnabled;
        [SerializeField] private bool crystalRotationEnabled = true;
        [SerializeField, Range(1.5f, 80f)] private float backgroundDistance = 17f;
        [SerializeField, Range(2f, 80f)] private float backgroundScale = 12f;
        [SerializeField, Range(0f, 1f)] private float backgroundAlpha = 0.9f;
        [SerializeField, Range(0f, 20f)] private float lightIntensity = 8f;
        [SerializeField, Range(CrystalLightRigSettings.ActiveLightCountMin, CrystalLightRigSettings.ActiveLightCountMax)] private int activeLightCount = CrystalLightRigSettings.ActiveLightCountMax;
        [SerializeField] private bool lightRigEnabled = true;

        [NonSerialized] private float crystalWorldHeight;

        public CrystalStage3DDebugMode DebugMode
        {
            get { return debugMode; }
            set { debugMode = value; }
        }

        public float CrystalScale
        {
            get { return Mathf.Clamp(crystalScale, 3f, 5f); }
        }

        public Vector2 CrystalScreenCenterOffset
        {
            get { return realMeshScreenCenter ? Vector2.zero : Vector2.ClampMagnitude(crystalScreenCenterOffset, 1.5f); }
        }

        public bool RealMeshScreenCenter
        {
            get { return realMeshScreenCenter; }
        }

        public float CameraFieldOfView
        {
            get { return Mathf.Clamp(cameraFieldOfView, 20f, 60f); }
        }

        public float CameraDistance
        {
            get { return ResolveCameraDistance(); }
        }

        public float BaseCameraDistance
        {
            get { return Mathf.Clamp(cameraDistance, 4f, 64f); }
        }

        public float TargetFrameHeight
        {
            get { return CrystalSpatialDiagnostics.ClampRealMeshCoverage(targetFrameHeight); }
        }

        public float CameraHeight
        {
            get { return Mathf.Clamp(cameraHeight, -1f, 1f); }
        }

        public float CameraOrbitRadius
        {
            get { return CameraOrbitEnabled ? Mathf.Clamp(cameraOrbitRadius, 0f, 0.8f) : 0f; }
        }

        public float CameraOrbitSpeed
        {
            get { return CameraOrbitEnabled ? Mathf.Clamp(cameraOrbitSpeed, 0f, 1.5f) : 0f; }
        }

        public bool CameraOrbitEnabled
        {
            get { return debugMode != CrystalStage3DDebugMode.FinalPremiumComposite && cameraOrbitEnabled; }
        }

        public bool ProductionCameraOrbitEnabled
        {
            get { return false; }
        }

        public bool CrystalRotationEnabled
        {
            get { return crystalRotationEnabled; }
        }

        public float BackgroundDistance
        {
            get { return Mathf.Clamp(Mathf.Max(backgroundDistance, CameraDistance + 3f), 1.5f, 80f); }
        }

        public float BackgroundScale
        {
            get { return Mathf.Clamp(Mathf.Max(backgroundScale, CameraDistance * 0.82f), 2f, 80f); }
        }

        public float BackgroundAlpha
        {
            get { return Mathf.Clamp01(backgroundAlpha); }
        }

        public float LightIntensity
        {
            get { return Mathf.Clamp(lightIntensity, 0f, 20f); }
            set { lightIntensity = Mathf.Clamp(value, 0f, 20f); }
        }

        public int ActiveLightCount
        {
            get { return Mathf.Clamp(activeLightCount, CrystalLightRigSettings.ActiveLightCountMin, CrystalLightRigSettings.ActiveLightCountMax); }
        }

        public bool LightRigEnabled
        {
            get { return lightRigEnabled; }
        }

        public bool ShowsBackground
        {
            get
            {
                return debugMode == CrystalStage3DDebugMode.ReflectionEnvironmentOnly
                    || debugMode == CrystalStage3DDebugMode.RefractionEnvironmentOnly;
            }
        }

        public bool UsesTransparentGlass
        {
            get { return debugMode != CrystalStage3DDebugMode.SolidLitGeometry; }
        }

        public void SyncFromShared(CrystalSharedSettings sharedSettings)
        {
            if (sharedSettings == null)
            {
                return;
            }

            crystalScale = sharedSettings.RealMeshScale;
            realMeshScreenCenter = sharedSettings.RealMeshScreenCenter;
            crystalScreenCenterOffset = sharedSettings.CrystalScreenCenterOffset;
            cameraDistance = sharedSettings.CrystalDistanceFromCamera;
            cameraOrbitEnabled = sharedSettings.CameraOrbitEnabled;
            crystalRotationEnabled = sharedSettings.CrystalRotationEnabled;
            activeLightCount = sharedSettings.RealMeshLightCount;
            lightRigEnabled = sharedSettings.CrystalLightRigEnabled;
            backgroundDistance = CameraDistance + 3f;
            backgroundScale = Mathf.Max(8f, sharedSettings.RealMeshScale * 3.2f);
        }

        public void SetCrystalWorldBounds(Vector3 localBoundsSize)
        {
            crystalWorldHeight = Mathf.Max(0f, localBoundsSize.y) * CrystalScale;
        }

        private float ResolveCameraDistance()
        {
            float resolvedDistance = BaseCameraDistance;
            if (crystalWorldHeight > 0.001f)
            {
                float halfFovRadians = CameraFieldOfView * 0.5f * Mathf.Deg2Rad;
                float denominator = 2f * Mathf.Tan(halfFovRadians) * TargetFrameHeight;
                if (denominator > 0.0001f)
                {
                    resolvedDistance = Mathf.Max(resolvedDistance, crystalWorldHeight / denominator);
                }
            }

            return Mathf.Clamp(resolvedDistance, 4f, 64f);
        }

        public string DebugModeLabel
        {
            get
            {
                switch (debugMode)
                {
                    case CrystalStage3DDebugMode.SolidLitGeometry:
                        return "Solid Lit Geometry";
                    case CrystalStage3DDebugMode.TransparentGlassOnly:
                        return "Transparent Glass Only";
                    case CrystalStage3DDebugMode.ReflectionEnvironmentOnly:
                        return "Reflection Environment Only";
                    case CrystalStage3DDebugMode.RefractionEnvironmentOnly:
                        return "Refraction Environment Only";
                    default:
                        return "Final Premium Composite";
                }
            }
        }
    }
}
