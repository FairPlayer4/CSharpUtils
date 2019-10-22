namespace CSharpUtilsNETFramework.GUI.Dialogs
{
    sealed partial class ProgressDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressDialog));
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.textBoxInfo = new System.Windows.Forms.TextBox();
            this.buttonStop = new System.Windows.Forms.Button();
            this.TLPMain = new System.Windows.Forms.TableLayoutPanel();
            this.percentageLabel = new System.Windows.Forms.Label();
            this.TLPMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.TLPMain.SetColumnSpan(this.progressBar, 2);
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar.Location = new System.Drawing.Point(3, 3);
            this.progressBar.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(481, 27);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 0;
            // 
            // textBoxInfo
            // 
            this.TLPMain.SetColumnSpan(this.textBoxInfo, 3);
            this.textBoxInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxInfo.Location = new System.Drawing.Point(3, 43);
            this.textBoxInfo.Multiline = true;
            this.textBoxInfo.Name = "textBoxInfo";
            this.textBoxInfo.ReadOnly = true;
            this.textBoxInfo.Size = new System.Drawing.Size(526, 202);
            this.textBoxInfo.TabIndex = 1;
            // 
            // buttonStop
            // 
            this.TLPMain.SetColumnSpan(this.buttonStop, 2);
            this.buttonStop.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonStop.Location = new System.Drawing.Point(432, 251);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(97, 23);
            this.buttonStop.TabIndex = 2;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.ButtonStop_Click);
            // 
            // TLPMain
            // 
            this.TLPMain.ColumnCount = 3;
            this.TLPMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TLPMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TLPMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TLPMain.Controls.Add(this.progressBar, 0, 0);
            this.TLPMain.Controls.Add(this.textBoxInfo, 0, 1);
            this.TLPMain.Controls.Add(this.buttonStop, 1, 2);
            this.TLPMain.Controls.Add(this.percentageLabel, 2, 0);
            this.TLPMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TLPMain.Location = new System.Drawing.Point(7, 7);
            this.TLPMain.Name = "TLPMain";
            this.TLPMain.RowCount = 3;
            this.TLPMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TLPMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TLPMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TLPMain.Size = new System.Drawing.Size(532, 277);
            this.TLPMain.TabIndex = 3;
            // 
            // percentageLabel
            // 
            this.percentageLabel.AutoSize = true;
            this.percentageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.percentageLabel.Location = new System.Drawing.Point(490, 0);
            this.percentageLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
            this.percentageLabel.Name = "percentageLabel";
            this.percentageLabel.Size = new System.Drawing.Size(39, 30);
            this.percentageLabel.TabIndex = 3;
            this.percentageLabel.Text = " 100% ";
            this.percentageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ProgressDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonStop;
            this.ClientSize = new System.Drawing.Size(546, 291);
            this.Controls.Add(this.TLPMain);
            this.MinimumSize = new System.Drawing.Size(390, 246);
            this.Name = "ProgressDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Progress";
            this.TopMost = true;
            this.TLPMain.ResumeLayout(false);
            this.TLPMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel TLPMain;
        private System.Windows.Forms.TextBox textBoxInfo;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label percentageLabel;
    }
}