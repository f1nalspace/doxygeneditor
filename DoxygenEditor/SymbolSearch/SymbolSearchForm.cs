using DoxygenEditor.Models;
using DoxygenEditor.Parsers.Entities;
using DoxygenEditor.SearchReplace;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DoxygenEditor.SymbolSearch
{
    public partial class SymbolSearchForm : Form
    {
        private readonly Timer _delayedTextChangeTimer;

        public string SearchText
        {
            get;
            private set;
        }

        public Type SearchType
        {
            get;
            private set;
        }

        private readonly IEnumerable<SymbolItemModel> _allItems;

        public SymbolItemModel SelectedItem
        {
            get;
            private set;
        }

        public SymbolSearchForm(IEnumerable<SymbolItemModel> allItems, IEnumerable<Type> allSearchTypes)
        {
            InitializeComponent();
            _allItems = allItems;
            DialogResult = DialogResult.Cancel;

            _delayedTextChangeTimer = new Timer() { Enabled = true, Interval = 500 };
            _delayedTextChangeTimer.Tick += (s, e) =>
            {
                _delayedTextChangeTimer.Stop();
                RefreshSearchResults(_allItems);
            };

            // @NOTE(final): Disable text and selected index changed event, so that we can call RefreshSearchResults initially - without triggering the timer
            textBoxSearch.TextChanged -= textBoxSearch_TextChanged;
            comboBoxSearchType.SelectedIndexChanged -= comboBoxSearchType_SelectedIndexChanged;

            textBoxSearch.Text = "";

            comboBoxSearchType.BeginUpdate();
            comboBoxSearchType.Items.Clear();
            comboBoxSearchType.Items.Add(new TypeStringModel(null));
            foreach (var t in allSearchTypes)
                comboBoxSearchType.Items.Add(new TypeStringModel(t));
            comboBoxSearchType.EndUpdate();
            comboBoxSearchType.SelectedIndex = 0;

            RefreshSearchResults(allItems);

            // Re-enable text and seleced index changed event
            textBoxSearch.TextChanged += textBoxSearch_TextChanged;
            comboBoxSearchType.SelectedIndexChanged += comboBoxSearchType_SelectedIndexChanged;
        }

        private void RefreshSearchResults(IEnumerable<SymbolItemModel> items)
        {
            ListViewItem selectedItem = null;
            listViewResults.BeginUpdate();
            listViewResults.Items.Clear();
            foreach (SymbolItemModel item in items)
            {
                if (SearchType != null)
                {
                    if (!item.TypeString.Equals(SearchType.Name))
                        continue;
                }

                if (!string.IsNullOrEmpty(SearchText))
                {
                    bool found = false;
                    if (item.Caption != null && item.Caption.IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) > -1)
                        found = true;
                    if (item.Id != null && item.Id.IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) > -1)
                        found = true;
                    if (!found)
                        continue;
                }

                ListViewItem listItem = new ListViewItem();
                listItem.Tag = item;
                listItem.Text = item.Id;
                listItem.SubItems.Add(item.Caption);
                listItem.SubItems.Add(item.TypeString);
                listViewResults.Items.Add(listItem);

                if (selectedItem == null)
                {
                    selectedItem = listItem;
                    selectedItem.Selected = true;
                }
            }
            listViewResults.EndUpdate();
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            SearchText = textBoxSearch.Text;
            _delayedTextChangeTimer.Stop();
            _delayedTextChangeTimer.Start();
        }

        private void comboBoxSearchType_SelectedIndexChanged(object sender, EventArgs e)
        {
            TypeStringModel v = (TypeStringModel)comboBoxSearchType.Items[comboBoxSearchType.SelectedIndex];
            SearchType = v.Type;
            _delayedTextChangeTimer.Stop();
            _delayedTextChangeTimer.Start();
        }

        private void JumpToSelectedItem()
        {
            if (SelectedItem != null)
            {
                DialogResult = DialogResult.OK;
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
            SelectedItem = selItem != null ? (SymbolItemModel)selItem.Tag : null;
        }

        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt || e.Shift || e.Control) return;
            if (e.KeyCode == Keys.Return)
                JumpToSelectedItem();
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
