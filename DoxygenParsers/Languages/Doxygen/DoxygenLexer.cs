using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TSP.DoxygenEditor.Languages.Utils;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public class DoxygenLexer : BaseLexer<DoxygenToken>
    {
        public DoxygenLexer(string source, TextPosition pos, int length) : base(source, pos, length)
        {

        }

        class CommandResultArgument
        {
            public DoxygenSyntax.ArgumentRule ArgRule { get; }
            public string Name => ArgRule.Name;
            public string Value { get; }
            public CommandResultArgument(DoxygenSyntax.ArgumentRule argRule, string value)
            {
                Value = value;
            }
        }

        class CommandResult
        {
            public TextPosition StartPos { get; }
            public DoxygenSyntax.CommandRule Rule { get; }
            public string CommandName { get; }
            public bool IsValid { get; set; }
            public List<CommandResultArgument> Arguments { get; }
            public DoxygenSyntax.CommandKind? Kind => Rule?.Kind;
            public CommandResult(TextPosition startPos, DoxygenSyntax.CommandRule rule = null, string commandName = null)
            {
                StartPos = startPos;
                Rule = rule;
                CommandName = commandName;
                Arguments = new List<CommandResultArgument>();
            }
        }

        private CommandResult LexCommandTokens()
        {
            Debug.Assert(DoxygenSyntax.IsCommandBegin(Buffer.Peek()));
            Debug.Assert(DoxygenSyntax.IsCommandIdentStart(Buffer.Peek(1)));

            // Command
            Buffer.StartLexeme();
            Buffer.AdvanceColumn();
            StringBuilder commandString = new StringBuilder();

            DoxygenTokenKind type = DoxygenTokenKind.Command;
            if (DoxygenSyntax.SpecialCommandStartChars.Contains(Buffer.Peek()))
            {
                // Special case for { } command
                if (Buffer.Peek() == '{' || Buffer.Peek() == '}')
                {
                    type = DoxygenTokenKind.GroupStart;
                    if (Buffer.Peek() == '}')
                        type = DoxygenTokenKind.GroupEnd;
                }
                // All other special case
                while (!Buffer.IsEOF)
                {
                    if (DoxygenSyntax.SpecialCommandStartChars.Contains(Buffer.Peek()))
                    {
                        commandString.Append(Buffer.Peek());
                        Buffer.AdvanceColumn();
                    }
                    else
                        break;
                }
            }
            else
            {
                // Normal case
                while (!Buffer.IsEOF)
                {
                    if (DoxygenSyntax.IsCommandIdent(Buffer.Peek()))
                    {
                        commandString.Append(Buffer.Peek());
                        Buffer.AdvanceColumn();
                    }
                    else
                        break;
                }
            }

            TextPosition commandStart = Buffer.LexemeStart;
            int commandLen = Buffer.LexemeWidth;
            string commandName = commandString.ToString();
            var rule = DoxygenSyntax.GetCommandRule(commandName);
            if (rule != null)
            {
                if (rule.Kind == DoxygenSyntax.CommandKind.StartCommandBlock)
                    type = DoxygenTokenKind.CommandStart;
                else if (rule.Kind == DoxygenSyntax.CommandKind.EndCommandBlock)
                    type = DoxygenTokenKind.CommandEnd;
            }
            else
            {
                // @NOTE(final): Group start/end are not a "known" command
                if (type != DoxygenTokenKind.GroupStart && type != DoxygenTokenKind.GroupEnd)
                    type = DoxygenTokenKind.InvalidCommand;
            }
            DoxygenToken commandToken = DoxygenTokenPool.Make(type, Buffer.LexemeRange, true);
            PushToken(commandToken);

            CommandResult result = new CommandResult(commandStart, rule, commandName);

            if (rule != null)
            {
                int argNumber = 0;
                int argCount = rule.Args.Count();
                foreach (var arg in rule.Args)
                {
                    // @TODO(final): Handle rule repeat type for arguments on same type
                    char first = Buffer.Peek();
                    if (!arg.Flags.HasFlag(DoxygenSyntax.ArgumentFlags.DirectlyAfterCommand))
                    {
                        if (SyntaxUtils.IsSpacing(first) || first == '\t')
                            SkipSpacings(SkipType.All);
                        else
                        {
                            // No more arguments are following
                            if (arg.IsRequired)
                            {
                                PushError(Buffer.TextPosition, $"Expected spacing character, but found '{Buffer.Peek()}' found for argument ({argNumber}:{arg}) in command '{commandName}'");
                                return (result);
                            }
                            break;
                        }
                    }

                    Buffer.StartLexeme();

                    // Prefix
                    string prefix = arg.Prefix;
                    string postfix = arg.Postfix;
                    bool hadPrefix = true;
                    if (!string.IsNullOrWhiteSpace(prefix))
                    {
                        if (Buffer.CompareText(0, prefix) == 0)
                        {
                            Buffer.AdvanceColumns(prefix.Length);
                            hadPrefix = true;
                        }
                        else
                            hadPrefix = false;
                    }

                    switch (arg.Kind)
                    {
                        case DoxygenSyntax.ArgumentKind.PrefixToPostfix:
                            {
                                if (hadPrefix)
                                {
                                    Debug.Assert(!string.IsNullOrWhiteSpace(postfix));
                                    bool foundPrefixToPostfix = false;
                                    while (!Buffer.IsEOF)
                                    {
                                        if (Buffer.CompareText(0, postfix) == 0)
                                        {
                                            Buffer.AdvanceColumns(postfix.Length);
                                            foundPrefixToPostfix = true;
                                            break;
                                        }
                                        Buffer.AdvanceColumn();
                                    }
                                    if (arg.IsOptional || foundPrefixToPostfix)
                                    {
                                        DoxygenToken argToken = DoxygenTokenPool.Make(DoxygenTokenKind.ArgumentCaption, Buffer.LexemeRange, foundPrefixToPostfix);
                                        PushToken(argToken);
                                    }
                                    else if (arg.IsRequired)
                                    {
                                        PushError(Buffer.TextPosition, $"Expected postfix '{postfix}' for argument ({argNumber}:{arg}) in command '{commandName}'");
                                        return (result);
                                    }
                                }
                                else if (arg.IsRequired)
                                {
                                    PushError(Buffer.TextPosition, $"Expected prefix '{prefix}' for argument ({argNumber}:{arg}) in command '{commandName}'");
                                    return (result);
                                }
                            }
                            break;

                        case DoxygenSyntax.ArgumentKind.MultipleObjectReference:
                        case DoxygenSyntax.ArgumentKind.SingleObjectReference:
                            {
                                // @TODO(final): ReferencedObject is not always a identifier
                                // Here are some examples of valid referenced objects:
                                // simple_identifier
                                // a_function()
                                // my::awesome::namespace::object
                                // my::awesome::namespace::function()
                                // my#awesome#namespace#function()
                                // method1,method2(),class#field
                                bool allowMultiple = arg.Kind == DoxygenSyntax.ArgumentKind.MultipleObjectReference;
                                bool requireIdent = true;
                                bool foundRef = false;
                                int referenceCount = 0;
                                while (!Buffer.IsEOF)
                                {
                                    int oldPos = Buffer.StreamPosition;
                                    char c0 = Buffer.Peek();
                                    char c1 = Buffer.Peek(1);
                                    if (!requireIdent)
                                    {
                                        if (c0 == ':' && c1 == ':')
                                        {
                                            Buffer.AdvanceColumns(2);
                                            requireIdent = true;
                                            continue;
                                        }
                                        else if (c0 == '#' || c0 == '.')
                                        {
                                            Buffer.AdvanceColumn();
                                            requireIdent = true;
                                            continue;
                                        }
                                        else if (c0 == ',' && referenceCount > 0 && allowMultiple)
                                        {
                                            Buffer.AdvanceColumn();
                                            requireIdent = true;
                                            continue;
                                        }
                                        else if (DoxygenSyntax.IsCommandBegin(c0) || char.IsWhiteSpace(c0))
                                        {
                                            // Correct termination of object-reference
                                            foundRef = true;
                                            break;
                                        }
                                        else
                                        {
                                            PushError(Buffer.TextPosition, $"Unexpected character '{Buffer.Peek()}' for argument ({argNumber}:{arg}) in command '{commandName}'");
                                            return (result);
                                        }
                                    }
                                    else
                                    {
                                        if (SyntaxUtils.IsIdentStart(c0))
                                        {
                                            requireIdent = false;
                                            while (!Buffer.IsEOF)
                                            {
                                                if (!SyntaxUtils.IsIdentPart(Buffer.Peek()))
                                                    break;
                                                Buffer.AdvanceColumn();
                                            }
                                            if (Buffer.Peek() == '(')
                                            {
                                                // Parse until right parent
                                                Buffer.AdvanceColumn();
                                                bool terminatedFunc = false;
                                                while (!Buffer.IsEOF)
                                                {
                                                    if (Buffer.Peek() == ')')
                                                    {
                                                        Buffer.AdvanceColumn();
                                                        terminatedFunc = true;
                                                        break;
                                                    }
                                                    Buffer.AdvanceAuto();
                                                }
                                                if (!terminatedFunc)
                                                {
                                                    PushError(Buffer.TextPosition, $"Unterminated function reference for argument ({argNumber}:{arg}) in command '{commandName}'");
                                                    return (result);
                                                }
                                            }
                                            ++referenceCount;
                                            continue;
                                        }
                                        else
                                        {
                                            PushError(Buffer.TextPosition, $"Requires identifier, but found '{Buffer.Peek()}' for argument ({argNumber}:{arg}) in command '{commandName}'");
                                            return (result);
                                        }
                                    }


                                }
                                if (Buffer.IsEOF)
                                {
                                    // Correct termination of object-reference when stream ends (Single-line)
                                    foundRef = true;
                                }
                                if (arg.IsOptional || foundRef)
                                {
                                    DoxygenToken argToken = DoxygenTokenPool.Make(DoxygenTokenKind.ArgumentIdent, Buffer.LexemeRange, foundRef);
                                    PushToken(argToken);
                                }
                                else if (arg.IsRequired)
                                {
                                    PushError(Buffer.TextPosition, $"Unexpected character '{Buffer.Peek()}' for argument ({argNumber}:{arg}) in command '{commandName}'");
                                    return (result);
                                }
                            }
                            break;

                        case DoxygenSyntax.ArgumentKind.Identifier:
                            {
                                bool foundIdent = false;

                                // Special handling for @param command and ... parameter
                                if ("param".Equals(commandName) && (arg.Kind == DoxygenSyntax.ArgumentKind.Identifier))
                                {
                                    if (Buffer.Peek() == '.')
                                    {
                                        char c1 = Buffer.Peek(1);
                                        char c2 = Buffer.Peek(2);
                                        if (c1 == '.' && c2 == '.')
                                        {
                                            Buffer.AdvanceColumns(3);
                                            foundIdent = true;
                                        }
                                    }
                                }

                                // We dont allow parsing a ident, when any special handling was matched
                                if (!foundIdent && SyntaxUtils.IsIdentStart(Buffer.Peek()))
                                {
                                    foundIdent = true;

                                    // @TODO(final): Expect spacing to terminate identifier - not wait until identifier is finished
                                    while (!Buffer.IsEOF)
                                    {
                                        if (char.IsWhiteSpace(Buffer.Peek()))
                                            break;
                                        else if (!SyntaxUtils.IsIdentPart(Buffer.Peek()))
                                        {
                                            PushError(Buffer.TextPosition, $"Expect identifier terminator (whitespace) but got '{Buffer.Peek()}' for argument ({argNumber}:{arg}) in command '{commandName}'");
                                            return (result);
                                        }
                                        Buffer.AdvanceColumn();
                                    }
                                }
                                if (arg.IsOptional || foundIdent)
                                {
                                    DoxygenToken argToken = DoxygenTokenPool.Make(DoxygenTokenKind.ArgumentIdent, Buffer.LexemeRange, foundIdent);
                                    PushToken(argToken);
                                }
                                else if (arg.IsRequired)
                                {
                                    PushError(Buffer.TextPosition, $"Unexpected character '{Buffer.Peek()}' for argument ({argNumber}:{arg}) in command '{commandName}'");
                                    return (result);
                                }
                            }
                            break;

                        case DoxygenSyntax.ArgumentKind.HeaderFile:
                        case DoxygenSyntax.ArgumentKind.HeaderName:
                            {
                                bool foundFilename = false;
                                bool requiredQuotes = arg.Kind == DoxygenSyntax.ArgumentKind.HeaderName;
                                char curChar = Buffer.Peek();
                                if (curChar == '<' || curChar == '\"')
                                {
                                    char quoteChar = curChar == '<' ? '>' : '\"';
                                    Buffer.AdvanceColumn();
                                    while (!Buffer.IsEOF)
                                    {
                                        curChar = Buffer.Peek();
                                        if (curChar == quoteChar)
                                        {
                                            Buffer.AdvanceColumn();
                                            foundFilename = true;
                                            break;
                                        }
                                        else if (SyntaxUtils.IsLineBreak(curChar))
                                            break;
                                        Buffer.AdvanceColumn();
                                    }
                                    if (!foundFilename)
                                    {
                                        PushError(Buffer.TextPosition, $"Unterminated filename, expect quote char '{quoteChar}' but got '{Buffer.Peek()}' for argument ({argNumber}:{arg}) in command '{commandName}'");
                                        return (result);
                                    }
                                }
                                else if (!requiredQuotes)
                                {
                                    if (SyntaxUtils.IsFilename(Buffer.Peek()))
                                    {
                                        foundFilename = true;
                                        while (!Buffer.IsEOF)
                                        {
                                            if (!SyntaxUtils.IsFilename(Buffer.Peek()))
                                                break;
                                            Buffer.AdvanceColumn();
                                        }
                                    }
                                }
                                if (arg.IsOptional || foundFilename)
                                {
                                    DoxygenToken argToken = DoxygenTokenPool.Make(DoxygenTokenKind.ArgumentFile, Buffer.LexemeRange, foundFilename);
                                    PushToken(argToken);
                                }
                                else if (arg.IsRequired)
                                {
                                    PushError(Buffer.TextPosition, $"Unexpected character '{Buffer.Peek()}' for argument ({argNumber}:{arg}) in command '{commandName}'");
                                    return (result);
                                }
                            }
                            break;

                        case DoxygenSyntax.ArgumentKind.SingleWord:
                            {
                                // @TODO(final): IsWordStart()
                                bool foundWord = false;
                                if (char.IsLetterOrDigit(Buffer.Peek()))
                                {
                                    foundWord = true;
                                    while (!Buffer.IsEOF)
                                    {
                                        // @TODO(final): IsWordPart()
                                        if (char.IsWhiteSpace(Buffer.Peek()))
                                            break;
                                        Buffer.AdvanceColumn();
                                    }
                                }
                                if (arg.IsOptional || foundWord)
                                {
                                    DoxygenToken argToken = DoxygenTokenPool.Make(DoxygenTokenKind.ArgumentCaption, Buffer.LexemeRange, foundWord);
                                    PushToken(argToken);
                                }
                                else if (arg.IsRequired)
                                {
                                    PushError(Buffer.TextPosition, $"Unexpected character '{Buffer.Peek()}' for argument ({argNumber}:{arg}) in command '{commandName}'");
                                    return (result);
                                }
                            }
                            break;

                        case DoxygenSyntax.ArgumentKind.QuotedString:
                            {
                                bool isComplete = false;

                                // @TODO(final): Make quotes configurable in the argument rule
                                bool hasQuote = Buffer.Peek() == '"' || Buffer.Peek() == '<';

                                char endQuote = char.MaxValue;
                                if (hasQuote)
                                {
                                    endQuote = Buffer.Peek() == '<' ? '>' : '"';
                                    Buffer.AdvanceColumn();
                                    while (!Buffer.IsEOF)
                                    {
                                        if (!hasQuote)
                                        {
                                            if (char.IsWhiteSpace(Buffer.Peek()))
                                                break;
                                        }
                                        else
                                        {
                                            if (Buffer.Peek() == endQuote)
                                            {
                                                Buffer.AdvanceColumn();
                                                isComplete = true;
                                                break;
                                            }
                                            else if (SyntaxUtils.IsLineBreak(Buffer.Peek()) || Buffer.Peek() == TextStream.InvalidCharacter)
                                                break;
                                        }
                                        Buffer.AdvanceColumn();
                                    }
                                    if (!isComplete)
                                    {
                                        PushError(Buffer.TextPosition, $"Unterminated quote string for argument ({argNumber}:{arg}) in command '{commandName}'");
                                        return (result);
                                    }
                                }
                                if (arg.IsOptional || isComplete)
                                {
                                    DoxygenToken argToken = DoxygenTokenPool.Make(DoxygenTokenKind.ArgumentText, Buffer.LexemeRange, isComplete);
                                    PushToken(argToken);
                                }
                                else if (arg.IsRequired)
                                {
                                    PushError(Buffer.TextPosition, $"Unexpected character '{Buffer.Peek()}' for argument ({argNumber}:{arg}) in command '{commandName}'");
                                    return (result);
                                }
                            }
                            break;

                        case DoxygenSyntax.ArgumentKind.UntilEndOfLine:
                            {
                                bool eolFound = false;
                                while (!Buffer.IsEOF)
                                {
                                    if (SyntaxUtils.IsLineBreak(Buffer.Peek()))
                                    {
                                        eolFound = true;
                                        break;
                                    }
                                    Buffer.AdvanceColumn();
                                }
                                if (Buffer.IsEOF)
                                    eolFound = true;
                                if (arg.IsOptional || eolFound)
                                {
                                    DoxygenToken argToken = DoxygenTokenPool.Make(DoxygenTokenKind.ArgumentText, Buffer.LexemeRange, true);
                                    PushToken(argToken);
                                }
                                else if (arg.IsRequired)
                                {
                                    PushError(Buffer.TextPosition, $"Unterminated end-of-line for argument ({argNumber}:{arg}) in command '{commandName}'");
                                    return (result);
                                }
                            }
                            break;

                        case DoxygenSyntax.ArgumentKind.ComplexLine:
                        case DoxygenSyntax.ArgumentKind.ComplexBlock:
                            // @TODO(final): Implement complex line/block properly
                            goto CommandDone;

                        default:
                            PushError(Buffer.TextPosition, $"Unsupported argument ({argNumber}:{arg}) in command '{commandName}'");
                            return (result);
                    }

                    // Postfix
                    if (hadPrefix && !string.IsNullOrWhiteSpace(postfix) && arg.Kind != DoxygenSyntax.ArgumentKind.PrefixToPostfix)
                    {
                        if (Buffer.CompareText(0, postfix) == 0)
                        {
                            Buffer.AdvanceColumns(prefix.Length);
                        }
                        else
                        {
                            PushError(Buffer.TextPosition, $"Expected postfix '{postfix}' for pp-argument({argNumber}:{arg}) in command '{commandName}'");
                            return (result);
                        }
                    }
                    ++argNumber;
                }
            }

CommandDone:
            result.IsValid = true;

            SkipSpacings(SkipType.All);

            if (SyntaxUtils.IsLineBreak(Buffer.Peek()))
            {
                Buffer.StartLexeme();
                SkipLineBreaks(SkipType.Single);
                DoxygenToken token = DoxygenTokenPool.Make(DoxygenTokenKind.EndOfLine, Buffer.LexemeRange, true);
                PushToken(token);
            }

            return (result);
        }

        private void StartText(LexState state)
        {
            DoxygenToken token = DoxygenTokenPool.Make(DoxygenTokenKind.TextStart, new TextRange(Buffer.TextPosition, 0), false);
            PushToken(token);
        }
        private void EndText(LexState state)
        {
            var lastTextStartOrEnd = Tokens.LastOrDefault(t => t.Kind == DoxygenTokenKind.TextStart || t.Kind == DoxygenTokenKind.TextEnd);
            if (lastTextStartOrEnd != null && lastTextStartOrEnd.Kind == DoxygenTokenKind.TextStart)
            {
                DoxygenToken token = DoxygenTokenPool.Make(DoxygenTokenKind.TextEnd, new TextRange(Buffer.TextPosition, 0), false);
                PushToken(token);
            }
        }

        [Flags]
        enum StateFlags
        {
            None = 0,
            InsideBlock = 1 << 0,
            SingleLine = 1 << 1,
            JavaDoc = 1 << 2,
            CommandContentStarted = 1 << 3,
        }



        class LexState
        {
            public StateFlags Flags { get; set; }
            public int CurrentLineStartIndex { get; set; }

            public LexState()
            {
                Flags = StateFlags.None;
                CurrentLineStartIndex = -1;
            }
        }

        private void Done(LexState state)
        {
            EndText(state);
            if (state.Flags.HasFlag(StateFlags.InsideBlock))
            {
                // Block was not closed, so we close it now
                state.Flags = StateFlags.None;
                PushToken(DoxygenTokenPool.Make(DoxygenTokenKind.DoxyBlockEnd, new TextRange(Buffer.TextPosition, 0), false));
            }
        }

        protected override bool LexNext()
        {
            LexState state = new LexState();
            state.CurrentLineStartIndex = Buffer.StreamPosition;
            do
            {
                char thisChar = Buffer.Peek();
                switch (thisChar)
                {
                    case ' ':
                    case '\v':
                    case '\f':
                    case '\t':
                        SkipSpacings(SkipType.All);
                        break;

                    case '\r':
                    case '\n':
                        {
                            if (state.Flags.HasFlag(StateFlags.InsideBlock) && state.Flags.HasFlag(StateFlags.SingleLine))
                            {
                                Done(state);
                                return (true);
                            }

                            // @NOTE(final): Detect if our line content until the line break was empty
                            Debug.Assert(Buffer.StreamPosition >= state.CurrentLineStartIndex);
                            int len = Buffer.StreamPosition - state.CurrentLineStartIndex;
                            bool wasEmptyLine = Buffer.MatchCharacters(state.CurrentLineStartIndex, len, char.IsWhiteSpace);

                            Buffer.StartLexeme();
                            SkipLineBreaks(SkipType.Single);
                            state.CurrentLineStartIndex = Buffer.StreamPosition;
                            PushToken(DoxygenTokenPool.Make(wasEmptyLine ? DoxygenTokenKind.EmptyLine : DoxygenTokenKind.EndOfLine, Buffer.LexemeRange, true));
                        }
                        break;

                    case '/':
                        {
                            char n = Buffer.Peek(1);
                            if (n == '*')
                            {
                                // Multi line
                                char n2 = Buffer.Peek(2);
                                if (DoxygenSyntax.MultiLineDocChars.Contains(n2))
                                {
                                    Debug.Assert(!state.Flags.HasFlag(StateFlags.InsideBlock));
                                    Buffer.StartLexeme();
                                    Buffer.AdvanceColumns(3);
                                    state.Flags = StateFlags.InsideBlock;
                                    if (n2 == '*') state.Flags |= StateFlags.JavaDoc;
                                    PushToken(DoxygenTokenPool.Make(DoxygenTokenKind.DoxyBlockStartMulti, Buffer.LexemeRange, true));
                                    StartText(state);
                                    continue;
                                }
                            }
                            else if (n == '/')
                            {
                                // Single line
                                char n2 = Buffer.Peek(2);
                                if (DoxygenSyntax.SingleLineDocChars.Contains(n2))
                                {
                                    Debug.Assert(!state.Flags.HasFlag(StateFlags.InsideBlock));
                                    Buffer.StartLexeme();
                                    Buffer.AdvanceColumns(3);
                                    state.Flags = StateFlags.InsideBlock | StateFlags.SingleLine;
                                    PushToken(DoxygenTokenPool.Make(DoxygenTokenKind.DoxyBlockStartSingle, Buffer.LexemeRange, true));
                                    StartText(state);
                                    continue;
                                }
                            }
                            Buffer.AdvanceColumn();
                        }
                        break;

                    case '*':
                        {
                            char n = Buffer.Peek(1);
                            if (state.Flags.HasFlag(StateFlags.InsideBlock))
                            {
                                if (n == '/')
                                {
                                    EndText(state);
                                    Buffer.StartLexeme();
                                    Buffer.AdvanceColumns(2);
                                    state.Flags = StateFlags.None;
                                    return PushToken(DoxygenTokenPool.Make(DoxygenTokenKind.DoxyBlockEnd, Buffer.LexemeRange, true));
                                }
                                else if (state.Flags.HasFlag(StateFlags.JavaDoc))
                                {
                                    // Push single star token (java doc style)
                                    EndText(state);
                                    Buffer.StartLexeme();
                                    Buffer.AdvanceColumn();
                                    PushToken(DoxygenTokenPool.Make(DoxygenTokenKind.DoxyBlockChars, Buffer.LexemeRange, true));
                                    StartText(state);
                                    state.CurrentLineStartIndex = Buffer.StreamPosition;
                                    continue;
                                }
                            }
                            Buffer.AdvanceColumn();
                        }
                        break;

                    case '@':
                    case '\\':
                        {
                            char n = Buffer.Peek(1);
                            if (state.Flags.HasFlag(StateFlags.InsideBlock) && DoxygenSyntax.IsCommandIdentStart(n))
                            {
                                EndText(state);
                                LexCommandTokens();
                                StartText(state);
                            }
                            else
                                Buffer.AdvanceColumn();
                        }
                        break;

                    case TextStream.InvalidCharacter:
                        {
                            if (Buffer.IsEOF)
                            {
                                Done(state);
                                PushToken(DoxygenTokenPool.Make(DoxygenTokenKind.EOF, new TextRange(Buffer.TextPosition, 0), false));
                                return (false);
                            }
                            else
                                Buffer.AdvanceColumn();
                        }
                        break;

                    default:
                        {
                            Buffer.AdvanceColumn();
                            break;
                        }
                }
            } while (!Buffer.IsEOF);
            Done(state);
            PushToken(DoxygenTokenPool.Make(DoxygenTokenKind.EOF, new TextRange(Buffer.TextPosition, 0), false));
            return (false);
        }
    }
}
