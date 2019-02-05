using DoxygenEditor.Models;
using System;
using System.IO;

namespace DoxygenEditor.Extensions
{
    static class ErrorMessageModelExtensions
    {
        public static Tuple<string, string> ToFileError(this ErrorMessageModel err, string filePath)
        {
            string fileName = !string.IsNullOrWhiteSpace(filePath) ? Path.GetFileName(filePath) : null;
            string text = err.Caption.Replace("%FILENAME%", fileName).Replace("%FILEPATH%", filePath);
            string caption = err.Text.Replace("%FILENAME%", fileName).Replace("%FILEPATH%", filePath);
            Tuple<string, string> result = new Tuple<string, string>(text, caption);
            return (result);
        }
    }
}
