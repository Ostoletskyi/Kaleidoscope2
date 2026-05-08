using System;
using System.Collections.Generic;
using System.Text;
using Kaleidoscope2.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Control
{
    [DisallowMultipleComponent]
    public sealed class RuntimeMenuController : MonoBehaviour
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private const float ControlPanelWidth = 360f;
        private const float DiagnosticsPanelWidth = 460f;
        private const float DiagnosticsPanelHeight = 820f;
        private const float HotkeysPanelWidth = 520f;
        private const float HotkeysPanelHeight = 430f;
        private const float RootMargin = 20f;
        private const float PanelPadding = 20f;
        private const float PanelSpacing = 14f;
        private const float LabelColumnWidth = 130f;
        private const float ControlHeight = 36f;
        private const float ButtonHeight = 38f;

        private static readonly Color PanelColor = new Color32(3, 13, 24, 246);
        private static readonly Color PanelStrongColor = new Color32(2, 8, 16, 252);
        private static readonly Color OverlayScrimColor = new Color32(0, 0, 0, 120);
        private static readonly Color RowColor = new Color32(13, 31, 48, 230);
        private static readonly Color FieldColor = new Color32(15, 36, 56, 245);
        private static readonly Color ButtonColor = new Color32(35, 53, 73, 245);
        private static readonly Color DisabledButtonColor = new Color32(75, 82, 93, 230);
        private static readonly Color TextColor = new Color32(245, 250, 255, 255);
        private static readonly Color MutedTextColor = new Color32(190, 205, 218, 255);
        private static readonly Color AccentBlue = new Color32(40, 190, 255, 255);
        private static readonly Color AccentOrange = new Color32(255, 133, 31, 255);
        private static readonly Color AccentGreen = new Color32(86, 230, 92, 255);
        private static readonly Color WarningColor = new Color32(255, 180, 48, 255);
        private static readonly Color ErrorColor = new Color32(255, 88, 78, 255);
        private static readonly Color DividerColor = new Color32(130, 170, 200, 120);

        [Header("Runtime References")]
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

        [Header("Runtime Layout")]
        [SerializeField] private bool buildRuntimeOverlayLayout = true;
        [SerializeField] private KeyCode diagnosticsToggleMouseButton = KeyCode.Mouse2;
        [SerializeField] private KeyCode hotkeysKey = KeyCode.F1;
        [SerializeField] private KeyCode resetCenterOffsetKey = KeyCode.R;
        [SerializeField] private KeyCode recordingPlaceholderKey = KeyCode.F9;
        [SerializeField] private KeyCode closeOverlayKey = KeyCode.Escape;
        [SerializeField] private float diagnosticsRefreshInterval = 0.15f;
        [SerializeField] private bool diagnosticsVisibleOnStart;

        private readonly StringBuilder textBuilder = new StringBuilder(1024);

        private RectTransform layoutRoot;
        private GameObject mainControlPanel;
        private GameObject diagnosticsPanel;
        private GameObject diagnosticsHiddenHint;
        private GameObject hotkeysScrim;
        private GameObject hotkeysPanel;

        private Text diagnosticsModeText;
        private Text diagnosticsSourceText;
        private Text diagnosticsMirrorCountText;
        private Text diagnosticsZoomText;
        private Text diagnosticsRotationSpeedText;
        private Text diagnosticsTunnelText;
        private Text diagnosticsRecordingText;
        private Text diagnosticsQualityText;
        private Text diagnosticsFpsText;
        private Text activeModulesText;
        private Text warningsText;
        private Text missingReferencesText;
        private Text errorsText;

        private Button diagnosticsCloseButton;
        private Button hotkeysCloseButton;
        private Text diagnosticsHintLabel;

        private Font uiFont;
        private Texture2D solidTexture;
        private Sprite solidSprite;
        private bool hotkeysVisible;
        private bool suppressCallbacks;
        private float nextDiagnosticsRefreshTime;

        private void Awake()
        {
            if (buildRuntimeOverlayLayout)
            {
                BuildRuntimeOverlayLayout();
            }

            EnsureSourceDropdownOptions();
            ConfigureSliderRanges();
        }

        private void OnEnable()
        {
            AddUiListeners();
        }

        private void Start()
        {
            if (!diagnosticsVisibleOnStart)
            {
                SetDiagnosticsVisible(false);
            }

            RefreshControlsFromState();
            RefreshDiagnosticsText();
            UpdateOutputTexture();
        }

        private void Update()
        {
            HandleMenuInput();
            RefreshControlsFromState();
            UpdateOutputTexture();

            if (Time.unscaledTime >= nextDiagnosticsRefreshTime)
            {
                RefreshDiagnosticsText();
                nextDiagnosticsRefreshTime = Time.unscaledTime + Mathf.Max(0.05f, diagnosticsRefreshInterval);
            }
        }

        private void OnDisable()
        {
            RemoveUiListeners();
        }

        private void OnDestroy()
        {
            DestroyGeneratedAsset(solidSprite);
            DestroyGeneratedAsset(solidTexture);
        }

        private void AddUiListeners()
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

            if (diagnosticsCloseButton != null)
            {
                diagnosticsCloseButton.onClick.AddListener(OnHideDiagnostics);
            }

            if (hotkeysCloseButton != null)
            {
                hotkeysCloseButton.onClick.AddListener(OnHideHotkeys);
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

        private void RemoveUiListeners()
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

            if (diagnosticsCloseButton != null)
            {
                diagnosticsCloseButton.onClick.RemoveListener(OnHideDiagnostics);
            }

            if (hotkeysCloseButton != null)
            {
                hotkeysCloseButton.onClick.RemoveListener(OnHideHotkeys);
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

        private void HandleMenuInput()
        {
            if (UnityEngine.Input.GetKeyDown(hotkeysKey))
            {
                SetHotkeysVisible(!hotkeysVisible);
            }

            if (UnityEngine.Input.GetKeyDown(closeOverlayKey))
            {
                HideOpenOverlay();
            }

            if (UnityEngine.Input.GetKeyDown(resetCenterOffsetKey))
            {
                OnResetCenterOffset();
            }

            if (UnityEngine.Input.GetKeyDown(recordingPlaceholderKey))
            {
                OnRecordingPlaceholderClicked();
            }

            if (UnityEngine.Input.GetKeyDown(diagnosticsToggleMouseButton))
            {
                OnToggleDiagnostics();
            }
        }

        private void HideOpenOverlay()
        {
            if (hotkeysVisible)
            {
                SetHotkeysVisible(false);
                return;
            }

            if (director != null && director.State.Diagnostics.HudVisible)
            {
                SetDiagnosticsVisible(false);
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

            SetDiagnosticsVisible(!director.State.Diagnostics.HudVisible);
        }

        private void OnHideDiagnostics()
        {
            SetDiagnosticsVisible(false);
        }

        private void OnHideHotkeys()
        {
            SetHotkeysVisible(false);
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

        private void SetDiagnosticsVisible(bool visible)
        {
            if (director == null)
            {
                return;
            }

            director.Dispatch(KaleidoscopeCommand.SetDiagnosticsVisible(visible));
        }

        private void SetHotkeysVisible(bool visible)
        {
            hotkeysVisible = visible;
            SetActiveIfDifferent(hotkeysScrim, hotkeysVisible);
            SetActiveIfDifferent(hotkeysPanel, hotkeysVisible);
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

            SetText(mirrorCountValue, state.MirrorSettings.MirrorCount.ToString());

            if (zoomSlider != null)
            {
                zoomSlider.SetValueWithoutNotify(state.MirrorSettings.Zoom);
            }

            SetText(zoomValue, state.MirrorSettings.Zoom.ToString("0.00"));

            if (rotationSpeedSlider != null)
            {
                rotationSpeedSlider.SetValueWithoutNotify(state.MirrorSettings.RotationSpeed);
            }

            SetText(rotationSpeedValue, state.MirrorSettings.RotationSpeed.ToString("0.0"));

            if (tunnelModeToggle != null)
            {
                tunnelModeToggle.SetIsOnWithoutNotify(state.TunnelEnabled);
            }

            SetText(diagnosticsButtonLabel, state.Diagnostics.HudVisible ? "Hide Diagnostics" : "Show Diagnostics");
            SetText(recordingButtonLabel, state.RecordingStatus == KaleidoscopeRecordingStatus.Idle ? "Start Recording" : "Stop Recording");

            suppressCallbacks = false;
            RefreshPanelVisibility(state);
        }

        private void RefreshPanelVisibility(KaleidoscopeState state)
        {
            bool diagnosticsVisible = state.Diagnostics.HudVisible;
            SetActiveIfDifferent(diagnosticsPanel, diagnosticsVisible);
            SetActiveIfDifferent(diagnosticsHiddenHint, !diagnosticsVisible);
            SetText(diagnosticsHintLabel, "Middle Mouse: Diagnostics");
        }

        private void RefreshDiagnosticsText()
        {
            if (director == null)
            {
                return;
            }

            KaleidoscopeState state = director.State;
            DiagnosticsState diagnostics = state.Diagnostics;

            SetText(diagnosticsModeText, state.ActiveVisualMode.ToString());
            SetText(diagnosticsSourceText, state.ActiveSourceMode.ToString());
            SetText(diagnosticsMirrorCountText, state.MirrorSettings.MirrorCount.ToString());
            SetText(diagnosticsZoomText, state.MirrorSettings.Zoom.ToString("0.00"));
            SetText(diagnosticsRotationSpeedText, state.MirrorSettings.RotationSpeed.ToString("0.0"));
            SetText(diagnosticsTunnelText, state.TunnelEnabled ? "Enabled" : "Disabled");
            SetText(diagnosticsRecordingText, GetRecordingLabel(state.RecordingStatus));
            SetText(diagnosticsQualityText, state.QualityLevel.ToString());
            SetText(diagnosticsFpsText, diagnostics.FramesPerSecond.ToString("0.0"));
            SetText(activeModulesText, BuildModuleStatusText(diagnostics));
            SetText(warningsText, BuildStringListText(diagnostics.Warnings));
            SetText(missingReferencesText, BuildStringListText(diagnostics.MissingReferences));
            SetText(errorsText, BuildStringListText(diagnostics.Errors));
        }

        private static string GetRecordingLabel(KaleidoscopeRecordingStatus status)
        {
            return status == KaleidoscopeRecordingStatus.Idle ? "Disabled (Idle)" : "Enabled (" + status + ")";
        }

        private string BuildModuleStatusText(DiagnosticsState diagnostics)
        {
            IReadOnlyList<KaleidoscopeModuleStatus> statuses = diagnostics.ModuleStatuses;

            if (statuses.Count == 0)
            {
                return "None registered.";
            }

            textBuilder.Length = 0;

            for (int index = 0; index < statuses.Count; index++)
            {
                KaleidoscopeModuleStatus status = statuses[index];
                string active = status.IsActive ? "Active" : "Inactive";
                string color = status.IsActive ? "#56E65C" : "#BECDDA";
                textBuilder.Append("<color=");
                textBuilder.Append(color);
                textBuilder.Append(">*</color> ");
                textBuilder.Append(status.ModuleId);
                textBuilder.Append(" - ");
                textBuilder.Append(active);

                if (!string.IsNullOrWhiteSpace(status.Message))
                {
                    textBuilder.Append(" - ");
                    textBuilder.Append(status.Message);
                }

                if (index < statuses.Count - 1)
                {
                    textBuilder.AppendLine();
                }
            }

            return textBuilder.ToString();
        }

        private string BuildStringListText(IReadOnlyList<string> values)
        {
            if (values.Count == 0)
            {
                return "None.";
            }

            textBuilder.Length = 0;

            for (int index = 0; index < values.Count; index++)
            {
                textBuilder.Append(values[index]);

                if (index < values.Count - 1)
                {
                    textBuilder.AppendLine();
                }
            }

            return textBuilder.ToString();
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

        private void BuildRuntimeOverlayLayout()
        {
            if (layoutRoot != null)
            {
                return;
            }

            RectTransform canvasRoot = ResolveCanvasRoot();
            if (canvasRoot == null)
            {
                return;
            }

            EnsureGeneratedSprite();
            HideLegacyControlPanel();
            StretchOutputPreview();

            layoutRoot = CreateRect("Kaleidoscope2_RuntimeUiOverlay", canvasRoot);
            layoutRoot.anchorMin = Vector2.zero;
            layoutRoot.anchorMax = Vector2.one;
            layoutRoot.offsetMin = Vector2.zero;
            layoutRoot.offsetMax = Vector2.zero;
            layoutRoot.SetAsLastSibling();

            BuildMainControlPanel(layoutRoot);
            BuildDiagnosticsOverlay(layoutRoot);
            BuildDiagnosticsHiddenHint(layoutRoot);
            BuildHotkeysHelpOverlay(layoutRoot);
            SetActiveIfDifferent(diagnosticsPanel, false);
            SetActiveIfDifferent(diagnosticsHiddenHint, true);
            SetHotkeysVisible(false);
        }

        private RectTransform ResolveCanvasRoot()
        {
            Canvas canvas = outputImage != null ? outputImage.canvas : null;

            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }

            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("Kaleidoscope2RuntimeCanvas", typeof(RectTransform));
                canvasObject.transform.SetParent(transform, false);
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<GraphicRaycaster>();

                CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            return canvas.transform as RectTransform;
        }

        private void HideLegacyControlPanel()
        {
            List<Transform> controlTransforms = new List<Transform>();
            AddTransform(controlTransforms, sourceModeDropdown);
            AddTransform(controlTransforms, mirrorCountSlider);
            AddTransform(controlTransforms, zoomSlider);
            AddTransform(controlTransforms, rotationSpeedSlider);
            AddTransform(controlTransforms, resetCenterOffsetButton);
            AddTransform(controlTransforms, toggleDiagnosticsButton);
            AddTransform(controlTransforms, tunnelModeToggle);
            AddTransform(controlTransforms, recordingButton);

            Transform commonAncestor = FindCommonAncestor(controlTransforms);
            if (commonAncestor != null && commonAncestor.GetComponent<Canvas>() == null)
            {
                commonAncestor.gameObject.SetActive(false);
                return;
            }

            for (int index = 0; index < controlTransforms.Count; index++)
            {
                if (controlTransforms[index] != null)
                {
                    controlTransforms[index].gameObject.SetActive(false);
                }
            }
        }

        private void StretchOutputPreview()
        {
            if (outputImage == null)
            {
                return;
            }

            RectTransform rectTransform = outputImage.rectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.SetAsFirstSibling();
            outputImage.raycastTarget = false;
        }

        private void BuildMainControlPanel(RectTransform parent)
        {
            RectTransform panel = CreatePanel(parent, "MainControlPanel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(RootMargin, -RootMargin), new Vector2(ControlPanelWidth, 0f), PanelColor);
            mainControlPanel = panel.gameObject;

            VerticalLayoutGroup layoutGroup = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset((int)PanelPadding, (int)PanelPadding, (int)PanelPadding, (int)PanelPadding);
            layoutGroup.spacing = PanelSpacing;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            ContentSizeFitter fitter = panel.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Text title = CreateText(panel, "Title", "KALEIDOSCOPE2 CONTROL", 18, FontStyle.Bold, TextColor, TextAnchor.MiddleLeft);
            AddLayoutElement(title.gameObject, -1f, 28f, 0f);

            BuildSourceModeRow(panel);
            BuildSliderBlock(panel, "Mirror Count", out mirrorCountSlider, out mirrorCountValue);
            BuildSliderBlock(panel, "Zoom", out zoomSlider, out zoomValue);
            BuildSliderBlock(panel, "Rotation Speed", out rotationSpeedSlider, out rotationSpeedValue);

            resetCenterOffsetButton = CreateButton(panel, "ResetCenterOffsetButton", "Reset Center Offset", ButtonColor, TextColor, ButtonHeight, out Text unusedResetLabel);
            BuildControlActionsRow(panel);
            tunnelModeToggle = CreateToggleRow(panel, "Tunnel Mode");
            recordingButton = CreateButton(panel, "RecordingButton", "Start Recording", DisabledButtonColor, MutedTextColor, ButtonHeight, out recordingButtonLabel);
        }

        private void BuildControlActionsRow(RectTransform parent)
        {
            RectTransform row = CreateHorizontalRow(parent, "ControlActionsRow", ButtonHeight, 10f);
            toggleDiagnosticsButton = CreateButton(row, "ToggleDiagnosticsButton", "Show Diagnostics", ButtonColor, TextColor, ButtonHeight, out diagnosticsButtonLabel);
            AddLayoutElement(toggleDiagnosticsButton.gameObject, -1f, ButtonHeight, 1f);
        }

        private void BuildDiagnosticsOverlay(RectTransform parent)
        {
            RectTransform panel = CreatePanel(parent, "DiagnosticsPanel", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-RootMargin, -74f), new Vector2(DiagnosticsPanelWidth, DiagnosticsPanelHeight), PanelStrongColor);
            diagnosticsPanel = panel.gameObject;

            VerticalLayoutGroup layoutGroup = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset((int)PanelPadding, (int)PanelPadding, (int)PanelPadding, (int)PanelPadding);
            layoutGroup.spacing = 10f;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            RectTransform titleRow = CreateHorizontalRow(panel, "DiagnosticsTitleRow", ButtonHeight, 10f);
            Text title = CreateText(titleRow, "Title", "DIAGNOSTICS", 18, FontStyle.Bold, TextColor, TextAnchor.MiddleLeft);
            AddLayoutElement(title.gameObject, -1f, ButtonHeight, 1f);
            diagnosticsCloseButton = CreateButton(titleRow, "CloseDiagnosticsButton", "X", ButtonColor, TextColor, ButtonHeight, out Text unusedLabel);
            AddLayoutElement(diagnosticsCloseButton.gameObject, ButtonHeight, ButtonHeight, 0f);

            CreateDivider(panel);
            CreateDiagnosticsMetricGrid(panel);
            CreateDivider(panel);
            CreateSection(panel, "ACTIVE MODULES", AccentBlue, 118f, out activeModulesText);
            CreateSection(panel, "WARNINGS", WarningColor, 46f, out warningsText);
            CreateSection(panel, "MISSING REFERENCES", WarningColor, 46f, out missingReferencesText);
            CreateSection(panel, "ERRORS", ErrorColor, 46f, out errorsText);
        }

        private void CreateDiagnosticsMetricGrid(RectTransform parent)
        {
            CreateInfoRow(parent, "Mode", out diagnosticsModeText, AccentBlue);
            CreateInfoRow(parent, "Source", out diagnosticsSourceText, AccentBlue);
            CreateInfoRow(parent, "Mirror Count", out diagnosticsMirrorCountText, TextColor);
            CreateInfoRow(parent, "Zoom", out diagnosticsZoomText, TextColor);
            CreateInfoRow(parent, "Rotation Speed", out diagnosticsRotationSpeedText, TextColor);
            CreateInfoRow(parent, "Tunnel", out diagnosticsTunnelText, AccentOrange);
            CreateInfoRow(parent, "Recording", out diagnosticsRecordingText, AccentOrange);
            CreateInfoRow(parent, "Quality", out diagnosticsQualityText, AccentBlue);
            CreateInfoRow(parent, "FPS", out diagnosticsFpsText, AccentGreen);
        }

        private void BuildDiagnosticsHiddenHint(RectTransform parent)
        {
            RectTransform hint = CreatePanel(parent, "HintBadge", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-RootMargin, -RootMargin), new Vector2(300f, 48f), PanelStrongColor);
            diagnosticsHiddenHint = hint.gameObject;

            VerticalLayoutGroup layoutGroup = hint.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset(12, 12, 6, 6);
            layoutGroup.spacing = 0f;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = true;

            diagnosticsHintLabel = CreateText(hint, "DiagnosticsHintLabel", "Middle Mouse: Diagnostics", 14, FontStyle.Bold, TextColor, TextAnchor.MiddleCenter);
            AddLayoutElement(diagnosticsHintLabel.gameObject, -1f, 18f, 0f);
            Text hotkeysHint = CreateText(hint, "HotkeysHintLabel", "F1: Hotkeys", 14, FontStyle.Bold, MutedTextColor, TextAnchor.MiddleCenter);
            AddLayoutElement(hotkeysHint.gameObject, -1f, 18f, 0f);
        }

        private void BuildHotkeysHelpOverlay(RectTransform parent)
        {
            RectTransform scrim = CreatePanel(parent, "HotkeysHelpScrim", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, OverlayScrimColor);
            Stretch(scrim);
            hotkeysScrim = scrim.gameObject;

            RectTransform panel = CreatePanel(scrim, "HotkeysPanel", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-RootMargin, RootMargin), new Vector2(HotkeysPanelWidth, HotkeysPanelHeight), PanelStrongColor);
            hotkeysPanel = panel.gameObject;

            VerticalLayoutGroup layoutGroup = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset((int)PanelPadding, (int)PanelPadding, (int)PanelPadding, (int)PanelPadding);
            layoutGroup.spacing = 8f;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            RectTransform titleRow = CreateHorizontalRow(panel, "HotkeysTitleRow", ButtonHeight, 10f);
            Text title = CreateText(titleRow, "Title", "HOTKEYS", 18, FontStyle.Bold, TextColor, TextAnchor.MiddleLeft);
            AddLayoutElement(title.gameObject, -1f, ButtonHeight, 1f);
            hotkeysCloseButton = CreateButton(titleRow, "CloseHotkeysButton", "X", ButtonColor, TextColor, ButtonHeight, out Text unusedLabel);
            AddLayoutElement(hotkeysCloseButton.gameObject, ButtonHeight, ButtonHeight, 0f);

            CreateDivider(panel);
            CreateHotkeyRow(panel, "Middle Mouse Button", "Show/Hide Diagnostics");
            CreateHotkeyRow(panel, "F1", "Show/Hide Hotkeys");
            CreateHotkeyRow(panel, "R", "Reset Center Offset");
            CreateHotkeyRow(panel, "F9", "Toggle Recording");
            CreateHotkeyRow(panel, "F10", "Screenshot");
            CreateHotkeyRow(panel, "PageUp/PageDown", "Preset Selection");
            CreateHotkeyRow(panel, "Esc", "Hide open overlay");
        }

        private void BuildSourceModeRow(RectTransform parent)
        {
            RectTransform row = CreateSettingRow(parent, "SourceModeRow", ControlHeight);
            Text label = CreateText(row, "Label", "Source Mode", 14, FontStyle.Bold, TextColor, TextAnchor.MiddleLeft);
            AddLayoutElement(label.gameObject, LabelColumnWidth, ControlHeight, 0f);
            sourceModeDropdown = CreateDropdown(row, "SourceModeDropdown");
            AddLayoutElement(sourceModeDropdown.gameObject, -1f, ControlHeight, 1f);
        }

        private void BuildSliderBlock(RectTransform parent, string label, out Slider slider, out Text valueText)
        {
            RectTransform row = CreateSettingRow(parent, label.Replace(" ", string.Empty) + "Row", 52f);
            Text labelText = CreateText(row, "Label", label, 14, FontStyle.Bold, TextColor, TextAnchor.MiddleLeft);
            AddLayoutElement(labelText.gameObject, LabelColumnWidth, 52f, 0f);

            RectTransform controlColumn = CreateRect("Control", row);
            VerticalLayoutGroup controlLayout = controlColumn.gameObject.AddComponent<VerticalLayoutGroup>();
            controlLayout.spacing = 4f;
            controlLayout.childControlWidth = true;
            controlLayout.childControlHeight = false;
            controlLayout.childForceExpandWidth = true;
            controlLayout.childForceExpandHeight = false;
            AddLayoutElement(controlColumn.gameObject, -1f, 52f, 1f);

            valueText = CreateText(controlColumn, "Value", "0", 14, FontStyle.Bold, MutedTextColor, TextAnchor.MiddleRight);
            AddLayoutElement(valueText.gameObject, -1f, 16f, 0f);

            slider = CreateSlider(controlColumn, label.Replace(" ", string.Empty) + "Slider");
        }

        private void CreateInfoRow(RectTransform parent, string label, out Text valueText, Color valueColor)
        {
            RectTransform row = CreateSettingRow(parent, label.Replace(" ", string.Empty) + "Row", 26f);
            Image background = row.gameObject.AddComponent<Image>();
            background.sprite = solidSprite;
            background.color = RowColor;

            Text labelText = CreateText(row, "Label", label + ":", 14, FontStyle.Bold, TextColor, TextAnchor.MiddleLeft);
            AddLayoutElement(labelText.gameObject, LabelColumnWidth, 26f, 0f);
            valueText = CreateText(row, "Value", string.Empty, 14, FontStyle.Bold, valueColor, TextAnchor.MiddleLeft);
            AddLayoutElement(valueText.gameObject, -1f, 26f, 1f);
        }

        private void CreateSection(RectTransform parent, string title, Color titleColor, float bodyHeight, out Text bodyText)
        {
            Text sectionTitle = CreateText(parent, title + "Title", title, 14, FontStyle.Bold, titleColor, TextAnchor.MiddleLeft);
            AddLayoutElement(sectionTitle.gameObject, -1f, 20f, 0f);

            bodyText = CreateText(parent, title + "Body", "None.", 14, FontStyle.Normal, TextColor, TextAnchor.UpperLeft);
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            AddLayoutElement(bodyText.gameObject, -1f, bodyHeight, 0f);
        }

        private void CreateHotkeyRow(RectTransform parent, string key, string action)
        {
            RectTransform row = CreateSettingRow(parent, "HotkeyRow", 32f);
            Image background = row.gameObject.AddComponent<Image>();
            background.sprite = solidSprite;
            background.color = RowColor;

            Text keyText = CreateText(row, "Key", key, 14, FontStyle.Bold, AccentBlue, TextAnchor.MiddleLeft);
            AddLayoutElement(keyText.gameObject, 150f, 32f, 0f);
            Text actionText = CreateText(row, "Action", action, 14, FontStyle.Bold, TextColor, TextAnchor.MiddleLeft);
            AddLayoutElement(actionText.gameObject, -1f, 32f, 1f);
        }

        private Toggle CreateToggleRow(RectTransform parent, string label)
        {
            RectTransform row = CreateSettingRow(parent, "ToggleRow", ControlHeight);
            Toggle toggle = row.gameObject.AddComponent<Toggle>();

            Text labelText = CreateText(row, "Label", label, 14, FontStyle.Bold, TextColor, TextAnchor.MiddleLeft);
            AddLayoutElement(labelText.gameObject, LabelColumnWidth, ControlHeight, 0f);

            RectTransform control = CreateHorizontalRow(row, "Control", ControlHeight, 8f);
            AddLayoutElement(control.gameObject, -1f, ControlHeight, 1f);

            RectTransform box = CreateRect("Box", control);
            Image boxImage = box.gameObject.AddComponent<Image>();
            boxImage.sprite = solidSprite;
            boxImage.color = FieldColor;
            AddLayoutElement(box.gameObject, 28f, 28f, 0f);

            RectTransform checkmark = CreateRect("Checkmark", box);
            Image checkImage = checkmark.gameObject.AddComponent<Image>();
            checkImage.sprite = solidSprite;
            checkImage.color = AccentBlue;
            checkmark.anchorMin = new Vector2(0.2f, 0.2f);
            checkmark.anchorMax = new Vector2(0.8f, 0.8f);
            checkmark.offsetMin = Vector2.zero;
            checkmark.offsetMax = Vector2.zero;

            Text valueText = CreateText(control, "Value", "Toggle", 14, FontStyle.Bold, MutedTextColor, TextAnchor.MiddleLeft);
            AddLayoutElement(valueText.gameObject, -1f, ControlHeight, 1f);

            toggle.targetGraphic = boxImage;
            toggle.graphic = checkImage;
            return toggle;
        }

        private Slider CreateSlider(RectTransform parent, string name)
        {
            RectTransform sliderTransform = CreateRect(name, parent);
            AddLayoutElement(sliderTransform.gameObject, -1f, 30f, 0f);

            Slider slider = sliderTransform.gameObject.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;

            RectTransform background = CreateRect("Background", sliderTransform);
            background.anchorMin = new Vector2(0f, 0.5f);
            background.anchorMax = new Vector2(1f, 0.5f);
            background.pivot = new Vector2(0.5f, 0.5f);
            background.sizeDelta = new Vector2(0f, 6f);
            Image backgroundImage = background.gameObject.AddComponent<Image>();
            backgroundImage.sprite = solidSprite;
            backgroundImage.color = new Color32(210, 224, 234, 230);

            RectTransform fillArea = CreateRect("Fill Area", sliderTransform);
            fillArea.anchorMin = new Vector2(0f, 0.5f);
            fillArea.anchorMax = new Vector2(1f, 0.5f);
            fillArea.pivot = new Vector2(0.5f, 0.5f);
            fillArea.offsetMin = new Vector2(0f, -4f);
            fillArea.offsetMax = new Vector2(0f, 4f);

            RectTransform fill = CreateRect("Fill", fillArea);
            Stretch(fill);
            Image fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.sprite = solidSprite;
            fillImage.color = AccentBlue;

            RectTransform handleArea = CreateRect("Handle Slide Area", sliderTransform);
            Stretch(handleArea);

            RectTransform handle = CreateRect("Handle", handleArea);
            handle.sizeDelta = new Vector2(22f, 22f);
            Image handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.sprite = solidSprite;
            handleImage.color = new Color32(102, 190, 255, 255);

            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            return slider;
        }

        private Dropdown CreateDropdown(RectTransform parent, string name)
        {
            RectTransform dropdownTransform = CreateRect(name, parent);
            Image background = dropdownTransform.gameObject.AddComponent<Image>();
            background.sprite = solidSprite;
            background.color = FieldColor;

            Dropdown dropdown = dropdownTransform.gameObject.AddComponent<Dropdown>();
            dropdown.targetGraphic = background;

            Text caption = CreateText(dropdownTransform, "Label", string.Empty, 14, FontStyle.Bold, AccentBlue, TextAnchor.MiddleLeft);
            caption.rectTransform.anchorMin = Vector2.zero;
            caption.rectTransform.anchorMax = Vector2.one;
            caption.rectTransform.offsetMin = new Vector2(12f, 0f);
            caption.rectTransform.offsetMax = new Vector2(-34f, 0f);
            dropdown.captionText = caption;

            Text arrow = CreateText(dropdownTransform, "Arrow", "v", 14, FontStyle.Bold, MutedTextColor, TextAnchor.MiddleCenter);
            arrow.rectTransform.anchorMin = new Vector2(1f, 0f);
            arrow.rectTransform.anchorMax = new Vector2(1f, 1f);
            arrow.rectTransform.pivot = new Vector2(1f, 0.5f);
            arrow.rectTransform.sizeDelta = new Vector2(30f, 0f);
            arrow.rectTransform.anchoredPosition = new Vector2(-6f, 0f);

            RectTransform template = CreatePanel(dropdownTransform, "Template", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(0f, 180f), PanelStrongColor);
            template.gameObject.SetActive(false);
            ScrollRect scrollRect = template.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            RectTransform viewport = CreatePanel(template, "Viewport", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color32(0, 0, 0, 0));
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform content = CreateRect("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            VerticalLayoutGroup contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            RectTransform item = CreateRect("Item", content);
            AddLayoutElement(item.gameObject, -1f, 32f, 0f);
            Image itemImage = item.gameObject.AddComponent<Image>();
            itemImage.sprite = solidSprite;
            itemImage.color = RowColor;
            Toggle itemToggle = item.gameObject.AddComponent<Toggle>();
            itemToggle.targetGraphic = itemImage;

            Text itemLabel = CreateText(item, "Item Label", string.Empty, 14, FontStyle.Bold, TextColor, TextAnchor.MiddleLeft);
            itemLabel.rectTransform.anchorMin = Vector2.zero;
            itemLabel.rectTransform.anchorMax = Vector2.one;
            itemLabel.rectTransform.offsetMin = new Vector2(12f, 0f);
            itemLabel.rectTransform.offsetMax = new Vector2(-8f, 0f);

            scrollRect.viewport = viewport;
            scrollRect.content = content;
            dropdown.template = template;
            dropdown.itemText = itemLabel;
            return dropdown;
        }

        private Button CreateButton(RectTransform parent, string name, string label, Color backgroundColor, Color labelColor, float height, out Text labelText)
        {
            RectTransform buttonTransform = CreateRect(name, parent);
            Image image = buttonTransform.gameObject.AddComponent<Image>();
            image.sprite = solidSprite;
            image.color = backgroundColor;

            Button button = buttonTransform.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = backgroundColor;
            colors.highlightedColor = Color.Lerp(backgroundColor, AccentBlue, 0.3f);
            colors.pressedColor = Color.Lerp(backgroundColor, Color.black, 0.25f);
            colors.disabledColor = DisabledButtonColor;
            button.colors = colors;

            labelText = CreateText(buttonTransform, "Label", label, 14, FontStyle.Bold, labelColor, TextAnchor.MiddleCenter);
            Stretch(labelText.rectTransform);
            AddLayoutElement(buttonTransform.gameObject, -1f, height, 0f);
            return button;
        }

        private RectTransform CreateHorizontalRow(RectTransform parent, string name, float height, float spacing)
        {
            RectTransform row = CreateRect(name, parent);
            HorizontalLayoutGroup layoutGroup = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = spacing;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = true;
            AddLayoutElement(row.gameObject, -1f, height, 0f);
            return row;
        }

        private RectTransform CreateSettingRow(RectTransform parent, string name, float height)
        {
            RectTransform row = CreateHorizontalRow(parent, name, height, 10f);
            HorizontalLayoutGroup layoutGroup = row.GetComponent<HorizontalLayoutGroup>();
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            return row;
        }

        private RectTransform CreatePanel(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            RectTransform panel = CreateRect(name, parent);
            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;
            panel.pivot = pivot;
            panel.anchoredPosition = anchoredPosition;
            panel.sizeDelta = size;

            Image image = panel.gameObject.AddComponent<Image>();
            image.sprite = solidSprite;
            image.color = color;
            return panel;
        }

        private Text CreateText(RectTransform parent, string name, string value, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment)
        {
            RectTransform textTransform = CreateRect(name, parent);
            Text textComponent = textTransform.gameObject.AddComponent<Text>();
            textComponent.font = GetUiFont();
            textComponent.text = value;
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = fontStyle;
            textComponent.color = color;
            textComponent.alignment = alignment;
            textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
            textComponent.verticalOverflow = VerticalWrapMode.Truncate;
            textComponent.raycastTarget = false;
            textComponent.supportRichText = true;

            Shadow shadow = textTransform.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color32(0, 0, 0, 180);
            shadow.effectDistance = new Vector2(1f, -1f);
            return textComponent;
        }

        private RectTransform CreateRect(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return (RectTransform)gameObject.transform;
        }

        private void AddLayoutElement(GameObject target, float preferredWidth, float preferredHeight, float flexibleWidth)
        {
            LayoutElement layoutElement = target.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = target.AddComponent<LayoutElement>();
            }

            if (preferredWidth >= 0f)
            {
                layoutElement.preferredWidth = preferredWidth;
            }

            if (preferredHeight >= 0f)
            {
                layoutElement.preferredHeight = preferredHeight;
                layoutElement.minHeight = preferredHeight;
            }

            layoutElement.flexibleWidth = flexibleWidth;
        }

        private void CreateDivider(RectTransform parent)
        {
            RectTransform divider = CreateRect("Divider", parent);
            Image image = divider.gameObject.AddComponent<Image>();
            image.sprite = solidSprite;
            image.color = DividerColor;
            AddLayoutElement(divider.gameObject, -1f, 2f, 0f);
        }

        private void EnsureGeneratedSprite()
        {
            if (solidSprite != null)
            {
                return;
            }

            solidTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            solidTexture.name = "Kaleidoscope2_UI_Solid";
            solidTexture.hideFlags = HideFlags.HideAndDontSave;
            solidTexture.SetPixel(0, 0, Color.white);
            solidTexture.Apply(false, true);

            solidSprite = Sprite.Create(solidTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            solidSprite.name = "Kaleidoscope2_UI_SolidSprite";
            solidSprite.hideFlags = HideFlags.HideAndDontSave;
        }

        private Font GetUiFont()
        {
            if (uiFont == null)
            {
                uiFont = LoadBuiltinFont("LegacyRuntime.ttf");

                if (uiFont == null)
                {
                    uiFont = LoadBuiltinFont("Arial.ttf");
                }
            }

            return uiFont;
        }

        private static Font LoadBuiltinFont(string fontName)
        {
            try
            {
                return Resources.GetBuiltinResource<Font>(fontName);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void SetText(Text target, string value)
        {
            if (target != null && target.text != value)
            {
                target.text = value;
            }
        }

        private static void SetActiveIfDifferent(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }

        private static void AddTransform<T>(List<Transform> transforms, T component) where T : Component
        {
            if (component != null)
            {
                transforms.Add(component.transform);
            }
        }

        private static Transform FindCommonAncestor(IReadOnlyList<Transform> transforms)
        {
            if (transforms == null || transforms.Count == 0)
            {
                return null;
            }

            Transform candidate = transforms[0];
            while (candidate != null)
            {
                bool containsAll = true;

                for (int index = 1; index < transforms.Count; index++)
                {
                    if (transforms[index] == null || !transforms[index].IsChildOf(candidate))
                    {
                        containsAll = false;
                        break;
                    }
                }

                if (containsAll)
                {
                    return candidate;
                }

                candidate = candidate.parent;
            }

            return null;
        }

        private static void DestroyGeneratedAsset(UnityEngine.Object asset)
        {
            if (asset == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(asset);
            }
            else
            {
                DestroyImmediate(asset);
            }
        }
    }
}
