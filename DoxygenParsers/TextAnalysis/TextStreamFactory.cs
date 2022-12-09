using System;
using System.Collections.Generic;

namespace TSP.DoxygenEditor.TextAnalysis
{
    public class TextStreamFactory : ITextStreamFactory
    {
        private static readonly List<ITextStreamFactory> _list = new List<ITextStreamFactory>();
        private static readonly Dictionary<ITextStreamFactory, Func<string, bool>> _funcs = new Dictionary<ITextStreamFactory, Func<string, bool>>();
        private static readonly ITextStreamFactory _defaultFactory = new TextStreamFactory();

        public static void Register(ITextStreamFactory factory, Func<string, bool> useFunc)
        {
            if (!_funcs.ContainsKey(factory))
            {
                _funcs.Add(factory, useFunc);
                _list.Add(factory);
            }
        }

        public static void Unregister(ITextStreamFactory factory)
        {
            if (_funcs.Remove(factory))
                _list.Remove(factory);
        }

        public static ITextStream Create(string source, int index, int length, TextPosition pos)
        {
            ITextStreamFactory factory = _defaultFactory;
            foreach (ITextStreamFactory item in _list)
            {
                if (_funcs.TryGetValue(item, out Func<string, bool> func))
                {
                    if (func(source))
                    {
                        factory = item;
                        break;
                    }
                }
            }
            ITextStream result = factory.Create(source, index, length, pos);
            return result;
        }

        protected virtual ITextStream CreateStream(string source, int index, int length, TextPosition pos)
            => new BasicTextStream(source, index, length, pos);

        ITextStream ITextStreamFactory.Create(string source, int index, int length, TextPosition pos)
            => CreateStream(source, index, length, pos);

        public override string ToString() => "Basic";
    }
}
