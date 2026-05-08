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

        [Header("Playback")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private bool autoplayOnLoad = true;

        private readonly List<string> playlist = new List<string>(256);
        private int playlistIndex;
        private Coroutine loadingRoutine;
        private string lastLoadedPath;

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
                || command.Type == KaleidoscopeCommandType.SetAudioFolderPath;
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
            }
        }

        public override KaleidoscopeModuleStatus GetStatus()
        {
            string detail = string.IsNullOrWhiteSpace(lastLoadedPath) ? "No audio selected." : "Loaded: " + Path.GetFileName(lastLoadedPath);
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
            StartLoading(path);
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

                StartLoading(playlist[0]);
            }
            catch (Exception exception)
            {
                ReportWarning("Failed to scan audio folder: " + exception.Message);
            }
        }

        private void StartLoading(string filePath)
        {
            EnsureAudioSource();

            if (loadingRoutine != null)
            {
                StopCoroutine(loadingRoutine);
            }

            loadingRoutine = StartCoroutine(LoadAudioClipCoroutine(filePath));
        }

        private IEnumerator LoadAudioClipCoroutine(string filePath)
        {
            lastLoadedPath = filePath;

            AudioType audioType = GuessAudioType(filePath);
            if (audioType == AudioType.UNKNOWN)
            {
                ReportWarning("Unsupported audio type: " + filePath);
                yield break;
            }

            string uri = "file:///" + filePath.Replace("\\", "/");
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri, audioType))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    ReportWarning("Audio load failed: " + request.error);
                    yield break;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                if (clip == null)
                {
                    ReportWarning("Audio clip decode returned null.");
                    yield break;
                }

                audioSource.clip = clip;

                if (autoplayOnLoad)
                {
                    audioSource.Play();
                }
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
