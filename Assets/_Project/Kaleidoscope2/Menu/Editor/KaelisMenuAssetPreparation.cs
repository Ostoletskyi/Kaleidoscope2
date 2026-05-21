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
        private const string ButtonsBasePath = "Assets/_Project/Kaleidoscope2/Menu/UI/Materials/buttons.png";
        private const string ButtonsGlowPath = "Assets/_Project/Kaleidoscope2/Menu/UI/Materials/buttons2.png";

        private const int DemoRow = 1;
        private const int SecondaryRow = 2;
        private const int ExitRow = 6;

        public static void Run()
        {
            Directory.CreateDirectory(GemButtonsRoot);
            Directory.CreateDirectory(MenuFontsRoot);

            Texture2D baseSheet = LoadReadableTexture(ButtonsBasePath);
            Texture2D glowSheet = LoadReadableTexture(ButtonsGlowPath);
            List<RectInt> baseRows = FindButtonRows(baseSheet);
            List<RectInt> glowRows = FindButtonRows(glowSheet);

            Require(baseRows.Count > ExitRow, "buttons.png must contain at least seven button rows.");
            Require(glowRows.Count > ExitRow, "buttons2.png must contain at least seven button rows.");

            WriteButton(baseSheet, baseRows[SecondaryRow], "button_primary_normal", new Color(1f, 0.58f, 0.1f), 0.62f);
            WriteButton(glowSheet, glowRows[SecondaryRow], "button_primary_hover", new Color(1f, 0.72f, 0.08f), 0.88f);
            WriteButton(glowSheet, glowRows[SecondaryRow], "button_primary_pressed", new Color(1f, 0.8f, 0.12f), 0.96f);

            WriteButton(baseSheet, baseRows[SecondaryRow], "button_secondary_normal");
            WriteButton(glowSheet, glowRows[SecondaryRow], "button_secondary_hover", new Color(1f, 0.7f, 0.12f), 0.78f);
            WriteButton(glowSheet, glowRows[SecondaryRow], "button_secondary_pressed", new Color(1f, 0.76f, 0.14f), 0.92f);
            WriteButton(glowSheet, glowRows[SecondaryRow], "button_secondary_selected");

            WriteButton(baseSheet, baseRows[DemoRow], "button_demo_normal");
            WriteButton(glowSheet, glowRows[DemoRow], "button_demo_active");

            WriteButton(baseSheet, baseRows[ExitRow], "button_exit_normal");
            WriteButton(glowSheet, glowRows[ExitRow], "button_exit_hover");
            WriteButton(glowSheet, glowRows[ExitRow], "button_exit_pressed");

            CreateFontAsset("Kaelis_Cinzel_Regular", "Assets/_Project/Kaleidoscope2/Menu/UI/Fonts/Cinzel/static/Cinzel-Regular.ttf");
            CreateFontAsset("Kaelis_Cinzel_Medium", "Assets/_Project/Kaleidoscope2/Menu/UI/Fonts/Cinzel/static/Cinzel-Medium.ttf");
            CreateFontAsset("Kaelis_Cinzel_SemiBold", "Assets/_Project/Kaleidoscope2/Menu/UI/Fonts/Cinzel/static/Cinzel-SemiBold.ttf");
            CreateFontAsset("Kaelis_Inter_18pt_Regular", "Assets/_Project/Kaleidoscope2/Menu/UI/Fonts/Inter/static/Inter_18pt-Regular.ttf");
            CreateFontAsset("Kaelis_Inter_18pt_Medium", "Assets/_Project/Kaleidoscope2/Menu/UI/Fonts/Inter/static/Inter_18pt-Medium.ttf");
            CreateFontAsset("Kaelis_Inter_18pt_SemiBold", "Assets/_Project/Kaleidoscope2/Menu/UI/Fonts/Inter/static/Inter_18pt-SemiBold.ttf");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[KAELIS Menu Assets] Gemstone buttons and menu TMP fonts prepared.");
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
            List<int> rows = new List<int>();
            for (int y = 0; y < sheet.height; y++)
            {
                int count = 0;
                for (int x = 0; x < sheet.width; x += 2)
                {
                    if (IsButtonPixel(sheet.GetPixel(x, y)))
                    {
                        count++;
                    }
                }

                if (count > 12)
                {
                    rows.Add(y);
                }
            }

            List<RectInt> rects = new List<RectInt>();
            if (rows.Count == 0)
            {
                return rects;
            }

            int start = rows[0];
            int previous = rows[0];
            for (int index = 1; index < rows.Count; index++)
            {
                int y = rows[index];
                if (y <= previous + 2)
                {
                    previous = y;
                    continue;
                }

                rects.Add(FindRowBounds(sheet, start, previous));
                start = y;
                previous = y;
            }

            rects.Add(FindRowBounds(sheet, start, previous));
            rects.Sort((left, right) => right.y.CompareTo(left.y));
            return rects;
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

            return new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        private static bool IsButtonPixel(Color color)
        {
            return color.a > 0.06f && (color.r < 0.965f || color.g < 0.965f || color.b < 0.965f);
        }

        private static void WriteButton(Texture2D sheet, RectInt rect, string assetName)
        {
            WriteButton(sheet, rect, assetName, Color.white, 0f);
        }

        private static void WriteButton(Texture2D sheet, RectInt rect, string assetName, Color tint, float tintStrength)
        {
            Texture2D clean = BuildCleanButton(sheet, rect);
            if (tintStrength > 0f)
            {
                ApplyGemTint(clean, tint, tintStrength);
            }

            string path = GemButtonsRoot + "/" + assetName + ".png";
            File.WriteAllBytes(path, clean.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spritePixelsPerUnit = 100f;
                importer.spriteBorder = new Vector4(178f, 42f, 178f, 42f);
                importer.SaveAndReimport();
            }
        }

        private static Texture2D BuildCleanButton(Texture2D sheet, RectInt rect)
        {
            int width = rect.width;
            int height = rect.height;
            int capWidth = Mathf.Clamp(Mathf.RoundToInt(width * 0.22f), 150, 190);
            int centerWidth = Mathf.Clamp(Mathf.RoundToInt(width * 0.12f), 80, width - (capWidth * 2));
            int centerStart = FindCleanPatchStart(sheet, rect, capWidth, centerWidth);

            Texture2D output = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Color[] pixels = new Color[width * height];
            for (int index = 0; index < pixels.Length; index++)
            {
                pixels[index] = clear;
            }

            output.SetPixels(pixels);

            CopyRegion(sheet, rect, output, 0, 0, 0, 0, capWidth, height);
            CopyRegion(sheet, rect, output, width - capWidth, 0, width - capWidth, 0, capWidth, height);

            for (int x = capWidth; x < width - capWidth; x++)
            {
                int sourceX = centerStart + Mathf.RoundToInt(Mathf.InverseLerp(capWidth, width - capWidth - 1, x) * (centerWidth - 1));
                for (int y = 0; y < height; y++)
                {
                    output.SetPixel(x, y, CleanPixel(sheet.GetPixel(rect.x + sourceX, rect.y + y)));
                }
            }

            // Remove baked sheet icons/text while preserving faceted material and readable edges.
            CoverWithMaterialPatch(sheet, rect, output, 52, Mathf.RoundToInt(height * 0.2f), 142, Mathf.RoundToInt(height * 0.62f), centerStart, centerWidth);
            CoverWithMaterialPatch(sheet, rect, output, Mathf.RoundToInt(width * 0.18f), Mathf.RoundToInt(height * 0.12f), Mathf.RoundToInt(width * 0.64f), Mathf.RoundToInt(height * 0.76f), centerStart, centerWidth);
            AddCenterReadabilityBand(output, capWidth, Mathf.RoundToInt(height * 0.18f), width - capWidth * 2, Mathf.RoundToInt(height * 0.64f));
            AddDarkPatch(output, 48, Mathf.RoundToInt(height * 0.22f), 150, Mathf.RoundToInt(height * 0.58f), 0.14f);

            output.Apply(false, false);
            return output;
        }

        private static int FindCleanPatchStart(Texture2D sheet, RectInt rect, int capWidth, int patchWidth)
        {
            int searchStart = Mathf.Max(capWidth + 24, Mathf.RoundToInt(rect.width * 0.42f));
            int searchEnd = Mathf.Min(rect.width - capWidth - patchWidth - 12, Mathf.RoundToInt(rect.width * 0.78f));
            int bestX = searchStart;
            float bestScore = float.MaxValue;

            for (int x = searchStart; x <= searchEnd; x += 4)
            {
                float score = 0f;
                int samples = 0;
                for (int px = 0; px < patchWidth; px += 4)
                {
                    for (int py = Mathf.RoundToInt(rect.height * 0.2f); py < Mathf.RoundToInt(rect.height * 0.8f); py += 4)
                    {
                        Color color = sheet.GetPixel(rect.x + x + px, rect.y + py);
                        if (!IsButtonPixel(color))
                        {
                            continue;
                        }

                        float luminance = (color.r * 0.299f) + (color.g * 0.587f) + (color.b * 0.114f);
                        score += luminance > 0.64f ? 5f : luminance;
                        samples++;
                    }
                }

                if (samples == 0)
                {
                    continue;
                }

                score /= samples;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestX = x;
                }
            }

            return Mathf.Clamp(bestX, capWidth + 8, rect.width - capWidth - patchWidth);
        }

        private static void CopyRegion(Texture2D source, RectInt sourceRect, Texture2D target, int targetX, int targetY, int sourceX, int sourceY, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    target.SetPixel(targetX + x, targetY + y, CleanPixel(source.GetPixel(sourceRect.x + sourceX + x, sourceRect.y + sourceY + y)));
                }
            }
        }

        private static void CoverWithMaterialPatch(Texture2D source, RectInt sourceRect, Texture2D target, int targetX, int targetY, int width, int height, int patchStartX, int patchWidth)
        {
            int maxX = Mathf.Min(target.width, targetX + width);
            int maxY = Mathf.Min(target.height, targetY + height);
            for (int y = Mathf.Max(0, targetY); y < maxY; y++)
            {
                for (int x = Mathf.Max(0, targetX); x < maxX; x++)
                {
                    int sourceX = patchStartX + Mathf.RoundToInt(Mathf.InverseLerp(targetX, maxX - 1, x) * (patchWidth - 1));
                    Color patch = CleanPixel(source.GetPixel(sourceRect.x + sourceX, sourceRect.y + y));
                    Color existing = target.GetPixel(x, y);
                    target.SetPixel(x, y, Color.Lerp(existing, patch, 0.98f));
                }
            }
        }

        private static void AddCenterReadabilityBand(Texture2D target, int x, int y, int width, int height)
        {
            int maxX = Mathf.Min(target.width, x + width);
            int maxY = Mathf.Min(target.height, y + height);
            for (int py = Mathf.Max(0, y); py < maxY; py++)
            {
                for (int px = Mathf.Max(0, x); px < maxX; px++)
                {
                    Color existing = target.GetPixel(px, py);
                    if (existing.a <= 0.01f)
                    {
                        continue;
                    }

                    float edge = Mathf.InverseLerp(0f, height * 0.5f, Mathf.Min(py - y, maxY - py));
                    float strength = Mathf.Lerp(0.08f, 0.34f, edge);
                    Color shaded = new Color(existing.r * (1f - strength), existing.g * (1f - strength), existing.b * (1f - strength), existing.a);
                    target.SetPixel(px, py, shaded);
                }
            }
        }

        private static void AddDarkPatch(Texture2D target, int x, int y, int width, int height, float strength)
        {
            int maxX = Mathf.Min(target.width, x + width);
            int maxY = Mathf.Min(target.height, y + height);
            for (int py = Mathf.Max(0, y); py < maxY; py++)
            {
                for (int px = Mathf.Max(0, x); px < maxX; px++)
                {
                    Color existing = target.GetPixel(px, py);
                    if (existing.a <= 0.01f)
                    {
                        continue;
                    }

                    target.SetPixel(px, py, new Color(existing.r * (1f - strength), existing.g * (1f - strength), existing.b * (1f - strength), existing.a));
                }
            }
        }

        private static void ApplyGemTint(Texture2D texture, Color tint, float strength)
        {
            Color[] pixels = texture.GetPixels();
            for (int index = 0; index < pixels.Length; index++)
            {
                Color color = pixels[index];
                if (color.a <= 0.01f)
                {
                    continue;
                }

                float luminance = (color.r * 0.299f) + (color.g * 0.587f) + (color.b * 0.114f);
                float highlight = Mathf.Clamp01(luminance * 1.35f);
                Color tinted = new Color(tint.r * highlight, tint.g * highlight, tint.b * highlight, color.a);
                pixels[index] = Color.Lerp(color, tinted, strength);
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
        }

        private static Color CleanPixel(Color color)
        {
            if (!IsButtonPixel(color))
            {
                return new Color(0f, 0f, 0f, 0f);
            }

            return color;
        }

        private static void CreateFontAsset(string assetName, string fontPath)
        {
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            Require(sourceFont != null, "Font missing: " + fontPath);

            string assetPath = MenuFontsRoot + "/" + assetName + ".asset";
            TMP_FontAsset oldAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (oldAsset != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
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
