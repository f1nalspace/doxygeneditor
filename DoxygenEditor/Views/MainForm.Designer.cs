namespace DoxygenEditor.Views
{
    partial class MainForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.miFile = new System.Windows.Forms.ToolStripMenuItem();
            this.miFileNew = new System.Windows.Forms.ToolStripMenuItem();
            this.miFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.miFileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.miFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.miFileSaveAll = new System.Windows.Forms.ToolStripMenuItem();
            this.miFileClose = new System.Windows.Forms.ToolStripMenuItem();
            this.miFileCloseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.miFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.scMainAndLog = new System.Windows.Forms.SplitContainer();
            this.scTreeAndFiles = new System.Windows.Forms.SplitContainer();
            this.tvTree = new System.Windows.Forms.TreeView();
            this.panTreeTop = new System.Windows.Forms.Panel();
            this.tcFiles = new System.Windows.Forms.TabControl();
            this.tcBottom = new System.Windows.Forms.TabControl();
            this.tpLog = new System.Windows.Forms.TabPage();
            this.dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.cmsTabActions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miCurrentTabSave = new System.Windows.Forms.ToolStripMenuItem();
            this.miCurrentTabClose = new System.Windows.Forms.ToolStripMenuItem();
            this.miCurrentTabCloseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.miCurrentTabCloseAllButThis = new System.Windows.Forms.ToolStripMenuItem();
            this.dlgSaveFile = new System.Windows.Forms.SaveFileDialog();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scMainAndLog)).BeginInit();
            this.scMainAndLog.Panel1.SuspendLayout();
            this.scMainAndLog.Panel2.SuspendLayout();
            this.scMainAndLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scTreeAndFiles)).BeginInit();
            this.scTreeAndFiles.Panel1.SuspendLayout();
            this.scTreeAndFiles.Panel2.SuspendLayout();
            this.scTreeAndFiles.SuspendLayout();
            this.tcBottom.SuspendLayout();
            this.cmsTabActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFile});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(878, 33);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // miFile
            // 
            this.miFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFileNew,
            this.miFileOpen,
            this.miFileSave,
            this.miFileSaveAs,
            this.miFileSaveAll,
            this.miFileClose,
            this.miFileCloseAll,
            this.toolStripMenuItem1,
            this.miFileExit});
            this.miFile.Name = "miFile";
            this.miFile.Size = new System.Drawing.Size(50, 29);
            this.miFile.Text = "File";
            // 
            // miFileNew
            // 
            this.miFileNew.Name = "miFileNew";
            this.miFileNew.Size = new System.Drawing.Size(167, 30);
            this.miFileNew.Text = "New";
            this.miFileNew.Click += new System.EventHandler(this.MenuActionFileNew);
            // 
            // miFileOpen
            // 
            this.miFileOpen.Name = "miFileOpen";
            this.miFileOpen.Size = new System.Drawing.Size(167, 30);
            this.miFileOpen.Text = "Open...";
            this.miFileOpen.Click += new System.EventHandler(this.MenuActionFileOpen);
            // 
            // miFileSave
            // 
            this.miFileSave.Name = "miFileSave";
            this.miFileSave.Size = new System.Drawing.Size(167, 30);
            this.miFileSave.Text = "Save";
            this.miFileSave.Click += new System.EventHandler(this.MenuActionFileSave);
            // 
            // miFileSaveAs
            // 
            this.miFileSaveAs.Name = "miFileSaveAs";
            this.miFileSaveAs.Size = new System.Drawing.Size(167, 30);
            this.miFileSaveAs.Text = "Save as...";
            this.miFileSaveAs.Click += new System.EventHandler(this.MenuActionFileSaveAs);
            // 
            // miFileSaveAll
            // 
            this.miFileSaveAll.Name = "miFileSaveAll";
            this.miFileSaveAll.Size = new System.Drawing.Size(167, 30);
            this.miFileSaveAll.Text = "Save all";
            // 
            // miFileClose
            // 
            this.miFileClose.Name = "miFileClose";
            this.miFileClose.Size = new System.Drawing.Size(167, 30);
            this.miFileClose.Text = "Close";
            this.miFileClose.Click += new System.EventHandler(this.MenuActionFileClose);
            // 
            // miFileCloseAll
            // 
            this.miFileCloseAll.Name = "miFileCloseAll";
            this.miFileCloseAll.Size = new System.Drawing.Size(167, 30);
            this.miFileCloseAll.Text = "Close all";
            this.miFileCloseAll.Click += new System.EventHandler(this.MenuActionFileCloseAll);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(164, 6);
            // 
            // miFileExit
            // 
            this.miFileExit.Name = "miFileExit";
            this.miFileExit.Size = new System.Drawing.Size(167, 30);
            this.miFileExit.Text = "Exit";
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Location = new System.Drawing.Point(0, 33);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(878, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Location = new System.Drawing.Point(0, 522);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(878, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // scMainAndLog
            // 
            this.scMainAndLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scMainAndLog.Location = new System.Drawing.Point(0, 58);
            this.scMainAndLog.Name = "scMainAndLog";
            this.scMainAndLog.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scMainAndLog.Panel1
            // 
            this.scMainAndLog.Panel1.Controls.Add(this.scTreeAndFiles);
            // 
            // scMainAndLog.Panel2
            // 
            this.scMainAndLog.Panel2.Controls.Add(this.tcBottom);
            this.scMainAndLog.Size = new System.Drawing.Size(878, 464);
            this.scMainAndLog.SplitterDistance = 333;
            this.scMainAndLog.TabIndex = 3;
            // 
            // scTreeAndFiles
            // 
            this.scTreeAndFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scTreeAndFiles.Location = new System.Drawing.Point(0, 0);
            this.scTreeAndFiles.Name = "scTreeAndFiles";
            // 
            // scTreeAndFiles.Panel1
            // 
            this.scTreeAndFiles.Panel1.Controls.Add(this.tvTree);
            this.scTreeAndFiles.Panel1.Controls.Add(this.panTreeTop);
            // 
            // scTreeAndFiles.Panel2
            // 
            this.scTreeAndFiles.Panel2.Controls.Add(this.tcFiles);
            this.scTreeAndFiles.Size = new System.Drawing.Size(878, 333);
            this.scTreeAndFiles.SplitterDistance = 291;
            this.scTreeAndFiles.TabIndex = 0;
            // 
            // tvTree
            // 
            this.tvTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvTree.Location = new System.Drawing.Point(0, 30);
            this.tvTree.Name = "tvTree";
            this.tvTree.Size = new System.Drawing.Size(291, 303);
            this.tvTree.TabIndex = 1;
            // 
            // panTreeTop
            // 
            this.panTreeTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panTreeTop.Location = new System.Drawing.Point(0, 0);
            this.panTreeTop.Name = "panTreeTop";
            this.panTreeTop.Size = new System.Drawing.Size(291, 30);
            this.panTreeTop.TabIndex = 2;
            // 
            // tcFiles
            // 
            this.tcFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcFiles.Location = new System.Drawing.Point(0, 0);
            this.tcFiles.Name = "tcFiles";
            this.tcFiles.SelectedIndex = 0;
            this.tcFiles.Size = new System.Drawing.Size(583, 333);
            this.tcFiles.TabIndex = 0;
            this.tcFiles.SelectedIndexChanged += new System.EventHandler(this.tcFiles_SelectedIndexChanged);
            this.tcFiles.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tcFiles_MouseClick);
            // 
            // tcBottom
            // 
            this.tcBottom.Controls.Add(this.tpLog);
            this.tcBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcBottom.Location = new System.Drawing.Point(0, 0);
            this.tcBottom.Name = "tcBottom";
            this.tcBottom.SelectedIndex = 0;
            this.tcBottom.Size = new System.Drawing.Size(878, 127);
            this.tcBottom.TabIndex = 0;
            // 
            // tpLog
            // 
            this.tpLog.Location = new System.Drawing.Point(4, 29);
            this.tpLog.Name = "tpLog";
            this.tpLog.Padding = new System.Windows.Forms.Padding(3);
            this.tpLog.Size = new System.Drawing.Size(870, 94);
            this.tpLog.TabIndex = 0;
            this.tpLog.Text = "tabPage1";
            this.tpLog.UseVisualStyleBackColor = true;
            // 
            // dlgOpenFile
            // 
            this.dlgOpenFile.AddExtension = false;
            this.dlgOpenFile.Filter = "Doxygen files (*.docs)|*.docs|All files (*.*)|*.*";
            this.dlgOpenFile.FilterIndex = 0;
            this.dlgOpenFile.Title = "Open file";
            // 
            // cmsTabActions
            // 
            this.cmsTabActions.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.cmsTabActions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miCurrentTabSave,
            this.miCurrentTabClose,
            this.miCurrentTabCloseAll,
            this.miCurrentTabCloseAllButThis});
            this.cmsTabActions.Name = "cmsTabActions";
            this.cmsTabActions.Size = new System.Drawing.Size(215, 124);
            // 
            // miCurrentTabSave
            // 
            this.miCurrentTabSave.Name = "miCurrentTabSave";
            this.miCurrentTabSave.Size = new System.Drawing.Size(214, 30);
            this.miCurrentTabSave.Text = "Save";
            this.miCurrentTabSave.Click += new System.EventHandler(this.MenuActionFileSave);
            // 
            // miCurrentTabClose
            // 
            this.miCurrentTabClose.Name = "miCurrentTabClose";
            this.miCurrentTabClose.Size = new System.Drawing.Size(214, 30);
            this.miCurrentTabClose.Text = "Close";
            this.miCurrentTabClose.Click += new System.EventHandler(this.MenuActionFileClose);
            // 
            // miCurrentTabCloseAll
            // 
            this.miCurrentTabCloseAll.Name = "miCurrentTabCloseAll";
            this.miCurrentTabCloseAll.Size = new System.Drawing.Size(214, 30);
            this.miCurrentTabCloseAll.Text = "Close all";
            this.miCurrentTabCloseAll.Click += new System.EventHandler(this.MenuActionFileCloseAll);
            // 
            // miCurrentTabCloseAllButThis
            // 
            this.miCurrentTabCloseAllButThis.Name = "miCurrentTabCloseAllButThis";
            this.miCurrentTabCloseAllButThis.Size = new System.Drawing.Size(214, 30);
            this.miCurrentTabCloseAllButThis.Text = "Close all but this";
            this.miCurrentTabCloseAllButThis.Click += new System.EventHandler(this.MenuActionFileCloseAllButThis);
            // 
            // dlgSaveFile
            // 
            this.dlgSaveFile.DefaultExt = "docs";
            this.dlgSaveFile.Filter = "Doxygen files (*.docs)|*.docs|C Header file (*.h)|*.h";
            this.dlgSaveFile.FilterIndex = 0;
            this.dlgSaveFile.Title = "Save file";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(878, 544);
            this.Controls.Add(this.scMainAndLog);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.scMainAndLog.Panel1.ResumeLayout(false);
            this.scMainAndLog.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scMainAndLog)).EndInit();
            this.scMainAndLog.ResumeLayout(false);
            this.scTreeAndFiles.Panel1.ResumeLayout(false);
            this.scTreeAndFiles.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scTreeAndFiles)).EndInit();
            this.scTreeAndFiles.ResumeLayout(false);
            this.tcBottom.ResumeLayout(false);
            this.cmsTabActions.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem miFile;
        private System.Windows.Forms.ToolStripMenuItem miFileExit;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.SplitContainer scMainAndLog;
        private System.Windows.Forms.SplitContainer scTreeAndFiles;
        private System.Windows.Forms.TabControl tcBottom;
        private System.Windows.Forms.TabPage tpLog;
        private System.Windows.Forms.TabControl tcFiles;
        private System.Windows.Forms.TreeView tvTree;
        private System.Windows.Forms.Panel panTreeTop;
        private System.Windows.Forms.ToolStripMenuItem miFileNew;
        private System.Windows.Forms.ToolStripMenuItem miFileOpen;
        private System.Windows.Forms.ToolStripMenuItem miFileSave;
        private System.Windows.Forms.ToolStripMenuItem miFileSaveAs;
        private System.Windows.Forms.ToolStripMenuItem miFileSaveAll;
        private System.Windows.Forms.ToolStripMenuItem miFileClose;
        private System.Windows.Forms.ToolStripMenuItem miFileCloseAll;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.OpenFileDialog dlgOpenFile;
        private System.Windows.Forms.ContextMenuStrip cmsTabActions;
        private System.Windows.Forms.ToolStripMenuItem miCurrentTabClose;
        private System.Windows.Forms.SaveFileDialog dlgSaveFile;
        private System.Windows.Forms.ToolStripMenuItem miCurrentTabCloseAll;
        private System.Windows.Forms.ToolStripMenuItem miCurrentTabCloseAllButThis;
        private System.Windows.Forms.ToolStripMenuItem miCurrentTabSave;
    }
}