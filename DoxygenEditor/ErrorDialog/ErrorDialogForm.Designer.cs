namespace TSP.DoxygenEditor.ErrorDialog
{
    partial class ErrorDialogForm
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
            this.pbIcon = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panTop = new System.Windows.Forms.Panel();
            this.panControls = new System.Windows.Forms.Panel();
            this.btnOk = new System.Windows.Forms.Button();
            this.panDetailsContainer = new System.Windows.Forms.Panel();
            this.panDetailsFull = new System.Windows.Forms.Panel();
            this.rtbDetails = new System.Windows.Forms.RichTextBox();
            this.panDetailsTop = new System.Windows.Forms.Panel();
            this.btnShowDetails = new System.Windows.Forms.Button();
            this.panMessage = new System.Windows.Forms.Panel();
            this.lblMessage = new System.Windows.Forms.Label();
            this.btnCopyDetails = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).BeginInit();
            this.panTop.SuspendLayout();
            this.panControls.SuspendLayout();
            this.panDetailsContainer.SuspendLayout();
            this.panDetailsFull.SuspendLayout();
            this.panDetailsTop.SuspendLayout();
            this.panMessage.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbIcon
            // 
            this.pbIcon.Dock = System.Windows.Forms.DockStyle.Left;
            this.pbIcon.Image = global::TSP.DoxygenEditor.Properties.Resources.StatusCriticalError_32x;
            this.pbIcon.Location = new System.Drawing.Point(10, 10);
            this.pbIcon.Name = "pbIcon";
            this.pbIcon.Size = new System.Drawing.Size(37, 37);
            this.pbIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbIcon.TabIndex = 0;
            this.pbIcon.TabStop = false;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(47, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.lblTitle.Size = new System.Drawing.Size(567, 37);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "[Title]";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panTop
            // 
            this.panTop.Controls.Add(this.lblTitle);
            this.panTop.Controls.Add(this.pbIcon);
            this.panTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panTop.Location = new System.Drawing.Point(0, 0);
            this.panTop.Name = "panTop";
            this.panTop.Padding = new System.Windows.Forms.Padding(10);
            this.panTop.Size = new System.Drawing.Size(624, 57);
            this.panTop.TabIndex = 2;
            // 
            // panControls
            // 
            this.panControls.Controls.Add(this.btnOk);
            this.panControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panControls.Location = new System.Drawing.Point(0, 381);
            this.panControls.Name = "panControls";
            this.panControls.Padding = new System.Windows.Forms.Padding(8);
            this.panControls.Size = new System.Drawing.Size(624, 40);
            this.panControls.TabIndex = 3;
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnOk.Location = new System.Drawing.Point(541, 8);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 24);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // panDetailsContainer
            // 
            this.panDetailsContainer.Controls.Add(this.panDetailsFull);
            this.panDetailsContainer.Controls.Add(this.panDetailsTop);
            this.panDetailsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panDetailsContainer.Location = new System.Drawing.Point(0, 122);
            this.panDetailsContainer.Name = "panDetailsContainer";
            this.panDetailsContainer.Padding = new System.Windows.Forms.Padding(8);
            this.panDetailsContainer.Size = new System.Drawing.Size(624, 259);
            this.panDetailsContainer.TabIndex = 5;
            // 
            // panDetailsFull
            // 
            this.panDetailsFull.Controls.Add(this.rtbDetails);
            this.panDetailsFull.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panDetailsFull.Location = new System.Drawing.Point(8, 37);
            this.panDetailsFull.Name = "panDetailsFull";
            this.panDetailsFull.Size = new System.Drawing.Size(608, 214);
            this.panDetailsFull.TabIndex = 2;
            // 
            // rtbDetails
            // 
            this.rtbDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbDetails.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbDetails.Location = new System.Drawing.Point(0, 0);
            this.rtbDetails.Name = "rtbDetails";
            this.rtbDetails.ReadOnly = true;
            this.rtbDetails.Size = new System.Drawing.Size(608, 214);
            this.rtbDetails.TabIndex = 0;
            this.rtbDetails.Text = "";
            this.rtbDetails.WordWrap = false;
            // 
            // panDetailsTop
            // 
            this.panDetailsTop.Controls.Add(this.btnCopyDetails);
            this.panDetailsTop.Controls.Add(this.btnShowDetails);
            this.panDetailsTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panDetailsTop.Location = new System.Drawing.Point(8, 8);
            this.panDetailsTop.Name = "panDetailsTop";
            this.panDetailsTop.Size = new System.Drawing.Size(608, 29);
            this.panDetailsTop.TabIndex = 3;
            // 
            // btnShowDetails
            // 
            this.btnShowDetails.Image = global::TSP.DoxygenEditor.Properties.Resources.CollapseArrow_16x;
            this.btnShowDetails.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnShowDetails.Location = new System.Drawing.Point(4, 3);
            this.btnShowDetails.Name = "btnShowDetails";
            this.btnShowDetails.Size = new System.Drawing.Size(113, 23);
            this.btnShowDetails.TabIndex = 2;
            this.btnShowDetails.Text = "Show details";
            this.btnShowDetails.UseVisualStyleBackColor = false;
            this.btnShowDetails.Click += new System.EventHandler(this.btnShowDetails_Click);
            // 
            // panMessage
            // 
            this.panMessage.Controls.Add(this.lblMessage);
            this.panMessage.Dock = System.Windows.Forms.DockStyle.Top;
            this.panMessage.Location = new System.Drawing.Point(0, 57);
            this.panMessage.Name = "panMessage";
            this.panMessage.Padding = new System.Windows.Forms.Padding(12, 8, 8, 8);
            this.panMessage.Size = new System.Drawing.Size(624, 65);
            this.panMessage.TabIndex = 6;
            // 
            // lblMessage
            // 
            this.lblMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMessage.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.Location = new System.Drawing.Point(12, 8);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(604, 49);
            this.lblMessage.TabIndex = 0;
            this.lblMessage.Text = "[Message Line1]\r\n[Message Line2]\r\n[Message Line3]\r\n";
            // 
            // btnCopyDetails
            // 
            this.btnCopyDetails.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCopyDetails.Location = new System.Drawing.Point(123, 3);
            this.btnCopyDetails.Name = "btnCopyDetails";
            this.btnCopyDetails.Size = new System.Drawing.Size(113, 23);
            this.btnCopyDetails.TabIndex = 3;
            this.btnCopyDetails.Text = "Copy details";
            this.btnCopyDetails.UseVisualStyleBackColor = false;
            this.btnCopyDetails.Click += new System.EventHandler(this.btnCopyDetails_Click);
            // 
            // ErrorDialogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 421);
            this.Controls.Add(this.panDetailsContainer);
            this.Controls.Add(this.panMessage);
            this.Controls.Add(this.panControls);
            this.Controls.Add(this.panTop);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ErrorDialogForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Error";
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).EndInit();
            this.panTop.ResumeLayout(false);
            this.panControls.ResumeLayout(false);
            this.panDetailsContainer.ResumeLayout(false);
            this.panDetailsFull.ResumeLayout(false);
            this.panDetailsTop.ResumeLayout(false);
            this.panMessage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbIcon;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panTop;
        private System.Windows.Forms.Panel panControls;
        private System.Windows.Forms.Panel panDetailsContainer;
        private System.Windows.Forms.Panel panDetailsFull;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Panel panMessage;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Panel panDetailsTop;
        private System.Windows.Forms.Button btnShowDetails;
        private System.Windows.Forms.RichTextBox rtbDetails;
        private System.Windows.Forms.Button btnCopyDetails;
    }
}