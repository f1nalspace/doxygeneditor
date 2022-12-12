using System;

namespace TSP.DoxygenEditor.Services
{
    public interface IConfigurationConverter
    {
        object ConvertFromString(string value, Type type);
        string ConvertToString(object value);
    }
}
