using System.Diagnostics;
using TSP.DoxygenEditor.Languages.Utils;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public class DoxygenConfigLexer : BaseLexer<DoxygenToken>
    {
        class DoxygenState : State
        {
            public override void StartLex(TextStream stream)
            {
            }
        }

        public DoxygenConfigLexer(string source, TextPosition pos, int length) : base(source, pos, length)
        {
        }

        protected override State CreateState()
        {
            return new DoxygenState();
        }

        private void LexValue()
        {
            Debug.Assert(SyntaxUtils.IsAlpha(Buffer.Peek()) || Buffer.Peek() == '@');

            Buffer.StartLexeme();
            if (Buffer.Peek() == '@')
                Buffer.AdvanceColumn();

            if (!SyntaxUtils.IsIdentStart(Buffer.Peek()))
            {
                AddError(Buffer.TextPosition, $"Requires identifier, but found '{Buffer.Peek()}'", "Value Key");
                return;
            }
            while (!Buffer.IsEOF)
            {
                if (!SyntaxUtils.IsIdentPart(Buffer.Peek()))
                    break;
                Buffer.AdvanceColumn();
            }
            PushToken(DoxygenTokenPool.Make(DoxygenTokenKind.ConfigKey, Buffer.LexemeRange, true));
            Buffer.SkipSpacings(TextStream.SkipType.All);

            char first = Buffer.Peek();
            char second = Buffer.Peek(1);

            DoxygenTokenKind opKind;
            Buffer.StartLexeme();
            if (first == '+' && second == '=')
            {
                Buffer.AdvanceColumns(2);
                opKind = DoxygenTokenKind.ConfigOpAddAssign;
            }
            else if (first == '=')
            {
                Buffer.AdvanceColumn();
                opKind = DoxygenTokenKind.ConfigOpAssign;
            }
            else
            {
                AddError(Buffer.TextPosition, $"Expect + or += operator, but found '{Buffer.Peek()}'", "Value Operator");
                return;
            }
            PushToken(DoxygenTokenPool.Make(opKind, Buffer.LexemeRange, true));
            Buffer.SkipSpacings(TextStream.SkipType.All);

            while (!Buffer.IsEOF)
            {
                // Value
                Buffer.StartLexeme();
                if (Buffer.Peek() == '"')
                {
                    Buffer.AdvanceColumn();
                    while (!Buffer.IsEOF)
                    {
                        if (Buffer.Peek() == '"')
                        {
                            Buffer.AdvanceColumn();
                            break;
                        }
                        Buffer.AdvanceAuto();
                    }
                }
                else if (Buffer.Peek() != '\\' && !char.IsWhiteSpace(Buffer.Peek()))
                {
                    while (!Buffer.IsEOF)
                    {
                        char c = Buffer.Peek();
                        if (char.IsWhiteSpace(c) || c == '\\')
                            break;
                        Buffer.AdvanceColumn();
                    }
                }
                if (Buffer.LexemeWidth>  0)
                    PushToken(DoxygenTokenPool.Make(DoxygenTokenKind.ConfigValue, Buffer.LexemeRange, true));

                bool allowNext = false;

                // Support for values separated by spaces (Filenames, Extensions, etc.)
                if (SyntaxUtils.IsSpacing(Buffer.Peek()))
                    allowNext = true;

                Buffer.SkipSpacings(TextStream.SkipType.All);

                if (Buffer.Peek() == '\\')
                {
                    Buffer.StartLexeme();
                    Buffer.AdvanceColumn();
                    PushToken(DoxygenTokenPool.Make(DoxygenTokenKind.ConfigOpAddLine, Buffer.LexemeRange, true));
                    if (!SyntaxUtils.IsLineBreak(Buffer.Peek()))
                    {
                        AddError(Buffer.TextPosition, $"Expect linebreak, but found '{Buffer.Peek()}'", "Next Value");
                        return;
                    }
                    Buffer.AdvanceLineAuto();
                    allowNext = true;
                }
                else if (SyntaxUtils.IsLineBreak(Buffer.Peek()))
                    Buffer.AdvanceLineAuto();

                if (!allowNext)
                    break;

                Buffer.SkipSpacings(TextStream.SkipType.All);
            }
        }

        protected override bool LexNext(State hiddenState)
        {
            DoxygenState state = (DoxygenState)hiddenState;
            do
            {
                char first = Buffer.Peek();
                switch (first)
                {
                    case ' ':
                    case '\v':
                    case '\f':
                    case '\t':
                        Buffer.SkipSpacings(TextStream.SkipType.All);
                        break;

                    case '\r':
                    case '\n':
                        Buffer.SkipLineBreaks(TextStream.SkipType.Single);
                        break;

                    case '#':
                        {
                            Buffer.StartLexeme();
                            Buffer.AdvanceColumn();
                            while (!Buffer.IsEOF)
                            {
                                if (SyntaxUtils.IsLineBreak(Buffer.Peek()))
                                    break;
                                Buffer.AdvanceAuto();
                            }
                            PushToken(DoxygenTokenPool.Make(DoxygenTokenKind.ConfigComment, Buffer.LexemeRange, true));
                        }
                        break;

                    default:
                        {
                            if (SyntaxUtils.IsAlpha(first) || first == '@')
                            {
                                int oldPos = Buffer.StreamPosition;
                                LexValue();
                                Debug.Assert(oldPos != Buffer.StreamPosition);
                                continue;
                            }

                            Buffer.AdvanceColumn();
                            break;
                        }
                }
            } while (!Buffer.IsEOF);
            PushToken(DoxygenTokenPool.Make(DoxygenTokenKind.EOF, new TextRange(Buffer.TextPosition, 0), false));
            return (false);
        }
    }
}
