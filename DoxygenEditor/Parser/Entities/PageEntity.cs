namespace DoxygenEditor.Parser.Entities
{
    public class PageEntity : Entity
    {
        public string PageId { get; }
        public string PageCaption { get; }
        public PageEntity(SequenceInfo lineInfo, string pageId, string pageCaption) : base(lineInfo)
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
        public MainPageEntity(SequenceInfo lineInfo) : base(lineInfo, null, "MainPage") { }
    }
}
