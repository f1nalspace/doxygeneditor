using DoxygenEditor.ViewModels;
using System;
using System.Windows.Forms;

namespace DoxygenEditor.Controls
{
    public partial class SearchReplaceControl : UserControl, ISearchReplaceControl
    {
        private int _initHeight;
        public SearchReplaceViewModel ViewModel { get; }

        public SearchReplaceControl()
        {
            InitializeComponent();
            _initHeight = Height;

            ViewModel = new SearchReplaceViewModel(this);

            cbSearchText.TextChanged += (s, e) =>
            {
                string text = ((ComboBox)s).Text;
                ViewModel.SearchText = text;
            };
            cbReplaceText.TextChanged += (s, e) =>
            {
                string text = ((ComboBox)s).Text;
                ViewModel.ReplaceText = text;
            };
            cbMatchCase.CheckedChanged += (s, e) =>
            {
                ViewModel.MatchCase = ((CheckBox)s).Checked;
            };
            cbMatchWords.CheckedChanged += (s, e) =>
            {
                ViewModel.WholeWord = ((CheckBox)s).Checked;
            };
            cbIsRegex.CheckedChanged += (s, e) =>
            {
                ViewModel.IsRegex = ((CheckBox)s).Checked;
            };
            cbWrap.CheckedChanged += (s, e) =>
            {
                ViewModel.Wrap = ((CheckBox)s).Checked;
            };
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
                        ViewModel.SearchExecutedCommand.Execute(SearchReplaceViewModel.SearchDirection.Next);
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
                        ViewModel.ReplaceExecutedCommand.Execute(SearchReplaceViewModel.ReplaceMode.Next);
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
            ViewModel.SearchExecutedCommand.Execute(SearchReplaceViewModel.SearchDirection.Next);
        }

        private void btnSearchPrev_Click(object sender, EventArgs e)
        {
            ViewModel.SearchExecutedCommand.Execute(SearchReplaceViewModel.SearchDirection.Prev);
        }

        private void btnReplaceNext_Click(object sender, EventArgs e)
        {
            ViewModel.ReplaceExecutedCommand.Execute(SearchReplaceViewModel.ReplaceMode.Next);
        }

        private void btnReplaceAll_Click(object sender, EventArgs e)
        {
            ViewModel.ReplaceExecutedCommand.Execute(SearchReplaceViewModel.ReplaceMode.All);
        }
    }
}
