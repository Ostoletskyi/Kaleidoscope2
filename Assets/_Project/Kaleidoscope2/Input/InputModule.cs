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
        [SerializeField] private KeyCode sixSegmentsKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode twelveSegmentsKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode twentyFourSegmentsKey = KeyCode.Alpha3;

        [Header("Center Focus (WASD / Russian layout ф ц ы в)")]
        [SerializeField] private float centerStepPerSecond = 1.25f;
        [SerializeField] private KeyCode centerUpKey = KeyCode.W;
        [SerializeField] private KeyCode centerLeftKey = KeyCode.A;
        [SerializeField] private KeyCode centerDownKey = KeyCode.S;
        [SerializeField] private KeyCode centerRightKey = KeyCode.D;

        [Header("Tunnel Bend (P / ; / L / ')")]
        [SerializeField] private KeyCode tunnelBendUpKey = KeyCode.P;
        [SerializeField] private KeyCode tunnelBendDownKey = KeyCode.Semicolon;
        [SerializeField] private KeyCode tunnelBendLeftKey = KeyCode.L;
        [SerializeField] private KeyCode tunnelBendRightKey = KeyCode.Quote;

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
            Vector2 centerOffset = mirror.CenterOffset;
            Vector2 tunnelInput = Vector2.zero;

            // Hold keys for continuous change.
            float perSecond = Mathf.Max(0f, unitsStepPerSecond) * deltaTime;
            float zoomDelta = Mathf.Max(0f, zoomStepPerSecond) * deltaTime;
            float centerDelta = Mathf.Max(0f, centerStepPerSecond) * deltaTime;

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

            if (UnityEngine.Input.GetKey(centerUpKey))
            {
                centerOffset.y += centerDelta;
            }
            if (UnityEngine.Input.GetKey(centerDownKey))
            {
                centerOffset.y -= centerDelta;
            }
            if (UnityEngine.Input.GetKey(centerLeftKey))
            {
                centerOffset.x -= centerDelta;
            }
            if (UnityEngine.Input.GetKey(centerRightKey))
            {
                centerOffset.x += centerDelta;
            }

            centerOffset = new Vector2(Mathf.Clamp(centerOffset.x, -1f, 1f), Mathf.Clamp(centerOffset.y, -1f, 1f));

            if (director.State.TunnelEnabled)
            {
                if (UnityEngine.Input.GetKey(tunnelBendUpKey))
                {
                    tunnelInput.y += 1f;
                }
                if (UnityEngine.Input.GetKey(tunnelBendDownKey))
                {
                    tunnelInput.y -= 1f;
                }
                if (UnityEngine.Input.GetKey(tunnelBendLeftKey))
                {
                    tunnelInput.x -= 1f;
                }
                if (UnityEngine.Input.GetKey(tunnelBendRightKey))
                {
                    tunnelInput.x += 1f;
                }
            }

            if (UnityEngine.Input.GetKeyDown(resetSpeedsKey))
            {
                rotation = 0f;
            }

            rotation = Mathf.Clamp(rotation, -500f, 500f);
            zoom = Mathf.Clamp(zoom, 0.1f, 8f);

            if (!Mathf.Approximately(zoom, mirror.Zoom))
            {
                director.Dispatch(KaleidoscopeCommand.SetMirrorZoom(zoom));
            }

            if (!Mathf.Approximately(rotation, mirror.RotationSpeed))
            {
                director.Dispatch(KaleidoscopeCommand.SetMirrorRotationSpeedUnits(rotation));
            }

            if (centerOffset != mirror.CenterOffset)
            {
                director.Dispatch(KaleidoscopeCommand.SetMirrorCenterOffset(centerOffset));
            }

            if (director.State.TunnelEnabled)
            {
                tunnelBendController.Tick(director.State, tunnelInput, deltaTime);
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

            Vector2 center = mirror.CenterOffset;
            Vector2 bend = director.State.TunnelBendState != null ? director.State.TunnelBendState.BendOffset : Vector2.zero;
            return CreateStatus("Zoom " + mirror.Zoom.ToString("0.00") + ", Rotation " + mirror.RotationSpeed.ToString("0") + ", Center " + center.ToString("0.00") + ", Tunnel bend " + bend.ToString("0.00") + ".");
        }
    }
}
