using TSP.DoxygenEditor.Models;
using System;
using System.IO;

namespace TSP.DoxygenEditor.Extensions
{
    static class ErrorMessageModelExtensions
    {
        public static Tuple<string, string> ToFileError(this ErrorMessageModel err, string filePath)
        {
            string fileName = !string.IsNullOrWhiteSpace(filePath) ? Path.GetFileName(filePath) : null;
            string caption = err.Caption.Replace("%FILENAME%", fileName).Replace("%FILEPATH%", filePath);
            string shortText = err.ShortText.Replace("%FILENAME%", fileName).Replace("%FILEPATH%", filePath);
            Tuple<string, string> result = new Tuple<string, string>(caption, shortText);
            return (result);
        }
    }
}
