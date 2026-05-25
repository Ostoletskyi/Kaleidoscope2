using System;
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
        [SerializeField, Range(0f, 1f)] private float volume = 0.55f;
        [SerializeField] private bool randomizePitch = true;
        [SerializeField, Range(0.5f, 1.5f)] private float minimumPitch = 0.97f;
        [SerializeField, Range(0.5f, 1.5f)] private float maximumPitch = 1.03f;
        [SerializeField, Range(0f, 0.25f)] private float minimumPlayInterval = 0.035f;

        public AudioClip ButtonPressClip { get { return buttonPressClip; } }
        public AudioClip CheckboxEnabledClip { get { return checkboxEnabledClip; } }
        public AudioClip CheckboxDisabledClip { get { return checkboxDisabledClip; } }
        public float Volume { get { return Mathf.Clamp01(volume); } }
        public bool RandomizePitch { get { return randomizePitch; } }
        public float MinimumPitch { get { return Mathf.Min(minimumPitch, maximumPitch); } }
        public float MaximumPitch { get { return Mathf.Max(minimumPitch, maximumPitch); } }
        public float MinimumPlayInterval { get { return Mathf.Max(0f, minimumPlayInterval); } }
    }

    [DisallowMultipleComponent]
    public sealed class MenuAudioFeedbackController : MonoBehaviour
    {
        private enum FeedbackType
        {
            ButtonPress,
            CheckboxEnabled,
            CheckboxDisabled
        }

        private static MenuAudioFeedbackController active;

        [SerializeField] private MenuAudioFeedbackSettings settings = new MenuAudioFeedbackSettings();

        private AudioSource audioSource;
        private float lastPlaybackTime = float.NegativeInfinity;
        private bool warnedButtonClipMissing;
        private bool warnedCheckboxEnabledClipMissing;
        private bool warnedCheckboxDisabledClipMissing;

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
            if (now - lastPlaybackTime < settings.MinimumPlayInterval)
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
