#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using CSharpUtilsNETStandard.Utils.Collections;
using CSharpUtilsNETStandard.Utils.Extensions.Collections;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework.GUI.ControlAdapters.AbstractAdapters
{
    public abstract class ControlObjectAdapter<TControl, TControlItem, TObject, TObjectText>
    {
        public const string DefaultGroupString = "-?-nogroup-?-";

        public delegate TObjectText GetObjectTextHandler(TObject itemValue);

        public delegate string GetObjectTooltipHandler(TObject obj, TControlItem item);

        public delegate void OnAfterValuesLoadedEvent(ControlObjectAdapter<TControl, TControlItem, TObject, TObjectText> adapter);

        [NotNull]
        public TControl Control { get; private set; }
        [NotNull]
        public GetObjectTextHandler ObjectTextHandler { get; private set; }
        [NotNull]
        public List<TObject> LoadedValues => ObjectControlItemBiMap.Keys.ToList();

        [NotNull]
        public ICollection<TObject> LoadedValuesAsCollection => ObjectControlItemBiMap.Keys;
        //TODO protected abstract bool Initialized { get; }

        public OnAfterValuesLoadedEvent OnAfterValuesLoaded;

        [NotNull]
        private readonly BiDictionary<TObject, TControlItem> ObjectControlItemBiMap = new BiDictionary<TObject, TControlItem>();

        protected ControlObjectAdapter([NotNull]TControl control, [CanBeNull] GetObjectTextHandler objectTextHandler = null)
        {
            Control = control;
            ObjectTextHandler = objectTextHandler ?? CreateDefaultObjectTextHandler;
        }

        protected ControlObjectAdapter([NotNull]TControl control, [NotNull, ItemNotNull] IEnumerable<TObject> values, [CanBeNull] GetObjectTextHandler objectTextHandler = null)
            : this(control, objectTextHandler)
        {
            LoadValues(values);
        }

        protected ControlObjectAdapter([NotNull]TControl control, [NotNull, ItemNotNull] IEnumerable<TObject> values, [NotNull] TObject selectedValue, [CanBeNull] GetObjectTextHandler objectTextHandler = null)
            : this(control, objectTextHandler)
        {
            LoadValues(values, selectedValue);
        }

        [NotNull]
        protected abstract TObjectText CreateDefaultObjectTextHandler([NotNull]TObject obj);

        #region Load Values
        [NotNull]
        protected abstract Dictionary<TObject, TControlItem> LoadValuesToControl([NotNull, ItemNotNull]IEnumerable<GroupedObject<TObject>> values);
        [NotNull]
        protected abstract Dictionary<TObject, TControlItem> LoadValuesToControl([NotNull, ItemNotNull]IEnumerable<TObject> values);

        public void LoadValues([NotNull, ItemNotNull]IEnumerable<TObject> values)
        {
            ObjectControlItemBiMap.Clear();
            ClearControlItems();
            Dictionary<TObject, TControlItem> valueToItemMap = LoadValuesToControl(values);
            UpdateBiMap(valueToItemMap);
        }

        public void LoadValues([NotNull, ItemNotNull]IEnumerable<GroupedObject<TObject>> values)
        {
            ObjectControlItemBiMap.Clear();
            ClearControlItems();
            Dictionary<TObject, TControlItem> valueToItemMap = LoadValuesToControl(values);
            UpdateBiMap(valueToItemMap);
        }

        private void UpdateBiMap([NotNull] Dictionary<TObject, TControlItem> valueToItemMap)
        {
            ObjectControlItemBiMap.AddRange(valueToItemMap);
            // Invoke Post-Load event (if any listener)
            OnAfterValuesLoaded?.Invoke(this);
        }

        public void LoadValues([NotNull, ItemNotNull]IEnumerable<TObject> values, [NotNull] TObject selectedValue)
        {
            LoadValues(values);
            SetSelectedValue(selectedValue);
        }

        public void LoadValues([NotNull, ItemNotNull] IEnumerable<TObject> values, int selectedIndex)
        {
            LoadValues(values);
            SetSelectedIndex(selectedIndex);
        }

        /// <summary>
        /// Clears loaded values by loading an empty List.
        /// </summary>
        public void ClearLoadedValues()
        {
            LoadValues(Enumerable.Empty<TObject>());
        }

        public abstract void ClearControlItems();

        # endregion

        # region Get Values

        [CanBeNull]
        public TObject GetValue(int index)
        {
            TControlItem controlItem = GetControlItem(index);
            return controlItem == null ? default : ObjectControlItemBiMap[controlItem];
        }

        [CanBeNull]
        public TObject GetValue([NotNull] TControlItem item) => ObjectControlItemBiMap[item];

        [NotNull, ItemNotNull]
        public IEnumerable<TObject> GetValues([NotNull] IEnumerable<TControlItem> controlItems) => controlItems.Select(GetValue).ItemNotNull();

        [CanBeNull]
        public TControlItem GetControlItem([NotNull]TObject obj) => ObjectControlItemBiMap[obj];

        [CanBeNull]
        public abstract TControlItem GetControlItem(int index);

        protected abstract int GetControlItemIndex([NotNull]TControlItem item);

        private int GetControlItemIndex([NotNull] TObject valueAssociatedToControlItem)
        {
            TControlItem controlItem = ObjectControlItemBiMap[valueAssociatedToControlItem];
            if (controlItem == null) return -1;
            return GetControlItemIndex(controlItem);
        }

        # region Convenience Getters

        [NotNull, ItemNotNull]
        public IEnumerable<TControlItem> GetControlItems() => ObjectControlItemBiMap.Values;

        /// <summary>
        /// Returns the control-items filtered by the condition
        /// specified for their respectively mapped values (see <see cref="GetValue(TControlItem)"/>).
        /// </summary>
        /// <param name="whereObj"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public List<TControlItem> GetControlItemsByValue([NotNull] Func<TObject, bool> whereObj)
        {
            return ObjectControlItemBiMap.Keys.Where(whereObj).Select(obj => ObjectControlItemBiMap[obj]).ToList();
        }

        #endregion

        #endregion


        [CanBeNull]
        public TObject GetSelectedValue()
        {
            int index = GetSelectedIndex();
            return index >= 0 ? GetValue(index) : default;
        }

        public void SetSelectedValue([CanBeNull] TObject value)
        {
            if (value == null) return;
            int index = GetControlItemIndex(value);
            SetSelectedIndex(index);
        }

        public abstract int GetSelectedIndex();
        public abstract void SetSelectedIndex(int index);
    }

    public sealed class GroupedObject<TObject>
    {
        public const string DefaultGroupString = "-?-nogroup-?-";
        [CanBeNull]
        public readonly object Tag;
        [NotNull]
        public readonly string GroupName;
        [NotNull, ItemNotNull]
        public readonly IReadOnlyList<TObject> GroupObjects;

        public GroupedObject([NotNull, ItemNotNull]IReadOnlyList<TObject> groupObjects, [CanBeNull] object tag = null)
        {
            GroupName = DefaultGroupString;
            GroupObjects = groupObjects;
            Tag = tag;
        }

        public GroupedObject([NotNull]string groupName, [NotNull, ItemNotNull]IReadOnlyList<TObject> groupObjects, [CanBeNull] object tag = null)
        {
            GroupName = groupName;
            GroupObjects = groupObjects;
            Tag = tag;
        }
    }
}
