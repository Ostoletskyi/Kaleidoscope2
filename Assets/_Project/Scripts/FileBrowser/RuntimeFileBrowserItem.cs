using System;

namespace Kaleidoscope2.FileBrowser
{
    public enum RuntimeFileBrowserItemType
    {
        ParentDirectory = 0,
        Drive = 1,
        Directory = 2,
        ImageFile = 3,
        File = 4,
        Header = 5,
        Message = 6
    }

    public sealed class RuntimeFileBrowserItem
    {
        public RuntimeFileBrowserItem(RuntimeFileBrowserItemType type, string displayName, string fullPath)
        {
            Type = type;
            DisplayName = displayName ?? string.Empty;
            FullPath = fullPath ?? string.Empty;
        }

        public RuntimeFileBrowserItemType Type { get; private set; }
        public string DisplayName { get; private set; }
        public string FullPath { get; private set; }

        public bool IsSelectable
        {
            get
            {
                return Type == RuntimeFileBrowserItemType.ParentDirectory
                    || Type == RuntimeFileBrowserItemType.Drive
                    || Type == RuntimeFileBrowserItemType.Directory
                    || Type == RuntimeFileBrowserItemType.ImageFile
                    || Type == RuntimeFileBrowserItemType.File;
            }
        }

        public static RuntimeFileBrowserItem Header(string label)
        {
            return new RuntimeFileBrowserItem(RuntimeFileBrowserItemType.Header, label, string.Empty);
        }

        public static RuntimeFileBrowserItem Message(string message)
        {
            return new RuntimeFileBrowserItem(RuntimeFileBrowserItemType.Message, message, string.Empty);
        }
    }
}
