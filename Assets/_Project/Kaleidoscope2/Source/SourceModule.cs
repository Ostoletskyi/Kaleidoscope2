using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.Source
{
    [DisallowMultipleComponent]
    public sealed class SourceModule : KaleidoscopeModuleBase, IKaleidoscopeSourceProvider
    {
        private const int TestTextureSize = 256;

        private Texture2D testTexture;

        public override string ModuleId
        {
            get { return "Source"; }
        }

        public Texture SourceTexture
        {
            get { return testTexture; }
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
            get { return testTexture; }
        }

        protected override void OnInitialized()
        {
            EnsureTestTexture();
        }

        private void OnDestroy()
        {
            if (testTexture == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(testTexture);
            }
            else
            {
                DestroyImmediate(testTexture);
            }
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            return command != null && command.Type == KaleidoscopeCommandType.SetSourceMode;
        }

        public override void HandleCommand(KaleidoscopeCommand command)
        {
            EnsureTestTexture();
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            string textureStatus = testTexture != null ? TestTextureSize + "x" + TestTextureSize + " test texture" : "No test texture";
            return CreateStatus("Placeholder source: " + CurrentSourceMode + " using " + textureStatus + ".");
        }

        private void EnsureTestTexture()
        {
            if (testTexture != null)
            {
                return;
            }

            testTexture = new Texture2D(TestTextureSize, TestTextureSize, TextureFormat.RGBA32, false)
            {
                name = "Kaleidoscope2_Source_TestGradient",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            for (int y = 0; y < TestTextureSize; y++)
            {
                for (int x = 0; x < TestTextureSize; x++)
                {
                    float horizontal = x / (float)(TestTextureSize - 1);
                    float vertical = y / (float)(TestTextureSize - 1);
                    Color color = new Color(horizontal, vertical, 1f - horizontal * 0.65f, 1f);
                    testTexture.SetPixel(x, y, color);
                }
            }

            testTexture.Apply(false, false);
        }
    }
}
