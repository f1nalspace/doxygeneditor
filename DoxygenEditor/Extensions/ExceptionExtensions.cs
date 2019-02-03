using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DoxygenEditor.Extensions
{
    static class ExceptionExtensions
    {
        private const int ERROR_SHARING_VIOLATION = 32;
        private const int ERROR_FILE_EXISTS = 80;
        private const int ERROR_ALREADY_EXISTS = 183;

        private static Dictionary<int, string> hResultToStringMapping = new Dictionary<int, string>()
        {
            { ERROR_SHARING_VIOLATION, "The file '%FILE%' is already open!" },
            { ERROR_FILE_EXISTS, "The file '%FILE%' already exists!" },
            { ERROR_ALREADY_EXISTS, "The file or path '%FILE%' already exists!" },
        };

        public static string ToHumanReadable(this Exception exception, string filePath)
        {
            StringBuilder result = new StringBuilder();
            Type t = exception.GetType();
            if (typeof(IOException).IsAssignableFrom(t))
            {
                IOException io = (IOException)exception;
                bool hasPath = !string.IsNullOrEmpty(filePath);
                if (hasPath && typeof(FileNotFoundException).IsAssignableFrom(t))
                    result.Append($"File by path '{filePath}' could not be found!");
                else if (hasPath && typeof(DirectoryNotFoundException).IsAssignableFrom(t))
                    result.Append($"Directory by path '{filePath}' could not be found!");
                else if (hasPath && typeof(DriveNotFoundException).IsAssignableFrom(t))
                    result.Append($"Drive by path '{filePath}' could not be found!");
                else if (hasPath && typeof(EndOfStreamException).IsAssignableFrom(t))
                    result.Append($"Unexpected end of stream in file '{filePath}': {exception.Message}!");
                else if (hasPath && typeof(FileLoadException).IsAssignableFrom(t))
                    result.Append($"Failed to load file '{filePath}': {exception.Message}!");
                else if (hasPath && typeof(PathTooLongException).IsAssignableFrom(t))
                    result.Append($"The path '{filePath}' is too long: {exception.Message}!");
                else
                {
                    int code = io.HResult & 0x0000FFFF;
                    string r;
                    if (hResultToStringMapping.TryGetValue(code, out r))
                    {
                        r = r.Replace("%FILE%", filePath);
                        result.Append(r);
                    }
                    else
                    {
                        result.AppendLine($"An exception occurred");
                        result.AppendLine($"Error code: {code}");
                        result.Append($"Message: {exception.Message}");
                    }
                }
            }
            else if (typeof(UnauthorizedAccessException).IsAssignableFrom(t))
                result.Append($"You do not have permissions to access the file/path '{filePath}'!");
            else
                result.Append(exception.Message);
#if DEBUG
            result.AppendLine();
            result.AppendLine();
            result.AppendLine("[DEBUG] Stack trace:");
            result.Append(exception.StackTrace);
#endif
            return (result.ToString());

        }
    }
}
