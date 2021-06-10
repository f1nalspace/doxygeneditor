using ClangSharp.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FSMLexing
{
    class Program
    {
        public struct TextPosition
        {
            public int Index { get; set; }
            public int Line { get; set; }
            public int Column { get; set; }

            public override string ToString()
            {
                return $"Index: {Index}, Line: {Line + 1}, Col: {Column + 1}";
            }
        }

        public enum TokenKind
        {
            Unknown = 0,
            Number,
            Ident,
            Spaces,
            Tab,
            Linebreak,

            Add,
            AddAssign,

            Assign,
            LogicalEquals,

            Negate,
            LogicalNot,

            Divide,

            SingleLineComment,
        }

        public struct Token
        {
            public TokenKind Kind { get; set; }
            public TextPosition Position { get; set; }
            public int Length { get; set; }
            public string Value { get; set; }

            public override string ToString()
            {
                string v = Value;
                v = v.Replace("\n", "\\{n}");
                v = v.Replace("\r", "\\{r}");
                v = v.Replace("\t", "\\{t}");
                v = v.Replace("\f", "\\{f}");
                v = v.Replace("\v", "\\{v}");
                return $"{Position}, Length: {Length}, Kind: {Kind} = {v}";
            }
        }

        class FSMLexer
        {
            enum EquivalenceClass : int
            {
                Invalid = -1,       // Invalid
                EOF = 0,            // End-of-file

                Char,               // Any char

                Letter,             // a-z A-Z
                Underscore,         // _
                Digit,              // 0-9
                Space,              //   
                Tab,                // \t
                LF,                 // \n
                CR,                 // \r

                Plus,               // +
                Minus,              // -
                Star,               // *
                Slash,              // /
                Equals,             // =
                Lesser,             // <
                Greater,            // >
                Pipe,               // |
                Ampersand,          // &

                ExclamationMark,    // !

            }

            private static readonly Dictionary<char, EquivalenceClass> SpecialEquivalentCharMap = new Dictionary<char, EquivalenceClass>() {
                { '_', EquivalenceClass.Underscore },

                { 'a', EquivalenceClass.Letter },
                { 'b', EquivalenceClass.Letter },
                { 'c', EquivalenceClass.Letter },
                { 'd', EquivalenceClass.Letter },
                { 'e', EquivalenceClass.Letter },
                { 'f', EquivalenceClass.Letter },
                { 'g', EquivalenceClass.Letter },
                { 'h', EquivalenceClass.Letter },
                { 'i', EquivalenceClass.Letter },
                { 'j', EquivalenceClass.Letter },
                { 'k', EquivalenceClass.Letter },
                { 'l', EquivalenceClass.Letter },
                { 'm', EquivalenceClass.Letter },
                { 'n', EquivalenceClass.Letter },
                { 'o', EquivalenceClass.Letter },
                { 'p', EquivalenceClass.Letter },
                { 'q', EquivalenceClass.Letter },
                { 'r', EquivalenceClass.Letter },
                { 's', EquivalenceClass.Letter },
                { 't', EquivalenceClass.Letter },
                { 'u', EquivalenceClass.Letter },
                { 'v', EquivalenceClass.Letter },
                { 'w', EquivalenceClass.Letter },
                { 'x', EquivalenceClass.Letter },
                { 'y', EquivalenceClass.Letter },
                { 'z', EquivalenceClass.Letter },
                { 'A', EquivalenceClass.Letter },
                { 'B', EquivalenceClass.Letter },
                { 'C', EquivalenceClass.Letter },
                { 'D', EquivalenceClass.Letter },
                { 'E', EquivalenceClass.Letter },
                { 'F', EquivalenceClass.Letter },
                { 'G', EquivalenceClass.Letter },
                { 'H', EquivalenceClass.Letter },
                { 'I', EquivalenceClass.Letter },
                { 'J', EquivalenceClass.Letter },
                { 'K', EquivalenceClass.Letter },
                { 'L', EquivalenceClass.Letter },
                { 'M', EquivalenceClass.Letter },
                { 'N', EquivalenceClass.Letter },
                { 'O', EquivalenceClass.Letter },
                { 'P', EquivalenceClass.Letter },
                { 'Q', EquivalenceClass.Letter },
                { 'R', EquivalenceClass.Letter },
                { 'S', EquivalenceClass.Letter },
                { 'T', EquivalenceClass.Letter },
                { 'U', EquivalenceClass.Letter },
                { 'V', EquivalenceClass.Letter },
                { 'W', EquivalenceClass.Letter },
                { 'X', EquivalenceClass.Letter },
                { 'Y', EquivalenceClass.Letter },
                { 'Z', EquivalenceClass.Letter },

                { '0', EquivalenceClass.Digit },
                { '1', EquivalenceClass.Digit },
                { '2', EquivalenceClass.Digit },
                { '3', EquivalenceClass.Digit },
                { '4', EquivalenceClass.Digit },
                { '5', EquivalenceClass.Digit },
                { '6', EquivalenceClass.Digit },
                { '7', EquivalenceClass.Digit },
                { '8', EquivalenceClass.Digit },
                { '9', EquivalenceClass.Digit },

                { ' ', EquivalenceClass.Space },
                { '\t', EquivalenceClass.Tab },
                { '\n', EquivalenceClass.LF },
                { '\r', EquivalenceClass.CR },

                { '+', EquivalenceClass.Plus },
                { '=', EquivalenceClass.Equals },
                { '!', EquivalenceClass.ExclamationMark },
                { '/', EquivalenceClass.Slash },
                { '*', EquivalenceClass.Star },
            };

            enum State
            {
                // Last states must be before start
                Failed = 0,
                EOF,
                Done,

                DoneAfterNext, // Regardless what comes next, the token is finished
                Start,
                Number,
                Ident,
                Space,
                Tab,
                CRStart,
                LFStart,

                Plus,
                PlusEquals,

                Equals,
                DoubleEquals,

                ExclamationMark,
                ExclamationMarkEquals,

                Slash,
                SingleLineComment,
            }

            private const int ASCIIStart = 10;
            private const int ASCIIEnd = 126;

            private const int UnicodeStart = 256;
            private const int UnicodeEnd = 65535;

            private const int ColumnsPerTab = 4;

            // Mapping any char to a EquivalenceClass
            private static readonly EquivalenceClass[] equivalenceCharMap = new EquivalenceClass[char.MaxValue];

            private static readonly int stateCount = Enum.GetNames(typeof(State)).Length;
            private static readonly int equivalenceClassCount = Enum.GetNames(typeof(EquivalenceClass)).Length;
            private static int[] columnCount = new int[equivalenceClassCount];
            private static int[] lineBreakStartCount = new int[stateCount];

            private readonly State[,] transitions = new State[stateCount, equivalenceClassCount];

            private TokenKind[] tokenKinds = null;

            static FSMLexer()
            {
                equivalenceCharMap[0] = EquivalenceClass.Invalid;
                for (int i = 1; i < char.MaxValue; ++i)
                {
                    equivalenceCharMap[i] = EquivalenceClass.Char;
                    if (SpecialEquivalentCharMap.TryGetValue((char)i, out EquivalenceClass eqClass))
                        equivalenceCharMap[i] = eqClass;
                }
                for (int state = 1; state < stateCount; ++state)
                    lineBreakStartCount[state] = ((State)state == State.CRStart || (State)state == State.LFStart) ? 1 : 0;
                for (int eqCls = 1; eqCls < equivalenceClassCount; ++eqCls)
                    columnCount[eqCls] = (EquivalenceClass.Tab == (EquivalenceClass)eqCls) ? ColumnsPerTab : 1;
                columnCount[(int)EquivalenceClass.LF] = 0;
                columnCount[(int)EquivalenceClass.CR] = 0;
            }

            public FSMLexer()
            {
            }

#if false
            private void SetEquivalentClass(char ch, EquivalenceClass eqCls)
            {
                equivalenceCharMap[(int)ch] = eqCls;
            }
            private void SetEquivalentClasses(Func<char, bool> inFunc, EquivalenceClass eqCls)
            {
                for (int i = 0; i < equivalenceClassCount; ++i)
                {
                    if (inFunc((char)i))
                        equivalenceCharMap[i] = eqCls;
                }
            }
            private void SetTransition(State fromState, EquivalenceClass eqCls, State toState)
            {
                transitions[(int)fromState, (int)eqCls] = toState;
            }
            private void SetTransitions(State fromState, State toState)
            {
                for (int i = 0; i < equivalenceClassCount; ++i)
                    transitions[(int)fromState, i] = toState;
            }
            private void SetTransitions(State fromState, Func<EquivalenceClass, bool> func, State toState)
            {
                for (int i = 0; i < equivalenceClassCount; ++i)
                {
                    EquivalenceClass eqCls = (EquivalenceClass)i;
                    if (func(eqCls))
                        transitions[(int)fromState, i] = toState;
                }
            }
            private void SetTransitions(State fromState, HashSet<EquivalenceClass> eqClasses, State toState, State? elseState = null)
            {
                for (int i = 0; i < equivalenceClassCount; ++i)
                {
                    EquivalenceClass eqCls = (EquivalenceClass)i;
                    if (eqClasses.Contains(eqCls))
                        transitions[(int)fromState, i] = toState;
                    else if (elseState.HasValue)
                        transitions[(int)fromState, i] = elseState.Value;
                }
            }
            private void SetTransitions(State fromState, EquivalenceClass[] eqClassesArray, State toState, State? elseState = null)
            {
                HashSet<EquivalenceClass> equivalenceClassesSet = new HashSet<EquivalenceClass>();
                foreach (EquivalenceClass eqCls in eqClassesArray)
                    equivalenceClassesSet.Add(eqCls);
                SetTransitions(fromState, equivalenceClassesSet, toState, elseState);
            }

            public static bool IsSpacing(char c)
            {
                bool result = c == ' ' || c == '\f' || c == '\v';
                return (result);
            }

            private void BuildTables()
            {
                tokenKinds = new TokenKind[stateCount];
                for (int stateIndex = 0; stateIndex < stateCount; ++stateIndex)
                    tokenKinds[stateIndex] = TokenKind.Unknown;

                // Equivalent classes
                equivalenceCharMap = new EquivalenceClass[256];
                for (int charIndex = 0; charIndex < 256; ++charIndex)
                    equivalenceCharMap[charIndex] = EquivalenceClass.Invalid;
                equivalenceCharMap['\0'] = EquivalenceClass.EOF;

                SetEquivalentClasses(char.IsDigit, EquivalenceClass.Digit);
                SetEquivalentClasses(char.IsLetter, EquivalenceClass.Letter);
                SetEquivalentClass('_', EquivalenceClass.Letter);
                SetEquivalentClass('\r', EquivalenceClass.CR);
                SetEquivalentClass('\n', EquivalenceClass.LF);
                SetEquivalentClass('\t', EquivalenceClass.Tab);
                SetEquivalentClasses(IsSpacing, EquivalenceClass.Space);
                SetEquivalentClass('+', EquivalenceClass.Plus);
                SetEquivalentClass('=', EquivalenceClass.Equals);
                SetEquivalentClass('!', EquivalenceClass.ExclamationMark);
                SetEquivalentClass('/', EquivalenceClass.Slash);
                SetEquivalentClass('*', EquivalenceClass.Star);

                // Counts
                columnCount = new int[equivalenceClassCount];
                lineBreakStartCount = new int[stateCount];
                for (int state = 1; state < stateCount; ++state)
                    lineBreakStartCount[state] = ((State)state == State.CRStart || (State)state == State.LFStart) ? 1 : 0;
                for (int eqCls = 1; eqCls < equivalenceClassCount; ++eqCls)
                    columnCount[eqCls] = (EquivalenceClass.Tab == (EquivalenceClass)eqCls) ? ColumnsPerTab : 1;
                columnCount[(int)EquivalenceClass.LF] = 0;
                columnCount[(int)EquivalenceClass.CR] = 0;

                // Transitions
                transitions = new State[stateCount, equivalenceClassCount];
                for (int stateIndex = 0; stateIndex < stateCount; ++stateIndex)
                    for (int eqCls = 0; eqCls < equivalenceClassCount; ++eqCls)
                        transitions[stateIndex, eqCls] = State.Failed;

                for (int stateIndex = 0; stateIndex < stateCount; ++stateIndex)
                    transitions[stateIndex, (int)EquivalenceClass.EOF] = State.EOF;

                for (int eqCls = 0; eqCls < equivalenceClassCount; ++eqCls)
                {
                    transitions[(int)State.EOF, eqCls] = State.EOF;
                    transitions[(int)State.DoneAfterNext, eqCls] = State.Done;
                }

                // Numbers
                SetTransition(State.Start, EquivalenceClass.Digit, State.Number);
                SetTransitions(State.Number, new[] { EquivalenceClass.Digit }, State.Number, State.Done);
                tokenKinds[(int)State.Number] = TokenKind.Number;

                // Idents
                SetTransitions(State.Start, new[] { EquivalenceClass.Letter, EquivalenceClass.Underscore }, State.Ident);
                SetTransitions(State.Ident, new[] { EquivalenceClass.Letter, EquivalenceClass.Digit, EquivalenceClass.Underscore }, State.Ident, State.Done);
                tokenKinds[(int)State.Ident] = TokenKind.Ident;

                // Spaces
                SetTransition(State.Start, EquivalenceClass.Space, State.Space);
                SetTransitions(State.Space, new[] { EquivalenceClass.Space }, State.Space, State.Done);
                tokenKinds[(int)State.Space] = TokenKind.Spaces;

                // Tab
                SetTransition(State.Start, EquivalenceClass.Tab, State.Tab);
                SetTransitions(State.Tab, State.Done);
                tokenKinds[(int)State.Tab] = TokenKind.Tab;

                // + / +=
                SetTransition(State.Start, EquivalenceClass.Plus, State.Plus);
                SetTransitions(State.Plus, State.Done);
                SetTransition(State.Plus, EquivalenceClass.Equals, State.PlusEquals);
                SetTransitions(State.PlusEquals, State.Done);
                tokenKinds[(int)State.PlusEquals] = TokenKind.AddAssign;
                tokenKinds[(int)State.Plus] = TokenKind.Add;

                // = / ==
                SetTransition(State.Start, EquivalenceClass.Equals, State.Equals);
                SetTransitions(State.Equals, State.Done);
                SetTransition(State.Equals, EquivalenceClass.Equals, State.DoubleEquals);
                SetTransitions(State.DoubleEquals, State.Done);
                tokenKinds[(int)State.DoubleEquals] = TokenKind.LogicalEquals;
                tokenKinds[(int)State.Equals] = TokenKind.Assign;

                // ! / !=
                SetTransition(State.Start, EquivalenceClass.ExclamationMark, State.ExclamationMark);
                SetTransitions(State.ExclamationMark, State.Done);
                SetTransition(State.ExclamationMark, EquivalenceClass.Equals, State.ExclamationMarkEquals);
                SetTransitions(State.ExclamationMarkEquals, State.Done);
                tokenKinds[(int)State.ExclamationMarkEquals] = TokenKind.LogicalNot;
                tokenKinds[(int)State.ExclamationMark] = TokenKind.Negate;

                // Linebreak
                SetTransition(State.Start, EquivalenceClass.CR, State.CRStart);
                SetTransition(State.Start, EquivalenceClass.LF, State.LFStart);
                SetTransitions(State.CRStart, (c) => c != EquivalenceClass.LF, State.Done);
                SetTransitions(State.LFStart, (c) => c != EquivalenceClass.CR, State.Done);
                SetTransition(State.CRStart, EquivalenceClass.LF, State.DoneAfterNext);
                SetTransition(State.LFStart, EquivalenceClass.CR, State.DoneAfterNext);
                tokenKinds[(int)State.CRStart] = TokenKind.Linebreak;
                tokenKinds[(int)State.LFStart] = TokenKind.Linebreak;

                // Comments
                SetTransition(State.Start, EquivalenceClass.Slash, State.Slash);
                SetTransitions(State.Slash, State.Done);
                SetTransition(State.Slash, EquivalenceClass.Slash, State.SingleLineComment);
                SetTransitions(State.SingleLineComment, (c) => (c != EquivalenceClass.CR && c != EquivalenceClass.LF && c != EquivalenceClass.EOF), State.Done);
                tokenKinds[(int)State.Slash] = TokenKind.Divide;
                tokenKinds[(int)State.SingleLineComment] = TokenKind.SingleLineComment;
            }
#endif

            public bool NextToken(string source, ref TextPosition pos, ref Token outToken)
            {
                Debug.Assert(source.EndsWith("\0"));

                TextPosition tokenStart = pos;
                TokenKind tokenKind = TokenKind.Unknown;

                State state = State.Start;
                int ch;
                EquivalenceClass eqCls;
                State prevState;

                int lastIndex;
                do
                {
                    lastIndex = pos.Index;

                    ch = source[pos.Index++];
                    int charIndex = (int)ch;
                    Debug.Assert(charIndex >= 0 && charIndex < equivalenceCharMap.Length);
                    eqCls = equivalenceCharMap[charIndex];
                    prevState = state;
                    state = transitions[(int)prevState, (int)eqCls];
                    if (state == State.Done) break;
                    if (state == State.Failed)
                    {
                        Console.Error.WriteLine($"State change failed for previous state: {prevState}, newState: {state}, eqCls: {eqCls}, char: '{ch}'");
                        break;
                    }

                    int columnsOffset = columnCount[(int)eqCls];
                    int linesOffset = lineBreakStartCount[(int)state];
                    pos.Column += columnsOffset;
                    if (linesOffset > 0)
                    {
                        pos.Column = 0;
                        pos.Line += linesOffset;
                    }

                    if (state != prevState)
                    {
                        //TokenKind newKind = tokenKinds[(int)state];
                        //if (tokenKind == TokenKind.Unknown || newKind > tokenKind)
                        //    tokenKind = newKind;
                    }
                } while (state > State.Done);

                if (state == State.Done)
                    pos.Index = lastIndex;

                outToken.Kind = tokenKind;
                outToken.Position = tokenStart;
                outToken.Length = (pos.Index - tokenStart.Index);
                outToken.Value = source.Substring(outToken.Position.Index, outToken.Length);

                return (pos.Index < source.Length) && state != State.Failed;
            }
        }

        static void Main(string[] args)
        {
            //string source = "42  abc\t_myAw3someIdent\n\t123+56\r\nThis is it!9+=25=3==5\0";
            string source = "// hallo welt\n\0";
            FSMLexer lexer = new FSMLexer();
            Token token = new Token();
            TextPosition pos = new TextPosition();
            while (lexer.NextToken(source, ref pos, ref token))
            {
                Console.WriteLine(token.ToString());
            }
            Console.ReadKey();
        }
    }
}