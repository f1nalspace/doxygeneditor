using DoxygenEditor.Parser.Entities;
using System.Diagnostics;
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

        private string ParseIdentifierString(ref LineState line)
        {
            if (!line.IsEndOfLine && (char.IsLetter(line.CurrentChar) || line.CurrentChar == '_'))
            {
                string result = "";
                while (!line.IsEndOfLine && (char.IsLetterOrDigit(line.CurrentChar) || line.CurrentChar == '_'))
                {
                    result += line.CurrentChar;
                    line.IncOffset();
                }
                return (result);
            }
            return null;
        }
        private Keyword ParseIdentifierKeyword(ref LineState line)
        {
            if (!line.IsEndOfLine && (char.IsLetter(line.CurrentChar) || line.CurrentChar == '_'))
            {
                Keyword result = new Keyword() { Start = line.Offset };
                string ident = ParseIdentifierString(ref line);
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
            Keyword result = ParseIdentifierKeyword(ref line);
            return (result);
        }

        private void ParsePage(ParseState state, SequenceInfo startInfo, ref LineState line, string pageCommand)
        {
            SkipWhitespaces(ref line);
            PageEntity pageEntity;
            if ("page".Equals(pageCommand))
            {
                // Identifier
                Keyword pageIdentKeyword = ParseIdentifierKeyword(ref line);
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
            Keyword sectionIdentKeyword = ParseIdentifierKeyword(ref line);
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

            bool insideDoxygenComment = false;
            while (result.LinePos < result.LineCount)
            {
                SequenceInfo lineInfo = result.LineInfos[result.LinePos];
                string line = sourceText.Substring(lineInfo.Start, lineInfo.Length);
                if (!insideDoxygenComment)
                {
                    if (line.TrimStart().StartsWith("/*!"))
                        insideDoxygenComment = true;
                    result.NextLine();
                    continue;
                }
                if (line.TrimStart().StartsWith("*/"))
                {
                    insideDoxygenComment = false;
                    result.NextLine();
                    continue;
                }
                Debug.Assert(insideDoxygenComment == true);
                LineState pageLine = result.GetCurrentLine();
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
                                ParsePage(result, startSequence, ref pageLine, keyword.Value);
                            else if ("section".Equals(keyword.Value) || "subsection".Equals(keyword.Value))
                                ParseSection(result, startSequence, ref pageLine, keyword.Value);
                        }
                    }
                }
                result.NextLine();
            }
            result.Finished();
            return (result);
        }
    }
}
