using System;
using System.Collections.Generic;
using Kaleidoscope2.FileBrowser;
using UnityEngine;

namespace Kaleidoscope2.ImageSource
{
    public sealed class ImageSlideshowController : IDisposable
    {
        private readonly RuntimeImageFolderScanner scanner = new RuntimeImageFolderScanner();
        private readonly ImageTextureLoader loader = new ImageTextureLoader();
        private readonly ImageCrossfadeController crossfade = new ImageCrossfadeController();
        private readonly List<string> imagePaths = new List<string>(512);
        private readonly List<Texture2D> imageAssets = new List<Texture2D>(512);
        private readonly System.Random random = new System.Random();

        private Texture2D currentTexture;
        private Texture2D nextTexture;
        private bool ownsCurrentTexture;
        private bool ownsNextTexture;
        private int currentIndex;
        private int nextIndex;
        private float timer;
        private float imageSwitchInterval = 30f;
        private float imageCrossfadeDuration = 2.5f;
        private bool shuffleImages;
        private bool loopImages = true;
        private bool active;
        private ImageSourceMode mode = ImageSourceMode.None;
        private string lastMessage = string.Empty;

        public Texture SourceTexture
        {
            get { return crossfade.OutputTexture != null ? crossfade.OutputTexture : currentTexture; }
        }

        public IReadOnlyList<string> ImagePaths
        {
            get { return imagePaths; }
        }

        public int CurrentImageIndex
        {
            get { return currentIndex; }
        }

        public int ImageCount
        {
            get { return imageAssets.Count > 0 ? imageAssets.Count : imagePaths.Count; }
        }

        public float ElapsedInCurrentImage
        {
            get { return timer; }
        }

        public float NextSwitchIn
        {
            get { return active && ImageCount > 1 ? Mathf.Max(0f, imageSwitchInterval - timer) : 0f; }
        }

        public bool IsActive
        {
            get { return active; }
        }

        public ImageSourceMode Mode
        {
            get { return mode; }
        }

        public string LastMessage
        {
            get { return lastMessage; }
        }

        public void Configure(float switchInterval, float crossfadeDuration, bool shuffle, bool loop)
        {
            imageSwitchInterval = Mathf.Max(0.1f, switchInterval);
            imageCrossfadeDuration = Mathf.Max(0.01f, crossfadeDuration);
            shuffleImages = shuffle;
            loopImages = loop;
        }

        public void SetImageFolder(string folderPath)
        {
            List<string> paths;
            string message;
            if (!scanner.TryFindImages(folderPath, out paths, out message))
            {
                lastMessage = message;
                StopSlideshow();
                return;
            }

            SetImagePaths(paths);
            mode = ImageSourceMode.FolderSlideshow;
        }

        public void SetImagePaths(IReadOnlyList<string> paths)
        {
            StopSlideshow();
            imagePaths.Clear();
            imageAssets.Clear();

            if (paths != null)
            {
                for (int index = 0; index < paths.Count; index++)
                {
                    string path = paths[index];
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        imagePaths.Add(path);
                    }
                }
            }

            mode = imagePaths.Count > 1 ? ImageSourceMode.FolderSlideshow : ImageSourceMode.SingleImage;
            StartSlideshow();
        }

        public void SetImageAssets(IReadOnlyList<Texture2D> assets)
        {
            StopSlideshow();
            imagePaths.Clear();
            imageAssets.Clear();
            if (assets != null)
            {
                for (int index = 0; index < assets.Count; index++)
                {
                    if (assets[index] != null)
                    {
                        imageAssets.Add(assets[index]);
                    }
                }
            }

            mode = ImageSourceMode.CuratedAssets;
            StartSlideshow();
        }

        public void StartSlideshow()
        {
            timer = 0f;
            currentIndex = 0;
            nextIndex = -1;
            active = ImageCount > 0;

            if (!active)
            {
                lastMessage = "No image paths were provided.";
                return;
            }

            if (!LoadCurrent(currentIndex))
            {
                active = false;
                return;
            }

            PrepareNextTexture();
        }

        public void StopSlideshow()
        {
            active = false;
            timer = 0f;
            nextIndex = -1;
            DestroyCurrentTextures();
            crossfade.SetCurrent(null);
        }

        public void NextImage()
        {
            if (!active || ImageCount <= 1)
            {
                return;
            }

            BeginTransition(GetNextIndex());
        }

        public void PreviousImage()
        {
            if (!active || ImageCount <= 1)
            {
                return;
            }

            int previousIndex = currentIndex - 1;
            if (previousIndex < 0)
            {
                previousIndex = loopImages ? ImageCount - 1 : 0;
            }

            BeginTransition(previousIndex);
        }

        public void Tick(float deltaTime)
        {
            if (!active || ImageCount == 0)
            {
                return;
            }

            if (crossfade.IsTransitioning)
            {
                bool completed = crossfade.Tick(deltaTime);
                if (completed)
                {
                    PromoteNextTexture();
                }

                return;
            }

            if (ImageCount <= 1)
            {
                return;
            }

            timer += Mathf.Max(0f, deltaTime);
            if (timer >= imageSwitchInterval)
            {
                BeginTransition(GetNextIndex());
            }
        }

        public void Dispose()
        {
            DestroyCurrentTextures();
            crossfade.Dispose();
        }

        public void RestorePosition(int index, float elapsed)
        {
            if (!active || ImageCount == 0)
            {
                return;
            }

            int resolved = Mathf.Clamp(index, 0, ImageCount - 1);
            if (resolved != currentIndex)
            {
                LoadCurrent(resolved);
                PrepareNextTexture();
            }

            timer = Mathf.Clamp(elapsed, 0f, imageSwitchInterval);
        }

        private bool LoadCurrent(int index)
        {
            DestroyTexture(ref currentTexture, ref ownsCurrentTexture);

            string message;
            Texture2D texture;
            bool ownsTexture;
            if (!TryLoadImageAt(index, out texture, out ownsTexture, out message))
            {
                lastMessage = message;
                return false;
            }

            currentTexture = texture;
            ownsCurrentTexture = ownsTexture;
            currentIndex = index;
            crossfade.SetCurrent(currentTexture);
            return true;
        }

        private void PrepareNextTexture()
        {
            DestroyTexture(ref nextTexture, ref ownsNextTexture);
            nextIndex = -1;

            if (ImageCount <= 1)
            {
                return;
            }

            nextIndex = GetNextIndex();
            string message;
            Texture2D texture;
            bool ownsTexture;
            if (!TryLoadImageAt(nextIndex, out texture, out ownsTexture, out message))
            {
                lastMessage = message;
                nextIndex = -1;
                return;
            }

            nextTexture = texture;
            ownsNextTexture = ownsTexture;
        }

        private void BeginTransition(int targetIndex)
        {
            if (targetIndex < 0 || targetIndex >= ImageCount || targetIndex == currentIndex)
            {
                timer = 0f;
                return;
            }

            if (nextTexture == null || nextIndex != targetIndex)
            {
                DestroyTexture(ref nextTexture, ref ownsNextTexture);
                string message;
                Texture2D texture;
                bool ownsTexture;
                if (!TryLoadImageAt(targetIndex, out texture, out ownsTexture, out message))
                {
                    lastMessage = message;
                    timer = 0f;
                    return;
                }

                nextTexture = texture;
                ownsNextTexture = ownsTexture;
                nextIndex = targetIndex;
            }

            crossfade.Begin(currentTexture, nextTexture, imageCrossfadeDuration);
            crossfade.RenderBlend();
        }

        private void PromoteNextTexture()
        {
            Texture2D oldTexture = currentTexture;
            bool ownedOldTexture = ownsCurrentTexture;
            currentTexture = nextTexture;
            ownsCurrentTexture = ownsNextTexture;
            currentIndex = nextIndex;
            nextTexture = null;
            ownsNextTexture = false;
            nextIndex = -1;
            timer = 0f;
            crossfade.SetCurrent(currentTexture);

            if (ownedOldTexture && oldTexture != null && oldTexture != currentTexture)
            {
                ImageTextureLoader.DestroyTexture(oldTexture);
            }

            PrepareNextTexture();
        }

        private int GetNextIndex()
        {
            if (ImageCount <= 1)
            {
                return currentIndex;
            }

            if (shuffleImages)
            {
                int candidate = currentIndex;
                int guard = 0;
                while (candidate == currentIndex && guard < 20)
                {
                    candidate = random.Next(0, ImageCount);
                    guard++;
                }

                return candidate;
            }

            int next = currentIndex + 1;
            if (next >= ImageCount)
            {
                return loopImages ? 0 : currentIndex;
            }

            return next;
        }

        private bool TryLoadImageAt(int index, out Texture2D texture, out bool ownsTexture, out string message)
        {
            texture = null;
            ownsTexture = false;
            message = string.Empty;

            if (index < 0 || index >= ImageCount)
            {
                message = "Image index is out of range.";
                return false;
            }

            if (imageAssets.Count > 0)
            {
                texture = imageAssets[index];
                message = "Curated asset reference loaded.";
                return true;
            }

            bool loaded = loader.TryLoad(imagePaths[index], out texture, out message);
            ownsTexture = loaded && texture != null;
            return loaded;
        }

        private void DestroyCurrentTextures()
        {
            DestroyTexture(ref currentTexture, ref ownsCurrentTexture);
            DestroyTexture(ref nextTexture, ref ownsNextTexture);
        }

        private static void DestroyTexture(ref Texture2D texture, ref bool ownsTexture)
        {
            if (ownsTexture && texture != null)
            {
                ImageTextureLoader.DestroyTexture(texture);
            }

            texture = null;
            ownsTexture = false;
        }
    }
}
