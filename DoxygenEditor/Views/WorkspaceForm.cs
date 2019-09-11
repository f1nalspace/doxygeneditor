using System;
using System.Drawing;
using System.Windows.Forms;
using TSP.DoxygenEditor.Models;

namespace TSP.DoxygenEditor.Views
{
    public partial class WorkspaceForm : Form
    {
        public WorkspaceModel Workspace { get; }

        public WorkspaceForm(WorkspaceModel model)
        {
            InitializeComponent();

            tcMain.Appearance = TabAppearance.FlatButtons;
            tcMain.ItemSize = new Size(0, 1);
            tcMain.SizeMode = TabSizeMode.Fixed;

            tvOptions.SelectedNode = tvOptions.Nodes[0];

            Workspace = new WorkspaceModel(string.Empty);
            if (model != null)
                Workspace.Assign(model);
            WorkspaceToVisual();
        }

        private void WorkspaceToVisual()
        {
            cbParserCppSkipFunctionBodies.Checked = Workspace.ParserCpp.ExcludeFunctionBodies;
            cbParserCppExcludeFunctionBodySymbols.Checked = Workspace.ParserCpp.ExcludeFunctionBodySymbols;
            cbParserCppExcludeFunctionCallSymbols.Checked = Workspace.ParserCpp.ExcludeFunctionCallSymbols;

            cbValidationCppExcludePreprocessorMatch.Checked = Workspace.ValidationCpp.ExcludePreprocessorMatch;
            cbValidationCppExcludePreprocessorUsage.Checked = Workspace.ValidationCpp.ExcludePreprocessorUsage;
            cbValidationCppRequireDoxygenReference.Checked = Workspace.ValidationCpp.RequireDoxygenReference;

            tbBuildSourcePath.Text = Workspace.Build.BaseDirectory;
            tbBuildDoxygenConfigFilePath.Text = Workspace.Build.ConfigFile;
            tbBuildDoxygenExecutablePath.Text = Workspace.Build.PathToDoxygen;
        }
        private void VisualToWorkspace()
        {
            Workspace.ParserCpp.ExcludeFunctionBodies = cbParserCppSkipFunctionBodies.Checked;
            Workspace.ParserCpp.ExcludeFunctionBodySymbols = cbParserCppExcludeFunctionBodySymbols.Checked;
            Workspace.ParserCpp.ExcludeFunctionCallSymbols = cbParserCppExcludeFunctionCallSymbols.Checked;

            Workspace.ValidationCpp.ExcludePreprocessorMatch = cbValidationCppExcludePreprocessorMatch.Checked;
            Workspace.ValidationCpp.ExcludePreprocessorUsage = cbValidationCppExcludePreprocessorUsage.Checked;
            Workspace.ValidationCpp.RequireDoxygenReference = cbValidationCppRequireDoxygenReference.Checked;

            Workspace.Build.BaseDirectory = tbBuildSourcePath.Text;
            Workspace.Build.ConfigFile = tbBuildDoxygenConfigFilePath.Text;
            Workspace.Build.PathToDoxygen = tbBuildDoxygenExecutablePath.Text;
        }

        private TabPage FindTabByOptionTag(string option)
        {
            foreach (TabPage tab in tcMain.TabPages)
            {
                string tabName = tab.Text;
                if (string.Equals(option, tabName))
                    return (tab);
            }
            return (null);
        }

        private void tvOptions_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Collapse && e.Action != TreeViewAction.Expand)
            {
                string option = e.Node.FullPath;
                lblOptionsTitle.Text = option.Replace("\\", " -> ");
                TabPage tab = FindTabByOptionTag(option);
                if (tab != null)
                    tcMain.SelectedTab = tab;
                else
                    tcMain.SelectedIndex = -1;
            }
        }

        private void tcMain_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void WorkspaceForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
                VisualToWorkspace();
        }

        private void btnSelectBuildDoxgenExecutable_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.SelectedPath = tbBuildDoxygenExecutablePath.Text;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    tbBuildDoxygenExecutablePath.Text = dlg.SelectedPath;
                }
            }
        }

        private void btnSelectBuildSourcePath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.SelectedPath = tbBuildSourcePath.Text;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    tbBuildSourcePath.Text = dlg.SelectedPath;
                }
            }
        }

        private void btnSelectBuildDoxygenConfigPath_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.FileName = tbBuildDoxygenConfigFilePath.Text;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    tbBuildDoxygenConfigFilePath.Text = dlg.FileName;
                }
            }
        }
    }
}
