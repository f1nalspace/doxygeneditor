using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoxygenEditor
{
    public class DoxygenParser
    {
        public class SourceLineInfo
        {
            public int LineIndex = 0;
            public int LineStartPos = 0;
            public int LineLength = 0;
            public SourceLineInfo()
            {
            }
        }

        public class LineState
        {
            public SourceLineInfo Info { get; }
            public string Value { get; }
            public int Offset { get; private set; }
            public int Length { get; set; }
            public char CurrentChar
            {
                get
                {
                    return Value[Offset];
                }
            }
            public bool IsEndOfLine
            {
                get
                {
                    bool result = !(Offset < Length);
                    return (result);
                }
            }
            public string RemainingValue
            {
                get
                {
                    int len = Length - Offset;
                    string result = Value.Substring(Offset, len);
                    return (result);
                }
            }
            public LineState(SourceLineInfo info, string value)
            {
                Info = info;
                Value = value;
                Offset = 0;
                Length = value.Length;
            }
            public void IncOffset(int addon = 1)
            {
                if (Offset < Length - (addon - 1))
                    Offset += addon;
                else
                    throw new InvalidOperationException("End of line reached!");
            }
            public override string ToString()
            {
                return Value;
            }
        }

        public abstract class Entity
        {
            public Entity Parent { get; protected set; }
            private readonly List<Entity> _children = new List<Entity>();
            public IEnumerable<Entity> Children { get { return _children; } }
            public SourceLineInfo LineInfo { get; }
            public Entity(SourceLineInfo lineInfo)
            {
                LineInfo = lineInfo;
                Parent = null;
            }
            public void AddChild(Entity child)
            {
                child.Parent = this;
                _children.Add(child);
            }
        }

        public class PageEntity : Entity
        {
            public string PageId { get; }
            public string PageCaption { get; }
            public PageEntity(SourceLineInfo lineInfo, string pageId, string pageCaption) : base(lineInfo)
            {
                PageId = pageId;
                PageCaption = pageCaption;
            }
            public override string ToString()
            {
                return $"{PageId} {PageCaption}";
            }
        }
        public class MainPageEntity : PageEntity
        {
            public MainPageEntity(SourceLineInfo lineInfo) : base(lineInfo, null, "MainPage") { }
        }

        public class RootEntity : Entity
        {
            public RootEntity() : base(null)
            {

            }
        }

        public class SectionEntity : Entity
        {
            public string SectionId { get; }
            public string SectionCaption { get; }
            public SectionEntity(SourceLineInfo lineInfo, string id, string caption) : base(lineInfo)
            {
                SectionId = id;
                SectionCaption = caption;
            }
        }

        public class SubSectionEntity : SectionEntity
        {
            public SubSectionEntity(SourceLineInfo lineInfo, string id, string caption) : base(lineInfo, id, caption)
            {
            }
        }

        public enum MessageType
        {
            Error = 0,
            Warning,
        }

        public class Message
        {
            public MessageType Type { get; }
            public string Text { get; }
            public SourceLineInfo LineInfo { get; }

            public Message(MessageType type, string text, SourceLineInfo lineInfo)
            {
                Type = type;
                Text = text;
                LineInfo = lineInfo;
            }

            public override string ToString()
            {
                return $"{Type}[{LineInfo.LineIndex}:{LineInfo.LineStartPos}] {Text}";
            }
        }

        public class Context
        {
            public string SourceText { get; }
            public int LinePos { get; private set; } = 0;
            public int LineCount { get { return LineInfos.Count; } }
            public readonly List<SourceLineInfo> LineInfos = new List<SourceLineInfo>();
            private readonly Stack<Entity> _entityStack = new Stack<Entity>();
            public readonly RootEntity RootEntity = new RootEntity();

            private readonly List<Message> _messages = new List<Message>();
            public IEnumerable<Message> Messages { get { return _messages; } }

            public Context(string sourceText)
            {
                SourceText = sourceText;
            }

            public Entity Top
            {
                get
                {
                    if (_entityStack.Count > 0)
                        return _entityStack.Peek();
                    return null;
                }
            }

            public void AddPage(Entity pageEntity)
            {
                _entityStack.Clear();
                _entityStack.Push(pageEntity);
                RootEntity.AddChild(pageEntity);
            }

            private void ClearUntilType(Type type)
            {
                while (_entityStack.Count > 0)
                {
                    Entity p = _entityStack.Peek();
                    if (!type.IsInstanceOfType(p))
                        _entityStack.Pop();
                    else
                        break;
                }
            }

            public void AddSection(Entity sectionEntity)
            {
                if (typeof(SectionEntity).Equals(sectionEntity.GetType()))
                    ClearUntilType(typeof(PageEntity));
                else
                {
                    Debug.Assert(typeof(SubSectionEntity).Equals(sectionEntity.GetType()));
                    ClearUntilType(typeof(SectionEntity));
                }
                if (_entityStack.Count > 0)
                {
                    Entity pageEntity = _entityStack.Peek();
                    pageEntity.AddChild(sectionEntity);
                    _entityStack.Push(sectionEntity);
                }
                else
                    AddMessage(MessageType.Error, sectionEntity.LineInfo, $"Section '{sectionEntity}' must be parented to a page!");
            }

            public void Push(Entity entity)
            {
                _entityStack.Push(entity);
            }
            public Entity Pop()
            {
                Debug.Assert(_entityStack.Count > 0);
                Entity result = _entityStack.Pop();
                return (result);
            }

            public void NextLine()
            {
                if (LinePos < LineCount)
                    ++LinePos;
                else
                    throw new InvalidOperationException("All lines are already consumed!");
            }

            public LineState GetCurrentLine()
            {
                if (LinePos < LineCount)
                {
                    SourceLineInfo lineInfo = LineInfos[LinePos];
                    string value = SourceText.Substring(lineInfo.LineStartPos, lineInfo.LineLength);
                    LineState result = new LineState(lineInfo, value);
                    return (result);
                }
                else
                    throw new InvalidOperationException("All lines are already consumed!");
            }

            public void AddMessage(MessageType type, SourceLineInfo lineInfo, string text)
            {
                _messages.Add(new Message(type, text, lineInfo));
            }
        }

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

        private void ParsePage(Context state, ref LineState line, string pageCommand)
        {
            SkipWhitespaces(ref line);
            PageEntity pageEntity;
            if ("page".Equals(pageCommand))
            {
                // Identifier
                Keyword pageIdentKeyword = ParseIdentifierKeyword(ref line);
                if (pageIdentKeyword == null)
                {
                    state.AddMessage(MessageType.Warning, line.Info, "Page requires a identifier!");
                    return;
                }
                string pageIdent = pageIdentKeyword.Value;

                // Caption
                SkipWhitespaces(ref line);
                Keyword pageCaptionKeyword = ParseRemainingKeyword(ref line);
                string pageCaption = pageCaptionKeyword.Value;
                pageEntity = new PageEntity(line.Info, pageIdent, pageCaption);
            }
            else
            {
                // Main Page
                Debug.Assert("mainpage".Equals(pageCommand));
                pageEntity = new MainPageEntity(line.Info);
            }
            state.AddPage(pageEntity);
        }

        private void ParseSection(Context state, ref LineState line, string sectionCommand)
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
                sectionEntity = new SectionEntity(line.Info, sectionIdent, sectionCaption);
            else
            {
                Debug.Assert("subsection".Equals(sectionCommand));
                sectionEntity = new SubSectionEntity(line.Info, sectionIdent, sectionCaption);
            }
            state.AddSection(sectionEntity);
        }

        public Context Parse(string sourceText)
        {
            //
            // 1.) Parse line infos (Start and length for each line)
            //
            Context result = new Context(sourceText);
            StringBuilder lineBuffer = new StringBuilder();
            int sourceCodeLen = sourceText.Length;
            int sourceCodePos = 0;
            int lineIndex = 0;
            SourceLineInfo currentLineInfo = new SourceLineInfo();
            while (sourceCodePos < sourceCodeLen)
            {
                char c = sourceText[sourceCodePos];
                if (c == '\n')
                {
                    currentLineInfo.LineLength = lineBuffer.Length;
                    currentLineInfo.LineIndex = lineIndex;
                    result.LineInfos.Add(currentLineInfo);

                    currentLineInfo = new SourceLineInfo();
                    currentLineInfo.LineStartPos = sourceCodePos + 1;

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
                SourceLineInfo lineInfo = result.LineInfos[result.LinePos];
                string line = sourceText.Substring(lineInfo.LineStartPos, lineInfo.LineLength);
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
                        // Command
                        SkipWhitespaces(ref pageLine);
                        Keyword keyword = ParseCommand(ref pageLine);
                        if (keyword != null)
                        {
                            if ("mainpage".Equals(keyword.Value) || "page".Equals(keyword.Value))
                                ParsePage(result, ref pageLine, keyword.Value);
                            else if ("section".Equals(keyword.Value) || "subsection".Equals(keyword.Value))
                                ParseSection(result, ref pageLine, keyword.Value);
                        }
                    }
                }
                result.NextLine();
            }
            return (result);
        }
    }
}
