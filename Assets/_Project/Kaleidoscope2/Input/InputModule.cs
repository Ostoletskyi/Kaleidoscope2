using Kaleidoscope2.Core;
using Kaleidoscope2.DiamondFocus;
using Kaleidoscope2.Tunnel;
using System;
using UnityEngine;

namespace Kaleidoscope2.InputSystem
{
    // Converts physical input to Director commands. No direct visual mutation.
    [DisallowMultipleComponent]
    public sealed class InputModule : KaleidoscopeModuleBase
    {
        private const string DiamondFocusModuleId = "DiamondFocus";

        [Header("References")]
        [SerializeField] private KaleidoscopeDirector director;

        [Header("Keys")]
        [SerializeField] private KeyCode toggleMenuKey = KeyCode.Mouse2;
        [SerializeField] private KeyCode closeMenuKey = KeyCode.Escape;
        [SerializeField] private KeyCode toggleHotkeysHelpKey = KeyCode.F1;
        [SerializeField] private KeyCode toggleGuidesKey = KeyCode.Keypad0;
        [SerializeField] private KeyCode toggleGuidesAlternateKey = KeyCode.Alpha0;
        [SerializeField] private KeyCode reanimateImageKey = KeyCode.KeypadMultiply;

        [Header("Mirror Control")]
        // Legacy: older scene iterations serialized this. Kept for backward compatibility.
#pragma warning disable 649
        [SerializeField] private float unitsStepPerPress = 25f;
#pragma warning restore 649
        [SerializeField] private float unitsStepPerSecond = 220f;
        [SerializeField] private float zoomStepPerSecond = 1.4f;
        [SerializeField] private KeyCode zoomInKey = KeyCode.UpArrow;
        [SerializeField] private KeyCode zoomOutKey = KeyCode.DownArrow;
        [SerializeField] private KeyCode rotationLeftKey = KeyCode.LeftArrow;
        [SerializeField] private KeyCode rotationRightKey = KeyCode.RightArrow;
        [SerializeField] private KeyCode resetSpeedsKey = KeyCode.Keypad5;
        [SerializeField] private KeyCode cycleVisualModeKey = KeyCode.KeypadEnter;
        [SerializeField] private KeyCode sixSegmentsKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode twelveSegmentsKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode twentyFourSegmentsKey = KeyCode.Alpha3;
        [SerializeField] private KeyCode fortyEightSegmentsKey = KeyCode.Alpha4;
        [SerializeField] private KeyCode ninetySixSegmentsKey = KeyCode.Alpha5;
        [SerializeField] private KeyCode oneHundredNinetyTwoSegmentsKey = KeyCode.Alpha6;
        [SerializeField] private KeyCode threeHundredEightyFourSegmentsKey = KeyCode.Alpha7;
        [SerializeField] private KeyCode sevenHundredSixtyEightSegmentsKey = KeyCode.Alpha8;
        [SerializeField] private KeyCode fifteenHundredThirtySixSegmentsKey = KeyCode.Alpha9;

        [Header("2D Keys (Russian layout й / у / ц / ы / ф / в)")]
        [SerializeField] private ModeMotionInputProfile classicMotionKeys = new ModeMotionInputProfile();

        [Header("2D Inertial Shift (Russian layout к)")]
        [SerializeField] private KeyCode classicShiftInertiaToggleKey = KeyCode.R;
        [SerializeField] private float classicShiftAccelerationPerSecond = 0.85f;
        [SerializeField] private float classicShiftMaxSpeed = 1.2f;
        [SerializeField] private float classicShiftInertiaStopSeconds = 5f;

        [Header("3D Tunnel Distortion (WASD / Russian layout ц ф ы в)")]
        [SerializeField] private ModeMotionInputProfile tunnelMotionKeys = new ModeMotionInputProfile();
        [SerializeField] private float bendStepPerSecond = 1.25f;
        [SerializeField] private KeyCode tunnelBendUpKey = KeyCode.W;
        [SerializeField] private KeyCode tunnelBendLeftKey = KeyCode.A;
        [SerializeField] private KeyCode tunnelBendDownKey = KeyCode.S;
        [SerializeField] private KeyCode tunnelBendRightKey = KeyCode.D;

        [Header("4D Motion Keys (Russian layout й / у / ц / ы / ф / в)")]
        [SerializeField] private ModeMotionInputProfile hoseMotionKeys = new ModeMotionInputProfile();

        [Header("4D Hose Bend (Russian layout ш л о д)")]
        [SerializeField] private KeyCode hoseBendUpKey = KeyCode.I;
        [SerializeField] private KeyCode hoseBendDownKey = KeyCode.K;
        [SerializeField] private KeyCode hoseBendLeftKey = KeyCode.J;
        [SerializeField] private KeyCode hoseBendRightKey = KeyCode.L;

        [Header("Diamond Focus")]
        [SerializeField] private KeyCode diamondRotateLeftKey = KeyCode.A;
        [SerializeField] private KeyCode diamondRotateRightKey = KeyCode.D;
        [SerializeField] private KeyCode diamondRotateUpKey = KeyCode.W;
        [SerializeField] private KeyCode diamondRotateDownKey = KeyCode.S;
        [SerializeField] private bool diamondLegacyWasdControlsEnabled;
        [SerializeField] private KeyCode diamondRotateDownLeftKey = KeyCode.Keypad1;
        [SerializeField] private KeyCode diamondRotateDownKeypadKey = KeyCode.Keypad2;
        [SerializeField] private KeyCode diamondRotateDownRightKey = KeyCode.Keypad3;
        [SerializeField] private KeyCode diamondRotateLeftKeypadKey = KeyCode.Keypad4;
        [SerializeField] private KeyCode diamondRotateRightKeypadKey = KeyCode.Keypad6;
        [SerializeField] private KeyCode diamondRotateUpLeftKey = KeyCode.Keypad7;
        [SerializeField] private KeyCode diamondRotateUpKeypadKey = KeyCode.Keypad8;
        [SerializeField] private KeyCode diamondRotateUpRightKey = KeyCode.Keypad9;
        [SerializeField] private KeyCode diamondSpeedDecreaseKey = KeyCode.Q;
        [SerializeField] private KeyCode diamondSpeedIncreaseKey = KeyCode.E;
        [SerializeField] private KeyCode diamondNextShapeKey = KeyCode.KeypadPlus;
        [SerializeField] private KeyCode diamondPreviousShapeKey = KeyCode.KeypadMinus;
        [SerializeField] private KeyCode diamondNextMaterialModeKey = KeyCode.KeypadPeriod;
        [SerializeField] private KeyCode diamondToggleKey = KeyCode.Backspace;
        [SerializeField] private float diamondSpeedStepPerSecond = 90f;

        [Header("4D Hose Profile (Russian layout г/н and щ/з)")]
        [SerializeField] private float hoseProfileUnitsStepPerSecond = 1000f;
        [SerializeField] private KeyCode hoseOpeningKey = KeyCode.U;
        [SerializeField] private KeyCode hoseOpeningDecreaseKey = KeyCode.Y;
        [SerializeField] private KeyCode hoseWallCurvatureKey = KeyCode.O;
        [SerializeField] private KeyCode hoseWallCurvatureDecreaseKey = KeyCode.P;
        [SerializeField] private KeyCode hoseChromaticAberrationToggleKey = KeyCode.LeftBracket;
        [SerializeField] private KeyCode hoseShakeAndNextImageKey = KeyCode.Space;

        [Header("5D Mobius Flight")]
        [SerializeField] private float fiveDFlightSpeedUnitsStepPerSecond = 1000f;
        [SerializeField] private KeyCode fiveDFlightSpeedIncreaseKey = KeyCode.KeypadPlus;
        [SerializeField] private KeyCode fiveDFlightSpeedDecreaseKey = KeyCode.KeypadMinus;
        [SerializeField] private KeyCode fiveDFlightSpeedIncreaseFallbackKey = KeyCode.Equals;
        [SerializeField] private KeyCode fiveDFlightSpeedDecreaseFallbackKey = KeyCode.Minus;
        [SerializeField] private KeyCode fiveDShakeAndNextImageKey = KeyCode.Space;

        [Header("7D Strategy Switching")]
        [SerializeField] private KeyCode sevenDNextStrategyKey = KeyCode.KeypadPlus;
        [SerializeField] private KeyCode sevenDPreviousStrategyKey = KeyCode.KeypadMinus;
        [SerializeField] private KeyCode sevenDNextStrategyFallbackKey = KeyCode.Equals;
        [SerializeField] private KeyCode sevenDPreviousStrategyFallbackKey = KeyCode.Minus;

        [Header("6D Keys (Russian layout й / у / ц / ы / ф / в)")]
        [SerializeField] private ModeMotionInputProfile sixDMotionKeys = new ModeMotionInputProfile();

        [Header("7D Keys (Russian layout й / у / ц / ы / ф / в)")]
        [SerializeField] private ModeMotionInputProfile sevenDMotionKeys = new ModeMotionInputProfile();

        [Header("Audio Playback (Russian layout я / ч / с)")]
        [SerializeField] private KeyCode previousAudioTrackKey = KeyCode.Z;
        [SerializeField] private KeyCode toggleAudioPlaybackKey = KeyCode.X;
        [SerializeField] private KeyCode nextAudioTrackKey = KeyCode.C;

        private readonly TunnelBendController tunnelBendController = new TunnelBendController();

        public override string ModuleId
        {
            get { return "Input"; }
        }

        public override void Validate()
        {
            if (director == null)
            {
                ReportMissingReference("Director");
            }
        }

        public override void Tick(float deltaTime)
        {
            if (director == null)
            {
                return;
            }

            EnsureMotionKeyProfiles();

            if (UnityEngine.Input.GetKeyDown(toggleMenuKey))
            {
                director.Dispatch(KaleidoscopeCommand.ToggleControlMenu());
            }

            if (UnityEngine.Input.GetKeyDown(toggleHotkeysHelpKey))
            {
                director.Dispatch(KaleidoscopeCommand.ToggleHotkeysHelp());
            }

            if (director.State.HotkeysHelpVisible && UnityEngine.Input.GetKeyDown(closeMenuKey))
            {
                director.Dispatch(KaleidoscopeCommand.SetHotkeysHelpVisible(false));
            }
            else if (director.State.ControlMenuVisible && UnityEngine.Input.GetKeyDown(closeMenuKey))
            {
                director.Dispatch(KaleidoscopeCommand.SetControlMenuVisible(false));
            }

            if (UnityEngine.Input.GetKeyDown(diamondToggleKey))
            {
                director.Dispatch(KaleidoscopeCommand.ToggleDiamondFocus());
            }

            if (UnityEngine.Input.GetKeyDown(reanimateImageKey))
            {
                director.Dispatch(KaleidoscopeCommand.StartImageReanimation());
            }

            if (UnityEngine.Input.GetKeyDown(previousAudioTrackKey))
            {
                director.Dispatch(KaleidoscopeCommand.PreviousAudioTrack());
            }

            if (UnityEngine.Input.GetKeyDown(toggleAudioPlaybackKey))
            {
                director.Dispatch(KaleidoscopeCommand.ToggleAudioPlayback());
            }

            if (UnityEngine.Input.GetKeyDown(nextAudioTrackKey))
            {
                director.Dispatch(KaleidoscopeCommand.NextAudioTrack());
            }

            if (director.State.ImageReanimationActive)
            {
                return;
            }

            if (UnityEngine.Input.GetKeyDown(toggleGuidesKey) || UnityEngine.Input.GetKeyDown(toggleGuidesAlternateKey))
            {
                director.Dispatch(KaleidoscopeCommand.ToggleMirrorGuides());
            }

            if (UnityEngine.Input.GetKeyDown(cycleVisualModeKey))
            {
                CycleVisualMode();
            }

            // When the menu is open, avoid stealing navigation keys from UI.
            if (director.State.ControlMenuVisible)
            {
                return;
            }

            MirrorSettings mirror = director.State.MirrorSettings;
            if (mirror == null)
            {
                return;
            }

            if (UnityEngine.Input.GetKeyDown(sixSegmentsKey))
            {
                director.Dispatch(KaleidoscopeCommand.SetMirrorCount(6));
            }
            if (UnityEngine.Input.GetKeyDown(twelveSegmentsKey))
            {
                director.Dispatch(KaleidoscopeCommand.SetMirrorCount(12));
            }
            if (UnityEngine.Input.GetKeyDown(twentyFourSegmentsKey))
            {
                director.Dispatch(KaleidoscopeCommand.SetMirrorCount(24));
            }
            if (UnityEngine.Input.GetKeyDown(fortyEightSegmentsKey))
            {
                director.Dispatch(KaleidoscopeCommand.SetMirrorCount(48));
            }
            if (UnityEngine.Input.GetKeyDown(ninetySixSegmentsKey))
            {
                director.Dispatch(KaleidoscopeCommand.SetMirrorCount(96));
            }
            if (UnityEngine.Input.GetKeyDown(oneHundredNinetyTwoSegmentsKey))
            {
                director.Dispatch(KaleidoscopeCommand.SetMirrorCount(192));
            }
            if (UnityEngine.Input.GetKeyDown(threeHundredEightyFourSegmentsKey))
            {
                director.Dispatch(KaleidoscopeCommand.SetMirrorCount(384));
            }
            if (UnityEngine.Input.GetKeyDown(sevenHundredSixtyEightSegmentsKey))
            {
                director.Dispatch(KaleidoscopeCommand.SetMirrorCount(768));
            }
            if (UnityEngine.Input.GetKeyDown(fifteenHundredThirtySixSegmentsKey))
            {
                director.Dispatch(KaleidoscopeCommand.SetMirrorCount(1536));
            }

            float rotation = mirror.RotationSpeed;
            float zoom = mirror.Zoom;
            KaleidoscopeVisualMode visualMode = director.State.ActiveVisualMode;
            if (visualMode == KaleidoscopeVisualMode.Classic && UnityEngine.Input.GetKeyDown(classicShiftInertiaToggleKey))
            {
                director.Dispatch(KaleidoscopeCommand.ToggleVisualMotionImageInertia(KaleidoscopeVisualMode.Classic));
            }

            bool diamondControlsActive = director.State.DiamondFocusSettings != null
                && IsDiamondFocusModuleRegistered()
                && director.State.DiamondFocusSettings.Enabled;

            if (diamondControlsActive)
            {
                DispatchDiamondControls(deltaTime);
            }

            TunnelSettings tunnelSettings = director.State.TunnelSettings;
            Vector2 tunnelBend = tunnelSettings != null ? tunnelSettings.Bend : Vector2.zero;
            float hoseOpeningUnits = tunnelSettings != null ? tunnelSettings.HoseOpeningUnits : 0f;
            float hoseWallCurvatureUnits = tunnelSettings != null ? tunnelSettings.HoseWallCurvatureUnits : 0f;
            FiveDSettings fiveDSettings = director.State.FiveDSettings;
            float fiveDFlightSpeedUnits = fiveDSettings != null ? fiveDSettings.FlightSpeedUnits : 0f;
            VisualMotionSettings visualMotionSettings = director.State.GetVisualMotionSettings(visualMode);
            float visualFlightSpeedUnits = visualMotionSettings != null ? visualMotionSettings.FlightSpeedUnits : 0f;
            Vector2 visualImageOffset = visualMotionSettings != null ? visualMotionSettings.ImageOffset : Vector2.zero;
            Vector2 visualImageVelocity = visualMotionSettings != null ? visualMotionSettings.ImageOffsetVelocity : Vector2.zero;
            ModeMotionInputProfile motionKeys = GetModeMotionKeys(visualMode);
            Vector2 hoseInput = Vector2.zero;

            // Hold keys for continuous change.
            float perSecond = Mathf.Max(0f, unitsStepPerSecond) * deltaTime;
            float zoomDelta = Mathf.Max(0f, zoomStepPerSecond) * deltaTime;
            float bendDelta = Mathf.Max(0f, bendStepPerSecond) * deltaTime;
            float hoseProfileDelta = Mathf.Max(1000f, hoseProfileUnitsStepPerSecond) * deltaTime;
            float fiveDFlightDelta = Mathf.Max(0f, fiveDFlightSpeedUnitsStepPerSecond) * deltaTime;
            float visualMotionFlightDelta = motionKeys != null ? Mathf.Max(0f, motionKeys.FlightSpeedUnitsStepPerSecond) * deltaTime : 0f;
            float visualMotionOffsetDelta = motionKeys != null ? Mathf.Max(0f, motionKeys.ImageOffsetStepPerSecond) * deltaTime : 0f;
            bool classicInertialShift = visualMode == KaleidoscopeVisualMode.Classic
                && visualMotionSettings != null
                && visualMotionSettings.ImageShiftInertiaEnabled;

            if (UnityEngine.Input.GetKey(zoomInKey))
            {
                zoom += zoomDelta;
            }
            if (UnityEngine.Input.GetKey(zoomOutKey))
            {
                zoom -= zoomDelta;
            }
            if (UnityEngine.Input.GetKey(rotationLeftKey))
            {
                rotation -= perSecond;
            }
            if (UnityEngine.Input.GetKey(rotationRightKey))
            {
                rotation += perSecond;
            }

            if (motionKeys != null && visualMotionSettings != null)
            {
                if (UnityEngine.Input.GetKey(motionKeys.FlightForwardKey))
                {
                    visualFlightSpeedUnits += visualMotionFlightDelta;
                }
                if (UnityEngine.Input.GetKey(motionKeys.FlightBackwardKey))
                {
                    visualFlightSpeedUnits -= visualMotionFlightDelta;
                }

                Vector2 imageMotionInput = Vector2.zero;
                if (UnityEngine.Input.GetKey(motionKeys.MoveUpKey))
                {
                    imageMotionInput.y += 1f;
                }
                if (UnityEngine.Input.GetKey(motionKeys.MoveDownKey))
                {
                    imageMotionInput.y -= 1f;
                }
                if (UnityEngine.Input.GetKey(motionKeys.MoveLeftKey))
                {
                    imageMotionInput.x -= 1f;
                }
                if (UnityEngine.Input.GetKey(motionKeys.MoveRightKey))
                {
                    imageMotionInput.x += 1f;
                }

                if (imageMotionInput.sqrMagnitude > 1f)
                {
                    imageMotionInput.Normalize();
                }

                if (classicInertialShift)
                {
                    float maxShiftSpeed = Mathf.Max(0.01f, classicShiftMaxSpeed);
                    if (imageMotionInput.sqrMagnitude > 0.0001f)
                    {
                        Vector2 targetVelocity = imageMotionInput * maxShiftSpeed;
                        float acceleration = Mathf.Max(0f, classicShiftAccelerationPerSecond) * deltaTime;
                        visualImageVelocity = Vector2.MoveTowards(visualImageVelocity, targetVelocity, acceleration);
                    }
                    else
                    {
                        float stopSeconds = Mathf.Max(0.1f, classicShiftInertiaStopSeconds);
                        float deceleration = maxShiftSpeed / stopSeconds * deltaTime;
                        visualImageVelocity = Vector2.MoveTowards(visualImageVelocity, Vector2.zero, deceleration);
                    }

                    if (visualImageVelocity.sqrMagnitude > 0.000001f)
                    {
                        visualImageOffset += visualImageVelocity * deltaTime;
                    }
                }
                else if (imageMotionInput.sqrMagnitude > 0.0001f)
                {
                    visualImageOffset += imageMotionInput * visualMotionOffsetDelta;
                }
                else if (visualImageVelocity.sqrMagnitude > 0.000001f)
                {
                    visualImageVelocity = Vector2.zero;
                }

            }

            if (motionKeys != null && visualMotionSettings != null && IsModeShakePressed(visualMode, motionKeys))
            {
                director.Dispatch(KaleidoscopeCommand.TriggerVisualMotionShake(visualMode));
                director.Dispatch(KaleidoscopeCommand.TriggerSourceNextImage());
            }

            if (visualMode == KaleidoscopeVisualMode.Tunnel)
            {
                if (UnityEngine.Input.GetKey(tunnelBendUpKey))
                {
                    tunnelBend.y += bendDelta;
                }
                if (UnityEngine.Input.GetKey(tunnelBendDownKey))
                {
                    tunnelBend.y -= bendDelta;
                }
                if (UnityEngine.Input.GetKey(tunnelBendLeftKey))
                {
                    tunnelBend.x -= bendDelta;
                }
                if (UnityEngine.Input.GetKey(tunnelBendRightKey))
                {
                    tunnelBend.x += bendDelta;
                }
            }

            if (visualMode == KaleidoscopeVisualMode.Hose)
            {
                if (UnityEngine.Input.GetKey(hoseBendUpKey))
                {
                    hoseInput.y += 1f;
                }
                if (UnityEngine.Input.GetKey(hoseBendDownKey))
                {
                    hoseInput.y -= 1f;
                }
                if (UnityEngine.Input.GetKey(hoseBendLeftKey))
                {
                    hoseInput.x -= 1f;
                }
                if (UnityEngine.Input.GetKey(hoseBendRightKey))
                {
                    hoseInput.x += 1f;
                }

                if (UnityEngine.Input.GetKey(hoseOpeningKey))
                {
                    hoseOpeningUnits += hoseProfileDelta;
                }
                if (UnityEngine.Input.GetKey(hoseOpeningDecreaseKey))
                {
                    hoseOpeningUnits -= hoseProfileDelta;
                }
                if (UnityEngine.Input.GetKey(hoseWallCurvatureKey))
                {
                    hoseWallCurvatureUnits += hoseProfileDelta;
                }
                if (UnityEngine.Input.GetKey(hoseWallCurvatureDecreaseKey))
                {
                    hoseWallCurvatureUnits -= hoseProfileDelta;
                }

                if (UnityEngine.Input.GetKeyDown(hoseChromaticAberrationToggleKey))
                {
                    director.Dispatch(KaleidoscopeCommand.ToggleTunnelHoseChromaticAberration());
                }
            }

            if (visualMode == KaleidoscopeVisualMode.FiveD)
            {
                if ((!diamondControlsActive && UnityEngine.Input.GetKey(fiveDFlightSpeedIncreaseKey)) || UnityEngine.Input.GetKey(fiveDFlightSpeedIncreaseFallbackKey))
                {
                    fiveDFlightSpeedUnits += fiveDFlightDelta;
                }
                if ((!diamondControlsActive && UnityEngine.Input.GetKey(fiveDFlightSpeedDecreaseKey)) || UnityEngine.Input.GetKey(fiveDFlightSpeedDecreaseFallbackKey))
                {
                    fiveDFlightSpeedUnits -= fiveDFlightDelta;
                }
                if (UnityEngine.Input.GetKeyDown(fiveDShakeAndNextImageKey))
                {
                    director.Dispatch(KaleidoscopeCommand.TriggerFiveDShake());
                    director.Dispatch(KaleidoscopeCommand.TriggerSourceNextImage());
                }
            }

            if (visualMode == KaleidoscopeVisualMode.SevenD)
            {
                if ((!diamondControlsActive && UnityEngine.Input.GetKeyDown(sevenDNextStrategyKey)) || UnityEngine.Input.GetKeyDown(sevenDNextStrategyFallbackKey))
                {
                    director.Dispatch(KaleidoscopeCommand.CycleSevenDStrategy(1));
                }

                if ((!diamondControlsActive && UnityEngine.Input.GetKeyDown(sevenDPreviousStrategyKey)) || UnityEngine.Input.GetKeyDown(sevenDPreviousStrategyFallbackKey))
                {
                    director.Dispatch(KaleidoscopeCommand.CycleSevenDStrategy(-1));
                }
            }

            if (!diamondControlsActive && UnityEngine.Input.GetKeyDown(resetSpeedsKey))
            {
                rotation = 0f;
                tunnelBend = Vector2.zero;
                hoseOpeningUnits = 0f;
                hoseWallCurvatureUnits = 0f;
                fiveDFlightSpeedUnits = 0f;
                visualFlightSpeedUnits = 0f;
                visualImageOffset = Vector2.zero;
                visualImageVelocity = Vector2.zero;
                tunnelBendController.Reset(director.State);
                director.Dispatch(KaleidoscopeCommand.ResetTunnelHoseProfile());
                ResetAllVisualMotion();
            }

            rotation = Mathf.Clamp(rotation, MirrorSettings.RotationSpeedMinUnits, MirrorSettings.RotationSpeedMaxUnits);
            zoom = Mathf.Clamp(zoom, 0.1f, 8f);
            tunnelBend = new Vector2(Mathf.Clamp(tunnelBend.x, -1f, 1f), Mathf.Clamp(tunnelBend.y, -1f, 1f));
            hoseOpeningUnits = Mathf.Clamp(hoseOpeningUnits, TunnelSettings.HoseProfileMinUnits, TunnelSettings.HoseProfileMaxUnits);
            hoseWallCurvatureUnits = Mathf.Clamp(hoseWallCurvatureUnits, TunnelSettings.HoseProfileMinUnits, TunnelSettings.HoseProfileMaxUnits);
            fiveDFlightSpeedUnits = Mathf.Clamp(fiveDFlightSpeedUnits, FiveDSettings.FlightSpeedMinUnits, FiveDSettings.FlightSpeedMaxUnits);
            visualFlightSpeedUnits = Mathf.Clamp(visualFlightSpeedUnits, VisualMotionSettings.FlightSpeedMinUnits, VisualMotionSettings.FlightSpeedMaxUnits);

            if (!Mathf.Approximately(zoom, mirror.Zoom))
            {
                director.Dispatch(KaleidoscopeCommand.SetMirrorZoom(zoom));
            }

            if (!Mathf.Approximately(rotation, mirror.RotationSpeed))
            {
                director.Dispatch(KaleidoscopeCommand.SetMirrorRotationSpeedUnits(rotation));
            }

            if (tunnelSettings != null && tunnelBend != tunnelSettings.Bend)
            {
                director.Dispatch(KaleidoscopeCommand.SetTunnelBend(tunnelBend));
            }

            if (tunnelSettings != null && !Mathf.Approximately(hoseOpeningUnits, tunnelSettings.HoseOpeningUnits))
            {
                director.Dispatch(KaleidoscopeCommand.SetTunnelHoseOpeningUnits(hoseOpeningUnits));
            }

            if (tunnelSettings != null && !Mathf.Approximately(hoseWallCurvatureUnits, tunnelSettings.HoseWallCurvatureUnits))
            {
                director.Dispatch(KaleidoscopeCommand.SetTunnelHoseWallCurvatureUnits(hoseWallCurvatureUnits));
            }

            if (fiveDSettings != null && !Mathf.Approximately(fiveDFlightSpeedUnits, fiveDSettings.FlightSpeedUnits))
            {
                director.Dispatch(KaleidoscopeCommand.SetFiveDFlightSpeedUnits(fiveDFlightSpeedUnits));
            }

            if (visualMotionSettings != null && !Mathf.Approximately(visualFlightSpeedUnits, visualMotionSettings.FlightSpeedUnits))
            {
                director.Dispatch(KaleidoscopeCommand.SetVisualMotionFlightSpeedUnits(visualMode, visualFlightSpeedUnits));
            }

            if (visualMotionSettings != null && visualImageOffset != visualMotionSettings.ImageOffset)
            {
                director.Dispatch(KaleidoscopeCommand.SetVisualMotionImageOffset(visualMode, visualImageOffset));
            }

            if (visualMotionSettings != null && visualImageVelocity != visualMotionSettings.ImageOffsetVelocity)
            {
                director.Dispatch(KaleidoscopeCommand.SetVisualMotionImageVelocity(visualMode, visualImageVelocity));
            }

            if (visualMode == KaleidoscopeVisualMode.Hose)
            {
                tunnelBendController.Tick(director.State, hoseInput, deltaTime);
            }
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            if (director == null)
            {
                return CreateStatus("Waiting for Director reference.");
            }

            MirrorSettings mirror = director.State.MirrorSettings;
            if (mirror == null)
            {
                return CreateStatus("Waiting for state.");
            }

            Vector2 bend = director.State.TunnelSettings != null ? director.State.TunnelSettings.Bend : Vector2.zero;
            Vector2 hoseBend = director.State.TunnelBendState != null ? director.State.TunnelBendState.BendOffset : Vector2.zero;
            TunnelSettings tunnelSettings = director.State.TunnelSettings;
            FiveDSettings fiveDSettings = director.State.FiveDSettings;
            float opening = tunnelSettings != null ? tunnelSettings.HoseOpeningUnits : 0f;
            float curvature = tunnelSettings != null ? tunnelSettings.HoseWallCurvatureUnits : 0f;
            bool chromaticAberration = tunnelSettings != null && tunnelSettings.HoseChromaticAberrationEnabled;
            float flightSpeed = fiveDSettings != null ? fiveDSettings.FlightSpeedUnits : 0f;
            VisualMotionSettings visualMotion = director.State.GetVisualMotionSettings(director.State.ActiveVisualMode);
            float modeFlightSpeed = visualMotion != null ? visualMotion.FlightSpeedUnits : 0f;
            bool classicInertia = director.State.ActiveVisualMode == KaleidoscopeVisualMode.Classic
                && visualMotion != null
                && visualMotion.ImageShiftInertiaEnabled;
            SevenDSettings sevenDSettings = director.State.SevenDSettings;
            string sevenDStrategy = sevenDSettings != null ? sevenDSettings.StrategyLabel : "None";
            return CreateStatus("Zoom " + mirror.Zoom.ToString("0.00") + ", Rotation " + mirror.RotationSpeed.ToString("0") + ", 3D bend " + bend.ToString("0.00") + ", 4D hose " + hoseBend.ToString("0.00") + ", G " + opening.ToString("0") + ", Shch " + curvature.ToString("0") + ", 4D CA " + (chromaticAberration ? "on" : "off") + ", 5D flight " + flightSpeed.ToString("0") + ", mode flight " + modeFlightSpeed.ToString("0") + ", 2D inertia " + (classicInertia ? "on" : "off") + ", 7D " + sevenDStrategy + ".");
        }

        private void CycleVisualMode()
        {
            KaleidoscopeVisualMode current = director.State.ActiveVisualMode;
            KaleidoscopeVisualMode next = current == KaleidoscopeVisualMode.Classic
                ? KaleidoscopeVisualMode.Tunnel
                : current == KaleidoscopeVisualMode.Tunnel
                    ? KaleidoscopeVisualMode.Hose
                    : current == KaleidoscopeVisualMode.Hose
                        ? KaleidoscopeVisualMode.FiveD
                        : current == KaleidoscopeVisualMode.FiveD
                            ? KaleidoscopeVisualMode.SixD
                            : current == KaleidoscopeVisualMode.SixD
                                ? KaleidoscopeVisualMode.SevenD
                            : KaleidoscopeVisualMode.Classic;

            director.Dispatch(KaleidoscopeCommand.SetVisualMode(next));
        }

        private ModeMotionInputProfile GetModeMotionKeys(KaleidoscopeVisualMode visualMode)
        {
            switch (visualMode)
            {
                case KaleidoscopeVisualMode.Classic:
                    return classicMotionKeys;
                case KaleidoscopeVisualMode.Tunnel:
                    return tunnelMotionKeys;
                case KaleidoscopeVisualMode.Hose:
                    return hoseMotionKeys;
                case KaleidoscopeVisualMode.SixD:
                    return sixDMotionKeys;
                case KaleidoscopeVisualMode.SevenD:
                    return sevenDMotionKeys;
                default:
                    return null;
            }
        }

        private void EnsureMotionKeyProfiles()
        {
            if (classicMotionKeys == null)
            {
                classicMotionKeys = new ModeMotionInputProfile();
            }

            if (tunnelMotionKeys == null)
            {
                tunnelMotionKeys = new ModeMotionInputProfile();
            }

            if (hoseMotionKeys == null)
            {
                hoseMotionKeys = new ModeMotionInputProfile();
            }

            if (sixDMotionKeys == null)
            {
                sixDMotionKeys = new ModeMotionInputProfile();
            }

            if (sevenDMotionKeys == null)
            {
                sevenDMotionKeys = new ModeMotionInputProfile();
            }
        }

        private bool IsModeShakePressed(KaleidoscopeVisualMode visualMode, ModeMotionInputProfile motionKeys)
        {
            if (motionKeys == null)
            {
                return false;
            }

            bool pressed = UnityEngine.Input.GetKeyDown(motionKeys.ShakeAndNextImageKey);
            if (visualMode == KaleidoscopeVisualMode.Hose && hoseShakeAndNextImageKey != motionKeys.ShakeAndNextImageKey)
            {
                pressed = pressed || UnityEngine.Input.GetKeyDown(hoseShakeAndNextImageKey);
            }

            return pressed;
        }

        private void ResetAllVisualMotion()
        {
            director.Dispatch(KaleidoscopeCommand.ResetVisualMotion(KaleidoscopeVisualMode.Classic));
            director.Dispatch(KaleidoscopeCommand.ResetVisualMotion(KaleidoscopeVisualMode.Tunnel));
            director.Dispatch(KaleidoscopeCommand.ResetVisualMotion(KaleidoscopeVisualMode.Hose));
            director.Dispatch(KaleidoscopeCommand.ResetVisualMotion(KaleidoscopeVisualMode.SixD));
            director.Dispatch(KaleidoscopeCommand.ResetVisualMotion(KaleidoscopeVisualMode.SevenD));
        }

        private void DispatchDiamondControls(float deltaTime)
        {
            Vector2 direction = DiamondInputRouter.NormalizeNumpadRotationInput(
                UnityEngine.Input.GetKey(diamondRotateDownLeftKey),
                UnityEngine.Input.GetKey(diamondRotateDownKeypadKey),
                UnityEngine.Input.GetKey(diamondRotateDownRightKey),
                UnityEngine.Input.GetKey(diamondRotateLeftKeypadKey),
                UnityEngine.Input.GetKey(diamondRotateRightKeypadKey),
                UnityEngine.Input.GetKey(diamondRotateUpLeftKey),
                UnityEngine.Input.GetKey(diamondRotateUpKeypadKey),
                UnityEngine.Input.GetKey(diamondRotateUpRightKey));

            if (diamondLegacyWasdControlsEnabled)
            {
                direction += DiamondInputRouter.NormalizeDirectionInput(
                    UnityEngine.Input.GetKey(diamondRotateLeftKey),
                    UnityEngine.Input.GetKey(diamondRotateRightKey),
                    UnityEngine.Input.GetKey(diamondRotateUpKey),
                    UnityEngine.Input.GetKey(diamondRotateDownKey));
                if (direction.sqrMagnitude > 1f)
                {
                    direction.Normalize();
                }
            }

            DiamondFocusSettings settings = director.State.DiamondFocusSettings;
            if (settings == null || direction != settings.TargetRotationDirection)
            {
                DiamondInputRouter.DispatchDirection(director, direction);
            }

            float speedDelta = 0f;
            float step = Mathf.Max(0f, diamondSpeedStepPerSecond) * Mathf.Max(0f, deltaTime);
            if (diamondLegacyWasdControlsEnabled && UnityEngine.Input.GetKey(diamondSpeedIncreaseKey))
            {
                speedDelta += step;
            }

            if (diamondLegacyWasdControlsEnabled && UnityEngine.Input.GetKey(diamondSpeedDecreaseKey))
            {
                speedDelta -= step;
            }

            DiamondInputRouter.DispatchSpeedDelta(director, speedDelta);

            if (UnityEngine.Input.GetKeyDown(diamondNextShapeKey))
            {
                director.Dispatch(KaleidoscopeCommand.NextDiamondShape());
            }

            if (UnityEngine.Input.GetKeyDown(diamondPreviousShapeKey))
            {
                director.Dispatch(KaleidoscopeCommand.PreviousDiamondShape());
            }

            if (UnityEngine.Input.GetKeyDown(diamondNextMaterialModeKey))
            {
                director.Dispatch(KaleidoscopeCommand.CycleDiamondMaterialMode(1));
            }
        }

        private bool IsDiamondFocusModuleRegistered()
        {
            if (director == null)
            {
                return false;
            }

            var modules = director.RegisteredModules;
            for (int index = 0; index < modules.Count; index++)
            {
                IKaleidoscopeModule module = modules[index];
                if (module != null && module.ModuleId == DiamondFocusModuleId)
                {
                    return true;
                }
            }

            return false;
        }

        [Serializable]
        private sealed class ModeMotionInputProfile
        {
            [SerializeField] private KeyCode flightForwardKey = KeyCode.Q;
            [SerializeField] private KeyCode flightBackwardKey = KeyCode.E;
            [SerializeField] private KeyCode moveUpKey = KeyCode.W;
            [SerializeField] private KeyCode moveDownKey = KeyCode.S;
            [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
            [SerializeField] private KeyCode moveRightKey = KeyCode.D;
            [SerializeField] private KeyCode shakeAndNextImageKey = KeyCode.Space;
            [SerializeField] private float flightSpeedUnitsStepPerSecond = 1000f;
            [SerializeField] private float imageOffsetStepPerSecond = 0.45f;

            public KeyCode FlightForwardKey
            {
                get { return flightForwardKey; }
            }

            public KeyCode FlightBackwardKey
            {
                get { return flightBackwardKey; }
            }

            public KeyCode MoveUpKey
            {
                get { return moveUpKey; }
            }

            public KeyCode MoveDownKey
            {
                get { return moveDownKey; }
            }

            public KeyCode MoveLeftKey
            {
                get { return moveLeftKey; }
            }

            public KeyCode MoveRightKey
            {
                get { return moveRightKey; }
            }

            public KeyCode ShakeAndNextImageKey
            {
                get { return shakeAndNextImageKey; }
            }

            public float FlightSpeedUnitsStepPerSecond
            {
                get { return flightSpeedUnitsStepPerSecond; }
            }

            public float ImageOffsetStepPerSecond
            {
                get { return imageOffsetStepPerSecond; }
            }
        }
    }
}
