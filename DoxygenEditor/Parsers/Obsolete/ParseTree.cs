using TSP.DoxygenEditor.Models;
using TSP.DoxygenEditor.Parsers.Obsolete.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TSP.DoxygenEditor.Parsers.Obsolete
{
    public class ParseTree : IDisposable
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

        public GroupEntity CurrentGroup { get; set; }
        public CommentEntity CurrentComment { get; set; }

        public ParseTree(string sourceText)
        {
            _watch = new Stopwatch();
            _watch.Start();
            SourceText = sourceText;
        }

        private void CollectEntities(Entity rootEntity, ref List<Entity> list)
        {
            foreach (Entity childEntity in rootEntity.Children)
            {
                list.Add(childEntity);
                CollectEntities(childEntity, ref list);
            }
        }

        public IEnumerable<Entity> GetAllEntities()
        {
            List<Entity> result = new List<Entity>();
            CollectEntities(RootEntity, ref result);
            return (result);
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

        public void PushComment()
        {
            Debug.Assert(CurrentComment != null);
            Entity t = Top;
            if (t == null)
                RootEntity.AddChild(CurrentComment);
            else
                t.AddChild(CurrentComment);
            CurrentComment = null;
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

        public void NextLine(int inc = 1)
        {
            if (LinePos + (inc - 1) <= LineCount)
                LinePos += inc;
            else
                throw new InvalidOperationException("All lines are already consumed!");
        }

        public LineBuffer CreateLineBuffer()
        {
            if (LinePos < LineCount)
            {
                SequenceInfo lineInfo = LineInfos[LinePos];
                string value = SourceText.Substring(lineInfo.Start, lineInfo.Length);
                LineBuffer result = new LineBuffer(lineInfo, value);
                return (result);
            }
            else
                throw new InvalidOperationException("All lines are already consumed!");
        }

        public void AddMessage(ParseMessage.MessageType type, SequenceInfo lineInfo, string text)
        {
            _messages.Add(new ParseMessage(type, text, lineInfo));
        }

        public void Dispose()
        {
        }

        public void AddGroup(GroupEntity group)
        {
            RootEntity.AddChild(group);
        }
    }
}
