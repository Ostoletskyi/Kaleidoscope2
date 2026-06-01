using UnityEngine;

namespace Kaleidoscope2.Demo
{
    [CreateAssetMenu(fileName = "Kaleidoscope2_DemoContentCatalog", menuName = "Kaleidoscope2/Demo Content Catalog")]
    public sealed class DemoContentCatalog : ScriptableObject
    {
        public const string ResourcePath = "Kaleidoscope2_DemoContentCatalog";
        public const string MeditationProfileId = "Meditation";
        public const string ReplayProfileId = "Replay";
        public const string BenchmarkProfileId = "Benchmark";
        public const string DefaultAudioTrackId = "Demo";
        public const string MeditationPlaylistId = "MeditationPlaylist";

        [SerializeField] private Texture2D[] demoImages = new Texture2D[0];
        [SerializeField] private AudioClip defaultMeditationAudio;
        [SerializeField] private AudioClip[] meditationAudioPlaylist = new AudioClip[0];

        public Texture2D[] DemoImages { get { return demoImages; } }
        public AudioClip DefaultMeditationAudio { get { return defaultMeditationAudio; } }
        public AudioClip[] MeditationAudioPlaylist { get { return meditationAudioPlaylist; } }

        public static DemoContentCatalog LoadDefault()
        {
            return Resources.Load<DemoContentCatalog>(ResourcePath);
        }

        public bool HasImages()
        {
            return demoImages != null && demoImages.Length > 0;
        }

        public bool HasMeditationAudio()
        {
            if (meditationAudioPlaylist != null)
            {
                for (int index = 0; index < meditationAudioPlaylist.Length; index++)
                {
                    if (meditationAudioPlaylist[index] != null)
                    {
                        return true;
                    }
                }
            }

            return defaultMeditationAudio != null;
        }
    }
}
