using System;
using System.IO;
using UnityEngine;

namespace Kaleidoscope2.ImageSource
{
    public sealed class ImageTextureLoader
    {
        public bool TryLoad(string filePath, out Texture2D texture, out string message)
        {
            texture = null;
            message = string.Empty;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                message = "Image path was empty.";
                return false;
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    message = "Image file does not exist: " + filePath;
                    return false;
                }

                string extension = Path.GetExtension(filePath);
                byte[] bytes = File.ReadAllBytes(filePath);
                if (bytes == null || bytes.Length == 0)
                {
                    message = "Image file was empty: " + filePath;
                    return false;
                }

                if (string.Equals(extension, ".bmp", StringComparison.OrdinalIgnoreCase))
                {
                    return TryLoadBmp(bytes, filePath, out texture, out message);
                }

                if (string.Equals(extension, ".tga", StringComparison.OrdinalIgnoreCase))
                {
                    return TryLoadTga(bytes, filePath, out texture, out message);
                }

                texture = CreateTexture(filePath);
                if (!texture.LoadImage(bytes, markNonReadable: false))
                {
                    DestroyTexture(texture);
                    texture = null;
                    message = "Failed to decode image: " + filePath;
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                message = "Failed to load image file: " + exception.Message;
                return false;
            }
        }

        private static bool TryLoadBmp(byte[] bytes, string filePath, out Texture2D texture, out string message)
        {
            texture = null;
            message = string.Empty;

            if (bytes.Length < 54 || bytes[0] != 'B' || bytes[1] != 'M')
            {
                message = "Unsupported BMP header: " + filePath;
                return false;
            }

            int pixelOffset = ReadInt32(bytes, 10);
            int headerSize = ReadInt32(bytes, 14);
            int width = ReadInt32(bytes, 18);
            int heightRaw = ReadInt32(bytes, 22);
            int planes = ReadInt16(bytes, 26);
            int bitsPerPixel = ReadInt16(bytes, 28);
            int compression = ReadInt32(bytes, 30);

            if (headerSize < 40 || planes != 1 || compression != 0 || (bitsPerPixel != 24 && bitsPerPixel != 32))
            {
                message = "Only uncompressed 24/32-bit BMP images are supported: " + filePath;
                return false;
            }

            int height = Mathf.Abs(heightRaw);
            if (width <= 0 || height <= 0)
            {
                message = "Invalid BMP dimensions: " + filePath;
                return false;
            }

            int bytesPerPixel = bitsPerPixel / 8;
            int rowStride = ((width * bytesPerPixel + 3) / 4) * 4;
            if (pixelOffset < 0 || pixelOffset + rowStride * height > bytes.Length)
            {
                message = "BMP pixel data is truncated: " + filePath;
                return false;
            }

            texture = CreateTexture(filePath, width, height);
            Color32[] pixels = new Color32[width * height];
            bool bottomUp = heightRaw > 0;

            for (int y = 0; y < height; y++)
            {
                int sourceY = bottomUp ? height - 1 - y : y;
                int rowOffset = pixelOffset + sourceY * rowStride;
                for (int x = 0; x < width; x++)
                {
                    int pixelIndex = rowOffset + x * bytesPerPixel;
                    byte b = bytes[pixelIndex];
                    byte g = bytes[pixelIndex + 1];
                    byte r = bytes[pixelIndex + 2];
                    byte a = bitsPerPixel == 32 ? bytes[pixelIndex + 3] : (byte)255;
                    pixels[y * width + x] = new Color32(r, g, b, a);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return true;
        }

        private static bool TryLoadTga(byte[] bytes, string filePath, out Texture2D texture, out string message)
        {
            texture = null;
            message = string.Empty;

            if (bytes.Length < 18)
            {
                message = "Unsupported TGA header: " + filePath;
                return false;
            }

            int idLength = bytes[0];
            int colorMapType = bytes[1];
            int imageType = bytes[2];
            int width = ReadUInt16(bytes, 12);
            int height = ReadUInt16(bytes, 14);
            int bitsPerPixel = bytes[16];
            int descriptor = bytes[17];

            if (colorMapType != 0 || (imageType != 2 && imageType != 10) || (bitsPerPixel != 24 && bitsPerPixel != 32))
            {
                message = "Only true-color 24/32-bit TGA images are supported: " + filePath;
                return false;
            }

            if (width <= 0 || height <= 0)
            {
                message = "Invalid TGA dimensions: " + filePath;
                return false;
            }

            int bytesPerPixel = bitsPerPixel / 8;
            int offset = 18 + idLength;
            if (offset >= bytes.Length)
            {
                message = "TGA pixel data is missing: " + filePath;
                return false;
            }

            texture = CreateTexture(filePath, width, height);
            Color32[] pixels = new Color32[width * height];
            bool originTop = (descriptor & 0x20) != 0;

            if (imageType == 2)
            {
                for (int sourceIndex = 0; sourceIndex < width * height; sourceIndex++)
                {
                    if (offset + bytesPerPixel > bytes.Length)
                    {
                        DestroyTexture(texture);
                        texture = null;
                        message = "TGA pixel data is truncated: " + filePath;
                        return false;
                    }

                    WriteTgaPixel(bytes, ref offset, bytesPerPixel, pixels, width, height, sourceIndex, originTop);
                }
            }
            else
            {
                int sourceIndex = 0;
                while (sourceIndex < width * height)
                {
                    if (offset >= bytes.Length)
                    {
                        DestroyTexture(texture);
                        texture = null;
                        message = "TGA RLE data is truncated: " + filePath;
                        return false;
                    }

                    byte packet = bytes[offset++];
                    int count = (packet & 0x7F) + 1;
                    bool runLength = (packet & 0x80) != 0;

                    if (runLength)
                    {
                        if (offset + bytesPerPixel > bytes.Length)
                        {
                            DestroyTexture(texture);
                            texture = null;
                            message = "TGA RLE pixel is truncated: " + filePath;
                            return false;
                        }

                        Color32 color = ReadTgaColor(bytes, offset, bytesPerPixel);
                        offset += bytesPerPixel;
                        for (int index = 0; index < count && sourceIndex < width * height; index++)
                        {
                            WriteTgaColor(color, pixels, width, height, sourceIndex++, originTop);
                        }
                    }
                    else
                    {
                        for (int index = 0; index < count && sourceIndex < width * height; index++)
                        {
                            if (offset + bytesPerPixel > bytes.Length)
                            {
                                DestroyTexture(texture);
                                texture = null;
                                message = "TGA raw packet is truncated: " + filePath;
                                return false;
                            }

                            WriteTgaPixel(bytes, ref offset, bytesPerPixel, pixels, width, height, sourceIndex++, originTop);
                        }
                    }
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return true;
        }

        private static void WriteTgaPixel(byte[] bytes, ref int offset, int bytesPerPixel, Color32[] pixels, int width, int height, int sourceIndex, bool originTop)
        {
            Color32 color = ReadTgaColor(bytes, offset, bytesPerPixel);
            offset += bytesPerPixel;
            WriteTgaColor(color, pixels, width, height, sourceIndex, originTop);
        }

        private static void WriteTgaColor(Color32 color, Color32[] pixels, int width, int height, int sourceIndex, bool originTop)
        {
            int x = sourceIndex % width;
            int sourceY = sourceIndex / width;
            int y = originTop ? height - 1 - sourceY : sourceY;
            pixels[y * width + x] = color;
        }

        private static Color32 ReadTgaColor(byte[] bytes, int offset, int bytesPerPixel)
        {
            byte b = bytes[offset];
            byte g = bytes[offset + 1];
            byte r = bytes[offset + 2];
            byte a = bytesPerPixel == 4 ? bytes[offset + 3] : (byte)255;
            return new Color32(r, g, b, a);
        }

        private static Texture2D CreateTexture(string filePath, int width = 2, int height = 2)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Kaleidoscope2_Image_" + Path.GetFileNameWithoutExtension(filePath),
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            return texture;
        }

        private static short ReadInt16(byte[] bytes, int offset)
        {
            return BitConverter.ToInt16(bytes, offset);
        }

        private static int ReadUInt16(byte[] bytes, int offset)
        {
            return BitConverter.ToUInt16(bytes, offset);
        }

        private static int ReadInt32(byte[] bytes, int offset)
        {
            return BitConverter.ToInt32(bytes, offset);
        }

        public static void DestroyTexture(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(texture);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
