namespace TSP.DoxygenEditor.Views
{
    partial class AboutForm
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
            this.panBottom = new System.Windows.Forms.Panel();
            this.btnOk = new System.Windows.Forms.Button();
            this.panClient = new System.Windows.Forms.Panel();
            this.tbLicense = new System.Windows.Forms.TextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.labelAppVersion = new System.Windows.Forms.Label();
            this.labelAppName = new System.Windows.Forms.Label();
            this.panBottom.SuspendLayout();
            this.panClient.SuspendLayout();
            this.SuspendLayout();
            // 
            // panBottom
            // 
            this.panBottom.Controls.Add(this.btnOk);
            this.panBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panBottom.Location = new System.Drawing.Point(0, 336);
            this.panBottom.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panBottom.Name = "panBottom";
            this.panBottom.Size = new System.Drawing.Size(640, 50);
            this.panBottom.TabIndex = 0;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnOk.Location = new System.Drawing.Point(234, 6);
            this.btnOk.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(165, 40);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // panClient
            // 
            this.panClient.Controls.Add(this.tbLicense);
            this.panClient.Controls.Add(this.labelDescription);
            this.panClient.Controls.Add(this.labelAppVersion);
            this.panClient.Controls.Add(this.labelAppName);
            this.panClient.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panClient.Location = new System.Drawing.Point(0, 0);
            this.panClient.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panClient.Name = "panClient";
            this.panClient.Size = new System.Drawing.Size(640, 336);
            this.panClient.TabIndex = 1;
            // 
            // tbLicense
            // 
            this.tbLicense.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLicense.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbLicense.Font = new System.Drawing.Font("Lucida Sans Unicode", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tbLicense.Location = new System.Drawing.Point(3, 145);
            this.tbLicense.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbLicense.Multiline = true;
            this.tbLicense.Name = "tbLicense";
            this.tbLicense.ReadOnly = true;
            this.tbLicense.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbLicense.Size = new System.Drawing.Size(634, 187);
            this.tbLicense.TabIndex = 4;
            this.tbLicense.WordWrap = false;
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Font = new System.Drawing.Font("Lucida Sans Unicode", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelDescription.Location = new System.Drawing.Point(7, 104);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(96, 17);
            this.labelDescription.TabIndex = 3;
            this.labelDescription.Text = "[Description]";
            // 
            // labelAppVersion
            // 
            this.labelAppVersion.AutoSize = true;
            this.labelAppVersion.Font = new System.Drawing.Font("Lucida Sans Unicode", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelAppVersion.Location = new System.Drawing.Point(6, 66);
            this.labelAppVersion.Name = "labelAppVersion";
            this.labelAppVersion.Size = new System.Drawing.Size(98, 17);
            this.labelAppVersion.TabIndex = 1;
            this.labelAppVersion.Text = "[AppVersion]";
            // 
            // labelAppName
            // 
            this.labelAppName.AutoSize = true;
            this.labelAppName.Font = new System.Drawing.Font("Lucida Sans Unicode", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelAppName.Location = new System.Drawing.Point(4, 16);
            this.labelAppName.Name = "labelAppName";
            this.labelAppName.Size = new System.Drawing.Size(130, 27);
            this.labelAppName.TabIndex = 0;
            this.labelAppName.Text = "[AppName]";
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 386);
            this.Controls.Add(this.panClient);
            this.Controls.Add(this.panBottom);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            this.panBottom.ResumeLayout(false);
            this.panClient.ResumeLayout(false);
            this.panClient.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panBottom;
        private System.Windows.Forms.Panel panClient;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label labelAppName;
        private System.Windows.Forms.Label labelAppVersion;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.TextBox tbLicense;
    }
}