using System;
using UnityEngine;

namespace Kaleidoscope2.Core
{
    [Serializable]
    public sealed class CrystalSharedSettings : ISerializationCallbackReceiver
    {
        private const float RealMeshScaleMinimum = 3f;
        private const float RealMeshScaleMaximum = 5f;
        private const float RealMeshScaleDefault = 3.6f;
        private const float CrystalDistanceDefault = 14f;

        [SerializeField] private CrystalRenderMode renderMode = CrystalRenderMode.Billboard2D;
        [SerializeField] private bool visible = true;
        [SerializeField] private CrystalShape shape = CrystalShape.ClassicDiamond;
        [SerializeField] private CrystalMaterialMode materialMode = CrystalMaterialMode.Diamond;
        [SerializeField] private Vector3 rotation;
        [SerializeField, Range(0f, 20f)] private float intensity = 8f;
        [SerializeField, Range(3f, 5f)] private float realMeshScale = 3.6f;
        [SerializeField, Range(4f, 16f)] private float crystalDistanceFromCamera = 14f;
        [SerializeField] private bool realMeshScreenCenter = true;
        [SerializeField] private Vector2 crystalScreenCenterOffset;
        [SerializeField] private bool useSolidGeometryValidationMaterial;
        [SerializeField] private bool cameraOrbitEnabled;
        [SerializeField] private bool crystalRotationEnabled = true;
        [SerializeField, Range(CrystalLightRigSettings.ActiveLightCountMin, CrystalLightRigSettings.ActiveLightCountMax)] private int realMeshLightCount = CrystalLightRigSettings.ActiveLightCountMax;
        [SerializeField] private bool crystalLightRigEnabled = true;
        [SerializeField, Range(0f, 1f)] private float realMeshAlpha = 0.58f;
        [SerializeField, Range(0f, 1f)] private float transparency;
        [SerializeField, Range(0f, 0.12f)] private float refractionStrength = 0.04f;
        [SerializeField, Range(0.5f, 8f)] private float fresnelPower = 3.2f;
        [SerializeField, Range(0f, 1f)] private float reflectionStrength = 0.6f;
        [SerializeField, Range(0f, 3f)] private float internalBrightness = 1f;
        [SerializeField, Range(0f, 1f)] private float minimumTransmission = 0.1f;
        [SerializeField, Range(0f, 1f)] private float specularStrength = 0.75f;
        [SerializeField] private string sourceTextureStatus = "Unbound";
        [NonSerialized] private bool realMeshScaleMigrationPending;
        [NonSerialized] private float realMeshScaleMigrationSource;

        public CrystalRenderMode RenderMode { get { return renderMode; } }
        public bool Visible { get { return visible; } }
        public CrystalShape Shape { get { return shape; } }
        public CrystalMaterialMode MaterialMode { get { return materialMode; } }
        public Vector3 Rotation { get { return rotation; } }
        public float Intensity { get { return Mathf.Clamp(intensity, 0f, 20f); } }
        public float RealMeshScale { get { return Mathf.Clamp(realMeshScale, RealMeshScaleMinimum, RealMeshScaleMaximum); } }
        public float CrystalDistanceFromCamera { get { return Mathf.Clamp(crystalDistanceFromCamera, 4f, 16f); } }
        public bool RealMeshScreenCenter { get { return realMeshScreenCenter; } }
        public Vector2 CrystalScreenCenterOffset { get { return realMeshScreenCenter ? Vector2.zero : Vector2.ClampMagnitude(crystalScreenCenterOffset, 1.5f); } }
        public bool UseSolidGeometryValidationMaterial { get { return useSolidGeometryValidationMaterial; } }
        public bool CameraOrbitEnabled { get { return cameraOrbitEnabled; } }
        public bool CrystalRotationEnabled { get { return crystalRotationEnabled; } }
        public int RealMeshLightCount { get { return Mathf.Clamp(realMeshLightCount, CrystalLightRigSettings.ActiveLightCountMin, CrystalLightRigSettings.ActiveLightCountMax); } }
        public bool CrystalLightRigEnabled { get { return crystalLightRigEnabled; } }
        public string RealMeshPlacementStatus
        {
            get
            {
                Vector2 offset = CrystalScreenCenterOffset;
                return "scale " + RealMeshScale.ToString("0.00")
                    + ", distance " + CrystalDistanceFromCamera.ToString("0.00")
                    + ", centered " + (RealMeshScreenCenter ? "true" : "false")
                    + ", offset " + offset.x.ToString("0.00") + "," + offset.y.ToString("0.00")
                    + ", camera orbit " + (CameraOrbitEnabled ? "enabled" : "disabled")
                    + ", crystal rotation " + (CrystalRotationEnabled ? "enabled" : "disabled")
                    + ", light rig " + (CrystalLightRigEnabled ? "enabled" : "disabled")
                    + ", light count " + RealMeshLightCount.ToString();
            }
        }
        public float RealMeshAlpha { get { return Mathf.Clamp01(realMeshAlpha); } }
        public float Transparency { get { return Mathf.Clamp01(transparency); } }
        public float RefractionStrength { get { return Mathf.Clamp(refractionStrength, 0f, 0.12f); } }
        public float FresnelPower { get { return Mathf.Clamp(fresnelPower, 0.5f, 8f); } }
        public float ReflectionStrength { get { return Mathf.Clamp01(reflectionStrength); } }
        public float InternalBrightness { get { return Mathf.Clamp(internalBrightness, 0f, 3f); } }
        public float MinimumTransmission { get { return Mathf.Clamp(minimumTransmission, 0f, 1f); } }
        public float SpecularStrength { get { return Mathf.Clamp01(specularStrength); } }
        public string SourceTextureStatus { get { return sourceTextureStatus; } }
        public string OpticalMaterialStatus
        {
            get
            {
                return "fresnel " + FresnelPower.ToString("0.00")
                    + ", refraction " + RefractionStrength.ToString("0.000")
                    + ", reflection " + ReflectionStrength.ToString("0.00")
                    + ", internal " + InternalBrightness.ToString("0.00")
                    + ", min transmission " + MinimumTransmission.ToString("0.00");
            }
        }

        public void SetRenderMode(CrystalRenderMode value)
        {
            renderMode = value == CrystalRenderMode.RealMesh3D
                ? CrystalRenderMode.RealMesh3D
                : CrystalRenderMode.Billboard2D;
        }

        public void ToggleRenderMode()
        {
            SetRenderMode(renderMode == CrystalRenderMode.RealMesh3D
                ? CrystalRenderMode.Billboard2D
                : CrystalRenderMode.RealMesh3D);
        }

        public void SyncFromDiamond(DiamondFocusSettings diamondSettings)
        {
            if (diamondSettings == null)
            {
                visible = false;
                sourceTextureStatus = "Missing diamond settings";
                return;
            }

            renderMode = diamondSettings.CrystalSimulationMode;
            visible = diamondSettings.Enabled;
            shape = FromDiamondShape(diamondSettings.Shape);
            materialMode = FromDiamondMaterialMode(diamondSettings.MaterialMode);
            rotation = diamondSettings.RotationEuler;
            CrystalLightRigSettings lightRigSettings = diamondSettings.CrystalLightRigSettings;
            intensity = lightRigSettings.LightIntensity;
            realMeshLightCount = lightRigSettings.ActiveLightCountLimit;
            crystalLightRigEnabled = lightRigSettings.RigEnabled;
            transparency = diamondSettings.Transparency;
            refractionStrength = ResolveSafeRefractionStrength(diamondSettings);
            fresnelPower = diamondSettings.FresnelPower;
            reflectionStrength = diamondSettings.ReflectionStrength;
            internalBrightness = diamondSettings.InternalBrightness;
            minimumTransmission = Mathf.Clamp(0.08f + diamondSettings.DirectTransmission * 0.58f, 0.06f, 0.42f);
            specularStrength = Mathf.Clamp01(0.28f + diamondSettings.ReflectionStrength * 0.68f);
            sourceTextureStatus = diamondSettings.KaleidoscopeTexBindingStatus;
        }

        public bool EnsureRealMeshProductionDefaults(out string migrationMessage)
        {
            migrationMessage = string.Empty;
            float previousScale = realMeshScale;
            bool migrated = ApplyRealMeshProductionDefaults(out previousScale);

            if (realMeshScaleMigrationPending)
            {
                previousScale = realMeshScaleMigrationSource;
                migrated = true;
                realMeshScaleMigrationPending = false;
            }

            CreateRealMeshMigrationMessage(migrated, previousScale, out migrationMessage);
            return migrated;
        }

        public void OnBeforeSerialize()
        {
            float previousScale;
            ApplyRealMeshProductionDefaults(out previousScale);
        }

        public void OnAfterDeserialize()
        {
            float previousScale;
            bool migrated = ApplyRealMeshProductionDefaults(out previousScale);
            if (migrated)
            {
                realMeshScaleMigrationPending = true;
                realMeshScaleMigrationSource = previousScale;
            }
        }

        private bool ApplyRealMeshProductionDefaults(out float previousScale)
        {
            previousScale = realMeshScale;
            bool migrated = false;
            if (float.IsNaN(realMeshScale) || float.IsInfinity(realMeshScale) || realMeshScale < RealMeshScaleMinimum)
            {
                realMeshScale = RealMeshScaleDefault;
                migrated = true;
            }
            else
            {
                realMeshScale = Mathf.Clamp(realMeshScale, RealMeshScaleMinimum, RealMeshScaleMaximum);
            }

            if (float.IsNaN(crystalDistanceFromCamera) || float.IsInfinity(crystalDistanceFromCamera) || crystalDistanceFromCamera <= 0f)
            {
                crystalDistanceFromCamera = CrystalDistanceDefault;
            }

            if (realMeshScreenCenter)
            {
                crystalScreenCenterOffset = Vector2.zero;
            }

            return migrated;
        }

        private static void CreateRealMeshMigrationMessage(bool migrated, float previousScale, out string migrationMessage)
        {
            migrationMessage = migrated
                ? "Migrated RealMesh3D realMeshScale from "
                    + previousScale.ToString("0.00")
                    + " to production default "
                    + RealMeshScaleDefault.ToString("0.00")
                    + "."
                : string.Empty;
        }

        public void SetSourceTextureStatus(bool bound)
        {
            sourceTextureStatus = bound ? "Bound" : "Missing / fallback";
        }

        public static string GetRenderModeLabel(CrystalRenderMode value)
        {
            return value == CrystalRenderMode.RealMesh3D ? "3D Premium" : "2D Performance";
        }

        private static CrystalShape FromDiamondShape(DiamondFocusShape value)
        {
            switch (value)
            {
                case DiamondFocusShape.FacetedCube:
                    return CrystalShape.FacetedCube;
                case DiamondFocusShape.DiscoBall:
                    return CrystalShape.DiscoBall;
                case DiamondFocusShape.TetrahedralCrystal:
                    return CrystalShape.TetrahedralCrystal;
                case DiamondFocusShape.RhombicCrystal:
                    return CrystalShape.RhombicCrystal;
                case DiamondFocusShape.OvalRingGem:
                    return CrystalShape.OvalRingGem;
                default:
                    return CrystalShape.ClassicDiamond;
            }
        }

        private static CrystalMaterialMode FromDiamondMaterialMode(DiamondCrystalMaterialMode value)
        {
            switch (value)
            {
                case DiamondCrystalMaterialMode.AbsoluteMirror:
                    return CrystalMaterialMode.AbsoluteMirror;
                case DiamondCrystalMaterialMode.FuturisticPlastic:
                    return CrystalMaterialMode.FuturisticPlastic;
                case DiamondCrystalMaterialMode.Mercury:
                case DiamondCrystalMaterialMode.StainlessSteel:
                case DiamondCrystalMaterialMode.Chrome:
                case DiamondCrystalMaterialMode.CastIron:
                case DiamondCrystalMaterialMode.PolishedBrass:
                    return CrystalMaterialMode.Metal;
                case DiamondCrystalMaterialMode.Emerald:
                case DiamondCrystalMaterialMode.Topaz:
                case DiamondCrystalMaterialMode.Ruby:
                case DiamondCrystalMaterialMode.Sapphire:
                case DiamondCrystalMaterialMode.Amethyst:
                case DiamondCrystalMaterialMode.Aquamarine:
                case DiamondCrystalMaterialMode.Garnet:
                    return CrystalMaterialMode.Gemstone;
                default:
                    return CrystalMaterialMode.Diamond;
            }
        }

        private static float ResolveSafeRefractionStrength(DiamondFocusSettings diamondSettings)
        {
            float coefficient01 = Mathf.InverseLerp(0f, 10f, diamondSettings.RefractionCoefficient);
            float mapped = diamondSettings.RefractionStrength * Mathf.Lerp(0.35f, 1f, coefficient01);
            return Mathf.Clamp(mapped, 0f, 0.085f);
        }
    }
}
