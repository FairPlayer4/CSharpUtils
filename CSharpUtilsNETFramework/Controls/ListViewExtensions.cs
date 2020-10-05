#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CSharpUtilsNETStandard.Utils.Extensions.General;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework.Controls
{
    [PublicAPI]
    public enum ColumnContentPriority { Low, Medium, High }

    [PublicAPI]
    public static class ListViewExtensions
    {
        public static int GetEmptyListViewHeight([NotNull] this ListView listView)
        {
            int difference = listView.GetOptimalListViewHeightDifference();
            return difference > 0 ? difference : 0;
        }

        public static int GetOptimalListViewHeightDifference([NotNull] this ListView listView)
        {
            int listViewHeight = listView.Height;
            int totalRowsHeight = 0;
            int tallestRow = 0;
            for (int i = 0; i < listView.Items.Count; i++)
            {
                int rowHeight = listView.Items[i].Bounds.Height;
                totalRowsHeight += rowHeight;
                if (rowHeight > tallestRow) tallestRow = rowHeight;
            }
            //TODO test this with groups
            //for (int i = 0; i < listView.Groups.Count; i++)
            //{
            //    ListViewGroup group = listView.Groups[i];
            //    for (int j = 0; j < group.Items.Count; j++)
            //    {
            //        int rowHeight = group.Items[j].Bounds.Height;
            //        totalRowsHeight += rowHeight;
            //        if (rowHeight > tallestRow) tallestRow = rowHeight;
            //    }
            //}

            //Extra Space for Headers
            totalRowsHeight += tallestRow * (listView.Groups.Count + 3);
            return listViewHeight - totalRowsHeight;
        }

        /// <summary>
        /// Allows easier access to columns by converting the to a list.
        /// </summary>
        [NotNull, ItemNotNull]
        public static IEnumerable<ColumnHeader> GetColumnHeaders([NotNull] this ListView listView) => listView.Columns.Cast<ColumnHeader>();

        [NotNull, ItemNotNull]
        public static IEnumerable<ListViewItem> GetListViewItems([NotNull] this ListView listView) => listView.Items.Cast<ListViewItem>();

        [NotNull, ItemNotNull]
        public static IEnumerable<ListViewItem> GetSelectedItems([NotNull] this ListView listView) => listView.SelectedItems.Cast<ListViewItem>();

        [NotNull, ItemNotNull]
        public static IEnumerable<ListViewItem> GetCheckedItems([NotNull] this ListView listView) => listView.CheckedItems.Cast<ListViewItem>();

        [CanBeNull]
        public static ListViewItem GetItemFromCursorPosition([NotNull] this ListView listView)
        {
            // translate the mouse position from screen coordinates to
            // client coordinates within the given ListView
            Point localPoint = listView.PointToClient(Cursor.Position);
            return listView.GetItemAt(localPoint.X, localPoint.Y);
        }

        // Scales ListView Columns so that they are not to small on high resolution screens
        public static void ScaleListViewColumns([NotNull] this ListView listview, SizeF factor)
        {
            foreach (ColumnHeader column in listview.Columns)
                column.Width = (int)Math.Round(column.Width * factor.Width);
        }

        // Required margin so the column text is displayed fully. Seems to work with different scaling factors (tested 100% and 200%)
        private const int ColumnHeaderTextMargin = 5;
        private const int CheckBoxColumnMinimumSize = 24;
        private const int ColumnWidthMarginForListViewWidth = 7;

        public static void AutoSizeColumns([NotNull] this ListView listView, bool fitInView = true, [CanBeNull] IReadOnlyList<int> manualColumnWidths = null, [CanBeNull] IReadOnlyList<ColumnContentPriority> columnPriorities = null)
        {
            ListView.ColumnHeaderCollection listViewColumns = listView.Columns;
            int columnCount = listViewColumns.Count;
            if (columnCount == 0) return;
            int[] minimumColumnWidths = new int[columnCount];
            int[] optimalColumnWidths = new int[columnCount];
            ColumnHeader[] columnHeaders = new ColumnHeader[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                ColumnHeader columnHeader = listViewColumns[i];
                columnHeaders[i] = columnHeader;
                int textWidth = TextRenderer.MeasureText(columnHeader.Text, listView.Font).Width;
                minimumColumnWidths[i] = textWidth + ColumnHeaderTextMargin;
                columnHeader.Width = -2; // This resizes the column to the content and also makes sure that the column header text is displayed.
                optimalColumnWidths[i] = columnHeader.Width;
            }
            if (listView.CheckBoxes && columnCount > 0)
            {
                minimumColumnWidths[0] = CheckBoxColumnMinimumSize;
            }
            if (manualColumnWidths != null)
            {
                for (int i = 0; i < manualColumnWidths.Count && i < columnCount; i++)
                {
                    int manualColumnWidth = manualColumnWidths[i];
                    if (manualColumnWidth < 0) continue;
                    minimumColumnWidths[i] = manualColumnWidth;
                    if (optimalColumnWidths[i] == manualColumnWidth) continue;
                    optimalColumnWidths[i] = manualColumnWidth;
                    columnHeaders[i].Width = manualColumnWidth;
                }
            }

            float scalingFactorYAxis = listView.GetScalingFactorYAxis();
            int marginListViewWidth = (listView.Width - columnCount * ColumnWidthMarginForListViewWidth * scalingFactorYAxis).ToIntSafeBounded();
            int totalOptimalColumnWidth = optimalColumnWidths.Sum();
            int difference = totalOptimalColumnWidth - marginListViewWidth;
            if (!fitInView || difference <= 0) return;
            ColumnContentPriority[] adaptedColumnPriorities = new ColumnContentPriority[columnCount];
            for (int i = 0; i < columnCount; i++) adaptedColumnPriorities[i] = ColumnContentPriority.Medium;
            if (columnPriorities != null)
                for (int i = 0; i < columnPriorities.Count && i < columnCount; i++)
                    adaptedColumnPriorities[i] = columnPriorities[i];
            int[] shares = FairShare(marginListViewWidth, optimalColumnWidths, minimumColumnWidths, adaptedColumnPriorities);
            for (int i = 0; i < columnCount; i++)
            {
                if (shares[i] == optimalColumnWidths[i]) continue;
                columnHeaders[i].Width = shares[i];
            }
        }

        // based on https://www.ece.rutgers.edu/~marsic/Teaching/CCN/minmax-fairsh.html
        // Weights are calculated based on demand so a high demand will produce a low weight
        // Prioritized columns get the highest weight
        // If any index is below minimum space then the demand is reduced to the minimum space (yielding a high weight) and the algorithm is rerun
        private static int[] FairShare(float availableWidth, [NotNull] int[] optimalWidths, [NotNull] int[] minimumWidths, [NotNull] ColumnContentPriority[] columnContentPriorities, int recursionCounter = 0)
        {
            int arrayLength = optimalWidths.Length;
            int minimumWidthsSum = minimumWidths.Sum();
            if (availableWidth < minimumWidthsSum)
            {
                int minimumWidthsSumOfHighPriorityColumns = minimumWidths.Where((width, i) => columnContentPriorities[i] == ColumnContentPriority.High).Sum();
                float factor;
                if (availableWidth >= minimumWidthsSumOfHighPriorityColumns) //The other alternative is unrealistic and will cause default behaviour
                {
                    float availableWidthWithoutHighPriorityColumns = availableWidth - minimumWidthsSumOfHighPriorityColumns;
                    int minimumWidthsSumWithoutHighPriorityColumns = minimumWidths.Where((x, i) => columnContentPriorities[i] != ColumnContentPriority.High).Sum();
                    factor = availableWidthWithoutHighPriorityColumns / minimumWidthsSumWithoutHighPriorityColumns;
                    return minimumWidths.Select((width, i) => columnContentPriorities[i] == ColumnContentPriority.High ? width : (width * factor).ToIntSafeBounded()).ToArray();
                }
                factor = availableWidth / minimumWidthsSum;
                return minimumWidths.Select(width => (width * factor).ToIntSafeBounded()).ToArray();
            }

            float[] initialWeights = optimalWidths.Select(x => availableWidth / x).ToArray();
            float heighestWeight = initialWeights.Max();
            float lowestWeight = initialWeights.Min();
            for (int i = 0; i < arrayLength; i++)
            {
                switch (columnContentPriorities[i])
                {
                    case ColumnContentPriority.Low:
                        initialWeights[i] = lowestWeight;
                        break;
                    case ColumnContentPriority.High:
                        initialWeights[i] = heighestWeight;
                        break;
                }
            }
            float[] normalizedWeights = initialWeights.Select(x => x / lowestWeight).ToArray();
            float normalizedWeightsSum = normalizedWeights.Sum();
            float widthWeightUnit = availableWidth / normalizedWeightsSum;
            float[] widthAllocation = new float[arrayLength];
            float widthAllocationSum = 0;
            float[] widthChange = new float[arrayLength];
            float[] previousWidthAllocation = new float[arrayLength];
            while (widthAllocationSum < availableWidth)
            {
                widthAllocation.CopyTo(previousWidthAllocation, 0);
                for (int i = 0; i < arrayLength; i++)
                {
                    if (widthAllocation[i] >= optimalWidths[i])
                    {
                        widthChange[i] = 0;
                        continue;
                    }
                    widthChange[i] = widthWeightUnit * normalizedWeights[i];
                    widthAllocationSum = widthAllocation.Sum();
                    if (widthAllocationSum + widthChange[i] > availableWidth)
                    {
                        float newWidthChange = availableWidth - widthAllocationSum;
                        widthChange[i] = newWidthChange;
                        widthAllocation[i] += newWidthChange;
                    }
                    else widthAllocation[i] += widthChange[i];
                }

                int columnsWithOptimalWidth = 0;
                for (int i = 0; i < arrayLength; i++)
                {
                    if (Math.Abs(widthAllocation[i] - optimalWidths[i]) < 0.1 || widthAllocation[i] > optimalWidths[i])
                    {
                        columnsWithOptimalWidth++;
                        widthChange[i] = optimalWidths[i] - previousWidthAllocation[i];
                        widthAllocation[i] = optimalWidths[i];
                    }
                }

                if (columnsWithOptimalWidth == arrayLength - 1) //Avoids further iterations if only one column needs to be filled
                {
                    widthAllocationSum = widthAllocation.Sum();
                    for (int i = 0; i < arrayLength; i++)
                    {
                        // ReSharper disable once CompareOfFloatsByEqualityOperator
                        if (widthAllocation[i] != optimalWidths[i])
                        {
                            float currentWidthAllocation = widthAllocation[i];
                            widthAllocation[i] = availableWidth - widthAllocationSum + currentWidthAllocation;
                        }
                    }
                }
                widthAllocationSum = widthAllocation.Sum();
            }
            bool rerunFairShare = false;
            int[] newOptimalWidths = new int[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                if (widthAllocation[i] < minimumWidths[i])
                {
                    newOptimalWidths[i] = minimumWidths[i];
                    rerunFairShare = true;
                }
                else
                {
                    newOptimalWidths[i] = optimalWidths[i];
                }
            }
            if (rerunFairShare && recursionCounter <= arrayLength) // recursion counter guarantees termination in extreme cases (may not be necessary)
            {
                return FairShare(availableWidth, newOptimalWidths, minimumWidths, columnContentPriorities, ++recursionCounter);
            }

            return widthAllocation.Select(x => x.ToIntSafeBounded()).ToArray(); //Always rounding down
        }

        public static bool IsItemIndexValid([NotNull] this ListView listView, int index) => index >= 0 && index < listView.Items.Count;

        public static void SortColumnOnClick([NotNull] this ListView listView, [NotNull] ColumnClickEventArgs e)
        {
            if (!(listView.ListViewItemSorter is ListViewColumnSorter sorter))
            {
                sorter = new ListViewColumnSorter();
                listView.ListViewItemSorter = sorter;
            }
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == sorter.ColumnToSort)
            {
                // Reverse the current sort direction for this column.
                sorter.OrderOfSort = sorter.OrderOfSort == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                sorter.ColumnToSort = e.Column;
                sorter.OrderOfSort = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            listView.Sort();
        }

        public static void SortByFirstColumn([NotNull] this ListView listView)
        {
            if (!(listView.ListViewItemSorter is ListViewColumnSorter sorter))
            {
                sorter = new ListViewColumnSorter();
                listView.ListViewItemSorter = sorter;
            }
            sorter.ColumnToSort = 0;
            sorter.OrderOfSort = SortOrder.Ascending;
            listView.Sort();
        }
    }

    [PublicAPI]
    public sealed class ListViewColumnSorter : IComparer, IComparer<ListViewItem>
    {
        public int ColumnToSort;
        public SortOrder OrderOfSort;

        /// <summary>
        /// Class constructor.  Initializes various elements
        /// </summary>
        public ListViewColumnSorter()
        {
            ColumnToSort = 0;
            OrderOfSort = SortOrder.Ascending;
        }

        public int Compare(object x, object y) => Compare(x as ListViewItem, y as ListViewItem);

        public int Compare(ListViewItem itemX, ListViewItem itemY)
        {
            if (itemX == null || itemY == null) return 0;
            string valueX = itemX.SubItems[ColumnToSort].Text;
            string valueY = itemY.SubItems[ColumnToSort].Text;

            int compareResult = new CaseInsensitiveComparer().Compare(valueX, valueY);

            switch (OrderOfSort)
            {
                case SortOrder.Ascending:
                    return compareResult;
                case SortOrder.Descending:
                    return -compareResult;
                default:
                    return 0;
            }
        }
    }
}
