namespace TSP.DoxygenEditor.Editor
{
    interface IParseControl
    {
        bool IsParsing { get; }
        void StartParsing(string text);
        void StopParsing();
    }
}
