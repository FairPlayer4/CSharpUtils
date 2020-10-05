#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CSharpUtilsNETFramework.GUI.ControlAdapters.AbstractAdapters;
using CSharpUtilsNETStandard.Utils;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework.GUI.ControlAdapters
{
    [PublicAPI]
    public class ComboboxObjectAdapter<TObject> : ControlObjectAdapter<ComboBox, object, TObject, string>
    {
        public delegate void SelectionChangedHandler(ComboboxObjectAdapter<TObject> adapter, TObject selectedItem);

        # region Constructors

        public ComboboxObjectAdapter([NotNull] ComboBox control, [CanBeNull] GetObjectTextHandler objectTextHandler = null)
            : base(control, objectTextHandler) { }

        public ComboboxObjectAdapter([NotNull] ComboBox control, [NotNull] IEnumerable<TObject> values, [CanBeNull] GetObjectTextHandler objectTextHandler = null)
            : base(control, values, objectTextHandler) { }

        public ComboboxObjectAdapter([NotNull] ComboBox control, [NotNull] IEnumerable<TObject> values, [NotNull] TObject selectedValue, [CanBeNull] GetObjectTextHandler objectTextHandler = null)
            : base(control, values, selectedValue, objectTextHandler) { }

        # endregion

        protected override string CreateDefaultObjectTextHandler(TObject obj) => obj + "";

        protected override Dictionary<TObject, object> LoadValuesToControl(IEnumerable<GroupedObject<TObject>> values)
        {
            var objectValues = values.FirstOrDefault(x => x.GroupName == DefaultGroupString);
            //TODO
            if (objectValues == null) throw new ArgumentException("This ComboboxObjectAdapter does not support groups!");
            return LoadValuesToControl(objectValues.GroupObjects);
        }

        protected override Dictionary<TObject, object> LoadValuesToControl(IEnumerable<TObject> values)
        {
            // Mapping does not work with ComboBox.Sorted enabled. Showing warning and disabling sorting.
            if (Control.Sorted)
            {
                Logger.PrintWarning("ComboBox.Sorted returns TRUE! The indexes of elements will constantly be changing and value-to-item mapping will most certainly fail! The adapter will therefore disable the sorting in order to allow mapping. Please sort the values before adding them to the ComboBox if you wish to have a sorted list.");

                Control.Sorted = false;
            }

            Dictionary<TObject, object> valuesToItemMap = new Dictionary<TObject, object>();

            // Load items
            foreach (TObject obj in values)
            {
                int index = Control.Items.Add(ObjectTextHandler(obj));

                valuesToItemMap[obj] = Control.Items[index];
            }
            return valuesToItemMap;
        }

        public override int GetSelectedIndex() => Control.SelectedIndex;

        public override void SetSelectedIndex(int index)
        {
            Control.SelectedIndex = index;
        }

        public override void ClearControlItems()
        {
            Control.Items.Clear();
        }

        public override object GetControlItem(int index) => Control.Items[index];

        protected override int GetControlItemIndex(object item) => Control.Items.IndexOf(item);

        public void AddSelectedIndexChangedListener([NotNull] SelectionChangedHandler selectionChangedHandler)
        {
            Control.SelectedIndexChanged += (sender, args) => selectionChangedHandler(this, GetSelectedValue());
        }

        public void AddSelectedValueChangedListener([NotNull] SelectionChangedHandler selectionChangedHandler)
        {
            Control.SelectedValueChanged += (sender, args) => selectionChangedHandler(this, GetSelectedValue());
        }
    }
}
