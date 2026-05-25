using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu
{
    [Serializable]
    public sealed class MenuAudioFeedbackSettings
    {
        [SerializeField] private AudioClip buttonPressClip;
        [SerializeField] private AudioClip checkboxEnabledClip;
        [SerializeField] private AudioClip checkboxDisabledClip;
        [SerializeField] private AudioClip buttonForwardClip;
        [SerializeField] private AudioClip buttonBackClip;
        [SerializeField, Range(0f, 1f)] private float volume = 0.55f;
        [SerializeField] private bool randomizePitch = true;
        [SerializeField, Range(0.5f, 1.5f)] private float minimumPitch = 0.97f;
        [SerializeField, Range(0.5f, 1.5f)] private float maximumPitch = 1.03f;
        [SerializeField, Range(0f, 0.25f)] private float minimumPlayInterval = 0.035f;
        [SerializeField, Range(0.02f, 0.3f)] private float sliderCooldown = 0.1f;

        public AudioClip ButtonPressClip { get { return buttonPressClip; } }
        public AudioClip CheckboxEnabledClip { get { return checkboxEnabledClip; } }
        public AudioClip CheckboxDisabledClip { get { return checkboxDisabledClip; } }
        public AudioClip ButtonForwardClip { get { return buttonForwardClip; } }
        public AudioClip ButtonBackClip { get { return buttonBackClip; } }
        public float Volume { get { return Mathf.Clamp01(volume); } }
        public bool RandomizePitch { get { return randomizePitch; } }
        public float MinimumPitch { get { return Mathf.Min(minimumPitch, maximumPitch); } }
        public float MaximumPitch { get { return Mathf.Max(minimumPitch, maximumPitch); } }
        public float MinimumPlayInterval { get { return Mathf.Max(0f, minimumPlayInterval); } }
        public float SliderCooldown { get { return Mathf.Clamp(sliderCooldown, 0.02f, 0.3f); } }
    }

    [DisallowMultipleComponent]
    public sealed class MenuAudioFeedbackController : MonoBehaviour
    {
        private enum FeedbackType
        {
            ButtonPress,
            CheckboxEnabled,
            CheckboxDisabled,
            SliderForward,
            SliderBack
        }

        private static MenuAudioFeedbackController active;

        [SerializeField] private MenuAudioFeedbackSettings settings = new MenuAudioFeedbackSettings();

        private AudioSource audioSource;
        private float lastPlaybackTime = float.NegativeInfinity;
        private bool warnedButtonClipMissing;
        private bool warnedCheckboxEnabledClipMissing;
        private bool warnedCheckboxDisabledClipMissing;
        private bool warnedSliderForwardClipMissing;
        private bool warnedSliderBackClipMissing;
        private readonly HashSet<int> boundSliderIds = new HashSet<int>();

        public MenuAudioFeedbackSettings Settings { get { return settings; } }
        public AudioSource PlaybackSource { get { return audioSource; } }

        internal static MenuAudioFeedbackController Ensure(GameObject host, MenuAudioFeedbackSettings configuredSettings)
        {
            if (host == null)
            {
                return null;
            }

            MenuAudioFeedbackController controller = host.GetComponent<MenuAudioFeedbackController>();
            if (controller == null)
            {
                controller = host.AddComponent<MenuAudioFeedbackController>();
            }

            controller.Configure(configuredSettings);
            active = controller;
            return controller;
        }

        internal static void BindButton(Button button)
        {
            if (button != null)
            {
                button.onClick.AddListener(PlayButtonPress);
            }
        }

        internal static void BindToggle(Toggle toggle)
        {
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(PlayCheckboxState);
            }
        }

        internal static void BindToggleButton(Button button, Func<bool> valueProvider)
        {
            if (button != null && valueProvider != null)
            {
                button.onClick.AddListener(() => PlayCheckboxState(valueProvider()));
            }
        }

        internal static void BindSlider(Slider slider)
        {
            if (active != null && slider != null)
            {
                active.BindSliderDirectionFeedback(slider);
            }
        }

        private static void PlayButtonPress()
        {
            if (active != null)
            {
                active.Play(FeedbackType.ButtonPress);
            }
        }

        private static void PlayCheckboxState(bool enabled)
        {
            if (active != null)
            {
                active.Play(enabled ? FeedbackType.CheckboxEnabled : FeedbackType.CheckboxDisabled);
            }
        }

        private void BindSliderDirectionFeedback(Slider slider)
        {
            if (!boundSliderIds.Add(slider.GetInstanceID()))
            {
                return;
            }

            float lastReportedValue = slider.value;
            slider.onValueChanged.AddListener(value =>
            {
                if (!slider.interactable)
                {
                    lastReportedValue = value;
                    return;
                }

                float range = Mathf.Max(0.001f, slider.maxValue - slider.minValue);
                float threshold = slider.wholeNumbers ? 0.5f : Mathf.Max(0.001f, range * 0.01f);
                float delta = value - lastReportedValue;
                if (Mathf.Abs(delta) < threshold)
                {
                    return;
                }

                Play(delta > 0f ? FeedbackType.SliderForward : FeedbackType.SliderBack);
                lastReportedValue = value;
            });
        }

        private void OnEnable()
        {
            active = this;
            EnsureSource();
        }

        private void OnDestroy()
        {
            if (active == this)
            {
                active = null;
            }
        }

        private void Configure(MenuAudioFeedbackSettings configuredSettings)
        {
            if (configuredSettings != null)
            {
                settings = configuredSettings;
            }

            EnsureSource();
        }

        private void EnsureSource()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.dopplerLevel = 0f;
        }

        private void Play(FeedbackType feedbackType)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            EnsureSource();
            AudioClip clip = ResolveClip(feedbackType);
            if (clip == null)
            {
                ReportMissingClipOnce(feedbackType);
                return;
            }

            float now = Time.unscaledTime;
            float cooldown = feedbackType == FeedbackType.SliderForward || feedbackType == FeedbackType.SliderBack
                ? settings.SliderCooldown
                : settings.MinimumPlayInterval;
            if (now - lastPlaybackTime < cooldown)
            {
                return;
            }

            audioSource.pitch = settings.RandomizePitch
                ? UnityEngine.Random.Range(settings.MinimumPitch, settings.MaximumPitch)
                : 1f;
            audioSource.PlayOneShot(clip, settings.Volume);
            lastPlaybackTime = now;
        }

        private AudioClip ResolveClip(FeedbackType feedbackType)
        {
            switch (feedbackType)
            {
                case FeedbackType.CheckboxEnabled:
                    return settings.CheckboxEnabledClip;
                case FeedbackType.CheckboxDisabled:
                    return settings.CheckboxDisabledClip;
                case FeedbackType.SliderForward:
                    return settings.ButtonForwardClip;
                case FeedbackType.SliderBack:
                    return settings.ButtonBackClip;
                default:
                    return settings.ButtonPressClip;
            }
        }

        private void ReportMissingClipOnce(FeedbackType feedbackType)
        {
            bool shouldWarn;
            switch (feedbackType)
            {
                case FeedbackType.CheckboxEnabled:
                    shouldWarn = !warnedCheckboxEnabledClipMissing;
                    warnedCheckboxEnabledClipMissing = true;
                    break;
                case FeedbackType.CheckboxDisabled:
                    shouldWarn = !warnedCheckboxDisabledClipMissing;
                    warnedCheckboxDisabledClipMissing = true;
                    break;
                case FeedbackType.SliderForward:
                    shouldWarn = !warnedSliderForwardClipMissing;
                    warnedSliderForwardClipMissing = true;
                    break;
                case FeedbackType.SliderBack:
                    shouldWarn = !warnedSliderBackClipMissing;
                    warnedSliderBackClipMissing = true;
                    break;
                default:
                    shouldWarn = !warnedButtonClipMissing;
                    warnedButtonClipMissing = true;
                    break;
            }

            if (shouldWarn)
            {
                Debug.LogWarning("[KAELIS Menu Audio] Missing UI feedback clip for " + feedbackType + ". Interaction continues without sound.", this);
            }
        }
    }
}
