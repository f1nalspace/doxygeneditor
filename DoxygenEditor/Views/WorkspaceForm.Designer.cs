namespace TSP.DoxygenEditor.Views
{
    partial class WorkspaceForm
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
            this.panControls = new System.Windows.Forms.Panel();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tpSources = new System.Windows.Forms.TabPage();
            this.gbIncludeDirs = new System.Windows.Forms.GroupBox();
            this.lbIncludeDirs = new System.Windows.Forms.ListBox();
            this.cmsIncludeDirs = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAddIncludeDir = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiEditIncludeDir = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRemoveIncludeDir = new System.Windows.Forms.ToolStripMenuItem();
            this.gbIncludeFilter = new System.Windows.Forms.GroupBox();
            this.tbIncludeFilter = new System.Windows.Forms.TextBox();
            this.panControls.SuspendLayout();
            this.tcMain.SuspendLayout();
            this.tpSources.SuspendLayout();
            this.gbIncludeDirs.SuspendLayout();
            this.cmsIncludeDirs.SuspendLayout();
            this.gbIncludeFilter.SuspendLayout();
            this.SuspendLayout();
            // 
            // panControls
            // 
            this.panControls.Controls.Add(this.btnOk);
            this.panControls.Controls.Add(this.btnCancel);
            this.panControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panControls.Location = new System.Drawing.Point(0, 343);
            this.panControls.Name = "panControls";
            this.panControls.Size = new System.Drawing.Size(582, 36);
            this.panControls.TabIndex = 0;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(419, 6);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(500, 6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // tcMain
            // 
            this.tcMain.Controls.Add(this.tpSources);
            this.tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcMain.Location = new System.Drawing.Point(0, 0);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(582, 343);
            this.tcMain.TabIndex = 1;
            // 
            // tpSources
            // 
            this.tpSources.Controls.Add(this.gbIncludeDirs);
            this.tpSources.Controls.Add(this.gbIncludeFilter);
            this.tpSources.Location = new System.Drawing.Point(4, 22);
            this.tpSources.Name = "tpSources";
            this.tpSources.Padding = new System.Windows.Forms.Padding(3);
            this.tpSources.Size = new System.Drawing.Size(574, 317);
            this.tpSources.TabIndex = 1;
            this.tpSources.Text = "Sources";
            this.tpSources.UseVisualStyleBackColor = true;
            // 
            // gbIncludeDirs
            // 
            this.gbIncludeDirs.Controls.Add(this.lbIncludeDirs);
            this.gbIncludeDirs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbIncludeDirs.Location = new System.Drawing.Point(3, 3);
            this.gbIncludeDirs.Name = "gbIncludeDirs";
            this.gbIncludeDirs.Padding = new System.Windows.Forms.Padding(6);
            this.gbIncludeDirs.Size = new System.Drawing.Size(568, 261);
            this.gbIncludeDirs.TabIndex = 0;
            this.gbIncludeDirs.TabStop = false;
            this.gbIncludeDirs.Text = "Include Directories";
            // 
            // lbIncludeDirs
            // 
            this.lbIncludeDirs.ContextMenuStrip = this.cmsIncludeDirs;
            this.lbIncludeDirs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbIncludeDirs.FormattingEnabled = true;
            this.lbIncludeDirs.Location = new System.Drawing.Point(6, 19);
            this.lbIncludeDirs.Name = "lbIncludeDirs";
            this.lbIncludeDirs.Size = new System.Drawing.Size(556, 236);
            this.lbIncludeDirs.TabIndex = 0;
            // 
            // cmsIncludeDirs
            // 
            this.cmsIncludeDirs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAddIncludeDir,
            this.tsmiEditIncludeDir,
            this.tsmiRemoveIncludeDir});
            this.cmsIncludeDirs.Name = "cmsIncludeDirs";
            this.cmsIncludeDirs.Size = new System.Drawing.Size(118, 70);
            this.cmsIncludeDirs.Opening += new System.ComponentModel.CancelEventHandler(this.cmsIncludeDirs_Opening);
            // 
            // tsmiAddIncludeDir
            // 
            this.tsmiAddIncludeDir.Name = "tsmiAddIncludeDir";
            this.tsmiAddIncludeDir.Size = new System.Drawing.Size(117, 22);
            this.tsmiAddIncludeDir.Text = "Add...";
            this.tsmiAddIncludeDir.Click += new System.EventHandler(this.tsmiAddIncludeDir_Click);
            // 
            // tsmiEditIncludeDir
            // 
            this.tsmiEditIncludeDir.Name = "tsmiEditIncludeDir";
            this.tsmiEditIncludeDir.Size = new System.Drawing.Size(117, 22);
            this.tsmiEditIncludeDir.Text = "Edit...";
            this.tsmiEditIncludeDir.Click += new System.EventHandler(this.tsmiEditIncludeDir_Click);
            // 
            // tsmiRemoveIncludeDir
            // 
            this.tsmiRemoveIncludeDir.Name = "tsmiRemoveIncludeDir";
            this.tsmiRemoveIncludeDir.Size = new System.Drawing.Size(117, 22);
            this.tsmiRemoveIncludeDir.Text = "Remove";
            this.tsmiRemoveIncludeDir.Click += new System.EventHandler(this.tsmiRemoveIncludeDir_Click);
            // 
            // gbIncludeFilter
            // 
            this.gbIncludeFilter.Controls.Add(this.tbIncludeFilter);
            this.gbIncludeFilter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gbIncludeFilter.Location = new System.Drawing.Point(3, 264);
            this.gbIncludeFilter.Name = "gbIncludeFilter";
            this.gbIncludeFilter.Size = new System.Drawing.Size(568, 50);
            this.gbIncludeFilter.TabIndex = 1;
            this.gbIncludeFilter.TabStop = false;
            this.gbIncludeFilter.Text = "Include filter";
            // 
            // tbIncludeFilter
            // 
            this.tbIncludeFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbIncludeFilter.Location = new System.Drawing.Point(6, 19);
            this.tbIncludeFilter.Name = "tbIncludeFilter";
            this.tbIncludeFilter.Size = new System.Drawing.Size(556, 20);
            this.tbIncludeFilter.TabIndex = 0;
            // 
            // WorkspaceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 379);
            this.Controls.Add(this.tcMain);
            this.Controls.Add(this.panControls);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WorkspaceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Workspace Configuration";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.WorkspaceForm_FormClosed);
            this.panControls.ResumeLayout(false);
            this.tcMain.ResumeLayout(false);
            this.tpSources.ResumeLayout(false);
            this.gbIncludeDirs.ResumeLayout(false);
            this.cmsIncludeDirs.ResumeLayout(false);
            this.gbIncludeFilter.ResumeLayout(false);
            this.gbIncludeFilter.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panControls;
        private System.Windows.Forms.TabControl tcMain;
        private System.Windows.Forms.TabPage tpSources;
        private System.Windows.Forms.GroupBox gbIncludeDirs;
        private System.Windows.Forms.GroupBox gbIncludeFilter;
        private System.Windows.Forms.TextBox tbIncludeFilter;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.ListBox lbIncludeDirs;
        private System.Windows.Forms.ContextMenuStrip cmsIncludeDirs;
        private System.Windows.Forms.ToolStripMenuItem tsmiAddIncludeDir;
        private System.Windows.Forms.ToolStripMenuItem tsmiRemoveIncludeDir;
        private System.Windows.Forms.ToolStripMenuItem tsmiEditIncludeDir;
    }
}