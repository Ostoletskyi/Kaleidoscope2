using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Kaleidoscope2.FileBrowser
{
    public sealed class RuntimeImageFolderScanner
    {
        public static readonly string[] SupportedImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".tga" };

        private readonly List<RuntimeFileBrowserItem> folders = new List<RuntimeFileBrowserItem>(128);
        private readonly List<RuntimeFileBrowserItem> files = new List<RuntimeFileBrowserItem>(256);
        private readonly List<RuntimeFileBrowserItem> drives = new List<RuntimeFileBrowserItem>(16);

        public bool TryScan(
            string folderPath,
            IReadOnlyList<string> allowedFileExtensions,
            out IReadOnlyList<RuntimeFileBrowserItem> scannedFolders,
            out IReadOnlyList<RuntimeFileBrowserItem> scannedFiles,
            out IReadOnlyList<RuntimeFileBrowserItem> scannedDrives,
            out string message)
        {
            folders.Clear();
            files.Clear();
            drives.Clear();
            message = string.Empty;

            scannedFolders = folders;
            scannedFiles = files;
            scannedDrives = drives;

            ScanDrives();
            Debug.Log("[FileBrowser] Scan path=" + folderPath);

            try
            {
                if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                {
                    message = "Папка не существует.";
                    Debug.LogWarning("[FileBrowser] Error=" + message);
                    return false;
                }

                string[] directories = Directory.GetDirectories(folderPath);
                Array.Sort(directories, StringComparer.OrdinalIgnoreCase);
                for (int index = 0; index < directories.Length; index++)
                {
                    string directory = directories[index];
                    string displayName = Path.GetFileName(directory);
                    if (string.IsNullOrWhiteSpace(displayName))
                    {
                        displayName = directory;
                    }

                    folders.Add(new RuntimeFileBrowserItem(RuntimeFileBrowserItemType.Directory, displayName, directory));
                }

                string[] folderFiles = Directory.GetFiles(folderPath);
                Array.Sort(folderFiles, StringComparer.OrdinalIgnoreCase);
                for (int index = 0; index < folderFiles.Length; index++)
                {
                    string file = folderFiles[index];
                    if (!HasAllowedExtension(file, allowedFileExtensions))
                    {
                        continue;
                    }

                    RuntimeFileBrowserItemType type = HasAllowedExtension(file, SupportedImageExtensions)
                        ? RuntimeFileBrowserItemType.ImageFile
                        : RuntimeFileBrowserItemType.File;
                    files.Add(new RuntimeFileBrowserItem(type, Path.GetFileName(file), file));
                }

                if (folders.Count == 0 && files.Count == 0)
                {
                    message = "Папка пуста или нет поддерживаемых изображений";
                }

                Debug.Log("[FileBrowser] Directories found=" + folders.Count);
                Debug.Log("[FileBrowser] Images found=" + files.Count);
                Debug.Log("[FileBrowser] Error=" + (string.IsNullOrWhiteSpace(message) ? "none" : message));
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                message = "Нет доступа к папке";
                Debug.LogWarning("[FileBrowser] Error=" + message);
                return false;
            }
            catch (Exception exception)
            {
                message = "Не удалось прочитать папку: " + exception.Message;
                Debug.LogWarning("[FileBrowser] Error=" + message);
                return false;
            }
        }

        public bool TryFindImages(string folderPath, out List<string> imagePaths, out string message)
        {
            imagePaths = new List<string>(256);
            message = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                {
                    message = "Папка не существует.";
                    return false;
                }

                string[] folderFiles = Directory.GetFiles(folderPath);
                Array.Sort(folderFiles, StringComparer.OrdinalIgnoreCase);
                for (int index = 0; index < folderFiles.Length; index++)
                {
                    string file = folderFiles[index];
                    if (HasAllowedExtension(file, SupportedImageExtensions))
                    {
                        imagePaths.Add(file);
                    }
                }

                if (imagePaths.Count == 0)
                {
                    message = "В папке нет поддерживаемых изображений.";
                    return false;
                }

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                message = "Нет доступа к папке";
                return false;
            }
            catch (Exception exception)
            {
                message = "Не удалось прочитать папку: " + exception.Message;
                return false;
            }
        }

        public static bool HasAllowedExtension(string filePath, IReadOnlyList<string> allowedExtensions)
        {
            if (string.IsNullOrWhiteSpace(filePath) || allowedExtensions == null)
            {
                return false;
            }

            string extension = Path.GetExtension(filePath);
            if (string.IsNullOrWhiteSpace(extension))
            {
                return false;
            }

            for (int index = 0; index < allowedExtensions.Count; index++)
            {
                if (string.Equals(extension, allowedExtensions[index], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void ScanDrives()
        {
            try
            {
                DriveInfo[] driveInfos = DriveInfo.GetDrives();
                for (int index = 0; index < driveInfos.Length; index++)
                {
                    DriveInfo drive = driveInfos[index];
                    if (drive == null)
                    {
                        continue;
                    }

                    string root = drive.Name;
                    drives.Add(new RuntimeFileBrowserItem(RuntimeFileBrowserItemType.Drive, root.TrimEnd('\\'), root));
                }
            }
            catch
            {
                // Drive list is optional. Folder navigation still works through direct paths.
            }
        }
    }
}
