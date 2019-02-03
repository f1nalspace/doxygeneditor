namespace DoxygenEditor.Lexers
{
    class BaseLexer
    {
        internal readonly SlidingTextBuffer Buffer;

        public BaseLexer(SourceBuffer source)
        {
            Buffer = new SlidingTextBuffer(source);
        }
    }
}
