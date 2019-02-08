namespace TSP.DoxygenEditor.Lexers.Html
{
    enum HtmlTokenType
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
