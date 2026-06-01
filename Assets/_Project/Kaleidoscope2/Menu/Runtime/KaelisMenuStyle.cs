using UnityEngine;

namespace Kaleidoscope2.Menu
{
    internal enum KaelisMenuButtonTone
    {
        Primary,
        Demo,
        Secondary,
        Exit
    }

    internal enum KaelisMenuFontRole
    {
        Logo,
        Subtitle,
        Button,
        PreviewLabel,
        Status
    }

    internal enum KaelisMenuIconKind
    {
        Diamond,
        Monitor,
        Layers,
        Optics,
        Star,
        Settings,
        Exit
    }

    internal readonly struct KaelisButtonPalette
    {
        public readonly Color Accent;
        public readonly Color AccentSoft;
        public readonly Color Dark;
        public readonly Color Glow;
        public readonly Color Text;

        public KaelisButtonPalette(Color accent, Color accentSoft, Color dark, Color glow, Color text)
        {
            Accent = accent;
            AccentSoft = accentSoft;
            Dark = dark;
            Glow = glow;
            Text = text;
        }
    }

    internal static class KaelisMenuStyle
    {
        public const float ReferenceWidth = 1920f;
        public const float ReferenceHeight = 1080f;
        public const float VisibilityFadeSeconds = 0.22f;
        public const float ActivationFlashDelay = 0.08f;

        public static readonly Vector2 SafeFrameMin = new Vector2(58f, 38f);
        public static readonly Vector2 SafeFrameMax = new Vector2(-58f, -42f);
        public static readonly Vector2 LeftPanelSize = new Vector2(500f, 0f);
        public static readonly Vector2 LeftPanelOffsetMin = new Vector2(0f, 86f);
        public static readonly Vector2 LeftPanelOffsetMax = new Vector2(500f, 0f);
        public static readonly Vector2 PreviewPanelOffsetMin = new Vector2(536f, 86f);
        public static readonly Vector2 PreviewPanelOffsetMax = Vector2.zero;
        public static readonly Rect PreviewUv = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
        public static readonly Rect LogoUv = new Rect(0f, 0f, 1f, 1f);

        public const float SectionHeaderHeight = 92f;
        public const float SectionHeaderSafeGap = 88f;
        public const float SectionSideInset = 34f;
        public const float SectionBottomInset = 34f;
        public static readonly Vector2 SectionViewportOffsetMin = new Vector2(SectionSideInset, SectionBottomInset);
        public static readonly Vector2 SectionViewportOffsetMax = new Vector2(-SectionSideInset, -(SectionHeaderHeight + SectionHeaderSafeGap));
        public static readonly RectOffset SectionContentPadding = new RectOffset(0, 0, 16, 0);

        public const float LogoBlockHeight = 276f;
        public const float LogoBlockTopInset = 22f;
        public static readonly Vector2 LogoFrameOffsetMin = new Vector2(28f, 10f);
        public static readonly Vector2 LogoFrameOffsetMax = new Vector2(-28f, -8f);
        public static readonly Vector2 LogoInnerOffsetMin = new Vector2(16f, 12f);
        public static readonly Vector2 LogoInnerOffsetMax = new Vector2(-16f, -12f);
        public const float LogoButtonStackTopInset = 334f;

        public const float PrimaryButtonHeight = 68f;
        public const float StandardButtonHeight = 60f;
        public const float ButtonCapWidth = 72f;
        public const float ButtonFillSpeed = 13f;
        public const float ButtonGlowSpeed = 15f;

        public static readonly Color BackgroundFallback = new Color(0.075f, 0.095f, 0.12f, 1f);
        public static readonly Color BackgroundTint = new Color(1f, 1f, 1f, 1f);
        public static readonly Color BackgroundScrim = new Color(0f, 0.006f, 0.012f, 0.18f);
        public static readonly Color PanelBase = new Color(0.014f, 0.035f, 0.048f, 0.62f);
        public static readonly Color PanelSheen = new Color(0.16f, 0.34f, 0.38f, 0.23f);
        public static readonly Color PanelInnerGlow = new Color(0.12f, 0.56f, 0.70f, 0.075f);
        public static readonly Color SectionPanelGlass = new Color(0.006f, 0.030f, 0.044f, 0.56f);
        public static readonly Color SectionPanelHaze = new Color(0.10f, 0.48f, 0.62f, 0.055f);
        public static readonly Color SectionPanelTopGlow = new Color(0.18f, 0.82f, 1f, 0.070f);
        public static readonly Color SectionFrameOuter = new Color(0.76f, 0.98f, 1f, 0.30f);
        public static readonly Color SectionFrameInner = new Color(0.20f, 0.82f, 0.92f, 0.18f);
        public static readonly Color SectionCorner = new Color(1f, 0.76f, 0.36f, 0.24f);
        public static readonly Color SectionHeaderDivider = new Color(0.58f, 0.94f, 1f, 0.22f);
        public static readonly Color IcyLine = new Color(0.80f, 0.97f, 1f, 0.48f);
        public static readonly Color CyanLine = new Color(0.20f, 0.82f, 0.92f, 0.34f);
        public static readonly Color Gold = new Color(1f, 0.68f, 0.22f, 1f);
        public static readonly Color GoldSoft = new Color(1f, 0.78f, 0.38f, 0.72f);
        public static readonly Color Cyan = new Color(0.18f, 0.78f, 0.88f, 1f);
        public static readonly Color Red = new Color(1f, 0.18f, 0.12f, 1f);
        public static readonly Color TextPrimary = new Color(1f, 0.965f, 0.90f, 1f);
        public static readonly Color TextSecondary = new Color(0.80f, 0.88f, 0.90f, 1f);
        public static readonly Color TextMuted = new Color(0.58f, 0.68f, 0.72f, 1f);

        public static KaelisButtonPalette GetButtonPalette(KaelisMenuButtonTone tone)
        {
            switch (tone)
            {
                case KaelisMenuButtonTone.Primary:
                    return new KaelisButtonPalette(
                        Gold,
                        new Color(1f, 0.83f, 0.28f, 0.9f),
                        new Color(0.15f, 0.09f, 0.02f, 0.94f),
                        new Color(1f, 0.62f, 0.11f, 1f),
                        TextPrimary);
                case KaelisMenuButtonTone.Demo:
                    return new KaelisButtonPalette(
                        new Color(0.18f, 0.88f, 0.90f, 1f),
                        new Color(0.48f, 1f, 0.94f, 0.84f),
                        new Color(0.012f, 0.13f, 0.14f, 0.94f),
                        new Color(0.18f, 0.90f, 0.94f, 1f),
                        TextPrimary);
                case KaelisMenuButtonTone.Exit:
                    return new KaelisButtonPalette(
                        Red,
                        new Color(1f, 0.32f, 0.26f, 0.82f),
                        new Color(0.14f, 0.015f, 0.02f, 0.94f),
                        new Color(1f, 0.11f, 0.08f, 1f),
                        TextPrimary);
                default:
                    return new KaelisButtonPalette(
                        Cyan,
                        new Color(0.58f, 0.94f, 1f, 0.76f),
                        new Color(0.012f, 0.08f, 0.12f, 0.94f),
                        new Color(0.16f, 0.76f, 0.92f, 1f),
                        TextPrimary);
            }
        }
    }
}
