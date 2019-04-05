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
        }
    }
}
