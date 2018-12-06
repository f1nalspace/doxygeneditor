using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace DoxygenEditor.Views
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(asm.Location);
            string appName = versionInfo.ProductName;
            string appVersion = versionInfo.FileVersion;
            string appCopyright = versionInfo.LegalCopyright;
            string appDescription = versionInfo.Comments;
            labelAppName.Text = appName;
            labelAppVersion.Text = $"Version {appVersion}";
            labelDescription.Text = appDescription;
            labelCopyright.Text = appCopyright;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
