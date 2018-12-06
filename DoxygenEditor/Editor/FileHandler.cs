using DoxygenEditor.Editor;
using DoxygenEditor.MVVM;
using DoxygenEditor.Services;

namespace DoxygenEditor.Editor
{
    public class FileHandler : BindableBase, IFileHandler
    {
        private bool _isChanged;
        public bool IsChanged
        {
            get { return _isChanged; }
            set { _isChanged = value; RaisePropertyChanged(() => IsChanged); }
        }

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; RaisePropertyChanged(() => FilePath); }
        }

        private readonly IFileActions _actions;
        private readonly string _filter;
        private readonly string _ext;

        public FileHandler(IFileActions actions, string filter, string ext)
        {
            _actions = actions;
            _filter = filter;
            _ext = ext;
        }

        public bool SaveWithConfirmation()
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                IDialogService dlgService = IOCContainer.Default.Get<IDialogService>();
                string newFilePath = dlgService.SaveFile("Save file", _filter, _ext, null);
                if (string.IsNullOrEmpty(newFilePath))
                    return (false);
                FilePath = newFilePath;
            }
            _actions.FileSave(_filePath);
            IsChanged = false;
            return (true);
        }

        public bool CloseConfirmation()
        {
            if (IsChanged)
            {
                var msgService = IOCContainer.Default.Get<IMessageBoxService>();
                var msgRes = msgService.Show("File has been changed. Do you want to save it?", "File save confirmation", MsgBoxButtons.YesNoCancel, MsgBoxIcon.Question);
                if (msgRes == MsgBoxResult.Yes)
                    return SaveWithConfirmation();
                else if (msgRes == MsgBoxResult.No)
                    return (true);
                else
                    return (false);
            }
            else
                return (true);
        }

        public bool SaveAs()
        {
            IDialogService dlgService = IOCContainer.Default.Get<IDialogService>();
            string newFilePath = dlgService.SaveFile("Save file as", _filter, _ext, FilePath);
            if (string.IsNullOrEmpty(newFilePath))
                return (false);
            _actions.FileSave(newFilePath);
            FilePath = newFilePath;
            IsChanged = false;
            return (true);
        }

        private bool Save()
        {
            bool result = SaveWithConfirmation();
            return (result);
        }

        public bool Open(string filePath)
        {
            if (!CloseConfirmation())
                return(false);
            string newFilePath;
            if (filePath == null)
            {
                IDialogService dlgService = IOCContainer.Default.Get<IDialogService>();
                newFilePath = dlgService.OpenFile("Open file", _filter, _ext, FilePath);
            }
            else
                newFilePath = filePath;
            if (!string.IsNullOrEmpty(newFilePath))
            {
                _actions.FileLoad(newFilePath);
                FilePath = newFilePath;
                IsChanged = false;
                return (true);
            }
            return (false);
        }

        public bool New()
        {
            if (!CloseConfirmation())
                return(false);
            _actions.FileNew();
            FilePath = null;
            IsChanged = false;
            return (true);
        }
    }
}
