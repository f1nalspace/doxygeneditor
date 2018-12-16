using DoxygenEditor.Parser.Entities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DoxygenEditor.Parser
{
    public class DoxygenParser : IDoxygenParser
    {
        private void SkipWhitespaces(ref LineState line)
        {
            while (!line.IsEndOfLine && (char.IsWhiteSpace(line.CurrentChar)))
            {
                line.IncOffset();
            }
        }
        private void SkipCharacters(ref LineState line, char[] untilChars)
        {
            while (!line.IsEndOfLine && (untilChars.Contains(line.CurrentChar)))
            {
                line.IncOffset();
            }
        }

        private string ParseIdentifierString(ref LineState line, char[] additionalChars)
        {
            if (!line.IsEndOfLine && (char.IsLetter(line.CurrentChar) || additionalChars.Contains(line.CurrentChar)))
            {
                string result = "";
                while (!line.IsEndOfLine && (char.IsLetterOrDigit(line.CurrentChar) || additionalChars.Contains(line.CurrentChar)))
                {
                    result += line.CurrentChar;
                    line.IncOffset();
                }
                return (result);
            }
            return null;
        }
        private Keyword ParseIdentifierKeyword(ref LineState line, char[] additionalChars)
        {
            if (!line.IsEndOfLine && (char.IsLetter(line.CurrentChar) || additionalChars.Contains(line.CurrentChar)))
            {
                Keyword result = new Keyword() { Start = line.Offset };
                string ident = ParseIdentifierString(ref line, additionalChars);
                result.Length = line.Offset - result.Start;
                result.Value = ident;
                return (result);
            }
            return null;
        }
        private Keyword ParseRemainingKeyword(ref LineState line)
        {
            Keyword result = new Keyword() { Start = line.Offset };
            result.Length = line.Length - line.Offset;
            result.Value = line.Value.Substring(line.Offset, result.Length);
            line.IncOffset(result.Length);
            return (result);
        }
        class Keyword
        {
            public int Start;
            public int Length;
            public string Value;
        }

        private Keyword ParseCommand(ref LineState line)
        {
            Debug.Assert(line.CurrentChar == '@');
            line.IncOffset();
            Keyword result = ParseIdentifierKeyword(ref line, new[] { '_', '{', '}' });
            return (result);
        }

        private void ParseHeaderFile(ParseState state, SequenceInfo startInfo, ref LineState line, string pageCommand)
        {
            SkipWhitespaces(ref line);
            Keyword headerFileKeyword = ParseRemainingKeyword(ref line);
            string headerFile = headerFileKeyword.Value;
            if (headerFile != null) headerFile = headerFile.Trim();
            state.AddHeaderFile(headerFile);
        }

        private void ParsePage(ParseState state, SequenceInfo startInfo, ref LineState line, string pageCommand)
        {
            SkipWhitespaces(ref line);
            PageEntity pageEntity;
            if ("page".Equals(pageCommand))
            {
                // Identifier
                Keyword pageIdentKeyword = ParseIdentifierKeyword(ref line, new[] { '_' });
                if (pageIdentKeyword == null)
                {
                    state.AddMessage(ParseMessage.MessageType.Warning, line.Info, "Page requires a identifier!");
                    return;
                }
                string pageIdent = pageIdentKeyword.Value;

                // Caption
                SkipWhitespaces(ref line);
                Keyword pageCaptionKeyword = ParseRemainingKeyword(ref line);
                string pageCaption = pageCaptionKeyword.Value;
                pageEntity = new PageEntity(startInfo, pageIdent, pageCaption);
            }
            else
            {
                // Main Page
                Debug.Assert("mainpage".Equals(pageCommand));
                pageEntity = new MainPageEntity(startInfo);
            }
            state.AddPage(pageEntity);
        }

        private void ParseSection(ParseState state, SequenceInfo startInfo, ref LineState line, string sectionCommand)
        {
            // Identifier
            SkipWhitespaces(ref line);
            Keyword sectionIdentKeyword = ParseIdentifierKeyword(ref line, new[] { '_' });
            if (sectionIdentKeyword == null) return;
            string sectionIdent = sectionIdentKeyword.Value;

            // Caption
            string sectionCaption = null;
            if (!line.IsEndOfLine)
            {
                SkipWhitespaces(ref line);
                Keyword sectionCaptionKeyword = ParseRemainingKeyword(ref line);
                if (sectionCaptionKeyword != null)
                    sectionCaption = sectionCaptionKeyword.Value;
            }

            SectionEntity sectionEntity;
            if ("section".Equals(sectionCommand))
                sectionEntity = new SectionEntity(startInfo, sectionIdent, sectionCaption);
            else
            {
                Debug.Assert("subsection".Equals(sectionCommand));
                sectionEntity = new SubSectionEntity(startInfo, sectionIdent, sectionCaption);
            }
            state.AddSection(sectionEntity);
        }

        private GroupEntity ParseDefGroup(SequenceInfo startInfo, ref LineState line, string command)
        {
            // Identifier
            SkipWhitespaces(ref line);
            Keyword identKeyword = ParseIdentifierKeyword(ref line, new[] { '_' });
            if (identKeyword == null) return null;
            string ident = identKeyword.Value;

            // Caption
            string caption = null;
            if (!line.IsEndOfLine)
            {
                SkipWhitespaces(ref line);
                Keyword captionKeyword = ParseRemainingKeyword(ref line);
                if (captionKeyword != null)
                    caption = captionKeyword.Value;
            }
            GroupEntity result = new GroupEntity(startInfo, ident, caption);
            return (result);
        }

        enum ParseMode
        {
            None,
            InsideDoxygenBlock,
            InsideJavaDocBlock,
        }

        private void ParseJavaDocLine(ParseState parseState, LineState docLine)
        {
            SkipWhitespaces(ref docLine);
            SkipCharacters(ref docLine, new[] { '*' });
            SkipWhitespaces(ref docLine);
            if (!docLine.IsEndOfLine)
            {
                if ('@'.Equals(docLine.CurrentChar))
                {
                    SequenceInfo startSequence = new SequenceInfo();
                    startSequence.Start = docLine.Info.Start + docLine.Offset;
                    startSequence.Line = docLine.Info.Line;
                    startSequence.Length = 1;

                    // Command
                    Keyword keyword = ParseCommand(ref docLine);
                    if (keyword != null)
                    {
                        if ("defgroup".Equals(keyword.Value))
                        {
                            GroupEntity groupEntity = ParseDefGroup(startSequence, ref docLine, keyword.Value);
                            parseState.CurrentGroup = groupEntity;
                        }
                        else if ("{".Equals(keyword.Value))
                        {
                            if (parseState.CurrentGroup == null)
                                throw new Exception("No 'defgroup' found!");
                            while (parseState.Top != null)
                                parseState.Pop();
                            Entity top = parseState.Top;
                            parseState.Push(parseState.CurrentGroup);

                        }
                        else if ("}".Equals(keyword.Value))
                        {
                            GroupEntity groupEntity = parseState.Top as GroupEntity;
                            if (groupEntity == null)
                                throw new Exception("Cannot close a non-opened group!");
                            parseState.Pop();
                        }
                    }
                }
            }
        }

        private void ParseDoxygenLine(ParseState parseState)
        {
            LineState pageLine = parseState.CreateLineState();
            SkipWhitespaces(ref pageLine);
            if (!pageLine.IsEndOfLine)
            {
                if ('@'.Equals(pageLine.CurrentChar))
                {
                    SequenceInfo startSequence = new SequenceInfo();
                    startSequence.Start = pageLine.Info.Start + pageLine.Offset;
                    startSequence.Line = pageLine.Info.Line;
                    startSequence.Length = 1;

                    // Command
                    Keyword keyword = ParseCommand(ref pageLine);
                    if (keyword != null)
                    {
                        if ("mainpage".Equals(keyword.Value) || "page".Equals(keyword.Value))
                            ParsePage(parseState, startSequence, ref pageLine, keyword.Value);
                        else if ("section".Equals(keyword.Value) || "subsection".Equals(keyword.Value))
                            ParseSection(parseState, startSequence, ref pageLine, keyword.Value);
                        else if ("headerfile".Equals(keyword.Value))
                            ParseHeaderFile(parseState, startSequence, ref pageLine, keyword.Value);
                    }
                }
            }
        }

        public ParseState Parse(string sourceText)
        {
            //
            // 1.) Parse line infos (Start and length for each line)
            //
            ParseState result = new ParseState(sourceText);
            StringBuilder lineBuffer = new StringBuilder();
            int sourceCodeLen = sourceText.Length;
            int sourceCodePos = 0;
            int lineIndex = 0;
            SequenceInfo currentLineInfo = new SequenceInfo();
            while (sourceCodePos < sourceCodeLen)
            {
                char c = sourceText[sourceCodePos];
                if (c == '\n')
                {
                    currentLineInfo.Length = lineBuffer.Length;
                    currentLineInfo.Line = lineIndex;
                    result.LineInfos.Add(currentLineInfo);

                    currentLineInfo = new SequenceInfo();
                    currentLineInfo.Start = sourceCodePos + 1;

                    lineBuffer.Clear();
                    ++lineIndex;
                }
                else
                    lineBuffer.Append(c);
                ++sourceCodePos;
            }

            //
            // 2.) Parse each line
            //
            Entity rootEntity = new RootEntity();

            ParseMode mode = ParseMode.None;
            while (result.LinePos < result.LineCount)
            {
                SequenceInfo lineInfo = result.LineInfos[result.LinePos];
                string line = sourceText.Substring(lineInfo.Start, lineInfo.Length);
                if (mode == ParseMode.None)
                {
                    if (line.TrimStart().StartsWith("/*!"))
                    {
                        // Doxygen block starts
                        mode = ParseMode.InsideDoxygenBlock;
                        result.NextLine();
                        continue;
                    }
                    else if (line.TrimStart().StartsWith("/**"))
                    {
                        if (line.TrimEnd().EndsWith("*/"))
                        {
                            LineState docLine = result.CreateLineState();
                            docLine.IncOffset(3);
                            ParseJavaDocLine(result, docLine);
                        }
                        else
                        {
                            // Java-doc starts
                            mode = ParseMode.InsideJavaDocBlock;
                        }
                        result.NextLine();
                        continue;
                    }
                    else if (line.TrimStart().StartsWith("//!"))
                    {
                        // Single line documentation block
                        result.NextLine();
                        continue;
                    }
                }
                if (mode == ParseMode.InsideDoxygenBlock || mode == ParseMode.InsideJavaDocBlock)
                {
                    // Either Java-doc or Doxygen block is finished?
                    if (line.TrimStart().StartsWith("*/"))
                    {
                        mode = ParseMode.None;
                        result.NextLine();
                        continue;
                    }
                }
                if (mode == ParseMode.InsideDoxygenBlock)
                    ParseDoxygenLine(result);
                else if (mode == ParseMode.InsideJavaDocBlock)
                {
                    LineState pageLine = result.CreateLineState();
                    ParseJavaDocLine(result, pageLine);
                }
                result.NextLine();
            }
            result.Finished();
            return (result);
        }
    }
}
