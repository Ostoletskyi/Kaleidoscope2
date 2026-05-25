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
        SetDiagnosticsVisible = 14,
        SetControlMenuVisible = 15,
        ToggleControlMenu = 16,
        SetImageFilePath = 17,
        SetImageFolderPath = 18,
        SetAudioFilePath = 19,
        SetAudioFolderPath = 20,
        ToggleMirrorGuides = 21,
        SetMirrorGuidesVisible = 22,
        SetMirrorRotationSpeedUnits = 23,
        SetMirrorForwardSpeedUnits = 24,
        SetTunnelBend = 25,
        SetTunnelHoseOpeningUnits = 26,
        SetTunnelHoseWallCurvatureUnits = 27,
        ResetTunnelHoseProfile = 28,
        TriggerSourceNextImage = 29,
        TriggerFiveDShake = 30,
        SetFiveDFlightSpeedUnits = 31,
        PreviousAudioTrack = 32,
        ToggleAudioPlayback = 33,
        NextAudioTrack = 34,
        ToggleTunnelHoseChromaticAberration = 35,
        SetTunnelHoseChromaticAberration = 36,
        TriggerTunnelShake = 37,
        SetSevenDStrategy = 38,
        CycleSevenDStrategy = 39,
        SetVisualMotionFlightSpeedUnits = 40,
        SetVisualMotionImageOffset = 41,
        ResetVisualMotion = 42,
        TriggerVisualMotionShake = 43,
        StartImageReanimation = 44,
        SetVisualMotionImageVelocity = 45,
        ToggleVisualMotionImageInertia = 46,
        SetVisualMotionImageInertia = 47,
        SetDiamondRotationDirection = 48,
        AdjustDiamondRotationSpeed = 49,
        SetDiamondRotationSpeed = 50,
        NextDiamondShape = 51,
        PreviousDiamondShape = 52,
        SetDiamondShape = 53,
        SetDiamondFocusEnabled = 54,
        ToggleDiamondFocus = 55,
        IncreaseDiamondRotationSpeed = 56,
        DecreaseDiamondRotationSpeed = 57,
        ToggleHotkeysHelp = 58,
        SetHotkeysHelpVisible = 59,
        CycleDiamondMaterialMode = 60,
        SetDiamondMaterialMode = 61,
        ToggleSecondDisplayOutput = 62,
        SetSecondDisplayOutputEnabled = 63,
        AdjustDiamondRefractionIndex = 64,
        SetDiamondRefractionIndex = 65,
        AdjustDiamondDirectedLightIntensity = 66,
        SetDiamondDirectedLightIntensity = 67,
        ToggleCrystalLightRig = 68,
        SetCrystalLightRigEnabled = 69,
        AdjustCrystalLightRigIntensity = 70,
        SetCrystalLightRigIntensity = 71,
        ToggleCrystalSimulationMode = 72,
        SetCrystalSimulationMode = 73,
        SetCrystalLightRigActiveLightCount = 74,
        AdjustPremiumCrystalScalePercent = 75,
        SetPremiumCrystalScalePercent = 76,
        TogglePremiumCrystalEffect = 77,
        ResetPremiumCrystalOpticalControls = 78,
        SetPremiumCrystalOptic = 79,
        SetPremiumCrystalEffectEnabled = 80,
        SetPremiumCrystalWheelScaleEnabled = 81,
        SetPremiumCrystalWheelScaleStepPercent = 82,
        ApplyPremiumCrystalPreset = 83,
        CycleCrystalDebugMode = 84,
        SetCrystalDebugMode = 85,
        ApplyExperimentalCrystalPreset = 86,
        RestorePreviousCrystalPreset = 87,
        SetPremiumCrystalShape = 88,
        CyclePremiumCrystalShape = 89,
        SetPremiumCrystalOpticalMode = 90,
        CyclePremiumCrystalOpticalMode = 91,
        SetCrystalDebugEffect = 92,
        CycleCrystalDebugEffect = 93,
        SetCrystalRuntimeControlModule = 94,
        CycleSelectedCrystalRuntimeControl = 95
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

        public static KaleidoscopeCommand SetControlMenuVisible(bool visible)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetControlMenuVisible)
            {
                boolValue = visible
            };
        }

        public static KaleidoscopeCommand ToggleControlMenu()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.ToggleControlMenu);
        }

        public static KaleidoscopeCommand SetImageFilePath(string path)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetImageFilePath)
            {
                stringValue = path
            };
        }

        public static KaleidoscopeCommand SetImageFolderPath(string path)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetImageFolderPath)
            {
                stringValue = path
            };
        }

        public static KaleidoscopeCommand SetAudioFilePath(string path)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetAudioFilePath)
            {
                stringValue = path
            };
        }

        public static KaleidoscopeCommand SetAudioFolderPath(string path)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetAudioFolderPath)
            {
                stringValue = path
            };
        }

        public static KaleidoscopeCommand ToggleMirrorGuides()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.ToggleMirrorGuides);
        }

        public static KaleidoscopeCommand SetMirrorGuidesVisible(bool visible)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetMirrorGuidesVisible)
            {
                boolValue = visible
            };
        }

        public static KaleidoscopeCommand SetMirrorRotationSpeedUnits(float speedUnits)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetMirrorRotationSpeedUnits)
            {
                floatValue = speedUnits
            };
        }

        public static KaleidoscopeCommand SetMirrorForwardSpeedUnits(float speedUnits)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetMirrorForwardSpeedUnits)
            {
                floatValue = speedUnits
            };
        }

        public static KaleidoscopeCommand SetTunnelBend(Vector2 bend)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetTunnelBend)
            {
                vector2Value = bend
            };
        }

        public static KaleidoscopeCommand SetTunnelHoseOpeningUnits(float openingUnits)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetTunnelHoseOpeningUnits)
            {
                floatValue = openingUnits
            };
        }

        public static KaleidoscopeCommand SetTunnelHoseWallCurvatureUnits(float curvatureUnits)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetTunnelHoseWallCurvatureUnits)
            {
                floatValue = curvatureUnits
            };
        }

        public static KaleidoscopeCommand ResetTunnelHoseProfile()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.ResetTunnelHoseProfile);
        }

        public static KaleidoscopeCommand TriggerSourceNextImage()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.TriggerSourceNextImage);
        }

        public static KaleidoscopeCommand TriggerFiveDShake()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.TriggerFiveDShake);
        }

        public static KaleidoscopeCommand SetFiveDFlightSpeedUnits(float speedUnits)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetFiveDFlightSpeedUnits)
            {
                floatValue = speedUnits
            };
        }

        public static KaleidoscopeCommand PreviousAudioTrack()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.PreviousAudioTrack);
        }

        public static KaleidoscopeCommand ToggleAudioPlayback()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.ToggleAudioPlayback);
        }

        public static KaleidoscopeCommand NextAudioTrack()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.NextAudioTrack);
        }

        public static KaleidoscopeCommand ToggleTunnelHoseChromaticAberration()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.ToggleTunnelHoseChromaticAberration);
        }

        public static KaleidoscopeCommand SetTunnelHoseChromaticAberration(bool enabled)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetTunnelHoseChromaticAberration)
            {
                boolValue = enabled
            };
        }

        public static KaleidoscopeCommand TriggerTunnelShake()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.TriggerTunnelShake);
        }

        public static KaleidoscopeCommand SetSevenDStrategy(SevenDVisualizationStrategy strategy)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetSevenDStrategy)
            {
                intValue = (int)strategy
            };
        }

        public static KaleidoscopeCommand CycleSevenDStrategy(int direction)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.CycleSevenDStrategy)
            {
                intValue = direction
            };
        }

        public static KaleidoscopeCommand SetVisualMotionFlightSpeedUnits(KaleidoscopeVisualMode visualMode, float speedUnits)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetVisualMotionFlightSpeedUnits)
            {
                visualModeValue = visualMode,
                floatValue = speedUnits
            };
        }

        public static KaleidoscopeCommand SetVisualMotionImageOffset(KaleidoscopeVisualMode visualMode, Vector2 imageOffset)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetVisualMotionImageOffset)
            {
                visualModeValue = visualMode,
                vector2Value = imageOffset
            };
        }

        public static KaleidoscopeCommand ResetVisualMotion(KaleidoscopeVisualMode visualMode)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.ResetVisualMotion)
            {
                visualModeValue = visualMode
            };
        }

        public static KaleidoscopeCommand TriggerVisualMotionShake(KaleidoscopeVisualMode visualMode)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.TriggerVisualMotionShake)
            {
                visualModeValue = visualMode
            };
        }

        public static KaleidoscopeCommand StartImageReanimation()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.StartImageReanimation);
        }

        public static KaleidoscopeCommand SetVisualMotionImageVelocity(KaleidoscopeVisualMode visualMode, Vector2 imageVelocity)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetVisualMotionImageVelocity)
            {
                visualModeValue = visualMode,
                vector2Value = imageVelocity
            };
        }

        public static KaleidoscopeCommand ToggleVisualMotionImageInertia(KaleidoscopeVisualMode visualMode)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.ToggleVisualMotionImageInertia)
            {
                visualModeValue = visualMode
            };
        }

        public static KaleidoscopeCommand SetVisualMotionImageInertia(KaleidoscopeVisualMode visualMode, bool enabled)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetVisualMotionImageInertia)
            {
                visualModeValue = visualMode,
                boolValue = enabled
            };
        }

        public static KaleidoscopeCommand SetDiamondRotationDirection(Vector2 direction)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetDiamondRotationDirection)
            {
                vector2Value = direction
            };
        }

        public static KaleidoscopeCommand AdjustDiamondRotationSpeed(float delta)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.AdjustDiamondRotationSpeed)
            {
                floatValue = delta
            };
        }

        public static KaleidoscopeCommand SetDiamondRotationSpeed(float speed)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetDiamondRotationSpeed)
            {
                floatValue = speed
            };
        }

        public static KaleidoscopeCommand IncreaseDiamondRotationSpeed(float amount)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.IncreaseDiamondRotationSpeed)
            {
                floatValue = amount
            };
        }

        public static KaleidoscopeCommand DecreaseDiamondRotationSpeed(float amount)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.DecreaseDiamondRotationSpeed)
            {
                floatValue = amount
            };
        }

        public static KaleidoscopeCommand NextDiamondShape()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.NextDiamondShape);
        }

        public static KaleidoscopeCommand PreviousDiamondShape()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.PreviousDiamondShape);
        }

        public static KaleidoscopeCommand SetDiamondShape(DiamondFocusShape shape)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetDiamondShape)
            {
                intValue = (int)shape
            };
        }

        public static KaleidoscopeCommand SetPremiumCrystalShape(PremiumCrystalShapeType shape)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetPremiumCrystalShape)
            {
                intValue = (int)shape
            };
        }

        public static KaleidoscopeCommand CyclePremiumCrystalShape(int direction)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.CyclePremiumCrystalShape)
            {
                intValue = direction == 0 ? 1 : direction
            };
        }

        public static KaleidoscopeCommand CycleDiamondMaterialMode(int direction)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.CycleDiamondMaterialMode)
            {
                intValue = direction
            };
        }

        public static KaleidoscopeCommand SetDiamondMaterialMode(DiamondCrystalMaterialMode mode)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetDiamondMaterialMode)
            {
                intValue = (int)mode
            };
        }

        public static KaleidoscopeCommand CycleCrystalDebugMode(int direction)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.CycleCrystalDebugMode)
            {
                intValue = direction == 0 ? 1 : direction
            };
        }

        public static KaleidoscopeCommand SetCrystalDebugMode(DiamondCrystalDebugMode mode)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetCrystalDebugMode)
            {
                intValue = (int)mode
            };
        }

        public static KaleidoscopeCommand SetCrystalDebugEffect(CrystalDebugEffectType effect)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetCrystalDebugEffect)
            {
                intValue = (int)effect,
                stringValue = CrystalDebugEffectLibrary.GetDisplayName(effect)
            };
        }

        public static KaleidoscopeCommand CycleCrystalDebugEffect(int direction)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.CycleCrystalDebugEffect)
            {
                intValue = direction == 0 ? 1 : direction
            };
        }

        public static KaleidoscopeCommand SetCrystalRuntimeControlModule(CrystalRuntimeControlModule module)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetCrystalRuntimeControlModule)
            {
                intValue = (int)module,
                stringValue = CrystalRuntimeControlLibrary.GetDisplayName(module)
            };
        }

        public static KaleidoscopeCommand CycleSelectedCrystalRuntimeControl(int direction)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.CycleSelectedCrystalRuntimeControl)
            {
                intValue = direction == 0 ? 1 : direction
            };
        }

        public static KaleidoscopeCommand AdjustDiamondRefractionIndex(float delta)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.AdjustDiamondRefractionIndex)
            {
                floatValue = delta
            };
        }

        public static KaleidoscopeCommand SetDiamondRefractionIndex(float value)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetDiamondRefractionIndex)
            {
                floatValue = value
            };
        }

        public static KaleidoscopeCommand AdjustDiamondDirectedLightIntensity(float delta)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.AdjustDiamondDirectedLightIntensity)
            {
                floatValue = delta
            };
        }

        public static KaleidoscopeCommand SetDiamondDirectedLightIntensity(float value)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetDiamondDirectedLightIntensity)
            {
                floatValue = value
            };
        }

        public static KaleidoscopeCommand ToggleCrystalLightRig()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.ToggleCrystalLightRig);
        }

        public static KaleidoscopeCommand SetCrystalLightRigEnabled(bool enabled)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetCrystalLightRigEnabled)
            {
                boolValue = enabled
            };
        }

        public static KaleidoscopeCommand AdjustCrystalLightRigIntensity(float delta)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.AdjustCrystalLightRigIntensity)
            {
                floatValue = delta
            };
        }

        public static KaleidoscopeCommand SetCrystalLightRigIntensity(float value)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetCrystalLightRigIntensity)
            {
                floatValue = value
            };
        }

        public static KaleidoscopeCommand SetCrystalLightRigActiveLightCount(int count)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetCrystalLightRigActiveLightCount)
            {
                intValue = count
            };
        }

        public static KaleidoscopeCommand AdjustPremiumCrystalScalePercent(float deltaPercent)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.AdjustPremiumCrystalScalePercent)
            {
                floatValue = deltaPercent
            };
        }

        public static KaleidoscopeCommand SetPremiumCrystalScalePercent(float percent)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetPremiumCrystalScalePercent)
            {
                floatValue = percent
            };
        }

        public static KaleidoscopeCommand TogglePremiumCrystalEffect(PremiumCrystalEffectToggle effect)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.TogglePremiumCrystalEffect)
            {
                intValue = (int)effect
            };
        }

        public static KaleidoscopeCommand SetPremiumCrystalEffectEnabled(PremiumCrystalEffectToggle effect, bool enabled)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetPremiumCrystalEffectEnabled)
            {
                intValue = (int)effect,
                boolValue = enabled
            };
        }

        public static KaleidoscopeCommand SetPremiumCrystalOptic(PremiumCrystalOpticsParameter parameter, float value)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetPremiumCrystalOptic)
            {
                intValue = (int)parameter,
                floatValue = value
            };
        }

        public static KaleidoscopeCommand SetPremiumCrystalWheelScaleEnabled(bool enabled)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetPremiumCrystalWheelScaleEnabled)
            {
                boolValue = enabled
            };
        }

        public static KaleidoscopeCommand SetPremiumCrystalWheelScaleStepPercent(float percent)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetPremiumCrystalWheelScaleStepPercent)
            {
                floatValue = percent
            };
        }

        public static KaleidoscopeCommand ApplyPremiumCrystalPreset(PremiumCrystalFactoryPreset preset)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.ApplyPremiumCrystalPreset)
            {
                intValue = (int)preset,
                stringValue = DiamondFocusSettings.GetPremiumCrystalFactoryPresetLabel(preset)
            };
        }

        public static KaleidoscopeCommand ApplyExperimentalCrystalPreset(CrystalExperimentPresetType preset)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.ApplyExperimentalCrystalPreset)
            {
                intValue = (int)preset,
                stringValue = CrystalExperimentPreset.GetLabel(preset)
            };
        }

        public static KaleidoscopeCommand RestorePreviousCrystalPreset()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.RestorePreviousCrystalPreset)
            {
                intValue = (int)CrystalExperimentPresetType.Normal,
                stringValue = CrystalExperimentPreset.GetLabel(CrystalExperimentPresetType.Normal)
            };
        }

        public static KaleidoscopeCommand SetPremiumCrystalOpticalMode(PremiumCrystalOpticalMode mode)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetPremiumCrystalOpticalMode)
            {
                intValue = (int)mode,
                stringValue = PremiumCrystalOpticalModeLibrary.GetLabel(mode)
            };
        }

        public static KaleidoscopeCommand CyclePremiumCrystalOpticalMode(int direction)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.CyclePremiumCrystalOpticalMode)
            {
                intValue = direction == 0 ? 1 : direction
            };
        }

        public static KaleidoscopeCommand ResetPremiumCrystalOpticalControls()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.ResetPremiumCrystalOpticalControls);
        }

        public static KaleidoscopeCommand ToggleCrystalSimulationMode()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.ToggleCrystalSimulationMode);
        }

        public static KaleidoscopeCommand SetCrystalSimulationMode(CrystalRenderMode mode)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetCrystalSimulationMode)
            {
                intValue = (int)mode
            };
        }

        public static KaleidoscopeCommand SetDiamondFocusEnabled(bool enabled)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetDiamondFocusEnabled)
            {
                boolValue = enabled
            };
        }

        public static KaleidoscopeCommand ToggleDiamondFocus()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.ToggleDiamondFocus);
        }

        public static KaleidoscopeCommand ToggleHotkeysHelp()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.ToggleHotkeysHelp);
        }

        public static KaleidoscopeCommand SetHotkeysHelpVisible(bool visible)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetHotkeysHelpVisible)
            {
                boolValue = visible
            };
        }

        public static KaleidoscopeCommand ToggleSecondDisplayOutput()
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.ToggleSecondDisplayOutput);
        }

        public static KaleidoscopeCommand SetSecondDisplayOutputEnabled(bool enabled)
        {
            return new KaleidoscopeCommand(KaleidoscopeCommandType.SetSecondDisplayOutputEnabled)
            {
                boolValue = enabled
            };
        }

        public override string ToString()
        {
            return type.ToString();
        }
    }
}
