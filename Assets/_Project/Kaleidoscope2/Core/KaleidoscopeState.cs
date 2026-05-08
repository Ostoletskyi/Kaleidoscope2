using System;
using System.Collections.Generic;
using Kaleidoscope2.Tunnel;
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
        Tunnel = 1
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
            tunnelEnabled = visualMode == KaleidoscopeVisualMode.Tunnel;
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
    }

    [Serializable]
    public sealed class MirrorSettings
    {
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
            mirrorCount = Mathf.Clamp(count, 1, 64);
        }

        public void SetRotation(float value)
        {
            rotation = value;
        }

        public void SetRotationSpeed(float value)
        {
            rotationSpeedUnits = Mathf.Clamp(value, -500f, 500f);
        }

        public void SetForwardSpeedUnits(float value)
        {
            forwardSpeedUnits = Mathf.Clamp(value, -500f, 500f);
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
        [SerializeField] private Vector2 bend = Vector2.zero;

        public Vector2 Bend
        {
            get { return bend; }
        }

        public void SetBend(Vector2 value)
        {
            bend = new Vector2(
                Mathf.Clamp(value.x, -1f, 1f),
                Mathf.Clamp(value.y, -1f, 1f));
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
