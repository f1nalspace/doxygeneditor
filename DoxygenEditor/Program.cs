using DoxygenEditor.Services;
using DoxygenEditor.Solid;
using DoxygenEditor.Views;
using System;
using System.Windows.Forms;

namespace DoxygenEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            IOCContainer.Register(new RegistryConfigurationService());
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm mainForm = new MainForm();
            Application.Run(mainForm);
        }
    }
}
