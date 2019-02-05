using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoxygenEditor.Lexers.Html
{
    class HtmlToken : BaseToken
    {
        public HtmlTokenType Type { get; }
        public override bool IsEOF => Type == HtmlTokenType.EOF;
        public override bool IsValid => Type != HtmlTokenType.Invalid;
        public HtmlToken(HtmlTokenType type, int index, int length, bool isComplete) : base(index, length, isComplete)
        {
            Type = type;
        }
    }
}
