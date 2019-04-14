namespace TSP.DoxygenEditor.Editor
{
    interface IParseControl
    {
        bool IsParsing();
        void StartParsing(string text);
        void StopParsing();
    }
}
