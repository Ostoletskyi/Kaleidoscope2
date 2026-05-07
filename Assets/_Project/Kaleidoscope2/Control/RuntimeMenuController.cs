using System;
using System.Collections.Generic;
using Kaleidoscope2.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Control
{
    [DisallowMultipleComponent]
    public sealed class RuntimeMenuController : MonoBehaviour
    {
        [SerializeField] private KaleidoscopeDirector director;
        [SerializeField] private RawImage outputImage;
        [SerializeField] private Dropdown sourceModeDropdown;
        [SerializeField] private Slider mirrorCountSlider;
        [SerializeField] private Text mirrorCountValue;
        [SerializeField] private Slider zoomSlider;
        [SerializeField] private Text zoomValue;
        [SerializeField] private Slider rotationSpeedSlider;
        [SerializeField] private Text rotationSpeedValue;
        [SerializeField] private Button resetCenterOffsetButton;
        [SerializeField] private Button toggleDiagnosticsButton;
        [SerializeField] private Text diagnosticsButtonLabel;
        [SerializeField] private Toggle tunnelModeToggle;
        [SerializeField] private Button recordingButton;
        [SerializeField] private Text recordingButtonLabel;

        private bool suppressCallbacks;

        private void Awake()
        {
            EnsureSourceDropdownOptions();
            ConfigureSliderRanges();
        }

        private void OnEnable()
        {
            if (sourceModeDropdown != null)
            {
                sourceModeDropdown.onValueChanged.AddListener(OnSourceModeChanged);
            }

            if (mirrorCountSlider != null)
            {
                mirrorCountSlider.onValueChanged.AddListener(OnMirrorCountChanged);
            }

            if (zoomSlider != null)
            {
                zoomSlider.onValueChanged.AddListener(OnZoomChanged);
            }

            if (rotationSpeedSlider != null)
            {
                rotationSpeedSlider.onValueChanged.AddListener(OnRotationSpeedChanged);
            }

            if (resetCenterOffsetButton != null)
            {
                resetCenterOffsetButton.onClick.AddListener(OnResetCenterOffset);
            }

            if (toggleDiagnosticsButton != null)
            {
                toggleDiagnosticsButton.onClick.AddListener(OnToggleDiagnostics);
            }

            if (tunnelModeToggle != null)
            {
                tunnelModeToggle.onValueChanged.AddListener(OnTunnelModeChanged);
            }

            if (recordingButton != null)
            {
                recordingButton.onClick.AddListener(OnRecordingPlaceholderClicked);
            }
        }

        private void Start()
        {
            RefreshControlsFromState();
            UpdateOutputTexture();
        }

        private void Update()
        {
            RefreshControlsFromState();
            UpdateOutputTexture();
        }

        private void OnDisable()
        {
            if (sourceModeDropdown != null)
            {
                sourceModeDropdown.onValueChanged.RemoveListener(OnSourceModeChanged);
            }

            if (mirrorCountSlider != null)
            {
                mirrorCountSlider.onValueChanged.RemoveListener(OnMirrorCountChanged);
            }

            if (zoomSlider != null)
            {
                zoomSlider.onValueChanged.RemoveListener(OnZoomChanged);
            }

            if (rotationSpeedSlider != null)
            {
                rotationSpeedSlider.onValueChanged.RemoveListener(OnRotationSpeedChanged);
            }

            if (resetCenterOffsetButton != null)
            {
                resetCenterOffsetButton.onClick.RemoveListener(OnResetCenterOffset);
            }

            if (toggleDiagnosticsButton != null)
            {
                toggleDiagnosticsButton.onClick.RemoveListener(OnToggleDiagnostics);
            }

            if (tunnelModeToggle != null)
            {
                tunnelModeToggle.onValueChanged.RemoveListener(OnTunnelModeChanged);
            }

            if (recordingButton != null)
            {
                recordingButton.onClick.RemoveListener(OnRecordingPlaceholderClicked);
            }
        }

        private void OnSourceModeChanged(int value)
        {
            if (suppressCallbacks || director == null)
            {
                return;
            }

            director.Dispatch(KaleidoscopeCommand.SetSourceMode((KaleidoscopeSourceMode)value));
        }

        private void OnMirrorCountChanged(float value)
        {
            if (suppressCallbacks || director == null)
            {
                return;
            }

            director.Dispatch(KaleidoscopeCommand.SetMirrorCount(Mathf.RoundToInt(value)));
        }

        private void OnZoomChanged(float value)
        {
            if (suppressCallbacks || director == null)
            {
                return;
            }

            director.Dispatch(KaleidoscopeCommand.SetMirrorZoom(value));
        }

        private void OnRotationSpeedChanged(float value)
        {
            if (suppressCallbacks || director == null)
            {
                return;
            }

            director.Dispatch(KaleidoscopeCommand.SetMirrorRotationSpeed(value));
        }

        private void OnResetCenterOffset()
        {
            if (director == null)
            {
                return;
            }

            director.Dispatch(KaleidoscopeCommand.SetMirrorCenterOffset(Vector2.zero));
        }

        private void OnToggleDiagnostics()
        {
            if (director == null)
            {
                return;
            }

            bool visible = !director.State.Diagnostics.HudVisible;
            director.Dispatch(KaleidoscopeCommand.SetDiagnosticsVisible(visible));
        }

        private void OnTunnelModeChanged(bool enabled)
        {
            if (suppressCallbacks || director == null)
            {
                return;
            }

            director.Dispatch(KaleidoscopeCommand.SetTunnelEnabled(enabled));
        }

        private void OnRecordingPlaceholderClicked()
        {
            if (director == null)
            {
                return;
            }

            KaleidoscopeRecordingStatus nextStatus = director.State.RecordingStatus == KaleidoscopeRecordingStatus.Idle
                ? KaleidoscopeRecordingStatus.Recording
                : KaleidoscopeRecordingStatus.Idle;

            director.Dispatch(KaleidoscopeCommand.SetRecordingStatus(nextStatus));
        }

        private void RefreshControlsFromState()
        {
            if (director == null)
            {
                return;
            }

            suppressCallbacks = true;

            KaleidoscopeState state = director.State;

            if (sourceModeDropdown != null)
            {
                sourceModeDropdown.SetValueWithoutNotify((int)state.ActiveSourceMode);
            }

            if (mirrorCountSlider != null)
            {
                mirrorCountSlider.SetValueWithoutNotify(state.MirrorSettings.MirrorCount);
            }

            if (mirrorCountValue != null)
            {
                mirrorCountValue.text = state.MirrorSettings.MirrorCount.ToString();
            }

            if (zoomSlider != null)
            {
                zoomSlider.SetValueWithoutNotify(state.MirrorSettings.Zoom);
            }

            if (zoomValue != null)
            {
                zoomValue.text = state.MirrorSettings.Zoom.ToString("0.00");
            }

            if (rotationSpeedSlider != null)
            {
                rotationSpeedSlider.SetValueWithoutNotify(state.MirrorSettings.RotationSpeed);
            }

            if (rotationSpeedValue != null)
            {
                rotationSpeedValue.text = state.MirrorSettings.RotationSpeed.ToString("0.0");
            }

            if (tunnelModeToggle != null)
            {
                tunnelModeToggle.SetIsOnWithoutNotify(state.TunnelEnabled);
            }

            if (diagnosticsButtonLabel != null)
            {
                diagnosticsButtonLabel.text = state.Diagnostics.HudVisible ? "Hide Diagnostics" : "Show Diagnostics";
            }

            if (recordingButtonLabel != null)
            {
                recordingButtonLabel.text = state.RecordingStatus == KaleidoscopeRecordingStatus.Idle
                    ? "Recording Placeholder"
                    : "Stop Placeholder Recording";
            }

            suppressCallbacks = false;
        }

        private void UpdateOutputTexture()
        {
            if (director == null || outputImage == null)
            {
                return;
            }

            Texture outputTexture = director.FinalOutputTexture;
            if (outputImage.texture != outputTexture)
            {
                outputImage.texture = outputTexture;
            }
        }

        private void EnsureSourceDropdownOptions()
        {
            if (sourceModeDropdown == null || sourceModeDropdown.options.Count > 0)
            {
                return;
            }

            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            Array values = Enum.GetValues(typeof(KaleidoscopeSourceMode));

            for (int index = 0; index < values.Length; index++)
            {
                options.Add(new Dropdown.OptionData(values.GetValue(index).ToString()));
            }

            sourceModeDropdown.options = options;
        }

        private void ConfigureSliderRanges()
        {
            if (mirrorCountSlider != null)
            {
                mirrorCountSlider.minValue = 1f;
                mirrorCountSlider.maxValue = 16f;
                mirrorCountSlider.wholeNumbers = true;
            }

            if (zoomSlider != null)
            {
                zoomSlider.minValue = 0.25f;
                zoomSlider.maxValue = 4f;
                zoomSlider.wholeNumbers = false;
            }

            if (rotationSpeedSlider != null)
            {
                rotationSpeedSlider.minValue = -180f;
                rotationSpeedSlider.maxValue = 180f;
                rotationSpeedSlider.wholeNumbers = false;
            }
        }
    }
}
