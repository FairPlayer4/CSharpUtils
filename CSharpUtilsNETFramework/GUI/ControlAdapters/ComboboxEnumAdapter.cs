#region Imports

using System;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework.GUI.ControlAdapters
{
    public class ComboboxEnumAdapter<TEnum>
    {
        public delegate string GetEnumValueTextHandler(TEnum enumValue);

        public readonly GetEnumValueTextHandler EnumValueTextHandler;

        public ComboboxEnumAdapter(ComboBox combo, TEnum initialValue, [CanBeNull] GetEnumValueTextHandler enumValueTextHandler = null)
        {
            Combo = combo;
            EnumType = typeof(TEnum);
            EnumValues = (TEnum[])Enum.GetValues(EnumType);
            EnumValueTextHandler = EnumValueTextHandler ?? (enumValue => enumValue.ToString());

            foreach (TEnum val in EnumValues)
            {
                Combo.Items.Add(EnumValueTextHandler(val));
            }

            // Select default-value
            SetSelectedValue(initialValue);
        }

        public ComboBox Combo { get; private set; }
        public Type EnumType { get; private set; }
        public TEnum[] EnumValues { get; private set; }

        public void SetSelectedValue(TEnum value)
        {
            Combo.SelectedIndex = EnumValues.ToList().IndexOf(value);
        }

        public TEnum GetSelectedValue() => EnumValues[Combo.SelectedIndex];

        public int GetSelectedIndex() => Combo.SelectedIndex;

        public void SetSelectedIndex(int index)
        {
            Combo.SelectedIndex = index;
        }

        public bool IsSelectedValueValid() => GetSelectedIndex() >= 0 && GetSelectedIndex() < EnumValues.Length;
    }
}
