using TSP.DoxygenEditor.Editor;
using TSP.DoxygenEditor.Extensions;
using TSP.DoxygenEditor.Models;
using TSP.DoxygenEditor.Natives;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.SearchReplace;
using TSP.DoxygenEditor.SymbolSearch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
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
            _defaultWorkspaceFilePath = Path.Combine(_dataPath, "DefaultWorkspace.doxyedit");

            _globalConfig = new GlobalConfigModel(companyName, appId);
            _globalConfig.Load();

            _workspace = new WorkspaceModel(_defaultWorkspaceFilePath);
            if (!string.IsNullOrWhiteSpace(_globalConfig.WorkspacePath))
            {
                WorkspaceModel loadedWorkspace = WorkspaceModel.Load(_globalConfig.WorkspacePath);
                if (loadedWorkspace == null)
                    ShowError("Workspace", $"Workspace '{Path.GetFileName(_globalConfig.WorkspacePath)}' not found", $"The workspace by path '{_globalConfig.WorkspacePath}' could not be load!");
                else
                    _workspace.Assign(loadedWorkspace);
            }
            _globalConfig.WorkspacePath = _workspace.FilePath;
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
            miViewShowWhitespaces.Checked = _workspace.View.IsWhitespaceVisible;
            RefreshRecentFiles();

            _searchControl = new SearchReplace.SearchReplaceControl();
            Controls.Add(_searchControl);

            TextSelectedTimer = new System.Windows.Forms.Timer() { Enabled = true, Interval = 500 };
            TextSelectedTimer.Tick += (s, e) =>
            {
                if (tcFiles.TabPages.Count > 0)
                {
                    Debug.Assert(tcFiles.SelectedTab != null);
                    EditorContext context = (EditorContext)tcFiles.SelectedTab.Tag;
                    UpdateMenuSelection(context);
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
                Text = $"{_appName} - {Path.GetFileNameWithoutExtension(_workspace.FilePath)}";
        }

        private void SetParseStatus(string status)
        {
            tsslblParseStatusLabel.Text = status;
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
                EditorContext context = null;
                if (tcFiles.TabPages.Count > 0)
                {
                    Debug.Assert(tcFiles.SelectedTab != null);
                    context = (EditorContext)tcFiles.SelectedTab.Tag;
                }
                UpdateMenuEditChange(context);
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
                EditorContext context = (EditorContext)tab.Tag;
                if (context.FilePath == null)
                {
                    string name = context.Name;
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

        private IEnumerable<EditorContext> GetChangedEditorContexts()
        {
            List<EditorContext> result = new List<EditorContext>();
            foreach (TabPage tab in tcFiles.TabPages)
            {
                EditorContext context = (EditorContext)tab.Tag;
                if (context.IsChanged)
                    result.Add(context);
            }
            return (result);
        }

        private EditorContext FindEditorContextById(ISymbolTableId id)
        {
            foreach (TabPage tab in tcFiles.TabPages)
            {
                EditorContext context = (EditorContext)tab.Tag;
                if (context == id)
                    return (context);
            }
            return (null);
        }

        private IEnumerable<EditorContext> GetAllEditorContexts()
        {
            List<EditorContext> result = new List<EditorContext>();
            foreach (TabPage tab in tcFiles.TabPages)
            {
                EditorContext context = (EditorContext)tab.Tag;
                result.Add(context);
            }
            return (result);
        }

        private void UpdateContext(EditorContext context)
        {
            IEnumerable<EditorContext> changedContexts = GetChangedEditorContexts();
            bool anyChanges = changedContexts.Count() > 0;

            miFileRefresh.Enabled = tbtnFileRefresh.Enabled = context != null;
            miFileSave.Enabled = tbtnFileSave.Enabled = context != null && context.IsChanged;
            miFileSaveAll.Enabled = tbtnFileSaveAll.Enabled = anyChanges;
            miFileClose.Enabled = tcFiles.SelectedTab != null;
            miFileCloseAll.Enabled = tcFiles.TabCount > 0;

            if (context != null)
            {
                string title = context.Name;
                if (context.IsChanged) title += "*";
                TabPage tab = (TabPage)context.Tab;
                tab.Text = title;
            }

            UpdateMenuEditChange(context);
            UpdateMenuSelection(context);
        }

        private EditorContext AddFileTab(string name)
        {
            int tabIndex = _newTabCounter++;
            TabPage newTab = new TabPage() { Text = name };
            EditorContext newContext = new EditorContext(this, _workspace, name, newTab, tabIndex);
            newContext.IsShowWhitespace = miViewShowWhitespaces.Checked;
            newContext.TabUpdating += (s, e) => UpdateContext((EditorContext)s);
            newContext.FocusChanged += (s, e) =>
            {
                UpdateMenuEditChange(newContext);
                UpdateMenuSelection(newContext);
            };
            newContext.ParseCompleted += (IParseInfo parseInfo) =>
            {
                RebuildSymbolTree(newContext, parseInfo.DoxyTree);
                AddPerformanceItems(newContext);
                bool isComplete = Interlocked.Decrement(ref _parseProgressCount) == 0;
                if (isComplete)
                {
                    Interlocked.Exchange(ref _parseTotalCount, 0);
                    IEnumerable<EditorContext> contexts = GetAllEditorContexts();
                    RefreshIssues(contexts);
                    SetParseStatus("");
                }
                else
                    SetParseStatus($"Parsing {_parseProgressCount} of {_parseTotalCount}");
            };
            newContext.ParseStarting += (IParseInfo parseInfo) =>
            {
                Interlocked.Increment(ref _parseTotalCount);
                Interlocked.Increment(ref _parseProgressCount);
                ClearPerformanceItems(newContext);
                SetParseStatus($"Parsing {_parseProgressCount} of {_parseTotalCount}");
            };
            newContext.JumpToEditor += (id, pos) =>
            {
                var foundContext = FindEditorContextById(id);
                if (foundContext != null)
                {
                    TabPage tab = (TabPage)foundContext.Tab;
                    tcFiles.SelectedIndex = tcFiles.TabPages.IndexOf(tab);
                    foundContext.GoToPosition(pos);
                }
            };
            newTab.Tag = newContext;
            newTab.Controls.Add(newContext.ContainerPanel);
            tcFiles.TabPages.Add(newTab);
            AddToSymbolTree(newContext, newContext.Name);
            return (newContext);
        }

        private void RemoveFileTab(EditorContext context)
        {
            context.Stop();
            RemoveFromSymbolTree(context);
            ClearPerformanceItems(context);
            GlobalSymbolCache.Remove(context);

            TabPage tab = (TabPage)context.Tab;
            tcFiles.TabPages.Remove(tab);
            context.Dispose();

            IEnumerable<EditorContext> contexts = GetAllEditorContexts();
            RefreshIssues(contexts);
        }

        private void OpenFileTab(string filePath)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(filePath));

            // Is the file already open?
            EditorContext alreadyOpenContext = null;
            foreach (TabPage tab in tcFiles.TabPages)
            {
                EditorContext context = (EditorContext)tab.Tag;
                if (string.Equals(context.FilePath, filePath))
                {
                    alreadyOpenContext = context;
                    break;
                }
            }

            if (alreadyOpenContext != null)
            {
                // Focus existing tab
                tcFiles.SelectedTab = (TabPage)alreadyOpenContext.Tab;
                alreadyOpenContext.SetFocus();
            }
            else
            {
                EditorContext newContext = AddFileTab(Path.GetFileName(filePath));
                TabPage tab = (TabPage)newContext.Tab;
                tcFiles.SelectedIndex = tcFiles.TabPages.IndexOf(tab);
                Tuple<bool, Exception> openRes = IOOpenFile(newContext, filePath);
                if (!openRes.Item1)
                {
                    Exception e = openRes.Item2;
                    var values = new Dictionary<string, string>() { { "filepath", filePath } };
                    var msg = e.ToErrorMessage("Open file", values);
                    ShowError(msg.Caption, msg.ShortText, msg.Details);
                    RemoveFileTab(newContext);
                }
                else
                {
                    _workspace.History.PushRecentFiles(filePath);
                    RefreshRecentFiles();

                    // Remove first tab when it was a "New" and is still unchanged
                    if (tcFiles.TabPages.Count == 2)
                    {
                        TabPage firstTab = tcFiles.TabPages[0];
                        EditorContext existingContext = (EditorContext)firstTab.Tag;
                        if (existingContext.FilePath == null && !existingContext.IsChanged)
                            RemoveFileTab(existingContext);
                    }

                    // Focus new tab
                    tcFiles.SelectedTab = tab;
                    newContext.SetFocus();
                }
            }
        }

        private bool CloseTabs(IEnumerable<EditorContext> contexts)
        {
            foreach (EditorContext context in contexts)
            {
                if (context.IsChanged)
                {
                    Tuple<bool, Exception> saveRes = SaveWithConfirmation(context, false);
                    if (!saveRes.Item1)
                        return (false);
                }
                RemoveFileTab(context);
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
                UpdateContext(null);
            else
            {
                TabPage selectedTab = tcFiles.TabPages[tcFiles.SelectedIndex];
                EditorContext context = (EditorContext)selectedTab.Tag;
                UpdateContext(context);
            }
        }
        #endregion

        #region IO
        private Tuple<bool, Exception> IOOpenFile(EditorContext context, string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string contents = reader.ReadToEnd();
                    context.FileEncoding = reader.CurrentEncoding;
                    context.SetText(contents);
                }
            }
            catch (IOException e)
            {
                return new Tuple<bool, Exception>(false, e);
            }
            context.Name = Path.GetFileName(filePath);
            context.FilePath = filePath;
            context.IsChanged = false;
            UpdateContext(context);
            return new Tuple<bool, Exception>(true, null);
        }

        private Tuple<bool, Exception> IOSaveFile(EditorContext context)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(context.FilePath, false, context.FileEncoding))
                {
                    writer.Write(context.GetText());
                    writer.Flush();
                }
                context.IsChanged = false;
                UpdateContext(context);
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
            var allFilePaths = GetAllEditorContexts().Where(f => !string.IsNullOrWhiteSpace(f.FilePath)).Select(f => f.FilePath);
            _workspace.History.UpdateLastOpenedFiles(allFilePaths);
            IEnumerable<EditorContext> changes = GetChangedEditorContexts();
            if (changes.Count() > 0)
                e.Cancel = !CloseTabs(changes);
            if (!e.Cancel)
            {
                _workspace.Save();
                _globalConfig.Save();
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {

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
                if (_workspace.History.LastOpenedFileCount > 0)
                {
                    foreach (var lastOpenedFilePath in _workspace.History.LastOpenedFiles)
                    {
                        if (!string.IsNullOrWhiteSpace(lastOpenedFilePath))
                            OpenFileTab(lastOpenedFilePath);
                    }
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
                        EditorContext context = (EditorContext)parentNode.Tag;
                        TabPage tab = (TabPage)context.Tab;
                        tcFiles.SelectedTab = tab;
                        tcFiles.Focus();
                        DoxygenNode entityNode = (DoxygenNode)selectedNode.Tag;
                        context.GoToPosition(entityNode.Entity.StartRange.Index);
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
        private Tuple<bool, Exception> SaveFileAs(EditorContext context, string filePath)
        {
            context.FilePath = filePath;
            context.Name = Path.GetFileName(filePath);
            RenamedInSymbolTree(context, context.Name);
            Tuple<bool, Exception> result = IOSaveFile(context);
            return (result);
        }

        private Tuple<bool, Exception> SaveWithConfirmation(EditorContext context, bool skipConfirmation)
        {
            Debug.Assert(context.IsChanged);
            string caption = $"File '{context.Name}' was changed";
            string text = $"The file '{context.Name}' contains changes, do you want to save it first before continue?";
            DialogResult r = skipConfirmation ? DialogResult.OK : MessageBox.Show(this, text, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (r == DialogResult.Cancel)
                return new Tuple<bool, Exception>(false, null);
            else if (r == DialogResult.No)
                return new Tuple<bool, Exception>(true, null);
            else
            {
                if (string.IsNullOrEmpty(context.FilePath))
                {
                    if (dlgSaveFile.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = dlgSaveFile.FileName;
                        Tuple<bool, Exception> result = SaveFileAs(context, filePath);
                        return (result);
                    }
                    else return new Tuple<bool, Exception>(false, null);
                }
                else
                {
                    Tuple<bool, Exception> result = IOSaveFile(context);
                    if (!result.Item1)
                    {
                        string filePath = context.FilePath;
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
            EditorContext context = (EditorContext)tcFiles.SelectedTab.Tag;
            if (context.IsChanged)
            {
                var caption = $"Revert file '{context.Name}'";
                var text = $"The file '{context.Name}' has changes, do you want to reload and revert it?";
                var dlgResult = MessageBox.Show(this, text, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dlgResult != DialogResult.Yes)
                    return;
            }
            IOOpenFile(context, context.FilePath);
        }
        private void MenuActionFileNew(object sender, EventArgs e)
        {
            string name = GetNextTabName("File");
            EditorContext newContext = AddFileTab(name);
            TabPage tab = (TabPage)newContext.Tab;
            tcFiles.SelectedIndex = tcFiles.TabPages.IndexOf(tab);
            newContext.SetFocus();
            UpdateContext(newContext);
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
            EditorContext context = (EditorContext)tcFiles.SelectedTab.Tag;
            Tuple<bool, Exception> r = SaveWithConfirmation(context, true);
            if (r.Item1)
            {
                _workspace.History.PushRecentFiles(context.FilePath);
                RefreshRecentFiles();
            }
        }
        private void MenuActionFileSaveAs(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            if (dlgSaveFile.ShowDialog() == DialogResult.OK)
            {
                string filePath = dlgSaveFile.FileName;
                EditorContext context = (EditorContext)tcFiles.SelectedTab.Tag;
                Tuple<bool, Exception> r = SaveFileAs(context, filePath);
                if (!r.Item1)
                {
                    Exception ex = r.Item2;
                    var values = new Dictionary<string, string>() { { "filepath", filePath } };
                    var msg = ex.ToErrorMessage("Save file", values);
                    ShowError(msg.Caption, msg.ShortText, msg.Details);
                }
                else
                {
                    _workspace.History.PushRecentFiles(filePath);
                    RefreshRecentFiles();
                }

            }
        }
        private void MenuActionFileSaveAll(object sender, EventArgs e)
        {
            foreach (TabPage tab in tcFiles.TabPages)
            {
                EditorContext context = (EditorContext)tab.Tag;
                if (context.IsChanged)
                {
                    Tuple<bool, Exception> saveRes = SaveWithConfirmation(context, true);
                    if (!saveRes.Item1)
                        return;
                }
            }
        }
        private void MenuActionFileClose(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorContext context = (EditorContext)tcFiles.SelectedTab.Tag;
            List<EditorContext> tabsToClose = new List<EditorContext>();
            tabsToClose.Add(context);
            CloseTabs(tabsToClose);
        }
        private void MenuActionFileCloseAll(object sender, EventArgs e)
        {
            List<EditorContext> tabsToClose = new List<EditorContext>();
            foreach (TabPage tab in tcFiles.TabPages)
                tabsToClose.Add((EditorContext)tab.Tag);
            CloseTabs(tabsToClose);
        }
        private void MenuActionFileCloseAllButThis(object sender, EventArgs e)
        {
            List<EditorContext> tabsToClose = new List<EditorContext>();
            foreach (TabPage tab in tcFiles.TabPages)
            {
                if (tab != tcFiles.SelectedTab)
                    tabsToClose.Add((EditorContext)tab.Tag);
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
            EditorContext context = (EditorContext)tcFiles.SelectedTab.Tag;
            context.ShowSearch();
        }
        private void MenuActionEditSearchAndReplaceQuickReplace(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorContext context = (EditorContext)tcFiles.SelectedTab.Tag;
            context.ShowReplace();
        }
        private void MenuActionEditUndo(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorContext context = (EditorContext)tcFiles.SelectedTab.Tag;
            context.Undo();
        }
        private void MenuActionEditRedo(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorContext context = (EditorContext)tcFiles.SelectedTab.Tag;
            context.Redo();
        }
        private void MenuActionEditCut(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorContext context = (EditorContext)tcFiles.SelectedTab.Tag;
            context.Cut();
        }
        private void MenuActionEditCopy(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorContext context = (EditorContext)tcFiles.SelectedTab.Tag;
            context.Copy();
        }
        private void MenuActionEditPaste(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorContext context = (EditorContext)tcFiles.SelectedTab.Tag;
            context.Paste();
        }
        private void MenuActionEditSelectAll(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorContext context = (EditorContext)tcFiles.SelectedTab.Tag;
            context.SelectAll();
        }

        private void MenuActionEditGoToSymbol(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorContext context = (EditorContext)tcFiles.SelectedTab.Tag;

            // @SPEED(final): Cache conversion to SymbolItemModel and types as well
            List<SymbolItemModel> symbols = new List<SymbolItemModel>();
            HashSet<string> types = new HashSet<string>();
            var allSources = GlobalSymbolCache.GetSources(context);
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
                context.GoToPosition(selectedItem.Position.Index);
            }
        }

        private void MenuActionViewShowWhitespaces(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            bool enabled = !item.Checked;
            foreach (TabPage tab in tcFiles.TabPages)
            {
                EditorContext context = (EditorContext)tab.Tag;
                context.IsShowWhitespace = enabled;
            }
            item.Checked = enabled;
            _workspace.View.IsWhitespaceVisible = enabled;
        }

        private void MenuActionHelpAbout(object sender, EventArgs e)
        {
            AboutForm form = new AboutForm();
            form.ShowDialog(this);
        }

        private void UpdateMenuSelection(EditorContext context)
        {
            miEditCut.Enabled = context != null && context.CanCut();
            miEditCopy.Enabled = context != null && context.CanCopy();
        }

        private void UpdateMenuEditChange(EditorContext context)
        {
            miEditUndo.Enabled = tbtnEditUndo.Enabled = context != null && context.CanUndo();
            miEditRedo.Enabled = tbtnEditRedo.Enabled = context != null && context.CanRedo();
            miEditPaste.Enabled = context != null && context.CanPaste();
        }
        private void RefreshRecentFiles()
        {
            miFileRecentFiles.DropDownItems.Clear();
            foreach (string recentFile in _workspace.History.RecentFiles)
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
            public EditorContext Context { get; }
            public TextPosition Pos { get; }
            public IssueTag(EditorContext context, TextPosition pos)
            {
                Context = context;
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
        private void AddIssuesFromNode(IEnumerable<EditorContext> contexts, EditorContext mainContext, IBaseNode rootNode, string fileName, string groupName)
        {
            if (typeof(CppNode).Equals(rootNode.GetType()))
            {
                CppNode cppNode = (CppNode)rootNode;
                CppEntity cppEntity = cppNode.Entity;
                if (cppEntity.IsDefinition && cppEntity.DocumentationNode != null && _workspace.ValidationCpp.RequireDoxygenReference)
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
                            AddIssue(lvDoxygenIssues, new IssueTag(mainContext, cppEntity.StartRange.Position), IssueType.Warning, "Not documented (Add a @see @ref [section or page id])", cppEntity.Id, cppEntity.Kind.ToString(), "C/C++ Documentation", cppEntity.StartRange.Position.Line + 1, fileName);
                        }
                    }
                }
            }
            foreach (var child in rootNode.Children)
            {
                AddIssuesFromNode(contexts, mainContext, child, fileName, "Child");
            }
        }
        private void RefreshIssues(IEnumerable<EditorContext> contexts)
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
            var validationConfig = new GlobalSymbolCache.ValidationConfigration()
            {
                ExcludeCppPreprocessorMatch = _workspace.ValidationCpp.ExcludePreprocessorMatch,
                ExcludeCppPreprocessorUsage = _workspace.ValidationCpp.ExcludePreprocessorUsage,
            };
            var symbolErrors = GlobalSymbolCache.Validate(validationConfig);
            foreach (var errorPair in symbolErrors)
            {
                var error = errorPair.Value;
                var context = (EditorContext)errorPair.Key;
                var symbol = (ReferenceSymbol)error.Tag;
                var nodeType = symbol.Node.GetType();
                if (typeof(CppNode).Equals(nodeType))
                    AddIssue(lvCppIssues, new IssueTag(context, error.Pos), IssueType.Error, error.Message, error.Symbol, error.What, error.Category, error.Pos.Line + 1, context.Name);
                else if (typeof(DoxygenNode).Equals(nodeType))
                    AddIssue(lvDoxygenIssues, new IssueTag(context, error.Pos), IssueType.Error, error.Message, error.Symbol, error.What, error.Category, error.Pos.Line + 1, context.Name);
            }

            foreach (EditorContext context in contexts)
            {
                IParseInfo parseInfo = context.ParseInfo;
                foreach (var error in parseInfo.Errors)
                {
                    var errorType = error.Tag.GetType();
                    if (typeof(CppLexer).Equals(errorType) || typeof(CppParser).Equals(errorType))
                        AddIssue(lvCppIssues, new IssueTag(context, error.Pos), IssueType.Error, error.Message, null, null, error.Category, error.Pos.Line + 1, context.Name);
                    else if (typeof(DoxygenLexer).Equals(errorType) || typeof(DoxygenParser).Equals(errorType))
                        AddIssue(lvDoxygenIssues, new IssueTag(context, error.Pos), IssueType.Error, error.Message, null, null, error.Category, error.Pos.Line + 1, context.Name);
                }

                if (parseInfo.CppTree != null)
                {
                    AddIssuesFromNode(contexts, context, parseInfo.CppTree, context.Name, "Root");
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
                EditorContext context = tag.Context;
                TabPage tab = (TabPage)context.Tab;
                tcFiles.SelectedTab = tab;
                context.GoToPosition(pos.Index);
            }
        }
        #endregion

        #region Performance
        private void AddPerformanceItem(IEditorId id, PerformanceItemModel item)
        {
            ListViewItem newItem = new ListViewItem(id.Name);
            newItem.Tag = item;
            newItem.SubItems.Add(item.Input);
            newItem.SubItems.Add(item.Output);
            newItem.SubItems.Add(item.What);
            newItem.SubItems.Add(item.Duration.ToMilliseconds());
            lvPerformance.Items.Add(newItem);
        }

        private void AddPerformanceItems(EditorContext context)
        {
            IParseInfo parseInfo = context.ParseInfo;
            ClearPerformanceItems(context);
            lvPerformance.BeginUpdate();
            foreach (var item in parseInfo.PerformanceItems)
            {
                AddPerformanceItem(context, item);
            }
            lvPerformance.Sort();
            lvPerformance.EndUpdate();
        }

        private void ClearPerformanceItems(IEditorId id)
        {
            List<ListViewItem> itemsToRemove = new List<ListViewItem>();
            lvPerformance.BeginUpdate();
            foreach (ListViewItem item in lvPerformance.Items)
            {
                PerformanceItemModel performanceItem = (PerformanceItemModel)item.Tag;
                if (performanceItem.Id == id)
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
            {
                _workspace.Assign(dlg.Workspace);
                IEnumerable<EditorContext> contexts = GetAllEditorContexts();
                foreach (EditorContext context in contexts) {
                    context.Reparse();
                }
            }
        }

        private void miWorkspaceNew_Click(object sender, EventArgs e)
        {
            dlgSaveWorkspace.FileName = null;
            if (dlgSaveWorkspace.ShowDialog() == DialogResult.OK)
            {
                _globalConfig.WorkspacePath = dlgSaveWorkspace.FileName;
                var newWorkspace = new WorkspaceModel(_globalConfig.WorkspacePath);
                _workspace.Assign(newWorkspace);
                UpdatedWorkspaceFile();
            }
        }

        private void mitWorkspaceLoad_Click(object sender, EventArgs e)
        {
            if (dlgOpenWorkspace.ShowDialog() == DialogResult.OK)
            {
                WorkspaceModel newWorkspace = WorkspaceModel.Load(dlgOpenWorkspace.FileName);
                if (newWorkspace == null)
                {
                    ShowError("Workspace", $"Workspace '{Path.GetFileName(_globalConfig.WorkspacePath)}' not found", $"The workspace by path '{_globalConfig.WorkspacePath}' could not be load!");
                    _workspace.Assign(new WorkspaceModel(_defaultWorkspaceFilePath));
                }
                else
                    _workspace.Assign(newWorkspace);
                _globalConfig.WorkspacePath = _workspace.FilePath;
                UpdatedWorkspaceFile();
            }
        }
    }
}