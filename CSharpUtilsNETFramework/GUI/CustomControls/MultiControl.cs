#region Imports

using System;
using System.Windows.Forms;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework.GUI.CustomControls
{
    [PublicAPI]
    public partial class MultiControl : UserControl
    {
        public Label Label => label;

        public TextBox TextBox => textBox;

        public ComboBox ComboBox => comboBox;

        public Button Button => button;

        public bool IsLabel
        {
            get => label.Visible;
            set { label.Visible = value; UpdateMultiControl(); }
        }

        public bool IsTextBox
        {
            get => textBox.Visible;
            set { textBox.Visible = value; UpdateMultiControl(); }
        }

        public bool IsComboBox
        {
            get => comboBox.Visible;
            set { comboBox.Visible = value; UpdateMultiControl(); }
        }

        public bool IsButton
        {
            get => button.Visible;
            set { button.Visible = value; UpdateMultiControl(); }
        }

        public void UpdateMultiControl()
        {
            int height = 0;
            if (IsLabel) height += label.Height + label.Margin.Vertical;
            if (IsTextBox) height += textBox.Height + textBox.Margin.Vertical;
            if (IsComboBox) height += comboBox.Height + comboBox.Margin.Vertical;
            if (IsButton) height += button.Height + button.Margin.Vertical;
            Height = height; //TODO Margin
        }

        public void SetText([CanBeNull] string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            label.Text = text;
            textBox.Text = text;
            button.Text = text;
        }

        public MultiControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateMultiControl();
        }
    }
}
