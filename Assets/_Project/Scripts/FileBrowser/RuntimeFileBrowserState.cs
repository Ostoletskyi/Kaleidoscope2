using System.Collections.Generic;

namespace Kaleidoscope2.FileBrowser
{
    public sealed class RuntimeFileBrowserState
    {
        private readonly List<RuntimeFileBrowserItem> folders = new List<RuntimeFileBrowserItem>(128);
        private readonly List<RuntimeFileBrowserItem> files = new List<RuntimeFileBrowserItem>(256);
        private readonly List<RuntimeFileBrowserItem> drives = new List<RuntimeFileBrowserItem>(16);

        public string CurrentPath { get; private set; }
        public string Message { get; private set; }
        public bool HasError { get; private set; }

        public IReadOnlyList<RuntimeFileBrowserItem> Folders
        {
            get { return folders; }
        }

        public IReadOnlyList<RuntimeFileBrowserItem> Files
        {
            get { return files; }
        }

        public IReadOnlyList<RuntimeFileBrowserItem> Drives
        {
            get { return drives; }
        }

        public int ImageCount
        {
            get { return files.Count; }
        }

        public void SetCurrentPath(string path)
        {
            CurrentPath = path ?? string.Empty;
        }

        public void SetMessage(string message, bool isError)
        {
            Message = message ?? string.Empty;
            HasError = isError;
        }

        public void SetItems(
            IReadOnlyList<RuntimeFileBrowserItem> scannedFolders,
            IReadOnlyList<RuntimeFileBrowserItem> scannedFiles,
            IReadOnlyList<RuntimeFileBrowserItem> scannedDrives)
        {
            folders.Clear();
            files.Clear();
            drives.Clear();

            AddRange(folders, scannedFolders);
            AddRange(files, scannedFiles);
            AddRange(drives, scannedDrives);
        }

        public void ClearItems()
        {
            folders.Clear();
            files.Clear();
        }

        private static void AddRange(List<RuntimeFileBrowserItem> target, IReadOnlyList<RuntimeFileBrowserItem> source)
        {
            if (source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                target.Add(source[index]);
            }
        }
    }
}
