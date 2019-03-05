namespace TSP.DoxygenEditor.Languages.Html
{
    public enum HtmlTokenKind
    {
        Invalid = -1,
        EOF,
        MetaTagStart,
        MetaTagStartAndClose,
        MetaTagClose,
        TagChars,
        TagName,
        AttrName,
        AttrValue,
        AttrChars,
    }
}
