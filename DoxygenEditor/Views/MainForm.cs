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
using TSP.DoxygenEditor.ErrorDialog;
using TSP.DoxygenEditor.TextAnalysis;
using TSP.DoxygenEditor.Languages.Doxygen;
using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.Symbols;
using System.Collections;
using TSP.DoxygenEditor.FilterControls;
using System.Threading;

namespace TSP.DoxygenEditor.Views
{
    public partial class MainForm : Form
    {
        private readonly GlobalConfigModel _globalConfig;
        private readonly WorkspaceModel _workspace;
        private readonly string _appName;
        private readonly string _dataPath;
        private readonly string _defaultWorkspaceFilePath;

        class PerformanceListViewItemComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                if (x == null || y == null)
                    return (-1);
                ListViewItem item1 = (ListViewItem)x;
                ListViewItem item2 = (ListViewItem)y;
                PerformanceItemModel a = (PerformanceItemModel)item1.Tag;
                PerformanceItemModel b = (PerformanceItemModel)item2.Tag;
                int result = a.Compare(b, a);
                return (result);
            }
        }

        private readonly FilterBarControl _doxygenIssuesFilterControl;
        private readonly FilterBarControl _cppIssuesFilterControl;
        private readonly FilterListView lvDoxygenIssues;
        private readonly FilterListView lvCppIssues;

        private void SetupIssueColumns(FilterListView listview)
        {
            listview.ImageList = imglstIcons;
            listview.AddColumn("Name", 150);
            listview.AddColumn("Symbol", 150);
            listview.AddColumn("Type", 100);
            listview.AddColumn("Category", 150);
            listview.AddColumn("Line", 100);
            listview.AddColumn("File", 200);
        }

        public MainForm()
        {
            InitializeComponent();

            var assembly = Assembly.GetExecutingAssembly();
            var asmName = assembly.GetName();
            FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string companyName = verInfo.CompanyName;
            string appId = verInfo.FileDescription;
            _appName = verInfo.ProductName;

            if (string.IsNullOrWhiteSpace(companyName))
                throw new Exception("Company name is missing in assembly!");
            if (string.IsNullOrWhiteSpace(appId))
                throw new Exception("Title is missing in assembly!");

            _dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), companyName, appId);
            if (!Directory.Exists(_dataPath)) Directory.CreateDirectory(_dataPath);
            _defaultWorkspaceFilePath = Path.Combine(_dataPath, "DefaultWorkspace.xml");

            _globalConfig = new GlobalConfigModel(companyName, appId);
            _globalConfig.Load();

            _workspace = new WorkspaceModel(_globalConfig.WorkspacePath);
            if (!string.IsNullOrWhiteSpace(_workspace.FilePath))
                _workspace.Load(_globalConfig.WorkspacePath);
            else
            {
                _workspace.FilePath = _defaultWorkspaceFilePath;
                _globalConfig.WorkspacePath = _defaultWorkspaceFilePath;
            }
            UpdatedWorkspaceFile();


            // @STUPID(final): Visual studio designer is so stupid, it cannot recognize usercontrols properly
            // so we need to manually add it ourself -.-
            lvDoxygenIssues = new FilterListView();
            lvDoxygenIssues.Dock = DockStyle.Fill;
            lvDoxygenIssues.ItemDoubleClick += Issues_ItemDoubleClick;
            SetupIssueColumns(lvDoxygenIssues);
            tpDoxygenIssues.Controls.Add(lvDoxygenIssues);
            _doxygenIssuesFilterControl = new FilterBarControl(lvDoxygenIssues);
            _doxygenIssuesFilterControl.ChangedFilter += (s, e) =>
            {
                lvDoxygenIssues.FilterText = e;
            };

            lvCppIssues = new FilterListView();
            lvCppIssues.Dock = DockStyle.Fill;
            lvCppIssues.ItemDoubleClick += Issues_ItemDoubleClick;
            SetupIssueColumns(lvCppIssues);
            tpCppIssues.Controls.Add(lvCppIssues);

            _cppIssuesFilterControl = new FilterBarControl(lvCppIssues);
            _cppIssuesFilterControl.ChangedFilter += (s, e) =>
            {
                lvCppIssues.FilterText = e;
            };

            lvPerformance.ListViewItemSorter = new PerformanceListViewItemComparer();

            // Update UI from config settings
            miViewShowWhitespaces.Checked = _workspace.IsWhitespaceVisible;
            RefreshRecentFiles();

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

        private void UpdatedWorkspaceFile()
        {
            if (_defaultWorkspaceFilePath.Equals(_workspace.FilePath))
                Text = $"{_appName} - Default Workspace";
            else
                Text = $"{_appName} - {Path.GetFileName(_workspace.FilePath)}";
        }

        private void SetParseStatus(string status)
        {
            tsParseStatusLabel.Text = status;
        }

        private void ShowError(string caption, string shortText, string details)
        {
            ErrorDialogForm dialog = new ErrorDialogForm();
            dialog.Title = caption;
            dialog.ShortText = shortText;
            dialog.Details = details;
            dialog.ShowDialog(this);
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
        private int _newTabCounter = 0;
        private int _parseTotalCount = 0;
        private int _parseProgressCount = 0;
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

            miFileRefresh.Enabled = tbtnFileRefresh.Enabled = editorState != null;
            miFileSave.Enabled = tbtnFileSave.Enabled = editorState != null && editorState.IsChanged;
            miFileSaveAll.Enabled = tbtnFileSaveAll.Enabled = anyChanges;
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
            int tabIndex = _newTabCounter++;
            TabPage newTab = new TabPage() { Text = name };
            EditorState newState = new EditorState(this, name, newTab, tabIndex);
            newState.IsShowWhitespace = miViewShowWhitespaces.Checked;
            newState.TabUpdating += (s, e) => UpdateTabState((EditorState)s);
            newState.FocusChanged += (s, e) =>
            {
                UpdateMenuEditChange(newState);
                UpdateMenuSelection(newState);
            };
            newState.ParseComplete += (EditorState editorState) =>
            {
                RebuildSymbolTree(editorState, editorState.DoxyTree);
                AddPerformanceItems(editorState);
                GlobalSymbolCache.AddOrReplaceTable(editorState.SymbolTable);
                bool isComplete = Interlocked.Decrement(ref _parseProgressCount) == 0;
                if (isComplete)
                {
                    Interlocked.Exchange(ref _parseTotalCount, 0);
                    IEnumerable<EditorState> states = GetAllEditorStates();
                    RefreshIssues(states);
                    SetParseStatus("");
                }
                else
                    SetParseStatus($"Parsing {_parseProgressCount} of {_parseTotalCount}");
            };
            newState.ParseStarting += (EditorState editorState) =>
            {
                Interlocked.Increment(ref _parseTotalCount);
                Interlocked.Increment(ref _parseProgressCount);
                ClearPerformanceItems(editorState);
                SetParseStatus($"Parsing {_parseProgressCount} of {_parseTotalCount}");
            };
            newTab.Tag = newState;
            newTab.Controls.Add(newState.ContainerPanel);
            tcFiles.TabPages.Add(newTab);
            AddToSymbolTree(newState, newState.Name);
            return (newState);
        }

        private void RemoveFileTab(EditorState editorState)
        {
            editorState.Stop();
            RemoveFromSymbolTree(editorState);
            ClearPerformanceItems(editorState);
            GlobalSymbolCache.Remove(editorState);

            TabPage tab = (TabPage)editorState.Tag;
            tcFiles.TabPages.Remove(tab);
            editorState.Dispose();

            IEnumerable<EditorState> states = GetAllEditorStates();
            RefreshIssues(states);
        }

        private void OpenFileTab(string filePath, bool pushRecentFile = false)
        {
            // Is the file already open?
            EditorState alreadyOpenState = null;
            foreach (TabPage tab in tcFiles.TabPages)
            {
                EditorState state = (EditorState)tab.Tag;
                if (string.Equals(state.FilePath, filePath))
                {
                    alreadyOpenState = state;
                    break;
                }
            }

            if (alreadyOpenState != null)
            {
                // Focus existing state
                tcFiles.SelectedTab = (TabPage)alreadyOpenState.Tag;
                alreadyOpenState.SetFocus();
            }
            else
            {
                EditorState newState = AddFileTab(Path.GetFileName(filePath));
                TabPage tab = (TabPage)newState.Tag;
                tcFiles.SelectedIndex = tcFiles.TabPages.IndexOf(tab);
                Tuple<bool, Exception> openRes = IOOpenFile(newState, filePath);
                if (!openRes.Item1)
                {
                    Exception e = openRes.Item2;
                    var values = new Dictionary<string, string>() { { "filepath", filePath } };
                    var msg = e.ToErrorMessage("Open file", values);
                    ShowError(msg.Caption, msg.ShortText, msg.Details);
                    RemoveFileTab(newState);
                }
                else
                {
                    _workspace.PushRecentFiles(filePath);

                    // Remove first tab when it was a "New" and is still unchanged
                    if (tcFiles.TabPages.Count == 2)
                    {
                        TabPage firstTab = tcFiles.TabPages[0];
                        EditorState existingState = (EditorState)firstTab.Tag;
                        if (existingState.FilePath == null && !existingState.IsChanged)
                            RemoveFileTab(existingState);
                    }

                    // Focus new tab/state
                    tcFiles.SelectedTab = tab;
                    newState.SetFocus();
                }
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


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _workspace.UpdateLastOpenedFiles(GetAllEditorStates().Select(f => f.FilePath));
            IEnumerable<EditorState> changes = GetChangedEditorStates();
            if (changes.Count() > 0)
                e.Cancel = !CloseTabs(changes);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _workspace.Save();
            _globalConfig.Save();
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
            {
                if (_globalConfig.RestoreLastOpenedFiles && _workspace.LastOpenedFileCount > 0)
                {
                    foreach (var lastOpenedFilePath in _workspace.LastOpenedFiles)
                        OpenFileTab(lastOpenedFilePath);
                }
            }
            if (tcFiles.TabPages.Count == 0)
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
                        editorState.GoToPosition(entityNode.Entity.StartRange.Index);
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

        private static HashSet<DoxygenEntityKind> AllowedDoxyEntities = new HashSet<DoxygenEntityKind>()
        {
            DoxygenEntityKind.BlockSingle,
            DoxygenEntityKind.BlockMulti,
            DoxygenEntityKind.Group,
            DoxygenEntityKind.Page,
            DoxygenEntityKind.Section,
            DoxygenEntityKind.SubSection,
            DoxygenEntityKind.SubSubSection,
        };

        private List<TreeNode> BuildSymbolTree(IBaseNode rootEntityNode, TreeNode rootTreeNode, IBaseNode selectedEntityNode)
        {
            List<TreeNode> result = new List<TreeNode>();
            foreach (IBaseNode childEntityNode in rootEntityNode.Children)
            {
                if (typeof(DoxygenNode).Equals(childEntityNode.GetType()))
                {
                    DoxygenNode doxyNode = (DoxygenNode)childEntityNode;
                    DoxygenEntity entity = doxyNode.Entity;
                    if (!AllowedDoxyEntities.Contains(entity.Kind))
                        continue;

                    TreeNode parentNode;
                    if (childEntityNode.ShowChildren)
                    {
                        parentNode = new TreeNode(entity.DisplayName);
                        parentNode.Tag = childEntityNode;
                        rootTreeNode.Nodes.Add(parentNode);
                        if (selectedEntityNode != null)
                        {
                            if (selectedEntityNode.CompareTo(childEntityNode) == 0)
                                result.Add(parentNode);
                        }
                    }
                    else
                        parentNode = rootTreeNode;
                    result.AddRange(BuildSymbolTree(childEntityNode, parentNode, selectedEntityNode));
                }
            }
            return (result);
        }

        private void RebuildSymbolTree(object fileTag, IBaseNode doxyTree)
        {
            DoxygenNode lastEntity = null;
            if (tvTree.SelectedNode != null)
                lastEntity = tvTree.SelectedNode.Tag as DoxygenNode;

            TreeNode newSelectedNode = null;

            // Find file node from tag
            TreeNode fileNode = FindRootSymbolNode(fileTag);
            Debug.Assert(fileNode != null);

            tvTree.BeginUpdate();
            fileNode.Nodes.Clear();

            if (doxyTree != null)
            {
                List<TreeNode> selNodes = BuildSymbolTree(doxyTree, fileNode, lastEntity);
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
                    {
                        string filePath = editorState.FilePath;
                        Exception e = result.Item2;
                        var values = new Dictionary<string, string>() { { "filepath", filePath } };
                        var msg = e.ToErrorMessage("Save file", values);
                        ShowError(msg.Caption, msg.ShortText, msg.Details);
                    }
                    return (result);
                }
            }
        }

        private void MenuActionFileRefresh(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            if (editorState.IsChanged)
            {
                var caption = $"Revert file '{editorState.Name}'";
                var text = $"The file '{editorState.Name}' has changes, do you want to reload and revert it?";
                var dlgResult = MessageBox.Show(this, text, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dlgResult != DialogResult.Yes)
                    return;
            }
            IOOpenFile(editorState, editorState.FilePath);
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
            {
                foreach (string filePath in dlgOpenFile.FileNames)
                    OpenFileTab(filePath);
            }
        }
        private void MenuActionFileSave(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            Tuple<bool, Exception> r = SaveWithConfirmation(editorState, true);
            if (r.Item1)
                _workspace.PushRecentFiles(editorState.FilePath);
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
                {
                    Exception ex = r.Item2;
                    var values = new Dictionary<string, string>() { { "filepath", filePath } };
                    var msg = ex.ToErrorMessage("Save file", values);
                    ShowError(msg.Caption, msg.ShortText, msg.Details);
                }
                else
                    _workspace.PushRecentFiles(filePath);

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
        private void MenuActionFileOpenRecentFile(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            string filePath = (string)item.Tag;
            OpenFileTab(filePath);
        }

        private void MenuActionFileExit(object sender, EventArgs e)
        {
            Close();
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
                // @SPEED(final): Cache conversion to SymbolItemModel and types as well
                List<SymbolItemModel> symbols = new List<SymbolItemModel>();
                HashSet<string> types = new HashSet<string>();
                var allSources = GlobalSymbolCache.GetSources(editorState);
                foreach (var source in allSources)
                {
                    if (source.Node == null) continue;
                    symbols.Add(new SymbolItemModel()
                    {
                        Caption = null,
                        Id = source.Name,
                        Type = source.Kind.ToString(),
                        Position = source.Range.Position,
                    });
                    types.Add(source.Kind.ToString());
                }
                SymbolSearchForm form = new SymbolSearchForm(symbols, types);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    SymbolItemModel selectedItem = form.SelectedItem;
                    editorState.GoToPosition(selectedItem.Position.Index);
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
            _workspace.IsWhitespaceVisible = enabled;
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
            miEditUndo.Enabled = tbtnEditUndo.Enabled = editorState != null && editorState.CanUndo();
            miEditRedo.Enabled = tbtnEditRedo.Enabled = editorState != null && editorState.CanRedo();
            miEditPaste.Enabled = editorState != null && editorState.CanPaste();
        }
        private void RefreshRecentFiles()
        {
            miFileRecentFiles.DropDownItems.Clear();
            foreach (string recentFile in _workspace.RecentFiles)
            {
                ToolStripMenuItem newItem = new ToolStripMenuItem(recentFile);
                newItem.Tag = recentFile;
                newItem.Click += MenuActionFileOpenRecentFile;
                miFileRecentFiles.DropDownItems.Add(newItem);
            }
        }
        #endregion

        #region Issues
        enum IssueType
        {
            Error,
            Warning,
            Info,
        }
        class IssueTag
        {
            public EditorState State { get; }
            public TextPosition Pos { get; }
            public IssueTag(EditorState state, TextPosition pos)
            {
                State = state;
                Pos = pos;
            }
        }
        private void AddIssue(FilterListView listView, IssueTag tag, IssueType type, string message, string symbolName, string symbolType, string group, int line, string file)
        {
            ListViewItem newItem = new ListViewItem(message);
            newItem.Tag = tag;
            newItem.ImageIndex = (int)type;
            newItem.SubItems.Add(symbolName);
            newItem.SubItems.Add(symbolType);
            newItem.SubItems.Add(group);
            newItem.SubItems.Add(line.ToString());
            newItem.SubItems.Add(file);
            listView.AddItem(newItem);
        }
        private readonly Regex _rexRefWithIdent = new Regex("^(@ref\\s+[a-zA-Z_][a-zA-Z0-9_]+)$", RegexOptions.Compiled);
        private void AddIssuesFromNode(IEnumerable<EditorState> states, EditorState state, IBaseNode rootNode, string fileName, string groupName)
        {
            if (typeof(CppNode).Equals(rootNode.GetType()))
            {
                CppNode cppNode = (CppNode)rootNode;
                CppEntity cppEntity = cppNode.Entity;
                if (cppEntity.DocumentationNode != null)
                {
                    DoxygenNode doxyNode = (DoxygenNode)cppEntity.DocumentationNode;
                    if (doxyNode.Entity.Kind == DoxygenEntityKind.BlockMulti)
                    {
                        DoxygenNode seeNode = doxyNode.TypedChildren.FirstOrDefault(c => c.Entity.Kind == DoxygenEntityKind.See) as DoxygenNode;
                        bool hasDocumented = false;
                        if (seeNode != null)
                        {
                            DoxygenNode refNode = seeNode.TypedChildren.FirstOrDefault(c => c.Entity.Kind == DoxygenEntityKind.Reference) as DoxygenNode;
                            if (refNode != null)
                            {
                                hasDocumented = true;
                            }
                        }

                        if (!hasDocumented)
                        {
                            AddIssue(lvCppIssues, new IssueTag(state, cppEntity.StartRange.Position), IssueType.Warning, "Not documented (Add a @see @ref [section or page id])", cppEntity.Id, cppEntity.Kind.ToString(), "C/C++", cppEntity.StartRange.Position.Line + 1, fileName);
                        }
                    }
                }
            }
            foreach (var child in rootNode.Children)
            {
                AddIssuesFromNode(states, state, child, fileName, "Child");
            }
        }
        private void RefreshIssues(IEnumerable<EditorState> states)
        {
            int selectedCppIssueIndex = lvCppIssues.SelectedIndex;
            int selectedDoxyIssueIndex = lvCppIssues.SelectedIndex;
            IssueTag selectedCppIssue = lvCppIssues.SelectedItem?.Tag as IssueTag;
            IssueTag selectedDoxyIssue = lvDoxygenIssues.SelectedItem?.Tag as IssueTag;

            lvCppIssues.BeginUpdate();
            lvDoxygenIssues.BeginUpdate();

            lvCppIssues.ClearSelection();
            lvDoxygenIssues.ClearSelection();

            lvCppIssues.ClearItems();
            lvDoxygenIssues.ClearItems();

            // Validate symbols from cache
            var symbolErrors = GlobalSymbolCache.Validate();
            foreach (var errorPair in symbolErrors)
            {
                var error = errorPair.Value;
                var state = (EditorState)errorPair.Key;
                var symbol = (ReferenceSymbol)error.Tag;
                var nodeType = symbol.Node.GetType();
                if (typeof(CppNode).Equals(nodeType))
                    AddIssue(lvCppIssues, new IssueTag(state, error.Pos), IssueType.Error, error.Message, error.Symbol, error.Type, error.Category, error.Pos.Line + 1, state.Name);
                else if (typeof(DoxygenNode).Equals(nodeType))
                    AddIssue(lvDoxygenIssues, new IssueTag(state, error.Pos), IssueType.Error, error.Message, error.Symbol, error.Type, error.Category, error.Pos.Line + 1, state.Name);
            }

            foreach (EditorState state in states)
            {
                foreach (var error in state.Errors)
                {
                    var errorType = error.Tag.GetType();
                    if (typeof(CppLexer).Equals(errorType) || typeof(CppParser).Equals(errorType))
                        AddIssue(lvCppIssues, new IssueTag(state, error.Pos), IssueType.Error, error.Message, null, null, error.Category, error.Pos.Line + 1, state.Name);
                    else if (typeof(DoxygenLexer).Equals(errorType) || typeof(DoxygenParser).Equals(errorType))
                        AddIssue(lvDoxygenIssues, new IssueTag(state, error.Pos), IssueType.Error, error.Message, null, null, error.Category, error.Pos.Line + 1, state.Name);
                }

                if (state.CppTree != null)
                {
                    AddIssuesFromNode(states, state, state.CppTree, state.Name, "Root");
                }
            }

            lvCppIssues.RefreshItems();
            lvDoxygenIssues.RefreshItems();

            lvCppIssues.SelectItemOrIndex(selectedCppIssue, selectedCppIssueIndex);
            lvDoxygenIssues.SelectItemOrIndex(selectedDoxyIssue, selectedDoxyIssueIndex);

            lvCppIssues.EndUpdate();
            lvDoxygenIssues.EndUpdate();

            tpCppIssues.Text = $"C/C++ Issues [{lvCppIssues.ItemCount}]";
            tpDoxygenIssues.Text = $"Doxygen Issues [{lvDoxygenIssues.ItemCount}]";
        }

        private void Issues_ItemDoubleClick(object sender, ListViewItem item)
        {
            if (item != null && item.Tag != null)
            {
                IssueTag tag = (IssueTag)item.Tag;
                TextPosition pos = tag.Pos;
                EditorState state = tag.State;
                TabPage tab = (TabPage)state.Tag;
                tcFiles.SelectedTab = tab;
                state.GoToPosition(pos.Index);
            }
        }
        #endregion

        #region Performance
        private void AddPerformanceItem(EditorState state, PerformanceItemModel item)
        {
            ListViewItem newItem = new ListViewItem(state.Name);
            newItem.Tag = item;
            newItem.SubItems.Add(item.Input);
            newItem.SubItems.Add(item.Output);
            newItem.SubItems.Add(item.What);
            newItem.SubItems.Add(item.Duration.ToMilliseconds());
            lvPerformance.Items.Add(newItem);
        }

        private void AddPerformanceItems(EditorState state)
        {
            ClearPerformanceItems(state);
            lvPerformance.BeginUpdate();
            foreach (var item in state.PerformanceItems)
            {
                AddPerformanceItem(state, item);
            }
            lvPerformance.Sort();
            lvPerformance.EndUpdate();
        }

        private void ClearPerformanceItems(object tag)
        {
            List<ListViewItem> itemsToRemove = new List<ListViewItem>();
            lvPerformance.BeginUpdate();
            foreach (ListViewItem item in lvPerformance.Items)
            {
                PerformanceItemModel performanceItem = (PerformanceItemModel)item.Tag;
                if (performanceItem.Tag == tag)
                    itemsToRemove.Add(item);
            }
            foreach (ListViewItem item in itemsToRemove)
            {
                lvPerformance.Items.Remove(item);
            }
            lvPerformance.EndUpdate();
        }
        #endregion

        private void miWorkspaceConfiguration_Click(object sender, EventArgs e)
        {
            WorkspaceForm dlg = new WorkspaceForm(_workspace);
            DialogResult r = dlg.ShowDialog(this);
            if (r == DialogResult.OK)
                _workspace.Overwrite(dlg.Workspace);
        }
    }
}