namespace TSP.DoxygenEditor.Views
{
    partial class ParseSourceForm
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
            rtbLog = new System.Windows.Forms.RichTextBox();
            SuspendLayout();
            // 
            // rtbLog
            // 
            rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            rtbLog.Font = new System.Drawing.Font("Lucida Console", 10.2F);
            rtbLog.Location = new System.Drawing.Point(0, 0);
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.Size = new System.Drawing.Size(800, 450);
            rtbLog.TabIndex = 1;
            rtbLog.Text = "";
            rtbLog.WordWrap = false;
            // 
            // ParseSourceForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(rtbLog);
            MinimizeBox = false;
            Name = "ParseSourceForm";
            Text = "Parsed Source";
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbLog;
    }
}