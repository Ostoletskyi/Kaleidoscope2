using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Kaleidoscope2.FileBrowser
{
    public sealed class RuntimeFileBrowserController
    {
        private readonly RuntimeImageFolderScanner scanner;
        private readonly RuntimePathValidator validator;
        private readonly RuntimeFileBrowserState state = new RuntimeFileBrowserState();
        private readonly List<string> allowedFileExtensions = new List<string>(16);

        public RuntimeFileBrowserController(RuntimeImageFolderScanner scanner, RuntimePathValidator validator)
        {
            this.scanner = scanner ?? new RuntimeImageFolderScanner();
            this.validator = validator ?? new RuntimePathValidator();
            SetAllowedFileExtensions(RuntimeImageFolderScanner.SupportedImageExtensions);
        }

        public event Action<RuntimeFileBrowserState> StateChanged;
        public event Action<string> FolderSelected;
        public event Action Closed;

        public RuntimeFileBrowserState State
        {
            get { return state; }
        }

        public void SetAllowedFileExtensions(IReadOnlyList<string> extensions)
        {
            allowedFileExtensions.Clear();
            if (extensions == null)
            {
                return;
            }

            for (int index = 0; index < extensions.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(extensions[index]))
                {
                    allowedFileExtensions.Add(extensions[index]);
                }
            }
        }

        public void Open(string startPath)
        {
            NavigateTo(startPath);
        }

        public void NavigateTo(string path)
        {
            string normalizedPath;
            string message;
            if (!validator.TryNormalizeDirectoryPath(path, out normalizedPath, out message))
            {
                state.SetCurrentPath(path);
                state.ClearItems();
                state.SetMessage(message, true);
                LogState("NavigateToInvalid");
                RaiseStateChanged();
                return;
            }

            IReadOnlyList<RuntimeFileBrowserItem> folders;
            IReadOnlyList<RuntimeFileBrowserItem> files;
            IReadOnlyList<RuntimeFileBrowserItem> drives;
            bool scanned = scanner.TryScan(normalizedPath, allowedFileExtensions, out folders, out files, out drives, out message);

            state.SetCurrentPath(normalizedPath);
            state.SetItems(folders, files, drives);
            state.SetMessage(message, !scanned);
            LogState("NavigateTo");
            RaiseStateChanged();
        }

        public void NavigateUp()
        {
            if (string.IsNullOrWhiteSpace(state.CurrentPath))
            {
                return;
            }

            try
            {
                DirectoryInfo parent = Directory.GetParent(state.CurrentPath);
                if (parent != null)
                {
                    NavigateTo(parent.FullName);
                    return;
                }
            }
            catch (Exception exception)
            {
                state.SetMessage("Не удалось подняться выше: " + exception.Message, true);
                RaiseStateChanged();
            }
        }

        public void SelectCurrentFolder()
        {
            if (string.IsNullOrWhiteSpace(state.CurrentPath))
            {
                state.SetMessage("Папка не выбрана.", true);
                RaiseStateChanged();
                return;
            }

            IReadOnlyList<RuntimeFileBrowserItem> folders;
            IReadOnlyList<RuntimeFileBrowserItem> files;
            IReadOnlyList<RuntimeFileBrowserItem> drives;
            string message;
            bool scanned = scanner.TryScan(state.CurrentPath, allowedFileExtensions, out folders, out files, out drives, out message);
            state.SetItems(folders, files, drives);
            LogState("SelectCurrentFolder");
            if (!scanned)
            {
                state.SetMessage(message, true);
                RaiseStateChanged();
                return;
            }

            if (files.Count == 0)
            {
                state.SetMessage("В текущей папке нет поддерживаемых файлов.", true);
                RaiseStateChanged();
                return;
            }

            Action<string> handler = FolderSelected;
            if (handler != null)
            {
                handler(state.CurrentPath);
            }
        }

        public void Close()
        {
            Action handler = Closed;
            if (handler != null)
            {
                handler();
            }
        }

        private void RaiseStateChanged()
        {
            Action<RuntimeFileBrowserState> handler = StateChanged;
            if (handler != null)
            {
                handler(state);
            }
        }

        private void LogState(string stage)
        {
            Debug.Log("[FileBrowser] State stage=" + stage
                + " CurrentPath=" + state.CurrentPath
                + " Folders.Count=" + state.Folders.Count
                + " Images.Count=" + state.Files.Count
                + " StatusMessage=" + state.Message);
        }
    }
}
