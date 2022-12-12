using System;
using System.Windows.Forms;

namespace TSP.DoxygenEditor.Views
{
    public partial class DirectoryInputForm : Form
    {
        public string Path {
            get { return tbPath.Text; }
        }

        public delegate bool HasPathEventHandler(object sender, string path);
        public event HasPathEventHandler HasPath;

        public DirectoryInputForm(string initialPath)
        {
            InitializeComponent();
            tbPath.Text = initialPath;
        }

        private void btnChooseDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.SelectedPath = tbPath.Text;
            DialogResult r = dlg.ShowDialog();
            if (r == DialogResult.OK)
                tbPath.Text = dlg.SelectedPath;
        }

        private void DirectoryInputForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbPath.Text))
                e.Cancel = true;
            if (HasPath.Invoke(this, tbPath.Text))
                e.Cancel = true;
        }
    }
}
