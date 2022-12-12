using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace TSP.DoxygenEditor.Extensions
{
    static class UIExtensions
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        private const int WM_SETREDRAW = 0x0b;

        public static void BeginUpdate(this RichTextBox rtb)
        {
            SendMessage(rtb.Handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
        }

        public static void EndUpdate(this RichTextBox rtb)
        {
            SendMessage(rtb.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            rtb.Invalidate();
        }

        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }

        public static void AutoSizeColumnList(this ListView listView)
        {
            listView.BeginUpdate();

            int[] columnSize = new int[32];
            int columnCount = listView.Columns.Count;
            Debug.Assert(columnCount < columnSize.Length);

            // Auto size using header
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            // Grab column size based on header
            for (int i = 0; i < columnCount; ++i)
                columnSize[i] = listView.Columns[i].Width;

            // Auto size using data
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            // Grab comumn size based on data and set max width
            for (int i = 0; i < columnCount; ++i)
            {
                ColumnHeader colHeader = listView.Columns[i];
                colHeader.Width = Math.Max(columnSize[i], colHeader.Width);
            }

            listView.EndUpdate();
        }
    }
}
