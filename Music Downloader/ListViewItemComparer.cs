using System;
using System.Collections;
using System.Windows.Forms;

namespace Music_Downloader
{
    class ListViewItemComparer : IComparer
    {
        private int col;
        public int Compare(object x, object y)
        {
            int returnVal = -1;
            returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text,
             ((ListViewItem)y).SubItems[col].Text);
            return returnVal;
        }
    }
}
