/***
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
    ## v0.9.2.0:
    - Bugfix: Fixed htmlonly doxygen block was parsed as doxygen

    ## v0.9.1.0:
    - New: Grouping for issues and performance listview
    - New: Build dialog filter from the global configuration
    - New: Introduced editor file type to only parse/highlight supported files
    - New: Introduced cpp/doxygen symbol resolver
    - New: Caption for source symbol
    - New: Doxygen config lexing and parsing
    - New: Compile & open documentation dialog
    - New: Auto sort and column size for issues and performance listview
    - Change: Jump to documentation start instead of the entity start for issues
    - Change: Replaced all "var" with proper type in sources
    - Change: New version history style
    - Bugfix: Fixed several minor/major issues
    - Bugfix: Fixed captions in symbol search was not shown
    - Bugfix: Fixed wrong auto indent after linebreak hopefully
    - Bugfix: Fixed wrong colorization for doxygen code blocks

    ## v0.9.0.0:
    - Proper #define argument parsing & highlighting
    - Lex multi & single line comments inside preprocessor
    - Internal refactoring for editor state vs parse state (Context)
    - Highlight and jump to a symbol in the editor
    - Improved function detections
    - Support for symbol jumping between tabs
    - Fixed missing highlighting for visible lines
    - Introduced workspaces

    ## v0.8.1.0:
    - Bugfixes

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

-------------------------------------------------------------------------------
	TODO
-------------------------------------------------------------------------------

Cpp:
    - Types parsing (C99)
    - Types parsing (C++ templates)
    - C++ namespace symbol parsing
    - Struct member parsing
    - Function argument parsing (@param validation)
    - Function return parsing (@return validation -> void or not void)
    - Code completion

Doxygen:
    - Group parsing
    - Code completion

Lexers:
    - Incremental lexing for very fast syntax highlighting

Editor:
    - Spellchecking (I am still looking for a existing library for that)

UI:
    - Button for generating & viewing doxygen documentation

Workspace:
    - Custom symbol tables

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
