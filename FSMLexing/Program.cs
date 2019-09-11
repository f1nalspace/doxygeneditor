using System;
using System.Collections.Generic;
using System.Diagnostics;

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
            enum State
            {
                // Last states must be before start
                Failed = 0,
                EOF,
                Done,

                DoneAfterNext,
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
                ExclamationMarkEquals
            }

            enum EquivalenceClass
            {
                Invalid = 0,
                EOF,
                Letter,
                Underscore,
                Digit,
                Space,
                Tab,
                LF,
                CR,
                Plus,
                Equals,
                ExclamationMark,
            }

            private readonly int stateCount;
            private readonly int equivalenceClassCount;
            private State[,] transitions = null;
            private EquivalenceClass[] equivalenceClasses = null;
            private TokenKind[] tokenKinds = null;
            private int[] columnCount = null;
            private int[] lineBreakStartCount = null;

            private int columnsPerTab = 4;

            public FSMLexer()
            {
                stateCount = Enum.GetNames(typeof(State)).Length;
                equivalenceClassCount = Enum.GetNames(typeof(EquivalenceClass)).Length;
                BuildTables();
            }

            private void SetEquivalentClass(char ch, EquivalenceClass eqCls)
            {
                equivalenceClasses[(int)ch] = eqCls;
            }
            private void SetEquivalentClasses(Func<char, bool> inFunc, EquivalenceClass eqCls)
            {
                for (int i = 0; i < 256; ++i)
                {
                    if (inFunc((char)i))
                        equivalenceClasses[i] = eqCls;
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
                equivalenceClasses = new EquivalenceClass[256];
                for (int charIndex = 0; charIndex < 256; ++charIndex)
                    equivalenceClasses[charIndex] = EquivalenceClass.Invalid;
                equivalenceClasses['\0'] = EquivalenceClass.EOF;

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

                // Counts
                columnCount = new int[equivalenceClassCount];
                lineBreakStartCount = new int[stateCount];
                for (int state = 1; state < stateCount; ++state)
                    lineBreakStartCount[state] = ((State)state == State.CRStart || (State)state == State.LFStart) ? 1 : 0;
                for (int eqCls = 1; eqCls < equivalenceClassCount; ++eqCls)
                    columnCount[eqCls] = (EquivalenceClass.Tab == (EquivalenceClass)eqCls) ? columnsPerTab : 1;
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
            }

            public bool NextToken(string source, ref TextPosition pos, ref Token outToken)
            {
                Debug.Assert(source.EndsWith("\0"));

                TextPosition tokenStart = pos;
                TokenKind tokenKind = TokenKind.Unknown;

                State state = State.Start;
                int ch = 0;
                EquivalenceClass eqCls = EquivalenceClass.Invalid;
                State prevState = State.Start;

                int lastIndex;
                do
                {
                    lastIndex = pos.Index;

                    ch = source[pos.Index++];
                    eqCls = equivalenceClasses[(int)ch];
                    prevState = state;
                    state = transitions[(int)prevState, (int)eqCls];
                    if (state == State.Done) break;

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
                        TokenKind newKind = tokenKinds[(int)state];
                        if (tokenKind == TokenKind.Unknown || newKind > tokenKind)
                            tokenKind = newKind;
                    }
                } while (state > State.Done);

                if (state == State.Done)
                    pos.Index = lastIndex;

                outToken.Kind = tokenKind;
                outToken.Position = tokenStart;
                outToken.Length = (pos.Index - tokenStart.Index);
                outToken.Value = source.Substring(outToken.Position.Index, outToken.Length);

                return (pos.Index < source.Length);
            }
        }

        static void Main(string[] args)
        {
            string source = "42  abc\t_myAw3someIdent\n\t123+56\r\nThis is it!9+=25=3==5\0";
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
