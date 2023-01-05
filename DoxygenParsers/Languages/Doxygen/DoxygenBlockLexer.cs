using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.Languages.Utils;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public class DoxygenBlockLexer : BaseLexer<DoxygenToken>
    {
        [Flags]
        enum StateFlags
        {
            None = 0,
            InsideBlock = 1 << 0,
            SingleLine = 1 << 1,
            JavaDoc = 1 << 2,
            CommandContentStarted = 1 << 3,
        }

        class DoxygenState : State
        {
            public StateFlags Flags { get; set; }
            public int CurrentLineStartIndex { get; set; }

            public DoxygenState()
            {
                Flags = StateFlags.None;
                CurrentLineStartIndex = -1;
            }

            public override void StartLex(ITextStream stream)
            {
                Flags = StateFlags.None;
                CurrentLineStartIndex = stream.StreamPosition;
            }
        }

        protected override State CreateState()
        {
            return new DoxygenState();
        }

        public DoxygenBlockLexer(string source, int index, int length, TextPosition pos) : base(source, index, length, pos, LanguageKind.Doxygen)
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

            // Command
            Buffer.StartLexeme();
            Buffer.AdvanceColumn();

            DoxygenTokenKind kind = DoxygenTokenKind.Command;
            {
                char first = Buffer.Peek();
                switch (first)
                {
                    case '{':
                    case '}':
                        kind = (first == '{') ? DoxygenTokenKind.GroupStart : DoxygenTokenKind.GroupEnd;
                        Buffer.AdvanceColumn();
                        break;

                    case '$':
                    case '@':
                    case '\\':
                    case '~':
                    case '<':
                    case '=':
                    case '>':
                    case '#':
                    case '"':
                        Buffer.AdvanceColumn();
                        break;

                    case ':':
                    case '|':
                    case '-':
                        Buffer.AdvanceColumnsWhile(d => d.Equals(first));
                        break;

                    default:
                        if (DoxygenSyntax.IsCommandIdentStart(first))
                        {
                            while (!Buffer.IsEOF)
                            {
                                if (!DoxygenSyntax.IsCommandIdentPart(Buffer.Peek()))
                                    break;
                                Buffer.AdvanceColumn();
                            }
                        }
                        break;
                }
            }

            TextPosition commandStart = Buffer.LexemeStart;
            int commandLen = Buffer.LexemeWidth;
            string commandName = Buffer.GetSourceText(Buffer.LexemeStart.Index + 1, commandLen - 1);
            DoxygenSyntax.CommandRule rule = DoxygenSyntax.GetCommandRule(commandName);
            if (rule != null)
            {
                if (rule.Kind == DoxygenSyntax.CommandKind.StartCommandBlock)
                    kind = DoxygenTokenKind.CommandStart;
                else if (rule.Kind == DoxygenSyntax.CommandKind.EndCommandBlock)
                    kind = DoxygenTokenKind.CommandEnd;
            }
            else
            {
                // @NOTE(final): Group start/end are not a "known" command
                if (kind != DoxygenTokenKind.GroupStart && kind != DoxygenTokenKind.GroupEnd)
                    kind = DoxygenTokenKind.InvalidCommand;
            }
            DoxygenToken commandToken = DoxygenTokenPool.Make(kind, Buffer.LexemeRange, true);
            PushToken(commandToken);

            CommandResult result = new CommandResult(commandStart, rule, commandName);

            string whereName = "Command";
            if (rule != null)
            {
                int argNumber = 0;
                int argCount = rule.Args.Count();
                bool noMoreArgs = false;
                foreach (DoxygenSyntax.ArgumentRule arg in rule.Args)
                {
                    // @TODO(final): Handle rule repeat type for arguments on same type
                    char first = Buffer.Peek();
                    if (!arg.Flags.HasFlag(DoxygenSyntax.ArgumentFlags.DirectlyAfterCommand))
                    {
                        if (SyntaxUtils.IsSpacing(first) || first == '\t')
                            Buffer.SkipSpaces(RepeatKind.All);
                        else
                        {
                            // No more arguments are following
                            noMoreArgs = true;
                        }
                    }

                    Buffer.StartLexeme();

                    // Prefix
                    string prefix = arg.Prefix;
                    string postfix = arg.Postfix;
                    bool hadPrefix = false;
                    if (prefix != null && !noMoreArgs)
                    {
                        if (!string.IsNullOrEmpty(prefix))
                        {
                            if (Buffer.MatchRelative(0, prefix))
                            {
                                Buffer.AdvanceColumns(prefix.Length);
                                hadPrefix = true;
                            }
                        }
                        else if ((prefix.Length == 0) && (!string.IsNullOrEmpty(postfix)))
                            hadPrefix = true;
                    }

                    switch (arg.Kind)
                    {
                        case DoxygenSyntax.ArgumentKind.PrefixToPostfix:
                            {
                                if (hadPrefix && !noMoreArgs)
                                {
                                    Debug.Assert(!string.IsNullOrEmpty(postfix));
                                    bool foundPrefixToPostfix = false;
                                    while (!Buffer.IsEOF)
                                    {
                                        if (Buffer.MatchRelative(0, postfix))
                                        {
                                            Buffer.AdvanceColumns(postfix.Length);
                                            foundPrefixToPostfix = true;
                                            break;
                                        }
                                        else if (SyntaxUtils.IsLineBreak(Buffer.Peek()))
                                            break;
                                        else
                                            Buffer.AdvanceColumn();
                                    }
                                    if (arg.IsOptional || foundPrefixToPostfix)
                                    {
                                        DoxygenToken argToken = DoxygenTokenPool.Make(DoxygenTokenKind.ArgumentCaption, Buffer.LexemeRange, foundPrefixToPostfix);
                                        PushToken(argToken);
                                    }
                                    else if (arg.IsRequired)
                                    {
                                        AddError(Buffer.TextPosition, $"Expected postfix '{postfix}' for argument ({argNumber}:{arg}) in command '{commandName}'", what: whereName, symbol: commandName);
                                        return (result);
                                    }
                                }
                                else if (arg.IsOptional)
                                {
                                    DoxygenToken argToken = DoxygenTokenPool.Make(DoxygenTokenKind.ArgumentCaption, Buffer.LexemeRange, false);
                                    PushToken(argToken);
                                }
                                else if (arg.IsRequired)
                                {
                                    AddError(Buffer.TextPosition, $"Expected prefix '{prefix}' for argument ({argNumber}:{arg}) in command '{commandName}'", what: whereName, symbol: commandName);
                                    return (result);
                                }
                            }
                            break;

                        case DoxygenSyntax.ArgumentKind.MultipleObjectReference:
                        case DoxygenSyntax.ArgumentKind.SingleObjectReference:
                            {
                                // Here are some examples of valid referenced objects:
                                // simple_identifier
                                // a_function()
                                // my::awesome::namespace::object
                                // my::awesome::namespace::function()
                                // my#awesome#namespace#function()
                                // method1,method2(),class#field
                                bool foundRef = false;
                                if (!noMoreArgs)
                                {
                                    bool allowMultiple = arg.Kind == DoxygenSyntax.ArgumentKind.MultipleObjectReference;
                                    bool requireIdent = true;
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
                                            else if (c0 == '#')
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
                                            else
                                            {
                                                // Correct termination of object-reference
                                                foundRef = true;
                                                break;
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
                                                        AddError(Buffer.TextPosition, $"Unterminated function reference for argument ({argNumber}:{arg}) in command '{commandName}'", what: whereName, symbol: commandName);
                                                        return (result);
                                                    }
                                                }
                                                ++referenceCount;
                                                continue;
                                            }
                                            else
                                            {
                                                AddError(Buffer.TextPosition, $"Requires identifier, but found '{Buffer.Peek()}' for argument ({argNumber}:{arg}) in command '{commandName}'", what: whereName, symbol: commandName);
                                                return (result);
                                            }
                                        }


                                    }
                                    if (Buffer.IsEOF)
                                    {
                                        // Correct termination of object-reference when stream ends (Single-line)
                                        foundRef = true;
                                    }
                                }
                                if (arg.IsOptional || foundRef)
                                {
                                    DoxygenToken argToken = DoxygenTokenPool.Make(DoxygenTokenKind.ArgumentIdent, Buffer.LexemeRange, foundRef);
                                    PushToken(argToken);
                                }
                                else if (arg.IsRequired)
                                {
                                    AddError(Buffer.TextPosition, $"Unexpected character '{Buffer.Peek()}' for argument ({argNumber}:{arg}) in command '{commandName}'", what: whereName, symbol: commandName);
                                    return (result);
                                }
                            }
                            break;

                        case DoxygenSyntax.ArgumentKind.Identifier:
                            {
                                bool foundIdent = false;

                                // Special handling for @param command and ... parameter
                                if (!noMoreArgs && "param".Equals(commandName) && (arg.Kind == DoxygenSyntax.ArgumentKind.Identifier))
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
                                if (!noMoreArgs && !foundIdent && SyntaxUtils.IsIdentStart(Buffer.Peek()))
                                {
                                    foundIdent = true;
                                    while (!Buffer.IsEOF)
                                    {
                                        if (!SyntaxUtils.IsIdentPart(Buffer.Peek()))
                                            break;
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
                                    AddError(Buffer.TextPosition, $"Unexpected character '{Buffer.Peek()}' for argument ({argNumber}:{arg}) in command '{commandName}'", what: whereName, symbol: commandName);
                                    return (result);
                                }
                            }
                            break;

                        case DoxygenSyntax.ArgumentKind.HeaderFile:
                        case DoxygenSyntax.ArgumentKind.HeaderName:
                            {
                                bool foundFilename = false;
                                if (!noMoreArgs)
                                {
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
                                            AddError(Buffer.TextPosition, $"Unterminated filename, expect quote char '{quoteChar}' but got '{Buffer.Peek()}' for argument ({argNumber}:{arg}) in command '{commandName}'", whereName, commandName);
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
                                }
                                if (arg.IsOptional || foundFilename)
                                {
                                    DoxygenToken argToken = DoxygenTokenPool.Make(DoxygenTokenKind.ArgumentFile, Buffer.LexemeRange, foundFilename);
                                    PushToken(argToken);
                                }
                                else if (arg.IsRequired)
                                {
                                    AddError(Buffer.TextPosition, $"Unexpected character '{Buffer.Peek()}' for argument ({argNumber}:{arg}) in command '{commandName}'", whereName, commandName);
                                    return (result);
                                }
                            }
                            break;

                        case DoxygenSyntax.ArgumentKind.SingleWord:
                            {
                                // @TODO(final): IsWordStart()
                                bool foundWord = false;
                                if (!noMoreArgs && char.IsLetterOrDigit(Buffer.Peek()))
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
                                    AddError(Buffer.TextPosition, $"Unexpected character '{Buffer.Peek()}' for argument ({argNumber}:{arg}) in command '{commandName}'", whereName, commandName);
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
                                if (hasQuote && !noMoreArgs)
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
                                        AddError(Buffer.TextPosition, $"Unterminated quote string for argument ({argNumber}:{arg}) in command '{commandName}'", whereName, commandName);
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
                                    AddError(Buffer.TextPosition, $"Unexpected character '{Buffer.Peek()}' for argument ({argNumber}:{arg}) in command '{commandName}'", whereName, commandName);
                                    return (result);
                                }
                            }
                            break;

                        case DoxygenSyntax.ArgumentKind.UntilEndOfLine:
                            {
                                bool eolFound = false;
                                if (!noMoreArgs)
                                {
                                    while (!Buffer.IsEOF)
                                    {
                                        if (SyntaxUtils.IsLineBreak(Buffer.Peek()))
                                        {
                                            eolFound = true;
                                            break;
                                        }
                                        Buffer.AdvanceAuto();
                                    }
                                    if (Buffer.IsEOF)
                                        eolFound = true;
                                }
                                if (arg.IsOptional || eolFound)
                                {
                                    DoxygenToken argToken = DoxygenTokenPool.Make(DoxygenTokenKind.ArgumentText, Buffer.LexemeRange, true);
                                    PushToken(argToken);
                                }
                                else if (arg.IsRequired)
                                {
                                    AddError(Buffer.TextPosition, $"Unterminated end-of-line for argument ({argNumber}:{arg}) in command '{commandName}'", whereName, commandName);
                                    return (result);
                                }
                            }
                            break;

                        case DoxygenSyntax.ArgumentKind.ComplexLine:
                        case DoxygenSyntax.ArgumentKind.ComplexBlock:
                            // @TODO(final): Implement complex line/block properly
                            goto CommandDone;

                        default:
                            AddError(Buffer.TextPosition, $"Unsupported argument ({argNumber}:{arg}) in command '{commandName}'", whereName, commandName);
                            return (result);
                    }

                    // Postfix
                    if (!noMoreArgs && (hadPrefix && !string.IsNullOrWhiteSpace(postfix) && arg.Kind != DoxygenSyntax.ArgumentKind.PrefixToPostfix))
                    {
                        if (Buffer.MatchRelative(0, postfix))
                        {
                            Buffer.AdvanceColumns(prefix.Length);
                        }
                        else
                        {
                            AddError(Buffer.TextPosition, $"Expected postfix '{postfix}' for pp-argument({argNumber}:{arg}) in command '{commandName}'", whereName, commandName);
                            return (result);
                        }
                    }
                    ++argNumber;
                }
            }

        CommandDone:
            result.IsValid = true;

            return (result);
        }

        private void StartText(DoxygenState state)
        {
            DoxygenToken token = DoxygenTokenPool.Make(DoxygenTokenKind.TextStart, new TextRange(Buffer.TextPosition, 0), false);
            PushToken(token);
        }
        private void EndText(DoxygenState state)
        {
            DoxygenToken lastTextStartOrEnd = Tokens.LastOrDefault(t => t.Kind == DoxygenTokenKind.TextStart || t.Kind == DoxygenTokenKind.TextEnd);
            if (lastTextStartOrEnd != null && lastTextStartOrEnd.Kind == DoxygenTokenKind.TextStart)
            {
                int textLength = Buffer.TextPosition.Index - lastTextStartOrEnd.Index;
                if (textLength > 0)
                {
                    DoxygenToken token = DoxygenTokenPool.Make(DoxygenTokenKind.TextEnd, new TextRange(Buffer.TextPosition, 0), false);
                    PushToken(token);
                }
                else
                    RemoveToken(lastTextStartOrEnd);
            }
        }

        private void Done(DoxygenState state)
        {
            EndText(state);
            if (state.Flags.HasFlag(StateFlags.InsideBlock))
            {
                // Block was not closed, so we close it now
                state.Flags = StateFlags.None;
                PushToken(DoxygenTokenPool.Make(DoxygenTokenKind.DoxyBlockEnd, new TextRange(Buffer.TextPosition, 0), false));
            }
        }

        private bool LexUntilCommandEnd(CommandResult commandResult, string beginCommand, string endCommand)
        {
            // Special case, we dont want to parse doxygen stuff inside several sections, such as "code" or "htmlonly".
            // So we wait until the end-command such as "endcode" or "endhtml" comes
            bool isComplete = false;
            while (!Buffer.IsEOF)
            {
                char c0 = Buffer.Peek();
                char c1 = Buffer.Peek(1);
                if ((c0 == '@' || c0 == '\\') && SyntaxUtils.IsIdentStart(c1))
                {
                    Buffer.StartLexeme();
                    Buffer.AdvanceColumn();
                    Buffer.AdvanceColumnsWhile(SyntaxUtils.IsIdentPart);
                    string ident = Buffer.GetSourceText(Buffer.LexemeStart.Index + 1, Buffer.LexemeWidth - 1);
                    if (endCommand.Equals(ident))
                    {
                        PushToken(DoxygenTokenPool.Make(DoxygenTokenKind.CommandEnd, Buffer.LexemeRange, true));
                        isComplete = true;
                        break;
                    }
                }
                else if (SyntaxUtils.IsLineBreak(c0))
                    Buffer.AdvanceLineAuto();
                else if ('\t'.Equals(c0))
                    Buffer.AdvanceTab();
                else
                    Buffer.AdvanceColumn();
            }
            if (!isComplete)
            {
                AddError(commandResult.StartPos, $"Unterminated command-block, expect '@{endCommand}' or '\\{endCommand}'", "{beginCommand}", commandResult.CommandName);
                return (false);
            }
            return (true);
        }

        protected override bool LexNext(State hiddenState)
        {
            DoxygenState state = (DoxygenState)hiddenState;
            state.Flags = StateFlags.None;
            state.CurrentLineStartIndex = Buffer.StreamPosition;
            do
            {
                char first = Buffer.Peek();
                char second = Buffer.Peek(1);
                char third = Buffer.Peek(2);
                char fourth = Buffer.Peek(3);
                switch (first)
                {
                    case ' ':
                    case '\v':
                    case '\f':
                    case '\t':
                        Buffer.SkipSpaces(RepeatKind.All);
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
                            bool wasEmptyLine = Buffer.MatchAbsolute(state.CurrentLineStartIndex, len, char.IsWhiteSpace) || (len == 0);

                            Buffer.StartLexeme();
                            Buffer.SkipLineBreaks(RepeatKind.Single);
                            state.CurrentLineStartIndex = Buffer.StreamPosition;
                            PushToken(DoxygenTokenPool.Make(wasEmptyLine ? DoxygenTokenKind.EmptyLine : DoxygenTokenKind.EndOfLine, Buffer.LexemeRange, true));
                        }
                        break;

                    case '/':
                        {
                            if (second == '*' && !state.Flags.HasFlag(StateFlags.InsideBlock))
                            {
                                // Multi line
                                if (DoxygenSyntax.MultiLineDocChars.Contains(third) && !DoxygenSyntax.MultiLineDocChars.Contains(fourth))
                                {
                                    Buffer.StartLexeme();
                                    Buffer.AdvanceColumns(3);
                                    state.Flags = StateFlags.InsideBlock;
                                    if (third == '*')
                                    {
                                        char n3 = Buffer.Peek();
                                        if (n3 == '/')
                                        {
                                            // Directly closed comment
                                            Buffer.AdvanceColumn();
                                            return PushToken(DoxygenTokenPool.Make(DoxygenTokenKind.DoxyBlockStartMulti, Buffer.LexemeRange, true));
                                        }
                                        state.Flags |= StateFlags.JavaDoc;
                                    }
                                    PushToken(DoxygenTokenPool.Make(DoxygenTokenKind.DoxyBlockStartMulti, Buffer.LexemeRange, true));
                                    StartText(state);
                                    continue;
                                }
                                else
                                {
                                    // Just skip until normal multi-line comment ends
                                    CppLexer.LexResult r = CppLexer.LexMultiLineComment(Buffer, true);
                                    if (!r.IsComplete)
                                    {
                                        AddError(Buffer.TextPosition, $"Unterminated multi-line comment, expect '*/' but got EOF", r.Kind.ToString());
                                        return (false);
                                    }
                                    continue;
                                }
                            }
                            else if (second == '/' && !state.Flags.HasFlag(StateFlags.InsideBlock))
                            {
                                // Single line
                                if (DoxygenSyntax.SingleLineDocChars.Contains(third) && !DoxygenSyntax.MultiLineDocChars.Contains(fourth))
                                {
                                    Buffer.StartLexeme();
                                    Buffer.AdvanceColumns(3);
                                    state.Flags = StateFlags.InsideBlock | StateFlags.SingleLine;
                                    PushToken(DoxygenTokenPool.Make(DoxygenTokenKind.DoxyBlockStartSingle, Buffer.LexemeRange, true));
                                    StartText(state);
                                    continue;
                                }
                                else
                                {
                                    // Just skip until normal single-line comment ends
                                    CppLexer.LexResult r = CppLexer.LexSingleLineComment(Buffer, true);
                                    if (!r.IsComplete)
                                    {
                                        AddError(Buffer.TextPosition, $"Unterminated single-line comment, expect linebreak but got EOF", r.Kind.ToString());
                                        return (false);
                                    }
                                    continue;
                                }
                            }
                            else
                                Buffer.AdvanceColumn();
                        }
                        break;

                    case '*':
                        {
                            if (state.Flags.HasFlag(StateFlags.InsideBlock))
                            {
                                if (second == '/')
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
                            if (state.Flags.HasFlag(StateFlags.InsideBlock))
                            {
                                EndText(state);
                                CommandResult commandResult = LexCommandTokens();
                                if (commandResult.IsValid)
                                {
                                    if ("code".Equals(commandResult.CommandName))
                                    {
                                        if (!LexUntilCommandEnd(commandResult, "code", "endcode"))
                                            return (false);
                                    }
                                    else if ("htmlonly".Equals(commandResult.CommandName))
                                    {
                                        if (!LexUntilCommandEnd(commandResult, "htmlonly", "endhtmlonly"))
                                            return (false);
                                    }
                                }
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
