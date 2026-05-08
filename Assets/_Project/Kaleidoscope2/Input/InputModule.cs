using Kaleidoscope2.Core;
using Kaleidoscope2.Tunnel;
using UnityEngine;

namespace Kaleidoscope2.InputSystem
{
    // Converts physical input to Director commands. No direct visual mutation.
    [DisallowMultipleComponent]
    public sealed class InputModule : KaleidoscopeModuleBase
    {
        [Header("References")]
        [SerializeField] private KaleidoscopeDirector director;

        [Header("Keys")]
        [SerializeField] private KeyCode toggleMenuKey = KeyCode.Mouse2;
        [SerializeField] private KeyCode closeMenuKey = KeyCode.Escape;
        [SerializeField] private KeyCode toggleGuidesKey = KeyCode.Keypad0;

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

        [Header("3D Tunnel Distortion (WASD / Russian layout ц ф ы в)")]
        [SerializeField] private float bendStepPerSecond = 1.25f;
        [SerializeField] private KeyCode tunnelBendUpKey = KeyCode.W;
        [SerializeField] private KeyCode tunnelBendLeftKey = KeyCode.A;
        [SerializeField] private KeyCode tunnelBendDownKey = KeyCode.S;
        [SerializeField] private KeyCode tunnelBendRightKey = KeyCode.D;

        [Header("4D Hose Bend (Russian layout ш л о д)")]
        [SerializeField] private KeyCode hoseBendUpKey = KeyCode.I;
        [SerializeField] private KeyCode hoseBendDownKey = KeyCode.K;
        [SerializeField] private KeyCode hoseBendLeftKey = KeyCode.J;
        [SerializeField] private KeyCode hoseBendRightKey = KeyCode.L;

        [Header("4D Hose Profile (Russian layout г/н and щ/з)")]
        [SerializeField] private float hoseProfileUnitsStepPerSecond = 1000f;
        [SerializeField] private KeyCode hoseOpeningKey = KeyCode.U;
        [SerializeField] private KeyCode hoseOpeningDecreaseKey = KeyCode.Y;
        [SerializeField] private KeyCode hoseWallCurvatureKey = KeyCode.O;
        [SerializeField] private KeyCode hoseWallCurvatureDecreaseKey = KeyCode.P;

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

            if (UnityEngine.Input.GetKeyDown(toggleMenuKey))
            {
                director.Dispatch(KaleidoscopeCommand.ToggleControlMenu());
            }

            if (director.State.ControlMenuVisible && UnityEngine.Input.GetKeyDown(closeMenuKey))
            {
                director.Dispatch(KaleidoscopeCommand.SetControlMenuVisible(false));
            }

            if (UnityEngine.Input.GetKeyDown(toggleGuidesKey))
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

            float rotation = mirror.RotationSpeed;
            float zoom = mirror.Zoom;
            KaleidoscopeVisualMode visualMode = director.State.ActiveVisualMode;
            TunnelSettings tunnelSettings = director.State.TunnelSettings;
            Vector2 tunnelBend = tunnelSettings != null ? tunnelSettings.Bend : Vector2.zero;
            float hoseOpeningUnits = tunnelSettings != null ? tunnelSettings.HoseOpeningUnits : 0f;
            float hoseWallCurvatureUnits = tunnelSettings != null ? tunnelSettings.HoseWallCurvatureUnits : 0f;
            Vector2 hoseInput = Vector2.zero;

            // Hold keys for continuous change.
            float perSecond = Mathf.Max(0f, unitsStepPerSecond) * deltaTime;
            float zoomDelta = Mathf.Max(0f, zoomStepPerSecond) * deltaTime;
            float bendDelta = Mathf.Max(0f, bendStepPerSecond) * deltaTime;
            float hoseProfileDelta = Mathf.Max(1000f, hoseProfileUnitsStepPerSecond) * deltaTime;

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
            }

            if (UnityEngine.Input.GetKeyDown(resetSpeedsKey))
            {
                rotation = 0f;
                tunnelBend = Vector2.zero;
                hoseOpeningUnits = 0f;
                hoseWallCurvatureUnits = 0f;
                tunnelBendController.Reset(director.State);
                director.Dispatch(KaleidoscopeCommand.ResetTunnelHoseProfile());
            }

            rotation = Mathf.Clamp(rotation, MirrorSettings.RotationSpeedMinUnits, MirrorSettings.RotationSpeedMaxUnits);
            zoom = Mathf.Clamp(zoom, 0.1f, 8f);
            tunnelBend = new Vector2(Mathf.Clamp(tunnelBend.x, -1f, 1f), Mathf.Clamp(tunnelBend.y, -1f, 1f));
            hoseOpeningUnits = Mathf.Clamp(hoseOpeningUnits, TunnelSettings.HoseProfileMinUnits, TunnelSettings.HoseProfileMaxUnits);
            hoseWallCurvatureUnits = Mathf.Clamp(hoseWallCurvatureUnits, TunnelSettings.HoseProfileMinUnits, TunnelSettings.HoseProfileMaxUnits);

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
            float opening = tunnelSettings != null ? tunnelSettings.HoseOpeningUnits : 0f;
            float curvature = tunnelSettings != null ? tunnelSettings.HoseWallCurvatureUnits : 0f;
            return CreateStatus("Zoom " + mirror.Zoom.ToString("0.00") + ", Rotation " + mirror.RotationSpeed.ToString("0") + ", 3D bend " + bend.ToString("0.00") + ", 4D hose " + hoseBend.ToString("0.00") + ", G " + opening.ToString("0") + ", Shch " + curvature.ToString("0") + ".");
        }

        private void CycleVisualMode()
        {
            KaleidoscopeVisualMode current = director.State.ActiveVisualMode;
            KaleidoscopeVisualMode next = current == KaleidoscopeVisualMode.Classic
                ? KaleidoscopeVisualMode.Tunnel
                : current == KaleidoscopeVisualMode.Tunnel
                    ? KaleidoscopeVisualMode.Hose
                    : KaleidoscopeVisualMode.Classic;

            director.Dispatch(KaleidoscopeCommand.SetVisualMode(next));
        }
    }
}
