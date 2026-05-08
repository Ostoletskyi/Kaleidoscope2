using Kaleidoscope2.Core;
using Kaleidoscope2.ImageSource;
using UnityEngine;

namespace Kaleidoscope2.Source
{
    [DisallowMultipleComponent]
    public sealed class SourceModule : KaleidoscopeModuleBase, IKaleidoscopeSourceProvider
    {
        private const int FallbackTextureSize = 256;

        [Header("Image Slideshow")]
        [SerializeField] private float imageSwitchInterval = 30f;
        [SerializeField] private float imageCrossfadeDuration = 2.5f;
        [SerializeField] private bool shuffleImages = false;
        [SerializeField] private bool loopImages = true;

        private Texture2D fallbackTexture;
        private readonly ImageSlideshowController slideshow = new ImageSlideshowController();

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
            ConfigureSlideshow();
            TrySyncFromState();
        }

        private void OnDestroy()
        {
            slideshow.Dispose();
            DestroyTexture(ref fallbackTexture);
        }

        public override void Tick(float deltaTime)
        {
            ConfigureSlideshow();
            slideshow.Tick(deltaTime);
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
                ? slideshow.Mode + ", image " + (slideshow.CurrentImageIndex + 1) + "/" + slideshow.ImagePaths.Count
                : "procedural";

            return CreateStatus("Source: " + CurrentSourceMode + " (" + sourceDetail + "), " + textureStatus
                + ", next switch in " + slideshow.NextSwitchIn.ToString("0.0") + "s, slideshow active: " + slideshow.IsActive + ".");
        }

        private Texture GetActiveTexture()
        {
            if (State != null && State.ActiveSourceMode == KaleidoscopeSourceMode.ImageTexture && slideshow.SourceTexture != null)
            {
                return slideshow.SourceTexture;
            }

            EnsureFallbackTexture();
            return fallbackTexture;
        }

        private void ConfigureSlideshow()
        {
            slideshow.Configure(imageSwitchInterval, imageCrossfadeDuration, shuffleImages, loopImages);
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

        private void TryLoadImageFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                ReportWarning("Image file path was empty.");
                return;
            }

            slideshow.SetImagePaths(new[] { filePath });
            if (!slideshow.IsActive)
            {
                ReportWarning("Failed to load image file: " + slideshow.LastMessage);
            }
        }

        private void TryLoadImageFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                ReportWarning("Image folder path was empty.");
                return;
            }

            slideshow.SetImageFolder(folderPath);
            if (!slideshow.IsActive)
            {
                ReportWarning("Failed to start image slideshow: " + slideshow.LastMessage);
            }
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
