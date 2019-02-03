using System;

namespace DoxygenEditor.Lexers
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TokenTextAttribute : Attribute
    {
        public string Text { get; }
        public TokenTextAttribute(string text)
        {
            Text = text;
        }
    }
}
