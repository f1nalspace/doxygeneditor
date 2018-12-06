using DoxygenEditor.Editor;
using DoxygenEditor.MVVM;
using DoxygenEditor.Parser;
using DoxygenEditor.Parser.Entities;
using DoxygenEditor.Utils;
using DoxygenEditor.ViewModels;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace DoxygenEditor.Views
{
    public partial class MainForm : Form
    {
        private readonly MainViewModel _viewModel;

        public MainForm()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            _viewModel.InsertEditorControl += _viewModel_InsertEditorControl;
            _viewModel.UpdateTree += _viewModel_UpdateTree;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (ReflectionUtils.GetMemberName((MainViewModel v) => v.IsChanged).Equals(e.PropertyName) || ReflectionUtils.GetMemberName((MainViewModel v) => v.FilePath).Equals(e.PropertyName))
                    UpdateFileState(_viewModel.FileHandler);
                else if (ReflectionUtils.GetMemberName((MainViewModel v) => v.LastStatus).Equals(e.PropertyName) || ReflectionUtils.GetMemberName((MainViewModel v) => v.LastParsedState).Equals(e.PropertyName) || ReflectionUtils.GetMemberName((MainViewModel v) => v.LastLexedState).Equals(e.PropertyName))
                    UpdateStatus(_viewModel.LastStatus, _viewModel.LastParsedState, _viewModel.LastLexedState);
            };

            // File menu
            CommandUtils.BindClickCommand(menuItemFileNew, _viewModel.NewFileCommand);
            CommandUtils.BindClickCommand(menuItemFileOpen, _viewModel.OpenFileCommand);
            CommandUtils.BindClickCommand(menuItemFileSave, _viewModel.SaveFileCommand);
            CommandUtils.BindClickCommand(menuItemFileSaveAs, _viewModel.SaveFileAsCommand);

            // Edit menu
            CommandUtils.BindClickCommand(menuItemEditUndo, _viewModel.UndoCommand);
            CommandUtils.BindClickCommand(menuItemEditRedo, _viewModel.RedoCommand);

            CommandUtils.BindClickCommand(menuItemEditCut, _viewModel.CutCommand);
            CommandUtils.BindClickCommand(menuItemEditCopy, _viewModel.CopyCommand);
            CommandUtils.BindClickCommand(menuItemEditPaste, _viewModel.PasteCommand);

            CommandUtils.BindClickCommand(menuItemEditSelectAll, _viewModel.SelectAllCommand);

            CommandUtils.BindClickCommand(menuItemEditFindReplaceQuickSearch, _viewModel.QuickSearchCommand);
            CommandUtils.BindClickCommand(menuItemEditFindReplaceQuickReplace, _viewModel.QuickReplaceCommand);

            CommandUtils.BindClickCommand(menuItemEditUndo, _viewModel.UndoCommand);
            CommandUtils.BindClickCommand(menuItemEditRedo, _viewModel.RedoCommand);

            // View menu
            CommandUtils.BindCheckCommand(menuItemViewShowWhitespaces, _viewModel.ViewWhitespacesCommand);

            // Tool buttons
            CommandUtils.BindClickCommand(toolButtonFileNew, _viewModel.NewFileCommand);
            CommandUtils.BindClickCommand(toolButtonFileOpen, _viewModel.OpenFileCommand);
            CommandUtils.BindClickCommand(toolButtonFileSave, _viewModel.SaveFileCommand);
            CommandUtils.BindClickCommand(toolButtonFileSaveAs, _viewModel.SaveFileAsCommand);
            CommandUtils.BindClickCommand(toolButtonEditUndo, _viewModel.UndoCommand);
            CommandUtils.BindClickCommand(toolButtonEditRedo, _viewModel.RedoCommand);
        }

        private void _viewModel_UpdateTree(object sender, ParseState parseState)
        {
            tvTree.BeginUpdate();
            tvTree.Nodes.Clear();
            if (parseState != null)
            {
                // Pages
                foreach (PageEntity pageEntity in parseState.RootEntity.Children.Where(p => typeof(PageEntity).IsInstanceOfType(p)))
                {
                    string pageCaption = !string.IsNullOrEmpty(pageEntity.PageCaption) ? pageEntity.PageCaption : pageEntity.PageId;
                    TreeNode pageNode = tvTree.Nodes.Add(pageEntity.PageId, pageCaption);
                    pageNode.Tag = pageEntity;

                    // Sections
                    foreach (SectionEntity sectionEntity in pageEntity.Children.Where(p => typeof(SectionEntity).Equals(p.GetType())))
                    {
                        string sectionCaption = !string.IsNullOrEmpty(sectionEntity.SectionCaption) ? sectionEntity.SectionCaption : sectionEntity.SectionId;
                        TreeNode sectionNode = pageNode.Nodes.Add(sectionEntity.SectionId, sectionCaption);
                        sectionNode.Tag = sectionEntity;

                        // Subsections
                        foreach (SubSectionEntity subSectionEntity in sectionEntity.Children.Where(p => typeof(SubSectionEntity).Equals(p.GetType())))
                        {
                            string subSectionCaption = !string.IsNullOrEmpty(subSectionEntity.SectionCaption) ? subSectionEntity.SectionCaption : subSectionEntity.SectionId;
                            TreeNode subSectionNode = sectionNode.Nodes.Add(subSectionEntity.SectionId, subSectionCaption);
                            subSectionNode.Tag = subSectionEntity;
                        }
                    }
                }
            }
            tvTree.EndUpdate();
        }

        private void _viewModel_InsertEditorControl(object sender, Control control)
        {
            splitContainer1.Panel2.Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            menuItemViewShowWhitespaces.PerformClick();
            _viewModel.ViewLoaded(this);
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

        private void UpdateFileState(IFileHandler fileHandler)
        {
            if (InvokeRequired)
                this.Invoke(new Action(() => UpdateFileState(fileHandler)));
            else
            {
                Text = _viewModel.WindowTitle;
                toolButtonFileSave.Enabled = fileHandler.IsChanged;
                menuItemFileSave.Enabled = fileHandler.IsChanged;
            }
        }

        private void tvTree_DoubleClick(object sender, EventArgs e)
        {
            if (tvTree.SelectedNode != null)
            {
                TreeNode treeNode = tvTree.SelectedNode;
                Entity entity = (Entity)treeNode.Tag;
                _viewModel.GoToPositionCommand.Execute(entity.LineInfo.Start);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool canClose = _viewModel.CanClose();
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
            SymbolSearchForm form = new SymbolSearchForm(_viewModel);
            form.ShowDialog(this);
        }
    }
}
