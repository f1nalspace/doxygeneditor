using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TSP.DoxygenEditor.Lexers.Cpp
{
    public static class CppTokenTypeExtensions
    {
        private static readonly Dictionary<CppTokenType, string> _cppTokenTexts = new Dictionary<CppTokenType, string>();

        static CppTokenTypeExtensions()
        {
            foreach (var field in typeof(CppTokenType).GetTypeInfo().DeclaredFields.Where(field => field.IsPublic && field.IsStatic))
            {
                var tokenText = field.GetCustomAttribute<TokenTextAttribute>();
                if (tokenText != null)
                {
                    var type = (CppTokenType)field.GetValue(null);
                    _cppTokenTexts.Add(type, tokenText.Text);
                }
            }
        }

        public static bool HasText(this CppTokenType type)
        {
            return _cppTokenTexts.ContainsKey(type);
        }

        public static string ToText(this CppTokenType type)
        {
            string value;
            _cppTokenTexts.TryGetValue(type, out value);
            return value;
        }
    }
}
