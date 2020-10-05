#region Imports

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CSharpUtilsNETFramework.Controls;
using CSharpUtilsNETFramework.GUI.ControlAdapters;
using CSharpUtilsNETFramework.GUI.ControlAdapters.AbstractAdapters;
using CSharpUtilsNETStandard.Utils;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework.GUI.GenericListSelectionControl
{
    [PublicAPI]
    public sealed class GenericColumnSpecification<T>
    {
        [NotNull]
        public string Name { get; private set; }

        [NotNull]
        public Func<T, string> ObjectToColumnValueString { get; private set; }

        public ColumnContentPriority ColumnContentPriority { get; private set; }

        [CanBeNull]
        public IReadOnlyDictionary<string, Color> ValueToForeColorMap { get; }

        //Add more things as necessary
        public GenericColumnSpecification([NotNull] string name, [NotNull] Func<T, string> objectToColumnValueString, ColumnContentPriority columnContentPriority = ColumnContentPriority.Medium, [CanBeNull] IReadOnlyDictionary<string, Color> valueToForeColorMap = null)
        {
            Name = name;
            ObjectToColumnValueString = objectToColumnValueString;
            ColumnContentPriority = columnContentPriority;
            ValueToForeColorMap = valueToForeColorMap;
        }
    }

    [PublicAPI]
    public sealed class GenericAdapterConfiguration<T>
    {
        [NotNull]
        public string CheckBoxColumnName { get; set; }

        [NotNull]
        public IReadOnlyList<GenericColumnSpecification<T>> ColumnSpecifications { get; set; }

        [NotNull]
        public IReadOnlyList<T> Items { get; set; }

        [NotNull]
        public IReadOnlyList<T> PreselectedItems { get; set; }

        [NotNull]
        public IReadOnlyList<GroupedObject<T>> GroupedObjectsList { get; set; }

        public GenericAdapterConfiguration()
        {
            CheckBoxColumnName = "";
            ColumnSpecifications = new GenericColumnSpecification<T>[0];
            Items = new T[0];
            PreselectedItems = new T[0];
            GroupedObjectsList = new GroupedObject<T>[0];
        }
    }

    [PublicAPI]
    public sealed class GenericListSelectionAdapter<T>
    {
        #region Events

        public delegate void NewItemSelectedOrCheckedHandler([NotNull] GenericListSelectionAdapter<T> sender, [NotNull] T item);

        public delegate void SelectionOrCheckChangedHandler([NotNull] GenericListSelectionAdapter<T> sender, [CanBeNull] T item);

        public delegate void BeforeCheckChangedHandler([NotNull] GenericListSelectionAdapter<T> sender, [CanBeNull] T item, ref bool allowCheckChanged);

        public delegate void GenericEventHandler([NotNull] GenericListSelectionAdapter<T> sender);

        [CanBeNull]
        public event GenericEventHandler NavigationClick;

        [CanBeNull]
        public event BeforeCheckChangedHandler ItemBeforeCheckChanged;

        [CanBeNull]
        public event SelectionOrCheckChangedHandler ItemAfterCheckChanged;

        [CanBeNull]
        public event SelectionOrCheckChangedHandler ItemAfterSelectionChanged;

        [CanBeNull]
        public event SelectionOrCheckChangedHandler ItemAfterSelectionOrCheckedChanged;

        [CanBeNull]
        public event NewItemSelectedOrCheckedHandler NewValidItemSelected;

        [CanBeNull]
        public event NewItemSelectedOrCheckedHandler NewValidItemChecked;

        [CanBeNull]
        public event NewItemSelectedOrCheckedHandler NewValidItemSelectedOrChecked;

        #endregion

        private readonly bool Initialized;

        [NotNull]
        public readonly GenericAdapterConfiguration<T> Configuration;

        [NotNull]
        public readonly ListSelectionControl ListSelectionControl;

        [NotNull]
        public readonly ListViewObjectAdapter<T> ListViewObjectAdapter;

        [NotNull]
        private IReadOnlyList<GroupedObject<T>> DataObjects => Configuration.GroupedObjectsList;

        [NotNull]
        public ListView ListView => ListSelectionControl.ListView;

        [NotNull]
        private Button NavigationButton => ListSelectionControl.NavigationButton;

        [NotNull]
        private Button OptionsButton => ListSelectionControl.OptionsButton;

        [NotNull]
        private ContextMenuStrip OptionsContextMenuStrip => ListSelectionControl.OptionsContextMenuStrip;

        [NotNull]
        private ContextMenuStrip NavigationContextMenuStrip => ListSelectionControl.NavigationContextMenuStrip;

        [NotNull]
        private ContextMenuStrip ColumnsContextMenuStrip => ListSelectionControl.ColumnsContextMenuStrip;

        public ListViewEventReaction CurrentListViewEventReaction => ListSelectionControl.CurrentListViewEventReaction;

        public void SetListViewEventReaction(ListViewEventReaction eventReaction)
        {
            ListSelectionControl.SetListViewEventReaction(eventReaction);
        }

        public void ResetLastListViewEventReaction()
        {
            ListSelectionControl.ResetListViewEventReaction();
        }

        private bool IsNavigable
        {
            get => ListSelectionControl.IsNavigable;
            set => ListSelectionControl.IsNavigable = value;
        }

        private bool HasOptions
        {
            get => ListSelectionControl.HasOptions;
            set => ListSelectionControl.HasOptions = value;
        }

        [NotNull, ItemNotNull]
        private IEnumerable<ListViewItem> ListViewItems => ListViewObjectAdapter.GetControlItems();

        [NotNull, ItemNotNull]
        private IEnumerable<T> GenericItems => ListViewObjectAdapter.GetValues(ListViewItems);

        [NotNull, ItemNotNull]
        private IEnumerable<ColumnHeader> ColumnHeaders => ListView.GetColumnHeaders();

        public int ListViewEmptyHeight => ListView.GetEmptyListViewHeight();

        public bool IgnoreAllListViewEvents => ListViewObjectAdapter.SortingByColumnClickInProgress;

        public GenericListSelectionAdapter([NotNull] ListSelectionControl listSelectionControl, [NotNull] GenericAdapterConfiguration<T> configuration)
        {
            ListSelectionControl = listSelectionControl;
            Configuration = configuration;
            ListSelectionControl.ColumnContentPriorities = Configuration.ColumnSpecifications.Select(x => x.ColumnContentPriority).ToList();
            ListSelectionControl.ItemAfterSelectionChanged += ListView_ItemAfterSelectionChanged;
            ListSelectionControl.ItemAfterCheckChanged += ListView_ItemAfterCheckChanged;
            ListSelectionControl.ItemBeforeCheckChanged += ListView_ItemBeforeCheckChanged;
            ListSelectionControl.NavigateClick += Navigate_Click;
            ListSelectionControl.OptionsClick += OptionsButton_Click;
            ListSelectionControl.CheckBoxColumnName = Configuration.CheckBoxColumnName;
            if (Configuration.ColumnSpecifications.Count > 0 && string.IsNullOrWhiteSpace(Configuration.ColumnSpecifications[0].Name))
            {
                Logger.PrintWarning("Columns with empty names are no longer required for Checkboxes. They are automatically added. If you want a special column name for the Checkboxes column then use the corresponding property of the configuration.");
            }
            // Two kinds of item-sets are supported: (1) without groups (2) items by groups
            //   (1) In case there are no groups.cd
            if (Configuration.GroupedObjectsList.Count == 0)
            {
                // Use old initialization (no groups)
                Configuration.GroupedObjectsList = new[] { new GroupedObject<T>(Configuration.Items) };
            }

            if (ListSelectionControl.EnableCheckBoxes)
            {
                var columnSpecifications = new List<GenericColumnSpecification<T>> { new GenericColumnSpecification<T>(Configuration.CheckBoxColumnName, item => "") };
                columnSpecifications.AddRange(Configuration.ColumnSpecifications);
                Configuration.ColumnSpecifications = columnSpecifications.ToArray();
            }
            ListViewObjectAdapter = CreateListViewObjectAdapter();
            ListSelectionControl.IgnoreAllListViewUserEventsFunction = () => IgnoreAllListViewEvents;
            // Load Data to ListView
            ListViewObjectAdapter.LoadValues(DataObjects);

            // (2) in case there are groups
            // Use old initialization (with groups)
            if (ListSelectionControl.EnableMultiselection)
            {
                ToolStripMenuItem selectAll = new ToolStripMenuItem("Select All", null, (sender, args) => ListViewObjectAdapter.SetAllChecked(true));
                ToolStripMenuItem deselectAll = new ToolStripMenuItem("Deselect All", null, (sender, args) => ListViewObjectAdapter.SetAllChecked(false));

                OptionsContextMenuStrip.Items.Add(new ToolStripSeparator());
                OptionsContextMenuStrip.Items.Add(selectAll);
                OptionsContextMenuStrip.Items.Add(deselectAll);
            }

            // Invoke methods which are optionally part of the _configuration
            if (HasOptions) HasOptions = false; //TODO
            // If no subset of initial columns is set, all columns will be shown initially

            // Create MenuItems for each column so that the user can show/hide the columns
            CreateMenuItemsForColumns();

            UpdateGuiOnSelection();
            Initialized = true;
        }

        #region Selection and Checking

        public void SetSelectedValue([NotNull] T value, ListViewEventReaction eventReaction)
        {
            SetListViewEventReaction(eventReaction);
            ListViewObjectAdapter.SetSelectedValue(value);
            ResetLastListViewEventReaction();
        }

        public bool IsValueChecked([NotNull] T value) => ListViewObjectAdapter.IsValueChecked(value);

        public void SetValuesCheckedEfficientIgnoreAllListViewEvents([NotNull] IReadOnlyList<T> values, bool checkValue)
        {
            SetListViewEventReaction(ListViewEventReaction.IgnoreAllListViewUserEvents);
            ListViewObjectAdapter.SetValuesCheckedEfficient(values, checkValue);
            ResetLastListViewEventReaction();
        }

        public void SetValueChecked([NotNull] T value, bool checkValue, ListViewEventReaction eventReaction)
        {
            SetListViewEventReaction(eventReaction);
            ListViewObjectAdapter.SetValueChecked(value, checkValue);
            ResetLastListViewEventReaction();
        }

        public void SetValuesChecked([NotNull, ItemNotNull] IEnumerable<T> values, bool checkValue, ListViewEventReaction eventReaction)
        {
            SetListViewEventReaction(eventReaction);
            ListViewObjectAdapter.SetValuesChecked(values, checkValue);
            ResetLastListViewEventReaction();
        }

        public void SelectFirstItem(ListViewEventReaction eventReaction)
        {
            SetListViewEventReaction(eventReaction);
            ListViewObjectAdapter.SetSelectedIndexIfExists(0);
            ResetLastListViewEventReaction();
        }

        public void CheckFirstItem(ListViewEventReaction eventReaction)
        {
            SetListViewEventReaction(eventReaction);
            ListViewObjectAdapter.SetCheckedIndexIfExists(0);
            ResetLastListViewEventReaction();
        }

        #endregion

        public bool IsAtLeastOneItemSelected() => NumberOfSelectedItems > 0;

        public bool IsSelectionValidForNavigation()
        {
            if (ListView.MultiSelect && ListView.CheckBoxes)
            {
                return ListView.SelectedIndices.Count == 1 || (ListView.SelectedIndices.Count == 0 && ListView.CheckedIndices.Count == 1);
            }
            return NumberOfSelectedItems == 1;
        }

        [CanBeNull]
        private T NavigationItem
        {
            get
            {
                if (!ListSelectionControl.EnableCheckBoxes) return ListViewObjectAdapter.GetSelectedValue();
                if (ListView.SelectedItems.Count > 0) return ListViewObjectAdapter.GetSelectedValue();
                return ListView.CheckedItems.Count > 0 ? ListViewObjectAdapter.GetCheckedValues().FirstOrDefault() : default;
            }
        }

        private void UpdateGuiOnSelection()
        {
            ListSelectionControl.RefreshListView();
            if (IsNavigable) NavigationButton.Enabled = IsSelectionValidForNavigation();
            //OptionsButton.Enabled = HasOptions;
        }

        public int NumberOfSelectedItems => ListView.CheckBoxes ? ListView.CheckedIndices.Count : ListView.SelectedIndices.Count;

        [CanBeNull]
        public T SelectedItem
        {
            get
            {
                if (NumberOfSelectedItems == 0) return default;
                T selectedValue = ListViewObjectAdapter.GetSelectedValue();
                if (selectedValue == null) return SelectedItems.FirstOrDefault(); //TODO think about correct behaviour for structs
                return selectedValue;
            }
        }

        [NotNull, ItemNotNull]
        public IEnumerable<T> SelectedItems
        {
            get
            {
                // if listview is configured with checkboxes return checked items
                IEnumerable<ListViewItem> selectedItems = ListView.CheckBoxes ? ListView.GetCheckedItems() : ListView.GetSelectedItems();
                return ListViewObjectAdapter.GetValues(selectedItems);
            }
        }

        [NotNull, ItemNotNull]
        public IEnumerable<T> UnselectedItems
        {
            get
            {
                HashSet<T> selectedItems = SelectedItems.ToHashSet();
                return Items.Where(x => !selectedItems.Contains(x));
            }
        }

        [NotNull]
        public Dictionary<T, bool> GetItemsWithCheckStatus()
        {
            Dictionary<T, bool> values = SelectedItems.ToDictionary(x => x, x => true);
            foreach (T unselectedItem in Items.Where(x => !values.ContainsKey(x)))
            {
                values[unselectedItem] = false;
            }
            return values;
        }

        [NotNull]
        private ListViewObjectAdapter<T> CreateListViewObjectAdapter()
        {
            return new ListViewObjectAdapter<T>(ListView, GetRowContentForDataObject, Configuration.ColumnSpecifications.Select(x => x.Name).ToArray())
            {
                OnItemCreated = (dataObject, listItem) =>
                                {
                                    listItem.Checked = Configuration.PreselectedItems.Contains(dataObject);
                                    listItem.Tag = dataObject;

                                    foreach (var columnSpec in Configuration.ColumnSpecifications)
                                    {
                                        if (columnSpec.ValueToForeColorMap == null) continue;
                                        foreach (ListViewItem.ListViewSubItem subItem in listItem.SubItems)
                                        {
                                            if (!columnSpec.ValueToForeColorMap.ContainsKey(subItem.Text)) continue;
                                            listItem.UseItemStyleForSubItems = false;
                                            subItem.BackColor = columnSpec.ValueToForeColorMap[subItem.Text];
                                        }
                                    }
                                }
            };
        }

        [NotNull]
        private ListViewAdapterRowContent GetRowContentForDataObject([NotNull] T rowObject)
        {
            // The old configuration creates the values of the ListView row-wise for each item. The new configuration queries the values column-wise for each row.
            string[] columnValuesAsRow = Configuration.ColumnSpecifications.Select(spec => spec.ObjectToColumnValueString(rowObject)).ToArray();
            // Create row-content for visible columns only:
            // Add Text for the column i when it is visible
            return new ListViewAdapterRowContent(columnValuesAsRow);
        }

        private void CreateMenuItemsForColumns()
        {
            ToolStripMenuItem itemResetColumnWidths = new ToolStripMenuItem("Reset column widths");
            itemResetColumnWidths.Click += (sender, args) => ListSelectionControl.ResetColumnWidths();
            ColumnsContextMenuStrip.Items.Add(itemResetColumnWidths);
            ToolStripMenuItem itemResetColumnSorting = new ToolStripMenuItem("Reset column sorting");
            itemResetColumnSorting.Click += (sender, args) => ListViewObjectAdapter.ResetSorting(Reload);
            ColumnsContextMenuStrip.Items.Add(itemResetColumnSorting);
            foreach (ColumnHeader columnHeader in ColumnHeaders.Where(y => y.Text.Length > 0))
            {
                // Created MenuItem
                ToolStripMenuItem item = new ToolStripMenuItem(columnHeader.Text)
                {
                    CheckOnClick = true,
                    Checked = true,
                    Tag = columnHeader
                };

                columnHeader.Tag = item;

                // On CheckStateChanged, the associated column will be removed or re-inserted
                // in regard to the column index (Columns can't get mixed up by continuously
                // doing this). After this, the columns are resized to match the control width.
                item.CheckStateChanged += (sender, args) =>
                                          {
                                              ColumnHeader itemColumnHeader = (ColumnHeader)item.Tag;
                                              if (item.Checked)
                                              {
                                                  int columnCount = 0;

                                                  foreach (string columnTitle in Configuration.ColumnSpecifications.Select(x => x.Name))
                                                  {
                                                      if (columnTitle.Equals(itemColumnHeader.Text))
                                                      {
                                                          ListView.Columns.Insert(columnCount, itemColumnHeader);
                                                      }
                                                      else
                                                      {
                                                          string title = columnTitle;
                                                          if (ColumnHeaders.Any(y => y.Text.Equals(title))) columnCount++;
                                                      }
                                                  }
                                              }
                                              else
                                              {
                                                  ListView.Columns.Remove(itemColumnHeader);
                                              }

                                              // reload data to align row-content to existing columns
                                              // is required because removing a Column n<m from a set of m columns will shift all data one column to the left since the listview does not handle such cases.
                                              ListViewObjectAdapter.LoadValues(DataObjects); // it is important to load the data like this, as the "DataObjects" also contain the group-mappings (if any)

                                              // Save to settings:
                                          };

                ColumnsContextMenuStrip.Items.Add(item);
            }
        }

        public void Reload([NotNull, ItemNotNull] IEnumerable<T> newItems)
        {
            Configuration.Items = newItems.ToList();
            Configuration.GroupedObjectsList = new[] { new GroupedObject<T>(Configuration.Items) };
            Reload();
        }

        public void Reload()
        {
            ListViewObjectAdapter.LoadValues(DataObjects);
            ListSelectionControl.AutoSizeColumns();
        }

        [NotNull]
        public ICollection<T> Items => ListViewObjectAdapter.LoadedValuesAsCollection;

        #region UI Event Handlers

        private void ListView_ItemAfterSelectionChanged([CanBeNull] object sender, [NotNull] ListViewItemSelectionChangedEventArgs selectionEvent)
        {
            HandleAfterSelectionAndCheckChangedEvents(selectionEvent, null);
        }

        private void ListView_ItemBeforeCheckChanged([CanBeNull] object sender, [NotNull] ItemCheckEventArgs e)
        {
            if (!Initialized) return;
            bool allowCheckChange = true;
            ItemBeforeCheckChanged?.Invoke(this, ListViewObjectAdapter.GetValue(e.Index), ref allowCheckChange);
            if (!allowCheckChange) e.NewValue = e.CurrentValue;
        }

        private void ListView_ItemAfterCheckChanged([CanBeNull] object sender, [NotNull] ItemCheckedEventArgs checkedEvent)
        {
            HandleAfterSelectionAndCheckChangedEvents(null, checkedEvent);
        }

        private bool _saveRecentEvents;

        private bool SaveRecentEvents
        {
            get => _saveRecentEvents;
            set
            {
                RecentSelectionAndCheckEvents.Clear();
                _saveRecentEvents = value;
            }
        }

        private void InvokeEventIfNotOccurred([NotNull] SelectionCheckEvent @event)
        {
            if (!RecentSelectionAndCheckEvents.Contains(@event))
            {
                HandleAfterSelectionAndCheckChangedEvents(@event);
            }
        }

        private sealed class SelectionCheckEvent : IEquatable<SelectionCheckEvent>
        {
            [CanBeNull]
            public ListViewItem Item { get; }

            [CanBeNull]
            public bool? IsSelected { get; }

            [CanBeNull]
            public bool? IsChecked { get; }

            private SelectionCheckEvent([CanBeNull] ListViewItem item, bool? isSelected, bool? isChecked)
            {
                Item = item;
                IsSelected = isSelected;
                IsChecked = isChecked;
            }

            [NotNull]
            public static SelectionCheckEvent GetSelectionEvent([CanBeNull] ListViewItem item, bool isSelected) => new SelectionCheckEvent(item, isSelected, null);

            [NotNull]
            public static SelectionCheckEvent GetCheckEvent([CanBeNull] ListViewItem item, bool isChecked) => new SelectionCheckEvent(item, null, isChecked);

            public SelectionCheckEvent([CanBeNull] ListViewItemSelectionChangedEventArgs selectionEvent, [CanBeNull] ItemCheckedEventArgs checkedEvent)
            {
                if (selectionEvent != null)
                {
                    Item = selectionEvent.Item;
                    IsSelected = selectionEvent.IsSelected;
                }
                else if (checkedEvent != null)
                {
                    Item = checkedEvent.Item;
                    IsChecked = Item?.Checked;
                }
            }

            public bool Equals(SelectionCheckEvent other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(Item, other.Item)
                    && IsSelected == other.IsSelected
                    && IsChecked == other.IsChecked;
            }

            public override bool Equals(object obj) => Equals(obj as SelectionCheckEvent);

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = (Item != null ? Item.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ IsSelected.GetHashCode();
                    hashCode = (hashCode * 397) ^ IsChecked.GetHashCode();
                    return hashCode;
                }
            }
        }

        private readonly HashSet<SelectionCheckEvent> RecentSelectionAndCheckEvents = new HashSet<SelectionCheckEvent>();

        private void HandleAfterSelectionAndCheckChangedEvents([CanBeNull] ListViewItemSelectionChangedEventArgs selectionEvent, [CanBeNull] ItemCheckedEventArgs checkedEvent)
        {
            HandleAfterSelectionAndCheckChangedEvents(new SelectionCheckEvent(selectionEvent, checkedEvent));
        }

        private void HandleAfterSelectionAndCheckChangedEvents([NotNull] SelectionCheckEvent selectionCheckEvent)
        {
            if (SaveRecentEvents) RecentSelectionAndCheckEvents.Add(selectionCheckEvent);
            if (!Initialized || IgnoreAllListViewEvents) return;
            if (!ListSelectionControl.EnableCheckBoxes && selectionCheckEvent.IsChecked != null) return; //Ignore Check Events if CheckBoxes are disabled
            SetListViewEventReaction(ListViewEventReaction.IgnoreSelectionAndCheckEvents);
            T value = default;
            bool bothSelectedAndChecked = ListSelectionControl.EnableCheckBoxes && !ListSelectionControl.EnableMultiselection;
            bool? isSelected = selectionCheckEvent.IsSelected;
            bool? isChecked = selectionCheckEvent.IsChecked;
            try
            {
                ListViewItem item = selectionCheckEvent.Item;
                if (item == null) return;
                value = ListViewObjectAdapter.GetValue(item);
                if (!bothSelectedAndChecked) return;
                if (isChecked == false) ListViewObjectAdapter.ClearSelection();
                else if (isSelected == false) ListViewObjectAdapter.UncheckAll();
                else ListViewObjectAdapter.SetOnlyOneItemSelectedAndChecked(item);
            }
            finally
            {
                UpdateGuiOnSelection();
                if (bothSelectedAndChecked)
                {
                    ItemAfterSelectionChanged?.Invoke(this, value);
                    ItemAfterCheckChanged?.Invoke(this, value);
                    if ((isSelected == true || isChecked == true) && value != null)
                    {
                        NewValidItemSelected?.Invoke(this, value);
                        NewValidItemChecked?.Invoke(this, value);
                        NewValidItemSelectedOrChecked?.Invoke(this, value);
                    }
                }
                else
                {
                    if (isSelected != null)
                    {
                        ItemAfterSelectionChanged?.Invoke(this, value);
                        if (isSelected == true && value != null)
                        {
                            NewValidItemSelected?.Invoke(this, value);
                            NewValidItemSelectedOrChecked?.Invoke(this, value);
                        }
                    }
                    if (isChecked != null)
                    {
                        ItemAfterCheckChanged?.Invoke(this, value);
                        if (isChecked == true && value != null)
                        {
                            NewValidItemChecked?.Invoke(this, value);
                            NewValidItemSelectedOrChecked?.Invoke(this, value);
                        }
                    }
                }
                ItemAfterSelectionOrCheckedChanged?.Invoke(this, value);
                ResetLastListViewEventReaction();
            }
        }

        private void OptionsButton_Click([CanBeNull] object sender, [CanBeNull] EventArgs e)
        {
            OptionsContextMenuStrip.Show(OptionsButton, new Point(1, OptionsButton.Height - 2));
        }

        private void Navigate_Click([CanBeNull] object sender, [CanBeNull] EventArgs e)
        {
            NavigationClick?.Invoke(this);
        }

        #endregion
    }
}
