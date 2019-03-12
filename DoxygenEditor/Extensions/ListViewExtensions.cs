using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TSP.DoxygenEditor.Extensions
{
    static class ListViewExtensions
    {
        public static void SelectItemOrIndex(this ListView listview, object tag, int index)
        {
            ListViewItem foundItem = null;
            if (tag != null)
            {
                foreach (ListViewItem item in listview.Items)
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
                if (listview.Items.Count > 0)
                    foundItem = listview.Items[Math.Min(index, listview.Items.Count - 1)];
            }
            if (foundItem != null)
                foundItem.Selected = true;
        }
    }
}
