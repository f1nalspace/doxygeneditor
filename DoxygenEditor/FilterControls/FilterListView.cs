using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using TSP.DoxygenEditor.Extensions;
using System;
using System.Collections;

namespace TSP.DoxygenEditor.FilterControls
{
    public partial class FilterListView : UserControl
    {
        private readonly ListView _listView;

        public int ItemCount => _listView.Items.Count;

        public int SelectedIndex => (_listView.Items.Count > 0 && _listView.SelectedIndices.Count > 0) ? _listView.SelectedIndices[0] : -1;
        public ListViewItem SelectedItem => (_listView.Items.Count > 0 && _listView.SelectedIndices.Count > 0) ? _listView.Items[_listView.SelectedIndices[0]] : null;

        public delegate void ItemDoubleClickEventHandler(object sender, ListViewItem item);
        public event ItemDoubleClickEventHandler ItemDoubleClick;

        public ImageList ImageList
        {
            set { _listView.SmallImageList = value; }
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
            private set
            {
                _filterColumn = value;
                BeginUpdate();
                RefreshItems();
                EndUpdate();
            }
        }
        public void SetFilterColumn(string columnName)
        {
            FilterColumn = GetColumnIndexByName(columnName);
        }

        private int GetColumnIndexByName(string name)
        {
            int idx = 0;
            foreach (ColumnHeader header in _listView.Columns)
            {
                if (string.Equals(header.Text, name))
                    return (idx);
                ++idx;
            }
            return (-1);
        }

        private int _groupColumn = -1;
        public int GroupColumn
        {
            get { return _groupColumn; }
            private set
            {
                _groupColumn = value;
                BeginUpdate();
                RefreshItems();
                EndUpdate();
            }
        }

        public void SetGroupColumn(string columnName)
        {
            GroupColumn = GetColumnIndexByName(columnName);
        }

        class ListviewItemSorter : IComparer
        {
            private readonly Comparison<ListViewItem> _comparer;

            public ListviewItemSorter(Comparison<ListViewItem> comparer)
            {
                _comparer = comparer;
            }
            public int Compare(object x, object y)
            {
                ListViewItem a = x as ListViewItem;
                ListViewItem b = y as ListViewItem;
                int result = _comparer(a, b);
                return (result);
            }
        }

        public Comparison<ListViewItem> Comparer
        {
            set { _listView.ListViewItemSorter = new ListviewItemSorter(value); }
        }

        private readonly List<ListViewItem> _items = new List<ListViewItem>();
        private string _filterText = null;

        public FilterListView() : base()
        {
            InitializeComponent();
            _listView = new ListView();
            _listView.Dock = DockStyle.Fill;
            _listView.View = View.Details;
            _listView.HideSelection = false;
            _listView.MultiSelect = false;
            _listView.FullRowSelect = true;
            _listView.VirtualMode = false;
            _listView.ShowGroups = true;
            _listView.DoubleClick += (s, e) =>
            {
                if (_listView.Items.Count > 0 && _listView.SelectedIndices.Count > 0)
                    ItemDoubleClick?.Invoke(this, _listView.Items[_listView.SelectedIndices[0]]);
            };
            Controls.Add(_listView);
        }

        public void ClearItems()
        {
            _items.Clear();
            _listView.Items.Clear();
            _listView.Groups.Clear();
        }

        public void AddItem(ListViewItem item)
        {
            _items.Add(item);
        }

        private bool MatchWildcard(string text, string filter, bool ignoreCase)
        {
            string regex = "^" + Regex.Escape(filter).Replace("\\?", ".").Replace("\\*", ".*") + "$";
            bool result = Regex.IsMatch(text, regex, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            return (result);
        }

        public void RefreshItems()
        {
            string filterText = _filterText;
            int filterCol = _filterColumn;
            int groupCol = _groupColumn;
            int colCount = _listView.Columns.Count;
            Dictionary<string, ListViewGroup> groupsMap = new Dictionary<string, ListViewGroup>();
            _listView.Items.Clear();
            _listView.Groups.Clear();
            foreach (ListViewItem item in _items)
            {
                bool canAdd = false;
                item.Group = null;
                if (string.IsNullOrWhiteSpace(filterText))
                    canAdd = true;
                else
                {
                    for (int colIndex = 0; colIndex < colCount; ++colIndex)
                    {
                        if (filterCol > -1 && filterCol != colIndex)
                            continue;
                        bool matches = MatchWildcard(item.SubItems[colIndex].Text, filterText, true);
                        canAdd |= matches;
                        if (canAdd)
                            break;
                    }
                }
                if (canAdd)
                {
                    if (groupCol > -1 && groupCol < colCount)
                    {
                        string groupValue = item.SubItems[groupCol].Text;
                        if (!string.IsNullOrWhiteSpace(groupValue))
                        {
                            if (!groupsMap.ContainsKey(groupValue))
                            {
                                ListViewGroup group = new ListViewGroup(groupValue);
                                groupsMap.Add(groupValue, group);
                                _listView.Groups.Add(group);
                            }
                            item.Group = groupsMap[groupValue];
                        }
                    }
                    _listView.Items.Add(item);
                }
            }
            if (_listView.ListViewItemSorter != null)
                _listView.Sort();
            _listView.AutoSizeColumnList();
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
            ListViewItem foundItem = null;
            if (tag != null)
            {
                foreach (ListViewItem item in _listView.Items)
                {
                    if (item.Tag == tag)
                    {
                        foundItem = item;
                        break;
                    }
                }
            }
            if (foundItem == null && index > -1)
            {
                if (_listView.Items.Count > 0)
                    foundItem = _listView.Items[Math.Min(index, _listView.Items.Count - 1)];
            }
            if (foundItem != null)
                foundItem.Selected = true;
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
