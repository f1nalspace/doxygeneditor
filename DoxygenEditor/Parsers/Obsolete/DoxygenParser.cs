using TSP.DoxygenEditor.Parsers.Obsolete.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TSP.DoxygenEditor.Parsers.Obsolete
{
    public class DoxygenParser
    {
        protected string ParseIdentifierString(TextBuffer buffer, HashSet<char> additionalChars)
        {
            if (!buffer.IsEOF && (char.IsLetter(buffer.CurrentChar) || additionalChars.Contains(buffer.CurrentChar)))
            {
                string result = "";
                while (!buffer.IsEOF && (char.IsLetterOrDigit(buffer.CurrentChar) || additionalChars.Contains(buffer.CurrentChar)))
                {
                    result += buffer.CurrentChar;
                    buffer.Skip();
                }
                return (result);
            }
            return (null);
        }

        protected Token ParseIdentifierKeyword(TextBuffer buffer, HashSet<char> additionalChars)
        {
            if (!buffer.IsEOF && (char.IsLetter(buffer.CurrentChar) || additionalChars.Contains(buffer.CurrentChar)))
            {
                Token result = new Token() { Offset = buffer.Offset };
                string ident = ParseIdentifierString(buffer, additionalChars);
                result.Length = result.Offset - result.Offset;
                result.Value = ident;
                return (result);
            }
            return null;
        }

        protected Token ParseRemainingKeyword(TextBuffer buffer)
        {
            Token result = new Token() { Offset = buffer.Offset };
            result.Length = buffer.Length - buffer.Offset;
            result.Value = buffer.Source.Substring(result.Offset, result.Length);
            buffer.Skip(result.Length);
            return (result);
        }

        private Token ParseCommand(TextBuffer buffer)
        {
            Debug.Assert(buffer.CurrentChar == '@' || buffer.CurrentChar == '\\');
            buffer.Skip();
            Token result = ParseIdentifierKeyword(buffer, new HashSet<char> { '_', '{', '}' });
            return (result);
        }

        private void ParsePage(ParseTree state, SequenceInfo startInfo, LineBuffer buffer, string pageCommand)
        {
            buffer.SkipWhitespaces();
            PageEntity pageEntity;
            if ("page".Equals(pageCommand))
            {
                // Identifier
                Token pageIdentKeyword = ParseIdentifierKeyword(buffer, new HashSet<char> { '_' });
                if (pageIdentKeyword == null)
                {
                    state.AddMessage(ParseMessage.MessageType.Warning, buffer.Info, "Page requires a identifier!");
                    return;
                }
                string pageIdent = pageIdentKeyword.Value;

                // Caption
                buffer.SkipWhitespaces();
                Token pageCaptionKeyword = ParseRemainingKeyword(buffer);
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

        private void ParseSection(ParseTree state, SequenceInfo startInfo, LineBuffer buffer, string sectionCommand)
        {
            // Identifier
            buffer.SkipWhitespaces();
            Token sectionIdentKeyword = ParseIdentifierKeyword(buffer, new HashSet<char> { '_' });
            if (sectionIdentKeyword == null) return;
            string sectionIdent = sectionIdentKeyword.Value;

            // Caption
            string sectionCaption = null;
            if (!buffer.IsEOF)
            {
                buffer.SkipWhitespaces();
                Token sectionCaptionKeyword = ParseRemainingKeyword(buffer);
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

        private GroupEntity ParseDefGroup(SequenceInfo startInfo, LineBuffer line, string command)
        {
            // Identifier
            line.SkipWhitespaces();
            Token identKeyword = ParseIdentifierKeyword(line, new HashSet<char> { '_' });
            if (identKeyword == null) return null;
            string ident = identKeyword.Value;

            // Caption
            string caption = null;
            if (!line.IsEOF)
            {
                line.SkipWhitespaces();
                Token captionKeyword = ParseRemainingKeyword(line);
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

        private void ParseDoxygenLine(ParseTree parseState)
        {
            LineBuffer pageLine = parseState.CreateLineBuffer();
            pageLine.SkipWhitespaces();
            pageLine.SkipCharacters(new HashSet<char> { '*' });
            pageLine.SkipWhitespaces();
            if (!pageLine.IsEOF)
            {
                if ('@'.Equals(pageLine.CurrentChar) || '\\'.Equals(pageLine.CurrentChar))
                {
                    SequenceInfo startSequence = new SequenceInfo();
                    startSequence.Start = pageLine.Info.Start + pageLine.Offset;
                    startSequence.Line = pageLine.Info.Line;
                    startSequence.Length = 1;

                    // Command
                    Token keyword = ParseCommand(pageLine);
                    if (keyword != null)
                    {
                        if ("mainpage".Equals(keyword.Value) || "page".Equals(keyword.Value))
                            ParsePage(parseState, startSequence, pageLine, keyword.Value);
                        else if ("section".Equals(keyword.Value) || "subsection".Equals(keyword.Value))
                            ParseSection(parseState, startSequence, pageLine, keyword.Value);
                        else if ("defgroup".Equals(keyword.Value))
                        {
                            GroupEntity groupEntity = ParseDefGroup(startSequence, pageLine, keyword.Value);
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
                            parseState.AddGroup(parseState.CurrentGroup);

                        }
                        else if ("}".Equals(keyword.Value))
                        {
                            GroupEntity groupEntity = parseState.Top as GroupEntity;
                            if (groupEntity == null)
                                throw new Exception("Cannot close a non-opened group!");
                            parseState.Pop();
                        }
                        else
                        {
                            Token remaining = ParseRemainingKeyword(pageLine);
                            ParamEntity paramEntity = new ParamEntity(startSequence, keyword.Value, remaining.Value?.Trim());
                            if (parseState.CurrentComment != null)
                                parseState.CurrentComment.AddChild(paramEntity);
                            else
                                Debug.WriteLine($"Unknown command: {keyword.Value}");
                        }
                    }
                }
            }
        }

        private readonly Regex functionDeclarationRex = new Regex("(^[^(]+)\\s*\\(([^)]*)\\)\\s*;", RegexOptions.Multiline);
        private readonly Regex typedefDeclarationRex = new Regex("(typedef\\s+(?:[a-zA-Z_][a-zA-Z0-9_]+))\\s+([a-zA-Z_][a-zA-Z0-9_]+)\\s*[{;]", RegexOptions.Multiline);
        private readonly Regex defineDeclarationRex = new Regex("#define\\s+([a-zA-Z_][a-zA-Z0-9_]+)", RegexOptions.Multiline);
        private DeclarationEntity ParseDeclarationBelow(ParseTree tree)
        {
            int linePos = tree.LinePos;
            SequenceInfo lineInfo = tree.LineInfos[linePos];
            string line = tree.SourceText.Substring(lineInfo.Start, lineInfo.Length);
            line = line.TrimStart();
            if (line.Length == 0) return (null);

            // Function
            Match m = functionDeclarationRex.Match(line);
            if (m.Success)
            {
                string everythingBefore = m.Groups[1].Value;
                string[] s = everythingBefore.Split(new char[] { ' ', '*' });
                string ident = s[s.Length - 1];
                int inc = (linePos - tree.LinePos) + 1;
                tree.NextLine(inc);
                return new DeclarationEntity(lineInfo, ident, DeclarationEntity.DeclType.Function);
            }

            // Typedef
            m = typedefDeclarationRex.Match(line);
            if (m.Success)
            {
                string everythingBefore = m.Groups[1].Value;
                string ident = m.Groups[2].Value;
                int inc = (linePos - tree.LinePos) + 1;
                tree.NextLine(inc);
                return new DeclarationEntity(lineInfo, ident, DeclarationEntity.DeclType.Typedef);
            }

            // Define
            m = defineDeclarationRex.Match(line);
            if (m.Success)
            {
                string ident = m.Groups[1].Value;
                int inc = (linePos - tree.LinePos) + 1;
                tree.NextLine(inc);
                return new DeclarationEntity(lineInfo, ident, DeclarationEntity.DeclType.Define);
            }

            return (null);
        }

        public ParseTree Parse(string sourceText)
        {
            //
            // 1.) Parse line infos (Start and length for each line)
            //
            ParseTree result = new ParseTree(sourceText);
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
                        result.CurrentComment = new CommentEntity(lineInfo);
                        mode = ParseMode.InsideDoxygenBlock;
                        result.NextLine();
                        continue;
                    }
                    else if (line.TrimStart().StartsWith("/**"))
                    {
                        if (line.TrimEnd().EndsWith("*/"))
                        {
                            // /* ... */ Single line comment style
                            result.CurrentComment = new CommentEntity(lineInfo);
                            LineBuffer docLine = result.CreateLineBuffer();
                            docLine.Skip(3);
                            ParseDoxygenLine(result);
                        }
                        else
                        {
                            // Java-doc starts
                            result.CurrentComment = new CommentEntity(lineInfo);
                            mode = ParseMode.InsideJavaDocBlock;
                        }
                        result.NextLine();
                        continue;
                    }
                    else if (line.TrimStart().StartsWith("//!"))
                    {
                        // Single line documentation block
                        result.CurrentComment = new CommentEntity(lineInfo);
                        result.NextLine();
                        DeclarationEntity signature = ParseDeclarationBelow(result);
                        if (signature != null)
                            result.CurrentComment.AddChild(signature);
                        result.PushComment();
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
                        DeclarationEntity signature = ParseDeclarationBelow(result);
                        if (signature != null)
                        {
                            Debug.Assert(result.CurrentComment != null);
                            result.CurrentComment.AddChild(signature);
                        }
                        result.PushComment();
                        continue;
                    }
                }
                if (mode == ParseMode.InsideDoxygenBlock || mode == ParseMode.InsideJavaDocBlock)
                    ParseDoxygenLine(result);
                result.NextLine();
            }
            result.Finished();
            return (result);
        }
    }
}
