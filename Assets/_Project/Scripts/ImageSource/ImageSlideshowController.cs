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
        private readonly System.Random random = new System.Random();

        private Texture2D currentTexture;
        private Texture2D nextTexture;
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

        public float NextSwitchIn
        {
            get { return active && imagePaths.Count > 1 ? Mathf.Max(0f, imageSwitchInterval - timer) : 0f; }
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

        public void StartSlideshow()
        {
            timer = 0f;
            currentIndex = 0;
            nextIndex = -1;
            active = imagePaths.Count > 0;

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
            if (!active || imagePaths.Count <= 1)
            {
                return;
            }

            BeginTransition(GetNextIndex());
        }

        public void PreviousImage()
        {
            if (!active || imagePaths.Count <= 1)
            {
                return;
            }

            int previousIndex = currentIndex - 1;
            if (previousIndex < 0)
            {
                previousIndex = loopImages ? imagePaths.Count - 1 : 0;
            }

            BeginTransition(previousIndex);
        }

        public void Tick(float deltaTime)
        {
            if (!active || imagePaths.Count == 0)
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

            if (imagePaths.Count <= 1)
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

        private bool LoadCurrent(int index)
        {
            DestroyTexture(ref currentTexture);

            string message;
            Texture2D texture;
            if (!TryLoadImageAt(index, out texture, out message))
            {
                lastMessage = message;
                return false;
            }

            currentTexture = texture;
            currentIndex = index;
            crossfade.SetCurrent(currentTexture);
            return true;
        }

        private void PrepareNextTexture()
        {
            DestroyTexture(ref nextTexture);
            nextIndex = -1;

            if (imagePaths.Count <= 1)
            {
                return;
            }

            nextIndex = GetNextIndex();
            string message;
            Texture2D texture;
            if (!TryLoadImageAt(nextIndex, out texture, out message))
            {
                lastMessage = message;
                nextIndex = -1;
                return;
            }

            nextTexture = texture;
        }

        private void BeginTransition(int targetIndex)
        {
            if (targetIndex < 0 || targetIndex >= imagePaths.Count || targetIndex == currentIndex)
            {
                timer = 0f;
                return;
            }

            if (nextTexture == null || nextIndex != targetIndex)
            {
                DestroyTexture(ref nextTexture);
                string message;
                Texture2D texture;
                if (!TryLoadImageAt(targetIndex, out texture, out message))
                {
                    lastMessage = message;
                    timer = 0f;
                    return;
                }

                nextTexture = texture;
                nextIndex = targetIndex;
            }

            crossfade.Begin(currentTexture, nextTexture, imageCrossfadeDuration);
            crossfade.RenderBlend();
        }

        private void PromoteNextTexture()
        {
            Texture2D oldTexture = currentTexture;
            currentTexture = nextTexture;
            currentIndex = nextIndex;
            nextTexture = null;
            nextIndex = -1;
            timer = 0f;
            crossfade.SetCurrent(currentTexture);

            if (oldTexture != null && oldTexture != currentTexture)
            {
                ImageTextureLoader.DestroyTexture(oldTexture);
            }

            PrepareNextTexture();
        }

        private int GetNextIndex()
        {
            if (imagePaths.Count <= 1)
            {
                return currentIndex;
            }

            if (shuffleImages)
            {
                int candidate = currentIndex;
                int guard = 0;
                while (candidate == currentIndex && guard < 20)
                {
                    candidate = random.Next(0, imagePaths.Count);
                    guard++;
                }

                return candidate;
            }

            int next = currentIndex + 1;
            if (next >= imagePaths.Count)
            {
                return loopImages ? 0 : currentIndex;
            }

            return next;
        }

        private bool TryLoadImageAt(int index, out Texture2D texture, out string message)
        {
            texture = null;
            message = string.Empty;

            if (index < 0 || index >= imagePaths.Count)
            {
                message = "Image index is out of range.";
                return false;
            }

            return loader.TryLoad(imagePaths[index], out texture, out message);
        }

        private void DestroyCurrentTextures()
        {
            DestroyTexture(ref currentTexture);
            DestroyTexture(ref nextTexture);
        }

        private static void DestroyTexture(ref Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            ImageTextureLoader.DestroyTexture(texture);
            texture = null;
        }
    }
}
