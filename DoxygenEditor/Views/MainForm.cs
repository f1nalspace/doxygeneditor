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
using System.Text;

namespace TSP.DoxygenEditor.Views
{
    public partial class MainForm : Form
    {
        private readonly GlobalConfigModel _globalConfig;
        private readonly WorkspaceModel _workspace;
        private readonly string _appName;
        private readonly string _dataPath;
        private readonly string _defaultWorkspaceFilePath;
        private readonly object _performanceItemsSummaryRoot = new object();

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
            listview.SetGroupColumn("File");
            listview.Comparer = (a, b) =>
            {
                IssueTag tagA = a.Tag as IssueTag;
                IssueTag tagB = b.Tag as IssueTag;

                // Sort by file path first
                if (!string.Equals(tagA.Context.FilePath, tagB.Context.FilePath))
                    return string.Compare(tagA.Context.FilePath, tagB.Context.FilePath);

                // Sort by line second
                if (Math.Abs(tagA.Pos.Line - tagB.Pos.Line) > 0)
                    return tagA.Pos.Line - tagB.Pos.Line;

                // Sort by issue type second
                if (Math.Abs(tagA.Type - tagB.Type) > 0)
                    return Math.Sign(tagA.Type - tagB.Type);

                return (0);
            };
        }

        public MainForm()
        {
            InitializeComponent();

            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName asmName = assembly.GetName();
            FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string companyName = verInfo.CompanyName;
            string appId = verInfo.FileDescription;

            _appName = $"{verInfo.ProductName}";

            if (string.IsNullOrWhiteSpace(companyName))
                throw new Exception("Company name is missing in assembly!");
            if (string.IsNullOrWhiteSpace(appId))
                throw new Exception("Title is missing in assembly!");

            _dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), companyName, appId);
            if (!Directory.Exists(_dataPath)) Directory.CreateDirectory(_dataPath);
            _defaultWorkspaceFilePath = Path.Combine(_dataPath, "DefaultWorkspace.doxyedit");

            _globalConfig = new GlobalConfigModel(companyName, appId);
            _globalConfig.Load();

            StringBuilder fileExtensionsFilter = new StringBuilder();
            string allExtensions = string.Join(";", _globalConfig.SupportedFileExtensions.Select(c => string.Join(";", c.Extensions.Select(x => $"*{x}"))));
            fileExtensionsFilter.Append($"All supported files|{allExtensions}|");
            foreach (GlobalConfigModel.FileExtensions supportedFileExtensionsEntry in _globalConfig.SupportedFileExtensions)
                fileExtensionsFilter.Append($"{supportedFileExtensionsEntry.Name}|{string.Join(";", supportedFileExtensionsEntry.Extensions.Select(x => $"*{x}"))}|");
            fileExtensionsFilter.Append($"All files (*.*)|*.*");
            dlgOpenFile.Filter = fileExtensionsFilter.ToString();
            dlgSaveFile.Filter = fileExtensionsFilter.ToString();

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

            const int minDistance = 20;
            int distance = scTreeAndFiles.ClientSize.Width / 2;
            if (_workspace.View.TreeSplitterDistance > 0)
            {
                int d = (int)(scTreeAndFiles.ClientSize.Width * _workspace.View.TreeSplitterDistance);
                distance = Math.Min(Math.Max(d, minDistance), scTreeAndFiles.ClientSize.Width - minDistance);
            }
            scTreeAndFiles.SplitterDistance = distance;
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

        private EditorContext AddFileTab(string name, EditorFileType fileType)
        {
            int tabIndex = _newTabCounter++;
            TabPage newTab = new TabPage() { Text = name };
            EditorContext context = new EditorContext(this, _workspace, name, newTab, tabIndex);
            context.FileType = fileType;
            context.IsShowWhitespace = miViewShowWhitespaces.Checked;
            context.TabUpdating += (s, e) => UpdateContext((EditorContext)s);
            context.FocusChanged += (s, e) =>
            {
                UpdateMenuEditChange(context);
                UpdateMenuSelection(context);
            };
            context.ParseCompleted += (IParseInfo parseInfo) =>
            {
                RebuildSymbolTree(context, parseInfo.DoxyBlockTree);
                AddPerformanceItemsFor(context);
                bool isComplete = Interlocked.Decrement(ref _parseProgressCount) == 0;
                if (isComplete)
                {
                    Interlocked.Exchange(ref _parseTotalCount, 0);
                    IEnumerable<EditorContext> contexts = GetAllEditorContexts();
                    IssuesTimings timings = RefreshIssues(contexts);
                    RefreshPerformanceSummary(timings);
                    SetParseStatus("");
                }
                else
                    SetParseStatus($"Parsing {_parseProgressCount} of {_parseTotalCount}");
            };
            context.ParseStarting += (IParseInfo parseInfo) =>
            {
                Interlocked.Increment(ref _parseTotalCount);
                Interlocked.Increment(ref _parseProgressCount);
                ClearPerformanceItemsFrom(context);
                SetParseStatus($"Parsing {_parseProgressCount} of {_parseTotalCount}");
            };
            context.JumpToEditor += (id, pos) =>
            {
                EditorContext foundContext = FindEditorContextById(id);
                if (foundContext != null)
                {
                    TabPage tab = (TabPage)foundContext.Tab;
                    tcFiles.SelectedIndex = tcFiles.TabPages.IndexOf(tab);
                    foundContext.GoToPosition(pos);
                }
            };
            newTab.Tag = context;
            newTab.Controls.Add(context.ContainerPanel);
            tcFiles.TabPages.Add(newTab);
            AddToSymbolTree(context, context.Name);
            return (context);
        }

        private void RemoveFileTab(EditorContext context)
        {
            context.Stop();
            RemoveFromSymbolTree(context);
            ClearPerformanceItemsFrom(context);
            GlobalSymbolCache.Remove(context);

            TabPage tab = (TabPage)context.Tab;
            tcFiles.TabPages.Remove(tab);
            context.Dispose();

            IEnumerable<EditorContext> contexts = GetAllEditorContexts();
            IssuesTimings timings = RefreshIssues(contexts);
            RefreshPerformanceSummary(timings);
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
                string fileExt = Path.GetExtension(filePath);
                EditorFileType supportedFileType = _globalConfig.GetFileTypeByExtension(fileExt);
                EditorContext newContext = AddFileTab(Path.GetFileName(filePath), supportedFileType);
                TabPage tab = (TabPage)newContext.Tab;
                tcFiles.SelectedIndex = tcFiles.TabPages.IndexOf(tab);
                Tuple<bool, Exception> openRes = IOOpenFile(newContext, filePath);
                if (!openRes.Item1)
                {
                    Exception e = openRes.Item2;
                    Dictionary<string, string> values = new Dictionary<string, string>() { { "filepath", filePath } };
                    ErrorMessageModel msg = e.ToErrorMessage("Open file", values);
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
            // Update workspace settings before application shutdown
            IEnumerable<string> allFilePaths = GetAllEditorContexts().Where(f => !string.IsNullOrWhiteSpace(f.FilePath)).Select(f => f.FilePath);
            _workspace.History.UpdateLastOpenedFiles(allFilePaths);
            _workspace.View.TreeSplitterDistance = scTreeAndFiles.SplitterDistance / (double)scTreeAndFiles.ClientSize.Width;

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
                    foreach (string lastOpenedFilePath in _workspace.History.LastOpenedFiles)
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
                        DoxygenBlockNode entityNode = (DoxygenBlockNode)selectedNode.Tag;
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

        private static HashSet<DoxygenBlockEntityKind> AllowedDoxyEntities = new HashSet<DoxygenBlockEntityKind>()
        {
            DoxygenBlockEntityKind.BlockSingle,
            DoxygenBlockEntityKind.BlockMulti,
            DoxygenBlockEntityKind.Group,
            DoxygenBlockEntityKind.Page,
            DoxygenBlockEntityKind.Section,
            DoxygenBlockEntityKind.SubSection,
            DoxygenBlockEntityKind.SubSubSection,
        };

        private List<TreeNode> BuildSymbolTree(IBaseNode rootEntityNode, TreeNode rootTreeNode, IBaseNode selectedEntityNode)
        {
            List<TreeNode> result = new List<TreeNode>();
            foreach (IBaseNode childEntityNode in rootEntityNode.Children)
            {
                if (typeof(DoxygenBlockNode).Equals(childEntityNode.GetType()))
                {
                    DoxygenBlockNode doxyNode = (DoxygenBlockNode)childEntityNode;
                    DoxygenBlockEntity entity = doxyNode.Entity;
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
            DoxygenBlockNode lastEntity = null;
            if (tvTree.SelectedNode != null)
                lastEntity = tvTree.SelectedNode.Tag as DoxygenBlockNode;

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
                        Dictionary<string, string> values = new Dictionary<string, string>() { { "filepath", filePath } };
                        ErrorMessageModel msg = e.ToErrorMessage("Save file", values);
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
                string caption = $"Revert file '{context.Name}'";
                string text = $"The file '{context.Name}' has changes, do you want to reload and revert it?";
                DialogResult dlgResult = MessageBox.Show(this, text, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dlgResult != DialogResult.Yes)
                    return;
            }
            IOOpenFile(context, context.FilePath);
        }
        private void MenuActionFileNew(object sender, EventArgs e)
        {
            string name = GetNextTabName("File");
            EditorContext newContext = AddFileTab(name, EditorFileType.Cpp);
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
                    Dictionary<string, string> values = new Dictionary<string, string>() { { "filepath", filePath } };
                    ErrorMessageModel msg = ex.ToErrorMessage("Save file", values);
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
            IEnumerable<SourceSymbol> allSources = GlobalSymbolCache.GetSources(context);
            foreach (SourceSymbol source in allSources)
            {
                if (source.Node == null) continue;
                symbols.Add(new SymbolItemModel()
                {
                    Caption = source.Caption,
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
            public IssueType Type { get; }
            public IssueTag(EditorContext context, TextPosition pos, IssueType type)
            {
                Context = context;
                Pos = pos;
                Type = type;
            }
        }
        private void AddIssue(FilterListView listView, IssueTag tag, string message, string symbolName, string symbolType, string group, int line, string file)
        {
            ListViewItem newItem = new ListViewItem(message);
            newItem.Tag = tag;
            newItem.ImageIndex = (int)tag.Type;
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
                    DoxygenBlockNode doxyNode = (DoxygenBlockNode)cppEntity.DocumentationNode;
                    TextPosition docsPos = cppEntity.DocumentationNode.StartRange.Position;
                    if (doxyNode.Entity.Kind == DoxygenBlockEntityKind.BlockMulti)
                    {
                        DoxygenBlockNode seeNode = doxyNode.TypedChildren.FirstOrDefault(c => c.Entity.Kind == DoxygenBlockEntityKind.See) as DoxygenBlockNode;
                        bool hasDocumented = false;
                        if (seeNode != null)
                        {
                            DoxygenBlockNode refNode = seeNode.TypedChildren.FirstOrDefault(c => c.Entity.Kind == DoxygenBlockEntityKind.Reference) as DoxygenBlockNode;
                            if (refNode != null)
                            {
                                hasDocumented = true;
                            }
                        }

                        if (!hasDocumented)
                        {
                            AddIssue(lvDoxygenIssues, new IssueTag(mainContext, docsPos, IssueType.Warning), "Missing documentation reference (Add a @see @ref [section or page id])", cppEntity.Id, cppEntity.Kind.ToString(), "C/C++ Documentation", cppEntity.StartRange.Position.Line + 1, fileName);
                        }
                    }
                }
            }
            foreach (IBaseNode child in rootNode.Children)
            {
                AddIssuesFromNode(contexts, mainContext, child, fileName, "Child");
            }
        }

        struct IssuesTimings
        {
            public TimeSpan ValidationDuration { get; set; }
            public TimeSpan ClearDuration { get; set; }
            public TimeSpan CollectDuration { get; set; }
            public TimeSpan RefreshDuration { get; set; }
            public TimeSpan SelectDuration { get; set; }
            public TimeSpan TotalDuration { get; set; }
        }

        private IssuesTimings RefreshIssues(IEnumerable<EditorContext> contexts)
        {
            Stopwatch total = Stopwatch.StartNew();
            Stopwatch w = new Stopwatch();
            IssuesTimings result = new IssuesTimings();

            // Validate symbols from cache
            w.Restart();
            GlobalSymbolCache.ValidationConfigration validationConfig = new GlobalSymbolCache.ValidationConfigration()
            {
                ExcludeCppPreprocessorMatch = _workspace.ValidationCpp.ExcludePreprocessorMatch,
                ExcludeCppPreprocessorUsage = _workspace.ValidationCpp.ExcludePreprocessorUsage,
            };
            IEnumerable<KeyValuePair<ISymbolTableId, TextError>> symbolErrors = GlobalSymbolCache.Validate(validationConfig);
            result.ValidationDuration = w.StopAndReturn();

            lvCppIssues.BeginUpdate();
            lvDoxygenIssues.BeginUpdate();

            //
            // Clear issues
            //
            w.Restart();
            lvCppIssues.ClearSelection();
            lvDoxygenIssues.ClearSelection();
            lvCppIssues.ClearItems();
            lvDoxygenIssues.ClearItems();
            result.ClearDuration = w.StopAndReturn();

            //
            // Collect issues
            //
            w.Restart();
            int selectedCppIssueIndex = lvCppIssues.SelectedIndex;
            int selectedDoxyIssueIndex = lvCppIssues.SelectedIndex;
            IssueTag selectedCppIssue = lvCppIssues.SelectedItem?.Tag as IssueTag;
            IssueTag selectedDoxyIssue = lvDoxygenIssues.SelectedItem?.Tag as IssueTag;

            w.Restart();
            foreach (KeyValuePair<ISymbolTableId, TextError> errorPair in symbolErrors)
            {
                TextError error = errorPair.Value;
                EditorContext context = (EditorContext)errorPair.Key;
                ReferenceSymbol symbol = (ReferenceSymbol)error.Tag;
                Type nodeType = symbol.Node.GetType();
                if (typeof(CppNode).Equals(nodeType))
                    AddIssue(lvCppIssues, new IssueTag(context, error.Pos, IssueType.Error), error.Message, error.Symbol, error.What, error.Category, error.Pos.Line + 1, context.Name);
                else if (typeof(DoxygenBlockNode).Equals(nodeType))
                    AddIssue(lvDoxygenIssues, new IssueTag(context, error.Pos, IssueType.Error), error.Message, error.Symbol, error.What, error.Category, error.Pos.Line + 1, context.Name);
            }

            foreach (EditorContext context in contexts)
            {
                IParseInfo parseInfo = context.ParseInfo;
                foreach (TextError error in parseInfo.Errors)
                {
                    Type errorType = error.Tag.GetType();
                    if (typeof(CppLexer).Equals(errorType) || typeof(CppParser).Equals(errorType))
                        AddIssue(lvCppIssues, new IssueTag(context, error.Pos, IssueType.Error), error.Message, null, null, error.Category, error.Pos.Line + 1, context.Name);
                    else if (typeof(DoxygenBlockLexer).Equals(errorType) || typeof(DoxygenConfigLexer).Equals(errorType) || typeof(DoxygenBlockParser).Equals(errorType))
                        AddIssue(lvDoxygenIssues, new IssueTag(context, error.Pos, IssueType.Error), error.Message, null, null, error.Category, error.Pos.Line + 1, context.Name);
                }

                if (parseInfo.CppTree != null)
                {
                    AddIssuesFromNode(contexts, context, parseInfo.CppTree, context.Name, "Root");
                }
            }
            result.CollectDuration = w.StopAndReturn();

            //
            // Refresh items
            //
            w.Restart();
            lvCppIssues.RefreshItems();
            lvDoxygenIssues.RefreshItems();
            result.RefreshDuration = w.StopAndReturn();

            //
            // Select item
            //
            w.Restart();
            lvCppIssues.SelectItemOrIndex(selectedCppIssue, selectedCppIssueIndex);
            lvDoxygenIssues.SelectItemOrIndex(selectedDoxyIssue, selectedDoxyIssueIndex);
            result.SelectDuration = w.StopAndReturn();

            result.TotalDuration = total.StopAndReturn();

            lvCppIssues.EndUpdate();
            lvDoxygenIssues.EndUpdate();

            tpCppIssues.Text = $"C/C++ Issues [{lvCppIssues.ItemCount}]";
            tpDoxygenIssues.Text = $"Doxygen Issues [{lvDoxygenIssues.ItemCount}]";

            return (result);
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
        private void AddPerformanceItem(PerformanceItemModel item, ListViewGroup group)
        {
            Debug.Assert(item != null);
            ListViewItem newItem = new ListViewItem(item.Name);
            newItem.Tag = item;
            newItem.SubItems.Add(item.Input);
            newItem.SubItems.Add(item.Output);
            newItem.SubItems.Add(item.What);
            newItem.SubItems.Add(item.Duration.ToMilliseconds());
            newItem.Group = group;
            lvPerformance.Items.Add(newItem);
        }



        private void AddPerformanceItemsFor(EditorContext context)
        {
            IParseInfo parseInfo = context.ParseInfo;
            ClearPerformanceItemsFrom(context);
            lvPerformance.BeginUpdate();
            Dictionary<string, ListViewGroup> groupNames = new Dictionary<string, ListViewGroup>();
            foreach (PerformanceItemModel item in parseInfo.PerformanceItems)
            {
                string groupName = item.Name;
                ListViewGroup group;
                if (!groupNames.ContainsKey(groupName))
                {
                    group = new ListViewGroup(groupName);
                    lvPerformance.Groups.Add(group);
                    groupNames.Add(groupName, group);
                }
                else group = groupNames[groupName];
                AddPerformanceItem(item, group);
            }
            lvPerformance.Sort();
            lvPerformance.AutoSizeColumnList();
            lvPerformance.EndUpdate();
        }

        private void ClearPerformanceItemsFrom(object tag, bool beUpdate = true)
        {
            List<ListViewItem> itemsToRemove = new List<ListViewItem>();
            if (beUpdate)
                lvPerformance.BeginUpdate();
            foreach (ListViewItem item in lvPerformance.Items)
            {
                PerformanceItemModel performanceItem = (PerformanceItemModel)item.Tag;
                if (performanceItem.Tag == tag)
                    itemsToRemove.Add(item);
            }
            ListViewGroup group = null;
            foreach (ListViewItem item in itemsToRemove)
            {
                lvPerformance.Items.Remove(item);
                if (group == null && item.Group != null)
                    group = item.Group;
            }
            if (group != null)
                lvPerformance.Groups.Remove(group);
            if (beUpdate)
                lvPerformance.EndUpdate();
        }

        private void RefreshPerformanceSummary(IssuesTimings timings)
        {
            lvPerformance.BeginUpdate();
            ClearPerformanceItemsFrom(_performanceItemsSummaryRoot, false);
            ListViewGroup newGroup = new ListViewGroup("Summary");
            lvPerformance.Groups.Add(newGroup);
            AddPerformanceItem(new PerformanceItemModel(_performanceItemsSummaryRoot, "Summary", -1, "", $"", "Issues (Total)", timings.TotalDuration), newGroup);
            AddPerformanceItem(new PerformanceItemModel(_performanceItemsSummaryRoot, "Summary", -1, "", $"", "Validation", timings.ValidationDuration), newGroup);
            AddPerformanceItem(new PerformanceItemModel(_performanceItemsSummaryRoot, "Summary", -1, "", $"", "Issues (Clear)", timings.ClearDuration), newGroup);
            AddPerformanceItem(new PerformanceItemModel(_performanceItemsSummaryRoot, "Summary", -1, "", $"", "Issues (Collect)", timings.CollectDuration), newGroup);
            AddPerformanceItem(new PerformanceItemModel(_performanceItemsSummaryRoot, "Summary", -1, "", $"", "Issues (Refresh)", timings.RefreshDuration), newGroup);
            AddPerformanceItem(new PerformanceItemModel(_performanceItemsSummaryRoot, "Summary", -1, "", $"", "Issues (Select)", timings.SelectDuration), newGroup);
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
                foreach (EditorContext context in contexts)
                {
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
                WorkspaceModel newWorkspace = new WorkspaceModel(_globalConfig.WorkspacePath);
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

        private void BuildDocumentationClick(object sender, EventArgs e)
        {
            miBuildDocumentation.Enabled = false;
            tbtnBuildDocumentation.Enabled = false;

            string baseDir = _workspace.Build.BaseDirectory;
            string configFilePath = null;
            if (!string.IsNullOrWhiteSpace(baseDir) && !string.IsNullOrWhiteSpace(_workspace.Build.ConfigFile))
            {
                if (Path.IsPathRooted(_workspace.Build.ConfigFile))
                    configFilePath = _workspace.Build.ConfigFile;
                else
                    configFilePath = Path.Combine(baseDir, _workspace.Build.ConfigFile);
            }
            string pathToDoxygen = _workspace.Build.PathToDoxygen;
            bool openInBrowser = _workspace.Build.OpenInBrowser;

            EditorContext configContext = null;
            IBaseNode configTree = null;
            foreach (TabPage tab in tcFiles.TabPages)
            {
                EditorContext context = (EditorContext)tab.Tag;
                if (context.FileType == EditorFileType.DoxyConfig)
                {
                    configContext = context;
                    break;
                }
            }

            if (configContext != null)
            {
                configFilePath = configContext.FilePath;
                configTree = configContext.ParseInfo.DoxyConfigTree;
                baseDir = Path.GetDirectoryName(configFilePath);
            }

            if ((configTree == null) && !string.IsNullOrWhiteSpace(configFilePath) && File.Exists(configFilePath))
            {
                // Config file comes from global configuration override, so we need to parse it
                string configContents = File.ReadAllText(configFilePath);
                if (!string.IsNullOrWhiteSpace(configFilePath))
                {
                    List<DoxygenToken> tokens = new List<DoxygenToken>();
                    using (DoxygenConfigLexer lexer = new DoxygenConfigLexer(configContents, configFilePath, new TextPosition(), configContents.Length))
                    {
                        tokens.AddRange(lexer.Tokenize());
                    }
                    using (DoxygenConfigParser parser = new DoxygenConfigParser(null))
                    {
                        parser.ParseTokens(tokens);
                        configTree = parser.Root;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(baseDir) && !string.IsNullOrWhiteSpace(configFilePath) && File.Exists(configFilePath))
            {
                string relativeOutputPath = null;

                if (configTree != null)
                {
                    IEnumerable<DoxygenConfigNode> children = configTree.GetChildrenAs<DoxygenConfigNode>();
                    foreach (DoxygenConfigNode child in children)
                    {
                        if (child.Entity.Kind == DoxygenConfigEntityKind.ConfigSet)
                        {
                            if ("OUTPUT_DIRECTORY".Equals(child.Entity.Id))
                            {
                                relativeOutputPath = child.Entity.Settings.FirstOrDefault();
                                break;
                            }
                        }
                    }
                }

                string outputPath = !string.IsNullOrWhiteSpace(relativeOutputPath) ? Path.Combine(baseDir, relativeOutputPath) : null;
                BuildDocumentationForm form = new BuildDocumentationForm(
                    baseDir: baseDir,
                    configFilePath: configFilePath,
                    compilerPath: pathToDoxygen,
                    outputPath: outputPath,
                    openInBrowser: openInBrowser);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _workspace.Build.OpenInBrowser = form.OpenInBrowser;
                }
            }
            else
            {
                string msg;
                if (string.IsNullOrWhiteSpace(configFilePath))
                    msg = "No doxygen configuration in tabs found";
                else
                    msg = $"doxygen configuration file '{configFilePath}' does not exists";
                MessageBox.Show(this, $"{msg}.{Environment.NewLine}Please add a valid doxygen configuration file with the extension '.doxygen'", "Missing doxygen configuration", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }

            miBuildDocumentation.Enabled = true;
            tbtnBuildDocumentation.Enabled = true;
        }
    }
}