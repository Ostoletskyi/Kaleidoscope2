using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu
{
    internal sealed class KaelisContentSelectionPanel
    {
        private readonly KaelisMenuAssets assets;
        private readonly KaelisMenuTooltip tooltip;
        private readonly RectTransform root;
        private readonly CanvasGroup canvasGroup;
        private TMP_Text imagePathText;
        private TMP_Text musicPathText;
        private TMP_Text validationText;

        private Action selectImageFolder;
        private Action clearImageFolder;
        private Action selectMusicFolder;
        private Action clearMusicFolder;
        private Action startExperience;
        private Action back;

        public KaelisContentSelectionPanel(RectTransform parent, KaelisMenuAssets assets, KaelisMenuTooltip tooltip)
        {
            this.assets = assets;
            this.tooltip = tooltip;

            root = KaelisMenuUiPrimitives.CreateRect("ContentSelectionFlow", parent);
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.sizeDelta = new Vector2(980f, 700f);
            root.anchoredPosition = Vector2.zero;

            KaelisMenuUiPrimitives.AddImage(root, assets.SolidSprite, new Color(0.006f, 0.030f, 0.044f, 0.88f), true);
            KaelisMenuUiPrimitives.AddFrame(root, new Color(0.76f, 0.98f, 1f, 0.46f), new Color(0.20f, 0.82f, 0.92f, 0.28f), 1.1f, assets.SolidSprite);
            KaelisMenuUiPrimitives.AddCornerCuts(root, new Color(1f, 0.76f, 0.36f, 0.38f), 42f, 1.25f, assets.SolidSprite);

            RectTransform haze = KaelisMenuUiPrimitives.CreateRect("ContentSelectionHaze", root);
            KaelisMenuUiPrimitives.Stretch(haze);
            KaelisMenuUiPrimitives.AddImage(haze, assets.SolidSprite, new Color(0.13f, 0.62f, 0.82f, 0.08f), false);

            canvasGroup = root.gameObject.AddComponent<CanvasGroup>();
            Build();
            SetVisible(false);
        }

        public GameObject GameObject
        {
            get { return root.gameObject; }
        }

        public void SetHandlers(Action selectImageFolder, Action clearImageFolder, Action selectMusicFolder, Action clearMusicFolder, Action startExperience, Action back)
        {
            this.selectImageFolder = selectImageFolder;
            this.clearImageFolder = clearImageFolder;
            this.selectMusicFolder = selectMusicFolder;
            this.clearMusicFolder = clearMusicFolder;
            this.startExperience = startExperience;
            this.back = back;
        }

        public void SetVisible(bool visible)
        {
            root.gameObject.SetActive(visible);
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        public void SetImageFolder(string path, bool valid)
        {
            SetPathText(imagePathText, path, valid, "No image folder selected");
        }

        public void SetMusicFolder(string path, bool valid)
        {
            SetPathText(musicPathText, path, valid, "No music folder selected");
        }

        public void SetValidation(string message, bool warning)
        {
            if (validationText == null)
            {
                return;
            }

            KaelisMenuLocalizationService.SetText(validationText, message);
            validationText.color = warning ? new Color(1f, 0.70f, 0.32f, 0.96f) : new Color(0.70f, 0.96f, 1f, 0.90f);
        }

        private void Build()
        {
            TMP_Text title = KaelisMenuUiPrimitives.CreateText(root, "ContentSelectionTitle", "SELECT EXPERIENCE CONTENT", 30f, KaelisMenuStyle.TextPrimary, TextAlignmentOptions.Center, assets.GetFont(KaelisMenuFontRole.Button));
            title.characterSpacing = 8f;
            RectTransform titleRect = (RectTransform)title.transform;
            titleRect.offsetMin = new Vector2(60f, 612f);
            titleRect.offsetMax = new Vector2(-60f, -34f);

            TMP_Text subtitle = KaelisMenuUiPrimitives.CreateText(root, "ContentSelectionSubtitle", "Choose image and audio sources for this session.", 15f, KaelisMenuStyle.TextSecondary, TextAlignmentOptions.Center, assets.GetFont(KaelisMenuFontRole.Status));
            subtitle.characterSpacing = 3f;
            RectTransform subtitleRect = (RectTransform)subtitle.transform;
            subtitleRect.offsetMin = new Vector2(70f, 570f);
            subtitleRect.offsetMax = new Vector2(-70f, -86f);

            imagePathText = BuildSourceBlock(
                "ImagesSourceBlock",
                "IMAGES SOURCE",
                "Supported: .jpg, .jpeg, .png, .bmp, .tga",
                "SELECT IMAGE FOLDER",
                "SelectImageFolderButton",
                "ClearImageFolderButton",
                new Vector2(58f, 342f),
                () => Invoke(selectImageFolder),
                () => Invoke(clearImageFolder));

            musicPathText = BuildSourceBlock(
                "MusicSourceBlock",
                "MUSIC SOURCE",
                "Supported: .mp3, .wav, .ogg, .aiff, .aif. Optional for visual-only sessions.",
                "SELECT MUSIC FOLDER",
                "SelectMusicFolderButton",
                "ClearMusicFolderButton",
                new Vector2(58f, 144f),
                () => Invoke(selectMusicFolder),
                () => Invoke(clearMusicFolder));

            validationText = KaelisMenuUiPrimitives.CreateText(root, "ContentValidationMessage", "Image folder is required. Music folder is optional.", 14f, new Color(0.70f, 0.96f, 1f, 0.90f), TextAlignmentOptions.Center, assets.GetFont(KaelisMenuFontRole.Status));
            validationText.enableWordWrapping = true;
            RectTransform validationRect = (RectTransform)validationText.transform;
            validationRect.offsetMin = new Vector2(80f, 94f);
            validationRect.offsetMax = new Vector2(-80f, -568f);

            RectTransform actions = KaelisMenuUiPrimitives.CreateRect("ContentSelectionActions", root);
            actions.anchorMin = new Vector2(0f, 0f);
            actions.anchorMax = new Vector2(1f, 0f);
            actions.pivot = new Vector2(0.5f, 0f);
            actions.offsetMin = new Vector2(236f, 36f);
            actions.offsetMax = new Vector2(-236f, 92f);
            HorizontalLayoutGroup layout = actions.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            BuildChip(actions, "BackContentButton", "BACK", KaelisMenuStyle.Cyan, () => Invoke(back));
            BuildChip(actions, "StartExperienceButton", "START EXPERIENCE", KaelisMenuStyle.GoldSoft, () => Invoke(startExperience));

            SetImageFolder(null, false);
            SetMusicFolder(null, false);
        }

        private TMP_Text BuildSourceBlock(string name, string title, string hint, string buttonLabel, string selectButtonName, string clearButtonName, Vector2 offsetMin, Action selectAction, Action clearAction)
        {
            RectTransform block = KaelisMenuUiPrimitives.CreateRect(name, root);
            block.anchorMin = new Vector2(0f, 0f);
            block.anchorMax = new Vector2(1f, 0f);
            block.offsetMin = offsetMin;
            block.offsetMax = new Vector2(-58f, offsetMin.y + 170f);
            KaelisMenuUiPrimitives.AddImage(block, assets.SolidSprite, new Color(0.010f, 0.060f, 0.080f, 0.58f), false);
            KaelisMenuUiPrimitives.AddFrame(block, new Color(0.25f, 0.88f, 1f, 0.28f), new Color(1f, 0.76f, 0.36f, 0.16f), 0.8f, assets.SolidSprite);

            TMP_Text titleText = KaelisMenuUiPrimitives.CreateText(block, "SourceTitle", title, 18f, KaelisMenuStyle.TextPrimary, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Button));
            titleText.characterSpacing = 5f;
            RectTransform titleRect = (RectTransform)titleText.transform;
            titleRect.offsetMin = new Vector2(26f, 116f);
            titleRect.offsetMax = new Vector2(-300f, -16f);

            TMP_Text path = KaelisMenuUiPrimitives.CreateText(block, "SourcePath", string.Empty, 14f, KaelisMenuStyle.TextSecondary, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Status));
            path.enableWordWrapping = true;
            RectTransform pathRect = (RectTransform)path.transform;
            pathRect.offsetMin = new Vector2(26f, 54f);
            pathRect.offsetMax = new Vector2(-300f, -64f);

            TMP_Text hintText = KaelisMenuUiPrimitives.CreateText(block, "SourceHint", hint, 12f, KaelisMenuStyle.TextMuted, TextAlignmentOptions.Left, assets.GetFont(KaelisMenuFontRole.Status));
            hintText.enableWordWrapping = true;
            RectTransform hintRect = (RectTransform)hintText.transform;
            hintRect.offsetMin = new Vector2(26f, 16f);
            hintRect.offsetMax = new Vector2(-300f, -116f);

            RectTransform buttons = KaelisMenuUiPrimitives.CreateRect("SourceActions", block);
            buttons.anchorMin = new Vector2(1f, 0.5f);
            buttons.anchorMax = new Vector2(1f, 0.5f);
            buttons.pivot = new Vector2(1f, 0.5f);
            buttons.sizeDelta = new Vector2(240f, 114f);
            buttons.anchoredPosition = new Vector2(-24f, 0f);
            VerticalLayoutGroup layout = buttons.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            BuildChip(buttons, selectButtonName, buttonLabel, KaelisMenuStyle.Cyan, selectAction);
            BuildChip(buttons, clearButtonName, "CLEAR", KaelisMenuStyle.TextMuted, clearAction);
            return path;
        }

        private void BuildChip(RectTransform parent, string name, string label, Color accent, Action action)
        {
            RectTransform chip = KaelisMenuUiPrimitives.CreateRect(name, parent);
            KaelisMenuUiPrimitives.AddLayout(chip.gameObject, -1f, 48f);
            Image surface = KaelisMenuUiPrimitives.AddImage(chip, assets.SolidSprite, new Color(0.008f, 0.050f, 0.068f, 0.72f), true);
            KaelisMenuUiPrimitives.AddFrame(chip, new Color(accent.r, accent.g, accent.b, 0.48f), new Color(accent.r, accent.g, accent.b, 0.22f), 0.95f, assets.SolidSprite);
            KaelisMenuSliderControl.AddWideHighlight(chip, assets, out CanvasGroup highlightGroup, out CanvasGroup flashGroup);

            KaelisMenuInteractiveRow interactive = chip.gameObject.AddComponent<KaelisMenuInteractiveRow>();
            interactive.Configure(surface, highlightGroup, flashGroup, tooltip, label, "Menu action.", string.Empty, string.Empty, KaelisMenuInputHintProvider.Get(KaelisMenuInputHintKind.Action));

            Button button = chip.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = surface;
            button.onClick.AddListener(() =>
            {
                interactive.Flash();
                Invoke(action);
            });

            TMP_Text text = KaelisMenuUiPrimitives.CreateText(chip, "Label", label, 13f, KaelisMenuStyle.TextPrimary, TextAlignmentOptions.Center, assets.GetFont(KaelisMenuFontRole.Button));
            text.characterSpacing = 3f;
            RectTransform textRect = (RectTransform)text.transform;
            textRect.offsetMin = new Vector2(8f, 0f);
            textRect.offsetMax = new Vector2(-8f, 0f);
        }

        private static void SetPathText(TMP_Text target, string path, bool valid, string emptyText)
        {
            if (target == null)
            {
                return;
            }

            bool hasPath = !string.IsNullOrWhiteSpace(path);
            if (hasPath)
            {
                KaelisMenuLocalizationService.SetRawText(target, path);
            }
            else
            {
                KaelisMenuLocalizationService.SetText(target, emptyText);
            }

            target.color = valid
                ? new Color(0.78f, 1f, 0.94f, 0.95f)
                : new Color(0.92f, 0.82f, 0.68f, 0.86f);
        }

        private static void Invoke(Action action)
        {
            if (action != null)
            {
                action();
            }
        }
    }
}
