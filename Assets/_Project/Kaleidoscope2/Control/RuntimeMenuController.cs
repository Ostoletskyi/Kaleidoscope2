using System;
using System.Collections.Generic;
using System.IO;
using Kaleidoscope2.Core;
using Kaleidoscope2.FileBrowser;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Control
{
    // Runtime control surface. By default it is hidden and only shown on middle mouse click.
    [DisallowMultipleComponent]
    public sealed class RuntimeMenuController : KaleidoscopeModuleBase
    {
        // Legacy serialized fields from earlier iterations. Kept to avoid breaking scene references.
        // They are unused by the current runtime menu implementation.
#pragma warning disable 649
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
#pragma warning restore 649

        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        private static readonly Color PanelColor = new Color32(8, 16, 24, 240);
        private static readonly Color PanelStrongColor = new Color32(8, 16, 24, 252);
        private static readonly Color ScrimColor = new Color32(0, 0, 0, 140);
        private static readonly Color ButtonColor = new Color32(38, 56, 78, 245);
        private static readonly Color MutedButtonColor = new Color32(60, 66, 74, 245);
        private static readonly Color TextColor = new Color32(245, 250, 255, 255);
        private static readonly Color MutedTextColor = new Color32(190, 205, 218, 255);
        private static readonly Color Accent = new Color32(40, 190, 255, 255);

        private static readonly string[] ImageExtensions = RuntimeImageFolderScanner.SupportedImageExtensions;
        private static readonly string[] AudioExtensions = { ".mp3", ".wav", ".ogg", ".aiff", ".aif" };

        [Header("Runtime References")]
        [SerializeField] private KaleidoscopeDirector director;
        [SerializeField] private RawImage outputImage;

        private RectTransform canvasRoot;
        private CanvasScaler canvasScaler;

        private GameObject menuRoot;
        private GameObject helpRoot;
        private GameObject browserRoot;

        private Text modeValueText;
        private Text imagePathText;
        private Text audioPathText;
        private Text guidesValueText;
        private Text zoomValueText;
        private Text rotationSpeedValueText;
        private Text hoseOpeningValueText;
        private Text hoseWallCurvatureValueText;
        private Text hoseChromaticAberrationValueText;
        private Text sevenDStrategyValueText;

        private BrowserMode activeBrowserMode;
        private string browserCurrentPath;
        private RectTransform browserListContent;
        private RectTransform browserDrivesContent;
        private Text browserPathText;
        private Text browserStatusText;
        private Text browserDiagnosticsText;
        private Button browserSelectFolderButton;
        private InputField browserPathInput;
        private RuntimeFileBrowserController fileBrowserController;
        private readonly RuntimeFileBrowserView fileBrowserView = new RuntimeFileBrowserView();
        private readonly List<GameObject> browserDriveButtonPool = new List<GameObject>(16);
        private bool browserDiagnosticsVisible;

        private Font uiFont;
        private Texture2D solidTexture;
        private Sprite solidSprite;

        private readonly List<GameObject> browserRowPool = new List<GameObject>(128);
        private float nextStateSyncTime;

        public override string ModuleId
        {
            get { return "Control"; }
        }

        protected override void OnInitialized()
        {
            // No-op: we build UI from the scene Canvas at runtime.
        }

        private void Awake()
        {
            EnsureSprites();
            EnsureFileBrowserController();
            EnsureCanvasRoot();
            EnsureOutputSurface();
            HideLegacyUi();
            BuildMenuUi();
            SyncUiFromState();

            SetMenuVisible(false, updateState: false);
        }

        private void Start()
        {
            // Intentionally empty. This module is expected to be registered through Bootstrap.
        }

        private void Update()
        {
            if (director == null || outputImage == null)
            {
                return;
            }

            UpdateOutputTexture();

            if (menuRoot != null && menuRoot.activeSelf && Time.unscaledTime >= nextStateSyncTime)
            {
                SyncUiFromState();
                nextStateSyncTime = Time.unscaledTime + 0.1f;
            }
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            if (command == null)
            {
                return false;
            }

            return command.Type == KaleidoscopeCommandType.ToggleControlMenu
                || command.Type == KaleidoscopeCommandType.SetControlMenuVisible;
        }

        public override void HandleCommand(KaleidoscopeCommand command)
        {
            if (command == null)
            {
                return;
            }

            switch (command.Type)
            {
                case KaleidoscopeCommandType.ToggleControlMenu:
                    SetMenuVisible(director != null ? director.State.ControlMenuVisible : (menuRoot == null || !menuRoot.activeSelf), updateState: false);
                    break;

                case KaleidoscopeCommandType.SetControlMenuVisible:
                    SetMenuVisible(director != null ? director.State.ControlMenuVisible : command.BoolValue, updateState: false);
                    break;
            }
        }

        private void OnDestroy()
        {
            DestroyGeneratedAsset(solidSprite);
            DestroyGeneratedAsset(solidTexture);
        }

        private void UpdateOutputTexture()
        {
            Texture output = director.FinalOutputTexture;
            if (outputImage.texture != output)
            {
                outputImage.texture = output;
            }
        }

        private void SetMenuVisible(bool visible, bool updateState)
        {
            if (menuRoot != null && menuRoot.activeSelf != visible)
            {
                menuRoot.SetActive(visible);
            }

            if (!visible)
            {
                SetActiveIfDifferent(helpRoot, false);
                SetActiveIfDifferent(browserRoot, false);
            }

            if (updateState && director != null)
            {
                director.Dispatch(KaleidoscopeCommand.SetControlMenuVisible(visible));
            }
        }

        private void SyncUiFromState()
        {
            if (director == null)
            {
                return;
            }

            KaleidoscopeState state = director.State;
            MirrorSettings mirror = state != null ? state.MirrorSettings : null;
            TunnelSettings tunnel = state != null ? state.TunnelSettings : null;

            if (modeValueText != null)
            {
                modeValueText.text = GetModeLabel(state.ActiveVisualMode);
            }

            if (guidesValueText != null)
            {
                guidesValueText.text = mirror != null && mirror.GuidesVisible ? "Вкл" : "Выкл";
            }

            if (zoomValueText != null)
            {
                zoomValueText.text = mirror != null ? mirror.Zoom.ToString("0.00") : "0.00";
            }

            if (rotationSpeedValueText != null)
            {
                rotationSpeedValueText.text = mirror != null ? mirror.RotationSpeed.ToString("0") : "0";
            }

            if (hoseOpeningValueText != null)
            {
                hoseOpeningValueText.text = tunnel != null ? tunnel.HoseOpeningUnits.ToString("0") : "0";
            }

            if (hoseWallCurvatureValueText != null)
            {
                hoseWallCurvatureValueText.text = tunnel != null ? tunnel.HoseWallCurvatureUnits.ToString("0") : "0";
            }

            if (hoseChromaticAberrationValueText != null)
            {
                hoseChromaticAberrationValueText.text = tunnel != null && tunnel.HoseChromaticAberrationEnabled ? "Вкл" : "Выкл";
            }

            if (sevenDStrategyValueText != null)
            {
                SevenDSettings sevenD = state != null ? state.SevenDSettings : null;
                sevenDStrategyValueText.text = sevenD != null ? GetSevenDStrategyLabel(sevenD.Strategy) : "Нет";
            }

            if (imagePathText != null)
            {
                string value = !string.IsNullOrWhiteSpace(state.ImageFilePath) ? state.ImageFilePath : state.ImageFolderPath;
                imagePathText.text = string.IsNullOrWhiteSpace(value) ? "Не выбрано" : ShortPath(value);
            }

            if (audioPathText != null)
            {
                string value = !string.IsNullOrWhiteSpace(state.AudioFilePath) ? state.AudioFilePath : state.AudioFolderPath;
                audioPathText.text = string.IsNullOrWhiteSpace(value) ? "Не выбрано" : ShortPath(value);
            }
        }

        private void EnsureCanvasRoot()
        {
            if (outputImage == null)
            {
                return;
            }

            Canvas canvas = outputImage.canvas;
            if (canvas == null)
            {
                return;
            }

            canvasRoot = canvas.transform as RectTransform;
            canvasScaler = canvas.GetComponent<CanvasScaler>();

            // Some earlier scene iterations left the Canvas root scaled to zero.
            if (canvasRoot != null && canvasRoot.localScale == Vector3.zero)
            {
                canvasRoot.localScale = Vector3.one;
            }

            if (canvasScaler != null)
            {
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
                canvasScaler.matchWidthOrHeight = 0.5f;
            }
        }

        private void EnsureOutputSurface()
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
            rectTransform.localScale = Vector3.one;
            outputImage.raycastTarget = false;
        }

        private void HideLegacyUi()
        {
            if (canvasRoot == null || outputImage == null)
            {
                return;
            }

            for (int i = 0; i < canvasRoot.childCount; i++)
            {
                Transform child = canvasRoot.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (child == outputImage.transform)
                {
                    continue;
                }

                child.gameObject.SetActive(false);
            }
        }

        private void BuildMenuUi()
        {
            if (canvasRoot == null)
            {
                return;
            }

            menuRoot = new GameObject("RuntimeMenuRoot", typeof(RectTransform));
            RectTransform rootRect = (RectTransform)menuRoot.transform;
            rootRect.SetParent(canvasRoot, false);
            Stretch(rootRect);

            Image scrim = menuRoot.AddComponent<Image>();
            scrim.sprite = solidSprite;
            scrim.color = ScrimColor;
            scrim.raycastTarget = true;

            RectTransform panel = CreatePanel(rootRect, "MenuPanel", new Vector2(620f, 880f));
            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = 10f;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            CreateTitle(panel, "KALEIDOSCOPE 2", "Меню");

            CreateRow(panel, "Изображение:", out imagePathText);
            CreateButton(panel, "PickImageFile", "Выбрать файл изображения", ButtonColor, () =>
            {
                OpenBrowser(BrowserMode.ImageFile);
            });
            CreateButton(panel, "PickImageFolder", "Выбрать папку с изображениями", ButtonColor, () =>
            {
                OpenBrowser(BrowserMode.ImageFolder);
            });

            CreateRow(panel, "Музыка:", out audioPathText);
            CreateButton(panel, "PickAudioFile", "Выбрать файл музыки", ButtonColor, () =>
            {
                OpenBrowser(BrowserMode.AudioFile);
            });
            CreateButton(panel, "PickAudioFolder", "Выбрать папку с музыкой", ButtonColor, () =>
            {
                OpenBrowser(BrowserMode.AudioFolder);
            });

            RectTransform modeRow = CreateRow(panel, "Режим:", out modeValueText);
            CreateButton(modeRow, "ToggleMode", "2D / 3D / 4D / 5D / 6D / 7D", ButtonColor, ToggleMode);

            CreateRow(panel, "4D Г:", out hoseOpeningValueText);
            CreateRow(panel, "4D Щ:", out hoseWallCurvatureValueText);
            RectTransform chromaticAberrationRow = CreateRow(panel, "4D CA:", out hoseChromaticAberrationValueText);
            CreateButton(chromaticAberrationRow, "Toggle4DCA", "Вкл/Выкл", ButtonColor, ToggleHoseChromaticAberration);
            RectTransform sevenDRow = CreateRow(panel, "7D:", out sevenDStrategyValueText);
            CreateButton(sevenDRow, "Prev7D", "-", MutedButtonColor, () => CycleSevenDStrategy(-1));
            CreateButton(sevenDRow, "Next7D", "+", ButtonColor, () => CycleSevenDStrategy(1));

            RectTransform guidesRow = CreateRow(panel, "Линии:", out guidesValueText);
            CreateButton(guidesRow, "ToggleGuides", "Вкл/Выкл", ButtonColor, ToggleGuides);

            CreateRow(panel, "Приближение:", out zoomValueText);
            CreateRow(panel, "Скорость вращения:", out rotationSpeedValueText);
            CreateButton(panel, "ResetMotion", "Сбросить движение", MutedButtonColor, ResetMotion);

            CreateButton(panel, "Help", "Помощь", ButtonColor, OpenHelp);
            CreateButton(panel, "Close", "Закрыть панель", MutedButtonColor, () => SetMenuVisible(false, updateState: true));

            BuildHelpUi();
            BuildBrowserUi();
        }

        private void BuildHelpUi()
        {
            helpRoot = new GameObject("HelpRoot", typeof(RectTransform));
            RectTransform rootRect = (RectTransform)helpRoot.transform;
            rootRect.SetParent(menuRoot.transform, false);
            Stretch(rootRect);

            Image scrim = helpRoot.AddComponent<Image>();
            scrim.sprite = solidSprite;
            scrim.color = ScrimColor;
            scrim.raycastTarget = true;

            RectTransform panel = CreatePanel(rootRect, "HelpPanel", new Vector2(720f, 720f));

            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = 10f;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            CreateTitle(panel, "Помощь", string.Empty);

            Text body = CreateText(panel, "HelpBody",
                "Управление:\n" +
                "Колесо мыши (клик) — открыть/закрыть меню\n" +
                "Esc — закрыть меню\n\n" +
                "0 / Num0 — мягкие линии стыка зеркал (вкл/выкл)\n" +
                "Num5 (доп. клавиатура) — сброс движения и 4D-профиля\n" +
                "NumEnter (боковой Enter) — переключение 2D / 3D / 4D / 5D / 6D / 7D\n" +
                "1..9 — 6/12/24/48/96/192/384/768/1536 зеркал\n" +
                "Z/X/C (рус. Я/Ч/С) — предыдущий / стоп-плей / следующий трек\n" +
                "Q/E (рус. Й/У) — полёт к центру и обратно в 2D/3D/4D/6D/7D\n" +
                "W/A/S/D (рус. Ц/Ф/Ы/В) — сдвиг изображения в активном режиме; в 3D дополнительно изгиб туннеля\n" +
                "I/K/J/L — изгиб 4D-шланга (на русской раскладке: Ш/Л/О/Д)\n\n" +
                "U/Y (рус. Г/Н) — ширина воронки 4D: -500..+500\n" +
                "O/P (рус. Щ/З) — кривизна стенок 4D: -500..+500\n" +
                "[ (рус. Х) — chromatic aberration в 4D\n" +
                "+/- — скорость полёта 5D к центру; в 7D — переключение стратегии\n" +
                "Space — встряхнуть активный визуальный режим и сменить изображение\n\n" +
                "Режимы:\n" +
                "2D — классический калейдоскоп (сегменты от центра).\n" +
                "3D — старое туннельное искажение с копиями калейдоскопа по бокам.\n" +
                "4D — воронка с независимой шириной Г и профилем стенок Щ.\n" +
                "5D — вечный полёт в центр по ленте Мёбиуса.\n\n" +
                "6D — псевдообъёмная оптика: depth warp, focus, haze и lens distortion.\n\n" +
                "7D — природные стратегии: Романеско, снежинки, структурный цвет, мурмурация, подсолнух.\n\n" +
                "Источники:\n" +
                "Изображение: JPG/PNG/BMP/TGA.\n" +
                "Музыка: MP3/WAV/OGG/AIFF (в зависимости от поддержки Unity на вашей платформе).",
                14,
                FontStyle.Normal,
                MutedTextColor,
                TextAnchor.UpperLeft);
            body.gameObject.AddComponent<LayoutElement>().preferredHeight = 560f;

            CreateButton(panel, "HelpClose", "Назад", ButtonColor, () => SetActiveIfDifferent(helpRoot, false));
            helpRoot.SetActive(false);
        }

        private void BuildBrowserUi()
        {
            browserRoot = new GameObject("BrowserRoot", typeof(RectTransform));
            RectTransform rootRect = (RectTransform)browserRoot.transform;
            rootRect.SetParent(menuRoot.transform, false);
            Stretch(rootRect);

            Image scrim = browserRoot.AddComponent<Image>();
            scrim.sprite = solidSprite;
            scrim.color = ScrimColor;
            scrim.raycastTarget = true;

            RectTransform panel = CreatePanel(rootRect, "BrowserPanel", new Vector2(980f, 780f));
            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateTitle(panel, "Выбор файлов", string.Empty);

            browserPathText = CreateText(panel, "BrowserPath", "", 14, FontStyle.Bold, TextColor, TextAnchor.MiddleLeft);
            browserPathText.gameObject.AddComponent<LayoutElement>().preferredHeight = 28f;

            browserStatusText = CreateText(panel, "BrowserStatus", "", 13, FontStyle.Normal, MutedTextColor, TextAnchor.MiddleLeft);
            browserStatusText.gameObject.AddComponent<LayoutElement>().preferredHeight = 26f;

            // Path input (direct jump) and drive chooser.
            RectTransform jumpRow = CreateRect("JumpRow", panel);
            HorizontalLayoutGroup jumpLayout = jumpRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            jumpLayout.spacing = 10f;
            jumpLayout.childControlWidth = true;
            jumpLayout.childControlHeight = true;
            jumpLayout.childForceExpandWidth = false;
            jumpLayout.childForceExpandHeight = false;
            jumpRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 38f;

            browserPathInput = CreateInputField(jumpRow, "PathInput", "Путь (например D:\\\\Music)", 14);
            SetLayoutPreferred(browserPathInput.gameObject, 720f, 38f);

            CreateButton(jumpRow, "Go", "Перейти", ButtonColor, () =>
            {
                if (browserPathInput == null)
                {
                    return;
                }

                string value = browserPathInput.text;
                TryNavigateToPath(value);
            });

            RectTransform drivesRow = CreateRect("DrivesRow", panel);
            browserDrivesContent = drivesRow;
            HorizontalLayoutGroup drivesLayout = drivesRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            drivesLayout.spacing = 8f;
            drivesLayout.childControlWidth = true;
            drivesLayout.childControlHeight = true;
            drivesLayout.childForceExpandWidth = false;
            drivesLayout.childForceExpandHeight = false;
            drivesRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 38f;

            RectTransform actions = CreateRect("Actions", panel);
            HorizontalLayoutGroup actionsLayout = actions.gameObject.AddComponent<HorizontalLayoutGroup>();
            actionsLayout.spacing = 10f;
            actionsLayout.childControlWidth = true;
            actionsLayout.childControlHeight = true;
            actionsLayout.childForceExpandWidth = false;
            actionsLayout.childForceExpandHeight = false;

            CreateButton(actions, "Up", "Вверх", ButtonColor, NavigateUp);
            browserSelectFolderButton = CreateButton(actions, "SelectFolder", "Выбрать эту папку", ButtonColor, SelectCurrentFolder);
            CreateButton(actions, "BrowserDebug", "Debug", MutedButtonColor, ToggleBrowserDiagnostics);
            CreateButton(actions, "CloseBrowser", "Закрыть", MutedButtonColor, () =>
            {
                if (fileBrowserController != null)
                {
                    fileBrowserController.Close();
                }
            });

            browserDiagnosticsText = CreateText(panel, "BrowserDiagnostics", "", 12, FontStyle.Normal, MutedTextColor, TextAnchor.UpperLeft);
            browserDiagnosticsText.gameObject.AddComponent<LayoutElement>().preferredHeight = 96f;
            browserDiagnosticsText.gameObject.SetActive(false);

            // List
            RectTransform listRoot = CreateRect("ListRoot", panel);
            Image listRootImage = listRoot.gameObject.AddComponent<Image>();
            listRootImage.sprite = solidSprite;
            listRootImage.color = new Color32(12, 20, 30, 248);
            LayoutElement listRootLayout = listRoot.gameObject.AddComponent<LayoutElement>();
            listRootLayout.minHeight = 360f;
            listRootLayout.preferredHeight = 500f;
            listRootLayout.flexibleWidth = 1f;

            ScrollRect scroll = listRoot.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;
            scroll.inertia = false;

            RectTransform viewport = CreateRect("Viewport", listRoot);
            Image viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.sprite = solidSprite;
            viewportImage.color = new Color32(24, 36, 52, 120);
            Mask mask = viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            Stretch(viewport);
            scroll.viewport = viewport;

            browserListContent = CreateRect("Content", viewport);
            browserListContent.anchorMin = new Vector2(0f, 1f);
            browserListContent.anchorMax = new Vector2(1f, 1f);
            browserListContent.pivot = new Vector2(0.5f, 1f);
            browserListContent.anchoredPosition = Vector2.zero;
            browserListContent.sizeDelta = new Vector2(0f, 0f);

            Image contentImage = browserListContent.gameObject.AddComponent<Image>();
            contentImage.sprite = solidSprite;
            contentImage.color = new Color32(40, 52, 66, 90);

            VerticalLayoutGroup contentLayout = browserListContent.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(10, 10, 10, 10);
            contentLayout.spacing = 6f;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childAlignment = TextAnchor.UpperLeft;

            ContentSizeFitter fitter = browserListContent.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = browserListContent;

            browserRoot.SetActive(false);
        }

        private void RefreshDriveButtons(RuntimeFileBrowserState browserState)
        {
            if (browserDrivesContent == null || browserState == null)
            {
                return;
            }

            IReadOnlyList<RuntimeFileBrowserItem> drives = browserState.Drives;
            int buttonIndex = 0;

            for (int index = 0; index < drives.Count; index++)
            {
                RuntimeFileBrowserItem drive = drives[index];
                if (drive == null)
                {
                    continue;
                }

                EnsureDriveButton(buttonIndex++, drive);
            }

            for (int index = buttonIndex; index < browserDriveButtonPool.Count; index++)
            {
                if (browserDriveButtonPool[index] != null)
                {
                    browserDriveButtonPool[index].SetActive(false);
                }
            }
        }

        private void EnsureDriveButton(int index, RuntimeFileBrowserItem drive)
        {
            while (browserDriveButtonPool.Count <= index)
            {
                browserDriveButtonPool.Add(null);
            }

            GameObject buttonObject = browserDriveButtonPool[index];
            if (buttonObject == null)
            {
                buttonObject = CreateButton(browserDrivesContent, "Drive_" + index, drive.DisplayName, MutedButtonColor, null).gameObject;
                browserDriveButtonPool[index] = buttonObject;
                SetLayoutPreferred(buttonObject, 72f, 40f);
            }

            buttonObject.SetActive(true);
            Text text = buttonObject.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = drive.DisplayName;
            }

            Button button = buttonObject.GetComponent<Button>();
            if (button != null)
            {
                RuntimeFileBrowserItem capturedDrive = drive;
                button.onClick.RemoveAllListeners();
                button.interactable = true;
                button.onClick.AddListener(() =>
                {
                    if (fileBrowserController != null)
                    {
                        fileBrowserController.NavigateTo(capturedDrive.FullPath);
                    }
                });
            }
        }

        private void EnsureFileBrowserController()
        {
            if (fileBrowserController != null)
            {
                return;
            }

            fileBrowserController = new RuntimeFileBrowserController(new RuntimeImageFolderScanner(), new RuntimePathValidator());
            fileBrowserController.StateChanged += ApplyFileBrowserState;
            fileBrowserController.FolderSelected += SelectFolderFromBrowser;
            fileBrowserController.Closed += () => SetActiveIfDifferent(browserRoot, false);
        }

        private void ApplyFileBrowserState(RuntimeFileBrowserState browserState)
        {
            if (browserState == null)
            {
                return;
            }

            SetBrowserPath(browserState.CurrentPath);

            if (browserStatusText != null)
            {
                browserStatusText.text = string.IsNullOrWhiteSpace(browserState.Message) ? "Готово." : browserState.Message;
                browserStatusText.color = browserState.HasError ? (Color)new Color32(255, 150, 120, 255) : MutedTextColor;
            }

            RefreshDriveButtons(browserState);
            RefreshBrowserList();
            RefreshBrowserDiagnostics();
        }

        private void SelectFolderFromBrowser(string folderPath)
        {
            if (director == null || string.IsNullOrWhiteSpace(folderPath))
            {
                return;
            }

            if (activeBrowserMode == BrowserMode.ImageFolder)
            {
                director.Dispatch(KaleidoscopeCommand.SetImageFolderPath(folderPath));
                director.Dispatch(KaleidoscopeCommand.SetSourceMode(KaleidoscopeSourceMode.ImageTexture));
            }
            else if (activeBrowserMode == BrowserMode.AudioFolder)
            {
                director.Dispatch(KaleidoscopeCommand.SetAudioFolderPath(folderPath));
            }

            SyncUiFromState();
            SetActiveIfDifferent(browserRoot, false);
        }

        private void ToggleBrowserDiagnostics()
        {
            browserDiagnosticsVisible = !browserDiagnosticsVisible;
            if (browserDiagnosticsText != null)
            {
                browserDiagnosticsText.gameObject.SetActive(browserDiagnosticsVisible);
            }

            RefreshBrowserDiagnostics();
        }

        private void RefreshBrowserDiagnostics()
        {
            if (!browserDiagnosticsVisible || browserDiagnosticsText == null || director == null)
            {
                return;
            }

            RuntimeFileBrowserState browserState = fileBrowserController != null ? fileBrowserController.State : null;
            MirrorSettings mirror = director.State != null ? director.State.MirrorSettings : null;
            string folder = browserState != null ? browserState.CurrentPath : string.Empty;
            int images = browserState != null ? browserState.ImageCount : 0;
            int segments = mirror != null ? mirror.MirrorCount : 0;

            browserDiagnosticsText.text =
                "Current folder: " + folder + "\n" +
                "Images found: " + images + "\n" +
                "Current image index: см. Source status\n" +
                "Next switch in: см. Source status\n" +
                "Segment count: " + segments + "\n" +
                "Slideshow active: " + (!string.IsNullOrWhiteSpace(director.State.ImageFolderPath)).ToString();
        }

        private string[] GetAllowedExtensionsForMode(BrowserMode mode)
        {
            switch (mode)
            {
                case BrowserMode.ImageFile:
                case BrowserMode.ImageFolder:
                    return ImageExtensions;
                case BrowserMode.AudioFile:
                case BrowserMode.AudioFolder:
                    return AudioExtensions;
                default:
                    return ImageExtensions;
            }
        }

        private string GetFileSectionTitle()
        {
            switch (activeBrowserMode)
            {
                case BrowserMode.ImageFile:
                case BrowserMode.ImageFolder:
                    return "Изображения:";
                case BrowserMode.AudioFile:
                case BrowserMode.AudioFolder:
                    return "Аудиофайлы:";
                default:
                    return "Файлы:";
            }
        }

        private void TryNavigateToPath(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            value = value.Trim();

            try
            {
                if (fileBrowserController != null)
                {
                    fileBrowserController.NavigateTo(value);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Control] Navigate to path failed: " + exception.Message);
            }
        }

        private void OpenHelp()
        {
            SetActiveIfDifferent(helpRoot, true);
        }

        private void ToggleMode()
        {
            if (director == null)
            {
                return;
            }

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
            SyncUiFromState();
        }

        private static string GetModeLabel(KaleidoscopeVisualMode mode)
        {
            switch (mode)
            {
                case KaleidoscopeVisualMode.Tunnel:
                    return "3D";
                case KaleidoscopeVisualMode.Hose:
                    return "4D";
                case KaleidoscopeVisualMode.FiveD:
                    return "5D";
                case KaleidoscopeVisualMode.SixD:
                    return "6D";
                case KaleidoscopeVisualMode.SevenD:
                    return "7D";
                default:
                    return "2D";
            }
        }

        private static string GetSevenDStrategyLabel(SevenDVisualizationStrategy strategy)
        {
            switch (strategy)
            {
                case SevenDVisualizationStrategy.Snowflake:
                    return "Снежинки";
                case SevenDVisualizationStrategy.StructuralColor:
                    return "Структурный цвет";
                case SevenDVisualizationStrategy.Murmuration:
                    return "Мурмурация";
                case SevenDVisualizationStrategy.Sunflower:
                    return "Подсолнух";
                default:
                    return "Капуста Романеско";
            }
        }

        private void ToggleGuides()
        {
            if (director == null)
            {
                return;
            }

            director.Dispatch(KaleidoscopeCommand.ToggleMirrorGuides());
            SyncUiFromState();
        }

        private void ToggleHoseChromaticAberration()
        {
            if (director == null)
            {
                return;
            }

            director.Dispatch(KaleidoscopeCommand.ToggleTunnelHoseChromaticAberration());
            SyncUiFromState();
        }

        private void CycleSevenDStrategy(int direction)
        {
            if (director == null)
            {
                return;
            }

            director.Dispatch(KaleidoscopeCommand.CycleSevenDStrategy(direction));
            SyncUiFromState();
        }

        private void ResetMotion()
        {
            if (director == null)
            {
                return;
            }

            director.Dispatch(KaleidoscopeCommand.SetMirrorRotationSpeedUnits(0f));
            director.Dispatch(KaleidoscopeCommand.SetTunnelBend(Vector2.zero));
            director.Dispatch(KaleidoscopeCommand.ResetTunnelHoseProfile());
            director.Dispatch(KaleidoscopeCommand.SetFiveDFlightSpeedUnits(0f));
            director.Dispatch(KaleidoscopeCommand.ResetVisualMotion(KaleidoscopeVisualMode.Classic));
            director.Dispatch(KaleidoscopeCommand.ResetVisualMotion(KaleidoscopeVisualMode.Tunnel));
            director.Dispatch(KaleidoscopeCommand.ResetVisualMotion(KaleidoscopeVisualMode.Hose));
            director.Dispatch(KaleidoscopeCommand.ResetVisualMotion(KaleidoscopeVisualMode.SixD));
            director.Dispatch(KaleidoscopeCommand.ResetVisualMotion(KaleidoscopeVisualMode.SevenD));
            SyncUiFromState();
        }

        private void OpenBrowser(BrowserMode mode)
        {
            if (director == null)
            {
                return;
            }

            activeBrowserMode = mode;
            EnsureFileBrowserController();
            SetActiveIfDifferent(browserRoot, true);

            // Folder select only makes sense for folder modes.
            bool folderMode = mode == BrowserMode.ImageFolder || mode == BrowserMode.AudioFolder;
            if (browserSelectFolderButton != null)
            {
                browserSelectFolderButton.gameObject.SetActive(folderMode);
            }

            string startPath = GetStartPathForMode(mode);
            if (fileBrowserController != null)
            {
                fileBrowserController.SetAllowedFileExtensions(GetAllowedExtensionsForMode(mode));
                fileBrowserController.Open(startPath);
            }
        }

        private string GetStartPathForMode(BrowserMode mode)
        {
            KaleidoscopeState state = director.State;
            string candidate = string.Empty;

            switch (mode)
            {
                case BrowserMode.ImageFile:
                    candidate = state.ImageFilePath;
                    break;
                case BrowserMode.ImageFolder:
                    candidate = state.ImageFolderPath;
                    break;
                case BrowserMode.AudioFile:
                    candidate = state.AudioFilePath;
                    break;
                case BrowserMode.AudioFolder:
                    candidate = state.AudioFolderPath;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(candidate))
            {
                try
                {
                    if (File.Exists(candidate))
                    {
                        return Path.GetDirectoryName(candidate);
                    }
                    if (Directory.Exists(candidate))
                    {
                        return candidate;
                    }
                }
                catch
                {
                    // ignore and fall back
                }
            }

            // Default to user's documents, then C:\.
            try
            {
                string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (!string.IsNullOrWhiteSpace(documents) && Directory.Exists(documents))
                {
                    return documents;
                }
            }
            catch
            {
            }

            return "C:\\";
        }

        private void SetBrowserPath(string path)
        {
            browserCurrentPath = path;
            if (browserPathText != null)
            {
                browserPathText.text = "Текущий путь: " + (path ?? string.Empty);
            }

            if (browserPathInput != null)
            {
                browserPathInput.text = path ?? string.Empty;
            }
        }

        private void NavigateUp()
        {
            if (string.IsNullOrWhiteSpace(browserCurrentPath))
            {
                return;
            }

            if (fileBrowserController != null)
            {
                fileBrowserController.NavigateUp();
            }
        }

        private void SelectCurrentFolder()
        {
            if (director == null || string.IsNullOrWhiteSpace(browserCurrentPath))
            {
                return;
            }

            if (activeBrowserMode == BrowserMode.ImageFolder)
            {
                if (fileBrowserController != null)
                {
                    fileBrowserController.SelectCurrentFolder();
                }
            }
            else if (activeBrowserMode == BrowserMode.AudioFolder)
            {
                if (fileBrowserController != null)
                {
                    fileBrowserController.SelectCurrentFolder();
                }
            }
        }

        private void RefreshBrowserList()
        {
            if (browserListContent == null)
            {
                return;
            }

            RuntimeFileBrowserState state = fileBrowserController != null ? fileBrowserController.State : null;
            IReadOnlyList<RuntimeFileBrowserItem> rows = fileBrowserView.BuildRows(state, GetFileSectionTitle());
            int rowIndex = 0;

            for (int index = 0; index < rows.Count; index++)
            {
                RuntimeFileBrowserItem item = rows[index];

                if (item.Type == RuntimeFileBrowserItemType.ParentDirectory)
                {
                    EnsureBrowserRow(rowIndex++, item.DisplayName, RuntimeFileBrowserItemType.ParentDirectory, NavigateUp);
                }
                else if (item.Type == RuntimeFileBrowserItemType.Directory)
                {
                    RuntimeFileBrowserItem capturedItem = item;
                    EnsureBrowserRow(rowIndex++, capturedItem.DisplayName, RuntimeFileBrowserItemType.Directory, () =>
                    {
                        if (fileBrowserController != null)
                        {
                            fileBrowserController.NavigateTo(capturedItem.FullPath);
                        }
                    });
                }
                else if (item.Type == RuntimeFileBrowserItemType.ImageFile || item.Type == RuntimeFileBrowserItemType.File)
                {
                    RuntimeFileBrowserItem capturedItem = item;
                    EnsureBrowserRow(rowIndex++, capturedItem.DisplayName, item.Type, () =>
                    {
                        SelectFile(capturedItem.FullPath);
                    });
                }
                else
                {
                    EnsureBrowserRow(rowIndex++, item.DisplayName, item.Type, null);
                }
            }

            // Disable remaining pooled rows.
            for (int i = rowIndex; i < browserRowPool.Count; i++)
            {
                if (browserRowPool[i] != null)
                {
                    browserRowPool[i].SetActive(false);
                }
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(browserListContent);
            RectTransform viewport = browserListContent.parent as RectTransform;
            if (viewport != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(viewport);
            }
            Canvas.ForceUpdateCanvases();

            LogBrowserUiHierarchy(rowIndex);
        }

        private void SelectFile(string filePath)
        {
            if (director == null)
            {
                return;
            }

            switch (activeBrowserMode)
            {
                case BrowserMode.ImageFile:
                case BrowserMode.ImageFolder:
                    director.Dispatch(KaleidoscopeCommand.SetImageFilePath(filePath));
                    director.Dispatch(KaleidoscopeCommand.SetSourceMode(KaleidoscopeSourceMode.ImageTexture));
                    break;

                case BrowserMode.AudioFile:
                case BrowserMode.AudioFolder:
                    director.Dispatch(KaleidoscopeCommand.SetAudioFilePath(filePath));
                    break;
            }

            SyncUiFromState();
            SetActiveIfDifferent(browserRoot, false);
        }

        private bool IsFileAllowedForMode(string filePath)
        {
            switch (activeBrowserMode)
            {
                case BrowserMode.ImageFile:
                case BrowserMode.ImageFolder:
                    return HasExtension(filePath, ImageExtensions);
                case BrowserMode.AudioFile:
                case BrowserMode.AudioFolder:
                    return HasExtension(filePath, AudioExtensions);
                default:
                    return false;
            }
        }

        private static bool HasExtension(string filePath, string[] allowed)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            string ext = Path.GetExtension(filePath);
            if (string.IsNullOrWhiteSpace(ext))
            {
                return false;
            }

            ext = ext.ToLowerInvariant();
            for (int i = 0; i < allowed.Length; i++)
            {
                if (ext == allowed[i])
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureBrowserRow(int index, string label, RuntimeFileBrowserItemType itemType, Action onClick)
        {
            while (browserRowPool.Count <= index)
            {
                browserRowPool.Add(null);
            }

            GameObject row = browserRowPool[index];
            if (row == null)
            {
                row = CreateBrowserRow(index, itemType);
                browserRowPool[index] = row;
            }

            row.SetActive(true);
            ConfigureBrowserRow(row, label, itemType, onClick);
        }

        private GameObject CreateBrowserRow(int index, RuntimeFileBrowserItemType itemType)
        {
            RectTransform rowRect = CreateRect("BrowserRow_" + index, browserListContent);
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(1f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.sizeDelta = new Vector2(0f, 40f);
            rowRect.localScale = Vector3.one;

            Image image = rowRect.gameObject.AddComponent<Image>();
            image.sprite = solidSprite;
            image.raycastTarget = true;
            image.color = GetBrowserRowColor(itemType, true);

            Button button = rowRect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;

            RectTransform textRect = CreateRect("Label", rowRect);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(14f, 4f);
            textRect.offsetMax = new Vector2(-14f, -4f);

            Text text = textRect.gameObject.AddComponent<Text>();
            text.font = GetUiFont();
            text.fontSize = itemType == RuntimeFileBrowserItemType.Header ? 15 : 14;
            text.fontStyle = itemType == RuntimeFileBrowserItemType.Header ? FontStyle.Bold : FontStyle.Normal;
            text.color = TextColor;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;

            return rowRect.gameObject;
        }

        private void ConfigureBrowserRow(GameObject row, string label, RuntimeFileBrowserItemType itemType, Action onClick)
        {
            if (row == null)
            {
                return;
            }

            bool selectable = onClick != null;

            LayoutElement element = row.GetComponent<LayoutElement>();
            if (element == null)
            {
                element = row.AddComponent<LayoutElement>();
            }

            element.minHeight = itemType == RuntimeFileBrowserItemType.Header ? 32f : 38f;
            element.preferredHeight = itemType == RuntimeFileBrowserItemType.Header ? 34f : 40f;
            element.flexibleWidth = 1f;

            Image image = row.GetComponent<Image>();
            if (image != null)
            {
                image.color = GetBrowserRowColor(itemType, selectable);
            }

            Button button = row.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.interactable = selectable;
                if (onClick != null)
                {
                    button.onClick.AddListener(() => onClick());
                }
            }

            Text text = row.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = label;
                text.fontSize = itemType == RuntimeFileBrowserItemType.Header ? 15 : 14;
                text.fontStyle = itemType == RuntimeFileBrowserItemType.Header ? FontStyle.Bold : FontStyle.Normal;
                text.color = itemType == RuntimeFileBrowserItemType.Message ? (Color)new Color32(255, 220, 150, 255) : TextColor;
                text.alignment = TextAnchor.MiddleLeft;
            }
        }

        private Color GetBrowserRowColor(RuntimeFileBrowserItemType itemType, bool selectable)
        {
            if (itemType == RuntimeFileBrowserItemType.Header)
            {
                return new Color32(18, 28, 38, 255);
            }

            if (itemType == RuntimeFileBrowserItemType.Message)
            {
                return new Color32(48, 36, 24, 245);
            }

            if (itemType == RuntimeFileBrowserItemType.Directory)
            {
                return new Color32(44, 76, 105, 255);
            }

            if (itemType == RuntimeFileBrowserItemType.ImageFile || itemType == RuntimeFileBrowserItemType.File)
            {
                return new Color32(38, 58, 68, 255);
            }

            return selectable ? ButtonColor : (Color)new Color32(24, 32, 42, 230);
        }

        private void LogBrowserUiHierarchy(int activeRowCount)
        {
            if (browserListContent == null)
            {
                Debug.LogWarning("[FileBrowserUI] content is null.");
                return;
            }

            Rect rect = browserListContent.rect;
            RectTransform viewport = browserListContent.parent as RectTransform;
            Debug.Log("[FileBrowserUI] content.activeInHierarchy=" + browserListContent.gameObject.activeInHierarchy
                + " content.rect.width=" + rect.width.ToString("0.0")
                + " content.rect.height=" + rect.height.ToString("0.0")
                + " viewport.activeInHierarchy=" + (viewport != null ? viewport.gameObject.activeInHierarchy.ToString() : "null")
                + " viewport.rect.width=" + (viewport != null ? viewport.rect.width.ToString("0.0") : "null")
                + " viewport.rect.height=" + (viewport != null ? viewport.rect.height.ToString("0.0") : "null")
                + " activeRows=" + activeRowCount);

            for (int index = 0; index < activeRowCount && index < browserRowPool.Count; index++)
            {
                GameObject row = browserRowPool[index];
                if (row == null)
                {
                    Debug.LogWarning("[FileBrowserUI] row " + index + " is null.");
                    continue;
                }

                RectTransform rowRect = row.transform as RectTransform;
                Text text = row.GetComponentInChildren<Text>();
                float height = rowRect != null ? rowRect.rect.height : 0f;
                string parentName = rowRect != null && rowRect.parent != null ? rowRect.parent.name : "none";
                int fontSize = text != null ? text.fontSize : 0;
                string color = text != null ? text.color.ToString() : "none";
                Debug.Log("[FileBrowserUI] row " + index
                    + " active=" + row.activeInHierarchy
                    + " height=" + height.ToString("0.0")
                    + " parent=" + parentName
                    + " fontSize=" + fontSize
                    + " textColor=" + color
                    + " label=" + (text != null ? text.text : "none"));
            }
        }

        private void EnsureSprites()
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

        private RectTransform CreatePanel(RectTransform parent, string name, Vector2 size)
        {
            RectTransform panel = CreateRect(name, parent);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = size;
            panel.anchoredPosition = Vector2.zero;

            Image image = panel.gameObject.AddComponent<Image>();
            image.sprite = solidSprite;
            image.color = PanelColor;

            return panel;
        }

        private void CreateTitle(RectTransform parent, string title, string subtitle)
        {
            Text titleText = CreateText(parent, "Title", title, 18, FontStyle.Bold, TextColor, TextAnchor.MiddleLeft);
            titleText.gameObject.AddComponent<LayoutElement>().preferredHeight = 28f;

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                Text subtitleText = CreateText(parent, "Subtitle", subtitle, 13, FontStyle.Normal, MutedTextColor, TextAnchor.MiddleLeft);
                subtitleText.gameObject.AddComponent<LayoutElement>().preferredHeight = 20f;
            }
        }

        private RectTransform CreateRow(RectTransform parent, string label, out Text valueText)
        {
            RectTransform row = CreateRect("Row_" + label, parent);
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            Text labelText = CreateText(row, "Label", label, 14, FontStyle.Bold, TextColor, TextAnchor.MiddleLeft);
            labelText.gameObject.AddComponent<LayoutElement>().preferredWidth = 140f;

            valueText = CreateText(row, "Value", "—", 14, FontStyle.Normal, Accent, TextAnchor.MiddleLeft);
            valueText.gameObject.AddComponent<LayoutElement>().preferredWidth = 400f;

            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 40f;
            return row;
        }

        private Button CreateButton(RectTransform parent, string name, string label, Color background, Action onClick)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.gameObject.AddComponent<LayoutElement>().preferredHeight = 40f;

            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = solidSprite;
            image.color = background;

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.interactable = onClick != null;
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }

            CreateText(rect, "Label", label, 14, FontStyle.Bold, TextColor, TextAnchor.MiddleCenter);

            return button;
        }

        private Text CreateText(RectTransform parent, string name, string value, int fontSize, FontStyle style, Color color, TextAnchor alignment)
        {
            RectTransform rect = CreateRect(name, parent);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = GetUiFont();
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;

            Stretch(rect);
            return text;
        }

        private InputField CreateInputField(RectTransform parent, string name, string placeholder, int fontSize)
        {
            RectTransform root = CreateRect(name, parent);
            root.gameObject.AddComponent<Image>().sprite = solidSprite;
            root.gameObject.GetComponent<Image>().color = PanelStrongColor;
            root.gameObject.AddComponent<LayoutElement>().preferredHeight = 38f;

            InputField input = root.gameObject.AddComponent<InputField>();

            // Text
            Text text = CreateText(root, "Text", string.Empty, fontSize, FontStyle.Normal, TextColor, TextAnchor.MiddleLeft);
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(10f, 6f);
            textRect.offsetMax = new Vector2(-10f, -6f);

            // Placeholder
            Text hint = CreateText(root, "Placeholder", placeholder, fontSize, FontStyle.Italic, MutedTextColor, TextAnchor.MiddleLeft);
            RectTransform hintRect = hint.rectTransform;
            hintRect.anchorMin = new Vector2(0f, 0f);
            hintRect.anchorMax = new Vector2(1f, 1f);
            hintRect.offsetMin = new Vector2(10f, 6f);
            hintRect.offsetMax = new Vector2(-10f, -6f);

            input.textComponent = text;
            input.placeholder = hint;
            input.lineType = InputField.LineType.SingleLine;
            input.contentType = InputField.ContentType.Standard;
            input.characterLimit = 260;

            return input;
        }

        private static void SetLayoutPreferred(GameObject target, float preferredWidth, float preferredHeight)
        {
            if (target == null)
            {
                return;
            }

            LayoutElement element = target.GetComponent<LayoutElement>();
            if (element == null)
            {
                element = target.AddComponent<LayoutElement>();
            }

            if (preferredWidth >= 0f)
            {
                element.preferredWidth = preferredWidth;
            }

            if (preferredHeight >= 0f)
            {
                element.preferredHeight = preferredHeight;
            }
        }

        private Font GetUiFont()
        {
            if (uiFont != null)
            {
                return uiFont;
            }

            uiFont = LoadBuiltinFont("LegacyRuntime.ttf");
            if (uiFont == null)
            {
                uiFont = LoadBuiltinFont("Arial.ttf");
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

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void SetActiveIfDifferent(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }

        private static string ShortPath(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (value.Length <= 60)
            {
                return value;
            }

            return "..." + value.Substring(value.Length - 57);
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

        private enum BrowserMode
        {
            ImageFile = 0,
            ImageFolder = 1,
            AudioFile = 2,
            AudioFolder = 3
        }
    }
}
