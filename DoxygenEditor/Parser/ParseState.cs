using DoxygenEditor.Parser.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DoxygenEditor.Parser
{
    public class ParseState
    {
        public string SourceText { get; protected set; }
        public int LinePos { get; private set; } = 0;
        public int LineCount { get { return LineInfos.Count; } }
        public readonly List<SequenceInfo> LineInfos = new List<SequenceInfo>();
        private readonly Stack<Entity> _entityStack = new Stack<Entity>();
        public readonly RootEntity RootEntity = new RootEntity();

        private readonly List<ParseMessage> _messages = new List<ParseMessage>();
        public IEnumerable<ParseMessage> Messages { get { return _messages; } }

        private readonly Stopwatch _watch;
        private TimeSpan _duration = new TimeSpan();
        public TimeSpan Duration
        {
            get { return _duration; }
        }

        public ParseState(string sourceText)
        {
            _watch = new Stopwatch();
            _watch.Start();
            SourceText = sourceText;
        }

        public void Finished()
        {
            _watch.Stop();
            _duration = _watch.Elapsed;
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
                AddMessage(ParseMessage.MessageType.Error, sectionEntity.LineInfo, $"Section '{sectionEntity}' must be parented to a page!");
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
                SequenceInfo lineInfo = LineInfos[LinePos];
                string value = SourceText.Substring(lineInfo.Start, lineInfo.Length);
                LineState result = new LineState(lineInfo, value);
                return (result);
            }
            else
                throw new InvalidOperationException("All lines are already consumed!");
        }

        public void AddMessage(ParseMessage.MessageType type, SequenceInfo lineInfo, string text)
        {
            _messages.Add(new ParseMessage(type, text, lineInfo));
        }
    }
}
