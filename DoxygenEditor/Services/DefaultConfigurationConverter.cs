using System;
using System.Drawing;
using TSP.DoxygenEditor.Extensions;

namespace TSP.DoxygenEditor.Services
{
    class DefaultConfigurationConverter : IConfigurationConverter
    {
        public object ConvertFromString(string value, Type type)
        {
            if (typeof(string).Equals(type))
                return value;
            if (typeof(int).Equals(type))
                return int.Parse(value);
            if (typeof(bool).Equals(type))
                return "true".Equals(value, StringComparison.InvariantCultureIgnoreCase);
            if (typeof(Color).Equals(type))
            {
                Color color = ColorTranslator.FromHtml(value);
                return (color);
            }
            else
                throw new Exception($"Unsupported conversion type '{type}'");
        }

        public string ConvertToString(object value)
        {
            if (value != null)
            {
                Type t = value.GetType();
                if (typeof(string).Equals(t))
                    return (string)value;
                else if (typeof(int).Equals(t))
                    return value.ToString();
                else if (typeof(bool).Equals(t))
                {
                    bool boolValue = (bool)value;
                    return boolValue ? "true" : "false";
                }
                else if (typeof(Color).Equals(t))
                {
                    Color colorValue = (Color)value;
                    string colorString = colorValue.ToHexString();
                    return (colorString);
                }
                else
                    throw new Exception($"Unsupported conversion type '{t}'");
            }
            return (null);
        }
    }
}
