using TSP.DoxygenEditor.Editor;
using TSP.DoxygenEditor.Extensions;
using TSP.DoxygenEditor.Models;
using TSP.DoxygenEditor.Natives;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.SearchReplace;
using TSP.DoxygenEditor.Services;
using TSP.DoxygenEditor.Solid;
using TSP.DoxygenEditor.SymbolSearch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Parsers.Doxygen;
using TSP.DoxygenEditor.Lexers.Doxygen;

namespace TSP.DoxygenEditor.Views
{
    public partial class MainForm : Form
    {
        private readonly IConfigurationService _configService;
        private readonly ConfigurationModel _config;
        private readonly string _appName;

        private static ErrorMessageModel _errorFileOpenMessage = new ErrorMessageModel($"Cannot open file '%FILEPATH%'!", $"File to open file '%FILENAME%'");
        private static ErrorMessageModel _errorFileSaveMessage = new ErrorMessageModel($"Cannot save file '%FILEPATH%'!", $"File to save file '%FILENAME%'");

        public MainForm()
        {
            InitializeComponent();

            FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            _appName = verInfo.ProductName;

            _configService = IOCContainer.Get<IConfigurationService>();
            _config = new ConfigurationModel();
            _config.Load(_configService);

            _searchControl = new SearchReplace.SearchReplaceControl();
            Controls.Add(_searchControl);

            TextSelectedTimer = new System.Windows.Forms.Timer() { Enabled = true, Interval = 500 };
            TextSelectedTimer.Tick += (s, e) =>
            {
                if (tcFiles.TabPages.Count > 0)
                {
                    Debug.Assert(tcFiles.SelectedTab != null);
                    EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
                    UpdateMenuSelection(editorState);
                }
                else UpdateMenuSelection(null);
            };
            NativeMethods.AddClipboardFormatListener(Handle);
        }

        private void ShowError(Exception exception, string text, string caption, string filePath)
        {
            string filename = !string.IsNullOrWhiteSpace(filePath) ? Path.GetFileName(filePath) : null;
            StringBuilder message = new StringBuilder();
            message.AppendLine($"[{_appName}] {text}!");
            if (exception != null)
            {
                message.AppendLine();
                message.Append(exception.ToHumanReadable(filePath));
            }
            MessageBox.Show(this, message.ToString(), caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void ShowError(Exception exception, ErrorMessageModel messageTemplate, string filePath)
        {
            Tuple<string, string> r = messageTemplate.ToFileError(filePath);
            ShowError(exception, r.Item1, r.Item2, filePath);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE)
            {
                EditorState editorState = null;
                if (tcFiles.TabPages.Count > 0)
                {
                    Debug.Assert(tcFiles.SelectedTab != null);
                    editorState = (EditorState)tcFiles.SelectedTab.Tag;
                }
                UpdateMenuEditChange(editorState);
            }
            base.WndProc(ref m);
        }


        #region Editor
        private System.Windows.Forms.Timer TextSelectedTimer { get; }
        private SearchReplaceControl _searchControl;



        #endregion

        #region Tabs
        private readonly Regex _rexIndexFromName = new Regex("(?<index>[0-9]+)$", RegexOptions.Compiled);
        private string GetNextTabName(string prefix)
        {
            int highIndex = 0;
            foreach (TabPage tab in tcFiles.TabPages)
            {
                EditorState tabState = (EditorState)tab.Tag;
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

        private IEnumerable<EditorState> GetChangedEditorStates()
        {
            List<EditorState> result = new List<EditorState>();
            foreach (TabPage tab in tcFiles.TabPages)
            {
                EditorState tabState = (EditorState)tab.Tag;
                if (tabState.IsChanged)
                    result.Add(tabState);
            }
            return (result);
        }

        private IEnumerable<EditorState> GetAllEditorStates()
        {
            List<EditorState> result = new List<EditorState>();
            foreach (TabPage tab in tcFiles.TabPages)
            {
                EditorState tabState = (EditorState)tab.Tag;
                result.Add(tabState);
            }
            return (result);
        }

        private void UpdateTabState(EditorState editorState)
        {
            IEnumerable<EditorState> changedEditorStates = GetChangedEditorStates();
            bool anyChanges = changedEditorStates.Count() > 0;

            miFileSave.Enabled = editorState != null && editorState.IsChanged;
            miFileSaveAll.Enabled = anyChanges;
            miFileClose.Enabled = tcFiles.SelectedTab != null;
            miFileCloseAll.Enabled = tcFiles.TabCount > 0;

            if (editorState != null)
            {
                string title = editorState.Name;
                if (editorState.IsChanged) title += "*";
                TabPage tab = (TabPage)editorState.Tag;
                tab.Text = title;
            }

            UpdateMenuEditChange(editorState);
            UpdateMenuSelection(editorState);
        }

        private EditorState AddFileTab(string name)
        {
            TabPage newTab = new TabPage() { Text = name };
            EditorState newState = new EditorState(this) { Name = name, Tag = newTab };
            newState.IsShowWhitespace = miViewShowWhitespaces.Checked;
            newState.TabUpdating += (s, e) => UpdateTabState((EditorState)s);
            newState.ParseComplete += (object s, bool allDone) =>
            {
                RebuildSymbolTree(newState, newState.DoxyTree);
                if (allDone)
                {
                    IEnumerable<EditorState> states = GetAllEditorStates();
                    RefreshIssues(states);
                }
            };
            newTab.Tag = newState;
            newTab.Controls.Add(newState.Container);
            tcFiles.TabPages.Add(newTab);
            AddToSymbolTree(newState, newState.Name);
            return (newState);
        }

        private void RemoveFileTab(EditorState editorState)
        {
            editorState.Stop();
            TabPage tab = (TabPage)editorState.Tag;
            tcFiles.TabPages.Remove(tab);
            RemoveFromSymbolTree(editorState);
        }

        private void OpenFileTab(string filePath)
        {
            EditorState newState = AddFileTab(Path.GetFileName(filePath));
            TabPage tab = (TabPage)newState.Tag;
            tcFiles.SelectedIndex = tcFiles.TabPages.IndexOf(tab);
            Tuple<bool, Exception> openRes = IOOpenFile(newState, filePath);
            if (!openRes.Item1)
            {
                ShowError(openRes.Item2, _errorFileOpenMessage, filePath);
                RemoveFileTab(newState);
            }
            else
            {
                // Remove first tab when it was a "New" and is still unchanged
                if (tcFiles.TabPages.Count == 2)
                {
                    TabPage firstTab = tcFiles.TabPages[0];
                    EditorState existingState = (EditorState)firstTab.Tag;
                    if (existingState.FilePath == null && !existingState.IsChanged)
                        tcFiles.TabPages.Remove(firstTab);
                }
                newState.SetFocus();
            }
        }

        private bool CloseTabs(IEnumerable<EditorState> editorStates)
        {
            foreach (EditorState editorState in editorStates)
            {
                if (editorState.IsChanged)
                {
                    Tuple<bool, Exception> saveRes = SaveWithConfirmation(editorState, false);
                    if (!saveRes.Item1)
                        return (false);
                }
                RemoveFileTab(editorState);
            }
            return (true);
        }
        #endregion

        #region IO
        private Tuple<bool, Exception> IOOpenFile(EditorState editorState, string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string contents = reader.ReadToEnd();
                    editorState.FileEncoding = reader.CurrentEncoding;
                    editorState.SetText(contents);
                }
            }
            catch (IOException e)
            {
                return new Tuple<bool, Exception>(false, e);
            }
            editorState.Name = Path.GetFileName(filePath);
            editorState.FilePath = filePath;
            editorState.IsChanged = false;
            UpdateTabState(editorState);
            return new Tuple<bool, Exception>(true, null);
        }

        private Tuple<bool, Exception> IOSaveFile(EditorState editorState)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(editorState.FilePath, false, editorState.FileEncoding))
                {
                    writer.Write(editorState.GetText());
                    writer.Flush();
                }
                editorState.IsChanged = false;
                UpdateTabState(editorState);
                return new Tuple<bool, Exception>(true, null);
            }
            catch (IOException e)
            {
                return new Tuple<bool, Exception>(false, e);
            }
        }
        #endregion

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
                TabPage selectedTab = tcFiles.TabPages[tcFiles.SelectedIndex];
                EditorState editorState = (EditorState)selectedTab.Tag;
                UpdateTabState(editorState);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            IEnumerable<EditorState> changes = GetChangedEditorStates();
            if (changes.Count() > 0)
                e.Cancel = !CloseTabs(changes);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                for (int i = 1; i < args.Length; ++i)
                    OpenFileTab(args[i]);
            }
            else
                MenuActionFileNew(this, new EventArgs());
        }

        #region Symbols
        private void tvTree_DoubleClick(object sender, EventArgs e)
        {
            if (tvTree.SelectedNode != null && tcFiles.SelectedIndex > -1)
            {
                TreeNode selectedNode = tvTree.SelectedNode;
                if (selectedNode.Level > 0)
                {
                    TreeNode parentNode = selectedNode.Parent;
                    while (parentNode != null)
                    {
                        if (parentNode.Parent != null)
                            parentNode = parentNode.Parent;
                        else
                            break;
                    }

                    if (parentNode != null)
                    {
                        EditorState editorState = (EditorState)parentNode.Tag;
                        TabPage tab = (TabPage)editorState.Tag;
                        tcFiles.SelectedTab = tab;
                        tcFiles.Focus();
                        DoxygenNode entityNode = (DoxygenNode)selectedNode.Tag;
                        editorState.GoToPosition(entityNode.Entity.Range.Index);
                    }
                }
            }
        }

        private TreeNode FindRootSymbolNode(object tag)
        {
            TreeNode result = null;
            foreach (TreeNode node in tvTree.Nodes)
            {
                if (node.Tag.Equals(tag))
                {
                    result = node;
                    break;
                }
            }
            return (result);
        }

        private void RemoveFromSymbolTree(object tag)
        {
            TreeNode foundNode = FindRootSymbolNode(tag);
            Debug.Assert(foundNode != null);

            tvTree.BeginUpdate();
            foundNode.Remove();
            tvTree.EndUpdate();
        }

        private void RenamedInSymbolTree(object tag, string name)
        {
            TreeNode foundNode = FindRootSymbolNode(tag);
            Debug.Assert(foundNode != null);

            tvTree.BeginUpdate();
            foundNode.Text = name;
            tvTree.EndUpdate();
        }

        private void AddToSymbolTree(object tag, string name)
        {
            TreeNode foundNode = FindRootSymbolNode(tag);
            Debug.Assert(foundNode == null);

            tvTree.BeginUpdate();
            TreeNode newNode = new TreeNode() { Text = name };
            newNode.Tag = tag;
            tvTree.Nodes.Add(newNode);
            tvTree.EndUpdate();
        }

        private static HashSet<DoxygenEntityType> AllowedDoxyEntities = new HashSet<DoxygenEntityType>()
        {
            DoxygenEntityType.Page,
            DoxygenEntityType.Section,
            DoxygenEntityType.SubSection,
            DoxygenEntityType.SubSubSection,
        };

        private List<TreeNode> BuildSymbolTree(BaseNode rootEntityNode, TreeNode rootTreeNode, BaseNode selectedEntityNode)
        {
            List<TreeNode> result = new List<TreeNode>();
            foreach (BaseNode childEntityNode in rootEntityNode.Children)
            {
                if (typeof(DoxygenNode).Equals(childEntityNode.GetType()))
                {
                    DoxygenEntity entity = (DoxygenEntity)childEntityNode.Entity;
                    if (!AllowedDoxyEntities.Contains(entity.Type))
                        continue;
                    TreeNode node = new TreeNode(entity.DisplayName);
                    node.Tag = childEntityNode;
                    rootTreeNode.Nodes.Add(node);
                    if (selectedEntityNode != null)
                    {
                        if (selectedEntityNode.Entity.CompareTo(childEntityNode.Entity) == 0)
                            result.Add(node);
                    }
                    if (childEntityNode.ShowChildren)
                        result.AddRange(BuildSymbolTree(childEntityNode, node, selectedEntityNode));
                }
            }
            return (result);
        }

        private void RebuildSymbolTree(object tag, BaseTree doxyTree)
        {
            DoxygenNode lastEntity = null;
            if (tvTree.SelectedNode != null)
                lastEntity = tvTree.SelectedNode.Tag as DoxygenNode;

            TreeNode newSelectedNode = null;

            // Find root node by tag
            TreeNode rootNode = FindRootSymbolNode(tag);
            Debug.Assert(rootNode != null);

            tvTree.BeginUpdate();
            rootNode.Nodes.Clear();

            if (doxyTree != null)
            {
                List<TreeNode> selNodes = BuildSymbolTree(doxyTree, rootNode, lastEntity);
                if (selNodes.Count == 1)
                    newSelectedNode = selNodes.First();
            }
            tvTree.EndUpdate();

            tvTree.SelectedNode = newSelectedNode;
            if (newSelectedNode != null)
                newSelectedNode.Expand();
        }
        #endregion

        #region Menu
        private Tuple<bool, Exception> SaveFileAs(EditorState editorState, string filePath)
        {
            editorState.FilePath = filePath;
            editorState.Name = Path.GetFileName(filePath);
            RenamedInSymbolTree(editorState, editorState.Name);
            Tuple<bool, Exception> result = IOSaveFile(editorState);
            return (result);
        }

        private Tuple<bool, Exception> SaveWithConfirmation(EditorState editorState, bool skipConfirmation)
        {
            Debug.Assert(editorState.IsChanged);
            string caption = $"File '{editorState.Name}' was changed";
            string text = $"The file '{editorState.Name}' contains changes, do you want to save it first before continue?";
            DialogResult r = skipConfirmation ? DialogResult.OK : MessageBox.Show(this, text, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (r == DialogResult.Cancel)
                return new Tuple<bool, Exception>(false, null);
            else if (r == DialogResult.No)
                return new Tuple<bool, Exception>(true, null);
            else
            {
                if (string.IsNullOrEmpty(editorState.FilePath))
                {
                    if (dlgSaveFile.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = dlgSaveFile.FileName;
                        Tuple<bool, Exception> result = SaveFileAs(editorState, filePath);
                        return (result);
                    }
                    else return new Tuple<bool, Exception>(false, null);
                }
                else
                {
                    Tuple<bool, Exception> result = IOSaveFile(editorState);
                    if (!result.Item1)
                        ShowError(result.Item2, _errorFileSaveMessage, editorState.FilePath);
                    return (result);
                }
            }
        }

        private void MenuActionFileNew(object sender, EventArgs e)
        {
            string name = GetNextTabName("File");
            EditorState newState = AddFileTab(name);
            TabPage tab = (TabPage)newState.Tag;
            tcFiles.SelectedIndex = tcFiles.TabPages.IndexOf(tab);
            newState.SetFocus();
            UpdateTabState(newState);
        }
        private void MenuActionFileOpen(object sender, EventArgs e)
        {
            if (dlgOpenFile.ShowDialog() == DialogResult.OK)

                foreach (string filePath in dlgOpenFile.FileNames)
                    OpenFileTab(filePath);
        }
        private void MenuActionFileSave(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            SaveWithConfirmation(editorState, true);
        }
        private void MenuActionFileSaveAs(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            if (dlgSaveFile.ShowDialog() == DialogResult.OK)
            {
                string filePath = dlgSaveFile.FileName;
                EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
                Tuple<bool, Exception> r = SaveFileAs(editorState, filePath);
                if (!r.Item1)
                    ShowError(r.Item2, _errorFileSaveMessage, filePath);
            }
        }
        private void MenuActionFileSaveAll(object sender, EventArgs e)
        {
            foreach (TabPage tab in tcFiles.TabPages)
            {
                EditorState editorState = (EditorState)tab.Tag;
                if (editorState.IsChanged)
                {
                    Tuple<bool, Exception> saveRes = SaveWithConfirmation(editorState, true);
                    if (!saveRes.Item1)
                        return;
                }
            }
        }
        private void MenuActionFileClose(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            List<EditorState> tabsToClose = new List<EditorState>();
            tabsToClose.Add(editorState);
            CloseTabs(tabsToClose);
        }
        private void MenuActionFileCloseAll(object sender, EventArgs e)
        {
            List<EditorState> tabsToClose = new List<EditorState>();
            foreach (TabPage tab in tcFiles.TabPages)
                tabsToClose.Add((EditorState)tab.Tag);
            CloseTabs(tabsToClose);
        }
        private void MenuActionFileCloseAllButThis(object sender, EventArgs e)
        {
            List<EditorState> tabsToClose = new List<EditorState>();
            foreach (TabPage tab in tcFiles.TabPages)
            {
                if (tab != tcFiles.SelectedTab)
                    tabsToClose.Add((EditorState)tab.Tag);
            }
            CloseTabs(tabsToClose);
        }

        private void MenuActionEditSearchAndReplaceQuickSearch(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            editorState.ShowSearch();
        }
        private void MenuActionEditSearchAndReplaceQuickReplace(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            editorState.ShowReplace();
        }
        private void MenuActionEditUndo(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            editorState.Undo();
        }
        private void MenuActionEditRedo(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            editorState.Redo();
        }
        private void MenuActionEditCut(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            editorState.Cut();
        }
        private void MenuActionEditCopy(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            editorState.Copy();
        }
        private void MenuActionEditPaste(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            editorState.Paste();
        }
        private void MenuActionEditSelectAll(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            editorState.SelectAll();
        }

        private void MenuActionEditGoToSymbol(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            if (editorState.DoxyTree != null)
            {
                List<SymbolItemModel> symbols = new List<SymbolItemModel>();
                HashSet<Type> types = new HashSet<Type>();

#if false
                IEnumerable<Entity> allEntities = editorState.DoxyTree.GetAllEntities();
                foreach (Entity entity in allEntities)
                {
                    Type t = entity.GetType();
                    if (typeof(PageEntity).Equals(t) ||
                        typeof(SectionEntity).Equals(t) ||
                        typeof(SubSectionEntity).Equals(t) ||
                        typeof(DeclarationEntity).Equals(t))
                    {
                        symbols.Add(new SymbolItemModel()
                        {
                            Caption = entity.DisplayName,
                            Id = entity.Id,
                            TypeString = entity.GetType().Name,
                            Position = entity.LineInfo.Start,
                        });
                        types.Add(entity.GetType());
                    }
                }
#endif

                SymbolSearchForm form = new SymbolSearchForm(symbols, types);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    SymbolItemModel selectedItem = form.SelectedItem;
                    editorState.GoToPosition(selectedItem.Position);
                }
            }
        }

        private void MenuActionViewShowWhitespaces(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            bool enabled = !item.Checked;
            foreach (TabPage tab in tcFiles.TabPages)
            {
                EditorState editorState = (EditorState)tab.Tag;
                editorState.IsShowWhitespace = enabled;
            }
            item.Checked = enabled;
        }

        private void MenuActionHelpAbout(object sender, EventArgs e)
        {
            AboutForm form = new AboutForm();
            form.ShowDialog(this);
        }

        private void UpdateMenuSelection(EditorState editorState)
        {
            miEditCut.Enabled = editorState != null && editorState.CanCut();
            miEditCopy.Enabled = editorState != null && editorState.CanCopy();
        }

        private void UpdateMenuEditChange(EditorState editorState)
        {
            miEditUndo.Enabled = editorState != null && editorState.CanUndo();
            miEditRedo.Enabled = editorState != null && editorState.CanRedo();
            miEditPaste.Enabled = editorState != null && editorState.CanPaste();
        }
        #endregion

        #region Issues
        enum IssueType
        {
            Error,
            Warning,
            Info,
        }
        struct IssueTag
        {
            public EditorState State { get; }
            public BaseEntity Entity { get; }
            public IssueTag(EditorState state, BaseEntity entity)
            {
                State = state;
                Entity = entity;
            }
        }
        private void AddIssue(IssueTag tag, IssueType type, string message, string symbolName, string symbolType, string group, string file)
        {
            ListViewItem newItem = new ListViewItem(message);
            newItem.Tag = tag;
            newItem.ImageIndex = (int)type;
            newItem.SubItems.Add(symbolName);
            newItem.SubItems.Add(symbolType);
            newItem.SubItems.Add(group);
            newItem.SubItems.Add(file);
            lvIssues.Items.Add(newItem);
        }
        private readonly Regex _rexRefWithIdent = new Regex("^(@ref\\s+[a-zA-Z_][a-zA-Z0-9_]+)$", RegexOptions.Compiled);
        private void AddIssuesFromEntity(IEnumerable<EditorState> states, EditorState state, BaseNode entityNode, string fileName, string groupName)
        {
#if false
            if (typeof(CommentEntity).Equals(entity.GetType()))
            {
                CommentEntity comment = (CommentEntity)entity;
                DeclarationEntity decl = entity.FindChildByType<DeclarationEntity>();
                if (decl != null)
                {
                    ParamEntity seeParam = comment.FindChildByExpression<ParamEntity>(f => "see".Equals(f.ParamName) && _rexRefWithIdent.IsMatch(f.ParamValue));
                    if (seeParam == null)
                        AddIssue(new IssueTag(state, decl), IssueType.Warning, "Missing documentation", decl.DisplayName, decl.DeclarationType.ToString(), groupName, fileName);
                }
            }
#endif
        }
        private void RefreshIssues(IEnumerable<EditorState> states)
        {
            lvIssues.BeginUpdate();
            lvIssues.Items.Clear();
            foreach (EditorState state in states)
            {
                if (state.DoxyTree != null)
                {
                    foreach (BaseNode childNode in state.DoxyTree.Children)
                    {
                        AddIssuesFromEntity(states, state, childNode, state.Name, "Root");
                    }
                }
            }
            lvIssues.EndUpdate();
            tpIssues.Text = $"Issues [{lvIssues.Items.Count}]";
        }

        private void lvIssues_DoubleClick(object sender, EventArgs e)
        {
            if (lvIssues.SelectedItems.Count > 0)
            {
                ListViewItem item = lvIssues.SelectedItems[0];
                if (item.Tag != null)
                {
                    IssueTag tag = (IssueTag)item.Tag;
                    BaseEntity entity = tag.Entity;
                    EditorState state = tag.State;
                    TabPage tab = (TabPage)state.Tag;
                    tcFiles.SelectedTab = tab;
                    state.GoToPosition(entity.Range.Index);
                }
            }
        }
        #endregion
    }
}