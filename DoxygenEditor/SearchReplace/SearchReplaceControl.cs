using System;
using System.Windows.Forms;

namespace TSP.DoxygenEditor.SearchReplace
{
    public partial class SearchReplaceControl : UserControl
    {
        private int _initHeight;

        public string SearchText
        {
            get { return cbSearchText.Text; }
            set { cbSearchText.Text = value; }
        }

        public string ReplaceText
        {
            get { return cbReplaceText.Text; }
            set { cbReplaceText.Text = value; }
        }

        public bool MatchCase
        {
            get { return cbMatchCase.Checked; }
            set { cbMatchCase.Checked = value; }
        }

        public bool MatchWords
        {
            get { return cbMatchWords.Checked; }
            set { cbMatchWords.Checked = value; }
        }

        public bool IsRegex
        {
            get { return cbIsRegex.Checked; }
            set { cbIsRegex.Checked = value; }
        }

        public bool IsWrap
        {
            get { return cbWrap.Checked; }
            set { cbWrap.Checked = value; }
        }

        public event ExecuteSearchEventHandler Search;
        public event ExecuteReplaceEventHandler Replace;

        public SearchReplaceControl()
        {
            InitializeComponent();
            _initHeight = Height;
        }

        private void btnToggleReplace_Click(object sender, EventArgs e)
        {
            if (panReplace.Visible)
                HideReplaceOnly();
            else
                ShowSearchAndReplace(false);
        }

        private void tbSearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Control && !e.Alt && !e.Shift)
            {
                if (e.KeyCode == Keys.Escape)
                    Hide();
                else if (e.KeyCode == Keys.Return)
                {
                    if (!string.IsNullOrEmpty(cbSearchText.Text))
                        Search?.Invoke(this, SearchDirection.Next);
                }
            }
        }

        private void tbReplaceText_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Control && !e.Alt && !e.Shift)
            {
                if (e.KeyCode == Keys.Escape)
                    Hide();
                else if (e.KeyCode == Keys.Return)
                {
                    if (!string.IsNullOrEmpty(cbSearchText.Text))
                        Replace?.Invoke(this, ReplaceMode.Next);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Hide();
        }

        public void ShowSearchOnly(bool focus)
        {
            panReplace.Hide();
            Height = _initHeight - panReplace.Height;
            if (!Visible)
                Show();
            if (focus)
                cbSearchText.Focus();
        }

        public void ShowSearchAndReplace(bool focus)
        {
            panReplace.Show();
            Height = _initHeight;
            if (!Visible)
                Show();
            if (focus)
                cbSearchText.Focus();
        }

        public void HideSearchReplace()
        {
            Hide();
        }

        public void HideReplaceOnly()
        {
            panReplace.Hide();
            Height = _initHeight - panReplace.Height;
        }

        public bool IsShown()
        {
            bool result = Visible;
            return (result);
        }

        private void btnSearchNext_Click(object sender, EventArgs e)
        {
            Search?.Invoke(this, SearchDirection.Next);
        }

        private void btnSearchPrev_Click(object sender, EventArgs e)
        {
            Search?.Invoke(this, SearchDirection.Prev);
        }

        private void btnReplaceNext_Click(object sender, EventArgs e)
        {
            Replace?.Invoke(this, ReplaceMode.Next);
        }

        private void btnReplaceAll_Click(object sender, EventArgs e)
        {
            Replace?.Invoke(this, ReplaceMode.All);
        }
    }
}
