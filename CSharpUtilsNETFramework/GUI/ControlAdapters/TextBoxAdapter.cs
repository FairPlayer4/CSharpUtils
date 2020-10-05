#region Imports

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CSharpUtilsNETStandard.Utils;
using CSharpUtilsNETStandard.Utils.Extensions.General;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework.GUI.ControlAdapters
{
    [PublicAPI]
    public abstract class TextBoxAdapter<T> : IDisposable
    {
        [PublicAPI]
        public sealed class ExtendedKeyPressEventArgs
        {
            [NotNull]
            public readonly TextBoxAdapter<T> Sender;

            public readonly char TypedCharacter;

            [CanBeNull]
            public readonly string CurrentString;

            [CanBeNull]
            public readonly T CurrentValue;

            public readonly int CursorPosition;

            public ExtendedKeyPressEventArgs([NotNull] TextBoxAdapter<T> sender, char character, [CanBeNull] string currentString, [CanBeNull] T currentValue, int cursorPosition)
            {
                Sender = sender;
                TypedCharacter = character;
                CurrentString = currentString;
                CurrentValue = currentValue;
                CursorPosition = cursorPosition;
            }
        }

        public delegate void ExtendedKeyPressEventHandler(ExtendedKeyPressEventArgs e);

        public delegate void AfterValueAcceptedEventHandler([NotNull] TextBoxAdapter<T> sender, [CanBeNull] string currentString, [CanBeNull] T currentValue);

        public delegate void BeforeValueAcceptedEventHandler(
            [NotNull] TextBoxAdapter<T> sender,
            [CanBeNull] string previousString,
            [CanBeNull] T previousValue,
            [CanBeNull] string newString,
            [CanBeNull] T newValue,
            [NotNull] CancelEventArgs cancelEvent
        );

        public bool AcceptValueOnValidKeyPress { get; set; }

        public bool ResizeTextBoxToContent { get; set; }

        public bool ShowToolTips { get; set; }

        public bool KeepFocusIfContentIsInvalid { get; set; }

        public bool HideShowDefaultValue { get; set; }

        [NotNull]
        protected abstract string ToolTipTextOnInvalidValue { get; }

        protected abstract bool AllowChar(char keyChar, [NotNull] string contentBefore, int cursorPosition, [NotNull] out string toolTipOnKeyPress);

        protected abstract bool IsContentValid([NotNull] string content);

        protected abstract bool TryRepairContent([NotNull] string content, [NotNull] out string repairedContent);

        [NotNull]
        private readonly ToolTip TextBoxToolTip;

        [CanBeNull]
        protected abstract T StringToValue([NotNull] string content);

        [NotNull]
        protected abstract string ValueToString([CanBeNull] T value);

        [NotNull]
        public string DefaultText { get; set; }

        [CanBeNull]
        public abstract T DefaultValue { get; set; }

        [CanBeNull]
        public T CurrentValue
        {
            get => StringToValue(CurrentString);
            set => CurrentString = ValueToString(value);
        }

        [NotNull]
        public string CurrentString
        {
            get => TextBox.Text;
            set
            {
                try
                {
                    DisableTextChangedEvent = true;
                    TextBox.Text = value;
                }
                finally
                {
                    DisableTextChangedEvent = false;
                }
            }
        }

        public int CursorPosition
        {
            get => TextBox.SelectionStart;
            set
            {
                if (value < 0) value = 0;
                int stringLength = CurrentString.Length;
                if (value > stringLength) value = stringLength;
                TextBox.SelectionStart = value;
            }
        }

        public void SetAutoCompleteValues([NotNull, ItemNotNull] IEnumerable<T> values)
        {
            TextBox.AutoCompleteCustomSource = new AutoCompleteStringCollection();
            TextBox.AutoCompleteCustomSource.AddRange(values.Select(ValueToString).ToArray());
            TextBox.AutoCompleteMode = AutoCompleteMode.Suggest;
            TextBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        [CanBeNull]
        public event AfterValueAcceptedEventHandler AfterValueAcceptedEvent;

        [CanBeNull]
        public event BeforeValueAcceptedEventHandler BeforeValueAcceptedEvent;

        [CanBeNull]
        public event ExtendedKeyPressEventHandler InvalidCharTypedEvent;

        [CanBeNull]
        public event ExtendedKeyPressEventHandler ValidCharTypedEvent;

        [CanBeNull]
        public string LastAcceptedString { get; private set; }

        [CanBeNull]
        public T LastAcceptedValue => LastAcceptedString == null ? DefaultValue : StringToValue(LastAcceptedString);

        [NotNull]
        protected readonly TextBox TextBox;

        private readonly int TextBoxInitialWidth;

        private bool DisableTextChangedEvent;

        protected TextBoxAdapter([NotNull] TextBox textBox)
        {
            TextBox = textBox;
            TextBoxInitialWidth = textBox.Width;
            TextBoxToolTip = new ToolTip();
            AddEventHandlers();
            DefaultText = "";
        }

        protected TextBoxAdapter([NotNull] TextBox textBox, [NotNull] string defaultText, bool hideShowDefaultValue) : this(textBox)
        {
            DefaultText = defaultText;
            CurrentString = defaultText;
            HideShowDefaultValue = hideShowDefaultValue;
        }

        private void AddEventHandlers()
        {
            TextBox.KeyDown += HandleKeyDownEvent;
            TextBox.KeyPress += HandleKeyPressEvent;
            TextBox.KeyUp += HandleKeyUpEvent;
            TextBox.Validating += HandleValidatingEvent;
            TextBox.TextChanged += HandleTextChangedEvent;
            TextBox.Enter += HandleEnterEvent;
            TextBox.Leave += HandleLeaveEvent;
        }

        private void RemoveEventHandlers()
        {
            TextBox.KeyDown -= HandleKeyDownEvent;
            TextBox.KeyPress -= HandleKeyPressEvent;
            TextBox.KeyUp -= HandleKeyUpEvent;
            TextBox.Validating -= HandleValidatingEvent;
            TextBox.TextChanged -= HandleTextChangedEvent;
            TextBox.Enter -= HandleEnterEvent;
            TextBox.Leave -= HandleLeaveEvent;
        }

        private void HandleEnterEvent([CanBeNull] object sender, [CanBeNull] EventArgs e)
        {
            if (CurrentString != DefaultText) return;
            LastAcceptedString = LastAcceptedString ?? DefaultText;

            if (!HideShowDefaultValue) return;
            TextBox.SelectionStart = 0;
            TextBox.SelectionLength = LastAcceptedString.Length;
        }

        private void HandleLeaveEvent([CanBeNull] object sender, [CanBeNull] EventArgs e)
        {
            if (HideShowDefaultValue && string.IsNullOrWhiteSpace(CurrentString)) CurrentString = DefaultText;
        }

        private void HandleTextChangedEvent([CanBeNull] object sender, [CanBeNull] EventArgs e)
        {
            if (DisableTextChangedEvent) return;
            HandleTextChangedEvent();
        }

        protected abstract void HandleTextChangedEvent();

        public void ResizeTextBox()
        {
            if (!ResizeTextBoxToContent) return;
            int textWidthWithMargins = TextRenderer.MeasureText(TextBox.Text, TextBox.Font).Width + 6;
            TextBox.Width = textWidthWithMargins > TextBoxInitialWidth ? textWidthWithMargins : TextBoxInitialWidth;
        }

        [CanBeNull]
        private bool? ValidKeyPressOccurred;

        [NotNull]
        private string TooltipTextOnKeyPress = "";

        [CanBeNull]
        private ExtendedKeyPressEventArgs LastKeyPressEventArgs;

        private void HandleKeyPressEvent([CanBeNull] object sender, [NotNull] KeyPressEventArgs e)
        {
            LastKeyPressEventArgs = new ExtendedKeyPressEventArgs(this, e.KeyChar, CurrentString, CurrentValue, CursorPosition);
            ValidKeyPressOccurred = AllowChar(e.KeyChar, CurrentString, CursorPosition, out TooltipTextOnKeyPress);
            e.Handled = ValidKeyPressOccurred == false;
        }

        private void HandleKeyDownEvent([CanBeNull] object sender, [NotNull] KeyEventArgs e)
        {
            TextBoxToolTip.Hide(TextBox);
            ToolTipTimer?.StopTimer(false);
            if (e.KeyCode.Equals(Keys.Enter))
            {
                if (ValidateAndRepairContent())
                {
                    SendValueAcceptedEvent();
                    e.Handled = true;
                }
                else
                {
                    Logger.PrintWarning("Content could not be validated.");
                }
            }
        }

        private void HandleKeyUpEvent([CanBeNull] object sender, [CanBeNull] KeyEventArgs e)
        {
            try
            {
                if (ValidKeyPressOccurred == false && LastKeyPressEventArgs != null)
                {
                    InvalidCharTypedEvent?.Invoke(LastKeyPressEventArgs);
                }

                if (ValidKeyPressOccurred == true && LastKeyPressEventArgs != null)
                {
                    ValidCharTypedEvent?.Invoke(LastKeyPressEventArgs);
                    if (AcceptValueOnValidKeyPress && ValidateAndRepairContent()) SendValueAcceptedEvent(); //TODO dangerous since char might not be written yet TEST!
                    ResizeTextBox();
                }

                if (ValidKeyPressOccurred != null) ShowToolTip(TooltipTextOnKeyPress);
            }
            finally
            {
                ValidKeyPressOccurred = null;
            }
        }

        private void SendValueAcceptedEvent()
        {
            string currentString = CurrentString;
            if (LastAcceptedString == currentString) return;
            if (BeforeValueAcceptedEvent != null)
            {
                CancelEventArgs cancelEvent = new CancelEventArgs();
                BeforeValueAcceptedEvent(this,
                                         LastAcceptedString,
                                         LastAcceptedValue,
                                         currentString,
                                         CurrentValue,
                                         cancelEvent);
                if (cancelEvent.Cancel)
                {
                    CurrentString = LastAcceptedString ?? DefaultText;
                    return;
                }
            }
            if (AfterValueAcceptedEvent != null)
            {
                LastAcceptedString = currentString;
                AfterValueAcceptedEvent(this, currentString, CurrentValue);
            }
        }

        private void HandleValidatingEvent([CanBeNull] object sender, [NotNull] CancelEventArgs e)
        {
            if (ValidateAndRepairContent()) SendValueAcceptedEvent();
            else if (KeepFocusIfContentIsInvalid) e.Cancel = true;
        }

        private bool ValidateAndRepairContent()
        {
            string currentString = CurrentString;
            if (!TryRepairContent(currentString, out string repairedString))
            {
                ShowToolTip("The entered value " + currentString + " is not valid\n" + ToolTipTextOnInvalidValue);
                return false;
            }
            if (repairedString != currentString) CurrentString = repairedString;
            return true;
        }

        [CanBeNull]
        private TimerAction<bool> ToolTipTimer;

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private void ShowToolTip([NotNull] string text)
        {
            if (!ShowToolTips || string.IsNullOrWhiteSpace(text)) return;
            TextBoxToolTip.Show(text, TextBox);
            if (ToolTipTimer == null)
                ToolTipTimer = new TimerAction<bool>(() => true,
                                                     value =>
                                                     {
                                                         // Need to check for null
                                                         if (TextBoxToolTip != null && TextBox != null) TextBoxToolTip.Hide(TextBox);
                                                     },
                                                     (value, newValue) => true,
                                                     2000);
            else ToolTipTimer.RestartTimer();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                RemoveEventHandlers();
                TextBoxToolTip.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    [PublicAPI]
    public sealed class IntegerTextBoxAdapter : TextBoxAdapter<int>
    {
        private const char Zero = '0';
        private const char Negation = '-';
        public IntRange AllowedRange { get; set; }

        [CanBeNull]
        public Func<int, bool> IsValueValidFunction { get; set; }

        public bool IsValueValid(int value) => AllowedRange.IsInRange(value) && (IsValueValidFunction == null || IsValueValidFunction(value));

        private int _defaultValue;

        public override int DefaultValue
        {
            get => _defaultValue;
            set
            {
                _defaultValue = value;
                DefaultText = ValueToString(value);
            }
        }

        public IntegerTextBoxAdapter([NotNull] TextBox textBox, IntRange range) : base(textBox)
        {
            AllowedRange = range;
            if (AllowedRange.OnlyNegative) CurrentString = Negation.ToString();
        }

        public IntegerTextBoxAdapter([NotNull] TextBox textBox, IntRange range, int? defaultValue, bool hideShowDefaultValue) : base(textBox, defaultValue.ToString(), hideShowDefaultValue)
        {
            if (defaultValue.HasValue) DefaultValue = defaultValue.Value;
            AllowedRange = range;
            if (AllowedRange.OnlyNegative) CurrentString = Negation.ToString();
        }

        protected override string ToolTipTextOnInvalidValue => "The value must be in the " + AllowedRange;

        protected override bool AllowChar(char keyChar, string contentBefore, int cursorPosition, out string toolTipOnKeyPress)
        {
            toolTipOnKeyPress = "";
            if (char.IsControl(keyChar)) return true;
            if (CursorPosition == 0 && keyChar == Negation && !AllowedRange.OnlyNonNegative && !contentBefore.StartsWithOrdinal(Negation.ToString())) return true;
            if (!char.IsDigit(keyChar))
            {
                toolTipOnKeyPress = "Only digits are allowed.";
                return false;
            }
            string newString = contentBefore.Insert(cursorPosition, keyChar.ToString()); //CursorPosition must be between 0 and length
            if (TryRepairContent(newString, out newString)) return true;
            toolTipOnKeyPress = "Only numbers in the " + AllowedRange + " are allowed.";
            //TODO think about input prevention
            return true;
        }

        protected override bool IsContentValid(string content)
        {
            if (!int.TryParse(content, out int number)) return false;
            return IsValueValid(number);
        }

        protected override bool TryRepairContent(string content, out string repairedContent)
        {
            if (IsContentValid(content))
            {
                repairedContent = content;
                return true;
            }
            StringBuilder recreation = new StringBuilder();
            bool negative = false;
            bool containsDigitThatIsNotZero = false;
            if (AllowedRange.OnlyNegative)
            {
                negative = true;
                recreation.Append(Negation);
            }

            foreach (char c in content)
            {
                if (negative)
                {
                    containsDigitThatIsNotZero |= c != Zero;
                    if (char.IsDigit(c)) recreation.Append(c);
                }
                else
                {
                    if (AllowedRange.OnlyNonNegative)
                    {
                        containsDigitThatIsNotZero |= c != Zero;
                        if (char.IsDigit(c)) recreation.Append(c);
                    }
                    else
                    {
                        if (c == Negation)
                        {
                            negative = true;
                            recreation.Append(Negation);
                        }
                        else if (char.IsDigit(c))
                        {
                            containsDigitThatIsNotZero |= c != Zero;
                            recreation.Append(c);
                        }
                    }
                }
            }

            if (containsDigitThatIsNotZero)
            {
                int startIndex = 0;
                if (negative) startIndex = 1;
                int removeLength = 0;
                for (int i = startIndex; i < recreation.Length && recreation[i] == Zero; i++) removeLength++;
                if (removeLength > 0) recreation.Remove(startIndex, removeLength);
            }

            repairedContent = recreation.ToString();
            return IsContentValid(repairedContent);
        }

        protected override int StringToValue(string content) => int.TryParse(content, out int number) ? number : DefaultValue;

        protected override string ValueToString(int value) => value.ToString();

        protected override void HandleTextChangedEvent()
        {
            if (AllowedRange.OnlyNegative && !CurrentString.StartsWithOrdinal(Negation.ToString())) CurrentString = Negation + CurrentString;
        }
    }
}
