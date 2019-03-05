using System;

namespace TSP.DoxygenEditor.Lexers
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TokenKindAttribute : Attribute
    {
        public TokenKindAttribute() { }
        public string Text { get; set; }
    }
}
