using System.ComponentModel;

namespace DoxygenEditor.Editor
{
    public interface IFileHandler : INotifyPropertyChanged
    {
        bool IsChanged { get; set; }
        string FilePath { get; set; }
        bool New();
        bool Open(string filePath);
        bool SaveAs();
        bool CloseConfirmation();
        bool SaveWithConfirmation();
    }
}
