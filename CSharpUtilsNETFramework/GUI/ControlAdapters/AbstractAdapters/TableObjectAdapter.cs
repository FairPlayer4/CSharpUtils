#region Imports

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework.GUI.ControlAdapters.AbstractAdapters
{
    public abstract class TableObjectAdapter<TControl, TControlItem, TObject, TObjectText> : ControlObjectAdapter<TControl, TControlItem, TObject, TObjectText>
    {
        [NotNull]
        protected abstract IEnumerable<int> SelectedIndexes { get; }

        protected abstract void SetValuesSelected([NotNull, ItemNotNull]IEnumerable<TObject> values, bool selected);
        public abstract void ClearSelection();

        public abstract void SetColumnHeaders(params string[] headerNames);

        [NotNull, ItemNotNull]
        public IEnumerable<TObject> SelectedValues
        {
            get => SelectedIndexes.Select(GetValue);
            set {
                ClearSelection();
                SetValuesSelected(value, true);
            }
        }

        [NotNull, ItemNotNull]
        public List<TObject> GetSelectedValues()
        {
            return SelectedValues.ToList();
        }

        # region Constructors

        protected TableObjectAdapter([NotNull] TControl control, [CanBeNull] GetObjectTextHandler objectTextHandler = null)
            : base(control, objectTextHandler)
        {
        }

        protected TableObjectAdapter([NotNull] TControl control, [NotNull, ItemNotNull] IEnumerable<TObject> values, [CanBeNull] GetObjectTextHandler objectTextHandler = null)
            : base(control, values, objectTextHandler)
        {
        }

        protected TableObjectAdapter([NotNull] TControl control, [NotNull, ItemNotNull] IEnumerable<TObject> values, [NotNull] TObject selectedValue, [CanBeNull] GetObjectTextHandler objectTextHandler = null)
            : base(control, values, selectedValue, objectTextHandler)
        {
        }

        # endregion
    }

}
