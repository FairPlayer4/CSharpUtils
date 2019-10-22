#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CSharpUtilsNETStandard.Utils;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework.GUI.Util
{
    public sealed class TLPManager
    {
        private readonly TableLayoutPanel _tlp;
        private readonly List<RowStyle> _visibleRowStyles = new List<RowStyle>();
        private readonly List<ColumnStyle> _visibleColumnStyles = new List<ColumnStyle>();
        private readonly List<int> _initialRowPixelHeights = new List<int>();
        private readonly List<int> _initialColumnPixelWidths = new List<int>();

        [NotNull] public IReadOnlyList<RowStyle> VisibleRowStyles => _visibleRowStyles.AsReadOnly();
        [NotNull] public IReadOnlyList<ColumnStyle> VisibleColumnStyles => _visibleColumnStyles.AsReadOnly();

        [NotNull] public IReadOnlyList<int> InitialRowPixelHeights => _initialRowPixelHeights.AsReadOnly();
        [NotNull] public IReadOnlyList<int> InitialColumnPixelWidths => _initialColumnPixelWidths.AsReadOnly();

        public float RowPixelPerPercent { get; set; }
        public float ColumnPixelPerPercent { get; set; }

        private bool _replace;

        public bool IsReady => IsTLPReady(_tlp);

        public TLPManager(TableLayoutPanel tlp)
        {
            if (!PassedChecks(tlp)) return;
            _tlp = tlp;
            foreach (RowStyle rowStyle in tlp.RowStyles.Cast<RowStyle>())
            {
                _initialRowPixelHeights.Add(-1);
                _visibleRowStyles.Add(new RowStyle(rowStyle.SizeType, rowStyle.Height));
            }
            foreach (ColumnStyle columnStyle in tlp.ColumnStyles.Cast<ColumnStyle>())
            {
                _initialColumnPixelWidths.Add(-1);
                _visibleColumnStyles.Add(new ColumnStyle(columnStyle.SizeType, columnStyle.Width));
            }
            RowPixelPerPercent = 0F;
            ColumnPixelPerPercent = 0F;
            if (!IsTLPReady(tlp)) _replace = true;
            else
            {
                SetInitialPixelSize();
                RowStyle percentRowStyle = tlp.RowStyles.Cast<RowStyle>().FirstOrDefault(rStyle => rStyle.SizeType == SizeType.Percent);
                if (percentRowStyle != null) RowPixelPerPercent = tlp.GetRowHeights()[tlp.RowStyles.IndexOf(percentRowStyle)] / percentRowStyle.Height;
                ColumnStyle percentColumnStyle = tlp.ColumnStyles.Cast<ColumnStyle>().FirstOrDefault(cStyle => cStyle.SizeType == SizeType.Percent);
                if (percentColumnStyle != null) ColumnPixelPerPercent = tlp.GetColumnWidths()[tlp.ColumnStyles.IndexOf(percentColumnStyle)] / percentColumnStyle.Width;
            }
        }

        public void SetInitialRowPixelHeightForRow(int row, int height)
        {
            if (PassedChecks(_tlp, new[] { row }) && _initialRowPixelHeights[row] == -1)
                _initialRowPixelHeights[row] = height;
        }

        public void SetInitialColumnPixelWidthForColumn(int column, int width)
        {
            if (PassedChecks(_tlp, columns: new[] { column }) && _initialColumnPixelWidths[column] == -1)
                _initialColumnPixelWidths[column] = width;
        }

        private void SetInitialPixelSize()
        {
            if (!IsTLPReady(_tlp)) return;
            int[] rowHeights = _tlp.GetRowHeights();
            for (int i = 0; i < _initialRowPixelHeights.Count; i++)
                if (_initialRowPixelHeights[i] == -1)
                    _initialRowPixelHeights[i] = rowHeights[i];
            int[] columnWidths = _tlp.GetColumnWidths();
            for (int i = 0; i < _initialColumnPixelWidths.Count; i++)
                if (_initialColumnPixelWidths[i] == -1)
                    _initialColumnPixelWidths[i] = columnWidths[i];
        }

        [CanBeNull]
        public TLPManager Update()
        {
            if (!PassedChecks(_tlp)) return null;
            if (_replace && IsTLPReady(_tlp))
            {
                SetInitialPixelSize();
                _replace = false;
            }
            if (IsTLPReady(_tlp))
            {
                RowStyle percentRowStyle = _tlp.RowStyles.Cast<RowStyle>().FirstOrDefault(rStyle => rStyle.SizeType == SizeType.Percent);
                if (percentRowStyle != null) RowPixelPerPercent = _tlp.GetRowHeights()[_tlp.RowStyles.IndexOf(percentRowStyle)] / percentRowStyle.Height;
                ColumnStyle percentColumnStyle = _tlp.ColumnStyles.Cast<ColumnStyle>().FirstOrDefault(cStyle => cStyle.SizeType == SizeType.Percent);
                if (percentColumnStyle != null) ColumnPixelPerPercent = _tlp.GetColumnWidths()[_tlp.ColumnStyles.IndexOf(percentColumnStyle)] / percentColumnStyle.Width;
            }
            for (int row = 1; row < _tlp.RowCount; row++)
                if (IsRowVisible(_tlp, row))
                    _visibleRowStyles[row] = new RowStyle(_tlp.RowStyles[row].SizeType, _tlp.RowStyles[row].Height);
            for (int column = 1; column < _tlp.ColumnCount; column++)
                if (IsColumnVisible(_tlp, column))
                    _visibleColumnStyles[column] = new ColumnStyle(_tlp.ColumnStyles[column].SizeType, _tlp.ColumnStyles[column].Width);
            return this;
        }

        public static bool IsTLPReady(TableLayoutPanel tlp) => PassedChecks(tlp) && tlp.GetRowHeights().Length == tlp.RowCount && tlp.GetColumnWidths().Length == tlp.ColumnCount && tlp.GetRowHeights().Sum() == tlp.Height && tlp.GetColumnWidths().Sum() == tlp.Width;

        public static bool IsRowVisible(TableLayoutPanel tlp, int row)
        {
            return PassedChecks(tlp, new[] { row }) && tlp.RowStyles[row].SizeType == SizeType.Percent;
        }

        public static bool IsColumnVisible(TableLayoutPanel tlp, int column)
        {
            return PassedChecks(tlp, columns: new[] { column }) && tlp.ColumnStyles[column].SizeType == SizeType.Percent;
        }

        public static bool IsTLPHeightResizable(TableLayoutPanel tlp)
        {
            if (!ContainsPercentRows(tlp)) return false; // Contains Checks
            foreach (RowStyle rowStyle in tlp.RowStyles.Cast<RowStyle>().Where(rowStyle => rowStyle.SizeType == SizeType.Percent))
            {
                bool result = true;
                int row = tlp.RowStyles.IndexOf(rowStyle);
                for (int column = 0; column < tlp.ColumnCount; column++)
                {
                    TableLayoutPanel innerTLP = GetNextTLPInCell(tlp, row, column);
                    result = result && (innerTLP == null || IsTLPHeightResizable(innerTLP));
                }
                if (result) return true;
            }
            return false;
        }

        public static bool IsTLPWidthResizable(TableLayoutPanel tlp)
        {
            if (!ContainsPercentColumns(tlp)) return false; // Contains Checks
            foreach (ColumnStyle columnStyle in tlp.ColumnStyles.Cast<ColumnStyle>().Where(columnStyle => columnStyle.SizeType == SizeType.Percent))
            {
                bool result = true;
                int column = tlp.ColumnStyles.IndexOf(columnStyle);
                for (int row = 0; row < tlp.RowCount; row++)
                {
                    TableLayoutPanel innerTLP = GetNextTLPInCell(tlp, row, column);
                    result = result && (innerTLP == null || IsTLPWidthResizable(innerTLP));
                }
                if (result) return true;
            }
            return false;
        }

        [CanBeNull]
        private static TableLayoutPanel GetNextTLPInCell(TableLayoutPanel tlp, int row, int column)
        {
            if (!PassedChecks(tlp, new[] { row }, new[] { column })) return null;
            Control control = tlp.GetControlFromPosition(column, row);
            while (control != null && !(control is TableLayoutPanel))
                control = control.Controls[0];
            return control as TableLayoutPanel;
        }

        public static bool ContainsPercentRows(TableLayoutPanel tlp)
        {
            return PassedChecks(tlp) && tlp.RowStyles.Cast<RowStyle>().Count(rowStyle => rowStyle.SizeType == SizeType.Percent) > 0;
        }

        public static bool ContainsSingleEmptyPercentRow(TableLayoutPanel tlp)
        {
            if (!PassedChecks(tlp)) return false;
            if (tlp.RowStyles.Cast<RowStyle>().Count(rowStyle => rowStyle.SizeType == SizeType.Percent) != 1) return false;
            for (int row = 0; row < tlp.RowCount; row++)
            {
                if (tlp.RowStyles[row].SizeType != SizeType.Percent) continue;
                return !TLPRowContainsControl(tlp, row);
            }
            return false;
        }

        public static bool TLPRowContainsControl(TableLayoutPanel tlp, int row)
        {
            return PassedChecks(tlp, new[] { row }) && TLPRowContainsControlNoChecks(tlp, row);
        }

        public static bool ContainsPercentColumns(TableLayoutPanel tlp)
        {
            return PassedChecks(tlp) && tlp.ColumnStyles.Cast<ColumnStyle>().Count(columnStyle => columnStyle.SizeType == SizeType.Percent) > 0;
        }

        public static bool ContainsSingleEmptyPercentColumn(TableLayoutPanel tlp)
        {
            if (!PassedChecks(tlp)) return false;
            if (tlp.ColumnStyles.Cast<ColumnStyle>().Count(columnStyle => columnStyle.SizeType == SizeType.Percent) != 1) return false;
            for (int column = 0; column < tlp.ColumnCount; column++)
            {
                if (tlp.ColumnStyles[column].SizeType != SizeType.Percent) continue;
                return !TLPColumnContainsControl(tlp, column);
            }
            return false;
        }

        public static bool TLPColumnContainsControl(TableLayoutPanel tlp, int column)
        {
            return PassedChecks(tlp, columns: new[] { column }) && TLPColumnContainsControlNoChecks(tlp, column);
        }

        private static bool TLPRowContainsControlNoChecks([NotNull] TableLayoutPanel tlp, int row, bool printControls = false)
        {
            bool result = false;
            foreach (var control in tlp.Controls.Cast<Control>())
            {
                int controlRow = tlp.GetCellPosition(control).Row;
                int controlRowSpan = tlp.GetRowSpan(control);
                if (!printControls)
                {
                    if (controlRow <= row && controlRow + controlRowSpan - 1 >= row) return true;
                }
                else
                {
                    if (controlRow > row || controlRow + controlRowSpan - 1 < row) continue;
                    result = true;
                }
            }
            return result;
        }

        private static bool TLPColumnContainsControlNoChecks([NotNull] TableLayoutPanel tlp, int column, bool printControls = false)
        {
            bool result = false;
            foreach (var control in tlp.Controls.Cast<Control>())
            {
                int controlColumn = tlp.GetCellPosition(control).Column;
                int controlColumnSpan = tlp.GetColumnSpan(control);
                if (!printControls)
                {
                    if (controlColumn <= column && controlColumn + controlColumnSpan - 1 >= column) return true;
                }
                else
                {
                    if (controlColumn > column || controlColumn + controlColumnSpan - 1 < column) continue;
                    result = true;
                }
            }
            return result;
        }

        public static bool PassedChecks([CanBeNull] TableLayoutPanel tlp, [CanBeNull] int[] rows = null, [CanBeNull] int[] columns = null)
        {
            try
            {
                // Fast Route if everything is fine.
                if (tlp != null && tlp.RowStyles.Count == tlp.RowCount && tlp.ColumnStyles.Count == tlp.ColumnCount &&
                tlp.RowStyles.Cast<RowStyle>().All(rowStyle => rowStyle.SizeType != SizeType.Absolute) &&
                tlp.ColumnStyles.Cast<ColumnStyle>().All(columnStyle => columnStyle.SizeType != SizeType.Absolute) &&
                (rows == null || rows.All(row => row > 0 && row < tlp.RowCount)) &&
                (columns == null || columns.All(column => column > 0 && column <= tlp.ColumnCount)))
                    return true;
                return PassedChecksWithRecovery(tlp, rows, columns);
            }
            catch (Exception e)
            {
                Logger.PrintError("An exception occurred during checking!", "TLP", e);
            }
            return false;
        }

        // Absolute RowStyles/ColumnStyles are not allowed
        private static bool PassedChecksWithRecovery([CanBeNull] TableLayoutPanel tlp, [CanBeNull] int[] rows = null, [CanBeNull] int[] columns = null)
        {
            if (tlp == null)
            {
                Logger.PrintError("The TableLayoutPanel argument was null!", "TLP", new ArgumentNullException(nameof(tlp)));
                return false;
            }
            if (tlp.RowStyles.Count != tlp.RowCount)
            {
                if (tlp.RowStyles.Count <= tlp.RowCount)
                    Logger.PrintWarning(string.Format("The RowCount: {0} is higher than the number of RowStyles: {1}!\nThe RowCount will be adjusted now to avoid further issues!", tlp.RowCount, tlp.RowStyles.Count), "TLP");
                if (tlp.RowStyles.Count >= tlp.RowCount)
                {
                    Logger.PrintTrace(string.Format("The number of RowStyles: {0} is higher than the RowCount: {1}!", tlp.RowStyles.Count, tlp.RowCount), "TLP");
                    List<int> rowStylesToBeRemoved = new List<int>();
                    for (int rowStyle = tlp.RowCount; rowStyle < tlp.RowStyles.Count; rowStyle++)
                    {
                        if (tlp.RowStyles[rowStyle].SizeType != SizeType.Absolute) continue;
                        Logger.PrintTrace(string.Format("The {0}. RowStyle ({1}, {2}) is likely to be a auto inserted RowStyle caused by a unknown bug in Visual Studio!\nIt will removed to avoid later issues!", rowStyle, tlp.RowStyles[rowStyle].SizeType, tlp.RowStyles[rowStyle].Height), "TLP");
                        rowStylesToBeRemoved.Add(rowStyle);
                    }
                    rowStylesToBeRemoved.Sort();
                    int adjustIndex = 0;
                    foreach (int rowStyle in rowStylesToBeRemoved)
                    {
                        tlp.RowStyles.RemoveAt(rowStyle - adjustIndex);
                        adjustIndex++;
                    }
                }
                tlp.RowCount = tlp.RowStyles.Count;
            }

            if (tlp.ColumnStyles.Count != tlp.ColumnCount)
            {
                if (tlp.ColumnStyles.Count <= tlp.ColumnCount)
                    Logger.PrintWarning(string.Format("The ColumnCount: {0} is higher than the number of ColumnStyles: {1}!\nThe ColumnCount will be adjusted now to avoid further issues!", tlp.ColumnCount, tlp.ColumnStyles.Count), "TLP");
                if (tlp.ColumnStyles.Count >= tlp.ColumnCount)
                {
                    Logger.PrintTrace(string.Format("The number of ColumnStyles: {0} is higher than the ColumnCount: {1}!", tlp.ColumnStyles.Count, tlp.ColumnCount), "TLP");
                    List<int> columnStylesToBeRemoved = new List<int>();
                    for (int columnStyle = tlp.ColumnCount; columnStyle < tlp.ColumnStyles.Count; columnStyle++)
                    {
                        if (tlp.ColumnStyles[columnStyle].SizeType != SizeType.Absolute) continue;
                        Logger.PrintTrace(string.Format("The {0}. ColumnStyle ({1}, {2}) is likely to be a auto inserted ColumnStyle caused by a unknown bug in Visual Studio!\nIt will removed to avoid later issues!", columnStyle, tlp.ColumnStyles[columnStyle].SizeType, tlp.ColumnStyles[columnStyle].Width), "TLP");
                        columnStylesToBeRemoved.Add(columnStyle);
                    }
                    columnStylesToBeRemoved.Sort();
                    int adjustIndex = 0;
                    foreach (int columnStyle in columnStylesToBeRemoved)
                    {
                        tlp.ColumnStyles.RemoveAt(columnStyle - adjustIndex);
                        adjustIndex++;
                    }
                }
                tlp.ColumnCount = tlp.ColumnStyles.Count;
            }

            for (int row = 0; row < tlp.RowCount; row++)
            {
                if (tlp.RowStyles[row].SizeType != SizeType.Absolute) continue;
                if (TLPRowContainsControlNoChecks(tlp, row, true))
                {
                    Logger.PrintTrace(string.Format("The {0}. RowStyle ({1}, {2}) has the SizeType Absolute and contains Controls!\nPlease change the SizeType to AutoSize or Percent to avoid issues!\nThe SizeType will be changed to AutoSize!", row, tlp.RowStyles[row].SizeType, tlp.RowStyles[row].Height), "TLP");
                    tlp.RowStyles[row].SizeType = SizeType.AutoSize; //TODO change eventually
                }
                else
                {
                    Logger.PrintTrace(string.Format("The {0}. RowStyle ({1}, {2}) has the SizeType Absolute but does not contain Controls.\nThis means the row is used as spacer row.\nBe aware that scaling may cause issues.", row, tlp.RowStyles[row].SizeType, tlp.RowStyles[row].Height), "TLP");
                }
            }

            for (int column = 0; column < tlp.ColumnCount; column++)
            {
                if (tlp.ColumnStyles[column].SizeType != SizeType.Absolute) continue;
                if (TLPColumnContainsControlNoChecks(tlp, column, true))
                {
                    Logger.PrintTrace(string.Format("The {0}. ColumnStyle ({1}, {2}) has the SizeType Absolute and contains Controls!\nPlease change the SizeType to AutoSize or Percent to avoid issues!\nThe SizeType will be changed to AutoSize!", column, tlp.ColumnStyles[column].SizeType, tlp.ColumnStyles[column].Width), "TLP");
                    tlp.ColumnStyles[column].SizeType = SizeType.AutoSize; //TODO change eventually
                }
                else
                {
                    Logger.PrintTrace(string.Format("The {0}. ColumnStyle ({1}, {2}) has the SizeType Absolute but does not contain Controls!\nThis means the column is used as spacer column.\nBe aware that scaling may cause issues.", column, tlp.ColumnStyles[column].SizeType, tlp.ColumnStyles[column].Width), "TLP");
                }
            }

            if (rows != null && rows.Any(row => row < 0 || row >= tlp.RowCount))
            {
                Logger.PrintError(string.Format("One or more rows in [{0}] is < 0 or >= RowCount: {1}!", string.Join(", ", rows), tlp.RowCount), "TLP", new ArgumentException("rows"));
                return false;
            }

            if (columns != null && columns.Any(column => column < 0 || column >= tlp.ColumnCount))
            {
                Logger.PrintError(string.Format("One or more columns in [{0}] is < 0 or >= ColumnCount: {1}!", string.Join(", ", columns), tlp.ColumnCount), "TLP", new ArgumentException("columns"));
                return false;
            }

            if (tlp.RowStyles.Cast<RowStyle>().Any(rowStyle => rowStyle.SizeType == SizeType.Absolute) ||
                tlp.ColumnStyles.Cast<ColumnStyle>().Any(columnStyle => columnStyle.SizeType == SizeType.Absolute))
            {
                Logger.PrintTrace("TLP did not pass checks! Check Trace for potential problems.", "TLP");
                return false;
            }

            return true;
        }

    }
}
