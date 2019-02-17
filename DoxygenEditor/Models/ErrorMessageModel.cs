namespace TSP.DoxygenEditor.Models
{
    public class ErrorMessageModel
    {
        public string Caption { get; }
        public string ShortText { get; }
        public string Details { get; set; }

        public ErrorMessageModel(string caption, string shortText)
        {
            Caption = caption;
            ShortText = shortText;
            Details = null;
        }
    }
}
