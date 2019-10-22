#region Imports

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CSharpUtilsNETStandard.Utils.Extensions.Collections;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework.Controls
{

    public static class ControlExtensions
    {
        [CanBeNull]
        public static T NullIfDisposed<T>([NotNull] this T control) where T : Control => control.IsDisposed ? null : control;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

        private const int WM_SETREDRAW = 11;

        public static void SuspendDrawing([NotNull]this Control parent)
        {
            SendMessage(parent.Handle, WM_SETREDRAW, false, 0);
        }

        public static void ResumeDrawing([NotNull]this Control parent)
        {
            SendMessage(parent.Handle, WM_SETREDRAW, true, 0);
            parent.Refresh();
        }

        public static float GetScalingFactorYAxis([NotNull]this Control control)
        {
            float scalingFactor = control.CreateGraphics().DpiY / 96;
            return scalingFactor;
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<Control> EnumerateControlsRecursive([CanBeNull]this Control parent) => parent.EnumerateControlsRecursive<Control>();

        [NotNull, ItemNotNull]
        public static IEnumerable<T> EnumerateControlsRecursive<T>([CanBeNull]this Control parent) where T : Control
        {
            if (parent == null) yield break;
            foreach (Control child in parent.Controls)
            {
                if (child is T tChild) yield return tChild;
                foreach (T descendant in child.EnumerateControlsRecursive<T>())
                    yield return descendant;
            }
        }

        public static bool AreBoundsVisibleOnScreen(this Rectangle bounds) =>
            IsPointVisible(new Point(bounds.Left, bounds.Top))
            && IsPointVisible(new Point(bounds.Right, bounds.Top))
            && IsPointVisible(new Point(bounds.Right, bounds.Bottom))
            && IsPointVisible(new Point(bounds.Left, bounds.Bottom));

        private static bool IsPointVisible(this Point p)
        {
            Screen screen = Screen.FromPoint(p);
            return screen.Bounds.Contains(p);
        }

        public static void InvokeIfRequiredAndNotDisposed([NotNull] this Control control, MethodInvoker action)
        {
            if (control.IsDisposed) return;
            //InvokeRequired will create a Handle which we may not want yet because it will force ownership of the control onto the current thread.
            //Therefore we first check if a Handle exists because if there is no Handle then there is definitely no InvokeRequired
            //The action could create a Handle but the developer needs to take care of that
            if (control.IsHandleCreated && control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }

        public static void RefreshStringListBox([NotNull]this ListBox listBox, [NotNull, ItemNotNull]IList<string> elements)
        {
            if (listBox.Items.Cast<object>().All(item => elements.Contains(item.ToString()) && listBox.Items.IndexOf(item) == elements.IndexOf(item.ToString())) && listBox.Items.Count == elements.Count) return;
            if (listBox.SelectionMode != SelectionMode.One) return;
            object selectedItem = listBox.SelectedItem;
            listBox.Items.Clear();
            listBox.Items.AddRange(elements.ToArray<object>());
            if (selectedItem != null && listBox.Items.Contains(selectedItem)) listBox.SelectedItem = selectedItem;
            else if (listBox.Items.Count > 0)
                listBox.SelectedIndex = 0;
        }

        /// <summary>
        /// For a ComboBox that contains strings.
        /// Does not cause an exception if the ComboBox previously contained objects that are not strings.
        /// The refresh only happens if the current ComboBox Item collection differs from the list of elements or if the refresh is forced.
        /// If the ComboBox contained Objects that are not Strings then they are replaced by Strings.
        /// </summary>
        /// <param name="comboBox">The ComboBox that is refreshed.</param>
        /// <param name="elementList">The elements with which to refresh the ComboBox.</param>
        /// <param name="nextSelection">The next element that will be selected by the ComboBox. If it is null or not in the list of elements then</param>
        /// <param name="forceSelection">If this parameter is true (default), then in case the next selection parameter is null or not possible (e.g. not in the list of elements) and the list of elements has at least one element, then the previous selection of the ComboBox is used as the next selection and if it is also null or not possible, then the first element will be selected. Otherwise nothing is selected.</param>
        /// <param name="ignoreOrder">If this parameter is true then the order of ComboBox Item collection can differ from the order of the list of elements as long as they contain the same elements. Otherwise (default) the order is considered when comparing the two.</param>
        /// <param name="forceRefresh">If this parameter is true then the ComboBox is always refreshed. Otherwise (default) the ComboBox is only refreshed if the ComboBox Item collection differs from the list of elements parameter.</param>
        public static void RefreshStringComboBox([NotNull]this ComboBox comboBox, [NotNull, ItemNotNull]IList<string> elementList, [CanBeNull]string nextSelection = null, bool forceSelection = true, bool ignoreOrder = false, bool forceRefresh = false)
        {
            if (!forceRefresh && comboBox.Items.Cast<object>().Select(item => item.ToString()).ContentEquals(elementList, ignoreOrder: ignoreOrder)) return;
            string selectedItem = null;
            if (nextSelection != null && elementList.Contains(nextSelection)) selectedItem = nextSelection;
            else if (forceSelection)
            {
                if (comboBox.SelectedItem != null && elementList.Contains(comboBox.SelectedItem.ToString())) selectedItem = comboBox.SelectedItem.ToString();
                else if (elementList.Count > 0) selectedItem = elementList[0];
            }
            comboBox.Items.Clear();
            comboBox.Items.AddRange(elementList.ToArray<object>());
            comboBox.SelectedItem = selectedItem;
        }

        public static void RefreshGenericComboBox<T>([NotNull]this ComboBox comboBox, [NotNull, ItemNotNull]IList<T> elementList, bool useNextSelection = false, [CanBeNull]T nextSelection = default, [CanBeNull]Func<T, string> stringRepresentationFunc = null, bool forceSelection = true, bool ignoreOrder = false, bool forceRefresh = false) where T : IEquatable<T>
        {
            if (!forceRefresh && comboBox.Items.Cast<T>().ContentEquals(elementList, ignoreOrder: ignoreOrder)) return;
            bool useSelectedItem = false;
            T selectedItem = default;
            if (useNextSelection && nextSelection != null && elementList.Contains(nextSelection))
            {
                useSelectedItem = true;
                selectedItem = nextSelection;
            }
            else if (forceSelection)
            {
                T currentSelection = (T)comboBox.SelectedItem;
                if (currentSelection != null && elementList.Contains(currentSelection))
                {
                    useSelectedItem = true;
                    selectedItem = currentSelection;
                }
                else if (elementList.Count > 0)
                {
                    useSelectedItem = true;
                    selectedItem = elementList[0];
                }
            }
            comboBox.Items.Clear();
            comboBox.Items.AddRange(stringRepresentationFunc == null ? elementList.Cast<object>().ToArray() : elementList.Select(x => new StringWrapper<T>(x, stringRepresentationFunc)).ToArray<object>());
            if (useSelectedItem) comboBox.SelectedItem = selectedItem;
        }

        /// <summary>
        /// Uses a defined ToString method instead of the regular ToString method of T.
        /// Useful for wrapping objects where you want a different behavior of the ToString method.
        /// For GUI usage e.g. ComboBox which normally uses the default ToString method.
        /// Can implicitly (and explicitly) be cast back to T.
        /// Example:
        /// StringWrapperT wrapper = new StringWrapperT(someObjectT, x => "Hello " + x)
        /// T obj = wrapper; //Valid (implicit)
        /// T obj = (T) wrapper; //Also Valid (explicit)
        /// Equatable to T. => obj.Equals()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public sealed class StringWrapper<T> : IEquatable<StringWrapper<T>>, IEquatable<T>
        {
            public StringWrapper([NotNull]T value, [NotNull]Func<T, string> toStringFunc)
            {
                _value = value;
                _toStringFunc = toStringFunc;
            }

            [NotNull] private readonly T _value;

            [NotNull]
            private readonly Func<T, string> _toStringFunc;

            public bool Equals(StringWrapper<T> other) => other != null && _value.Equals(other._value);

            public bool Equals(T other) => other != null && _value.Equals(other);

            public override bool Equals(object obj)
            {
                StringWrapper<T> wrapper = obj as StringWrapper<T>;
                if (wrapper != null) return _value.Equals(wrapper._value);
                return obj is T && _value.Equals(obj);
            }

            public override int GetHashCode() => _value.GetHashCode();

            public override string ToString() => _toStringFunc(_value) ?? "";

            public static bool operator ==([CanBeNull] StringWrapper<T> wrapper1, [CanBeNull] StringWrapper<T> wrapper2) => Equals(wrapper1, wrapper2);

            public static bool operator ==([CanBeNull] StringWrapper<T> wrapper, [CanBeNull] T obj) => Equals(wrapper, obj);

            public static bool operator ==([CanBeNull] T obj, [CanBeNull] StringWrapper<T> wrapper) => wrapper == obj;

            public static bool operator !=([CanBeNull] StringWrapper<T> wrapper1, [CanBeNull] StringWrapper<T> wrapper2) => !(wrapper1 == wrapper2);

            public static bool operator !=([CanBeNull] StringWrapper<T> wrapper, [CanBeNull] T obj) => !(wrapper == obj);

            public static bool operator !=([CanBeNull] T obj, [CanBeNull] StringWrapper<T> wrapper) => !(wrapper == obj);

            [CanBeNull]
            public static implicit operator T([CanBeNull] StringWrapper<T> wrapper) => wrapper == null ? default : wrapper._value;
        }
    }
}
