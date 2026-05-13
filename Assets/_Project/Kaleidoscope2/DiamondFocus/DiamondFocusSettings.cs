using System;
using UnityEngine;

namespace Kaleidoscope2.Core
{
    public enum DiamondFocusShape
    {
        ClassicDiamond = 0,
        FacetedCube = 1,
        TwelveFacetCrystal = 2,
        TetrahedralCrystal = 3,
        HighDetailDiamond = 4
    }

    public enum DiamondCrystalMaterialMode
    {
        AbsoluteMirror = 0,
        Diamond = 1,
        Glow = 2,
        OpticalPhysics = 3,
        GeneratedMaterial = 4
    }

    public enum DiamondGeneratedMaterialKind
    {
        Wood = 0,
        Metal = 1,
        Plastic = 2,
        Stone = 3
    }

    [Serializable]
    public sealed class DiamondFocusSettings
    {
        public const int ShapeCount = 5;
        public const int MaterialModeCount = 5;
        public const int GeneratedMaterialKindCount = 4;

        [SerializeField] private bool enabled;
        [SerializeField] private DiamondFocusShape shape = DiamondFocusShape.ClassicDiamond;
        [SerializeField] private DiamondCrystalMaterialMode materialMode = DiamondCrystalMaterialMode.Diamond;
        [SerializeField] private DiamondGeneratedMaterialKind generatedMaterialKind = DiamondGeneratedMaterialKind.Wood;
        [SerializeField] private Color generatedMaterialColor = new Color(0.85f, 0.96f, 1f, 1f);
        [SerializeField] private int generatedMaterialSeed = 1;
        [SerializeField] private Vector2 targetRotationDirection = new Vector2(0.35f, 0.75f);
        [SerializeField] private Vector2 currentRotationDirection = new Vector2(0.35f, 0.75f);
        [SerializeField] private Vector3 rotationEuler = Vector3.zero;
        [SerializeField] private Vector3 rotationVelocity = new Vector3(10f, 17f, 4f);
        [SerializeField] private float currentRotationSpeed = 20f;
        [SerializeField] private float targetRotationSpeed = 20f;

        [Header("Rotation")]
        [SerializeField] private float minRotationSpeed = 0f;
        [SerializeField] private float defaultRotationSpeed = 20f;
        [SerializeField] private float maxRotationSpeed = 240f;
        [SerializeField] private float speedAcceleration = 90f;
        [SerializeField] private float directionAcceleration = 3.5f;

        [Header("Optics")]
        [SerializeField, Range(0f, 1f)] private float transparency = 0.48f;
        [SerializeField, Range(0f, 0.12f)] private float refractionStrength = 0.042f;
        [SerializeField, Range(0f, 1f)] private float reflectionStrength = 0.42f;
        [SerializeField, Range(0.5f, 8f)] private float fresnelPower = 3.2f;
        [SerializeField, Range(0f, 2f)] private float edgeHighlight = 0.9f;
        [SerializeField, Range(0f, 0.04f)] private float chromaticAberrationBase = 0.004f;
        [SerializeField, Range(0f, 0.04f)] private float chromaticAberrationExtra = 0.012f;
        [SerializeField, Range(0f, 2f)] private float facetContrast = 1.05f;
        [SerializeField, Range(0f, 1.5f)] private float internalGlow = 0.32f;
        [SerializeField, Range(0f, 3f)] private float bloomBoostBase = 0.24f;
        [SerializeField, Range(0f, 3f)] private float bloomBoostExtra = 1.25f;
        [SerializeField, Range(0.1f, 1f)] private float screenScale = 0.38f;

        [Header("Optical Material Mode")]
        [SerializeField, Range(1f, 3f)] private float opticalIOR = 2.42f;
        [SerializeField, Range(0f, 2f)] private float opticalCaustics = 0.8f;
        [SerializeField, Range(0f, 2f)] private float opticalDispersion = 0.9f;
        [SerializeField, Range(0f, 2f)] private float totalInternalReflection = 0.9f;

        public bool Enabled { get { return enabled; } }
        public DiamondFocusShape Shape { get { return shape; } }
        public int ShapeIndex { get { return Mathf.Clamp((int)shape, 0, ShapeCount - 1); } }
        public DiamondCrystalMaterialMode MaterialMode { get { return materialMode; } }
        public int MaterialModeIndex { get { return Mathf.Clamp((int)materialMode, 0, MaterialModeCount - 1); } }
        public DiamondGeneratedMaterialKind GeneratedMaterialKind { get { return generatedMaterialKind; } }
        public int GeneratedMaterialKindIndex { get { return Mathf.Clamp((int)generatedMaterialKind, 0, GeneratedMaterialKindCount - 1); } }
        public Color GeneratedMaterialColor { get { return generatedMaterialColor; } }
        public int GeneratedMaterialSeed { get { return generatedMaterialSeed; } }
        public Vector2 TargetRotationDirection { get { return targetRotationDirection; } }
        public Vector2 ResolvedTargetRotationDirection { get { return NormalizeOrDefault(targetRotationDirection); } }
        public Vector2 CurrentRotationDirection { get { return NormalizeOrDefault(currentRotationDirection); } }
        public Vector3 RotationEuler { get { return rotationEuler; } }
        public Vector3 RotationVelocity { get { return rotationVelocity; } }
        public float CurrentRotationSpeed { get { return Mathf.Clamp(currentRotationSpeed, MinRotationSpeed, MaxRotationSpeed); } }
        public float TargetRotationSpeed { get { return Mathf.Clamp(targetRotationSpeed, MinRotationSpeed, MaxRotationSpeed); } }
        public float MinRotationSpeed { get { return Mathf.Max(0f, minRotationSpeed); } }
        public float DefaultRotationSpeed { get { return Mathf.Clamp(defaultRotationSpeed, MinRotationSpeed, MaxRotationSpeed); } }
        public float MaxRotationSpeed { get { return Mathf.Max(1f, maxRotationSpeed); } }
        public float SpeedAcceleration { get { return Mathf.Max(1f, speedAcceleration); } }
        public float DirectionAcceleration { get { return Mathf.Max(0.01f, directionAcceleration); } }
        public float Transparency { get { return Mathf.Clamp01(transparency); } }
        public float RefractionStrength { get { return Mathf.Clamp(refractionStrength, 0f, 0.12f); } }
        public float ReflectionStrength { get { return Mathf.Clamp01(reflectionStrength); } }
        public float FresnelPower { get { return Mathf.Clamp(fresnelPower, 0.5f, 8f); } }
        public float EdgeHighlight { get { return Mathf.Max(0f, edgeHighlight); } }
        public float ChromaticAberrationAmount { get { return chromaticAberrationBase + NormalizedRotationSpeed * chromaticAberrationExtra; } }
        public float FacetContrast { get { return Mathf.Max(0f, facetContrast); } }
        public float InternalGlow { get { return Mathf.Max(0f, internalGlow); } }
        public float BloomBoost { get { return Mathf.Max(0f, bloomBoostBase + NormalizedRotationSpeed * bloomBoostExtra); } }
        public float ScreenScale { get { return Mathf.Clamp(screenScale, 0.1f, 1f); } }
        public float OpticalIOR { get { return Mathf.Clamp(opticalIOR, 1f, 3f); } }
        public float OpticalCaustics { get { return Mathf.Max(0f, opticalCaustics); } }
        public float OpticalDispersion { get { return Mathf.Max(0f, opticalDispersion); } }
        public float TotalInternalReflection { get { return Mathf.Max(0f, totalInternalReflection); } }
        public float NormalizedRotationSpeed { get { return MaxRotationSpeed > 0.0001f ? Mathf.Clamp01(CurrentRotationSpeed / MaxRotationSpeed) : 0f; } }
        public string ShapeLabel { get { return GetShapeLabel(shape); } }
        public string MaterialModeLabel { get { return GetMaterialModeLabel(materialMode); } }
        public string GeneratedMaterialLabel { get { return GetGeneratedMaterialKindLabel(generatedMaterialKind); } }

        public void SetEnabled(bool value)
        {
            enabled = value;
            if (enabled)
            {
                EnsureDefaultSpin();
            }
        }

        public void ToggleEnabled()
        {
            SetEnabled(!enabled);
        }

        public void SetShape(DiamondFocusShape value)
        {
            shape = value;
        }

        public void SetShapeIndex(int index)
        {
            int wrapped = index % ShapeCount;
            if (wrapped < 0)
            {
                wrapped += ShapeCount;
            }

            shape = (DiamondFocusShape)wrapped;
        }

        public void CycleShape(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            SetShapeIndex(ShapeIndex + (direction > 0 ? 1 : -1));
        }

        public void SetMaterialMode(DiamondCrystalMaterialMode value)
        {
            materialMode = value;
            RegenerateMaterialVariant();
        }

        public void SetMaterialModeIndex(int index)
        {
            int wrapped = index % MaterialModeCount;
            if (wrapped < 0)
            {
                wrapped += MaterialModeCount;
            }

            SetMaterialMode((DiamondCrystalMaterialMode)wrapped);
        }

        public void CycleMaterialMode(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            SetMaterialModeIndex(MaterialModeIndex + (direction > 0 ? 1 : -1));
        }

        public void SetTargetRotationDirection(Vector2 value)
        {
            targetRotationDirection = value.sqrMagnitude > 0.0001f ? value.normalized : Vector2.zero;
        }

        public void SetCurrentRotationDirection(Vector2 value)
        {
            currentRotationDirection = NormalizeOrDefault(value);
        }

        public void SetCurrentRotationSpeed(float value)
        {
            float speed = Mathf.Clamp(value, MinRotationSpeed, MaxRotationSpeed);
            Vector3 direction = rotationVelocity.sqrMagnitude > 0.0001f
                ? rotationVelocity.normalized
                : BuildRotationAxis(CurrentRotationDirection);
            SetRotationVelocity(direction * speed);
        }

        public void SetTargetRotationSpeed(float value)
        {
            targetRotationSpeed = Mathf.Clamp(value, MinRotationSpeed, MaxRotationSpeed);
        }

        public void AdjustTargetRotationSpeed(float delta)
        {
            SetTargetRotationSpeed(TargetRotationSpeed + delta);
            SetCurrentRotationSpeed(CurrentRotationSpeed + delta);
        }

        public void SetRotationVelocity(Vector3 value)
        {
            rotationVelocity = Vector3.ClampMagnitude(value, MaxRotationSpeed);
            currentRotationSpeed = rotationVelocity.magnitude;

            Vector2 planar = new Vector2(rotationVelocity.y, rotationVelocity.x);
            currentRotationDirection = planar.sqrMagnitude > 0.0001f ? planar.normalized : Vector2.zero;
            targetRotationSpeed = currentRotationSpeed;
        }

        public void AddRotationVelocity(Vector3 delta)
        {
            SetRotationVelocity(rotationVelocity + delta);
        }

        public void SetRotationEuler(Vector3 value)
        {
            rotationEuler = new Vector3(TrimLongAngle(value.x), TrimLongAngle(value.y), TrimLongAngle(value.z));
        }

        public Vector3 BuildRotationAxis(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return rotationVelocity.sqrMagnitude > 0.0001f
                    ? rotationVelocity.normalized
                    : new Vector3(0.45f, 0.78f, 0.2f).normalized;
            }

            direction.Normalize();
            return new Vector3(direction.y, direction.x, direction.x * direction.y * 0.45f).normalized;
        }

        public static string GetShapeLabel(DiamondFocusShape value)
        {
            switch (value)
            {
                case DiamondFocusShape.FacetedCube:
                    return "Faceted Cube";
                case DiamondFocusShape.TwelveFacetCrystal:
                    return "12-Facet Crystal";
                case DiamondFocusShape.TetrahedralCrystal:
                    return "Tetrahedral Crystal";
                case DiamondFocusShape.HighDetailDiamond:
                    return "96-Facet Diamond";
                default:
                    return "Classic Diamond";
            }
        }

        public static string GetMaterialModeLabel(DiamondCrystalMaterialMode value)
        {
            switch (value)
            {
                case DiamondCrystalMaterialMode.AbsoluteMirror:
                    return "Absolute Mirror";
                case DiamondCrystalMaterialMode.Glow:
                    return "Generated Glow";
                case DiamondCrystalMaterialMode.OpticalPhysics:
                    return "Optical IOR";
                case DiamondCrystalMaterialMode.GeneratedMaterial:
                    return "Generated Material";
                default:
                    return "Diamond";
            }
        }

        public static string GetGeneratedMaterialKindLabel(DiamondGeneratedMaterialKind value)
        {
            switch (value)
            {
                case DiamondGeneratedMaterialKind.Metal:
                    return "Metal";
                case DiamondGeneratedMaterialKind.Plastic:
                    return "Plastic";
                case DiamondGeneratedMaterialKind.Stone:
                    return "Stone";
                default:
                    return "Wood";
            }
        }

        private static Vector2 NormalizeOrDefault(Vector2 value)
        {
            return value.sqrMagnitude > 0.0001f ? value.normalized : new Vector2(0.35f, 0.75f).normalized;
        }

        private void RegenerateMaterialVariant()
        {
            generatedMaterialSeed = UnityEngine.Random.Range(1, int.MaxValue);

            if (materialMode == DiamondCrystalMaterialMode.Glow)
            {
                float hue = UnityEngine.Random.value;
                float saturation = UnityEngine.Random.Range(0.55f, 0.92f);
                float value = UnityEngine.Random.Range(0.8f, 1f);
                generatedMaterialColor = Color.HSVToRGB(hue, saturation, value);
                generatedMaterialColor.a = 1f;
                return;
            }

            if (materialMode == DiamondCrystalMaterialMode.GeneratedMaterial)
            {
                generatedMaterialKind = (DiamondGeneratedMaterialKind)UnityEngine.Random.Range(0, GeneratedMaterialKindCount);
                generatedMaterialColor = GenerateMaterialColor(generatedMaterialKind);
            }
        }

        private static Color GenerateMaterialColor(DiamondGeneratedMaterialKind kind)
        {
            switch (kind)
            {
                case DiamondGeneratedMaterialKind.Metal:
                    return UnityEngine.Random.value > 0.5f
                        ? new Color(UnityEngine.Random.Range(0.62f, 0.95f), UnityEngine.Random.Range(0.58f, 0.88f), UnityEngine.Random.Range(0.5f, 0.78f), 1f)
                        : new Color(UnityEngine.Random.Range(0.78f, 1f), UnityEngine.Random.Range(0.42f, 0.64f), UnityEngine.Random.Range(0.22f, 0.38f), 1f);

                case DiamondGeneratedMaterialKind.Plastic:
                    return Color.HSVToRGB(UnityEngine.Random.value, UnityEngine.Random.Range(0.35f, 0.75f), UnityEngine.Random.Range(0.55f, 0.9f));

                case DiamondGeneratedMaterialKind.Stone:
                    float stone = UnityEngine.Random.Range(0.35f, 0.7f);
                    return new Color(stone * UnityEngine.Random.Range(0.75f, 1.1f), stone * UnityEngine.Random.Range(0.78f, 1.08f), stone * UnityEngine.Random.Range(0.82f, 1.14f), 1f);

                default:
                    return new Color(UnityEngine.Random.Range(0.42f, 0.72f), UnityEngine.Random.Range(0.2f, 0.42f), UnityEngine.Random.Range(0.08f, 0.2f), 1f);
            }
        }

        private void EnsureDefaultSpin()
        {
            if (rotationVelocity.magnitude >= 0.1f)
            {
                SetRotationVelocity(rotationVelocity);
                return;
            }

            SetRotationVelocity(new Vector3(DefaultRotationSpeed * 0.45f, DefaultRotationSpeed * 0.78f, DefaultRotationSpeed * 0.2f));
        }

        private static float TrimLongAngle(float value)
        {
            const float limit = 100000f;
            if (value > limit || value < -limit)
            {
                return Mathf.Repeat(value + limit, limit * 2f) - limit;
            }

            return value;
        }
    }
}
