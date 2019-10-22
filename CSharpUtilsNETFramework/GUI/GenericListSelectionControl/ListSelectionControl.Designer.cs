using System.Windows.Forms;

namespace CSharpUtilsNETFramework.GUI.GenericListSelectionControl
{
    sealed partial class ListSelectionControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.listView = new System.Windows.Forms.ListView();
            this.ColumnsContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.OptionsButton = new System.Windows.Forms.Button();
            this.OptionsContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.NavigationButton = new System.Windows.Forms.Button();
            this.NavigationContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.TLPMain = new System.Windows.Forms.TableLayoutPanel();
            this.TitleLabel = new System.Windows.Forms.Label();
            this.ItemToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.TLPMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(134, 6);
            // 
            // listView
            // 
            this.listView.CheckBoxes = true;
            this.TLPMain.SetColumnSpan(this.listView, 3);
            this.listView.ContextMenuStrip = this.ColumnsContextMenuStrip;
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(3, 45);
            this.listView.Name = "listView";
            this.listView.OwnerDraw = true;
            this.listView.Size = new System.Drawing.Size(326, 263);
            this.listView.TabIndex = 6;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.ListView_ColumnClick);
            this.listView.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.ListView_ColumnWidthChanging);
            this.listView.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.ListView_DrawColumnHeader);
            this.listView.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.ListView_DrawItem);
            this.listView.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.ListView_DrawSubItem);
            this.listView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ListView_ItemBeforeCheckChanged);
            this.listView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.ListView_ItemAfterCheckChanged);
            this.listView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.ListView_ItemAfterSelectionChanged);
            this.listView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListView_KeyDown);
            this.listView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ListView_KeyUp);
            // 
            // ColumnsContextMenuStrip
            // 
            this.ColumnsContextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ColumnsContextMenuStrip.Name = "ColumnsContextMenuStrip";
            this.ColumnsContextMenuStrip.Size = new System.Drawing.Size(61, 4);
            this.ColumnsContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.ColumnsContextMenuStrip_Opening);
            // 
            // DescriptionLabel
            // 
            this.DescriptionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DescriptionLabel.Location = new System.Drawing.Point(3, 13);
            this.DescriptionLabel.Name = "DescriptionLabel";
            this.DescriptionLabel.Size = new System.Drawing.Size(164, 29);
            this.DescriptionLabel.TabIndex = 9;
            this.DescriptionLabel.Text = "labelDescription";
            this.DescriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // OptionsButton
            // 
            this.OptionsButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OptionsButton.Location = new System.Drawing.Point(254, 16);
            this.OptionsButton.Name = "OptionsButton";
            this.OptionsButton.Size = new System.Drawing.Size(75, 23);
            this.OptionsButton.TabIndex = 10;
            this.OptionsButton.Text = "Options";
            this.OptionsButton.UseVisualStyleBackColor = true;
            this.OptionsButton.Click += new System.EventHandler(this.OptionsButton_Click);
            // 
            // OptionsContextMenuStrip
            // 
            this.OptionsContextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.OptionsContextMenuStrip.Name = "NavigationContextMenuStrip";
            this.OptionsContextMenuStrip.Size = new System.Drawing.Size(61, 4);
            // 
            // NavigationButton
            // 
            this.NavigationButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NavigationButton.Location = new System.Drawing.Point(173, 16);
            this.NavigationButton.Name = "NavigationButton";
            this.NavigationButton.Size = new System.Drawing.Size(75, 23);
            this.NavigationButton.TabIndex = 11;
            this.NavigationButton.Text = "Navigate";
            this.NavigationButton.UseVisualStyleBackColor = true;
            this.NavigationButton.Click += new System.EventHandler(this.NavigateButton_Click);
            // 
            // NavigationContextMenuStrip
            // 
            this.NavigationContextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.NavigationContextMenuStrip.Name = "NavigationContextMenuStrip";
            this.NavigationContextMenuStrip.Size = new System.Drawing.Size(61, 4);
            // 
            // TLPMain
            // 
            this.TLPMain.ColumnCount = 3;
            this.TLPMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TLPMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TLPMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TLPMain.Controls.Add(this.OptionsButton, 2, 1);
            this.TLPMain.Controls.Add(this.NavigationButton, 1, 1);
            this.TLPMain.Controls.Add(this.DescriptionLabel, 0, 1);
            this.TLPMain.Controls.Add(this.listView, 0, 2);
            this.TLPMain.Controls.Add(this.TitleLabel, 0, 0);
            this.TLPMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TLPMain.Location = new System.Drawing.Point(0, 0);
            this.TLPMain.Name = "TLPMain";
            this.TLPMain.RowCount = 3;
            this.TLPMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TLPMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TLPMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TLPMain.Size = new System.Drawing.Size(332, 311);
            this.TLPMain.TabIndex = 12;
            // 
            // TitleLabel
            // 
            this.TitleLabel.AutoSize = true;
            this.TLPMain.SetColumnSpan(this.TitleLabel, 3);
            this.TitleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TitleLabel.Location = new System.Drawing.Point(3, 0);
            this.TitleLabel.Name = "TitleLabel";
            this.TitleLabel.Size = new System.Drawing.Size(326, 13);
            this.TitleLabel.TabIndex = 12;
            this.TitleLabel.Text = "labelTitle";
            this.TitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ItemToolTip
            // 
            this.ItemToolTip.AutoPopDelay = 5000;
            this.ItemToolTip.InitialDelay = 500;
            this.ItemToolTip.ReshowDelay = 500;
            // 
            // ListSelectionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TLPMain);
            this.Name = "ListSelectionControl";
            this.Size = new System.Drawing.Size(332, 311);
            this.TLPMain.ResumeLayout(false);
            this.TLPMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public Label DescriptionLabel;
        public ContextMenuStrip OptionsContextMenuStrip;
        public ContextMenuStrip NavigationContextMenuStrip;
        public ToolStripSeparator toolStripSeparator;
        public ContextMenuStrip ColumnsContextMenuStrip;
        public TableLayoutPanel TLPMain;
        public Button NavigationButton;
        public Button OptionsButton;
        public ListView listView;
        public Label TitleLabel;
        private ToolTip ItemToolTip;
    }
}
