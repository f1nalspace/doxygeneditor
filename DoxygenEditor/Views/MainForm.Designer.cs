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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lastStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolLabelStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lastParsedLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.lastLexedLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tvTree = new System.Windows.Forms.TreeView();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuItemFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemFileNew = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemFileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemEditFindReplace = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemEditFindReplaceQuickSearch = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemEditFindReplaceQuickReplace = new System.Windows.Forms.ToolStripMenuItem();
            this.searchSymbolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemEditUndo = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemEditRedo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemEditCut = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemEditCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemEditPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemEditSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemView = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemViewShowWhitespaces = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolButtonFileNew = new System.Windows.Forms.ToolStripButton();
            this.toolButtonFileOpen = new System.Windows.Forms.ToolStripButton();
            this.toolButtonFileSave = new System.Windows.Forms.ToolStripButton();
            this.toolButtonFileSaveAs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolButtonEditUndo = new System.Windows.Forms.ToolStripButton();
            this.toolButtonEditRedo = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lastStatusLabel,
            this.toolLabelStatus,
            this.lastParsedLabel,
            this.lastLexedLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 423);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1111, 29);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lastStatusLabel
            // 
            this.lastStatusLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.lastStatusLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
            this.lastStatusLabel.Name = "lastStatusLabel";
            this.lastStatusLabel.Size = new System.Drawing.Size(63, 24);
            this.lastStatusLabel.Text = "[Status]";
            // 
            // toolLabelStatus
            // 
            this.toolLabelStatus.Name = "toolLabelStatus";
            this.toolLabelStatus.Size = new System.Drawing.Size(0, 24);
            // 
            // lastParsedLabel
            // 
            this.lastParsedLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.lastParsedLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
            this.lastParsedLabel.Name = "lastParsedLabel";
            this.lastParsedLabel.Size = new System.Drawing.Size(57, 24);
            this.lastParsedLabel.Text = "[Parse]";
            // 
            // lastLexedLabel
            // 
            this.lastLexedLabel.Name = "lastLexedLabel";
            this.lastLexedLabel.Size = new System.Drawing.Size(54, 24);
            this.lastLexedLabel.Text = "[Lexer]";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 55);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvTree);
            this.splitContainer1.Size = new System.Drawing.Size(1111, 368);
            this.splitContainer1.SplitterDistance = 369;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 2;
            // 
            // tvTree
            // 
            this.tvTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvTree.HideSelection = false;
            this.tvTree.Location = new System.Drawing.Point(0, 0);
            this.tvTree.Margin = new System.Windows.Forms.Padding(4);
            this.tvTree.Name = "tvTree";
            this.tvTree.Size = new System.Drawing.Size(369, 368);
            this.tvTree.TabIndex = 0;
            this.tvTree.TabStop = false;
            this.tvTree.DoubleClick += new System.EventHandler(this.tvTree_DoubleClick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemFile,
            this.menuItemEdit,
            this.menuItemView,
            this.menuItemHelp});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1111, 28);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // menuItemFile
            // 
            this.menuItemFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemFileNew,
            this.menuItemFileOpen,
            this.menuItemFileSave,
            this.menuItemFileSaveAs,
            this.toolStripMenuItem1,
            this.menuItemFileExit});
            this.menuItemFile.Name = "menuItemFile";
            this.menuItemFile.Size = new System.Drawing.Size(44, 24);
            this.menuItemFile.Text = "File";
            // 
            // menuItemFileNew
            // 
            this.menuItemFileNew.Image = ((System.Drawing.Image)(resources.GetObject("menuItemFileNew.Image")));
            this.menuItemFileNew.Name = "menuItemFileNew";
            this.menuItemFileNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.menuItemFileNew.Size = new System.Drawing.Size(182, 26);
            this.menuItemFileNew.Text = "New";
            // 
            // menuItemFileOpen
            // 
            this.menuItemFileOpen.Image = ((System.Drawing.Image)(resources.GetObject("menuItemFileOpen.Image")));
            this.menuItemFileOpen.Name = "menuItemFileOpen";
            this.menuItemFileOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.menuItemFileOpen.Size = new System.Drawing.Size(182, 26);
            this.menuItemFileOpen.Text = "Open...";
            // 
            // menuItemFileSave
            // 
            this.menuItemFileSave.Image = ((System.Drawing.Image)(resources.GetObject("menuItemFileSave.Image")));
            this.menuItemFileSave.Name = "menuItemFileSave";
            this.menuItemFileSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.menuItemFileSave.Size = new System.Drawing.Size(182, 26);
            this.menuItemFileSave.Text = "Save";
            // 
            // menuItemFileSaveAs
            // 
            this.menuItemFileSaveAs.Image = ((System.Drawing.Image)(resources.GetObject("menuItemFileSaveAs.Image")));
            this.menuItemFileSaveAs.Name = "menuItemFileSaveAs";
            this.menuItemFileSaveAs.Size = new System.Drawing.Size(182, 26);
            this.menuItemFileSaveAs.Text = "Save as...";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(179, 6);
            // 
            // menuItemFileExit
            // 
            this.menuItemFileExit.Name = "menuItemFileExit";
            this.menuItemFileExit.Size = new System.Drawing.Size(182, 26);
            this.menuItemFileExit.Text = "Exit";
            // 
            // menuItemEdit
            // 
            this.menuItemEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemEditFindReplace,
            this.searchSymbolsToolStripMenuItem,
            this.toolStripMenuItem4,
            this.menuItemEditUndo,
            this.menuItemEditRedo,
            this.toolStripMenuItem2,
            this.menuItemEditCut,
            this.menuItemEditCopy,
            this.menuItemEditPaste,
            this.toolStripMenuItem3,
            this.menuItemEditSelectAll});
            this.menuItemEdit.Name = "menuItemEdit";
            this.menuItemEdit.Size = new System.Drawing.Size(47, 24);
            this.menuItemEdit.Text = "Edit";
            // 
            // menuItemEditFindReplace
            // 
            this.menuItemEditFindReplace.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemEditFindReplaceQuickSearch,
            this.menuItemEditFindReplaceQuickReplace});
            this.menuItemEditFindReplace.Name = "menuItemEditFindReplace";
            this.menuItemEditFindReplace.Size = new System.Drawing.Size(247, 26);
            this.menuItemEditFindReplace.Text = "Find && Replace";
            // 
            // menuItemEditFindReplaceQuickSearch
            // 
            this.menuItemEditFindReplaceQuickSearch.Name = "menuItemEditFindReplaceQuickSearch";
            this.menuItemEditFindReplaceQuickSearch.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.menuItemEditFindReplaceQuickSearch.Size = new System.Drawing.Size(238, 26);
            this.menuItemEditFindReplaceQuickSearch.Text = "Quick Search...";
            // 
            // menuItemEditFindReplaceQuickReplace
            // 
            this.menuItemEditFindReplaceQuickReplace.Name = "menuItemEditFindReplaceQuickReplace";
            this.menuItemEditFindReplaceQuickReplace.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.menuItemEditFindReplaceQuickReplace.Size = new System.Drawing.Size(238, 26);
            this.menuItemEditFindReplaceQuickReplace.Text = "Quick Replace...";
            // 
            // searchSymbolsToolStripMenuItem
            // 
            this.searchSymbolsToolStripMenuItem.Name = "searchSymbolsToolStripMenuItem";
            this.searchSymbolsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.searchSymbolsToolStripMenuItem.Size = new System.Drawing.Size(247, 26);
            this.searchSymbolsToolStripMenuItem.Text = "Search symbols...";
            this.searchSymbolsToolStripMenuItem.Click += new System.EventHandler(this.searchSymbolsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(244, 6);
            // 
            // menuItemEditUndo
            // 
            this.menuItemEditUndo.Image = ((System.Drawing.Image)(resources.GetObject("menuItemEditUndo.Image")));
            this.menuItemEditUndo.Name = "menuItemEditUndo";
            this.menuItemEditUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.menuItemEditUndo.Size = new System.Drawing.Size(247, 26);
            this.menuItemEditUndo.Text = "Undo";
            // 
            // menuItemEditRedo
            // 
            this.menuItemEditRedo.Image = ((System.Drawing.Image)(resources.GetObject("menuItemEditRedo.Image")));
            this.menuItemEditRedo.Name = "menuItemEditRedo";
            this.menuItemEditRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.menuItemEditRedo.Size = new System.Drawing.Size(247, 26);
            this.menuItemEditRedo.Text = "Redo";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(244, 6);
            // 
            // menuItemEditCut
            // 
            this.menuItemEditCut.Name = "menuItemEditCut";
            this.menuItemEditCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.menuItemEditCut.Size = new System.Drawing.Size(247, 26);
            this.menuItemEditCut.Text = "Cut";
            // 
            // menuItemEditCopy
            // 
            this.menuItemEditCopy.Name = "menuItemEditCopy";
            this.menuItemEditCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.menuItemEditCopy.Size = new System.Drawing.Size(247, 26);
            this.menuItemEditCopy.Text = "Copy";
            // 
            // menuItemEditPaste
            // 
            this.menuItemEditPaste.Name = "menuItemEditPaste";
            this.menuItemEditPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.menuItemEditPaste.Size = new System.Drawing.Size(247, 26);
            this.menuItemEditPaste.Text = "Paste";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(244, 6);
            // 
            // menuItemEditSelectAll
            // 
            this.menuItemEditSelectAll.Name = "menuItemEditSelectAll";
            this.menuItemEditSelectAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.menuItemEditSelectAll.Size = new System.Drawing.Size(247, 26);
            this.menuItemEditSelectAll.Text = "Select All";
            // 
            // menuItemView
            // 
            this.menuItemView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemViewShowWhitespaces});
            this.menuItemView.Name = "menuItemView";
            this.menuItemView.Size = new System.Drawing.Size(53, 24);
            this.menuItemView.Text = "View";
            // 
            // menuItemViewShowWhitespaces
            // 
            this.menuItemViewShowWhitespaces.Name = "menuItemViewShowWhitespaces";
            this.menuItemViewShowWhitespaces.Size = new System.Drawing.Size(204, 26);
            this.menuItemViewShowWhitespaces.Text = "Show whitespaces";
            // 
            // menuItemHelp
            // 
            this.menuItemHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemHelpAbout});
            this.menuItemHelp.Name = "menuItemHelp";
            this.menuItemHelp.Size = new System.Drawing.Size(53, 24);
            this.menuItemHelp.Text = "Help";
            // 
            // menuItemHelpAbout
            // 
            this.menuItemHelpAbout.Name = "menuItemHelpAbout";
            this.menuItemHelpAbout.Size = new System.Drawing.Size(125, 26);
            this.menuItemHelpAbout.Text = "About";
            this.menuItemHelpAbout.Click += new System.EventHandler(this.menuItemHelpAbout_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolButtonFileNew,
            this.toolButtonFileOpen,
            this.toolButtonFileSave,
            this.toolButtonFileSaveAs,
            this.toolStripSeparator1,
            this.toolButtonEditUndo,
            this.toolButtonEditRedo});
            this.toolStrip1.Location = new System.Drawing.Point(0, 28);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1111, 27);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolButtonFileNew
            // 
            this.toolButtonFileNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButtonFileNew.Image = ((System.Drawing.Image)(resources.GetObject("toolButtonFileNew.Image")));
            this.toolButtonFileNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButtonFileNew.Name = "toolButtonFileNew";
            this.toolButtonFileNew.Size = new System.Drawing.Size(24, 24);
            this.toolButtonFileNew.Text = "New file";
            // 
            // toolButtonFileOpen
            // 
            this.toolButtonFileOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButtonFileOpen.Image = ((System.Drawing.Image)(resources.GetObject("toolButtonFileOpen.Image")));
            this.toolButtonFileOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButtonFileOpen.Name = "toolButtonFileOpen";
            this.toolButtonFileOpen.Size = new System.Drawing.Size(24, 24);
            this.toolButtonFileOpen.Text = "Open file...";
            // 
            // toolButtonFileSave
            // 
            this.toolButtonFileSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButtonFileSave.Image = ((System.Drawing.Image)(resources.GetObject("toolButtonFileSave.Image")));
            this.toolButtonFileSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButtonFileSave.Name = "toolButtonFileSave";
            this.toolButtonFileSave.Size = new System.Drawing.Size(24, 24);
            this.toolButtonFileSave.Text = "Save file";
            // 
            // toolButtonFileSaveAs
            // 
            this.toolButtonFileSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButtonFileSaveAs.Image = ((System.Drawing.Image)(resources.GetObject("toolButtonFileSaveAs.Image")));
            this.toolButtonFileSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButtonFileSaveAs.Name = "toolButtonFileSaveAs";
            this.toolButtonFileSaveAs.Size = new System.Drawing.Size(24, 24);
            this.toolButtonFileSaveAs.Text = "Save file as...";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // toolButtonEditUndo
            // 
            this.toolButtonEditUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButtonEditUndo.Image = ((System.Drawing.Image)(resources.GetObject("toolButtonEditUndo.Image")));
            this.toolButtonEditUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButtonEditUndo.Name = "toolButtonEditUndo";
            this.toolButtonEditUndo.Size = new System.Drawing.Size(24, 24);
            this.toolButtonEditUndo.Text = "Undo";
            // 
            // toolButtonEditRedo
            // 
            this.toolButtonEditRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButtonEditRedo.Image = ((System.Drawing.Image)(resources.GetObject("toolButtonEditRedo.Image")));
            this.toolButtonEditRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButtonEditRedo.Name = "toolButtonEditRedo";
            this.toolButtonEditRedo.Size = new System.Drawing.Size(24, 24);
            this.toolButtonEditRedo.Text = "Redo";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1111, 452);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            this.Text = "DoxygenEditor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView tvTree;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem menuItemFile;
        private System.Windows.Forms.ToolStripMenuItem menuItemFileExit;
        private System.Windows.Forms.ToolStripMenuItem menuItemFileOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripStatusLabel lastStatusLabel;
        private System.Windows.Forms.ToolStripMenuItem menuItemFileNew;
        private System.Windows.Forms.ToolStripMenuItem menuItemFileSave;
        private System.Windows.Forms.ToolStripMenuItem menuItemFileSaveAs;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolButtonFileOpen;
        private System.Windows.Forms.ToolStripButton toolButtonFileSave;
        private System.Windows.Forms.ToolStripStatusLabel toolLabelStatus;
        private System.Windows.Forms.ToolStripMenuItem menuItemEdit;
        private System.Windows.Forms.ToolStripMenuItem menuItemEditUndo;
        private System.Windows.Forms.ToolStripMenuItem menuItemEditRedo;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem menuItemEditCut;
        private System.Windows.Forms.ToolStripMenuItem menuItemEditCopy;
        private System.Windows.Forms.ToolStripMenuItem menuItemEditPaste;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem menuItemEditSelectAll;
        private System.Windows.Forms.ToolStripMenuItem menuItemEditFindReplace;
        private System.Windows.Forms.ToolStripMenuItem menuItemEditFindReplaceQuickSearch;
        private System.Windows.Forms.ToolStripMenuItem menuItemEditFindReplaceQuickReplace;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem menuItemView;
        private System.Windows.Forms.ToolStripMenuItem menuItemViewShowWhitespaces;
        private System.Windows.Forms.ToolStripMenuItem menuItemHelp;
        private System.Windows.Forms.ToolStripMenuItem menuItemHelpAbout;
        private System.Windows.Forms.ToolStripMenuItem searchSymbolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolButtonFileSaveAs;
        private System.Windows.Forms.ToolStripButton toolButtonFileNew;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolButtonEditUndo;
        private System.Windows.Forms.ToolStripButton toolButtonEditRedo;
        private System.Windows.Forms.ToolStripStatusLabel lastParsedLabel;
        private System.Windows.Forms.ToolStripStatusLabel lastLexedLabel;
    }
}

