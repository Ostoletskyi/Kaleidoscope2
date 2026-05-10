using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kaleidoscope2.Core
{
    public enum KaleidoscopeSourceMode
    {
        None = 0,
        PhysicsChamber = 1,
        ImageTexture = 2,
        VideoTexture = 3,
        ProceduralTexture = 4,
        ExternalRenderTexture = 5
    }

    public enum KaleidoscopeVisualMode
    {
        Classic = 0,
        Tunnel = 1,
        Hose = 2,
        FiveD = 3,
        SixD = 4,
        SevenD = 5
    }

    public enum SevenDVisualizationStrategy
    {
        Romanesco = 0,
        Snowflake = 1,
        StructuralColor = 2,
        Murmuration = 3,
        Sunflower = 4
    }

    public enum KaleidoscopeRecordingStatus
    {
        Idle = 0,
        Preparing = 1,
        Recording = 2,
        Exporting = 3,
        Complete = 4,
        Error = 5
    }

    public enum KaleidoscopeQualityLevel
    {
        Preview = 0,
        High = 1,
        Ultra = 2,
        OfflineRender = 3
    }

    [Serializable]
    public sealed class KaleidoscopeState
    {
        [SerializeField] private KaleidoscopeSourceMode activeSourceMode = KaleidoscopeSourceMode.ProceduralTexture;
        [SerializeField] private KaleidoscopeVisualMode activeVisualMode = KaleidoscopeVisualMode.Classic;
        [SerializeField] private MirrorSettings mirrorSettings = new MirrorSettings();
        [SerializeField] private CameraSettings cameraSettings = new CameraSettings();
        [SerializeField] private TunnelSettings tunnelSettings = new TunnelSettings();
        [SerializeField] private FiveDSettings fiveDSettings = new FiveDSettings();
        [SerializeField] private SixDSettings sixDSettings = new SixDSettings();
        [SerializeField] private SevenDSettings sevenDSettings = new SevenDSettings();
        [SerializeField] private VisualMotionSettings classicMotionSettings = new VisualMotionSettings();
        [SerializeField] private VisualMotionSettings tunnelMotionSettings = new VisualMotionSettings();
        [SerializeField] private VisualMotionSettings hoseMotionSettings = new VisualMotionSettings();
        [SerializeField] private VisualMotionSettings sixDMotionSettings = new VisualMotionSettings();
        [SerializeField] private VisualMotionSettings sevenDMotionSettings = new VisualMotionSettings();
        [SerializeField] private TunnelBendSettings tunnelBendSettings = new TunnelBendSettings();
        [SerializeField] private TunnelBendState tunnelBendState = new TunnelBendState();
        [SerializeField] private string imageFilePath = string.Empty;
        [SerializeField] private string imageFolderPath = string.Empty;
        [SerializeField] private string audioFilePath = string.Empty;
        [SerializeField] private string audioFolderPath = string.Empty;
        [SerializeField] private bool controlMenuVisible;
        [SerializeField] private string activePreset = "None";
        [SerializeField] private bool tunnelEnabled;
        [SerializeField] private KaleidoscopeRecordingStatus recordingStatus = KaleidoscopeRecordingStatus.Idle;
        [SerializeField] private KaleidoscopeQualityLevel qualityLevel = KaleidoscopeQualityLevel.Preview;
        [SerializeField] private DiagnosticsState diagnostics = new DiagnosticsState();

        public KaleidoscopeSourceMode ActiveSourceMode
        {
            get { return activeSourceMode; }
        }

        public KaleidoscopeVisualMode ActiveVisualMode
        {
            get { return activeVisualMode; }
        }

        public MirrorSettings MirrorSettings
        {
            get { return mirrorSettings; }
        }

        public CameraSettings CameraSettings
        {
            get { return cameraSettings; }
        }

        public TunnelSettings TunnelSettings
        {
            get { return tunnelSettings; }
        }

        public FiveDSettings FiveDSettings
        {
            get { return fiveDSettings; }
        }

        public SixDSettings SixDSettings
        {
            get { return sixDSettings; }
        }

        public SevenDSettings SevenDSettings
        {
            get { return sevenDSettings; }
        }

        public VisualMotionSettings ClassicMotionSettings
        {
            get { return classicMotionSettings; }
        }

        public VisualMotionSettings TunnelMotionSettings
        {
            get { return tunnelMotionSettings; }
        }

        public VisualMotionSettings HoseMotionSettings
        {
            get { return hoseMotionSettings; }
        }

        public VisualMotionSettings SixDMotionSettings
        {
            get { return sixDMotionSettings; }
        }

        public VisualMotionSettings SevenDMotionSettings
        {
            get { return sevenDMotionSettings; }
        }

        public TunnelBendSettings TunnelBendSettings
        {
            get { return tunnelBendSettings; }
        }

        public TunnelBendState TunnelBendState
        {
            get { return tunnelBendState; }
        }

        public string ImageFilePath
        {
            get { return imageFilePath; }
        }

        public string ImageFolderPath
        {
            get { return imageFolderPath; }
        }

        public string AudioFilePath
        {
            get { return audioFilePath; }
        }

        public string AudioFolderPath
        {
            get { return audioFolderPath; }
        }

        public bool ControlMenuVisible
        {
            get { return controlMenuVisible; }
        }

        public string ActivePreset
        {
            get { return activePreset; }
        }

        public bool TunnelEnabled
        {
            get { return tunnelEnabled; }
        }

        public bool HoseModeEnabled
        {
            get { return activeVisualMode == KaleidoscopeVisualMode.Hose; }
        }

        public bool FiveDModeEnabled
        {
            get { return activeVisualMode == KaleidoscopeVisualMode.FiveD; }
        }

        public bool SixDModeEnabled
        {
            get { return activeVisualMode == KaleidoscopeVisualMode.SixD; }
        }

        public bool SevenDModeEnabled
        {
            get { return activeVisualMode == KaleidoscopeVisualMode.SevenD; }
        }

        public KaleidoscopeRecordingStatus RecordingStatus
        {
            get { return recordingStatus; }
        }

        public KaleidoscopeQualityLevel QualityLevel
        {
            get { return qualityLevel; }
        }

        public DiagnosticsState Diagnostics
        {
            get { return diagnostics; }
        }

        public void EnsureInitialized()
        {
            if (mirrorSettings == null)
            {
                mirrorSettings = new MirrorSettings();
            }

            if (cameraSettings == null)
            {
                cameraSettings = new CameraSettings();
            }

            if (tunnelSettings == null)
            {
                tunnelSettings = new TunnelSettings();
            }

            if (fiveDSettings == null)
            {
                fiveDSettings = new FiveDSettings();
            }

            if (sixDSettings == null)
            {
                sixDSettings = new SixDSettings();
            }

            if (sevenDSettings == null)
            {
                sevenDSettings = new SevenDSettings();
            }

            if (classicMotionSettings == null)
            {
                classicMotionSettings = new VisualMotionSettings();
            }

            if (tunnelMotionSettings == null)
            {
                tunnelMotionSettings = new VisualMotionSettings();
            }

            if (hoseMotionSettings == null)
            {
                hoseMotionSettings = new VisualMotionSettings();
            }

            if (sixDMotionSettings == null)
            {
                sixDMotionSettings = new VisualMotionSettings();
            }

            if (sevenDMotionSettings == null)
            {
                sevenDMotionSettings = new VisualMotionSettings();
            }

            if (tunnelBendSettings == null)
            {
                tunnelBendSettings = new TunnelBendSettings();
            }

            if (tunnelBendState == null)
            {
                tunnelBendState = new TunnelBendState();
            }

            if (diagnostics == null)
            {
                diagnostics = new DiagnosticsState();
            }
        }

        public void SetSourceMode(KaleidoscopeSourceMode sourceMode)
        {
            activeSourceMode = sourceMode;
        }

        public void SetVisualMode(KaleidoscopeVisualMode visualMode)
        {
            activeVisualMode = visualMode;
            tunnelEnabled = visualMode == KaleidoscopeVisualMode.Tunnel
                || visualMode == KaleidoscopeVisualMode.Hose
                || visualMode == KaleidoscopeVisualMode.FiveD;
        }

        public void SetTunnelEnabled(bool enabled)
        {
            tunnelEnabled = enabled;
            activeVisualMode = enabled ? KaleidoscopeVisualMode.Tunnel : KaleidoscopeVisualMode.Classic;
        }

        public void SetRecordingStatus(KaleidoscopeRecordingStatus status)
        {
            recordingStatus = status;
        }

        public void SetActivePreset(string presetName)
        {
            activePreset = string.IsNullOrWhiteSpace(presetName) ? "None" : presetName;
        }

        public void SetQualityLevel(KaleidoscopeQualityLevel level)
        {
            qualityLevel = level;
        }

        public void SetImageFilePath(string path)
        {
            imageFilePath = path ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(imageFilePath))
            {
                imageFolderPath = string.Empty;
                activeSourceMode = KaleidoscopeSourceMode.ImageTexture;
            }
        }

        public void SetImageFolderPath(string path)
        {
            imageFolderPath = path ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(imageFolderPath))
            {
                imageFilePath = string.Empty;
                activeSourceMode = KaleidoscopeSourceMode.ImageTexture;
            }
        }

        public void SetAudioFilePath(string path)
        {
            audioFilePath = path ?? string.Empty;
        }

        public void SetAudioFolderPath(string path)
        {
            audioFolderPath = path ?? string.Empty;
        }

        public void SetControlMenuVisible(bool visible)
        {
            controlMenuVisible = visible;
        }

        public void SetFramesPerSecond(float framesPerSecond)
        {
            diagnostics.SetFramesPerSecond(framesPerSecond);
        }

        public void SetModuleStatuses(IReadOnlyList<KaleidoscopeModuleStatus> statuses)
        {
            diagnostics.SetModuleStatuses(statuses);
        }

        public void ReportWarning(string warning)
        {
            diagnostics.ReportWarning(warning);
            Debug.LogWarning(warning);
        }

        public void ReportError(string error)
        {
            diagnostics.ReportError(error);
            Debug.LogError(error);
        }

        public void ReportMissingReference(string referenceName)
        {
            diagnostics.ReportMissingReference(referenceName);
            Debug.LogWarning("[MissingReference] " + referenceName);
        }

        public void ClearWarnings()
        {
            diagnostics.ClearWarnings();
        }

        public void ClearErrors()
        {
            diagnostics.ClearErrors();
        }

        public void ClearMissingReferences()
        {
            diagnostics.ClearMissingReferences();
        }

        public void SetDiagnosticsVisible(bool visible)
        {
            diagnostics.SetHudVisible(visible);
        }

        public VisualMotionSettings GetVisualMotionSettings(KaleidoscopeVisualMode visualMode)
        {
            switch (visualMode)
            {
                case KaleidoscopeVisualMode.Tunnel:
                    return tunnelMotionSettings;
                case KaleidoscopeVisualMode.Hose:
                    return hoseMotionSettings;
                case KaleidoscopeVisualMode.SixD:
                    return sixDMotionSettings;
                case KaleidoscopeVisualMode.SevenD:
                    return sevenDMotionSettings;
                case KaleidoscopeVisualMode.Classic:
                    return classicMotionSettings;
                default:
                    return null;
            }
        }

        public void ResetVisualMotion(KaleidoscopeVisualMode visualMode)
        {
            VisualMotionSettings settings = GetVisualMotionSettings(visualMode);
            if (settings != null)
            {
                settings.Reset();
            }
        }
    }

    [Serializable]
    public sealed class MirrorSettings
    {
        public const int MirrorCountMin = 1;
        public const int MirrorCountMax = 1536;
        public const float RotationSpeedMinUnits = -5000f;
        public const float RotationSpeedMaxUnits = 5000f;

        [SerializeField] private int mirrorCount = 6;
        [SerializeField] private float rotation;
        [SerializeField] private float rotationSpeedUnits;
        [SerializeField] private float forwardSpeedUnits;
        [SerializeField] private float zoom = 1f;
        [SerializeField] private Vector2 centerOffset = Vector2.zero;
        [SerializeField] private bool guidesVisible;

        public int MirrorCount
        {
            get { return mirrorCount; }
        }

        public float Rotation
        {
            get { return rotation; }
        }

        public float RotationSpeed
        {
            get { return rotationSpeedUnits; }
        }

        public float ForwardSpeedUnits
        {
            get { return forwardSpeedUnits; }
        }

        public bool GuidesVisible
        {
            get { return guidesVisible; }
        }

        public float Zoom
        {
            get { return zoom; }
        }

        public Vector2 CenterOffset
        {
            get { return centerOffset; }
        }

        public void SetMirrorCount(int count)
        {
            mirrorCount = Mathf.Clamp(count, MirrorCountMin, MirrorCountMax);
        }

        public void SetRotation(float value)
        {
            rotation = value;
        }

        public void SetRotationSpeed(float value)
        {
            rotationSpeedUnits = Mathf.Clamp(value, RotationSpeedMinUnits, RotationSpeedMaxUnits);
        }

        public void SetForwardSpeedUnits(float value)
        {
            forwardSpeedUnits = Mathf.Clamp(value, RotationSpeedMinUnits, RotationSpeedMaxUnits);
        }

        public void SetGuidesVisible(bool visible)
        {
            guidesVisible = visible;
        }

        public void SetZoom(float value)
        {
            zoom = Mathf.Clamp(value, 0.1f, 8f);
        }

        public void SetCenterOffset(Vector2 value)
        {
            centerOffset = value;
        }
    }

    [Serializable]
    public sealed class CameraSettings
    {
        [SerializeField] private int renderWidth = 1920;
        [SerializeField] private int renderHeight = 1080;

        public int RenderWidth
        {
            get { return renderWidth; }
        }

        public int RenderHeight
        {
            get { return renderHeight; }
        }

        public void SetRenderSize(int width, int height)
        {
            renderWidth = Mathf.Max(1, width);
            renderHeight = Mathf.Max(1, height);
        }
    }

    [Serializable]
    public sealed class TunnelSettings
    {
        public const float HoseProfileMinUnits = -500f;
        public const float HoseProfileMaxUnits = 500f;

        [SerializeField] private Vector2 bend = Vector2.zero;
        [SerializeField] private float hoseOpeningUnits;
        [SerializeField] private float hoseWallCurvatureUnits;
        [SerializeField] private bool hoseChromaticAberrationEnabled;

        public Vector2 Bend
        {
            get { return bend; }
        }

        public float HoseOpeningUnits
        {
            get { return hoseOpeningUnits; }
        }

        public float HoseWallCurvatureUnits
        {
            get { return hoseWallCurvatureUnits; }
        }

        public bool HoseChromaticAberrationEnabled
        {
            get { return hoseChromaticAberrationEnabled; }
        }

        public float HoseOpeningNormalized
        {
            get { return Mathf.InverseLerp(HoseProfileMinUnits, HoseProfileMaxUnits, hoseOpeningUnits) * 2f - 1f; }
        }

        public float HoseWallCurvatureNormalized
        {
            get { return Mathf.InverseLerp(HoseProfileMinUnits, HoseProfileMaxUnits, hoseWallCurvatureUnits) * 2f - 1f; }
        }

        public void SetBend(Vector2 value)
        {
            bend = new Vector2(
                Mathf.Clamp(value.x, -1f, 1f),
                Mathf.Clamp(value.y, -1f, 1f));
        }

        public void SetHoseOpeningUnits(float value)
        {
            hoseOpeningUnits = Mathf.Clamp(value, HoseProfileMinUnits, HoseProfileMaxUnits);
        }

        public void SetHoseWallCurvatureUnits(float value)
        {
            hoseWallCurvatureUnits = Mathf.Clamp(value, HoseProfileMinUnits, HoseProfileMaxUnits);
        }

        public void SetHoseChromaticAberrationEnabled(bool enabled)
        {
            hoseChromaticAberrationEnabled = enabled;
        }

        public void ToggleHoseChromaticAberration()
        {
            hoseChromaticAberrationEnabled = !hoseChromaticAberrationEnabled;
        }

        public void ResetHoseProfile()
        {
            hoseOpeningUnits = 0f;
            hoseWallCurvatureUnits = 0f;
        }
    }

    [Serializable]
    public sealed class FiveDSettings
    {
        public const float FlightSpeedMinUnits = -5000f;
        public const float FlightSpeedMaxUnits = 5000f;

        [SerializeField] private float flightSpeedUnits = 650f;
        [SerializeField] private float imageSwitchInterval = 20f;

        public float FlightSpeedUnits
        {
            get { return flightSpeedUnits; }
        }

        public float ImageSwitchInterval
        {
            get { return Mathf.Max(0.1f, imageSwitchInterval); }
        }

        public void SetFlightSpeedUnits(float value)
        {
            flightSpeedUnits = Mathf.Clamp(value, FlightSpeedMinUnits, FlightSpeedMaxUnits);
        }
    }

    [Serializable]
    public sealed class VisualMotionSettings
    {
        public const float FlightSpeedMinUnits = -5000f;
        public const float FlightSpeedMaxUnits = 5000f;
        public const float ImageOffsetMin = -4f;
        public const float ImageOffsetMax = 4f;

        [SerializeField] private float flightSpeedUnits;
        [SerializeField] private Vector2 imageOffset = Vector2.zero;

        public float FlightSpeedUnits
        {
            get { return Mathf.Clamp(flightSpeedUnits, FlightSpeedMinUnits, FlightSpeedMaxUnits); }
        }

        public Vector2 ImageOffset
        {
            get { return imageOffset; }
        }

        public void SetFlightSpeedUnits(float value)
        {
            flightSpeedUnits = Mathf.Clamp(value, FlightSpeedMinUnits, FlightSpeedMaxUnits);
        }

        public void SetImageOffset(Vector2 value)
        {
            imageOffset = new Vector2(
                Mathf.Clamp(value.x, ImageOffsetMin, ImageOffsetMax),
                Mathf.Clamp(value.y, ImageOffsetMin, ImageOffsetMax));
        }

        public void Reset()
        {
            flightSpeedUnits = 0f;
            imageOffset = Vector2.zero;
        }
    }

    [Serializable]
    public sealed class SixDSettings
    {
        [SerializeField] private bool depthWarpEnabled = true;
        [SerializeField] private bool opticalLookEnabled = true;
        [SerializeField] private bool volumetricIllusionEnabled = true;
        [SerializeField, Range(0f, 2f)] private float depthStrength = 0.82f;
        [SerializeField, Range(0f, 0.12f)] private float parallaxScale = 0.035f;
        [SerializeField, Range(0f, 1f)] private float focusStrength = 0.38f;
        [SerializeField, Range(0.4f, 4f)] private float focusFalloff = 1.75f;
        [SerializeField, Range(0f, 1.5f)] private float opticalCompression = 0.52f;
        [SerializeField, Range(0f, 1.5f)] private float centerPull = 0.74f;
        [SerializeField, Range(0f, 1f)] private float volumetricDensity = 0.42f;
        [SerializeField, Range(0f, 1f)] private float hazeStrength = 0.36f;
        [SerializeField, Range(0f, 0.03f)] private float chromaticAmount = 0.0065f;
        [SerializeField, Range(0f, 1f)] private float distortionStrength = 0.34f;
        [SerializeField, Range(0f, 1f)] private float motionBreathing = 0.32f;

        public bool DepthWarpEnabled
        {
            get { return depthWarpEnabled; }
        }

        public bool OpticalLookEnabled
        {
            get { return opticalLookEnabled; }
        }

        public bool VolumetricIllusionEnabled
        {
            get { return volumetricIllusionEnabled; }
        }

        public float DepthStrength
        {
            get { return Mathf.Clamp(depthStrength, 0f, 2f); }
        }

        public float ParallaxScale
        {
            get { return Mathf.Clamp(parallaxScale, 0f, 0.12f); }
        }

        public float FocusStrength
        {
            get { return Mathf.Clamp01(focusStrength); }
        }

        public float FocusFalloff
        {
            get { return Mathf.Clamp(focusFalloff, 0.4f, 4f); }
        }

        public float OpticalCompression
        {
            get { return Mathf.Clamp(opticalCompression, 0f, 1.5f); }
        }

        public float CenterPull
        {
            get { return Mathf.Clamp(centerPull, 0f, 1.5f); }
        }

        public float VolumetricDensity
        {
            get { return Mathf.Clamp01(volumetricDensity); }
        }

        public float HazeStrength
        {
            get { return Mathf.Clamp01(hazeStrength); }
        }

        public float ChromaticAmount
        {
            get { return Mathf.Clamp(chromaticAmount, 0f, 0.03f); }
        }

        public float DistortionStrength
        {
            get { return Mathf.Clamp01(distortionStrength); }
        }

        public float MotionBreathing
        {
            get { return Mathf.Clamp01(motionBreathing); }
        }

        public void SetDepthWarpEnabled(bool enabled)
        {
            depthWarpEnabled = enabled;
        }

        public void SetOpticalLookEnabled(bool enabled)
        {
            opticalLookEnabled = enabled;
        }

        public void SetVolumetricIllusionEnabled(bool enabled)
        {
            volumetricIllusionEnabled = enabled;
        }
    }

    [Serializable]
    public sealed class SevenDSettings
    {
        public const int StrategyCount = 5;

        [SerializeField] private SevenDVisualizationStrategy strategy = SevenDVisualizationStrategy.Romanesco;
        [SerializeField, Range(0f, 1f)] private float effectStrength = 0.92f;
        [SerializeField, Range(0.2f, 3f)] private float patternScale = 1.15f;
        [SerializeField, Range(0f, 3f)] private float motionSpeed = 0.7f;
        [SerializeField, Range(0f, 1f)] private float sourceBlend = 0.22f;

        public SevenDVisualizationStrategy Strategy
        {
            get { return strategy; }
        }

        public int StrategyIndex
        {
            get { return Mathf.Clamp((int)strategy, 0, StrategyCount - 1); }
        }

        public float EffectStrength
        {
            get { return Mathf.Clamp01(effectStrength); }
        }

        public float PatternScale
        {
            get { return Mathf.Clamp(patternScale, 0.2f, 3f); }
        }

        public float MotionSpeed
        {
            get { return Mathf.Clamp(motionSpeed, 0f, 3f); }
        }

        public float SourceBlend
        {
            get { return Mathf.Clamp01(sourceBlend); }
        }

        public string StrategyLabel
        {
            get { return GetStrategyLabel(strategy); }
        }

        public void SetStrategy(SevenDVisualizationStrategy value)
        {
            strategy = value;
        }

        public void SetStrategyIndex(int index)
        {
            int wrapped = WrapIndex(index);
            strategy = (SevenDVisualizationStrategy)wrapped;
        }

        public void CycleStrategy(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            SetStrategyIndex(StrategyIndex + (direction > 0 ? 1 : -1));
        }

        public static string GetStrategyLabel(SevenDVisualizationStrategy value)
        {
            switch (value)
            {
                case SevenDVisualizationStrategy.Snowflake:
                    return "Snowflakes";
                case SevenDVisualizationStrategy.StructuralColor:
                    return "Structural Color";
                case SevenDVisualizationStrategy.Murmuration:
                    return "Murmuration";
                case SevenDVisualizationStrategy.Sunflower:
                    return "Sunflower";
                default:
                    return "Romanesco";
            }
        }

        private static int WrapIndex(int index)
        {
            int value = index % StrategyCount;
            if (value < 0)
            {
                value += StrategyCount;
            }

            return value;
        }
    }

    [Serializable]
    public sealed class DiagnosticsState
    {
        [SerializeField] private float framesPerSecond;
        [SerializeField] private bool hudVisible;
        [SerializeField] private List<KaleidoscopeModuleStatus> moduleStatuses = new List<KaleidoscopeModuleStatus>();
        [SerializeField] private List<string> warnings = new List<string>();
        [SerializeField] private List<string> errors = new List<string>();
        [SerializeField] private List<string> missingReferences = new List<string>();

        public float FramesPerSecond
        {
            get { return framesPerSecond; }
        }

        public bool HudVisible
        {
            get { return hudVisible; }
        }

        public IReadOnlyList<KaleidoscopeModuleStatus> ModuleStatuses
        {
            get { return moduleStatuses; }
        }

        public IReadOnlyList<string> Warnings
        {
            get { return warnings; }
        }

        public IReadOnlyList<string> Errors
        {
            get { return errors; }
        }

        public IReadOnlyList<string> MissingReferences
        {
            get { return missingReferences; }
        }

        public void SetFramesPerSecond(float value)
        {
            framesPerSecond = Mathf.Max(0f, value);
        }

        public void SetHudVisible(bool visible)
        {
            hudVisible = visible;
        }

        public void SetModuleStatuses(IReadOnlyList<KaleidoscopeModuleStatus> statuses)
        {
            moduleStatuses.Clear();

            if (statuses == null)
            {
                return;
            }

            for (int index = 0; index < statuses.Count; index++)
            {
                moduleStatuses.Add(statuses[index]);
            }
        }

        public void ReportWarning(string warning)
        {
            AddUnique(warnings, warning);
        }

        public void ReportError(string error)
        {
            AddUnique(errors, error);
        }

        public void ReportMissingReference(string referenceName)
        {
            AddUnique(missingReferences, referenceName);
        }

        public void ClearWarnings()
        {
            warnings.Clear();
        }

        public void ClearErrors()
        {
            errors.Clear();
        }

        public void ClearMissingReferences()
        {
            missingReferences.Clear();
        }

        private static void AddUnique(List<string> values, string value)
        {
            if (values == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            for (int index = 0; index < values.Count; index++)
            {
                if (values[index] == value)
                {
                    return;
                }
            }

            values.Add(value);
        }
    }
}
