namespace DoxygenEditor.Controls
{
    public interface ISearchReplaceControl
    {
        bool IsShown();
        void ShowSearchOnly(bool focus);
        void ShowSearchAndReplace(bool focus);
        void HideSearchReplace();
        void HideReplaceOnly();
    }
}
