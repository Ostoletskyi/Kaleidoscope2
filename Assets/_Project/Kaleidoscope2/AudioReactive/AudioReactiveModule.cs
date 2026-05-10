using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Kaleidoscope2.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace Kaleidoscope2.AudioReactive
{
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
        private int playlistIndex;
        private Coroutine loadingRoutine;
        private string lastLoadedPath;
        private bool stopRequested;
        private PlaybackState playbackState = PlaybackState.Idle;
        private float currentTrackStartedAt;

        public override string ModuleId
        {
            get { return "AudioReactive"; }
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
                detail += ", track " + (playlistIndex + 1) + "/" + Mathf.Max(playlist.Count, 1);
                detail += ", time " + audioSource.time.ToString("0.0") + "/" + audioSource.clip.length.ToString("0.0");
            }

            return CreateStatus(detail);
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
            playlist.Clear();
            playlistIndex = 0;

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
            playlist.Clear();
            playlistIndex = 0;

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

        private void StartLoadingAtIndex(int index, bool playWhenLoaded)
        {
            if (playlist.Count == 0)
            {
                ReportWarning("No audio tracks are loaded.");
                return;
            }

            if (index < 0)
            {
                index = playlist.Count - 1;
            }
            else if (index >= playlist.Count)
            {
                index = 0;
            }

            playlistIndex = index;
            StartLoading(playlist[playlistIndex], playWhenLoaded);
        }

        private void StartLoading(string filePath, bool playWhenLoaded)
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
            loadingRoutine = StartCoroutine(LoadAudioClipCoroutine(filePath, playWhenLoaded));
        }

        private IEnumerator LoadAudioClipCoroutine(string filePath, bool playWhenLoaded)
        {
            lastLoadedPath = filePath;

            AudioType audioType = GuessAudioType(filePath);
            if (audioType == AudioType.UNKNOWN)
            {
                ReportWarning("Unsupported audio type: " + filePath);
                playbackState = PlaybackState.Idle;
                loadingRoutine = null;
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
                    yield break;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                if (clip == null)
                {
                    ReportWarning("Audio clip decode returned null.");
                    playbackState = PlaybackState.Idle;
                    loadingRoutine = null;
                    yield break;
                }

                audioSource.clip = clip;

                if (playWhenLoaded)
                {
                    PlayLoadedClip();
                }
                else
                {
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
                if (playlist.Count > 0)
                {
                    StartLoadingAtIndex(playlistIndex, true);
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

        private void PlayNextTrack()
        {
            if (playlist.Count == 0)
            {
                ReportWarning("No audio playlist loaded.");
                return;
            }

            StartLoadingAtIndex(playlistIndex + 1, true);
        }

        private void PlayPreviousTrack()
        {
            if (playlist.Count == 0)
            {
                ReportWarning("No audio playlist loaded.");
                return;
            }

            StartLoadingAtIndex(playlistIndex - 1, true);
        }

        private void PlayLoadedClip()
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

            audioSource.time = 0f;
            audioSource.Play();
            currentTrackStartedAt = Time.unscaledTime;
            playbackState = PlaybackState.Playing;
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

            float expectedEndAt = currentTrackStartedAt + clipLength;
            return Time.unscaledTime >= expectedEndAt - TrackEndToleranceSeconds && audioSource.time > clipLength * 0.75f;
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
