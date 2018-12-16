using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DoxygenEditor.Views
{
    public partial class MainForm : Form
    {
        class TabEditorState
        {
            public TabPage Tab { get; }
            public ScintillaNET.Scintilla Editor { get; }
            public string FilePath { get; set; }
            public string Name { get; set; }
            public bool IsChanged { get; set; }
            public Encoding FileEncoding { get; set; }
            public int MaxLineNumberCharLength { get; set; }

            public TabEditorState(TabPage tab, ScintillaNET.Scintilla editor)
            {
                Tab = tab;
                Editor = editor;
                FilePath = null;
                Name = null;
                IsChanged = false;
                FileEncoding = Encoding.UTF8;
                MaxLineNumberCharLength = 0;
            }
        }

        public MainForm()
        {
            InitializeComponent();

        }

        private readonly Regex _rexIndexFromName = new Regex("(?<index>[0-9]+)$", RegexOptions.Compiled);
        private string GetNextName(string prefix)
        {
            int highIndex = 0;
            foreach (TabPage tab in tcFiles.TabPages)
            {
                TabEditorState tabState = (TabEditorState)tab.Tag;
                if (tabState.FilePath == null)
                {
                    string name = tabState.Name;
                    Match m = _rexIndexFromName.Match(name);
                    if (m.Success)
                    {
                        int testIndex = int.Parse(m.Groups["index"].Value);
                        if (testIndex > highIndex)
                            highIndex = testIndex;
                    }
                }
            }
            string result = $"{prefix}{highIndex + 1}";
            return (result);
        }

        private void SetupEditor(ScintillaNET.Scintilla editor)
        {
            editor.WrapMode = ScintillaNET.WrapMode.None;
            editor.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            editor.CaretLineVisible = true;
            editor.CaretLineBackColorAlpha = 50;
            editor.CaretLineBackColor = Color.CornflowerBlue;
            editor.TabWidth = editor.IndentWidth = 4;
            editor.Margins[0].Width = 16;
            editor.ViewWhitespace = ScintillaNET.WhitespaceMode.Invisible;
            editor.UseTabs = true;

            Font editorFont = new Font(FontFamily.GenericMonospace, 14.0f, FontStyle.Regular);
            editor.StyleResetDefault();
            editor.Styles[ScintillaNET.Style.Default].Font = editorFont.Name;
            editor.Styles[ScintillaNET.Style.Default].Size = (int)editorFont.SizeInPoints;
            editor.StyleClearAll();

            editor.TextChanged += (s, e) =>
            {
                ScintillaNET.Scintilla thisEditor = (ScintillaNET.Scintilla)s;
                TabEditorState tabState = (TabEditorState)thisEditor.Parent.Tag;

                // Autofit left-margin to fit-in line count number
                int maxLineNumberCharLength = thisEditor.Lines.Count.ToString().Length;
                if (maxLineNumberCharLength != tabState.MaxLineNumberCharLength)
                {
                    thisEditor.Margins[0].Width = thisEditor.TextWidth(ScintillaNET.Style.LineNumber, new string('9', maxLineNumberCharLength + 1));
                    tabState.MaxLineNumberCharLength = maxLineNumberCharLength;
                }

                tabState.IsChanged = true;
                UpdateTabState(tabState);
            };
        }

        private TabEditorState AddFileTab(string name)
        {
            ScintillaNET.Scintilla editor = new ScintillaNET.Scintilla()
            {
                Dock = DockStyle.Fill,
            };
            SetupEditor(editor);
            TabPage newTab = new TabPage() { Text = name };
            TabEditorState newState = new TabEditorState(newTab, editor) { Name = name };
            newTab.Tag = newState;
            newTab.Controls.Add(editor);
            tcFiles.TabPages.Add(newTab);
            return (newState);
        }

        private void MenuActionFileNew(object sender, EventArgs e)
        {
            string name = GetNextName("File");
            TabEditorState newState = AddFileTab(name);
            tcFiles.SelectedIndex = tcFiles.TabPages.IndexOf(newState.Tab);
            if (newState.Editor.CanFocus) newState.Editor.Focus();
            UpdateTabState(newState);
        }

        private void MenuActionFileOpen(object sender, EventArgs e)
        {
            if (dlgOpenFile.ShowDialog() == DialogResult.OK)
            {
                string filePath = dlgOpenFile.FileName;
                TabEditorState newState = AddFileTab(Path.GetFileName(filePath));
                tcFiles.SelectedIndex = tcFiles.TabPages.IndexOf(newState.Tab);
                if (!OpenFile(newState, filePath))
                {
                    // @TODO(final): Handle error when file could not be opened
                    tcFiles.TabPages.Remove(newState.Tab);
                }
                else
                {
                    if (newState.Editor.CanFocus) newState.Editor.Focus();
                }
            }
        }

        private bool OpenFile(TabEditorState tabState, string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string contents = reader.ReadToEnd();
                    tabState.FileEncoding = reader.CurrentEncoding;
                    tabState.Editor.Text = contents;
                }
            }
            catch (IOException e)
            {
                return (false);
            }
            tabState.Name = Path.GetFileName(filePath);
            tabState.FilePath = filePath;
            tabState.IsChanged = false;
            UpdateTabState(tabState);
            return (true);
        }

        private bool SaveFile(TabEditorState tabState)
        {
            tabState.IsChanged = false;
            UpdateTabState(tabState);
            return (true);
        }

        private bool SaveFileAs(TabEditorState tabState, string filePath)
        {
            tabState.FilePath = filePath;
            tabState.Name = Path.GetFileName(filePath);
            bool result = SaveFile(tabState);
            return (result);
        }

        private bool SaveWithConfirmation(TabEditorState tabState)
        {
            Debug.Assert(tabState.IsChanged);
            string caption = $"File '{tabState.Name}' was changed";
            string text = $"The file '{tabState.Name}' contains changes, do you want to save it first before continue?";
            DialogResult r = MessageBox.Show(this, text, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (r == DialogResult.Cancel)
                return (false);
            else if (r == DialogResult.No)
                return (true);
            else
            {
                if (string.IsNullOrEmpty(tabState.FilePath))
                {
                    if (dlgSaveFile.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = dlgSaveFile.FileName;
                        bool result = SaveFileAs(tabState, filePath);
                        return (result);
                    }
                    else return (false);
                }
                else
                {
                    bool result = SaveFile(tabState);
                    return (result);
                }
            }
        }

        private void MenuActionFileSaveAs(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            if (dlgSaveFile.ShowDialog() == DialogResult.OK)
            {
                string filePath = dlgSaveFile.FileName;
                TabEditorState tabState = (TabEditorState)tcFiles.SelectedTab.Tag;
                SaveFileAs(tabState, filePath);
            }
        }



        private void MenuActionFileSave(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            TabEditorState tabState = (TabEditorState)tcFiles.SelectedTab.Tag;
            SaveWithConfirmation(tabState);
        }

        private void MenuActionFileClose(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            TabPage tab = tcFiles.SelectedTab;
            tcFiles.TabPages.Remove(tab);
        }

        private bool CloseTabs(IEnumerable<TabEditorState> tabStates)
        {
            foreach (TabEditorState tabState in tabStates)
            {
                if (tabState.IsChanged)
                {
                    if (!SaveWithConfirmation(tabState))
                        return (false);
                }
                tcFiles.TabPages.Remove(tabState.Tab);
            }
            return (true);
        }

        private void MenuActionFileCloseAll(object sender, EventArgs e)
        {
            List<TabEditorState> tabsToClose = new List<TabEditorState>();
            foreach (TabPage tab in tcFiles.TabPages)
                tabsToClose.Add((TabEditorState)tab.Tag);
            CloseTabs(tabsToClose);
        }

        private void MenuActionFileCloseAllButThis(object sender, EventArgs e)
        {
            List<TabEditorState> tabsToClose = new List<TabEditorState>();
            foreach (TabPage tab in tcFiles.TabPages)
            {
                if (tab != tcFiles.SelectedTab)
                    tabsToClose.Add((TabEditorState)tab.Tag);
            }
            CloseTabs(tabsToClose);
        }

        private IEnumerable<TabEditorState> GetChanges()
        {
            List<TabEditorState> result = new List<TabEditorState>();
            foreach (TabPage tab in tcFiles.TabPages)
            {
                TabEditorState tabState = (TabEditorState)tab.Tag;
                if (tabState.IsChanged)
                    result.Add(tabState);
            }
            return (result);
        }

        private void UpdateTabState(TabEditorState tabState)
        {
            IEnumerable<TabEditorState> changes = GetChanges();
            bool anyChanges = changes.Count() > 0;

            miFileSave.Enabled = tabState != null && tabState.IsChanged;
            miFileSaveAll.Enabled = anyChanges;
            miFileClose.Enabled = tcFiles.SelectedTab != null;
            miFileCloseAll.Enabled = tcFiles.TabCount > 0;

            if (tabState != null)
            {
                string title = tabState.Name;
                if (tabState.IsChanged) title += "*";
                tabState.Tab.Text = title;
            }
        }

        private void tcFiles_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < tcFiles.TabCount; ++i)
                {
                    Rectangle r = tcFiles.GetTabRect(i);
                    if (r.Contains(e.Location))
                    {
                        cmsTabActions.Show(tcFiles, e.Location);
                        break;
                    }
                }
            }
        }

        private void tcFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcFiles.SelectedIndex == -1)
                UpdateTabState(null);
            else
            {
                TabPage tab = tcFiles.TabPages[tcFiles.SelectedIndex];
                TabEditorState tabState = (TabEditorState)tab.Tag;
                UpdateTabState(tabState);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            IEnumerable<TabEditorState> changes = GetChanges();
            if (changes.Count() > 0)
                e.Cancel = !CloseTabs(changes);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MenuActionFileNew(this, new EventArgs());
        }
    }
}
