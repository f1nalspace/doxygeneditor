﻿/***
Visual-Editor for authoring and validating doxygen comments/documentation.

-------------------------------------------------------------------------------
	About
-------------------------------------------------------------------------------

A simple C# based editor for Doxygen Documentation files, based on Scintilla.

-------------------------------------------------------------------------------
	License
-------------------------------------------------------------------------------

Doxygen-Editor is released under the following license:

MIT License

Copyright (c) 2018 Torsten Spaete

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

-------------------------------------------------------------------------------
	Version History
-------------------------------------------------------------------------------

    ## v0.8.0.0:
    - Finished doxygen lexer
    - Finished doxygen parser
    - Finished cpp lexer
    - Finished cpp lexer
    - Improved highlighting performance
    - Improved editor UI a lot
    - Changed to store configuration into xml file instead of registry
    - Separated c/c++ and doxygen issues
    - Fixed a ton parsing bugs
    - Fixed a ton editor bugs
    - Added a symbol cache for validating symbols
    - Added performance tab
    - Added nice filtering for issues

    ## v0.7.0.0:
    - Initial version

***/

using TSP.DoxygenEditor.Services;
using TSP.DoxygenEditor.Solid;
using TSP.DoxygenEditor.Views;
using System;
using System.Windows.Forms;
using TSP.DoxygenEditor.ErrorDialog;

namespace TSP.DoxygenEditor
{
    static class Program
    {
        static void ShowUnhandledException(Exception exception)
        {
            ErrorDialogForm dialog = new ErrorDialogForm();
            dialog.Title = "Unexpected Exception";
            dialog.ShortText = exception.Message;
            dialog.Text = exception.ToString();
            IWin32Window owner = Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null;
            dialog.ShowDialog(owner);
            Application.Exit();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                ShowUnhandledException(((Exception)e.ExceptionObject));
            };
            Application.ThreadException += (s, e) =>
            {
                ShowUnhandledException(e.Exception);
            };
            IOCContainer.Register(new XMLConfigurationService());
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
