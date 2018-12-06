using DoxygenEditor.Models;
using DoxygenEditor.ViewModels;
using System;
using System.Windows.Forms;

namespace DoxygenEditor.Views
{
    public partial class SymbolSearchForm : Form
    {
        private readonly Timer _delayedTextChangeTimer;

        private readonly SymbolSearchViewModel _viewModel;

        public SymbolSearchForm(MainViewModel mainViewModel)
        {
            InitializeComponent();

            _viewModel = new SymbolSearchViewModel(mainViewModel);
            _viewModel.UpdatedSearchResults += (s, items) =>
            {
                listViewResults.BeginUpdate();
                listViewResults.Items.Clear();
                foreach (var item in items)
                {
                    ListViewItem listItem = new ListViewItem();
                    listItem.Tag = item;
                    listItem.Text = item.Id;
                    listItem.SubItems.Add(item.Caption);
                    listItem.SubItems.Add(item.TypeString);
                    listViewResults.Items.Add(listItem);
                }
                listViewResults.EndUpdate();
            };

            _delayedTextChangeTimer = new Timer() { Enabled = true, Interval = 500 };
            _delayedTextChangeTimer.Tick += (s, e) =>
            {
                _delayedTextChangeTimer.Stop();
                _viewModel.SearchCommand.Execute(null);
            };

            comboBoxSearchType.BeginUpdate();
            comboBoxSearchType.Items.Clear();
            comboBoxSearchType.Items.Add(new TypeStringModel(null));
            foreach (var t in _viewModel.SearchTypes)
                comboBoxSearchType.Items.Add(new TypeStringModel(t));
            comboBoxSearchType.EndUpdate();

            textBoxSearch.Text = "";
            comboBoxSearchType.SelectedIndex = 0;
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            _viewModel.SearchText = textBoxSearch.Text;
            _delayedTextChangeTimer.Stop();
            _delayedTextChangeTimer.Start();
        }

        private void comboBoxSearchType_SelectedIndexChanged(object sender, EventArgs e)
        {
            TypeStringModel v = (TypeStringModel)comboBoxSearchType.Items[comboBoxSearchType.SelectedIndex];
            _viewModel.SearchType = v.Type;
            _delayedTextChangeTimer.Stop();
            _delayedTextChangeTimer.Start();
        }

        private void JumpToSelectedItem()
        {
            var sel = _viewModel.SelectedResultItem;
            if (sel != null)
            {
                _viewModel.JumpToCommand.Execute(sel);
                Close();
            }
        }

        private void listViewResults_DoubleClick(object sender, EventArgs e)
        {
            JumpToSelectedItem();
        }

        private void listViewResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewItem selItem = listViewResults.SelectedItems.Count > 0 ? listViewResults.SelectedItems[0] : null;
            if (selItem != null)
                _viewModel.SelectedResultItem = (SymbolSearchResultItemViewModel)selItem.Tag;
            else
                _viewModel.SelectedResultItem = null;
        }

        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt || e.Shift || e.Control) return;
            if (e.KeyCode == Keys.Return)
            {
                ListViewItem selItem = listViewResults.Items.Count > 0 ? listViewResults.Items[0] : null;
                if (selItem != null)
                    _viewModel.SelectedResultItem = (SymbolSearchResultItemViewModel)selItem.Tag;
                else
                    _viewModel.SelectedResultItem = null;
                JumpToSelectedItem();
            }
            else if (e.KeyCode == Keys.Escape)
                Close();
        }

        private void listViewResults_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt || e.Shift || e.Control) return;
            if (e.KeyCode == Keys.Return)
                JumpToSelectedItem();
            else if (e.KeyCode == Keys.Escape)
                Close();
        }
    }
}
