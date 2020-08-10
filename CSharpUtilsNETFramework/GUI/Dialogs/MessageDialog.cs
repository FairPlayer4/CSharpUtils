#region Imports

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework.GUI.Dialogs
{
    [PublicAPI]
    public partial class MessageDialog : ExtendedForm
    {
        #region Enums

        public enum Buttons
        {
            YesNo,
            YesNoCancel,
            Ok,
            OkCancel,
            Close
        }

        public enum DefaultButton
        {
            YesOk,
            No,
            CancelClose
        }

        public enum IconBox
        {
            Asterisk,
            Custom,
            Error,
            Exclamation,
            Hand,
            Information,
            None,
            Question,
            Warning
        }

        #endregion

        #region Properties

        public string MainText
        {
            get => labelMainText.Text;
            set => labelMainText.Text = value;
        }

        public string Title
        {
            get => Text;
            set => Text = value;
        }

        private Buttons _selectedButtons;

        public Buttons SelectedButtons
        {
            get => _selectedButtons;
            set
            {
                _selectedButtons = value;
                UpdateButtons();
            }
        }

        private DefaultButton _selectedDefaultButton;

        public DefaultButton SelectedDefaultButton
        {
            get => _selectedDefaultButton;
            set
            {
                _selectedDefaultButton = value;
                UpdateButtons();
            }
        }

        private IconBox _selectedIcon;

        public IconBox SelectedIcon
        {
            get => _selectedIcon;
            set
            {
                _selectedIcon = value;
                UpdateIcon();
            }
        }

        private Image _customIcon;

        public Image CustomIcon
        {
            get => _customIcon;
            set
            {
                _customIcon = value;
                UpdateIcon();
            }
        }

        private string _toolTipYesOk;

        public string ToolTipYesOk
        {
            get => _toolTipYesOk;
            set
            {
                _toolTipYesOk = value;
                UpdateToolTips();
            }
        }

        private string _toolTipNo;

        public string ToolTipNo
        {
            get => _toolTipNo;
            set
            {
                _toolTipNo = value;
                UpdateToolTips();
            }
        }

        private string _toolTipCancelClose;

        public string ToolTipCancelClose
        {
            get => _toolTipCancelClose;
            set
            {
                _toolTipCancelClose = value;
                UpdateToolTips();
            }
        }

        /// <summary>
        /// Prevents the Closing of the Dialog until a Button is pressed.
        /// </summary>
        public bool PreventClosing { get; set; }

        // is needed to handle the closing button of windows forms [X]
        public bool ButtonYesNoOkPressed { get; private set; }

        public Action ActionYesOk { get; set; }

        public Action ActionNo { get; set; }

        public Func<bool> FuncAllowCancelClose { get; set; }

        private bool _ready;

        #endregion

        #region Constructors

        /// <summary>
        /// Base Constructor can be used for manual configuration
        /// </summary>
        public MessageDialog()
        {
            InitializeComponent();
        }

        public MessageDialog(string mainText) : this()
        {
            MainText = mainText;
            SelectedButtons = Buttons.Ok;
            SelectedDefaultButton = DefaultButton.YesOk;
            SelectedIcon = IconBox.None;
        }

        public MessageDialog(string mainText, string title) : this(mainText)
        {
            Title = title;
        }

        public MessageDialog(string mainText, string title, Buttons buttons) : this(mainText, title)
        {
            SelectedButtons = buttons;
            SelectedDefaultButton = _selectedButtons == Buttons.Close ? DefaultButton.CancelClose : DefaultButton.YesOk;
        }

        public MessageDialog(string mainText, string title, Buttons buttons, IconBox icon) : this(mainText, title, buttons)
        {
            SelectedIcon = icon;
        }

        public MessageDialog(string mainText, string title, Buttons buttons, IconBox icon, DefaultButton defaultButton) : this(mainText, title, buttons, icon)
        {
            SelectedDefaultButton = defaultButton;
        }

        public MessageDialog(string mainText, string title, Buttons buttons, Image customIcon, DefaultButton defaultButton) : this(title, mainText, buttons, IconBox.Custom, defaultButton)
        {
            CustomIcon = customIcon;
        }

        public MessageDialog(string mainText, string title, Buttons buttons, IconBox icon, DefaultButton defaultButton, [CanBeNull] Image customIcon = null, bool preventClosing = false, [CanBeNull] Action actionYesOk = null, [CanBeNull] Action actionNo = null, [CanBeNull] Func<bool> funcAllowCancelClose = null) : this(title, mainText, buttons, icon, defaultButton)
        {
            CustomIcon = customIcon;
            PreventClosing = preventClosing;
            ActionYesOk = actionYesOk;
            ActionNo = actionNo;
            FuncAllowCancelClose = funcAllowCancelClose;
        }

        #endregion

        #region Show Methods

        public static DialogResult Show(string mainText)
        {
            using (MessageDialog dialog = new MessageDialog(mainText))
            {
                return dialog.ShowDialog();
            }
        }

        public static DialogResult Show(string mainText, string title)
        {
            using (MessageDialog dialog = new MessageDialog(mainText, title))
            {
                return dialog.ShowDialog();
            }
        }

        public static DialogResult Show(string mainText, string title, Buttons buttons)
        {
            using (MessageDialog dialog = new MessageDialog(mainText, title, buttons))
            {
                return dialog.ShowDialog();
            }
        }

        public static DialogResult Show(string mainText, string title, Buttons buttons, IconBox icon)
        {
            using (MessageDialog dialog = new MessageDialog(mainText, title, buttons, icon))
            {
                return dialog.ShowDialog();
            }
        }

        public static DialogResult Show(string mainText, string title, Buttons buttons, IconBox icon, DefaultButton defaultButton)
        {
            using (MessageDialog dialog = new MessageDialog(mainText, title, buttons, icon, defaultButton))
            {
                return dialog.ShowDialog();
            }
        }

        public static DialogResult Show(IWin32Window owner, string mainText)
        {
            using (MessageDialog dialog = new MessageDialog(mainText))
            {
                return dialog.ShowDialog(owner);
            }
        }

        public static DialogResult Show(IWin32Window owner, string mainText, string title)
        {
            using (MessageDialog dialog = new MessageDialog(mainText, title))
            {
                return dialog.ShowDialog(owner);
            }
        }

        public static DialogResult Show(IWin32Window owner, string mainText, string title, Buttons buttons)
        {
            using (MessageDialog dialog = new MessageDialog(mainText, title, buttons))
            {
                return dialog.ShowDialog(owner);
            }
        }

        public static DialogResult Show(IWin32Window owner, string mainText, string title, Buttons buttons, IconBox icon)
        {
            using (MessageDialog dialog = new MessageDialog(mainText, title, buttons, icon))
            {
                return dialog.ShowDialog(owner);
            }
        }

        public static DialogResult Show(IWin32Window owner, string mainText, string title, Buttons buttons, IconBox icon, DefaultButton defaultButton)
        {
            using (MessageDialog dialog = new MessageDialog(mainText, title, buttons, icon, defaultButton))
            {
                return dialog.ShowDialog(owner);
            }
        }

        #endregion

        #region Useful Methods

        /// <summary>
        /// Adapts the size of the Form if the MainText is too large
        /// </summary>
        public void AdaptSizeToMainText()
        {
            // Might fail if a long string without spaces is used (Exceptional case)
            Size mainTextSize = TextRenderer.MeasureText(labelMainText.CreateGraphics(), MainText, labelMainText.Font, new Size(labelMainText.Width, int.MaxValue), TextFormatFlags.WordBreak);
            if (mainTextSize.Height > labelMainText.Height) Height += mainTextSize.Height - labelMainText.Height;
        }

        #endregion

        #region Event Handling

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _ready = true;
            UpdateButtons();
            UpdateIcon();
            UpdateToolTips();
            AdaptSizeToMainText();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (ButtonYesNoOkPressed) return;
            if (FuncAllowCancelClose != null)
            {
                e.Cancel = !FuncAllowCancelClose();
            }
            else e.Cancel = PreventClosing;
        }

        private void ButtonYesOK_Click(object sender, EventArgs e)
        {
            ButtonYesNoOkPressed = true;
            ActionYesOk?.Invoke();
            if (_selectedButtons == Buttons.YesNo || _selectedButtons == Buttons.YesNoCancel)
                DialogResult = DialogResult.Yes;
            else
                DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonNo_Click(object sender, EventArgs e)
        {
            ButtonYesNoOkPressed = true;
            ActionNo?.Invoke();
            DialogResult = DialogResult.No;
            Close();
        }

        private void ButtonCancelClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        #region GUI Update

        private void UpdateButtons()
        {
            if (!_ready) return;
            switch (_selectedButtons)
            {
                case Buttons.YesNo:
                    buttonYesOK.Text = "Yes";
                    buttonNo.Text = "No";
                    SetColumnVisibility(TLPMain, true, TLPMain.ColumnCount - 2, TLPMain.ColumnCount - 3);
                    SetColumnVisibility(TLPMain, false, TLPMain.ColumnCount - 1);
                    break;
                case Buttons.YesNoCancel:
                    buttonYesOK.Text = "Yes";
                    buttonNo.Text = "No";
                    buttonCancelClose.Text = "Cancel";
                    SetColumnVisibility(TLPMain, true, TLPMain.ColumnCount - 1, TLPMain.ColumnCount - 2, TLPMain.ColumnCount - 3);
                    break;
                case Buttons.Ok:
                    buttonYesOK.Text = "OK";
                    SetColumnVisibility(TLPMain, true, TLPMain.ColumnCount - 3);
                    SetColumnVisibility(TLPMain, false, TLPMain.ColumnCount - 1, TLPMain.ColumnCount - 2);
                    break;
                case Buttons.OkCancel:
                    buttonYesOK.Text = "OK";
                    buttonCancelClose.Text = "Cancel";
                    SetColumnVisibility(TLPMain, true, TLPMain.ColumnCount - 1, TLPMain.ColumnCount - 3);
                    SetColumnVisibility(TLPMain, false, TLPMain.ColumnCount - 2);
                    break;
                case Buttons.Close:
                    buttonCancelClose.Text = "Close";
                    SetColumnVisibility(TLPMain, true, TLPMain.ColumnCount - 1);
                    SetColumnVisibility(TLPMain, false, TLPMain.ColumnCount - 2, TLPMain.ColumnCount - 3);
                    break;
                default:
                    buttonYesOK.Text = "Yes";
                    buttonNo.Text = "No";
                    buttonCancelClose.Text = "Cancel";
                    SetColumnVisibility(TLPMain, true, TLPMain.ColumnCount - 1, TLPMain.ColumnCount - 2, TLPMain.ColumnCount - 3);
                    break;
            }
            if (_selectedDefaultButton == DefaultButton.YesOk)
                ActiveControl = buttonYesOK;
            else if (_selectedDefaultButton == DefaultButton.No)
                ActiveControl = buttonNo;
            else if (_selectedDefaultButton == DefaultButton.CancelClose)
                ActiveControl = buttonCancelClose;
        }

        private void UpdateIcon()
        {
            if (!_ready) return;
            switch (_selectedIcon)
            {
                case IconBox.Asterisk:
                    SetIcon(SystemIcons.Asterisk);
                    break;
                case IconBox.Custom:
                    SetIcon(_customIcon);
                    break;
                case IconBox.Error:
                    SetIcon(SystemIcons.Error);
                    break;
                case IconBox.Exclamation:
                    SetIcon(SystemIcons.Exclamation);
                    break;
                case IconBox.Hand:
                    SetIcon(SystemIcons.Hand);
                    break;
                case IconBox.Information:
                    SetIcon(SystemIcons.Information);
                    break;
                case IconBox.None:
                    SetIcon();
                    break;
                case IconBox.Question:
                    SetIcon(SystemIcons.Question);
                    break;
                case IconBox.Warning:
                    SetIcon(SystemIcons.Asterisk);
                    break;
                default:
                    SetIcon();
                    break;
            }
        }

        private void SetIcon()
        {
            SetColumnVisibility(TLPMain, false, 0, 1, 2);
            pictureBoxIcon.Image = null;
        }

        private void SetIcon([CanBeNull] Icon icon)
        {
            SetColumnVisibility(TLPMain, icon != null, 0, 1, 2);
            pictureBoxIcon.Image = icon?.ToBitmap();
        }

        private void SetIcon([CanBeNull] Image icon)
        {
            SetColumnVisibility(TLPMain, icon != null, 0, 1, 2);
            pictureBoxIcon.Image = icon;
        }

        private void UpdateToolTips()
        {
            //if (!_ready) return;
            //TODO if needed
        }

        #endregion

    }
}
