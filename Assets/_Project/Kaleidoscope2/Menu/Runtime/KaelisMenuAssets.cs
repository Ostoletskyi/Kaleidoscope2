using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Kaleidoscope2.Menu
{
    internal readonly struct KaelisMenuButtonTextures
    {
        public readonly Texture2D Normal;
        public readonly Texture2D Hover;
        public readonly Texture2D Pressed;
        public readonly Texture2D Selected;

        public KaelisMenuButtonTextures(Texture2D normal, Texture2D hover, Texture2D pressed, Texture2D selected)
        {
            Normal = normal;
            Hover = hover;
            Pressed = pressed;
            Selected = selected;
        }

        public Texture2D BestNormal
        {
            get { return Normal != null ? Normal : BestLit; }
        }

        public Texture2D BestLit
        {
            get
            {
                if (Hover != null)
                {
                    return Hover;
                }

                if (Pressed != null)
                {
                    return Pressed;
                }

                return Selected;
            }
        }

        public Texture2D BestPressed
        {
            get
            {
                if (Pressed != null)
                {
                    return Pressed;
                }

                return BestLit;
            }
        }

        public Texture2D BestSelected
        {
            get
            {
                if (Selected != null)
                {
                    return Selected;
                }

                return BestLit;
            }
        }
    }

    internal sealed class KaelisMenuAssets
    {
        private const int ProceduralButtonWidth = 1024;
        private const int ProceduralButtonHeight = 160;

        private readonly List<Object> generatedObjects = new List<Object>();

        private KaelisMenuAssets()
        {
        }

        public Texture BackgroundTexture { get; private set; }
        public Texture PreviewTexture { get; private set; }
        public Texture FallbackConceptTexture { get; private set; }
        public Texture LogoTexture { get; private set; }
        public float LogoAspect { get; private set; }
        public Sprite SolidSprite { get; private set; }
        public TMP_FontAsset CinzelRegular { get; private set; }
        public TMP_FontAsset CinzelMedium { get; private set; }
        public TMP_FontAsset CinzelSemiBold { get; private set; }
        public TMP_FontAsset InterRegular { get; private set; }
        public TMP_FontAsset InterMedium { get; private set; }
        public TMP_FontAsset InterSemiBold { get; private set; }

        private KaelisMenuButtonTextures primaryButton;
        private KaelisMenuButtonTextures demoButton;
        private KaelisMenuButtonTextures secondaryButton;
        private KaelisMenuButtonTextures exitButton;

        public static KaelisMenuAssets Load(Texture background, Texture preview, Texture fallback)
        {
            KaelisMenuAssets assets = new KaelisMenuAssets();
            assets.FallbackConceptTexture = fallback;
            assets.BackgroundTexture = background != null ? background : fallback;
            assets.PreviewTexture = preview != null ? preview : (fallback != null ? fallback : background);
            assets.LogoTexture = Resources.Load<Texture2D>("Logos/Logo");
            assets.LogoAspect = assets.LogoTexture != null && assets.LogoTexture.height > 0
                ? (float)assets.LogoTexture.width / assets.LogoTexture.height
                : 1.777f;
            assets.CreateSolidSprite();
            assets.LoadFonts();
            assets.LoadButtons();
            assets.ReportValidation();
            return assets;
        }

        public TMP_FontAsset GetFont(KaelisMenuFontRole role)
        {
            switch (role)
            {
                case KaelisMenuFontRole.Logo:
                    return CinzelRegular != null ? CinzelRegular : CinzelMedium;
                case KaelisMenuFontRole.Subtitle:
                    return CinzelRegular != null ? CinzelRegular : CinzelMedium;
                case KaelisMenuFontRole.Button:
                    return CinzelSemiBold != null ? CinzelSemiBold : CinzelMedium;
                case KaelisMenuFontRole.Status:
                    return InterMedium != null ? InterMedium : InterRegular;
                default:
                    return CinzelMedium != null ? CinzelMedium : CinzelRegular;
            }
        }

        public KaelisMenuButtonTextures GetButtonTextures(KaelisMenuButtonTone tone)
        {
            switch (tone)
            {
                case KaelisMenuButtonTone.Primary:
                    return primaryButton;
                case KaelisMenuButtonTone.Demo:
                    return demoButton;
                case KaelisMenuButtonTone.Exit:
                    return exitButton;
                default:
                    return secondaryButton;
            }
        }

        public void Dispose()
        {
            for (int index = 0; index < generatedObjects.Count; index++)
            {
                DestroyObject(generatedObjects[index]);
            }

            generatedObjects.Clear();
        }

        private void CreateSolidSprite()
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.name = "KAELIS_Menu_Solid";
            texture.SetPixels(new[]
            {
                Color.white, Color.white,
                Color.white, Color.white
            });
            texture.Apply(false, true);
            generatedObjects.Add(texture);

            SolidSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            SolidSprite.name = "KAELIS_Menu_SolidSprite";
            generatedObjects.Add(SolidSprite);
        }

        private void LoadFonts()
        {
            CinzelRegular = Resources.Load<TMP_FontAsset>("MenuFonts/Kaelis_Cinzel_Regular");
            CinzelMedium = Resources.Load<TMP_FontAsset>("MenuFonts/Kaelis_Cinzel_Medium");
            CinzelSemiBold = Resources.Load<TMP_FontAsset>("MenuFonts/Kaelis_Cinzel_SemiBold");
            InterRegular = Resources.Load<TMP_FontAsset>("MenuFonts/Kaelis_Inter_18pt_Regular");
            InterMedium = Resources.Load<TMP_FontAsset>("MenuFonts/Kaelis_Inter_18pt_Medium");
            InterSemiBold = Resources.Load<TMP_FontAsset>("MenuFonts/Kaelis_Inter_18pt_SemiBold");
        }

        private void LoadButtons()
        {
            primaryButton = LoadButtonSet("primary", KaelisMenuButtonTone.Primary);
            demoButton = LoadButtonSet("demo", KaelisMenuButtonTone.Demo);
            secondaryButton = LoadButtonSet("secondary", KaelisMenuButtonTone.Secondary);
            exitButton = LoadButtonSet("exit", KaelisMenuButtonTone.Exit);
        }

        private KaelisMenuButtonTextures LoadButtonSet(string key, KaelisMenuButtonTone tone)
        {
            Texture2D normal = LoadTexture("GemButtons/button_" + key + "_normal");
            Texture2D hover = LoadTexture("GemButtons/button_" + key + "_hover");
            Texture2D pressed = LoadTexture("GemButtons/button_" + key + "_pressed");
            Texture2D selected = LoadTexture("GemButtons/button_" + key + "_selected");

            if (tone == KaelisMenuButtonTone.Demo && hover == null)
            {
                hover = LoadTexture("GemButtons/button_demo_active");
            }

            if (tone == KaelisMenuButtonTone.Demo && pressed == null)
            {
                pressed = hover;
            }

            if (normal == null)
            {
                normal = CreateProceduralButton(tone, 0f);
            }

            if (hover == null)
            {
                hover = CreateProceduralButton(tone, 0.5f);
            }

            if (pressed == null)
            {
                pressed = CreateProceduralButton(tone, 0.9f);
            }

            if (selected == null)
            {
                selected = tone == KaelisMenuButtonTone.Demo || tone == KaelisMenuButtonTone.Secondary
                    ? CreateProceduralButton(tone, 0.68f)
                    : pressed;
            }

            return new KaelisMenuButtonTextures(normal, hover, pressed, selected);
        }

        private static Texture2D LoadTexture(string path)
        {
            Texture2D texture = Resources.Load<Texture2D>(path);
            if (texture != null)
            {
                return texture;
            }

            Sprite sprite = Resources.Load<Sprite>(path);
            return sprite != null ? sprite.texture : null;
        }

        private Texture2D CreateProceduralButton(KaelisMenuButtonTone tone, float lit)
        {
            KaelisButtonPalette palette = KaelisMenuStyle.GetButtonPalette(tone);
            Texture2D texture = new Texture2D(ProceduralButtonWidth, ProceduralButtonHeight, TextureFormat.RGBA32, false);
            texture.name = "KAELIS_Procedural_" + tone + "_" + Mathf.RoundToInt(lit * 100f);

            Color clear = new Color(0f, 0f, 0f, 0f);
            int chamfer = Mathf.RoundToInt(ProceduralButtonHeight * 0.32f);

            for (int y = 0; y < ProceduralButtonHeight; y++)
            {
                float y01 = y / (ProceduralButtonHeight - 1f);
                float center = 1f - Mathf.Abs((y01 - 0.5f) * 2f);
                for (int x = 0; x < ProceduralButtonWidth; x++)
                {
                    float x01 = x / (ProceduralButtonWidth - 1f);
                    int leftLimit = Mathf.RoundToInt(Mathf.Lerp(chamfer, 0f, center));
                    int rightLimit = ProceduralButtonWidth - leftLimit - 1;
                    if (x < leftLimit || x > rightLimit)
                    {
                        texture.SetPixel(x, y, clear);
                        continue;
                    }

                    float edge = Mathf.Min(
                        Mathf.InverseLerp(leftLimit, leftLimit + 38f, x),
                        Mathf.InverseLerp(rightLimit, rightLimit - 38f, x));
                    float verticalEdge = Mathf.Min(Mathf.InverseLerp(0f, 20f, y), Mathf.InverseLerp(ProceduralButtonHeight - 1f, ProceduralButtonHeight - 21f, y));
                    float rim = 1f - Mathf.Clamp01(Mathf.Min(edge, verticalEdge));
                    float shimmer = Mathf.Sin((x01 * 38f) + (y01 * 17f)) * 0.5f + 0.5f;
                    float facets = Mathf.Sin((x01 * 11f) - (y01 * 23f)) * 0.5f + 0.5f;
                    float litAmount = Mathf.Clamp01(lit + (rim * 0.45f) + (shimmer * facets * 0.12f));
                    float shade = Mathf.Lerp(0.66f, 1.18f, center) + (facets * 0.10f);

                    Color body = Color.Lerp(palette.Dark, palette.Accent, litAmount * 0.72f);
                    body = Color.Lerp(body, palette.AccentSoft, litAmount * litAmount * 0.22f);
                    body.r *= shade;
                    body.g *= shade;
                    body.b *= shade;
                    body.a = Mathf.Lerp(0.84f, 1f, Mathf.Clamp01(center + lit));

                    if (rim > 0.62f)
                    {
                        body = Color.Lerp(body, palette.AccentSoft, Mathf.InverseLerp(0.62f, 1f, rim));
                    }

                    if (y > ProceduralButtonHeight * 0.73f || y < ProceduralButtonHeight * 0.18f)
                    {
                        body = Color.Lerp(body, palette.AccentSoft, 0.16f + lit * 0.20f);
                    }

                    texture.SetPixel(x, y, body);
                }
            }

            texture.Apply(false, true);
            generatedObjects.Add(texture);
            return texture;
        }

        private void ReportValidation()
        {
            if (BackgroundTexture == null || PreviewTexture == null)
            {
                Debug.LogWarning("[KAELIS Menu Assets] Background/preview texture references are not assigned. The menu will use procedural glass fallbacks.");
            }

            if (GetFont(KaelisMenuFontRole.Logo) == null || GetFont(KaelisMenuFontRole.Button) == null)
            {
                Debug.LogWarning("[KAELIS Menu Assets] Menu TMP font assets were not found. TMP fallback fonts will be used.");
            }

            if (LogoTexture == null)
            {
                Debug.LogWarning("[KAELIS Menu Assets] Logo texture was not found at Resources/Logos/Logo. The menu will use TMP fallback branding.");
            }
        }

        private static void DestroyObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target);
            }
            else
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}
