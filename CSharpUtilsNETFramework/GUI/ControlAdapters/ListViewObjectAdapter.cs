#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CSharpUtilsNETFramework.Controls;
using CSharpUtilsNETFramework.GUI.ControlAdapters.AbstractAdapters;
using CSharpUtilsNETFramework.GUI.Dialogs;
using CSharpUtilsNETStandard.Utils;
using CSharpUtilsNETStandard.Utils.Extensions.General;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework.GUI.ControlAdapters
{
    [PublicAPI]
    public sealed class ListViewObjectAdapter<TObject> : TableObjectAdapter<ListView, ListViewItem, TObject, ListViewAdapterRowContent>
    {
        public delegate void ItemCreatedEventHandler(TObject value, ListViewItem item);
        public ItemCreatedEventHandler OnItemCreated;

        public delegate void OnAfterSortedEvent(int columnIndex);
        public OnAfterSortedEvent OnAfterSorted;

        /// <summary>
        /// Allows to test whether the ListView is currently being sorted as result of a column-click.
        /// </summary>
        public bool SortingByColumnClickInProgress { get; private set; }

        public bool CreateSorterIfColumnClicked { get; set; }

        private GetObjectTooltipHandler _tooltipHandler;

        private readonly Dictionary<ListViewItem, List<Control>> _listViewItemsToChildControlMap = new Dictionary<ListViewItem, List<Control>>();

        public ListViewObjectAdapter([NotNull] ListView control, [NotNull] GetObjectTextHandler objectTextHandler, [NotNull] string[] columnHeaders, [CanBeNull] GetObjectTooltipHandler tooltipHandler = null, bool createSorterIfColumnClicked = true)
            : base(control, objectTextHandler)
        {
            // ReSharper disable once UnusedVariable
            IntPtr handle = control.Handle; //This makes sure that the ListView Handle is created => Prevents some bad states where the ListView Items have no valid index
            SortingByColumnClickInProgress = false;

            if (control.View != View.Details)
            {
                Logger.PrintWarning("ListView.View property is NOT set to View.Details but to " + control.View + ". Was this on purpose?");
            }

            CreateSorterIfColumnClicked = createSorterIfColumnClicked;
            _tooltipHandler = tooltipHandler;

            SetColumnHeaders(columnHeaders);

            Control.ColumnClick += listView_ColumnClick; // needed for sorting

            // Creating private listeners used for actions which depend on ListViewItem-Mapping, which only exists AFTER Loading of data.
            // The mapping of the value<->item pairs is only stored within the abstract base class, which only does this after the LoadValues() method terminates.
            // This listener, however, is called by the base-class after the base.LoadValues() terminates.
            OnAfterValuesLoaded += adapter =>
            {
                // Setting the tool tips is only possible AFTER loading the values. Therefore this is done using an event-listener.
                if (_tooltipHandler != null) SetTooltips(_tooltipHandler);
            };
        }

        # region Adapter Methods

        protected override ListViewAdapterRowContent CreateDefaultObjectTextHandler(TObject obj) => new ListViewAdapterRowContent(obj + "");

        protected override Dictionary<TObject, ListViewItem> LoadValuesToControl(IEnumerable<GroupedObject<TObject>> values)
        {
            List<GroupedObject<TObject>> valueList = values.ToList();
            Dictionary<TObject, ListViewItem> valueToItemMap = LoadValuesToControl(valueList.SelectMany(x => x.GroupObjects));

            if (valueList.All(x => x.GroupName == DefaultGroupString)) return valueToItemMap;
            Control.BeginUpdate();
            foreach (var groupedObject in valueList)
            {
                var listViewGroup = new ListViewGroup(groupedObject.GroupName) { Tag = groupedObject.Tag };

                Control.Groups.Add(listViewGroup);

                listViewGroup.Items.AddRange(groupedObject.GroupObjects.Select(x => valueToItemMap[x]).ToArray());
            }
            Control.EndUpdate();
            // After this method terminates, this mapping is stored into the BiDirectional hashmap of the base class.
            // Then, the OnAfterLoaded-listeners are called. There the tooltips are set, if any tooltip-handler is defined.

            return valueToItemMap;
        }

        protected override Dictionary<TObject, ListViewItem> LoadValuesToControl(IEnumerable<TObject> values)
        {

            // Delete manual child-controls and clear mapping:
            foreach (Control control in _listViewItemsToChildControlMap.Values.SelectMany(control => control)) Control.Controls.Remove(control);
            // Remove all child-controls. Note: Control.Controls.Clear(); would remove ANY child-controls - not only those managed by the adapter!
            _listViewItemsToChildControlMap.Clear();

            // Create new value-to-item mapping:
            Dictionary<TObject, ListViewItem> valueToItemMap = new Dictionary<TObject, ListViewItem>();
            Control.BeginUpdate();
            // Load items:
            List<TObject> valuesList = values.ToList();
            List<ListViewItem> listViewItemsList = new List<ListViewItem>();
            foreach (TObject obj in valuesList)
            {
                ListViewAdapterRowContent rowContent = ObjectTextHandler(obj);

                // Printing a warning if data is missing for columns or more data than columns:
                if (rowContent.ColumnTextArray.Length > Control.Columns.Count)
                {
                    string warningMessage = string.Format("{0} values returned for a row with {1} columns! However, this may be intended in case of hidden/0-sized columns.\n-> Values: {2}\n-> Columns: {3}", rowContent.ColumnTextArray.Length, Control.Columns.Count, string.Join(" | ", rowContent.ColumnTextArray), string.Join(" | ", Control.GetColumnHeaders().Select(col => col.Text)));
                    Logger.PrintWarning(warningMessage);
                }

                ListViewItem item = new ListViewItem(rowContent.ColumnTextArray)
                {
                    Checked = rowContent.Checked
                };

                if (rowContent.Font != null) item.Font = rowContent.Font;

                // Add item to listView ==> Row will be added.
                listViewItemsList.Add(item);
            }

            bool veryLongRunning = listViewItemsList.Count > 300;
            bool continueLoading = true;
            ProgressDialog progressDialog = new ProgressDialog("Loading Elements");
            if (veryLongRunning)
            {
                progressDialog.ActionStop = () => continueLoading = false;
                progressDialog.ActionClose = () => continueLoading = false;
                progressDialog.ShowPercentage = true;
                progressDialog.ProgressBar.Style = ProgressBarStyle.Blocks;
                progressDialog.UpdateProgressBarValueSafe(0);
                progressDialog.RunInSeparateUIThreadAndDisposeAfterwards();
            }
            int index = 0;
            int count = 200;
            int totalCount = listViewItemsList.Count;
            while (index + count < totalCount)
            {
                if (!continueLoading) break;
                Control.Items.AddRange(listViewItemsList.GetRange(index, count).ToArray());
                index += count;
                if (veryLongRunning) progressDialog.UpdateProgressBarValueSafe(100 * index / totalCount);
                count = Math.Ceiling(count / (float)2).ToIntSafeBounded();
            }
            if (continueLoading)
            {
                if (index < totalCount)
                {
                    Control.Items.AddRange(listViewItemsList.GetRange(index, totalCount - index).ToArray());
                }

                index = totalCount;
            }
            for (int i = 0; i < index; i++)
            {
                TObject obj = valuesList[i];
                ListViewItem item = listViewItemsList[i];
                // Mapping so that item can be retrieved by its value
                valueToItemMap[obj] = item;

                // call post item-creation event (if any)
                OnItemCreated?.Invoke(obj, item);
            }
            if (veryLongRunning) progressDialog.CloseAndDisposeSeparateUIThread();
            Control.EndUpdate();
            return valueToItemMap;
        }

        public override int GetSelectedIndex()
        {
            if (Control.SelectedIndices.Count > 0) return Control.SelectedIndices[0];
            return -1;
        }

        public override void SetSelectedIndex(int index)
        {
            ClearSelection();
            if (Control.IsItemIndexValid(index)) Control.Items[index].Selected = true;
            else Logger.PrintWarning("Could not find ListViewItem with the index " + index);
        }

        public bool SetSelectedIndexIfExists(int index)
        {
            ClearSelection();
            if (!Control.IsItemIndexValid(index)) return false;
            Control.Items[index].Selected = true;
            return true;
        }

        public bool SetCheckedIndexIfExists(int index)
        {
            ClearSelection();
            if (!Control.IsItemIndexValid(index)) return false;
            Control.Items[index].Checked = true;
            return true;
        }

        public override void ClearControlItems()
        {
            Control.Items.Clear();
        }

        public override ListViewItem GetControlItem(int index)
        {
            if (Control.IsItemIndexValid(index)) return Control.Items[index];
            Logger.PrintWarning("Could not find ListViewItem with the index " + index);
            return null;
        }

        protected override int GetControlItemIndex(ListViewItem item) => item.Index;

        protected override IEnumerable<int> SelectedIndexes => Control.SelectedIndices.Cast<int>();

        protected override void SetValuesSelected(IEnumerable<TObject> values, bool selected)
        {
            foreach (ListViewItem item in values.Select(GetControlItem).Where(x => x != null)) item.Selected = selected;
        }

        public override void ClearSelection()
        {
            Control.SelectedIndices.Clear();
        }

        #endregion

        #region Checked Items

        private const string CheckBoxesDeactivatedWarning = "Item is being checked while ListView.CheckBoxes is set to FALSE. Operation will NOT be visible!";

        private void CheckBoxesValidityCheck()
        {
            if (!Control.CheckBoxes) Logger.PrintWarning(CheckBoxesDeactivatedWarning);
        }

        [NotNull, ItemNotNull]
        public IEnumerable<TObject> GetCheckedValues()
        {
            CheckBoxesValidityCheck();
            return Control.GetCheckedItems().Select(x => GetValue(x.Index));
        }

        public bool IsValueChecked([NotNull] TObject value)
        {
            CheckBoxesValidityCheck();
            ListViewItem controlItem = GetControlItem(value);
            return controlItem != null && controlItem.Checked;
        }

        public void SetValueChecked([NotNull]Func<TObject, bool> isCheckedFunc)
        {
            CheckBoxesValidityCheck();
            foreach (TObject value in LoadedValues) SetValueCheckedInternal(value, isCheckedFunc(value));
        }

        public void SetValueChecked([NotNull] TObject value, bool @checked)
        {
            CheckBoxesValidityCheck();
            SetValueCheckedInternal(value, @checked);
        }

        private void SetValueCheckedInternal([NotNull] TObject value, bool @checked)
        {
            ListViewItem controlItem = GetControlItem(value);
            if (controlItem == null) return;
            controlItem.Checked = @checked;
        }

        public void SetValuesChecked([NotNull] IEnumerable<TObject> values, bool @checked)
        {
            CheckBoxesValidityCheck();
            SetValuesCheckedInternal(values, @checked);
        }

        private void SetValuesCheckedInternal([NotNull] IEnumerable<TObject> values, bool @checked)
        {
            foreach (TObject value in values) SetValueCheckedInternal(value, @checked);
        }

        public void SetValuesCheckedEfficient([NotNull] IReadOnlyList<TObject> values, bool @checked)
        {
            CheckBoxesValidityCheck();
            ListViewItem lastControlItem = null;
            int lastIndex = 0;
            for (int i = values.Count - 1; i >= 0; i--)
            {
                lastControlItem = GetControlItem(values[i]);
                if (lastControlItem == null || lastControlItem.Checked == @checked) continue;
                lastIndex = i;
                break;
            }
            if (lastControlItem == null) return;
            for (int i = 0; i < lastIndex; i++) SetValueCheckedInternal(values[i], @checked);
            lastControlItem.Checked = @checked;
        }

        public void SetAllChecked(bool @checked)
        {
            CheckBoxesValidityCheck();
            foreach (ListViewItem item in Control.GetListViewItems()) item.Checked = @checked;
        }

        public void UncheckAll()
        {
            SetAllChecked(false);
        }

        public void SetOnlyOneItemSelectedAndChecked([NotNull]ListViewItem listViewItem)
        {
            CheckBoxesValidityCheck();
            foreach (ListViewItem item in Control.GetListViewItems().Where(x => !ReferenceEquals(x, listViewItem)))
            {
                item.Checked = false;
                item.Selected = false;
            }
            listViewItem.Checked = true;
            listViewItem.Selected = true;
        }

        public void SetAllSelected(bool selected)
        {
            foreach (ListViewItem item in Control.GetListViewItems()) item.Selected = selected;
        }

        # endregion

        # region Additional Usability Methods

        public void SetTooltips(GetObjectTooltipHandler tooltipHandler)
        {
            _tooltipHandler = tooltipHandler;

            // Show tooltips if handler defined
            Control.ShowItemToolTips = true;
            // Update tooltips:
            foreach (ListViewItem item in Control.Items)
            {
                item.ToolTipText = _tooltipHandler(GetValue(item.Index), item);

                // Note that the ListView only shows Tooltips for the first Column. This is a long known shortcoming.
                // see: https://social.msdn.microsoft.com/Forums/vstudio/en-US/2c641698-36f6-404d-af00-d8e876cfd847/getting-tooltip-to-display-on-all-columns-of-listview-not-just-the-first?forum=vbgeneral
            }
        }

        #endregion


        #region ListView Methods



        # region Handling Manual Child Controls

        [NotNull]
        public List<Control> GetChildControlsForItem([NotNull] ListViewItem item) => _listViewItemsToChildControlMap.ContainsKey(item) ? _listViewItemsToChildControlMap[item].ToList() : new List<Control>();

        public void AddControlToListViewItem([NotNull] ListViewItem item, int columnIndex, [NotNull] Control controlToAddToListViewItem, bool automaticallyUpdateVerticalPositionOfControlWhenSortingListView = true, bool autoUpdateControlBounds = true)
        {
            // Check if listview item is assigned to ListView yet
            if (item.ListView == null)
                throw new ArgumentException("ListViewItem must be part of a listView in order to add a child control!", nameof(item));

            // Set default bounds
            if (autoUpdateControlBounds)
                controlToAddToListViewItem.Bounds = item.SubItems[columnIndex].Bounds;

            // Add control to ListViewItem
            item.ListView.Controls.Add(controlToAddToListViewItem);

            // If desired keep track of child-control
            if (!automaticallyUpdateVerticalPositionOfControlWhenSortingListView) return;
            List<Control> controlsForThisItem;
            if (!_listViewItemsToChildControlMap.ContainsKey(item))
            {
                controlsForThisItem = new List<Control>();
                _listViewItemsToChildControlMap[item] = controlsForThisItem;
            }
            else
            {
                controlsForThisItem = _listViewItemsToChildControlMap[item];
            }

            // List this control as child-control of the listViewItem
            controlsForThisItem.Add(controlToAddToListViewItem);
        }

        [NotNull]
        public TextBox AddTextBoxToListViewItem([NotNull] ListViewItem item, int columnIndex)
        {
            TextBox textBox = new TextBox();
            AddControlToListViewItem(item, columnIndex, textBox);
            return textBox;
        }

        [NotNull]
        public RadioButton AddRadioButtonToListViewItem([NotNull] ListViewItem item, int columnIndex)
        {
            RadioButton radioButton = new RadioButton();
            AddControlToListViewItem(item, columnIndex, radioButton);
            return radioButton;
        }

        [NotNull]
        public ComboBox AddComboboxToListViewItem([NotNull] ListViewItem item, int columnIndex)
        {
            ComboBox comboBox = new ComboBox();
            AddControlToListViewItem(item, columnIndex, comboBox);
            return comboBox;
        }

        # endregion

        public override void SetColumnHeaders([NotNull] params string[] headerNames)
        {
            foreach (string name in headerNames)
            {
                string key = Control.Columns.Count.ToString();
                Control.Columns.Add(key, name);
            }
        }

        public void AutoResizeColumnHeaders()
        {
            Control.AutoSizeColumns();
        }

        # region Sorting

        /// <summary>
        /// Sort ListViewItems by their Text or by a custom value.
        /// </summary>
        private void SetItemSorter([CanBeNull] IComparer itemComparator = null)
        {
            Control.ListViewItemSorter = itemComparator == null ? new BasicListViewItemComparer() : new BasicListViewItemComparer(itemComparator);
        }

        public void Sort(int columnIndex, IComparer itemComparator)
        {
            BasicListViewItemComparer comparer = itemComparator as BasicListViewItemComparer;
            Control.ListViewItemSorter = comparer ?? new BasicListViewItemComparer(itemComparator);
            Sort(columnIndex);
        }

        public void ResetSorting([NotNull] Action reloadListViewValues)
        {
            Control.Sorting = SortOrder.None;
            reloadListViewValues();
            // Perform After-Sort events
            for (int i = 0; i < Control.Columns.Count; i++)
            {
                InternalAfterSorted(i);
            }
        }

        public void Sort(int columnIndex)
        {
            BasicListViewItemComparer comparer;

            if (Control.ListViewItemSorter == null)
            {
                // Wrap comparer in BasicListViewItemComparer
                comparer = new BasicListViewItemComparer();
            }
            else
            {
                Logger.PrintInfo("Provided comparer is not of type " + nameof(BasicListViewItemComparer) + " and will be wrapped.");
                BasicListViewItemComparer itemComparer = Control.ListViewItemSorter as BasicListViewItemComparer;
                comparer = itemComparer ?? new BasicListViewItemComparer(Control.ListViewItemSorter);
            }

            // Assign comparer
            if (!ReferenceEquals(Control.ListViewItemSorter, comparer)) // Note that this check before assignment is not redundant because assigning a Comparer has side-effects
            {
                Control.ListViewItemSorter = comparer;
            }

            // Determine whether the column is the same as the last column clicked.
            if (columnIndex != comparer.Column)
            {
                // Set the sort column to the new column.
                comparer.Column = columnIndex;
                // Set the sort order to ascending by default.
                Control.Sorting = SortOrder.Ascending;
            }
            else
            {
                // Determine what the last sort order was and change it.

                Control.Sorting = Control.Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
                // Note: Setting ListView.Sorting to "None" removes the sorter.
            }

            // update Comparer settings:
            comparer.SortOrder = Control.Sorting;

            Control.Sort();

            // Perform After-Sort events
            InternalAfterSorted(columnIndex);
        }

        private void InternalAfterSorted(int columnIndex)
        {
            // Update locations of child-controls (if any)
            foreach (ListViewItem item in _listViewItemsToChildControlMap.Keys.ToList()) // ToList() for allowing modification of list during iteration
            {
                try
                {
                    // If item has been removed already, e.g. when listview has been cleared
                    if (item.ListView == null)
                    {
                        _listViewItemsToChildControlMap.Remove(item);
                        continue;
                    }

                    // Get child-controls for this ListViewItem by map
                    List<Control> associatedChildControls = _listViewItemsToChildControlMap[item];

                    // Update positions of child-controls
                    foreach (Control childControl in associatedChildControls)
                    {
                        try
                        {
                            // Update vertical position to the new vertical position of the row:
                            childControl.Bounds = new Rectangle(new Point(childControl.Bounds.X, item.SubItems[columnIndex].Bounds.Y), childControl.Bounds.Size); // take only the y-coordinate from the row
                        }
                        catch (Exception e)
                        {
                            Logger.PrintError("Error when trying to update position of control of item. Control has been ignored. No harm done. Control was: " + childControl, nameof(ListViewObjectAdapter<TObject>), e);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.PrintError("Error when trying to update position of controls for item. Item has been ignored. No harm done. Item was: " + item, nameof(ListViewObjectAdapter<TObject>), e);
                }
            }

            // Invoke after-sort event (if any listener)
            OnAfterSorted?.Invoke(columnIndex);
        }

        # endregion

        # endregion

        # region ListView Events

        private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (Control.CheckBoxes && e.Column == 0 || SortingByColumnClickInProgress) return;

            Logger.PrintTrace("ListView column clicked.");

            // Use default-sorter if user clicks on column and none is set (and option enabled):
            if (!CreateSorterIfColumnClicked && Control.ListViewItemSorter == null)
            {
                Logger.PrintInfo("User-Click on column ignored as AutoCreation of Comparer is disabled.");
                return;
            }

            // So that external events resulting from the sorting can check this flag externally and ignore the calls during that time:
            try
            {
                SortingByColumnClickInProgress = true;
                Sort(e.Column);
            }
            finally
            {
                SortingByColumnClickInProgress = false;
            }
        }

        # endregion

        # region ListView Utility Classes

        public delegate int ItemComparatorDelegate(ListViewItem item1, ListViewItem item2, int column, SortOrder sortOrder);

        private sealed class BasicListViewItemComparer : IComparer
        {
            // See: https://msdn.microsoft.com/en-us/library/ms996467.aspx
            [NotNull]
            private readonly ItemComparatorDelegate _itemComparatorDelegate;

            public int Column { get; set; }
            public SortOrder SortOrder { get; set; }

            private BasicListViewItemComparer([NotNull]ItemComparatorDelegate comparatorDelegate, int column = 0, SortOrder sortOrder = SortOrder.None)
            {
                _itemComparatorDelegate = comparatorDelegate;
                Column = column;
                SortOrder = sortOrder;
            }

            /// <summary>
            /// Sort items using the provided comparator.
            /// </summary>
            /// <param name="comparator"></param>
            /// <param name="column"></param>
            /// <param name="sortOrder"></param>
            public BasicListViewItemComparer(IComparer comparator, int column = 0, SortOrder sortOrder = SortOrder.None)
                : this((item1, item2, i, order) => DefaultComparator(item1, item2, i, order, comparator), column, sortOrder)
            {
            }

            /// <summary>
            /// Sorts items by their text.
            /// </summary>
            public BasicListViewItemComparer(int column = 0, SortOrder sortOrder = SortOrder.None)
                : this((item1, item2, i, order) => DefaultComparator(item1, item2, i, order), column, sortOrder)
            {
            }

            private static int DefaultComparator([NotNull] ListViewItem item1, [NotNull] ListViewItem item2, int column, SortOrder sortOrder, [CanBeNull] IComparer comparer = null)
            {
                // Sort by index if SortOrder is "none":
                if (sortOrder == SortOrder.None)
                {
                    return item1.Index == item2.Index ? 0 : (item1.Index < item2.Index ? -1 : 1);
                }

                int result;

                if (comparer == null)
                {
                    // When both values are integer values, they are compared as such. String-comparison otherwise
                    try
                    {
                        int value1 = int.Parse(item1.SubItems[column].Text);
                        int value2 = int.Parse(item2.SubItems[column].Text);
                        result = value1 == value2 ? 0 : (value1 < value2 ? -1 : 1);
                    }
                    catch (Exception)
                    {
                        result = string.CompareOrdinal(item1.SubItems[column].Text, item2.SubItems[column].Text);
                    }
                }
                else
                {
                    result = comparer.Compare(item1.SubItems[column].Text, item2.SubItems[column].Text);
                }

                return sortOrder == SortOrder.Descending ? result * -1 : result;
            }

            public int Compare(object x, object y)
            {
                ListViewItem item1 = (ListViewItem)x;
                ListViewItem item2 = (ListViewItem)y;

                int sortResult = _itemComparatorDelegate(item1, item2, Column, SortOrder);

                return sortResult;
            }

        }

        # endregion

    }

    [PublicAPI]
    public sealed class ListViewAdapterRowContent
    {
        public bool Checked { get; private set; }
        [NotNull, ItemNotNull]
        public string[] ColumnTextArray { get; private set; }
        [CanBeNull]
        public Font Font { get; set; }

        public ListViewAdapterRowContent([NotNull, ItemNotNull]params string[] columnTextArray)
            : this(false, columnTextArray)
        {
        }

        public ListViewAdapterRowContent(bool @checked = false, [NotNull, ItemNotNull]params string[] columnTextArray)
        {
            Checked = @checked;
            ColumnTextArray = columnTextArray;
        }
    }
}
