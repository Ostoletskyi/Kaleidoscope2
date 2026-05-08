using System;
using System.IO;

namespace Kaleidoscope2.FileBrowser
{
    public sealed class RuntimePathValidator
    {
        public bool TryNormalizeDirectoryPath(string path, out string normalizedPath, out string message)
        {
            normalizedPath = string.Empty;
            message = string.Empty;

            if (string.IsNullOrWhiteSpace(path))
            {
                message = "Путь не указан.";
                return false;
            }

            try
            {
                string candidate = path.Trim();
                if (candidate.Length == 2 && char.IsLetter(candidate[0]) && candidate[1] == ':')
                {
                    candidate += Path.DirectorySeparatorChar;
                }

                if (File.Exists(candidate))
                {
                    candidate = Path.GetDirectoryName(candidate);
                }

                if (string.IsNullOrWhiteSpace(candidate))
                {
                    message = "Не удалось определить папку.";
                    return false;
                }

                string fullPath = Path.GetFullPath(candidate);
                if (!Directory.Exists(fullPath))
                {
                    message = "Папка не существует: " + fullPath;
                    return false;
                }

                normalizedPath = fullPath;
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                message = "Нет доступа к папке";
                return false;
            }
            catch (Exception exception)
            {
                message = "Некорректный путь: " + exception.Message;
                return false;
            }
        }
    }
}
