using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            Workspace = new WorkspaceModel(string.Empty);
            if (model != null)
                Workspace.Overwrite(model);

            lbIncludeDirs.BeginUpdate();
            lbIncludeDirs.Items.Clear();
            foreach (var includePath in Workspace.IncludeDirectories)
                lbIncludeDirs.Items.Add(includePath);
            lbIncludeDirs.EndUpdate();

            tbIncludeFilter.Text = Workspace.IncludeFilter;
        }

        private void tsmiAddIncludeDir_Click(object sender, EventArgs e)
        {
            DirectoryInputForm dialog = new DirectoryInputForm(string.Empty);
            dialog.HasPath += (s, p) => lbIncludeDirs.Items.Contains(p);
            DialogResult r = dialog.ShowDialog(this);
            if (r == DialogResult.OK)
                lbIncludeDirs.Items.Add(dialog.Path);
        }

        private void cmsIncludeDirs_Opening(object sender, CancelEventArgs e)
        {
            bool hasSelected = lbIncludeDirs.Items.Count > 0 && lbIncludeDirs.SelectedIndex > -1; ;
            tsmiEditIncludeDir.Enabled = hasSelected;
            tsmiRemoveIncludeDir.Enabled = hasSelected;
        }

        private void tsmiEditIncludeDir_Click(object sender, EventArgs e)
        {
            int idx = lbIncludeDirs.SelectedIndex;
            string path = (string)lbIncludeDirs.Items[idx];
            DirectoryInputForm dialog = new DirectoryInputForm(path);
            dialog.HasPath += (s, p) => lbIncludeDirs.Items.Contains(p);
            DialogResult r = dialog.ShowDialog(this);
            if (r == DialogResult.OK)
                lbIncludeDirs.Items[idx] = dialog.Path;
        }

        private void tsmiRemoveIncludeDir_Click(object sender, EventArgs e)
        {
            int idx = lbIncludeDirs.SelectedIndex;
            lbIncludeDirs.Items.RemoveAt(idx);
        }

        private void WorkspaceForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Workspace.IncludeDirectories.Clear();
            foreach (string item in lbIncludeDirs.Items)
                Workspace.IncludeDirectories.Add(item);
            Workspace.IncludeFilter = tbIncludeFilter.Text;
        }
    }
}
