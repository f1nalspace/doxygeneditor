using DoxygenEditor.Editor;
using DoxygenEditor.Models;
using DoxygenEditor.MVVM;
using DoxygenEditor.Parser;
using DoxygenEditor.Parser.Entities;
using DoxygenEditor.Utils;
using DoxygenEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DoxygenEditor.Views
{
    public partial class OldMainForm : Form
    {
        private readonly Dictionary<MainViewModel, TabPage> _viewModels = new Dictionary<MainViewModel, TabPage>();
        private readonly RootEntity _rootEntity;
        private MainViewModel _activeViewModel;
        private MainViewModel ActiveViewModel
        {
            get { return _activeViewModel; }
            set
            {
                _activeViewModel = value;
                ActiveViewModelChanged(_activeViewModel);
            }
        }

        private void ActiveViewModelChanged(MainViewModel viewModel)
        {
            // File menu
            CommandUtils.BindClickCommand(menuItemFileNew, viewModel.NewFileCommand);
            CommandUtils.BindClickCommand(menuItemFileOpen, viewModel.OpenFileCommand);
            CommandUtils.BindClickCommand(menuItemFileSave, viewModel.SaveFileCommand);
            CommandUtils.BindClickCommand(menuItemFileSaveAs, viewModel.SaveFileAsCommand);
            CommandUtils.BindClickCommand(menuItemFileReload, viewModel.ReloadFileCommand);

            // Edit menu
            CommandUtils.BindClickCommand(menuItemEditUndo, viewModel.UndoCommand);
            CommandUtils.BindClickCommand(menuItemEditRedo, viewModel.RedoCommand);

            CommandUtils.BindClickCommand(menuItemEditCut, viewModel.CutCommand);
            CommandUtils.BindClickCommand(menuItemEditCopy, viewModel.CopyCommand);
            CommandUtils.BindClickCommand(menuItemEditPaste, viewModel.PasteCommand);

            CommandUtils.BindClickCommand(menuItemEditSelectAll, viewModel.SelectAllCommand);

            CommandUtils.BindClickCommand(menuItemEditFindReplaceQuickSearch, viewModel.QuickSearchCommand);
            CommandUtils.BindClickCommand(menuItemEditFindReplaceQuickReplace, viewModel.QuickReplaceCommand);

            CommandUtils.BindClickCommand(menuItemEditUndo, viewModel.UndoCommand);
            CommandUtils.BindClickCommand(menuItemEditRedo, viewModel.RedoCommand);

            // View menu
            CommandUtils.BindCheckCommand(menuItemViewShowWhitespaces, viewModel.ViewWhitespacesCommand);

            // Tool buttons
            CommandUtils.BindClickCommand(toolButtonFileNew, viewModel.NewFileCommand);
            CommandUtils.BindClickCommand(toolButtonFileOpen, viewModel.OpenFileCommand);
            CommandUtils.BindClickCommand(toolButtonFileSave, viewModel.SaveFileCommand);
            CommandUtils.BindClickCommand(toolButtonFileSaveAs, viewModel.SaveFileAsCommand);
            CommandUtils.BindClickCommand(toolButtonEditUndo, viewModel.UndoCommand);
            CommandUtils.BindClickCommand(toolButtonEditRedo, viewModel.RedoCommand);
        }

        private MainViewModel CreateMainViewModel()
        {
            MainViewModel result = new MainViewModel();
            result = new MainViewModel();
            result.InsertEditorControl += viewModel_InsertEditorControl;
            result.UpdateTree += _viewModel_UpdateTree;
            result.ConfigurationChanged += (s, e) =>
            {
                MainViewModel mv = (MainViewModel)s;
                menuItemFileRecentFiles.DropDownItems.Clear();
                foreach (var recentFile in e.RecentFiles)
                {
                    ToolStripMenuItem recentItem = new ToolStripMenuItem(recentFile);
                    recentItem.Tag = recentFile;
                    recentItem.Click += (s2, e2) =>
                    {
                        string filePath = (string)(s2 as ToolStripMenuItem).Tag;
                        mv.OpenFileCommand.Execute(filePath);
                    };
                    menuItemFileRecentFiles.DropDownItems.Add(recentItem);
                }
                menuItemFileRecentFiles.Enabled = menuItemFileRecentFiles.DropDownItems.Count > 0;
            };
            result.PropertyChanged += (s, e) =>
            {
                MainViewModel mv = (MainViewModel)s;
                TabPage tab = _viewModels[mv];
                if (ReflectionUtils.GetMemberName((MainViewModel v) => v.IsChanged).Equals(e.PropertyName) || ReflectionUtils.GetMemberName((MainViewModel v) => v.FilePath).Equals(e.PropertyName))
                    UpdateFileState(mv, mv.FileHandler);
                else if (ReflectionUtils.GetMemberName((MainViewModel v) => v.LastStatus).Equals(e.PropertyName) || ReflectionUtils.GetMemberName((MainViewModel v) => v.LastParsedState).Equals(e.PropertyName) || ReflectionUtils.GetMemberName((MainViewModel v) => v.LastLexedState).Equals(e.PropertyName))
                    UpdateStatus(mv.LastStatus, mv.LastParsedState, mv.LastLexedState);
                else if (ReflectionUtils.GetMemberName((MainViewModel v) => v.LogItems).Equals(e.PropertyName))
                    UpdateLog(mv.LogItems);
            };
            return (result);
        }

        public OldMainForm()
        {
            InitializeComponent();
            _rootEntity = new RootEntity();

            MainViewModel rootViewModel = CreateMainViewModel();
            _viewModels.Add(rootViewModel, new TabPage() { Tag = rootViewModel });

            foreach (var viewModel in _viewModels)
            {
                tabControlFiles.TabPages.Add(viewModel.Value);
                viewModel.Key.ViewLoaded(this);
            }

            ActiveViewModel = _viewModels.Keys.First();
        }

        private int RefreshTreeChild(Entity sourceRoot, TreeNode targetRoot, Regex filterRex)
        {
            int result = 0;
            bool matched = (filterRex == null) || (filterRex.IsMatch(sourceRoot.DisplayName));
            if (matched)
                ++result;
            foreach (var sourceChild in sourceRoot.Children)
            {
                TreeNode targetChild = new TreeNode(sourceChild.DisplayName);
                targetChild.Tag = sourceChild;
                int count = RefreshTreeChild(sourceChild, targetChild, filterRex);
                if (count > 0)
                    targetRoot.Nodes.Add(targetChild);
                result += count;
            }
            return (result);
        }
        private void RefreshTree(TreeView tree, string filter)
        {
            tree.BeginUpdate();
            tree.Nodes.Clear();
            Regex rex = !string.IsNullOrEmpty(filter) ? new Regex(Regex.Escape(filter), RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase) : null;
            foreach (var sourceEntity in _rootEntity.Children)
            {
                TreeNode targetNode = new TreeNode(sourceEntity.DisplayName);
                targetNode.Tag = sourceEntity;
                int count = RefreshTreeChild(sourceEntity, targetNode, rex);
                if (count > 0)
                    tree.Nodes.Add(targetNode);
            }
            tree.EndUpdate();
        }
        private void _viewModel_UpdateTree(object sender, ParseState parseState)
        {
            _rootEntity.Clear();
            if (parseState != null)
            {
                //
                // Clone entities
                //

                // Pages
                foreach (PageEntity sourcePageEntity in parseState.RootEntity.Children.Where(p => typeof(PageEntity).IsInstanceOfType(p)))
                {
                    string pageCaption = !string.IsNullOrEmpty(sourcePageEntity.PageCaption) ? sourcePageEntity.PageCaption : sourcePageEntity.PageId;
                    PageEntity targetPageEntity = new PageEntity(sourcePageEntity.LineInfo, sourcePageEntity.PageId, sourcePageEntity.PageCaption);
                    _rootEntity.AddChild(targetPageEntity);

                    // Sections
                    foreach (SectionEntity sourceSectionEntity in sourcePageEntity.Children.Where(p => typeof(SectionEntity).Equals(p.GetType())))
                    {
                        string sectionCaption = !string.IsNullOrEmpty(sourceSectionEntity.SectionCaption) ? sourceSectionEntity.SectionCaption : sourceSectionEntity.SectionId;
                        SectionEntity targetSectionEntity = new SectionEntity(sourceSectionEntity.LineInfo, sourceSectionEntity.SectionId, sourceSectionEntity.SectionCaption);
                        targetPageEntity.AddChild(targetSectionEntity);

                        // Subsections
                        foreach (SubSectionEntity subSectionEntity in sourceSectionEntity.Children.Where(p => typeof(SubSectionEntity).Equals(p.GetType())))
                        {
                            string subSectionCaption = !string.IsNullOrEmpty(subSectionEntity.SectionCaption) ? subSectionEntity.SectionCaption : subSectionEntity.SectionId;
                            SubSectionEntity targetSubSectionEntity = new SubSectionEntity(subSectionEntity.LineInfo, subSectionEntity.SectionId, subSectionEntity.SectionCaption);
                            targetSectionEntity.AddChild(targetSubSectionEntity);
                        }
                    }
                }
            }
            RefreshTree(tvTree, tbTreeFilter.Text);
        }

        private void viewModel_InsertEditorControl(object sender, Control control)
        {
            MainViewModel vm = (MainViewModel)sender;
            var tab = _viewModels[vm];
            tab.Controls.Clear();
            tab.Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            foreach (var viewModel in _viewModels.Keys)
                viewModel.ViewLoaded(this);
        }

        private void UpdateStatus(string lastStatus, string parsedState, string lexedState)
        {
            if (InvokeRequired)
                this.Invoke(new Action(() => UpdateStatus(lastStatus, parsedState, lexedState)));
            else
            {
                lastStatusLabel.Text = lastStatus;
                lastParsedLabel.Text = parsedState;
                lastLexedLabel.Text = lexedState;
            }
        }

        private void UpdateFileState(MainViewModel vm, IFileHandler fileHandler)
        {
            if (InvokeRequired)
                this.Invoke(new Action(() => UpdateFileState(vm, fileHandler)));
            else
            {
                TabPage tabPage = _viewModels[vm];
                tabPage.Text = vm.WindowTitle;
                toolButtonFileSave.Enabled = fileHandler.IsChanged;
                menuItemFileSave.Enabled = fileHandler.IsChanged;
            }
        }

        private void UpdateLog(IEnumerable<LogItemModel> items)
        {
            if (InvokeRequired)
                this.Invoke(new Action(() => UpdateLog(items)));
            else
            {
                lvLog.BeginUpdate();
                lvLog.Items.Clear();
                foreach (var item in items)
                {
                    ListViewItem listItem = new ListViewItem();
                    listItem.ImageIndex = (int)item.Icon;
                    listItem.Text = item.Name;
                    listItem.SubItems.Add(item.Source);
                    lvLog.Items.Add(listItem);
                }
                lvLog.EndUpdate();
            }
        }

        private void tvTree_DoubleClick(object sender, EventArgs e)
        {
            if (tvTree.SelectedNode != null)
            {
                TreeNode treeNode = tvTree.SelectedNode;
                Entity entity = (Entity)treeNode.Tag;
                ActiveViewModel.GoToPositionCommand.Execute(entity.LineInfo.Start);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool canClose = ActiveViewModel.CanClose();
            if (!canClose)
                e.Cancel = true;
        }

        private void menuItemHelpAbout_Click(object sender, EventArgs e)
        {
            AboutForm form = new AboutForm();
            form.ShowDialog(this);
        }

        private void searchSymbolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SymbolSearchForm form = new SymbolSearchForm(ActiveViewModel);
            form.ShowDialog(this);
        }

        private void btnClearTreeFilter_Click(object sender, EventArgs e)
        {
            tbTreeFilter.Text = string.Empty;
        }

        private void tbTreeFilter_TextChanged(object sender, EventArgs e)
        {
            RefreshTree(tvTree, tbTreeFilter.Text);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ActiveViewModel.ViewClosed(this);
        }

        private void menuItemFileClearRecentFiles_Click(object sender, EventArgs e)
        {
            ActiveViewModel.Configuration.ClearRecentFiles();
        }

        private void tabControlFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlFiles.SelectedIndex > -1)
            {
                TabPage tab = tabControlFiles.TabPages[tabControlFiles.SelectedIndex];
                ActiveViewModel = tab.Tag as MainViewModel;
            }
            else
                ActiveViewModel = null;
        }
    }
}
