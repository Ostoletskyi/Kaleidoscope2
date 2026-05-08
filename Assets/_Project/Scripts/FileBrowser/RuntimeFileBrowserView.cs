using System.Collections.Generic;
using UnityEngine;

namespace Kaleidoscope2.FileBrowser
{
    public sealed class RuntimeFileBrowserView
    {
        private readonly List<RuntimeFileBrowserItem> rows = new List<RuntimeFileBrowserItem>(512);

        public IReadOnlyList<RuntimeFileBrowserItem> BuildRows(RuntimeFileBrowserState state, string fileSectionTitle)
        {
            rows.Clear();

            if (state == null)
            {
                rows.Add(RuntimeFileBrowserItem.Message("Браузер не инициализирован."));
                return rows;
            }

            rows.Add(new RuntimeFileBrowserItem(RuntimeFileBrowserItemType.ParentDirectory, "..", state.CurrentPath));

            if (!string.IsNullOrWhiteSpace(state.Message))
            {
                rows.Add(RuntimeFileBrowserItem.Message(state.Message));
            }

            rows.Add(RuntimeFileBrowserItem.Header("Папки:"));

            IReadOnlyList<RuntimeFileBrowserItem> folders = state.Folders;
            for (int index = 0; index < folders.Count; index++)
            {
                RuntimeFileBrowserItem folder = folders[index];
                Debug.Log("[FileBrowserView] Create folder row: " + folder.DisplayName);
                rows.Add(new RuntimeFileBrowserItem(RuntimeFileBrowserItemType.Directory, folder.DisplayName, folder.FullPath));
            }

            if (folders.Count == 0 && state.Files.Count == 0 && string.IsNullOrWhiteSpace(state.Message))
            {
                rows.Add(RuntimeFileBrowserItem.Message("Папка пуста или нет поддерживаемых изображений"));
            }

            rows.Add(RuntimeFileBrowserItem.Header(string.IsNullOrWhiteSpace(fileSectionTitle) ? "Изображения:" : fileSectionTitle));

            IReadOnlyList<RuntimeFileBrowserItem> files = state.Files;
            for (int index = 0; index < files.Count; index++)
            {
                RuntimeFileBrowserItem file = files[index];
                Debug.Log("[FileBrowserView] Create image row: " + file.DisplayName);
                rows.Add(new RuntimeFileBrowserItem(file.Type, file.DisplayName, file.FullPath));
            }

            if (files.Count == 0)
            {
                rows.Add(RuntimeFileBrowserItem.Message("Нет поддерживаемых изображений."));
            }

            Debug.Log("[FileBrowserView] Total rows created=" + rows.Count);
            return rows;
        }
    }
}
