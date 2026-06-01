using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu.About
{
    internal sealed class KaelisAboutScreenView
    {
        private readonly KaelisMenuAssets assets;

        public KaelisAboutScreenView(KaelisMenuAssets assets)
        {
            this.assets = assets;
        }

        public void Build(RectTransform parent, KaelisAboutContent content)
        {
            RectTransform stack = CreateContentStack(parent);
            AddHeroCard(stack, content);
            AddStoryCard(stack, content);
            AddCreditsCard(stack, content);
        }

        private void AddHeroCard(RectTransform parent, KaelisAboutContent content)
        {
            RectTransform card = CreateMenuCard(parent, "AboutHeroCard", 0.40f);

            TMP_Text title = CreateLayoutText(card, "AboutProjectTitle", content.ProjectTitle, 42f, KaelisMenuStyle.TextPrimary, TextAlignmentOptions.Center, 8f, true, BrandFont(content.ProjectTitle));
            title.lineSpacing = 0f;
            title.fontStyle = FontStyles.UpperCase;

            TMP_Text description = CreateLayoutText(card, "AboutShortDescription", content.ShortDescription, 16.5f, KaelisMenuStyle.TextPrimary, TextAlignmentOptions.Center, 0f, true, BodyFont());
            description.lineSpacing = 4f;

            AddFieldPair(card, "Created by", content.Author, "AboutAuthor");
            AddFieldPair(card, "Music and Sound", content.Music, "AboutMusic");
            AddFieldPair(card, "YouTube", content.YouTube, "AboutYoutube");
            AddFieldPair(card, "Contact", content.PrimaryEmail + "\n" + content.MusicEmail, "AboutContact");
            AddFieldPair(card, "Copyright", content.Copyright, "AboutCopyright");
            AddFieldPair(card, "License", content.License, "AboutLicense");

            TMP_Text musicNote = CreateLayoutText(card, "AboutMusicRights", content.MusicRightsNote, 14.5f, KaelisMenuStyle.TextSecondary, TextAlignmentOptions.Center, 0f, true, BodyFont());
            musicNote.lineSpacing = 4f;
        }

        private void AddStoryCard(RectTransform parent, KaelisAboutContent content)
        {
            RectTransform card = CreateMenuCard(parent, "AboutStoryCard", 0.34f);
            AddCardHeading(card, "CREATION STORY", "AboutCreationStoryTitle");
            AddCardBody(card, "AboutCreationStoryText", content.CreationStory);
        }

        private void AddCreditsCard(RectTransform parent, KaelisAboutContent content)
        {
            RectTransform card = CreateMenuCard(parent, "AboutCreditsCard", 0.34f);
            AddCardHeading(card, "CREDITS & LICENSE", "AboutCreditsLicenseTitle");
            AddCardBody(card, "AboutCreditsLicenseText", content.CreditsAndLicense);
        }

        private RectTransform CreateContentStack(RectTransform parent)
        {
            RectTransform stack = KaelisMenuUiPrimitives.CreateRect("AboutContentStack", parent);

            VerticalLayoutGroup layout = stack.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 18, 22);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = stack.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return stack;
        }

        private RectTransform CreateMenuCard(RectTransform parent, string name, float backgroundAlpha)
        {
            RectTransform card = KaelisMenuUiPrimitives.CreateRect(name, parent);
            KaelisMenuUiPrimitives.AddImage(card, assets.SolidSprite, new Color(0.004f, 0.026f, 0.034f, backgroundAlpha), false);
            KaelisMenuUiPrimitives.AddFrame(card, new Color(0.58f, 0.94f, 1f, 0.22f), new Color(1f, 0.78f, 0.34f, 0.12f), 0.9f, assets.SolidSprite);
            KaelisMenuUiPrimitives.AddCornerCuts(card, new Color(1f, 0.76f, 0.36f, 0.18f), 22f, 1f, assets.SolidSprite);

            VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(34, 34, 28, 30);
            layout.spacing = 11f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = card.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return card;
        }

        private void AddCardHeading(RectTransform parent, string value, string name)
        {
            TMP_Text heading = CreateLayoutText(parent, name, value, 18f, KaelisMenuStyle.GoldSoft, TextAlignmentOptions.Left, 2f, false, HeadingFont());
            heading.fontStyle = FontStyles.UpperCase;
        }

        private void AddCardBody(RectTransform parent, string name, string value)
        {
            TMP_Text text = CreateLayoutText(parent, name, value, 15.2f, KaelisMenuStyle.TextSecondary, TextAlignmentOptions.TopLeft, 0f, true, BodyFont());
            text.lineSpacing = 5f;
        }

        private void AddFieldPair(RectTransform parent, string heading, string value, string objectPrefix)
        {
            RectTransform field = KaelisMenuUiPrimitives.CreateRect(objectPrefix + "Field", parent);
            VerticalLayoutGroup layout = field.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 3f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = field.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            TMP_Text label = CreateLayoutText(field, objectPrefix + "Label", heading, 12f, KaelisMenuStyle.GoldSoft, TextAlignmentOptions.Left, 1.8f, false, HeadingFont());
            label.fontStyle = FontStyles.UpperCase;

            TMP_Text body = CreateLayoutText(field, objectPrefix + "Value", value, 15.4f, KaelisMenuStyle.TextSecondary, TextAlignmentOptions.Left, 0f, true, BodyFont());
            body.lineSpacing = 3f;
        }

        private TMP_Text CreateLayoutText(RectTransform parent, string name, string value, float size, Color color, TextAlignmentOptions alignment, float characterSpacing, bool rawText, TMP_FontAsset font)
        {
            TMP_Text text = KaelisMenuUiPrimitives.CreateText(parent, name, value, size, color, alignment, font);
            text.characterSpacing = characterSpacing;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;
            text.lineSpacing = 3f;
            text.margin = new Vector4(0f, 0f, 0f, 2f);
            text.raycastTarget = false;

            if (rawText)
            {
                KaelisMenuLocalizationService.SetRawText(text, value);
            }
            else
            {
                KaelisMenuLocalizationService.SetText(text, value);
            }

            RectTransform rect = (RectTransform)text.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            ContentSizeFitter fitter = text.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return text;
        }

        private TMP_FontAsset BrandFont(string value)
        {
            return IsLatinOnly(value) && assets.CinzelSemiBold != null
                ? assets.CinzelSemiBold
                : HeadingFont();
        }

        private TMP_FontAsset HeadingFont()
        {
            return assets.InterSemiBold != null ? assets.InterSemiBold : assets.GetFont(KaelisMenuFontRole.Button);
        }

        private TMP_FontAsset BodyFont()
        {
            return assets.InterRegular != null ? assets.InterRegular : assets.GetFont(KaelisMenuFontRole.Status);
        }

        private static bool IsLatinOnly(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                if (character > 0x024F)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
