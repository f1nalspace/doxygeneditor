using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TSP.DoxygenEditor.FilterControls
{
    public partial class FilterBarControl : UserControl
    {
        public enum FilterBarMode
        {
            Hidden,
            Visible,
            Collapsed,
        }

        public enum FilterBarPosition
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
        }

        private FilterBarMode _activeMode = FilterBarMode.Collapsed;
        private FilterBarPosition _activePosition = FilterBarPosition.BottomRight;

        private int _maxHeight = 0;
        private int _maxWidth = 0;
        private int _minWidth = 0;
        private Control _viewControl;

        public delegate void ChangedFilterEventHandler(object sender, string newFilter);
        public event ChangedFilterEventHandler ChangedFilter;

        public FilterBarControl(Control viewControl)
        {
            InitializeComponent();

            Parent = viewControl.Parent;
            int prevIndex = Parent.Controls.IndexOf(viewControl);
            Parent.Controls.Add(this);
            Parent.Controls.SetChildIndex(this, prevIndex);
            viewControl.SizeChanged += (s, e) =>
            {
                RepositionBar();
            };
            _viewControl = viewControl;
            _maxHeight = panFilterEdit.Height;
            _maxWidth = Width;
            _minWidth = panFilterEdit.Padding.All * 2 + btnToggleVisibility.Width + btnClear.Width;
            Height = _maxHeight;
            SetBarMode(_activeMode);
        }

        private void RepositionBar()
        {
            switch (_activePosition)
            {
                case FilterBarPosition.TopLeft:
                    Left = _viewControl.Left + _viewControl.Location.X;
                    Top = _viewControl.Top + _viewControl.Location.Y;
                    break;
                case FilterBarPosition.TopRight:
                    Left = _viewControl.Right - Width - _viewControl.Location.X;
                    Top = _viewControl.Top + _viewControl.Location.Y;
                    break;
                case FilterBarPosition.BottomLeft:
                    Left = _viewControl.Left + _viewControl.Location.X;
                    Top = _viewControl.Height - Height - _viewControl.Location.Y;
                    break;
                case FilterBarPosition.BottomRight:
                    Left = _viewControl.Right - Width - _viewControl.Location.X;
                    Top = _viewControl.Bottom - Height - _viewControl.Location.Y;
                    break;
            }
        }

        public void SetBarMode(FilterBarMode mode)
        {
            switch (mode)
            {
                case FilterBarMode.Visible:
                    {
                        tbFilter.Show();
                        lblFilter.Show();
                        btnClear.ImageKey = !string.IsNullOrWhiteSpace(tbFilter.Text) ? "DeleteFilter_16x.png" : "FilterTextbox_16x.png";
                        btnToggleVisibility.ImageKey = "CollapseArrow_16x.png";
                        Width = _maxWidth;
                        Show();
                    }
                    break;

                case FilterBarMode.Collapsed:
                    {
                        tbFilter.Hide();
                        lblFilter.Hide();
                        btnClear.ImageKey = !string.IsNullOrWhiteSpace(tbFilter.Text) ? "DeleteFilter_16x.png" : "FilterTextbox_16x.png";
                        btnToggleVisibility.ImageKey = "ExpandArrow_16x.png";
                        Width = _minWidth;
                        Show();
                    }
                    break;
                case FilterBarMode.Hidden:
                    {
                        panFilterEdit.Hide();
                        lblFilter.Hide();
                        tbFilter.Hide();
                        Width = _minWidth;
                        Hide();
                    }
                    break;
            }
            RepositionBar();
            _activeMode = mode;
        }

        private void btnToggleVisibility_Click(object sender, EventArgs e)
        {
            if (_activeMode == FilterBarMode.Collapsed)
                SetBarMode(FilterBarMode.Visible);
            else if (_activeMode == FilterBarMode.Visible)
                SetBarMode(FilterBarMode.Collapsed);
        }

        private void tbFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                btnClear.ImageKey = !string.IsNullOrWhiteSpace(tbFilter.Text) ? "DeleteFilter_16x.png" : "FilterTextbox_16x.png";
                ChangedFilter?.Invoke(this, tbFilter.Text);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbFilter.Text))
            {
                tbFilter.Text = string.Empty;
                btnClear.ImageKey = "FilterTextbox_16x.png";
                ChangedFilter?.Invoke(this, string.Empty);
            }
        }

        public void SetFilter(string newFilter)
        {
            tbFilter.Text = newFilter;
            btnClear.ImageKey = !string.IsNullOrWhiteSpace(tbFilter.Text) ? "DeleteFilter_16x.png" : "FilterTextbox_16x.png";
        }
    }
}
