using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TSP.DoxygenEditor.ErrorDialog
{
    public partial class ErrorDialogForm : Form
    {
        public string Title
        {
            get { return lblTitle.Text; }
            set { lblTitle.Text = value; }
        }
        public string ShortText
        {
            get { return lblMessage.Text; }
            set { lblMessage.Text = value; }
        }
        public string Details
        {
            get { return rtbDetails.Text; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    rtbDetails.Text = value;
                    panDetailsContainer.Show();
                    ShowDetails(false);
                }
                else
                    HideDetailsContainer();
            }
        }

        private int _fullHeight;
        private int _detailsContainerHeight;
        private int _detailsFullHeight;

        private void ShowDetails(bool show)
        {
            if (!show)
            {
                btnShowDetails.Image = TSP.DoxygenEditor.Properties.Resources.ExpandArrow_16x;
                panDetailsFull.Hide();
                Height = _fullHeight - _detailsFullHeight;
            }
            else
            {
                btnShowDetails.Image = TSP.DoxygenEditor.Properties.Resources.CollapseArrow_16x;
                panDetailsFull.Show();
                Height = _fullHeight;
            }
        }
        private void HideDetailsContainer()
        {
            panDetailsContainer.Hide();
            Height = _fullHeight - _detailsContainerHeight;
        }

        public ErrorDialogForm()
        {
            InitializeComponent();
            _fullHeight = Height;
            _detailsContainerHeight = panDetailsContainer.Height;
            _detailsFullHeight = panDetailsFull.Height;
            HideDetailsContainer();
        }

        private void btnShowDetails_Click(object sender, EventArgs e)
        {
            ShowDetails(!panDetailsFull.Visible);
        }

        private void btnCopyDetails_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(rtbDetails.Text))
            {
                Clipboard.Clear();
                Clipboard.SetText(rtbDetails.Text);
            }
        }
    }
}
