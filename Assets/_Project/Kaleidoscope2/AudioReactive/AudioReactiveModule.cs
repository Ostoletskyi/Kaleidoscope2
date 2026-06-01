using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Kaleidoscope2.Core;
using Kaleidoscope2.Demo;
using UnityEngine;
using UnityEngine.Networking;

namespace Kaleidoscope2.AudioReactive
{
    [Serializable]
    public struct AudioRuntimeSnapshot
    {
        public bool UsingDemoContent;
        public string DemoTrackId;
        public List<string> Playlist;
        public int PlaylistIndex;
        public string LastLoadedPath;
        public bool WasPlaying;
        public float PlaybackTime;
    }

    [DisallowMultipleComponent]
    public sealed class AudioReactiveModule : KaleidoscopeModuleBase
    {
        private static readonly string[] AudioExtensions = { ".mp3", ".wav", ".ogg", ".aiff", ".aif" };
        private const float TrackEndToleranceSeconds = 0.08f;

        private enum PlaybackState
        {
            Idle = 0,
            Loading = 1,
            Playing = 2,
            Stopped = 3
        }

        [Header("Playback")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private bool autoplayOnLoad = true;

        private readonly List<string> playlist = new List<string>(256);
        private readonly List<AudioClip> curatedPlaylist = new List<AudioClip>(32);
        private int currentTrackIndex;
        private Coroutine loadingRoutine;
        private string lastLoadedPath;
        private bool stopRequested;
        private PlaybackState playbackState = PlaybackState.Idle;
        private float currentTrackExpectedEndAt;
        private bool usingDemoContent;
        private bool usingDemoContentFallback;
        private string activeDemoTrackId = string.Empty;
        private int consecutiveLoadFailures;

        public override string ModuleId
        {
            get { return "AudioReactive"; }
        }
        public bool UsingDemoContentFallback { get { return usingDemoContentFallback; } }
        public int CurrentTrackIndex { get { return currentTrackIndex; } }
        public int ActivePlaylistCount { get { return GetPlaylistCount(); } }
        public bool CuratedDemoAudioUnavailable
        {
            get
            {
                return usingDemoContent
                    && usingDemoContentFallback
                    && audioSource != null
                    && audioSource.clip == null
                    && GetPlaylistCount() == 0;
            }
        }

        protected override void OnInitialized()
        {
            EnsureAudioSource();
            TrySyncFromState();
        }

        public override bool CanHandle(KaleidoscopeCommand command)
        {
            if (command == null)
            {
                return false;
            }

            return command.Type == KaleidoscopeCommandType.SetAudioFilePath
                || command.Type == KaleidoscopeCommandType.SetAudioFolderPath
                || command.Type == KaleidoscopeCommandType.SetDemoAudioContent
                || command.Type == KaleidoscopeCommandType.SetAudioPlaybackEnabled
                || command.Type == KaleidoscopeCommandType.PreviousAudioTrack
                || command.Type == KaleidoscopeCommandType.ToggleAudioPlayback
                || command.Type == KaleidoscopeCommandType.NextAudioTrack;
        }

        public override void HandleCommand(KaleidoscopeCommand command)
        {
            if (command == null)
            {
                return;
            }

            switch (command.Type)
            {
                case KaleidoscopeCommandType.SetAudioFilePath:
                    LoadSingleFile(command.StringValue);
                    break;

                case KaleidoscopeCommandType.SetAudioFolderPath:
                    LoadFolder(command.StringValue);
                    break;

                case KaleidoscopeCommandType.SetDemoAudioContent:
                    LoadDemoAudio(command.StringValue);
                    break;

                case KaleidoscopeCommandType.SetAudioPlaybackEnabled:
                    SetPlaybackEnabled(command.BoolValue);
                    break;

                case KaleidoscopeCommandType.PreviousAudioTrack:
                    PlayPreviousTrack();
                    break;

                case KaleidoscopeCommandType.ToggleAudioPlayback:
                    TogglePlayback();
                    break;

                case KaleidoscopeCommandType.NextAudioTrack:
                    PlayNextTrack();
                    break;
            }
        }

        public override void Tick(float deltaTime)
        {
            if (audioSource == null || audioSource.clip == null || loadingRoutine != null)
            {
                return;
            }

            if (playbackState != PlaybackState.Playing || stopRequested)
            {
                return;
            }

            if (audioSource.isPlaying)
            {
                return;
            }

            if (HasReachedTrackEnd())
            {
                PlayNextTrack();
                return;
            }

            ResumeInterruptedPlayback();
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            string detail = string.IsNullOrWhiteSpace(lastLoadedPath) ? "No audio selected." : "Loaded: " + Path.GetFileName(lastLoadedPath);
            if (audioSource != null && audioSource.clip != null)
            {
                detail += ", " + playbackState.ToString().ToLowerInvariant();
                detail += audioSource.isPlaying ? " (audible)" : " (not audible)";
                detail += ", track " + (currentTrackIndex + 1) + "/" + Mathf.Max(GetPlaylistCount(), 1);
                detail += ", time " + audioSource.time.ToString("0.0") + "/" + audioSource.clip.length.ToString("0.0");
            }

            return CreateStatus(detail);
        }

        public AudioRuntimeSnapshot CaptureRuntimeSnapshot()
        {
            return new AudioRuntimeSnapshot
            {
                UsingDemoContent = usingDemoContent,
                DemoTrackId = activeDemoTrackId,
                Playlist = new List<string>(playlist),
                PlaylistIndex = currentTrackIndex,
                LastLoadedPath = lastLoadedPath,
                WasPlaying = audioSource != null && audioSource.isPlaying,
                PlaybackTime = audioSource != null && audioSource.clip != null ? audioSource.time : 0f
            };
        }

        public void RestoreRuntimeSnapshot(AudioRuntimeSnapshot snapshot)
        {
            if (snapshot.UsingDemoContent)
            {
                LoadDemoAudio(snapshot.DemoTrackId);
                if (curatedPlaylist.Count > 0)
                {
                    SetCuratedClipAtIndex(snapshot.PlaylistIndex, false, snapshot.PlaybackTime);
                }
                else if (playlist.Count > 0)
                {
                    StartLoadingAtIndex(snapshot.PlaylistIndex, false, snapshot.PlaybackTime);
                }

                SetPlaybackEnabled(snapshot.WasPlaying, snapshot.PlaybackTime);
                return;
            }

            usingDemoContent = false;
            usingDemoContentFallback = false;
            activeDemoTrackId = string.Empty;
            playlist.Clear();
            curatedPlaylist.Clear();
            if (snapshot.Playlist != null)
            {
                playlist.AddRange(snapshot.Playlist);
            }

            currentTrackIndex = Mathf.Clamp(snapshot.PlaylistIndex, 0, Mathf.Max(0, playlist.Count - 1));
            if (playlist.Count > 0)
            {
                StartLoadingAtIndex(currentTrackIndex, snapshot.WasPlaying, snapshot.PlaybackTime);
            }
            else if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = null;
                playbackState = PlaybackState.Idle;
            }
        }

        private void EnsureAudioSource()
        {
            if (audioSource != null)
            {
                return;
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.mute = false;
            audioSource.ignoreListenerPause = true;

            if (audioSource.volume <= 0.001f)
            {
                audioSource.volume = 1f;
            }
        }

        private void TrySyncFromState()
        {
            if (State == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(State.AudioFilePath))
            {
                LoadSingleFile(State.AudioFilePath);
                return;
            }

            if (!string.IsNullOrWhiteSpace(State.AudioFolderPath))
            {
                LoadFolder(State.AudioFolderPath);
            }
        }

        private void LoadSingleFile(string path)
        {
            usingDemoContent = false;
            usingDemoContentFallback = false;
            activeDemoTrackId = string.Empty;
            playlist.Clear();
            curatedPlaylist.Clear();
            currentTrackIndex = 0;

            if (string.IsNullOrWhiteSpace(path))
            {
                ReportWarning("Audio file path was empty.");
                return;
            }

            if (!File.Exists(path))
            {
                ReportWarning("Audio file does not exist: " + path);
                return;
            }

            if (!HasSupportedExtension(path))
            {
                ReportWarning("Unsupported audio file type: " + path);
                return;
            }

            playlist.Add(path);
            StartLoadingAtIndex(0, autoplayOnLoad);
        }

        private void LoadFolder(string folderPath)
        {
            usingDemoContent = false;
            usingDemoContentFallback = false;
            activeDemoTrackId = string.Empty;
            playlist.Clear();
            curatedPlaylist.Clear();
            currentTrackIndex = 0;

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                ReportWarning("Audio folder path was empty.");
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                ReportWarning("Audio folder does not exist: " + folderPath);
                return;
            }

            try
            {
                string[] files = Directory.GetFiles(folderPath);
                for (int i = 0; i < files.Length; i++)
                {
                    if (HasSupportedExtension(files[i]))
                    {
                        playlist.Add(files[i]);
                    }
                }

                playlist.Sort(StringComparer.OrdinalIgnoreCase);

                if (playlist.Count == 0)
                {
                    ReportWarning("No supported audio files found in folder: " + folderPath);
                    return;
                }

                StartLoadingAtIndex(0, autoplayOnLoad);
            }
            catch (Exception exception)
            {
                ReportWarning("Failed to scan audio folder: " + exception.Message);
            }
        }

        private void LoadDemoAudio(string trackId)
        {
            EnsureAudioSource();
            usingDemoContent = true;
            usingDemoContentFallback = false;
            activeDemoTrackId = string.IsNullOrWhiteSpace(trackId) ? DemoContentCatalog.DefaultAudioTrackId : trackId;
            playlist.Clear();
            curatedPlaylist.Clear();
            currentTrackIndex = 0;
            consecutiveLoadFailures = 0;

            DemoContentCatalog catalog = DemoContentCatalog.LoadDefault();
            if (catalog != null)
            {
                AudioClip[] authoredPlaylist = catalog.MeditationAudioPlaylist;
                if (authoredPlaylist != null)
                {
                    for (int index = 0; index < authoredPlaylist.Length; index++)
                    {
                        if (authoredPlaylist[index] != null)
                        {
                            curatedPlaylist.Add(authoredPlaylist[index]);
                        }
                    }
                }

                if (curatedPlaylist.Count == 0 && catalog.DefaultMeditationAudio != null)
                {
                    curatedPlaylist.Add(catalog.DefaultMeditationAudio);
                }

                if (curatedPlaylist.Count > 0)
                {
                    SetCuratedClipAtIndex(0, false, 0f);
                    return;
                }
            }

            string packagedDirectory = Path.Combine(Application.streamingAssetsPath, "Kaleidoscope2", "DemoContent", "Audio");
            if (Directory.Exists(packagedDirectory))
            {
                usingDemoContentFallback = true;
                string[] packagedFiles = Directory.GetFiles(packagedDirectory);
                Array.Sort(packagedFiles, StringComparer.OrdinalIgnoreCase);
                for (int index = 0; index < packagedFiles.Length; index++)
                {
                    if (HasSupportedExtension(packagedFiles[index]))
                    {
                        playlist.Add(packagedFiles[index]);
                    }
                }

                if (playlist.Count > 0)
                {
                    StartLoadingAtIndex(0, false, 0f);
                    return;
                }
            }

            usingDemoContentFallback = true;
            audioSource.Stop();
            audioSource.clip = null;
            playbackState = PlaybackState.Idle;
            ReportWarning("Curated demo audio unavailable; continuing without audio.");
        }

        private void SetCuratedClipAtIndex(int index, bool playWhenReady, float startTime)
        {
            if (curatedPlaylist.Count == 0)
            {
                return;
            }

            index = ResolveWrappedTrackIndex(index, curatedPlaylist.Count);

            if (loadingRoutine != null)
            {
                StopCoroutine(loadingRoutine);
                loadingRoutine = null;
            }

            currentTrackIndex = index;
            audioSource.Stop();
            audioSource.clip = curatedPlaylist[currentTrackIndex];
            lastLoadedPath = "catalog:" + audioSource.clip.name;
            stopRequested = !playWhenReady;
            if (playWhenReady)
            {
                PlayLoadedClip(startTime);
            }
            else
            {
                audioSource.time = Mathf.Clamp(startTime, 0f, Mathf.Max(0f, audioSource.clip.length - TrackEndToleranceSeconds));
                playbackState = PlaybackState.Stopped;
            }
        }

        private void StartLoadingAtIndex(int index, bool playWhenLoaded, float startTime = 0f)
        {
            if (playlist.Count == 0)
            {
                ReportWarning("No audio tracks are loaded.");
                return;
            }

            currentTrackIndex = ResolveWrappedTrackIndex(index, playlist.Count);
            StartLoading(playlist[currentTrackIndex], playWhenLoaded, startTime);
        }

        private void StartLoading(string filePath, bool playWhenLoaded, float startTime = 0f)
        {
            EnsureAudioSource();

            if (loadingRoutine != null)
            {
                StopCoroutine(loadingRoutine);
            }

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            stopRequested = !playWhenLoaded;
            playbackState = PlaybackState.Loading;
            loadingRoutine = StartCoroutine(LoadAudioClipCoroutine(filePath, playWhenLoaded, startTime));
        }

        private IEnumerator LoadAudioClipCoroutine(string filePath, bool playWhenLoaded, float startTime)
        {
            lastLoadedPath = filePath;

            AudioType audioType = GuessAudioType(filePath);
            if (audioType == AudioType.UNKNOWN)
            {
                ReportWarning("Unsupported audio type: " + filePath);
                playbackState = PlaybackState.Idle;
                loadingRoutine = null;
                TrySkipFailedCuratedFile(playWhenLoaded);
                yield break;
            }

            string uri = "file:///" + filePath.Replace("\\", "/");
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri, audioType))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    ReportWarning("Audio load failed: " + request.error);
                    playbackState = PlaybackState.Idle;
                    loadingRoutine = null;
                    TrySkipFailedCuratedFile(playWhenLoaded);
                    yield break;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                if (clip == null)
                {
                    ReportWarning("Audio clip decode returned null.");
                    playbackState = PlaybackState.Idle;
                    loadingRoutine = null;
                    TrySkipFailedCuratedFile(playWhenLoaded);
                    yield break;
                }

                audioSource.clip = clip;

                if (playWhenLoaded)
                {
                    PlayLoadedClip(startTime);
                }
                else
                {
                    audioSource.time = Mathf.Clamp(startTime, 0f, Mathf.Max(0f, audioSource.clip.length - TrackEndToleranceSeconds));
                    playbackState = PlaybackState.Stopped;
                }

                loadingRoutine = null;
            }
        }

        private void TogglePlayback()
        {
            EnsureAudioSource();

            if (audioSource.clip == null)
            {
                if (curatedPlaylist.Count > 0)
                {
                    SetCuratedClipAtIndex(currentTrackIndex, true, 0f);
                }
                else if (playlist.Count > 0)
                {
                    StartLoadingAtIndex(currentTrackIndex, true);
                }
                else
                {
                    ReportWarning("No audio track selected.");
                }

                return;
            }

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
                stopRequested = true;
                playbackState = PlaybackState.Stopped;
                return;
            }

            PlayLoadedClip();
        }

        private void SetPlaybackEnabled(bool enabled, float startTime = 0f)
        {
            EnsureAudioSource();
            if (!enabled)
            {
                audioSource.Stop();
                stopRequested = true;
                playbackState = PlaybackState.Stopped;
                return;
            }

            if (audioSource.clip != null)
            {
                PlayLoadedClip(startTime);
            }
            else if (playlist.Count > 0)
            {
                StartLoadingAtIndex(currentTrackIndex, true, startTime);
            }
            else if (curatedPlaylist.Count > 0)
            {
                SetCuratedClipAtIndex(currentTrackIndex, true, startTime);
            }
        }

        private void PlayNextTrack()
        {
            if (curatedPlaylist.Count > 0)
            {
                SetCuratedClipAtIndex(currentTrackIndex + 1, true, 0f);
                return;
            }

            if (playlist.Count == 0)
            {
                ReportWarning("No audio playlist loaded.");
                return;
            }

            StartLoadingAtIndex(currentTrackIndex + 1, true);
        }

        private void PlayPreviousTrack()
        {
            if (curatedPlaylist.Count > 0)
            {
                SetCuratedClipAtIndex(currentTrackIndex - 1, true, 0f);
                return;
            }

            if (playlist.Count == 0)
            {
                ReportWarning("No audio playlist loaded.");
                return;
            }

            StartLoadingAtIndex(currentTrackIndex - 1, true);
        }

        private void PlayLoadedClip(float startTime = 0f)
        {
            if (audioSource == null || audioSource.clip == null)
            {
                ReportWarning("No decoded audio clip is ready to play.");
                playbackState = PlaybackState.Idle;
                return;
            }

            stopRequested = false;
            audioSource.mute = false;
            audioSource.spatialBlend = 0f;

            if (audioSource.volume <= 0.001f)
            {
                audioSource.volume = 1f;
            }

            audioSource.time = Mathf.Clamp(startTime, 0f, Mathf.Max(0f, audioSource.clip.length - TrackEndToleranceSeconds));
            audioSource.Play();
            currentTrackExpectedEndAt = Time.unscaledTime + Mathf.Max(0f, audioSource.clip.length - audioSource.time);
            playbackState = PlaybackState.Playing;
            consecutiveLoadFailures = 0;
        }

        private int GetPlaylistCount()
        {
            return curatedPlaylist.Count > 0 ? curatedPlaylist.Count : playlist.Count;
        }

        private void TrySkipFailedCuratedFile(bool playWhenLoaded)
        {
            if (!usingDemoContent || playlist.Count == 0)
            {
                return;
            }

            consecutiveLoadFailures++;
            if (consecutiveLoadFailures >= playlist.Count)
            {
                ReportWarning("No valid curated demo audio tracks could be loaded; continuing silently.");
                return;
            }

            StartLoadingAtIndex(currentTrackIndex + 1, playWhenLoaded);
        }

        private bool HasReachedTrackEnd()
        {
            if (audioSource == null || audioSource.clip == null)
            {
                return false;
            }

            float clipLength = audioSource.clip.length;
            if (clipLength <= TrackEndToleranceSeconds)
            {
                return true;
            }

            if (audioSource.time >= clipLength - TrackEndToleranceSeconds)
            {
                return true;
            }

            // Unity may reset AudioSource.time to zero when a non-looping clip completes.
            // Track the expected end time so a completed playlist clip advances instead of restarting.
            return Time.unscaledTime >= currentTrackExpectedEndAt - TrackEndToleranceSeconds;
        }

        public static int ResolveWrappedTrackIndex(int requestedIndex, int trackCount)
        {
            if (trackCount <= 0)
            {
                return 0;
            }

            int resolved = requestedIndex % trackCount;
            return resolved < 0 ? resolved + trackCount : resolved;
        }

        private void ResumeInterruptedPlayback()
        {
            if (audioSource == null || audioSource.clip == null)
            {
                return;
            }

            int timeSamples = audioSource.timeSamples;
            audioSource.mute = false;
            audioSource.Play();

            if (timeSamples > 0 && timeSamples < audioSource.clip.samples)
            {
                audioSource.timeSamples = timeSamples;
            }
        }

        private static AudioType GuessAudioType(string filePath)
        {
            string ext = Path.GetExtension(filePath);
            if (string.IsNullOrWhiteSpace(ext))
            {
                return AudioType.UNKNOWN;
            }

            ext = ext.ToLowerInvariant();
            switch (ext)
            {
                case ".mp3":
                    return AudioType.MPEG;
                case ".wav":
                    return AudioType.WAV;
                case ".ogg":
                    return AudioType.OGGVORBIS;
                case ".aiff":
                case ".aif":
                    return AudioType.AIFF;
            }

            return AudioType.UNKNOWN;
        }

        private static bool HasSupportedExtension(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            string ext = Path.GetExtension(filePath);
            if (string.IsNullOrWhiteSpace(ext))
            {
                return false;
            }

            ext = ext.ToLowerInvariant();
            for (int i = 0; i < AudioExtensions.Length; i++)
            {
                if (ext == AudioExtensions[i])
                {
                    return true;
                }
            }

            return false;
        }
    }
}
