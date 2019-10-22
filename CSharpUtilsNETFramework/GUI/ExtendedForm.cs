#region Imports

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CSharpUtilsNETFramework.Controls;
using CSharpUtilsNETFramework.GUI.Util;
using CSharpUtilsNETStandard.Utils;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework.GUI
{
    // TODO Add more Standardization
    // TODO Extract TableLayoutPanel Logic eventually
    // TODO Refactor TableLayoutPanel Logic and make it usable for UserControls
    // TODO Rework Sizing Logic to be more flexible
    // TODO Add Documentation
    public partial class ExtendedForm : Form
    {
        #region Constructors

        // Set between 0 and 1 otherwise MinimumSize is not active
        protected float MinimalPercentageOfRows { get; set; }

        // Set between 0 and 1 otherwise MinimumSize is not active
        protected float MinimalPercentageOfColumns { get; set; }

        // Set this to false in your constructor if you want to force a certain minimum or maximum size
        protected bool EnableAutomaticMinimumMaximumSizeCalculation { get; set; }

        protected ExtendedForm()
        {
            InitializeComponent();

            // Properties default-values:
            MinimalPercentageOfRows = 0.5F;
            MinimalPercentageOfColumns = 0.5F;
            EnableAutomaticMinimumMaximumSizeCalculation = true;
        }

        #endregion

        # region Overridden Control Methods

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (Controls.Count != 1) return;
            // Store all TableLayoutPanels in their Design Size
            if (!this.EnumerateControlsRecursive().All(IsValid)) Logger.PrintTrace(string.Format("{0} Type: {1} has potential problems with dynamic resizing!", Name, GetType().Name));
            foreach (var tlp in this.EnumerateControlsRecursive().OfType<TableLayoutPanel>())
                StoreTLP(tlp);
            SetMinimumMaximumSize();
        }

        // Also scales ListView Columns so that they are not to small on high resolution screens
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);
            foreach (var listView in this.EnumerateControlsRecursive().OfType<ListView>())
                listView.ScaleListViewColumns(factor);
        }

        #endregion

        #region Size and Validity

        [NotNull]
        private readonly Dictionary<TableLayoutPanel, TLPManager> _tlpStorages = new Dictionary<TableLayoutPanel, TLPManager>();

        protected void SetMinimumMaximumSize()
        {
            if (MinimalPercentageOfRows < 0F || MinimalPercentageOfRows > 1F || MinimalPercentageOfColumns < 0F || MinimalPercentageOfColumns > 1F || Controls.Count != 1 || !EnableAutomaticMinimumMaximumSizeCalculation) return;
            bool heightResizable = false;
            bool widthResizable = false;
            Size calculatedMinimumSize = Size.Empty;
            this.SuspendDrawing();
            TableLayoutPanel tlp = GetTopTLP();
            if (tlp != null && TLPManager.PassedChecks(tlp))
            {
                StoreTLP(tlp);
                TLPManager tlpStore = _tlpStorages[tlp].Update();
                if (tlpStore?.IsReady == true)
                {
                    float minHeight = Height;
                    for (int row = 0; row < tlp.RowCount; row++)
                        minHeight -= tlp.RowStyles[row].SizeType != SizeType.Percent ? 0 : (1 - MinimalPercentageOfRows) * tlpStore.InitialRowPixelHeights[row];
                    float minWidth = Width;
                    for (int column = 0; column < tlp.ColumnCount; column++)
                        minWidth -= tlp.ColumnStyles[column].SizeType != SizeType.Percent ? 0 : (1 - MinimalPercentageOfColumns) * tlpStore.InitialColumnPixelWidths[column];
                    if (minWidth > 0 && minHeight > 0)
                        calculatedMinimumSize = new Size((int)minWidth, (int)minHeight);
                    if (TLPManager.ContainsPercentRows(tlp)) heightResizable = true;
                    if (TLPManager.ContainsPercentColumns(tlp)) widthResizable = true;
                    if (!heightResizable && !widthResizable)
                    {
                        SetMinMaxSize(Size, Size);
                        FormBorderStyle = FormBorderStyle.FixedDialog;
                    }
                    else if (!heightResizable) SetMinMaxSize(new Size(calculatedMinimumSize.Width, Height), new Size(100000, Height));
                    else if (!widthResizable) SetMinMaxSize(new Size(Width, calculatedMinimumSize.Height), new Size(Width, 100000));
                    else SetMinMaxSize(calculatedMinimumSize, Size.Empty);
                }
            }
            this.ResumeDrawing();
        }

        [CanBeNull]
        private TableLayoutPanel GetTopTLP()
        {
            if (Controls.Count != 1) return null;
            Control control = Controls[0];
            while (!(control is TableLayoutPanel) && control.Controls.Count == 1)
                control = control.Controls[0];
            TableLayoutPanel tlp = control as TableLayoutPanel;
            return tlp;
        }

        private bool IsValid([NotNull] Control control)
        {
            if (control.Dock == DockStyle.None && !(control is TabPage) && !string.IsNullOrWhiteSpace(control.Name))
            {
                Logger.PrintTrace(string.Format("{0} Type: {1} is not docked in Form {2} Type: {3}!", control.Name, control.GetType(), Name, GetType().Name));
                return false;
            }
            if (control.Controls.Count <= 1) return true;
            if (control is TableLayoutPanel || control is DataGridView || control is TabControl || control is SplitContainer || control is NumericUpDown) return true;
            Logger.PrintTrace(string.Format("{0} Type: {1} has multiple child controls but is not a TableLayoutPanel, DataGridView, TabControl, SplitContainer or NumericUpDown in Form {2} Type: {3}!", control.Name, control.GetType(), Name, GetType().Name));
            return false;
        }

        private void SetMinMaxSize(Size minSize, Size maxSize)
        {
            MinimumSize = minSize;
            MaximumSize = maxSize;
        }

        protected void ResetAndDisableAutomaticSizeCalculation()
        {
            _tlpStorages.Clear();
            SetMinMaxSize(Size.Empty, Size.Empty);
        }

        protected void EnableAutomaticSizeCalculation()
        {
            foreach (var tlp in this.EnumerateControlsRecursive().OfType<TableLayoutPanel>())
                StoreTLP(tlp);
            SetMinimumMaximumSize();
        }

        # endregion

        #region TableLayoutPanel Utilities

        private void StoreTLP([NotNull] TableLayoutPanel tlp)
        {
            if (_tlpStorages.ContainsKey(tlp)) return;
            TLPManager tlpManager = TLPManager.CreateTLPManager(tlp);
            if (tlpManager != null) _tlpStorages.Add(tlp, tlpManager);
        }

        protected void SetRowVisibility([NotNull] TableLayoutPanel tlp, bool visible, int row, [NotNull] params int[] rows)
        {
            List<int> rowList = new List<int>(rows) { row };
            SetRowColumnVisibility(tlp, rowList, null, visible);
        }

        protected void SetColumnVisibility([NotNull] TableLayoutPanel tlp, bool visible, int column, [NotNull] params int[] columns)
        {
            List<int> columnList = new List<int>(columns) { column };
            SetRowColumnVisibility(tlp, null, columnList, visible);
        }

        private void SetRowColumnVisibility([NotNull] TableLayoutPanel tlp, [CanBeNull] List<int> rowList, [CanBeNull] List<int> columnList, bool visible)
        {
            try
            {
                // The tlp must be visible!
                // Don't call the method in the constructor!
                // Override OnLoad() to use this method before the window is shown!
                if (!tlp.Visible)
                {
                    Logger.PrintWarning("SetRowColumnVisibility can only be used on a visible TableLayoutPanel. Make sure that the window/dialog is initialized and don't call this method in the constructor. To use it before the window/dialog is shown override the OnLoad() method.");
                    return;
                }
                Logger.PrintGarbage("SetRowColumnVisibility started.");
                var watch = System.Diagnostics.Stopwatch.StartNew();
                if (rowList == null && columnList != null) rowList = new List<int>();
                if (rowList != null && columnList == null) columnList = new List<int>();
                if (!PassedTLPChecks(tlp, rowList, columnList, visible)) return;
                // ReSharper disable AssignNullToNotNullAttribute
                List<RowStyle> rowStyleList = rowList.Select(row => tlp.RowStyles[row]).ToList();
                List<ColumnStyle> columnStyleList = columnList.Select(column => tlp.ColumnStyles[column]).ToList();
                // ReSharper restore AssignNullToNotNullAttribute
                List<Control> controlList = GetTLPAndParentControlList(tlp, rowList.Count > 0, columnList.Count > 0);
                if (controlList == null) return;
                Logger.PrintGarbage("SetRowColumnVisibility main checks passed.");
                SetMinMaxSize(Size.Empty, Size.Empty);
                StoreTLP(tlp);
                TLPManager tlpStore = _tlpStorages[tlp].Update();
                if (tlpStore == null) return;
                var sizeChange = GetSizeChange(tlp, tlpStore, rowList, columnList, visible);
                if (!sizeChange.HasValue) return;
                Logger.PrintGarbage("SetRowColumnVisibility Size Change was successfully obtained.");
                int heightChange = sizeChange.Value.Item1;
                int widthChange = sizeChange.Value.Item2;
                ApplyVisibilityAndSizeChange(tlp, tlpStore, rowList, columnList, rowStyleList, columnStyleList, controlList, heightChange, widthChange, visible);
                SetMinimumMaximumSize();
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;
                Logger.PrintTrace("Time needed to apply Row/Column Visibility: " + elapsedMs + "ms");
                Logger.PrintGarbage("SetRowColumnVisibility completed.");
            }
            catch (Exception e)
            {
                Logger.PrintError("An exception occurred while setting the row / column visibility of a TableLayoutPanel.", nameof(ExtendedForm), e);
            }
        }

        #region SetRowColumnVisibility Helper Methods

        //Modifies the two lists
        private bool PassedTLPChecks([NotNull] TableLayoutPanel tlp, [CanBeNull] List<int> rowList, [CanBeNull] List<int> columnList, bool visible)
        {
            // Checking Parameters
            if (rowList == null || columnList == null || !TLPManager.PassedChecks(tlp, rowList.ToArray(), columnList.ToArray()) || tlp.FindForm() != this || Controls.Count != 1) return false;
            rowList.Sort();
            columnList.Sort();
            // Checking that the column is not already set to the desired visibility (prevents hiding columns that contain elements that are hidden because their state can't be returned)
            for (int i = 0; i < columnList.Count; i++)
            {
                if (tlp.Controls.Cast<Control>().Where(control => tlp.GetCellPosition(control).Column == columnList[i]).Any(control => control.Visible != visible)) continue;
                columnList.RemoveAt(i);
                if (i <= columnList.Count) i--;
            }

            for (int i = 0; i < rowList.Count; i++)
            {
                if (tlp.Controls.Cast<Control>().Where(control => tlp.GetCellPosition(control).Row == rowList[i]).Any(control => control.Visible != visible)) continue;
                rowList.RemoveAt(i);
                if (i <= rowList.Count) i--;
            }

            return rowList.Count > 0 || columnList.Count > 0;
        }

        [CanBeNull]
        private List<Control> GetTLPAndParentControlList([NotNull] TableLayoutPanel tlp, bool checkRows, bool checkColumns)
        {
            List<Control> controlList = new List<Control> { tlp };
            Control childControl = tlp;
            for (Control parentControl = tlp.Parent; parentControl != this && parentControl != null; parentControl = parentControl.Parent)
            {
                if (parentControl is GroupBox groupBox && groupBox.Controls.Count != 1) return null;
                if (parentControl is UserControl userControl && userControl.Controls.Count != 1) return null;
                if (parentControl is TableLayoutPanel parentTLP)
                {
                    int childRow = parentTLP.GetCellPosition(childControl).Row;
                    int childColumn = parentTLP.GetCellPosition(childControl).Column;
                    if (checkRows && parentTLP.Controls.Cast<Control>().Any(tlpChild => tlpChild != childControl && parentTLP.GetCellPosition(tlpChild).Row == childRow))
                        return null;
                    if (checkColumns && parentTLP.Controls.Cast<Control>().Any(tlpChild => tlpChild != childControl && parentTLP.GetCellPosition(tlpChild).Column == childColumn))
                        return null;
                }
                if (!(parentControl is GroupBox || parentControl is TableLayoutPanel || parentControl is UserControl || (parentControl is Panel && !parentControl.GetType().IsSubclassOf(typeof(Panel))))) return null; // Other Controls cannot be handled
                controlList.Add(parentControl);
                childControl = parentControl;
            }
            controlList.Add(this);
            return controlList;
        }

        /// <summary>
        /// This method is complex if the TableLayoutPanel is not ready meaning that all row widths and column heights must be calculated manually.
        /// During the Load Event the TableLayoutPanel methods GetRowHeights() and GetColumnWidths() don't work properly.
        /// </summary>
        /// <param name="tlp"></param>
        /// <param name="tlpStore"></param>
        /// <param name="rowList"></param>
        /// <param name="columnList"></param>
        /// <param name="visible"></param>
        /// <returns></returns>
        [CanBeNull]
        private static (int, int)? GetSizeChange([NotNull] TableLayoutPanel tlp, [NotNull] TLPManager tlpStore, [NotNull] List<int> rowList, [NotNull] List<int> columnList, bool visible)
        {
            if (!tlpStore.IsReady) return visible ? null : GetSizeChangeManually(tlp, tlpStore, rowList, columnList);
            int heightChange = 0;
            int widthChange = 0;
            if (visible)
            {
                foreach (int row in rowList)
                {
                    if (tlpStore.VisibleRowStyles[row].SizeType != SizeType.Percent)
                        heightChange += tlpStore.InitialRowPixelHeights[row];
                    else
                        heightChange += (int)(tlpStore.VisibleRowStyles[row].Height * tlpStore.RowPixelPerPercent);
                }
                foreach (int column in columnList)
                {
                    if (tlpStore.VisibleColumnStyles[column].SizeType != SizeType.Percent)
                        widthChange += tlpStore.InitialColumnPixelWidths[column];
                    else
                        widthChange += (int)(tlpStore.VisibleColumnStyles[column].Width * tlpStore.ColumnPixelPerPercent);
                }
            }
            else
            {
                heightChange += tlp.GetRowHeights().Where((row, index) => rowList.Contains(index)).Sum();
                widthChange += tlp.GetColumnWidths().Where((column, index) => columnList.Contains(index)).Sum();
            }
            return (heightChange, widthChange);
        }

        #region TLP Manual Calculation of Row Heights and Column Widths

        [CanBeNull]
        private static (int, int)? GetSizeChangeManually([NotNull] TableLayoutPanel tlp, [NotNull] TLPManager tlpStore, [NotNull] List<int> rowList, [NotNull] List<int> columnList)
        {
            Logger.PrintTrace("SetRowColumnVisibility is applied before the TableLayoutPanel is ready.");
            int heightChange = 0;
            int widthChange = 0;
            List<int> undoneRows = new List<int>(rowList);
            List<int> undoneColumns = new List<int>(columnList);
            List<Control> controlsWithHigherRowSpan = new List<Control>();
            List<Control> controlsWithHigherColumnSpan = new List<Control>();
            bool isRowPixelPerPercentSet = false;
            bool isColumnPixelPerPercentSet = false;
            foreach (int row in rowList)
            {
                foreach (Control control in tlp.Controls)
                {
                    var cellPosition = tlp.GetCellPosition(control);
                    if (cellPosition.Row != row) continue;
                    if (tlp.GetRowSpan(control) == 1 && undoneRows.Contains(row))
                    {
                        if (tlp.RowStyles[row].SizeType == SizeType.Percent && !isRowPixelPerPercentSet)
                        {
                            RowStyle percentRowStyle = tlp.RowStyles[row];
                            tlpStore.RowPixelPerPercent = control.Height / percentRowStyle.Height;
                            isRowPixelPerPercentSet = true;
                        }
                        tlpStore.SetInitialRowPixelHeightForRow(row, control.Height);
                        undoneRows.Remove(row);
                    }
                    else if (tlp.GetRowSpan(control) != 1)
                    {
                        controlsWithHigherRowSpan.Add(control);
                    }
                }
            }
            foreach (int column in columnList)
            {
                foreach (Control control in tlp.Controls)
                {
                    var cellPosition = tlp.GetCellPosition(control);
                    if (cellPosition.Column != column) continue;
                    if (tlp.GetColumnSpan(control) == 1 && undoneColumns.Contains(column))
                    {
                        if (tlp.ColumnStyles[column].SizeType == SizeType.Percent && !isColumnPixelPerPercentSet)
                        {
                            ColumnStyle percentColumnStyle = tlp.ColumnStyles[column];
                            tlpStore.ColumnPixelPerPercent = control.Width / percentColumnStyle.Width;
                            isColumnPixelPerPercentSet = true;
                        }
                        tlpStore.SetInitialColumnPixelWidthForColumn(column, control.Width);
                        undoneColumns.Remove(column);
                    }
                    else if (tlp.GetColumnSpan(control) != 1)
                    {
                        controlsWithHigherColumnSpan.Add(control);
                    }
                }
            }
            List<int> undoneRows2 = new List<int>(undoneRows);
            List<int> undoneColumns2 = new List<int>(undoneColumns);
            List<int> finishedRows = new List<int>();
            List<int> finishedColumns = new List<int>();
            foreach (int undoneRow in undoneRows.Where(undoneRow => tlp.RowStyles[undoneRow].SizeType == SizeType.Percent))
            {
                tlpStore.SetInitialRowPixelHeightForRow(undoneRow, (int)(tlp.RowStyles[undoneRow].Height * tlpStore.RowPixelPerPercent));
                undoneRows2.Remove(undoneRow);
            }
            foreach (int undoneColumn in undoneColumns.Where(undoneColumn => tlp.ColumnStyles[undoneColumn].SizeType == SizeType.Percent))
            {
                tlpStore.SetInitialColumnPixelWidthForColumn(undoneColumn, (int)(tlp.ColumnStyles[undoneColumn].Width * tlpStore.ColumnPixelPerPercent));
                undoneColumns2.Remove(undoneColumn);
            }
            // AutoSize Rows or Columns are no longer allowed
            if (!isRowPixelPerPercentSet)
            {
                if (undoneRows2.Any(undoneRow => tlp.RowStyles[undoneRow].SizeType == SizeType.AutoSize || !undoneRows2.Contains(undoneRow + 1) && !undoneRows2.Contains(undoneRow - 1)))
                    return null;
                foreach (int undoneRow in undoneRows2)
                {
                    if (isRowPixelPerPercentSet) break;
                    foreach (var spanControl in controlsWithHigherRowSpan)
                    {
                        bool connectsRows = true;
                        for (int row = undoneRow; row < tlp.GetRowSpan(spanControl) + undoneRow; row++)
                            connectsRows = connectsRows && rowList.Contains(row);
                        if (!connectsRows) continue;
                        float sumPercentage = Enumerable.Range(undoneRow, tlp.GetRowSpan(spanControl) - 1).Sum(y => tlp.RowStyles[y].Height);
                        tlpStore.RowPixelPerPercent = tlp.RowStyles[undoneRow].Height / sumPercentage * spanControl.Height / tlp.RowStyles[undoneRow].Height;
                        isRowPixelPerPercentSet = true;
                        break;
                    }
                }
                foreach (int undoneRow in undoneRows2)
                {
                    tlpStore.SetInitialRowPixelHeightForRow(undoneRow, (int)(tlp.RowStyles[undoneRow].Height * tlpStore.RowPixelPerPercent));
                    finishedRows.Add(undoneRow);
                }
            }
            if (!isColumnPixelPerPercentSet)
            {
                if (undoneColumns2.Any(undoneColumn => tlp.ColumnStyles[undoneColumn].SizeType == SizeType.AutoSize || !undoneColumns2.Contains(undoneColumn + 1) && !undoneColumns2.Contains(undoneColumn - 1)))
                    return null;
                foreach (int undoneColumn in undoneColumns2)
                {
                    if (isColumnPixelPerPercentSet) break;
                    foreach (var spanControl in controlsWithHigherColumnSpan)
                    {
                        bool connectsColumns = true;
                        for (int column = undoneColumn; column < tlp.GetColumnSpan(spanControl) + undoneColumn; column++)
                            connectsColumns = connectsColumns && columnList.Contains(column);
                        if (!connectsColumns) continue;
                        float sumPercentage = Enumerable.Range(undoneColumn, tlp.GetColumnSpan(spanControl) - 1).Sum(y => tlp.ColumnStyles[y].Width);
                        tlpStore.ColumnPixelPerPercent = tlp.ColumnStyles[undoneColumn].Width / sumPercentage * spanControl.Width / tlp.ColumnStyles[undoneColumn].Width;
                        isColumnPixelPerPercentSet = true;
                        break;
                    }
                }
                foreach (int undoneColumn in undoneColumns2)
                {
                    tlpStore.SetInitialColumnPixelWidthForColumn(undoneColumn, (int)(tlp.ColumnStyles[undoneColumn].Width * tlpStore.ColumnPixelPerPercent));
                    finishedColumns.Add(undoneColumn);
                }
            }

            if (finishedRows.All(undoneRows2.Contains) && finishedColumns.All(undoneColumns2.Contains))
            {
                heightChange += rowList.Sum(row => tlpStore.InitialRowPixelHeights[row]);
                widthChange += columnList.Sum(column => tlpStore.InitialColumnPixelWidths[column]);
            }
            else
            {
                Logger.PrintTrace("The size change of the TableLayoutPanel could not be determined.\nThis can occur in more complex layouts when the window is initially loaded.");
                return null;
            }
            return (heightChange, widthChange);
        }

        #endregion

        private void ApplyVisibilityAndSizeChange([NotNull] TableLayoutPanel tlp, [NotNull] TLPManager tlpStore, [NotNull] List<int> rowList, [NotNull] List<int> columnList, [NotNull] List<RowStyle> rowStyleList, [NotNull] List<ColumnStyle> columnStyleList, [NotNull] List<Control> controlList, int heightChange, int widthChange, bool visible)
        {
            this.SuspendDrawing();
            List<Tuple<Control, DockStyle>> list = UndockAdjustSpanningControls(tlp, tlpStore, rowList, columnList, visible);
            foreach (Control c in tlp.Controls.Cast<Control>().Where(control => rowList.Contains(tlp.GetCellPosition(control).Row) || columnList.Contains(tlp.GetCellPosition(control).Column)))
                c.Visible = visible;
            rowStyleList.ForEach(rowStyle => rowStyle.SizeType = visible ? tlpStore.VisibleRowStyles[tlp.RowStyles.IndexOf(rowStyle)].SizeType : SizeType.AutoSize);
            columnStyleList.ForEach(columnStyle => columnStyle.SizeType = visible ? tlpStore.VisibleColumnStyles[tlp.ColumnStyles.IndexOf(columnStyle)].SizeType : SizeType.AutoSize);
            if (tlp.Parent == this)
            {
                Height += visible ? heightChange : -heightChange;
                Width += visible ? widthChange : -widthChange;
            }
            else
            {
                Dictionary<Control, DockStyle> controlDockStyles = new Dictionary<Control, DockStyle>();
                foreach (var control in controlList.Where(control => control != this))
                {
                    controlDockStyles.Add(control, control.Dock);
                    control.Dock = DockStyle.None;
                }

                foreach (var control in controlList)
                {
                    control.Height += visible ? heightChange : -heightChange;
                    control.Width += visible ? widthChange : -widthChange;
                }
                foreach (var control in controlList.Where(control => control != this))
                    control.Dock = controlDockStyles[control];
            }
            list?.ForEach(tuple => tuple.Item1.Dock = tuple.Item2);
            this.ResumeDrawing();
        }

        [CanBeNull]
        private static List<Tuple<Control, DockStyle>> UndockAdjustSpanningControls([NotNull] TableLayoutPanel tlp, [NotNull] TLPManager tlpStore, [NotNull] List<int> rowList, [NotNull] List<int> columnList, bool visible)
        {
            if (visible) return null;
            List<Tuple<Control, DockStyle>> list = new List<Tuple<Control, DockStyle>>();
            foreach (var control in tlp.Controls.Cast<Control>())
            {
                int row = tlp.GetCellPosition(control).Row;
                int column = tlp.GetCellPosition(control).Column;
                if (rowList.Contains(row) || columnList.Contains(column)) continue; // Control is made invisible anyway
                int rowSpan = tlp.GetRowSpan(control);
                int columnSpan = tlp.GetColumnSpan(control);
                int heightChange = 0;
                int widthChange = 0;
                foreach (int i in rowList)
                {
                    if (i <= row) continue;
                    if (i >= row + rowSpan) continue;
                    if (tlpStore.IsReady) heightChange -= tlp.GetRowHeights()[i];
                    else heightChange -= tlpStore.InitialRowPixelHeights[i];
                }
                foreach (int i in columnList)
                {
                    if (i <= column) continue;
                    if (i >= column + columnSpan) continue;
                    if (tlpStore.IsReady) widthChange -= tlp.GetColumnWidths()[i];
                    else widthChange -= tlpStore.InitialColumnPixelWidths[i];
                }
                if (heightChange == 0 && widthChange == 0) continue;
                list.Add(new Tuple<Control, DockStyle>(control, control.Dock));
                //int height = control.Height;
                //int width = control.Width;
                control.Dock = DockStyle.None;
                control.Height = 0;//height + heightChange;
                control.Width = 0; //width + widthChange;
            }

            return list;
        }

        #endregion

        #endregion

    }
}
