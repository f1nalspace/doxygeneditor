using DoxygenEditor.MVVM;
using DoxygenEditor.Services;
using DoxygenEditor.ViewModels;
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
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm mainForm = new MainForm();
            IOCContainer.Default.Register(new WinFormsMessageBoxService(mainForm));
            IOCContainer.Default.Register(new WinFormsDialogService(mainForm));
            Application.Run(mainForm);
        }
    }
}
