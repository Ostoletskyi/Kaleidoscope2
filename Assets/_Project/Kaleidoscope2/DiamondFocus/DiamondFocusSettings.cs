using System;
using UnityEngine;

namespace Kaleidoscope2.Core
{
    public enum DiamondFocusShape
    {
        ClassicDiamond = 0,
        FacetedCube = 1,
        DiscoBall = 2,
        TetrahedralCrystal = 3,
        RhombicCrystal = 4,
        OvalRingGem = 5
    }

    public enum DiamondCrystalMaterialMode
    {
        AbsoluteMirror = 0,
        Diamond = 1,
        Glow = 2,
        OpticalPhysics = 3,
        GeneratedMaterial = 4
    }

    public enum DiamondCrystalDebugMode
    {
        FinalCrystalComposite = 0,
        RawKaleidoscopeTex = 1,
        RefractionOnly = 2,
        ReflectionOnly = 3
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
        public const int ShapeCount = 6;
        public const int MaterialModeCount = 5;
        public const int GeneratedMaterialKindCount = 4;
        public const float RefractionIndexMin = 0f;
        public const float RefractionIndexMax = 10f;

        [SerializeField] private bool enabled;
        [SerializeField] private DiamondFocusShape shape = DiamondFocusShape.ClassicDiamond;
        [SerializeField] private DiamondFocusShape shapeTransitionFromShape = DiamondFocusShape.ClassicDiamond;
        [SerializeField] private DiamondFocusShape shapeTransitionToShape = DiamondFocusShape.ClassicDiamond;
        [SerializeField] private bool shapeTransitionActive;
        [SerializeField] private float shapeTransitionElapsed;
        [SerializeField, Range(0.1f, 5f)] private float shapeTransitionDuration = 2f;
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
        [SerializeField, Range(0f, 1f)] private float transparency;
        [SerializeField, Range(0f, 0.12f)] private float refractionStrength = 0.074f;
        [SerializeField, Range(0f, 2f)] private float dispersionStrength = 1f;
        [SerializeField, Range(0f, 1f)] private float reflectionStrength = 0.72f;
        [SerializeField, Range(0.5f, 8f)] private float fresnelPower = 3.2f;
        [SerializeField, Range(0f, 3f)] private float internalBrightness = 1f;
        [SerializeField, Range(0f, 1f)] private float noiseDistortionStrength = 0.08f;
        [SerializeField, Range(0f, 2f)] private float edgeHighlight = 1.35f;
        [SerializeField, Range(0f, 0.04f)] private float chromaticAberrationBase = 0.009f;
        [SerializeField, Range(0f, 0.04f)] private float chromaticAberrationExtra = 0.016f;
        [SerializeField, Range(0f, 2f)] private float facetContrast = 1.45f;
        [SerializeField, Range(0f, 1.5f)] private float internalGlow = 0.62f;
        [SerializeField, Range(0f, 3f)] private float bloomBoostBase = 0.44f;
        [SerializeField, Range(0f, 3f)] private float bloomBoostExtra = 1.25f;
        [SerializeField, Range(0.1f, 1f)] private float screenScale = 0.38f;

        [Header("Cinematic Crystal Optics")]
        [SerializeField, Range(0f, 2f)] private float diamondLikeRefraction = 1.8f;
        [SerializeField, Range(0f, 3f)] private float spectralDispersion = 2.25f;
        [SerializeField, Range(0f, 3f)] private float highEnergyCaustics = 1.85f;
        [SerializeField, Range(0f, 3f)] private float multiBounceInternalReflections = 1.75f;
        [SerializeField, Range(0f, 2f)] private float cinematicCrystalOptics = 1.45f;
        [SerializeField, Range(0f, 2f)] private float physicallyBasedRefraction = 1.55f;
        [SerializeField, Range(0f, 3f)] private float deepVolumetricLightScattering = 1.35f;
        [SerializeField, Range(0f, 1f)] private float crystalSolidity = 1f;
        [SerializeField, Range(0f, 3f)] private float blueWhitePlasmaEnergy = 2.05f;
        [SerializeField, Range(0f, 1f)] private float directTransmission = 0.06f;
        [SerializeField, Range(0f, 3f)] private float totalInternalReturn = 1.65f;
        [SerializeField, Range(0f, 3f)] private float spectralFireIntensity = 1.85f;
        [SerializeField, Range(0f, 2f)] private float facetDepthContrast = 1.22f;

        [Header("Optical Material Mode")]
        [SerializeField, Range(0f, 10f)] private float opticalIOR = 2.42f;
        [SerializeField, Range(0f, 2f)] private float opticalCaustics = 0.8f;
        [SerializeField, Range(0f, 2f)] private float opticalDispersion = 1.45f;
        [SerializeField, Range(0f, 2f)] private float totalInternalReflection = 1.45f;

        [Header("Debug")]
        [SerializeField] private DiamondCrystalDebugMode debugMode = DiamondCrystalDebugMode.FinalCrystalComposite;

        public bool Enabled { get { return enabled; } }
        public DiamondFocusShape Shape { get { return shape; } }
        public int ShapeIndex { get { return Mathf.Clamp((int)shape, 0, ShapeCount - 1); } }
        public DiamondFocusShape ShapeTransitionFromShape { get { return shapeTransitionFromShape; } }
        public DiamondFocusShape ShapeTransitionToShape { get { return shapeTransitionToShape; } }
        public bool ShapeTransitionActive { get { return shapeTransitionActive; } }
        public float ShapeTransitionDuration { get { return Mathf.Max(0.1f, shapeTransitionDuration); } }
        public float ShapeTransitionProgress
        {
            get
            {
                return shapeTransitionActive
                    ? Mathf.Clamp01(shapeTransitionElapsed / ShapeTransitionDuration)
                    : 1f;
            }
        }
        public float ShapeTransitionSmoothProgress
        {
            get
            {
                float value = ShapeTransitionProgress;
                return value * value * (3f - 2f * value);
            }
        }
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
        public float DispersionStrength { get { return Mathf.Max(0f, dispersionStrength); } }
        public float ReflectionStrength { get { return Mathf.Clamp01(reflectionStrength); } }
        public float FresnelPower { get { return Mathf.Clamp(fresnelPower, 0.5f, 8f); } }
        public float InternalBrightness { get { return Mathf.Max(0f, internalBrightness); } }
        public float NoiseDistortionStrength { get { return Mathf.Clamp01(noiseDistortionStrength); } }
        public float EdgeHighlight { get { return Mathf.Max(0f, edgeHighlight); } }
        public float ChromaticAberrationAmount { get { return chromaticAberrationBase + NormalizedRotationSpeed * chromaticAberrationExtra; } }
        public float FacetContrast { get { return Mathf.Max(0f, facetContrast); } }
        public float InternalGlow { get { return Mathf.Max(0f, internalGlow); } }
        public float BloomBoost { get { return Mathf.Max(0f, bloomBoostBase + NormalizedRotationSpeed * bloomBoostExtra); } }
        public float ScreenScale { get { return Mathf.Clamp(screenScale, 0.1f, 1f); } }
        public float DiamondLikeRefraction { get { return Mathf.Max(0f, diamondLikeRefraction); } }
        public float SpectralDispersion { get { return Mathf.Max(0f, spectralDispersion); } }
        public float HighEnergyCaustics { get { return Mathf.Max(0f, highEnergyCaustics); } }
        public float MultiBounceInternalReflections { get { return Mathf.Max(0f, multiBounceInternalReflections); } }
        public float CinematicCrystalOptics { get { return Mathf.Max(0f, cinematicCrystalOptics); } }
        public float PhysicallyBasedRefraction { get { return Mathf.Max(0f, physicallyBasedRefraction); } }
        public float DeepVolumetricLightScattering { get { return Mathf.Max(0f, deepVolumetricLightScattering); } }
        public float CrystalSolidity { get { return Mathf.Clamp01(crystalSolidity); } }
        public float BlueWhitePlasmaEnergy { get { return Mathf.Max(0f, blueWhitePlasmaEnergy); } }
        public float DirectTransmission { get { return Mathf.Clamp01(directTransmission); } }
        public float TotalInternalReturn { get { return Mathf.Max(0f, totalInternalReturn); } }
        public float SpectralFireIntensity { get { return Mathf.Max(0f, spectralFireIntensity); } }
        public float FacetDepthContrast { get { return Mathf.Max(0f, facetDepthContrast); } }
        public float OpticalIOR { get { return RefractionIndex; } }
        public float RefractionIndex { get { return Mathf.Clamp(opticalIOR, RefractionIndexMin, RefractionIndexMax); } }
        public float OpticalCaustics { get { return Mathf.Max(0f, opticalCaustics); } }
        public float OpticalDispersion { get { return Mathf.Max(0f, opticalDispersion); } }
        public float TotalInternalReflection { get { return Mathf.Max(0f, totalInternalReflection); } }
        public float NormalizedRotationSpeed { get { return MaxRotationSpeed > 0.0001f ? Mathf.Clamp01(CurrentRotationSpeed / MaxRotationSpeed) : 0f; } }
        public string ShapeLabel { get { return GetShapeLabel(shape); } }
        public string MaterialModeLabel { get { return GetMaterialModeLabel(materialMode); } }
        public string GeneratedMaterialLabel { get { return GetGeneratedMaterialKindLabel(generatedMaterialKind); } }
        public DiamondCrystalDebugMode DebugMode { get { return debugMode; } }

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
            shapeTransitionFromShape = value;
            shapeTransitionToShape = value;
            shapeTransitionActive = false;
            shapeTransitionElapsed = 0f;
        }

        public void BeginShapeTransition(DiamondFocusShape value)
        {
            DiamondFocusShape target = (DiamondFocusShape)Mathf.Clamp((int)value, 0, ShapeCount - 1);
            DiamondFocusShape from = shapeTransitionActive ? shapeTransitionToShape : shape;

            shape = target;
            shapeTransitionFromShape = from;
            shapeTransitionToShape = target;
            shapeTransitionElapsed = 0f;
            shapeTransitionActive = from != target;
        }

        public void TickShapeTransition(float deltaTime)
        {
            if (!shapeTransitionActive)
            {
                return;
            }

            shapeTransitionElapsed = Mathf.Min(ShapeTransitionDuration, shapeTransitionElapsed + Mathf.Max(0f, deltaTime));
            if (shapeTransitionElapsed >= ShapeTransitionDuration)
            {
                shapeTransitionActive = false;
                shapeTransitionFromShape = shapeTransitionToShape;
                shape = shapeTransitionToShape;
            }
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

        public void SetRefractionIndex(float value)
        {
            opticalIOR = Mathf.Clamp(value, RefractionIndexMin, RefractionIndexMax);
        }

        public void AdjustRefractionIndex(float delta)
        {
            SetRefractionIndex(RefractionIndex + delta);
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
                case DiamondFocusShape.DiscoBall:
                    return "Disco Ball";
                case DiamondFocusShape.TetrahedralCrystal:
                    return "Triangular Crystal";
                case DiamondFocusShape.RhombicCrystal:
                    return "Rhombic Crystal";
                case DiamondFocusShape.OvalRingGem:
                    return "Oval Ring Gem";
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
            generatedMaterialSeed = generatedMaterialSeed >= int.MaxValue - 1 ? 1 : generatedMaterialSeed + 1;

            if (materialMode == DiamondCrystalMaterialMode.Glow)
            {
                generatedMaterialColor = new Color(0.68f, 0.9f, 1f, 1f);
                return;
            }

            if (materialMode == DiamondCrystalMaterialMode.GeneratedMaterial)
            {
                generatedMaterialKind = (DiamondGeneratedMaterialKind)((GeneratedMaterialKindIndex + 1) % GeneratedMaterialKindCount);
                generatedMaterialColor = GenerateMaterialColor(generatedMaterialKind);
            }
        }

        private static Color GenerateMaterialColor(DiamondGeneratedMaterialKind kind)
        {
            switch (kind)
            {
                case DiamondGeneratedMaterialKind.Metal:
                    return new Color(0.86f, 0.82f, 0.72f, 1f);

                case DiamondGeneratedMaterialKind.Plastic:
                    return new Color(0.72f, 0.9f, 1f, 1f);

                case DiamondGeneratedMaterialKind.Stone:
                    return new Color(0.54f, 0.58f, 0.63f, 1f);

                default:
                    return new Color(0.62f, 0.38f, 0.18f, 1f);
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
