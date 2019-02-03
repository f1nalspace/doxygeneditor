namespace DoxygenEditor.SearchReplace
{
    public enum SearchDirection
    {
        Prev,
        Next,
    }

    public enum ReplaceMode
    {
        Next,
        All
    }

    public delegate void ExecuteSearchEventHandler(object sender, SearchDirection direction);
    public delegate void ExecuteReplaceEventHandler(object sender, ReplaceMode mode);
}
