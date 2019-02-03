namespace DoxygenEditor.Models
{
    public class ErrorMessageModel
    {
        public string Text { get; }
        public string Caption { get; }

        public ErrorMessageModel(string text, string caption)
        {
            Text = text;
            Caption = caption;
        }
    }
}
