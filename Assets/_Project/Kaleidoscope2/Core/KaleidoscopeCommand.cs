using System;
using UnityEngine;

namespace Kaleidoscope2.Core
{
    public enum KaleidoscopeCommandType
    {
        None = 0,
        SetSourceMode = 1,
        SetVisualMode = 2,
        SetMirrorCount = 3,
        SetMirrorRotation = 4,
        SetMirrorZoom = 5,
        SetMirrorCenterOffset = 6,
        SetTunnelEnabled = 7,
        SetRecordingStatus = 8,
        SetActivePreset = 9,
        SetQualityLevel = 10,
        ValidateSystem = 11,
        ClearDiagnostics = 12,
        SetMirrorRotationSpeed = 13,
        SetDiagnosticsVisible = 14
    }

    [Serializable]
    public sealed class KaleidoscopeCommand
    {
        [SerializeField] private KaleidoscopeCommandType type;
        [SerializeField] private KaleidoscopeSourceMode sourceModeValue;
        [SerializeField] private KaleidoscopeVisualMode visualModeValue;
        [SerializeField] private KaleidoscopeRecordingStatus recordingStatusValue;
        [SerializeField] private KaleidoscopeQualityLevel qualityLevelValue;
        [SerializeField] private int intValue;
        [SerializeField] private float floatValue;
        [SerializeField] private bool boolValue;
        [SerializeField] private string stringValue;
        [SerializeField] private Vector2 vector2Value;

        private KaleidoscopeCommand(KaleidoscopeCommandType commandType)
        {
            type = commandType;
        }

        public KaleidoscopeCommandType Type
        {
            get { return type; }
        }

        public KaleidoscopeSourceMode SourceModeValue
        {
            get { return sourceModeValue; }
        }

        public KaleidoscopeVisualMode VisualModeValue
        {
            get { return visualModeValue; }
        }

        public KaleidoscopeRecordingStatus RecordingStatusValue
        {
            get { return recordingStatusValue; }
        }

        public KaleidoscopeQualityLevel QualityLevelValue
        {
            get { return qualityLevelValue; }
        }

        public int IntValue
        {
            get { return intValue; }
        }

        public float FloatValue
        {
            get { return floatValue; }
        }

        public bool BoolValue
        {
            get { return boolValue; }
        }

        public string StringValue
        {
            get { return stringValue; }
        }

        public Vector2 Vector2Value
        {
            get { return vector2Value; }
        }

        public static KaleidoscopeCommand SetSourceMode(KaleidoscopeSourceMode sourceMode)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetSourceMode)
            {
                sourceModeValue = sourceMode
            };
        }

        public static KaleidoscopeCommand SetVisualMode(KaleidoscopeVisualMode visualMode)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetVisualMode)
            {
                visualModeValue = visualMode
            };
        }

        public static KaleidoscopeCommand SetMirrorCount(int count)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetMirrorCount)
            {
                intValue = count
            };
        }

        public static KaleidoscopeCommand SetMirrorRotation(float rotation)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetMirrorRotation)
            {
                floatValue = rotation
            };
        }

        public static KaleidoscopeCommand SetMirrorRotationSpeed(float rotationSpeed)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetMirrorRotationSpeed)
            {
                floatValue = rotationSpeed
            };
        }

        public static KaleidoscopeCommand SetMirrorZoom(float zoom)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetMirrorZoom)
            {
                floatValue = zoom
            };
        }

        public static KaleidoscopeCommand SetMirrorCenterOffset(Vector2 centerOffset)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetMirrorCenterOffset)
            {
                vector2Value = centerOffset
            };
        }

        public static KaleidoscopeCommand SetTunnelEnabled(bool enabled)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetTunnelEnabled)
            {
                boolValue = enabled
            };
        }

        public static KaleidoscopeCommand SetRecordingStatus(KaleidoscopeRecordingStatus status)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetRecordingStatus)
            {
                recordingStatusValue = status
            };
        }

        public static KaleidoscopeCommand SetActivePreset(string presetName)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetActivePreset)
            {
                stringValue = presetName
            };
        }

        public static KaleidoscopeCommand SetQualityLevel(KaleidoscopeQualityLevel qualityLevel)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetQualityLevel)
            {
                qualityLevelValue = qualityLevel
            };
        }

        public static KaleidoscopeCommand ValidateSystem()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.ValidateSystem);
        }

        public static KaleidoscopeCommand ClearDiagnostics()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.ClearDiagnostics);
        }

        public static KaleidoscopeCommand SetDiagnosticsVisible(bool visible)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetDiagnosticsVisible)
            {
                boolValue = visible
            };
        }

        public override string ToString()
        {
            return type.ToString();
        }
    }
}
