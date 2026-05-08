using System;
using System.Collections.Generic;
using System.IO;
using Kaleidoscope2.Core;
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

        private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg" };
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
        private Text forwardSpeedValueText;
        private Text rotationSpeedValueText;

        private BrowserMode activeBrowserMode;
        private string browserCurrentPath;
        private RectTransform browserListContent;
        private Text browserPathText;
        private Button browserSelectFolderButton;
        private InputField browserPathInput;

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

            if (modeValueText != null)
            {
                modeValueText.text = state.TunnelEnabled ? "3D (Tunnel)" : "2D";
            }

            if (guidesValueText != null)
            {
                guidesValueText.text = mirror != null && mirror.GuidesVisible ? "Вкл" : "Выкл";
            }

            if (forwardSpeedValueText != null)
            {
                forwardSpeedValueText.text = mirror != null ? mirror.ForwardSpeedUnits.ToString("0") : "0";
            }

            if (rotationSpeedValueText != null)
            {
                rotationSpeedValueText.text = mirror != null ? mirror.RotationSpeed.ToString("0") : "0";
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

            RectTransform panel = CreatePanel(rootRect, "MenuPanel", new Vector2(620f, 680f));
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
            CreateButton(modeRow, "ToggleMode", "2D / 3D", ButtonColor, ToggleMode);

            RectTransform guidesRow = CreateRow(panel, "Линии:", out guidesValueText);
            CreateButton(guidesRow, "ToggleGuides", "Вкл/Выкл", ButtonColor, ToggleGuides);

            CreateRow(panel, "Скорость вперёд:", out forwardSpeedValueText);
            CreateRow(panel, "Скорость вращения:", out rotationSpeedValueText);
            CreateButton(panel, "ResetSpeeds", "Сбросить скорости", MutedButtonColor, ResetSpeeds);

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

            RectTransform panel = CreatePanel(rootRect, "HelpPanel", new Vector2(720f, 640f));

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
                "Num0 (доп. клавиатура) — линии сегментов (вкл/выкл)\n" +
                "Стрелки: Вверх/Вниз — скорость движения вперёд, Влево/Вправо — скорость вращения\n\n" +
                "Режимы:\n" +
                "2D — классический калейдоскоп (сегменты от центра).\n" +
                "3D — туннельная перспектива на основе финальной текстуры.\n\n" +
                "Источники:\n" +
                "Изображение: PNG/JPG.\n" +
                "Музыка: MP3/WAV/OGG/AIFF (в зависимости от поддержки Unity на вашей платформе).",
                14,
                FontStyle.Normal,
                MutedTextColor,
                TextAnchor.UpperLeft);
            body.gameObject.AddComponent<LayoutElement>().preferredHeight = 420f;

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
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            CreateTitle(panel, "Выбор файлов", string.Empty);

            browserPathText = CreateText(panel, "BrowserPath", "", 14, FontStyle.Bold, TextColor, TextAnchor.MiddleLeft);
            browserPathText.gameObject.AddComponent<LayoutElement>().preferredHeight = 28f;

            // Path input (direct jump) and drive chooser.
            RectTransform jumpRow = CreateRect("JumpRow", panel);
            HorizontalLayoutGroup jumpLayout = jumpRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            jumpLayout.spacing = 10f;
            jumpLayout.childControlHeight = true;
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
            HorizontalLayoutGroup drivesLayout = drivesRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            drivesLayout.spacing = 8f;
            drivesLayout.childControlHeight = true;
            drivesLayout.childForceExpandHeight = false;
            drivesRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 38f;

            BuildDriveButtons(drivesRow);

            RectTransform actions = CreateRect("Actions", panel);
            HorizontalLayoutGroup actionsLayout = actions.gameObject.AddComponent<HorizontalLayoutGroup>();
            actionsLayout.spacing = 10f;
            actionsLayout.childControlHeight = true;
            actionsLayout.childForceExpandHeight = false;

            CreateButton(actions, "Up", "Вверх", ButtonColor, NavigateUp);
            browserSelectFolderButton = CreateButton(actions, "SelectFolder", "Выбрать эту папку", ButtonColor, SelectCurrentFolder);
            CreateButton(actions, "CloseBrowser", "Закрыть", MutedButtonColor, () => SetActiveIfDifferent(browserRoot, false));

            // List
            RectTransform listRoot = CreateRect("ListRoot", panel);
            listRoot.gameObject.AddComponent<Image>().color = PanelStrongColor;
            listRoot.gameObject.GetComponent<Image>().sprite = solidSprite;
            LayoutElement listRootLayout = listRoot.gameObject.AddComponent<LayoutElement>();
            listRootLayout.preferredHeight = 500f;

            ScrollRect scroll = listRoot.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            RectTransform viewport = CreateRect("Viewport", listRoot);
            viewport.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            Mask mask = viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            Stretch(viewport);
            scroll.viewport = viewport;

            browserListContent = CreateRect("Content", viewport);
            VerticalLayoutGroup contentLayout = browserListContent.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(10, 10, 10, 10);
            contentLayout.spacing = 6f;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandHeight = false;

            ContentSizeFitter fitter = browserListContent.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = browserListContent;

            browserRoot.SetActive(false);
        }

        private void BuildDriveButtons(RectTransform parent)
        {
            try
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                for (int i = 0; i < drives.Length; i++)
                {
                    DriveInfo drive = drives[i];
                    if (drive == null)
                    {
                        continue;
                    }

                    string root = drive.Name;
                    string label = root.TrimEnd('\\');

                    Button button = CreateButton(parent, "Drive_" + label, label, MutedButtonColor, () =>
                    {
                        SetBrowserPath(root);
                        RefreshBrowserList();
                    });
                    SetLayoutPreferred(button.gameObject, 72f, 40f);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Control] Drive listing failed: " + exception.Message);
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
                if (File.Exists(value))
                {
                    string folder = Path.GetDirectoryName(value);
                    if (!string.IsNullOrWhiteSpace(folder))
                    {
                        SetBrowserPath(folder);
                        RefreshBrowserList();
                    }

                    return;
                }

                if (Directory.Exists(value))
                {
                    SetBrowserPath(value);
                    RefreshBrowserList();
                    return;
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

            bool nextTunnelEnabled = !director.State.TunnelEnabled;
            director.Dispatch(KaleidoscopeCommand.SetTunnelEnabled(nextTunnelEnabled));
            SyncUiFromState();
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

        private void ResetSpeeds()
        {
            if (director == null)
            {
                return;
            }

            director.Dispatch(KaleidoscopeCommand.SetMirrorForwardSpeedUnits(0f));
            director.Dispatch(KaleidoscopeCommand.SetMirrorRotationSpeedUnits(0f));
            SyncUiFromState();
        }

        private void OpenBrowser(BrowserMode mode)
        {
            if (director == null)
            {
                return;
            }

            activeBrowserMode = mode;
            SetActiveIfDifferent(browserRoot, true);

            // Folder select only makes sense for folder modes.
            bool folderMode = mode == BrowserMode.ImageFolder || mode == BrowserMode.AudioFolder;
            if (browserSelectFolderButton != null)
            {
                browserSelectFolderButton.gameObject.SetActive(folderMode);
            }

            string startPath = GetStartPathForMode(mode);
            SetBrowserPath(startPath);
            RefreshBrowserList();
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
                browserPathText.text = path ?? string.Empty;
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

            try
            {
                DirectoryInfo parent = Directory.GetParent(browserCurrentPath);
                if (parent != null)
                {
                    SetBrowserPath(parent.FullName);
                    RefreshBrowserList();
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Control] NavigateUp failed: " + exception.Message);
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
                director.Dispatch(KaleidoscopeCommand.SetImageFolderPath(browserCurrentPath));
                director.Dispatch(KaleidoscopeCommand.SetSourceMode(KaleidoscopeSourceMode.ImageTexture));
            }
            else if (activeBrowserMode == BrowserMode.AudioFolder)
            {
                director.Dispatch(KaleidoscopeCommand.SetAudioFolderPath(browserCurrentPath));
            }

            SyncUiFromState();
            SetActiveIfDifferent(browserRoot, false);
        }

        private void RefreshBrowserList()
        {
            if (browserListContent == null)
            {
                return;
            }

            string path = browserCurrentPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (!Directory.Exists(path))
            {
                Debug.LogWarning("[Control] Directory does not exist: " + path);
                return;
            }

            int rowIndex = 0;

            try
            {
                string[] dirs = Directory.GetDirectories(path);
                Array.Sort(dirs, StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < dirs.Length; i++)
                {
                    string dir = dirs[i];
                    EnsureBrowserRow(rowIndex++, "[DIR] " + Path.GetFileName(dir), () =>
                    {
                        SetBrowserPath(dir);
                        RefreshBrowserList();
                    });
                }

                string[] files = Directory.GetFiles(path);
                Array.Sort(files, StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < files.Length; i++)
                {
                    string file = files[i];
                    if (!IsFileAllowedForMode(file))
                    {
                        continue;
                    }

                    EnsureBrowserRow(rowIndex++, Path.GetFileName(file), () =>
                    {
                        SelectFile(file);
                    });
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Control] RefreshBrowserList failed: " + exception.Message);
            }

            // Disable remaining pooled rows.
            for (int i = rowIndex; i < browserRowPool.Count; i++)
            {
                if (browserRowPool[i] != null)
                {
                    browserRowPool[i].SetActive(false);
                }
            }
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
                    director.Dispatch(KaleidoscopeCommand.SetImageFilePath(filePath));
                    director.Dispatch(KaleidoscopeCommand.SetSourceMode(KaleidoscopeSourceMode.ImageTexture));
                    break;

                case BrowserMode.AudioFile:
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
                    return HasExtension(filePath, ImageExtensions);
                case BrowserMode.AudioFile:
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

        private void EnsureBrowserRow(int index, string label, Action onClick)
        {
            while (browserRowPool.Count <= index)
            {
                browserRowPool.Add(null);
            }

            GameObject row = browserRowPool[index];
            if (row == null)
            {
                row = CreateButton(browserListContent, "Row" + index, label, ButtonColor, onClick).gameObject;
                browserRowPool[index] = row;
            }
            else
            {
                row.SetActive(true);
                Text text = row.GetComponentInChildren<Text>();
                if (text != null)
                {
                    text.text = label;
                }

                Button button = row.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => onClick());
                }
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
            button.onClick.AddListener(() => onClick());

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
