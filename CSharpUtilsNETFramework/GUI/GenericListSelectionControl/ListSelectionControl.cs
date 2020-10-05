#region Imports

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using CSharpUtilsNETFramework.Controls;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework.GUI.GenericListSelectionControl
{
    [PublicAPI]
    public enum ListViewEventReaction
    {
        ReactToAllEvents = 0,
        IgnoreSelectionEvents = 1,
        IgnoreCheckEvents = 2,
        IgnoreSelectionAndCheckEvents = 3,
        IgnoreAllListViewUserEvents = 4
    }

    /// <summary>
    /// This class is for the Designer.
    /// Do not use any properties or methods manually.
    /// Use the GenericListSelectionAdapter.
    /// </summary>
    [PublicAPI]
    public sealed partial class ListSelectionControl : UserControl
    {
        #region Events

        [CanBeNull]
        public event ListViewItemSelectionChangedEventHandler ItemAfterSelectionChanged;

        [CanBeNull]
        public event ItemCheckEventHandler ItemBeforeCheckChanged;

        [CanBeNull]
        public event ItemCheckedEventHandler ItemAfterCheckChanged;

        [CanBeNull]
        public event EventHandler NavigateClick;

        [CanBeNull]
        public event EventHandler OptionsClick;

        #endregion

        //TODO changes if columns change (dynamic difficult because of visibility changes)
        //TODO see potential fix in SimpleExportDialog
        public const int NavigationButtonColumn = 1;

        public const int OptionsButtonColumn = 2;

        [NotNull]
        public ListView ListView => listView;

        public bool IsNavigable
        {
            get => NavigationButton.Visible;
            set
            {
                NavigationButton.Visible = value;
                Realign();
            }
        }

        public bool HasOptions
        {
            get => OptionsButton.Visible;
            set
            {
                OptionsButton.Visible = value;
                Realign();
            }
        }

        public bool ShowTitle
        {
            get => TitleLabel.Visible;
            set
            {
                TitleLabel.Visible = value;
                Realign();
            }
        }

        public bool ShowDescription
        {
            get => DescriptionLabel.Visible;
            set
            {
                DescriptionLabel.Visible = value;
                Realign();
            }
        }

        [CanBeNull]
        public string Description
        {
            get => DescriptionLabel.Text;
            set => DescriptionLabel.Text = value;
        }

        [CanBeNull]
        public string Title
        {
            get => TitleLabel.Text;
            set => TitleLabel.Text = value;
        }

        [NotNull]
        public Font TitleFont
        {
            get => TitleLabel.Font;
            set => TitleLabel.Font = value;
        }

        public bool EnableMultiselection
        {
            get => listView.MultiSelect;
            set => listView.MultiSelect = value;
        }

        public bool EnableCheckBoxes
        {
            get => listView.CheckBoxes;
            set => listView.CheckBoxes = value;
        }

        public bool EnableGridLines
        {
            get => listView.GridLines;
            set => listView.GridLines = value;
        }

        public ListViewEventReaction CurrentListViewEventReaction
        {
            get
            {
                if (IgnoreAllListViewUserEventsFunction != null && IgnoreAllListViewUserEventsFunction()) return ListViewEventReaction.IgnoreAllListViewUserEvents;
                if (EventReactionStack.Count == 0) return ListViewEventReaction.ReactToAllEvents;
                if (EventReactionStack.Contains(ListViewEventReaction.IgnoreAllListViewUserEvents)) return ListViewEventReaction.IgnoreAllListViewUserEvents;
                if (EventReactionStack.Contains(ListViewEventReaction.IgnoreSelectionAndCheckEvents)) return ListViewEventReaction.IgnoreSelectionAndCheckEvents;
                bool ignoreCheck = EventReactionStack.Contains(ListViewEventReaction.IgnoreCheckEvents);
                bool ignoreSelection = EventReactionStack.Contains(ListViewEventReaction.IgnoreSelectionEvents);
                if (ignoreCheck && ignoreSelection) return ListViewEventReaction.IgnoreSelectionAndCheckEvents;
                if (ignoreCheck) return ListViewEventReaction.IgnoreCheckEvents;
                if (ignoreSelection) return ListViewEventReaction.IgnoreSelectionEvents;
                return ListViewEventReaction.ReactToAllEvents;
            }
        }

        [NotNull]
        private readonly Stack<ListViewEventReaction> EventReactionStack = new Stack<ListViewEventReaction>();

        public void SetListViewEventReaction(ListViewEventReaction eventReaction)
        {
            EventReactionStack.Push(eventReaction);
        }

        public void ResetListViewEventReaction()
        {
            EventReactionStack.Pop();
        }

        public bool IgnoreCheckEvents
        {
            get
            {
                ListViewEventReaction currentEventReaction = CurrentListViewEventReaction;
                return currentEventReaction == ListViewEventReaction.IgnoreAllListViewUserEvents || currentEventReaction == ListViewEventReaction.IgnoreSelectionAndCheckEvents || currentEventReaction == ListViewEventReaction.IgnoreCheckEvents;
            }
        }

        public bool IgnoreSelectionEvents
        {
            get
            {
                ListViewEventReaction currentEventReaction = CurrentListViewEventReaction;
                return currentEventReaction == ListViewEventReaction.IgnoreAllListViewUserEvents || currentEventReaction == ListViewEventReaction.IgnoreSelectionAndCheckEvents || currentEventReaction == ListViewEventReaction.IgnoreSelectionEvents;
            }
        }

        [CanBeNull, Browsable(false)]
        public Func<bool> IgnoreAllListViewUserEventsFunction { private get; set; }

        [Browsable(false)]
        public bool IgnoreAllListViewUserEvents => CurrentListViewEventReaction == ListViewEventReaction.IgnoreAllListViewUserEvents;

        private const int InitialTitleLabelRow = 0;

        private readonly Padding InitialTitleLabelMargin;

        public ListSelectionControl()
        {
            InitializeComponent();
            InitialTitleLabelMargin = TitleLabel.Margin;
            TabIndex = 0;
            LastButtonState = GetCurrentButtonState();
            LastLabelState = GetCurrentLabelState();
        }

        #region Internal Layout of the Control

        private enum ButtonState { Nothing, OnlyNavigation, OnlyOptions, NavigationAndOptions }

        private enum LabelState { Nothing, OnlyTitle, OnlyDescription, TitleAndDescription }

        private ButtonState GetCurrentButtonState()
        {
            if (IsNavigable) return HasOptions ? ButtonState.NavigationAndOptions : ButtonState.OnlyNavigation;
            return HasOptions ? ButtonState.OnlyOptions : ButtonState.Nothing;
        }

        private LabelState GetCurrentLabelState()
        {
            if (ShowTitle) return ShowDescription ? LabelState.TitleAndDescription : LabelState.OnlyTitle;
            return ShowDescription ? LabelState.OnlyDescription : LabelState.Nothing;
        }

        private ButtonState LastButtonState { get; set; }
        private LabelState LastLabelState { get; set; }

        private void ResetTitleLabel()
        {
            if (TLPMain.GetRow(TitleLabel) == InitialTitleLabelRow) return;
            TLPMain.SetRow(TitleLabel, InitialTitleLabelRow);
            TLPMain.SetColumnSpan(TitleLabel, 3);
            TitleLabel.Margin = InitialTitleLabelMargin;
            if (!TLPMain.Contains(DescriptionLabel)) TLPMain.Controls.Add(DescriptionLabel, 0, 1);
        }

        private void Realign()
        {
            ButtonState currentButtonState = GetCurrentButtonState();
            LabelState currentLabelState = GetCurrentLabelState();

            if (LastButtonState != currentButtonState)
            {
                UpdateColumnVisibility(IsNavigable, NavigationButtonColumn);
                UpdateColumnVisibility(HasOptions, OptionsButtonColumn);
            }
            if (LastLabelState != currentLabelState)
                switch (currentLabelState)
                {
                    case LabelState.TitleAndDescription:
                    case LabelState.OnlyDescription:
                    case LabelState.Nothing:
                        ResetTitleLabel();
                        break;
                    case LabelState.OnlyTitle:
                        if (currentButtonState == ButtonState.Nothing) ResetTitleLabel();
                        else
                        {
                            TLPMain.Controls.Remove(DescriptionLabel);
                            TLPMain.SetColumnSpan(TitleLabel, 1);
                            TLPMain.SetRow(TitleLabel, 1);
                            int rightShift = 0;
                            if (IsNavigable) rightShift += NavigationButton.Width;
                            if (HasOptions) rightShift += OptionsButton.Width;
                            TitleLabel.Margin = new Padding(InitialTitleLabelMargin.Left + rightShift, InitialTitleLabelMargin.Top, InitialTitleLabelMargin.Right, InitialTitleLabelMargin.Bottom);
                        }
                        break;
                }

            LastButtonState = GetCurrentButtonState();
            LastLabelState = GetCurrentLabelState();
        }

        private void UpdateColumnVisibility(bool visible, int column)
        {
            ColumnStyle style = TLPMain.ColumnStyles[column];
            style.SizeType = visible ? SizeType.AutoSize : SizeType.Percent;
            style.Width = 0F;
        }

        #endregion

        private readonly bool IsInDesignerMode = Process.GetCurrentProcess().ProcessName == "devenv";

        protected override void OnLoad([CanBeNull] EventArgs e)
        {
            base.OnLoad(e);
            Realign();
            LastButtonState = GetCurrentButtonState();
            LastLabelState = GetCurrentLabelState();
            if (IsInDesignerMode) return;
            AutoSizeColumns();
            Form parent = FindForm();
            if (parent != null) parent.ResizeEnd += (sender, args) => AutoSizeColumns();
        }

        private void ColumnsContextMenuStrip_Opening([CanBeNull] object sender, [NotNull] CancelEventArgs e)
        {
            Point locationOnScreen = listView.PointToScreen(new Point(0, 0));

            Point relativeMousePosition = new Point(MousePosition.X - locationOnScreen.X, MousePosition.Y - locationOnScreen.Y);

            // Are we still on the ColumnHeaders?
            if (relativeMousePosition.Y > 23) e.Cancel = true;
        }

        private void ListView_ItemAfterSelectionChanged([CanBeNull] object sender, [CanBeNull] ListViewItemSelectionChangedEventArgs e)
        {
            if (IgnoreSelectionEvents) return;
            ItemAfterSelectionChanged?.Invoke(sender, e);
        }

        private void ListView_ItemBeforeCheckChanged([CanBeNull] object sender, [NotNull] ItemCheckEventArgs e)
        {
            if (DisableChecking && EnableMultiselection) e.NewValue = e.CurrentValue;
            if (IgnoreCheckEvents) return;
            ItemBeforeCheckChanged?.Invoke(sender, e);
        }

        private void ListView_ItemAfterCheckChanged([CanBeNull] object sender, [CanBeNull] ItemCheckedEventArgs e)
        {
            if (IgnoreCheckEvents) return;
            ItemAfterCheckChanged?.Invoke(sender, e);
        }

        private void OptionsButton_Click([CanBeNull] object sender, [CanBeNull] EventArgs e)
        {
            OptionsClick?.Invoke(sender, e);
        }

        private void NavigateButton_Click([CanBeNull] object sender, [CanBeNull] EventArgs e)
        {
            NavigateClick?.Invoke(sender, e);
        }

        private bool DisableListViewRefresh { get; set; }

        public void RefreshListView()
        {
            if (DisableListViewRefresh) return;
            if (ColumnHeaderCheckBoxState != LastColumnHeaderCheckBoxState) listView.Refresh();
            LastColumnHeaderCheckBoxState = ColumnHeaderCheckBoxState;
        }

        private CheckBoxState LastColumnHeaderCheckBoxState { get; set; }

        private CheckBoxState ColumnHeaderCheckBoxState
        {
            get
            {
                if (IgnoreAllListViewUserEvents) return LastColumnHeaderCheckBoxState;
                if (listView.CheckedItems.Count == 0) return CheckBoxState.UncheckedNormal;
                return listView.CheckedItems.Count == listView.Items.Count ? CheckBoxState.CheckedNormal : CheckBoxState.MixedNormal;
            }
            set
            {
                DisableListViewRefresh = true;
                listView.BeginUpdate();
                HashSet<int> checkedIndexes = listView.CheckedIndices.Cast<int>().ToHashSet();
                bool newCheckedState = value == CheckBoxState.CheckedNormal;
                SetListViewEventReaction(ListViewEventReaction.IgnoreCheckEvents);
                int itemCount = listView.Items.Count;
                ListViewItem eventTrigger = null; //For performance reasons will only trigger one event
                for (int i = 0; i < itemCount; i++)
                {
                    if (checkedIndexes.Contains(i) == newCheckedState) continue;
                    if (eventTrigger == null) eventTrigger = listView.Items[i];
                    else listView.Items[i].Checked = newCheckedState;
                }
                ResetListViewEventReaction();
                if (eventTrigger != null) eventTrigger.Checked = newCheckedState;
                listView.EndUpdate();
                DisableListViewRefresh = false;
                RefreshListView();
            }
        }

        private CheckBoxState GetNextColumnHeaderCheckBoxState() => ColumnHeaderCheckBoxState == CheckBoxState.CheckedNormal ? CheckBoxState.UncheckedNormal : CheckBoxState.CheckedNormal;

        private int ManualCheckBoxColumnSize = -1;

        private bool AutoSizeAfterDrawingColumns = true;

        [NotNull]
        private string _checkBoxColumnName = string.Empty;

        [NotNull]
        public string CheckBoxColumnName
        {
            get => _checkBoxColumnName;
            set
            {
                _checkBoxColumnName = value;
                if (listView.Columns.Count == 0 || !EnableCheckBoxes) return;
                AutoSizeAfterDrawingColumns = true;
                listView.Columns[0].Text = EnableMultiselection ? "" : value;
            }
        }

        private void ListView_DrawColumnHeader([CanBeNull] object sender, [NotNull] DrawListViewColumnHeaderEventArgs e)
        {
            if (e.ColumnIndex == 0 && EnableMultiselection && EnableCheckBoxes && !IgnoreAllListViewUserEvents)
            {
                e.DrawBackground();
                if (!string.IsNullOrWhiteSpace(CheckBoxColumnName))
                {
                    Size textSize = TextRenderer.MeasureText(CheckBoxColumnName, listView.Font);
                    ManualCheckBoxColumnSize = 20 + textSize.Width; //Check with Scaling
                    CheckBoxRenderer.DrawCheckBox(e.Graphics,
                                                  new Point(e.Bounds.Left + 4, e.Bounds.Top + 5),
                                                  new Rectangle(e.Bounds.Left + 20, e.Bounds.Top + 5, textSize.Width, textSize.Height),
                                                  CheckBoxColumnName,
                                                  listView.Font,
                                                  false,
                                                  ColumnHeaderCheckBoxState);
                }
                else
                {
                    ManualCheckBoxColumnSize = -1;
                    CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(e.Bounds.Left + 4, e.Bounds.Top + 5), ColumnHeaderCheckBoxState);
                }

                if (AutoSizeAfterDrawingColumns)
                {
                    AutoSizeAfterDrawingColumns = false;
                    AutoSizeColumns();
                }
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        private void ListView_DrawItem([CanBeNull] object sender, [NotNull] DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void ListView_DrawSubItem([CanBeNull] object sender, [NotNull] DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void ListView_ColumnClick([CanBeNull] object sender, [NotNull] ColumnClickEventArgs e)
        {
            if (e.Column != 0 || !EnableCheckBoxes || !EnableMultiselection || IgnoreAllListViewUserEvents) return;
            ColumnHeaderCheckBoxState = GetNextColumnHeaderCheckBoxState();
        }

        private bool DisableChecking { get; set; }

        private void ListView_KeyDown([CanBeNull] object sender, [NotNull] KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.ControlKey) DisableChecking = true;
        }

        private void ListView_KeyUp([CanBeNull] object sender, [NotNull] KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.ControlKey) DisableChecking = false;
        }

        //TODO think about hidden columns
        [Browsable(false), CanBeNull]
        public IReadOnlyList<ColumnContentPriority> ColumnContentPriorities { get; set; }

        private readonly HashSet<int> UserAdjustedColumns = new HashSet<int>();

        public void ResetColumnWidths()
        {
            UserAdjustedColumns.Clear();
            AutoSizeColumns();
        }

        public void AutoSizeColumns()
        {
            int columnCount = listView.Columns.Count;
            int[] manualColumnSizes = new int[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                if (!UserAdjustedColumns.Contains(i)) manualColumnSizes[i] = -1;
                else manualColumnSizes[i] = listView.Columns[i].Width;
            }
            if (EnableCheckBoxes && !UserAdjustedColumns.Contains(0)) manualColumnSizes[0] = ManualCheckBoxColumnSize;
            listView.AutoSizeColumns(true, manualColumnSizes, ColumnContentPriorities);
        }

        private void ListView_ColumnWidthChanging([CanBeNull] object sender, [NotNull] ColumnWidthChangingEventArgs e)
        {
            UserAdjustedColumns.Add(e.ColumnIndex);
        }
    }
}
