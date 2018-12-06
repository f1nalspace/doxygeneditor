namespace DoxygenEditor.Editor
{
    public interface IFileActions
    {
        void FileNew();
        void FileSave(string filePath);
        void FileLoad(string filePath);
    }
}
