namespace DoxygenEditor.Services
{
    interface IDialogService
    {
        string OpenFile(string caption, string filter, string ext, string defaultPath);
        string SaveFile(string caption, string filter, string ext, string defaultPath);
    }
}
