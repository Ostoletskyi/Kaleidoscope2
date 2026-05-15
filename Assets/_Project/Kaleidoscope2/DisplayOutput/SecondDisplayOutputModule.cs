using Kaleidoscope2.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.DisplayOutput
{
    [DisallowMultipleComponent]
    public sealed class SecondDisplayOutputModule : KaleidoscopeModuleBase
    {
        [Header("References")]
        [SerializeField] private KaleidoscopeDirector director;

        [Header("Second Display")]
        [SerializeField, Min(1)] private int displayIndex = 1;
        [SerializeField] private bool preserveAspect = true;

        private GameObject outputRoot;
        private Canvas outputCanvas;
        private RawImage outputImage;
        private AspectRatioFitter aspectFitter;
        private bool displayActivated;
        private bool missingDisplayWarningReported;

        public override string ModuleId
        {
            get { return "DisplayOutput"; }
        }

        public override void Tick(float deltaTime)
        {
            if (director == null || State == null)
            {
                return;
            }

            if (!State.SecondDisplayOutputEnabled)
            {
                SetOutputVisible(false);
                return;
            }

            if (!HasTargetDisplay())
            {
                SetOutputVisible(false);
                if (!missingDisplayWarningReported)
                {
                    ReportWarning("Display 2 is not available. Connect a second monitor and press F12 again.");
                    missingDisplayWarningReported = true;
                }

                return;
            }

            missingDisplayWarningReported = false;
            ActivateTargetDisplay();
            EnsureOutputCanvas();
            ApplyOutputTexture(director.FinalOutputTexture);
            SetOutputVisible(true);
        }

        public override void Validate()
        {
            if (director == null)
            {
                ReportMissingReference("Director");
            }
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            if (State == null)
            {
                return CreateStatus("Waiting for state.");
            }

            string enabled = State.SecondDisplayOutputEnabled ? "enabled" : "disabled";
            string display = HasTargetDisplay() ? "Display " + (displayIndex + 1).ToString() : "Display " + (displayIndex + 1).ToString() + " missing";
            Texture output = director != null ? director.FinalOutputTexture : null;
            string texture = output != null ? output.width + "x" + output.height : "no texture yet";
            return CreateStatus(enabled + ", " + display + ", final output " + texture + ".");
        }

        private void OnDestroy()
        {
            DestroyRuntimeObject(outputRoot);
            outputRoot = null;
            outputCanvas = null;
            outputImage = null;
            aspectFitter = null;
        }

        private bool HasTargetDisplay()
        {
            return displayIndex >= 0 && Display.displays != null && displayIndex < Display.displays.Length;
        }

        private void ActivateTargetDisplay()
        {
            if (displayActivated || !HasTargetDisplay())
            {
                return;
            }

            Display.displays[displayIndex].Activate();
            displayActivated = true;
        }

        private void EnsureOutputCanvas()
        {
            if (outputRoot != null && outputCanvas != null && outputImage != null)
            {
                outputCanvas.targetDisplay = displayIndex;
                return;
            }

            outputRoot = new GameObject("Kaleidoscope2_SecondDisplayOutput", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            outputRoot.transform.SetParent(transform, false);

            outputCanvas = outputRoot.GetComponent<Canvas>();
            outputCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            outputCanvas.targetDisplay = displayIndex;
            outputCanvas.sortingOrder = -100;

            CanvasScaler scaler = outputRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform rootRect = (RectTransform)outputRoot.transform;
            Stretch(rootRect);

            GameObject imageObject = new GameObject("FinalOutputImage", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            imageObject.transform.SetParent(outputRoot.transform, false);

            RectTransform imageRect = (RectTransform)imageObject.transform;
            Stretch(imageRect);

            outputImage = imageObject.GetComponent<RawImage>();
            outputImage.raycastTarget = false;
            outputImage.color = Color.black;
            aspectFitter = imageObject.GetComponent<AspectRatioFitter>();
        }

        private void ApplyOutputTexture(Texture texture)
        {
            if (outputImage == null)
            {
                return;
            }

            if (outputImage.texture != texture)
            {
                outputImage.texture = texture;
            }

            outputImage.color = texture != null ? Color.white : Color.black;
            ApplyAspect(texture);
        }

        private void ApplyAspect(Texture texture)
        {
            if (outputImage == null)
            {
                return;
            }

            if (aspectFitter == null)
            {
                aspectFitter = outputImage.GetComponent<AspectRatioFitter>();
            }

            if (aspectFitter == null)
            {
                return;
            }

            bool useAspect = preserveAspect && texture != null && texture.height > 0;
            aspectFitter.enabled = useAspect;
            if (useAspect)
            {
                aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                aspectFitter.aspectRatio = texture.width / (float)texture.height;
            }
            else
            {
                Stretch(outputImage.rectTransform);
            }
        }

        private void SetOutputVisible(bool visible)
        {
            if (outputRoot != null && outputRoot.activeSelf != visible)
            {
                outputRoot.SetActive(visible);
            }
        }

        private static void Stretch(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }

        private static void DestroyRuntimeObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
