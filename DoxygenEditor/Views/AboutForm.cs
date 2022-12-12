using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace TSP.DoxygenEditor.Views
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
#if DEBUG
            appVersion += " (Debug-Build)";
#else
            appVersion += " (Release-Build)";
#endif
            labelAppName.Text = appName;
            labelAppVersion.Text = $"Version {appVersion}";
            labelDescription.Text = appDescription;
            string licenseText = DoxygenEditor.Properties.Resources.LICENSE;
            licenseText = licenseText.Replace("\n", Environment.NewLine);
            tbLicense.Text = licenseText;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
