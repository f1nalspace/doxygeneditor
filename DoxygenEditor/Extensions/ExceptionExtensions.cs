using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TSP.DoxygenEditor.Models;

namespace TSP.DoxygenEditor.Extensions
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

        class ValueStrings
        {
            public string FilePath { get; set; }

            public static ValueStrings FromDictionary(Dictionary<string, string> values)
            {
                ValueStrings result = new ValueStrings();
                result.FilePath = values.ContainsKey("filepath") ? values["filepath"] : null;
                return (result);
            }
        }

        public static ErrorMessageModel ToErrorMessage(this Exception exception, string caption, Dictionary<string, string> values)
        {
            StringBuilder shortText = new StringBuilder();
            Type t = exception.GetType();
            ValueStrings valueStrings = ValueStrings.FromDictionary(values);
            if (typeof(IOException).IsAssignableFrom(t))
            {
                IOException io = (IOException)exception;
                bool hasPath = !string.IsNullOrEmpty(valueStrings.FilePath);
                if (hasPath && typeof(FileNotFoundException).IsAssignableFrom(t))
                    shortText.Append($"File by path '{valueStrings.FilePath}' does not exists.");
                else if (hasPath && typeof(DirectoryNotFoundException).IsAssignableFrom(t))
                    shortText.Append($"Directory by path '{valueStrings.FilePath}' does not exists.");
                else if (hasPath && typeof(DriveNotFoundException).IsAssignableFrom(t))
                    shortText.Append($"Drive by path '{valueStrings.FilePath}' does not exists.");
                else if (hasPath && typeof(EndOfStreamException).IsAssignableFrom(t))
                    shortText.Append($"Unexpected end of stream in file '{valueStrings.FilePath}': {exception.Message}");
                else if (hasPath && typeof(FileLoadException).IsAssignableFrom(t))
                    shortText.Append($"Failed to load file '{valueStrings.FilePath}:'{Environment.NewLine}{exception.Message}");
                else if (hasPath && typeof(PathTooLongException).IsAssignableFrom(t))
                    shortText.Append($"The path '{valueStrings.FilePath}' is too long:{Environment.NewLine}{exception.Message}");
                else
                    shortText.Append(exception.Message);
            }
            else if (typeof(UnauthorizedAccessException).IsAssignableFrom(t))
                shortText.Append($"You do not have permissions to access the file/path '{valueStrings.FilePath}'");
            else
                shortText.Append(exception.Message);
            ErrorMessageModel result = new ErrorMessageModel(caption, shortText.ToString())
            {
                Details = exception.ToHumanReadable(values)
            };
            return (result);
        }

        private static string ToHumanReadable(this Exception exception, Dictionary<string, string> values)
        {
            StringBuilder result = new StringBuilder();
            Type t = exception.GetType();
            ValueStrings valueStrings = ValueStrings.FromDictionary(values);
            if (typeof(IOException).IsAssignableFrom(t))
            {
                IOException io = (IOException)exception;
                bool hasPath = !string.IsNullOrEmpty(valueStrings.FilePath);
                if (hasPath && typeof(FileNotFoundException).IsAssignableFrom(t))
                    result.Append($"File by path '{valueStrings.FilePath}' does not exists.");
                else if (hasPath && typeof(DirectoryNotFoundException).IsAssignableFrom(t))
                    result.Append($"Directory by path '{valueStrings.FilePath}' does not exists.");
                else if (hasPath && typeof(DriveNotFoundException).IsAssignableFrom(t))
                    result.Append($"Drive by path '{valueStrings.FilePath}' does not exists.");
                else if (hasPath && typeof(EndOfStreamException).IsAssignableFrom(t))
                    result.Append($"Unexpected end of stream in file '{valueStrings.FilePath}': {exception.Message}.");
                else if (hasPath && typeof(FileLoadException).IsAssignableFrom(t))
                    result.Append($"Failed to load file '{valueStrings.FilePath}': {exception.Message}.");
                else if (hasPath && typeof(PathTooLongException).IsAssignableFrom(t))
                    result.Append($"The path '{valueStrings.FilePath}' is too long: {exception.Message}.");
                else
                {
                    int code = io.HResult & 0x0000FFFF;
                    string r;
                    if (hResultToStringMapping.TryGetValue(code, out r))
                    {
                        r = r.Replace("%FILE%", valueStrings.FilePath);
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
                result.Append($"You do not have permissions to access the file/path '{valueStrings.FilePath}'!");
            else
                result.Append(exception.Message);
            result.AppendLine();
            result.AppendLine();
            result.AppendLine("Stack trace:");
            result.Append(exception.StackTrace);
            return (result.ToString());

        }
    }
}
