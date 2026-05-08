using System;
using System.Collections.Generic;
using System.IO;
using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Source
{
    [DisallowMultipleComponent]
    public sealed class SourceModule : KaleidoscopeModuleBase, IKaleidoscopeSourceProvider
    {
        private const int FallbackTextureSize = 256;
        private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg" };

        private Texture2D fallbackTexture;
        private Texture2D loadedTexture;
        private readonly List<string> folderImages = new List<string>(128);
        private int folderImageIndex;

        public override string ModuleId
        {
            get { return "Source"; }
        }

        public Texture SourceTexture
        {
            get { return GetActiveTexture(); }
        }

        public KaleidoscopeSourceMode SourceMode
        {
            get { return CurrentSourceMode; }
        }

        public KaleidoscopeSourceMode CurrentSourceMode
        {
            get { return State != null ? State.ActiveSourceMode : KaleidoscopeSourceMode.None; }
        }

        public Texture CurrentSourceTexture
        {
            get { return GetActiveTexture(); }
        }

        protected override void OnInitialized()
        {
            EnsureFallbackTexture();
            TrySyncFromState();
        }

        private void OnDestroy()
        {
            DestroyTexture(ref loadedTexture);
            DestroyTexture(ref fallbackTexture);
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            if (command == null)
            {
                return false;
            }

            return command.Type == KaleidoscopeCommandType.SetSourceMode
                || command.Type == KaleidoscopeCommandType.SetImageFilePath
                || command.Type == KaleidoscopeCommandType.SetImageFolderPath;
        }

        public override void HandleCommand(KaleidoscopeCommand command)
        {
            EnsureFallbackTexture();

            if (command == null)
            {
                return;
            }

            switch (command.Type)
            {
                case KaleidoscopeCommandType.SetImageFilePath:
                    TryLoadImageFile(command.StringValue);
                    break;

                case KaleidoscopeCommandType.SetImageFolderPath:
                    TryLoadImageFolder(command.StringValue);
                    break;

                case KaleidoscopeCommandType.SetSourceMode:
                    // If the user switches away from ImageTexture, we keep the loaded texture cached,
                    // but fall back to the procedural texture until ImageTexture is selected again.
                    break;
            }
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            Texture active = GetActiveTexture();
            string textureStatus = active != null ? active.width + "x" + active.height + " texture" : "No texture";
            string sourceDetail = CurrentSourceMode == KaleidoscopeSourceMode.ImageTexture
                ? (!string.IsNullOrWhiteSpace(State != null ? State.ImageFilePath : null) ? "file" : "folder")
                : "procedural";

            return CreateStatus("Source: " + CurrentSourceMode + " (" + sourceDetail + "), " + textureStatus + ".");
        }

        private Texture2D GetActiveTexture()
        {
            if (State != null && State.ActiveSourceMode == KaleidoscopeSourceMode.ImageTexture && loadedTexture != null)
            {
                return loadedTexture;
            }

            EnsureFallbackTexture();
            return fallbackTexture;
        }

        private void TrySyncFromState()
        {
            if (State == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(State.ImageFilePath))
            {
                TryLoadImageFile(State.ImageFilePath);
                return;
            }

            if (!string.IsNullOrWhiteSpace(State.ImageFolderPath))
            {
                TryLoadImageFolder(State.ImageFolderPath);
            }
        }

        private void EnsureFallbackTexture()
        {
            if (fallbackTexture != null)
            {
                return;
            }

            fallbackTexture = new Texture2D(FallbackTextureSize, FallbackTextureSize, TextureFormat.RGBA32, false)
            {
                name = "Kaleidoscope2_Source_FallbackGradient",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            for (int y = 0; y < FallbackTextureSize; y++)
            {
                for (int x = 0; x < FallbackTextureSize; x++)
                {
                    float horizontal = x / (float)(FallbackTextureSize - 1);
                    float vertical = y / (float)(FallbackTextureSize - 1);
                    float pulse = Mathf.Sin((horizontal + vertical) * Mathf.PI * 2f) * 0.25f + 0.75f;
                    Color color = new Color(horizontal * pulse, vertical, 1f - horizontal * 0.65f, 1f);
                    fallbackTexture.SetPixel(x, y, color);
                }
            }

            fallbackTexture.Apply(false, false);
        }

        private void TryLoadImageFolder(string folderPath)
        {
            folderImages.Clear();
            folderImageIndex = 0;

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                ReportWarning("Image folder path was empty.");
                return;
            }

            try
            {
                if (!Directory.Exists(folderPath))
                {
                    ReportWarning("Image folder does not exist: " + folderPath);
                    return;
                }

                string[] files = Directory.GetFiles(folderPath);
                for (int index = 0; index < files.Length; index++)
                {
                    string file = files[index];
                    if (IsSupportedImageFile(file))
                    {
                        folderImages.Add(file);
                    }
                }

                if (folderImages.Count == 0)
                {
                    ReportWarning("No supported images found in folder: " + folderPath);
                    return;
                }

                TryLoadImageFile(folderImages[0]);
            }
            catch (Exception exception)
            {
                ReportWarning("Failed to scan image folder: " + exception.Message);
            }
        }

        private void TryLoadImageFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                ReportWarning("Image file path was empty.");
                return;
            }

            if (!IsSupportedImageFile(filePath))
            {
                ReportWarning("Unsupported image file type: " + filePath);
                return;
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    ReportWarning("Image file does not exist: " + filePath);
                    return;
                }

                byte[] bytes = File.ReadAllBytes(filePath);
                if (bytes == null || bytes.Length == 0)
                {
                    ReportWarning("Image file was empty: " + filePath);
                    return;
                }

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                {
                    name = "Kaleidoscope2_Source_Image",
                    wrapMode = TextureWrapMode.Repeat,
                    filterMode = FilterMode.Bilinear
                };

                if (!texture.LoadImage(bytes, markNonReadable: false))
                {
                    DestroyTexture(texture);
                    ReportWarning("Failed to decode image: " + filePath);
                    return;
                }

                DestroyTexture(ref loadedTexture);
                loadedTexture = texture;
            }
            catch (Exception exception)
            {
                ReportWarning("Failed to load image file: " + exception.Message);
            }
        }

        private static bool IsSupportedImageFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            string extension = Path.GetExtension(filePath);
            if (string.IsNullOrWhiteSpace(extension))
            {
                return false;
            }

            extension = extension.ToLowerInvariant();

            for (int index = 0; index < ImageExtensions.Length; index++)
            {
                if (extension == ImageExtensions[index])
                {
                    return true;
                }
            }

            return false;
        }

        private static void DestroyTexture(ref Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            DestroyTexture(texture);
            texture = null;
        }

        private static void DestroyTexture(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(texture);
            }
            else
            {
                DestroyImmediate(texture);
            }
        }
    }
}
