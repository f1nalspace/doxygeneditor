using System.IO;
using System.Windows.Forms;

namespace DoxygenEditor.Services
{
    class WinFormsDialogService : IDialogService
    {
        private readonly IWin32Window _owner;

        public WinFormsDialogService(IWin32Window owner)
        {
            _owner = owner;
        }

        private string GetInitialDirectory(string path)
        {
            string result = null;
            if (!string.IsNullOrEmpty(path))
            {
                FileInfo f = new FileInfo(path);
                if ((f.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    result = f.FullName;
                else
                    result = f.Directory.FullName;
            }
            return (result);
        }
        private string GetInitialFileName(string path)
        {
            string result = null;
            if (!string.IsNullOrEmpty(path))
            {
                FileInfo f = new FileInfo(path);
                if ((f.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
                    result = f.Name;
            }
            return (result);
        }

        public string OpenFile(string caption, string filter, string ext, string defaultPath)
        {
            string initDir = GetInitialDirectory(defaultPath);
            string initFileName = GetInitialFileName(defaultPath);
            OpenFileDialog dlg = new OpenFileDialog()
            {
                Filter = filter,
                DefaultExt = ext,
                Multiselect = false,
                InitialDirectory = initDir,
                FileName = initFileName,
            };
            DialogResult r = dlg.ShowDialog(_owner);
            if (r == DialogResult.OK)
                return (dlg.FileName);
            return (null);
        }

        public string SaveFile(string caption, string filter, string ext, string defaultPath)
        {
            string initDir = GetInitialDirectory(defaultPath);
            string initFileName = GetInitialFileName(defaultPath);
            SaveFileDialog dlg = new SaveFileDialog()
            {
                Filter = filter,
                DefaultExt = ext,
                AddExtension = true,
                InitialDirectory = initDir,
                FileName = initFileName,
            };
            DialogResult r = dlg.ShowDialog(_owner);
            if (r == DialogResult.OK)
                return (dlg.FileName);
            return (null);
        }
    }
}
