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
            var appVersion = asm.GetName().Version.ToString();
            var appName = asm.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Unknown Product";
            var appCopyright = asm.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "No Copyright";
            var appDescription = asm.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "No Description";

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
