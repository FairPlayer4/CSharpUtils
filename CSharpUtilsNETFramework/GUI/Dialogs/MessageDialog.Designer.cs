namespace CSharpUtilsNETFramework.GUI.Dialogs
{
    partial class MessageDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.labelMainText = new System.Windows.Forms.Label();
            this.buttonNo = new System.Windows.Forms.Button();
            this.buttonYesOK = new System.Windows.Forms.Button();
            this.TLPMain = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBoxIcon = new System.Windows.Forms.PictureBox();
            this.buttonCancelClose = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.TLPMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // labelMainText
            // 
            this.TLPMain.SetColumnSpan(this.labelMainText, 4);
            this.labelMainText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelMainText.Location = new System.Drawing.Point(65, 3);
            this.labelMainText.Margin = new System.Windows.Forms.Padding(3);
            this.labelMainText.Name = "labelMainText";
            this.TLPMain.SetRowSpan(this.labelMainText, 3);
            this.labelMainText.Size = new System.Drawing.Size(337, 60);
            this.labelMainText.TabIndex = 0;
            this.labelMainText.Text = "MainText";
            this.labelMainText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonNo
            // 
            this.buttonNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonNo.Location = new System.Drawing.Point(242, 69);
            this.buttonNo.Name = "buttonNo";
            this.buttonNo.Size = new System.Drawing.Size(75, 24);
            this.buttonNo.TabIndex = 1;
            this.buttonNo.Text = "No";
            this.buttonNo.UseVisualStyleBackColor = true;
            this.buttonNo.Click += new System.EventHandler(this.ButtonNo_Click);
            // 
            // buttonYesOK
            // 
            this.buttonYesOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonYesOK.Location = new System.Drawing.Point(161, 69);
            this.buttonYesOK.Name = "buttonYesOK";
            this.buttonYesOK.Size = new System.Drawing.Size(75, 24);
            this.buttonYesOK.TabIndex = 1;
            this.buttonYesOK.Text = "YesOK";
            this.buttonYesOK.UseVisualStyleBackColor = true;
            this.buttonYesOK.Click += new System.EventHandler(this.ButtonYesOK_Click);
            // 
            // TLPMain
            // 
            this.TLPMain.ColumnCount = 7;
            this.TLPMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.TLPMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TLPMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.TLPMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.TLPMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TLPMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TLPMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TLPMain.Controls.Add(this.buttonNo, 5, 3);
            this.TLPMain.Controls.Add(this.buttonYesOK, 4, 3);
            this.TLPMain.Controls.Add(this.pictureBoxIcon, 1, 1);
            this.TLPMain.Controls.Add(this.labelMainText, 3, 0);
            this.TLPMain.Controls.Add(this.buttonCancelClose, 6, 3);
            this.TLPMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TLPMain.Location = new System.Drawing.Point(7, 7);
            this.TLPMain.Name = "TLPMain";
            this.TLPMain.RowCount = 4;
            this.TLPMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TLPMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TLPMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TLPMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TLPMain.Size = new System.Drawing.Size(405, 96);
            this.TLPMain.TabIndex = 3;
            // 
            // pictureBoxIcon
            // 
            this.pictureBoxIcon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxIcon.Location = new System.Drawing.Point(15, 17);
            this.pictureBoxIcon.Name = "pictureBoxIcon";
            this.pictureBoxIcon.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxIcon.TabIndex = 2;
            this.pictureBoxIcon.TabStop = false;
            // 
            // buttonCancelClose
            // 
            this.buttonCancelClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonCancelClose.Location = new System.Drawing.Point(323, 69);
            this.buttonCancelClose.Name = "buttonCancelClose";
            this.buttonCancelClose.Size = new System.Drawing.Size(79, 24);
            this.buttonCancelClose.TabIndex = 3;
            this.buttonCancelClose.Text = "CancelClose";
            this.buttonCancelClose.UseVisualStyleBackColor = true;
            this.buttonCancelClose.Click += new System.EventHandler(this.ButtonCancelClose_Click);
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 2000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // MessageDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(419, 110);
            this.Controls.Add(this.TLPMain);
            this.MinimumSize = new System.Drawing.Size(387, 137);
            this.Name = "MessageDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Title";
            this.TopMost = true;
            this.TLPMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label labelMainText;
        public System.Windows.Forms.Button buttonNo;
        public System.Windows.Forms.Button buttonYesOK;
        private System.Windows.Forms.TableLayoutPanel TLPMain;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.PictureBox pictureBoxIcon;
        private System.Windows.Forms.Button buttonCancelClose;
    }
}