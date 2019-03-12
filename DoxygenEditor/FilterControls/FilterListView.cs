using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using TSP.DoxygenEditor.Extensions;
using System;

namespace TSP.DoxygenEditor.FilterControls
{
    public partial class FilterListView : UserControl
    {
        private readonly ListView _listView;

        public int ItemCount => _listView.Items.Count;
        public int SelectedIndex => (_listView.VirtualListSize > 0 && _listView.SelectedIndices.Count > 0) ? _listView.SelectedIndices[0] : -1;
        public ListViewItem SelectedItem => (_listView.VirtualListSize > 0 && _listView.SelectedIndices.Count > 0) ? _filteredItems[_listView.SelectedIndices[0]] : null;
        public delegate void ItemDoubleClickEventHandler(object sender, ListViewItem item);
        public event ItemDoubleClickEventHandler ItemDoubleClick;
        public ImageList ImageList
        {
            get { return _listView.SmallImageList; }
            set { _listView.SmallImageList = value; }
        }

        private readonly List<ListViewItem> _items = new List<ListViewItem>();
        private string _filterText = null;

        private readonly List<ListViewItem> _filteredItems = new List<ListViewItem>();

        private void RebuildFilteredItems()
        {
            _filteredItems.Clear();
            string filterText = _filterText;
            int filterCol = _filterColumn;
            foreach (var item in _items)
            {
                bool canAdd = false;
                if (string.IsNullOrWhiteSpace(filterText))
                    canAdd = true;
                else
                {
                    for (int colIndex = 0; colIndex < _listView.Columns.Count; ++colIndex)
                    {
                        if (filterCol > -1 && filterCol != colIndex)
                            continue;
                        bool matches = MatchWildcard(item.SubItems[colIndex].Text, filterText);
                        canAdd |= matches;
                        if (canAdd)
                            break;
                    }
                }
                if (canAdd)
                    _filteredItems.Add(item);
            }
        }

        public string FilterText
        {
            get { return _filterText; }
            set
            {
                _filterText = value;
                BeginUpdate();
                RefreshItems();
                EndUpdate();
            }
        }
        private int _filterColumn = -1;
        public int FilterColumn
        {
            get { return _filterColumn; }
            set
            {
                _filterColumn = value;
                BeginUpdate();
                RefreshItems();
                EndUpdate();
            }
        }

        public FilterListView() : base()
        {
            InitializeComponent();
            _listView = new ListView();
            _listView.Dock = DockStyle.Fill;
            _listView.View = View.Details;
            _listView.HideSelection = false;
            _listView.MultiSelect = false;
            _listView.FullRowSelect = true;
            _listView.VirtualMode = true;
            _listView.DoubleClick += (s, e) =>
            {
                if (_listView.VirtualListSize > 0 && _listView.SelectedIndices.Count > 0)
                    ItemDoubleClick?.Invoke(this, _filteredItems[_listView.SelectedIndices[0]]);
            };
            _listView.RetrieveVirtualItem += (s, e) =>
            {
                e.Item = _filteredItems[e.ItemIndex];
            };
            Controls.Add(_listView);
        }

        public void ClearItems()
        {
            _items.Clear();
            _filteredItems.Clear();
            _listView.VirtualListSize = 0;
        }

        public void AddItem(ListViewItem item)
        {
            _items.Add(item);
        }

        private bool MatchWildcard(string text, string filter)
        {
            string regex = "^" + Regex.Escape(filter).Replace("\\?", ".").Replace("\\*", ".*") + "$";
            bool result = Regex.IsMatch(text, regex);
            return (result);
        }

        public void RefreshItems()
        {
            RebuildFilteredItems();
            _listView.VirtualListSize = _filteredItems.Count;
        }

        public void BeginUpdate()
        {
            _listView.BeginUpdate();
        }
        public void EndUpdate()
        {
            _listView.EndUpdate();
        }

        public void SelectItemOrIndex(object tag, int index)
        {
            _listView.SelectItemOrIndex(tag, index);
        }

        public void ClearSelection()
        {
            _listView.SelectedIndices.Clear();
        }

        public void AddColumn(string name, int width)
        {
            _listView.Columns.Add(name, width);
        }
    }
}
