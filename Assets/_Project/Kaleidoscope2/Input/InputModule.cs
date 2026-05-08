using Kaleidoscope2.Core;
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

        [Header("Speed Control (Units)")]
        // Legacy: older scene iterations serialized this. Kept for backward compatibility.
#pragma warning disable 649
        [SerializeField] private float unitsStepPerPress = 25f;
#pragma warning restore 649
        [SerializeField] private float unitsStepPerSecond = 220f;
        [SerializeField] private KeyCode forwardIncreaseKey = KeyCode.UpArrow;
        [SerializeField] private KeyCode forwardDecreaseKey = KeyCode.DownArrow;
        [SerializeField] private KeyCode rotationLeftKey = KeyCode.LeftArrow;
        [SerializeField] private KeyCode rotationRightKey = KeyCode.RightArrow;
        [SerializeField] private KeyCode resetSpeedsKey = KeyCode.Keypad5;

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

            float forward = mirror.ForwardSpeedUnits;
            float rotation = mirror.RotationSpeed;

            // Hold keys for continuous change.
            float perSecond = Mathf.Max(0f, unitsStepPerSecond) * deltaTime;

            if (UnityEngine.Input.GetKey(forwardIncreaseKey))
            {
                forward += perSecond;
            }
            if (UnityEngine.Input.GetKey(forwardDecreaseKey))
            {
                forward -= perSecond;
            }
            if (UnityEngine.Input.GetKey(rotationLeftKey))
            {
                rotation -= perSecond;
            }
            if (UnityEngine.Input.GetKey(rotationRightKey))
            {
                rotation += perSecond;
            }

            if (UnityEngine.Input.GetKeyDown(resetSpeedsKey))
            {
                forward = 0f;
                rotation = 0f;
            }

            forward = Mathf.Clamp(forward, -500f, 500f);
            rotation = Mathf.Clamp(rotation, -500f, 500f);

            if (!Mathf.Approximately(forward, mirror.ForwardSpeedUnits))
            {
                director.Dispatch(KaleidoscopeCommand.SetMirrorForwardSpeedUnits(forward));
            }

            if (!Mathf.Approximately(rotation, mirror.RotationSpeed))
            {
                director.Dispatch(KaleidoscopeCommand.SetMirrorRotationSpeedUnits(rotation));
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

            return CreateStatus("Forward " + mirror.ForwardSpeedUnits.ToString("0") + ", Rotation " + mirror.RotationSpeed.ToString("0") + ".");
        }
    }
}
