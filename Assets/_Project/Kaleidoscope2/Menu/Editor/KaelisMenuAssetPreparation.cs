using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Kaleidoscope2.Menu.Editor
{
    public static class KaelisMenuAssetPreparation
    {
        private const string ResourceRoot = "Assets/_Project/Kaleidoscope2/Menu/UI/Resources";
        private const string GemButtonsRoot = ResourceRoot + "/GemButtons";
        private const string MenuFontsRoot = ResourceRoot + "/MenuFonts";
        private const string LogoResourcesRoot = ResourceRoot + "/Logos";
        private const string ButtonSheetPath = "Assets/_Project/Kaleidoscope2/Menu/UI/Buttons/buttons.png";
        private const string BackgroundPath = "Assets/_Project/Kaleidoscope2/Menu/UI/Backgrounds/Background.png";
        private const string LogoPath = "Assets/_Project/Kaleidoscope2/Menu/UI/Logos/Logo.png";
        private const string LogoResourcePath = LogoResourcesRoot + "/Logo.png";

        private const int OutputWidth = 768;
        private const int OutputHeight = 120;

        private static readonly Vector4 ButtonSpriteBorder = new Vector4(118f, 42f, 118f, 42f);

        private readonly struct ButtonGrade
        {
            public readonly string Key;
            public readonly int RowIndex;
            public readonly Color Tint;

            public ButtonGrade(string key, int rowIndex, Color tint)
            {
                Key = key;
                RowIndex = rowIndex;
                Tint = tint;
            }
        }

        public static void Run()
        {
            Directory.CreateDirectory(GemButtonsRoot);
            Directory.CreateDirectory(MenuFontsRoot);
            Directory.CreateDirectory(LogoResourcesRoot);

            PrepareButtonSheetImporter(ButtonSheetPath);
            PrepareUiTextureImporter(BackgroundPath, false);
            PrepareUiTextureImporter(LogoPath, true);
            CopyLogoResource();

            Texture2D sheet = LoadReadableTexture(ButtonSheetPath);
            List<RectInt> rows = FindButtonRows(sheet);
            Require(rows.Count >= 6, "buttons.png must contain at least six visible gemstone rows.");

            WriteButtonSet(sheet, rows, new ButtonGrade("primary", 0, new Color(1f, 0.68f, 0.18f, 1f)));
            WriteButtonSet(sheet, rows, new ButtonGrade("demo", 1, new Color(0.20f, 0.90f, 0.92f, 1f)));
            WriteButtonSet(sheet, rows, new ButtonGrade("secondary", 2, new Color(0.18f, 0.72f, 1f, 1f)));
            WriteButtonSet(sheet, rows, new ButtonGrade("exit", rows.Count - 1, new Color(1f, 0.18f, 0.12f, 1f)));

            CopyButtonAsset("button_demo_selected.png", "button_demo_active.png");

            CreateFontAsset("Kaelis_Cinzel_Regular", "Assets/_Project/Kaleidoscope2/Menu/UI/Fonts/Cinzel/static/Cinzel-Regular.ttf");
            CreateFontAsset("Kaelis_Cinzel_Medium", "Assets/_Project/Kaleidoscope2/Menu/UI/Fonts/Cinzel/static/Cinzel-Medium.ttf");
            CreateFontAsset("Kaelis_Cinzel_SemiBold", "Assets/_Project/Kaleidoscope2/Menu/UI/Fonts/Cinzel/static/Cinzel-SemiBold.ttf");
            CreateFontAsset("Kaelis_Inter_18pt_Regular", "Assets/_Project/Kaleidoscope2/Menu/UI/Fonts/Inter/static/Inter_18pt-Regular.ttf");
            CreateFontAsset("Kaelis_Inter_18pt_Medium", "Assets/_Project/Kaleidoscope2/Menu/UI/Fonts/Inter/static/Inter_18pt-Medium.ttf");
            CreateFontAsset("Kaelis_Inter_18pt_SemiBold", "Assets/_Project/Kaleidoscope2/Menu/UI/Fonts/Inter/static/Inter_18pt-SemiBold.ttf");

            Object.DestroyImmediate(sheet);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[KAELIS Menu Assets] Gemstone buttons rebuilt from UI/Buttons/buttons.png. Rows detected: " + rows.Count + ".");
        }

        private static void WriteButtonSet(Texture2D sheet, List<RectInt> rows, ButtonGrade grade)
        {
            RectInt source = rows[Mathf.Clamp(grade.RowIndex, 0, rows.Count - 1)];
            WriteButton(sheet, source, "button_" + grade.Key + "_normal", grade.Tint, 0f);
            WriteButton(sheet, source, "button_" + grade.Key + "_hover", grade.Tint, 0.34f);
            WriteButton(sheet, source, "button_" + grade.Key + "_pressed", grade.Tint, 0.66f);
            WriteButton(sheet, source, "button_" + grade.Key + "_selected", grade.Tint, 0.46f);
        }

        private static void WriteButton(Texture2D sheet, RectInt source, string assetName, Color tint, float lit)
        {
            Texture2D output = BuildButtonTexture(sheet, source, tint, lit);
            string path = GemButtonsRoot + "/" + assetName + ".png";
            File.WriteAllBytes(path, output.EncodeToPNG());
            Object.DestroyImmediate(output);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            ConfigureButtonImporter(path);
        }

        private static Texture2D LoadReadableTexture(string path)
        {
            Require(File.Exists(path), path + " missing.");
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.name = Path.GetFileNameWithoutExtension(path);
            texture.LoadImage(File.ReadAllBytes(path), false);
            return texture;
        }

        private static List<RectInt> FindButtonRows(Texture2D sheet)
        {
            List<int> occupiedRows = new List<int>();
            for (int y = 0; y < sheet.height; y++)
            {
                int occupied = 0;
                for (int x = 0; x < sheet.width; x += 2)
                {
                    if (IsButtonPixel(sheet.GetPixel(x, y)))
                    {
                        occupied++;
                    }
                }

                if (occupied > sheet.width * 0.16f)
                {
                    occupiedRows.Add(y);
                }
            }

            List<RectInt> rows = new List<RectInt>();
            if (occupiedRows.Count == 0)
            {
                return rows;
            }

            int start = occupiedRows[0];
            int previous = occupiedRows[0];
            for (int index = 1; index < occupiedRows.Count; index++)
            {
                int y = occupiedRows[index];
                if (y <= previous + 2)
                {
                    previous = y;
                    continue;
                }

                rows.Add(FindRowBounds(sheet, start, previous));
                start = y;
                previous = y;
            }

            rows.Add(FindRowBounds(sheet, start, previous));
            rows.RemoveAll(rect => rect.width < sheet.width * 0.50f || rect.height < 24);
            rows.Sort((left, right) => right.y.CompareTo(left.y));
            return rows;
        }

        private static RectInt FindRowBounds(Texture2D sheet, int yMin, int yMax)
        {
            int minX = sheet.width;
            int maxX = 0;
            int minY = sheet.height;
            int maxY = 0;

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = 0; x < sheet.width; x++)
                {
                    if (!IsButtonPixel(sheet.GetPixel(x, y)))
                    {
                        continue;
                    }

                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y);
                    maxY = Mathf.Max(maxY, y);
                }
            }

            minX = Mathf.Max(0, minX - 4);
            minY = Mathf.Max(0, minY - 4);
            maxX = Mathf.Min(sheet.width - 1, maxX + 4);
            maxY = Mathf.Min(sheet.height - 1, maxY + 4);
            return new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        private static Texture2D BuildButtonTexture(Texture2D sheet, RectInt source, Color tint, float lit)
        {
            Texture2D output = new Texture2D(OutputWidth, OutputHeight, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);

            for (int y = 0; y < OutputHeight; y++)
            {
                float y01 = y / (OutputHeight - 1f);
                for (int x = 0; x < OutputWidth; x++)
                {
                    float x01 = x / (OutputWidth - 1f);
                    float sourceX = (source.x + (x01 * (source.width - 1))) / sheet.width;
                    float sourceY = (source.y + (y01 * (source.height - 1))) / sheet.height;
                    Color color = sheet.GetPixelBilinear(sourceX, sourceY);

                    if (IsTransparentPixel(color))
                    {
                        output.SetPixel(x, y, clear);
                        continue;
                    }

                    float luminance = (color.r * 0.299f) + (color.g * 0.587f) + (color.b * 0.114f);
                    float centerX = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.18f, 0.30f, x01)) * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.74f, 0.88f, x01)));
                    float centerY = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.23f, 0.40f, y01)) * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.60f, 0.80f, y01)));
                    float readability = centerX * centerY;
                    float edgeLift = Mathf.Max(
                        1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.00f, 0.16f, x01)),
                        Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.84f, 1.00f, x01)));
                    float topGlow = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.70f, 0.93f, y01));

                    Color tinted = Color.Lerp(color, new Color(tint.r * Mathf.Max(0.36f, luminance), tint.g * Mathf.Max(0.36f, luminance), tint.b * Mathf.Max(0.36f, luminance), color.a), 0.10f + (lit * 0.18f));
                    tinted.r *= 1f + (lit * 0.16f) + (edgeLift * 0.06f) + (topGlow * lit * 0.10f);
                    tinted.g *= 1f + (lit * 0.16f) + (edgeLift * 0.06f) + (topGlow * lit * 0.10f);
                    tinted.b *= 1f + (lit * 0.16f) + (edgeLift * 0.06f) + (topGlow * lit * 0.10f);

                    float darken = Mathf.Lerp(1f, 0.56f + (lit * 0.10f), readability * 0.42f);
                    tinted.r *= darken;
                    tinted.g *= darken;
                    tinted.b *= darken;
                    tinted.a = color.a;

                    output.SetPixel(x, y, tinted);
                }
            }

            output.Apply(false, false);
            return output;
        }

        private static bool IsButtonPixel(Color color)
        {
            return !IsTransparentPixel(color);
        }

        private static bool IsTransparentPixel(Color color)
        {
            if (color.a <= 0.04f)
            {
                return true;
            }

            float max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
            float min = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
            float saturation = max - min;
            if (max > 0.94f && saturation < 0.08f)
            {
                return true;
            }

            return color.g > 0.42f && color.g > color.r * 1.18f && color.g > color.b * 1.18f;
        }

        private static void CopyButtonAsset(string fromName, string toName)
        {
            string fromPath = GemButtonsRoot + "/" + fromName;
            string toPath = GemButtonsRoot + "/" + toName;
            if (!File.Exists(fromPath))
            {
                return;
            }

            File.Copy(fromPath, toPath, true);
            AssetDatabase.ImportAsset(toPath, ImportAssetOptions.ForceUpdate);
            ConfigureButtonImporter(toPath);
        }

        private static void CopyLogoResource()
        {
            Require(File.Exists(LogoPath), "Logo.png missing.");
            File.Copy(LogoPath, LogoResourcePath, true);
            AssetDatabase.ImportAsset(LogoResourcePath, ImportAssetOptions.ForceUpdate);
            PrepareUiTextureImporter(LogoResourcePath, true);
        }

        private static void ConfigureButtonImporter(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 100f;
            importer.compressionQuality = 100;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.spriteBorder = ButtonSpriteBorder;
            importer.SaveAndReimport();
        }

        private static void PrepareButtonSheetImporter(string path)
        {
            PrepareDefaultTextureImporter(path, true);
        }

        private static void PrepareUiTextureImporter(string path, bool alpha)
        {
            PrepareDefaultTextureImporter(path, alpha);
        }

        private static void PrepareDefaultTextureImporter(string path, bool alpha)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = alpha;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.compressionQuality = 100;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }

        private static void CreateFontAsset(string assetName, string fontPath)
        {
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            Require(sourceFont != null, "Font missing: " + fontPath);

            string assetPath = MenuFontsRoot + "/" + assetName + ".asset";
            TMP_FontAsset oldAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (oldAsset != null)
            {
                oldAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                EditorUtility.SetDirty(oldAsset);
                return;
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
            Require(fontAsset != null, "Could not create TMP font asset from: " + fontPath);
            fontAsset.name = assetName;
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            AssetDatabase.CreateAsset(fontAsset, assetPath);

            if (fontAsset.atlasTextures != null)
            {
                for (int index = 0; index < fontAsset.atlasTextures.Length; index++)
                {
                    Texture2D atlasTexture = fontAsset.atlasTextures[index];
                    if (atlasTexture == null)
                    {
                        continue;
                    }

                    atlasTexture.name = assetName + "_Atlas";
                    AssetDatabase.AddObjectToAsset(atlasTexture, fontAsset);
                }
            }

            if (fontAsset.material != null)
            {
                fontAsset.material.name = assetName + "_Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            fontAsset.ReadFontAssetDefinition();
            EditorUtility.SetDirty(fontAsset);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new System.InvalidOperationException("[KAELIS Menu Assets] " + message);
            }
        }
    }
}
