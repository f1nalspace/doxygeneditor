using TSP.DoxygenEditor.Natives;

namespace TSP.DoxygenEditor.Views
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
            NativeMethods.RemoveClipboardFormatListener(Handle);
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
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
            this.miEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.miEditGoTo = new System.Windows.Forms.ToolStripMenuItem();
            this.miEditGoToSymbol = new System.Windows.Forms.ToolStripMenuItem();
            this.miEditFindAndReplace = new System.Windows.Forms.ToolStripMenuItem();
            this.miEditFindAndReplaceQuickFind = new System.Windows.Forms.ToolStripMenuItem();
            this.miEditFindAndReplaceQuickReplace = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.miEditUndo = new System.Windows.Forms.ToolStripMenuItem();
            this.miEditRedo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.miEditCut = new System.Windows.Forms.ToolStripMenuItem();
            this.miEditCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.miEditPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.miEditSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.miView = new System.Windows.Forms.ToolStripMenuItem();
            this.miViewShowWhitespaces = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tbtnFileNew = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.scMainAndLog = new System.Windows.Forms.SplitContainer();
            this.scTreeAndFiles = new System.Windows.Forms.SplitContainer();
            this.tvTree = new System.Windows.Forms.TreeView();
            this.panTreeTop = new System.Windows.Forms.Panel();
            this.tcFiles = new System.Windows.Forms.TabControl();
            this.tcBottom = new System.Windows.Forms.TabControl();
            this.tpIssues = new System.Windows.Forms.TabPage();
            this.lvIssues = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imglstIcons = new System.Windows.Forms.ImageList(this.components);
            this.dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.cmsTabActions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miCurrentTabSave = new System.Windows.Forms.ToolStripMenuItem();
            this.miCurrentTabClose = new System.Windows.Forms.ToolStripMenuItem();
            this.miCurrentTabCloseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.miCurrentTabCloseAllButThis = new System.Windows.Forms.ToolStripMenuItem();
            this.dlgSaveFile = new System.Windows.Forms.SaveFileDialog();
            this.miHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.miHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scMainAndLog)).BeginInit();
            this.scMainAndLog.Panel1.SuspendLayout();
            this.scMainAndLog.Panel2.SuspendLayout();
            this.scMainAndLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scTreeAndFiles)).BeginInit();
            this.scTreeAndFiles.Panel1.SuspendLayout();
            this.scTreeAndFiles.Panel2.SuspendLayout();
            this.scTreeAndFiles.SuspendLayout();
            this.tcBottom.SuspendLayout();
            this.tpIssues.SuspendLayout();
            this.cmsTabActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFile,
            this.miEdit,
            this.miView,
            this.miHelp});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(780, 28);
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
            this.miFile.Size = new System.Drawing.Size(44, 24);
            this.miFile.Text = "File";
            // 
            // miFileNew
            // 
            this.miFileNew.Image = global::TSP.DoxygenEditor.Properties.Resources.NewFile_16x;
            this.miFileNew.Name = "miFileNew";
            this.miFileNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.miFileNew.Size = new System.Drawing.Size(225, 26);
            this.miFileNew.Text = "New";
            this.miFileNew.Click += new System.EventHandler(this.MenuActionFileNew);
            // 
            // miFileOpen
            // 
            this.miFileOpen.Image = global::TSP.DoxygenEditor.Properties.Resources.OpenFile_16x;
            this.miFileOpen.Name = "miFileOpen";
            this.miFileOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.miFileOpen.Size = new System.Drawing.Size(225, 26);
            this.miFileOpen.Text = "Open...";
            this.miFileOpen.Click += new System.EventHandler(this.MenuActionFileOpen);
            // 
            // miFileSave
            // 
            this.miFileSave.Image = global::TSP.DoxygenEditor.Properties.Resources.Save_16x;
            this.miFileSave.Name = "miFileSave";
            this.miFileSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.miFileSave.Size = new System.Drawing.Size(225, 26);
            this.miFileSave.Text = "Save";
            this.miFileSave.Click += new System.EventHandler(this.MenuActionFileSave);
            // 
            // miFileSaveAs
            // 
            this.miFileSaveAs.Image = global::TSP.DoxygenEditor.Properties.Resources.SaveAs_16x;
            this.miFileSaveAs.Name = "miFileSaveAs";
            this.miFileSaveAs.Size = new System.Drawing.Size(225, 26);
            this.miFileSaveAs.Text = "Save as...";
            this.miFileSaveAs.Click += new System.EventHandler(this.MenuActionFileSaveAs);
            // 
            // miFileSaveAll
            // 
            this.miFileSaveAll.Image = global::TSP.DoxygenEditor.Properties.Resources.SaveAll_16x;
            this.miFileSaveAll.Name = "miFileSaveAll";
            this.miFileSaveAll.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.miFileSaveAll.Size = new System.Drawing.Size(225, 26);
            this.miFileSaveAll.Text = "Save all";
            this.miFileSaveAll.Click += new System.EventHandler(this.MenuActionFileSaveAll);
            // 
            // miFileClose
            // 
            this.miFileClose.Image = global::TSP.DoxygenEditor.Properties.Resources.CloseDocument_16x;
            this.miFileClose.Name = "miFileClose";
            this.miFileClose.Size = new System.Drawing.Size(225, 26);
            this.miFileClose.Text = "Close";
            this.miFileClose.Click += new System.EventHandler(this.MenuActionFileClose);
            // 
            // miFileCloseAll
            // 
            this.miFileCloseAll.Image = global::TSP.DoxygenEditor.Properties.Resources.CloseGroup_16x;
            this.miFileCloseAll.Name = "miFileCloseAll";
            this.miFileCloseAll.Size = new System.Drawing.Size(225, 26);
            this.miFileCloseAll.Text = "Close all";
            this.miFileCloseAll.Click += new System.EventHandler(this.MenuActionFileCloseAll);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(222, 6);
            // 
            // miFileExit
            // 
            this.miFileExit.Image = global::TSP.DoxygenEditor.Properties.Resources.Exit_16x;
            this.miFileExit.Name = "miFileExit";
            this.miFileExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.miFileExit.Size = new System.Drawing.Size(225, 26);
            this.miFileExit.Text = "Exit";
            // 
            // miEdit
            // 
            this.miEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miEditGoTo,
            this.miEditFindAndReplace,
            this.toolStripMenuItem2,
            this.miEditUndo,
            this.miEditRedo,
            this.toolStripMenuItem3,
            this.miEditCut,
            this.miEditCopy,
            this.miEditPaste,
            this.toolStripMenuItem4,
            this.miEditSelectAll});
            this.miEdit.Name = "miEdit";
            this.miEdit.Size = new System.Drawing.Size(47, 24);
            this.miEdit.Text = "Edit";
            // 
            // miEditGoTo
            // 
            this.miEditGoTo.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miEditGoToSymbol});
            this.miEditGoTo.Name = "miEditGoTo";
            this.miEditGoTo.Size = new System.Drawing.Size(200, 26);
            this.miEditGoTo.Text = "Go To";
            // 
            // miEditGoToSymbol
            // 
            this.miEditGoToSymbol.Name = "miEditGoToSymbol";
            this.miEditGoToSymbol.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.miEditGoToSymbol.Size = new System.Drawing.Size(195, 26);
            this.miEditGoToSymbol.Text = "Symbol...";
            this.miEditGoToSymbol.Click += new System.EventHandler(this.MenuActionEditGoToSymbol);
            // 
            // miEditFindAndReplace
            // 
            this.miEditFindAndReplace.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miEditFindAndReplaceQuickFind,
            this.miEditFindAndReplaceQuickReplace});
            this.miEditFindAndReplace.Name = "miEditFindAndReplace";
            this.miEditFindAndReplace.Size = new System.Drawing.Size(200, 26);
            this.miEditFindAndReplace.Text = "Find And Replace";
            // 
            // miEditFindAndReplaceQuickFind
            // 
            this.miEditFindAndReplaceQuickFind.Image = global::TSP.DoxygenEditor.Properties.Resources.QuickFind_16x;
            this.miEditFindAndReplaceQuickFind.Name = "miEditFindAndReplaceQuickFind";
            this.miEditFindAndReplaceQuickFind.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.miEditFindAndReplaceQuickFind.Size = new System.Drawing.Size(231, 26);
            this.miEditFindAndReplaceQuickFind.Text = "Quick Find";
            this.miEditFindAndReplaceQuickFind.Click += new System.EventHandler(this.MenuActionEditSearchAndReplaceQuickSearch);
            // 
            // miEditFindAndReplaceQuickReplace
            // 
            this.miEditFindAndReplaceQuickReplace.Image = global::TSP.DoxygenEditor.Properties.Resources.QuickReplace_16x;
            this.miEditFindAndReplaceQuickReplace.Name = "miEditFindAndReplaceQuickReplace";
            this.miEditFindAndReplaceQuickReplace.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.miEditFindAndReplaceQuickReplace.Size = new System.Drawing.Size(231, 26);
            this.miEditFindAndReplaceQuickReplace.Text = "Quick Replace";
            this.miEditFindAndReplaceQuickReplace.Click += new System.EventHandler(this.MenuActionEditSearchAndReplaceQuickReplace);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(197, 6);
            // 
            // miEditUndo
            // 
            this.miEditUndo.Image = global::TSP.DoxygenEditor.Properties.Resources.Undo_16x;
            this.miEditUndo.Name = "miEditUndo";
            this.miEditUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.miEditUndo.Size = new System.Drawing.Size(200, 26);
            this.miEditUndo.Text = "Undo";
            this.miEditUndo.Click += new System.EventHandler(this.MenuActionEditUndo);
            // 
            // miEditRedo
            // 
            this.miEditRedo.Image = global::TSP.DoxygenEditor.Properties.Resources.Redo_16x;
            this.miEditRedo.Name = "miEditRedo";
            this.miEditRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.miEditRedo.Size = new System.Drawing.Size(200, 26);
            this.miEditRedo.Text = "Redo";
            this.miEditRedo.Click += new System.EventHandler(this.MenuActionEditRedo);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(197, 6);
            // 
            // miEditCut
            // 
            this.miEditCut.Image = global::TSP.DoxygenEditor.Properties.Resources.Cut_16x;
            this.miEditCut.Name = "miEditCut";
            this.miEditCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.miEditCut.Size = new System.Drawing.Size(200, 26);
            this.miEditCut.Text = "Cut";
            this.miEditCut.Click += new System.EventHandler(this.MenuActionEditCut);
            // 
            // miEditCopy
            // 
            this.miEditCopy.Image = global::TSP.DoxygenEditor.Properties.Resources.Copy_16x;
            this.miEditCopy.Name = "miEditCopy";
            this.miEditCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.miEditCopy.Size = new System.Drawing.Size(200, 26);
            this.miEditCopy.Text = "Copy";
            this.miEditCopy.Click += new System.EventHandler(this.MenuActionEditCopy);
            // 
            // miEditPaste
            // 
            this.miEditPaste.Image = global::TSP.DoxygenEditor.Properties.Resources.Paste_16x;
            this.miEditPaste.Name = "miEditPaste";
            this.miEditPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.miEditPaste.Size = new System.Drawing.Size(200, 26);
            this.miEditPaste.Text = "Paste";
            this.miEditPaste.Click += new System.EventHandler(this.MenuActionEditPaste);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(197, 6);
            // 
            // miEditSelectAll
            // 
            this.miEditSelectAll.Image = global::TSP.DoxygenEditor.Properties.Resources.SelectAll_16x;
            this.miEditSelectAll.Name = "miEditSelectAll";
            this.miEditSelectAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.miEditSelectAll.Size = new System.Drawing.Size(200, 26);
            this.miEditSelectAll.Text = "Select All";
            this.miEditSelectAll.Click += new System.EventHandler(this.MenuActionEditSelectAll);
            // 
            // miView
            // 
            this.miView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miViewShowWhitespaces});
            this.miView.Name = "miView";
            this.miView.Size = new System.Drawing.Size(53, 24);
            this.miView.Text = "View";
            // 
            // miViewShowWhitespaces
            // 
            this.miViewShowWhitespaces.Checked = true;
            this.miViewShowWhitespaces.CheckState = System.Windows.Forms.CheckState.Checked;
            this.miViewShowWhitespaces.Name = "miViewShowWhitespaces";
            this.miViewShowWhitespaces.Size = new System.Drawing.Size(207, 26);
            this.miViewShowWhitespaces.Text = "Show Whitespaces";
            this.miViewShowWhitespaces.Click += new System.EventHandler(this.MenuActionViewShowWhitespaces);
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbtnFileNew});
            this.toolStrip1.Location = new System.Drawing.Point(0, 28);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(780, 31);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tbtnFileNew
            // 
            this.tbtnFileNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbtnFileNew.Image = global::TSP.DoxygenEditor.Properties.Resources.NewFile_16x;
            this.tbtnFileNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnFileNew.Name = "tbtnFileNew";
            this.tbtnFileNew.Size = new System.Drawing.Size(28, 28);
            this.tbtnFileNew.Text = "toolStripButton1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Location = new System.Drawing.Point(0, 413);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 12, 0);
            this.statusStrip1.Size = new System.Drawing.Size(780, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // scMainAndLog
            // 
            this.scMainAndLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scMainAndLog.Location = new System.Drawing.Point(0, 59);
            this.scMainAndLog.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
            this.scMainAndLog.Size = new System.Drawing.Size(780, 354);
            this.scMainAndLog.SplitterDistance = 231;
            this.scMainAndLog.SplitterWidth = 3;
            this.scMainAndLog.TabIndex = 3;
            // 
            // scTreeAndFiles
            // 
            this.scTreeAndFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scTreeAndFiles.Location = new System.Drawing.Point(0, 0);
            this.scTreeAndFiles.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
            this.scTreeAndFiles.Size = new System.Drawing.Size(780, 231);
            this.scTreeAndFiles.SplitterDistance = 258;
            this.scTreeAndFiles.TabIndex = 0;
            // 
            // tvTree
            // 
            this.tvTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvTree.HideSelection = false;
            this.tvTree.Location = new System.Drawing.Point(0, 24);
            this.tvTree.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tvTree.Name = "tvTree";
            this.tvTree.Size = new System.Drawing.Size(258, 207);
            this.tvTree.TabIndex = 1;
            this.tvTree.DoubleClick += new System.EventHandler(this.tvTree_DoubleClick);
            // 
            // panTreeTop
            // 
            this.panTreeTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panTreeTop.Location = new System.Drawing.Point(0, 0);
            this.panTreeTop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panTreeTop.Name = "panTreeTop";
            this.panTreeTop.Size = new System.Drawing.Size(258, 24);
            this.panTreeTop.TabIndex = 2;
            // 
            // tcFiles
            // 
            this.tcFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcFiles.Location = new System.Drawing.Point(0, 0);
            this.tcFiles.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tcFiles.Name = "tcFiles";
            this.tcFiles.SelectedIndex = 0;
            this.tcFiles.Size = new System.Drawing.Size(518, 231);
            this.tcFiles.TabIndex = 0;
            this.tcFiles.SelectedIndexChanged += new System.EventHandler(this.tcFiles_SelectedIndexChanged);
            this.tcFiles.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tcFiles_MouseClick);
            // 
            // tcBottom
            // 
            this.tcBottom.Controls.Add(this.tpIssues);
            this.tcBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcBottom.Location = new System.Drawing.Point(0, 0);
            this.tcBottom.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tcBottom.Name = "tcBottom";
            this.tcBottom.SelectedIndex = 0;
            this.tcBottom.Size = new System.Drawing.Size(780, 120);
            this.tcBottom.TabIndex = 0;
            // 
            // tpIssues
            // 
            this.tpIssues.Controls.Add(this.lvIssues);
            this.tpIssues.Location = new System.Drawing.Point(4, 25);
            this.tpIssues.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tpIssues.Name = "tpIssues";
            this.tpIssues.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tpIssues.Size = new System.Drawing.Size(772, 91);
            this.tpIssues.TabIndex = 0;
            this.tpIssues.Text = "Issues";
            this.tpIssues.UseVisualStyleBackColor = true;
            // 
            // lvIssues
            // 
            this.lvIssues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.lvIssues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvIssues.FullRowSelect = true;
            this.lvIssues.Location = new System.Drawing.Point(3, 2);
            this.lvIssues.MultiSelect = false;
            this.lvIssues.Name = "lvIssues";
            this.lvIssues.Size = new System.Drawing.Size(766, 87);
            this.lvIssues.SmallImageList = this.imglstIcons;
            this.lvIssues.TabIndex = 0;
            this.lvIssues.UseCompatibleStateImageBehavior = false;
            this.lvIssues.View = System.Windows.Forms.View.Details;
            this.lvIssues.DoubleClick += new System.EventHandler(this.lvIssues_DoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Message";
            this.columnHeader1.Width = 200;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Symbol";
            this.columnHeader2.Width = 200;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Type";
            this.columnHeader3.Width = 100;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Group";
            this.columnHeader4.Width = 100;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "File";
            this.columnHeader5.Width = 100;
            // 
            // imglstIcons
            // 
            this.imglstIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imglstIcons.ImageStream")));
            this.imglstIcons.TransparentColor = System.Drawing.Color.Transparent;
            this.imglstIcons.Images.SetKeyName(0, "StatusCriticalError_16x.png");
            this.imglstIcons.Images.SetKeyName(1, "StatusWarning_16x.png");
            this.imglstIcons.Images.SetKeyName(2, "StatusInformation_16x.png");
            // 
            // dlgOpenFile
            // 
            this.dlgOpenFile.AddExtension = false;
            this.dlgOpenFile.Filter = "All supported files (*.h;*.docs)|*.h;*.docs|Doxygen files (*.docs)|*.docs|C Heade" +
    "r files (*.h)|*.h|All files (*.*)|*.*";
            this.dlgOpenFile.FilterIndex = 0;
            this.dlgOpenFile.Multiselect = true;
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
            this.cmsTabActions.Size = new System.Drawing.Size(188, 100);
            // 
            // miCurrentTabSave
            // 
            this.miCurrentTabSave.Name = "miCurrentTabSave";
            this.miCurrentTabSave.Size = new System.Drawing.Size(187, 24);
            this.miCurrentTabSave.Text = "Save";
            this.miCurrentTabSave.Click += new System.EventHandler(this.MenuActionFileSave);
            // 
            // miCurrentTabClose
            // 
            this.miCurrentTabClose.Name = "miCurrentTabClose";
            this.miCurrentTabClose.Size = new System.Drawing.Size(187, 24);
            this.miCurrentTabClose.Text = "Close";
            this.miCurrentTabClose.Click += new System.EventHandler(this.MenuActionFileClose);
            // 
            // miCurrentTabCloseAll
            // 
            this.miCurrentTabCloseAll.Name = "miCurrentTabCloseAll";
            this.miCurrentTabCloseAll.Size = new System.Drawing.Size(187, 24);
            this.miCurrentTabCloseAll.Text = "Close all";
            this.miCurrentTabCloseAll.Click += new System.EventHandler(this.MenuActionFileCloseAll);
            // 
            // miCurrentTabCloseAllButThis
            // 
            this.miCurrentTabCloseAllButThis.Name = "miCurrentTabCloseAllButThis";
            this.miCurrentTabCloseAllButThis.Size = new System.Drawing.Size(187, 24);
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
            // miHelp
            // 
            this.miHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miHelpAbout});
            this.miHelp.Name = "miHelp";
            this.miHelp.Size = new System.Drawing.Size(53, 24);
            this.miHelp.Text = "Help";
            // 
            // miHelpAbout
            // 
            this.miHelpAbout.Name = "miHelpAbout";
            this.miHelpAbout.Size = new System.Drawing.Size(181, 26);
            this.miHelpAbout.Text = "About...";
            this.miHelpAbout.Click += new System.EventHandler(this.MenuActionHelpAbout);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(780, 435);
            this.Controls.Add(this.scMainAndLog);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Doxygen Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.scMainAndLog.Panel1.ResumeLayout(false);
            this.scMainAndLog.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scMainAndLog)).EndInit();
            this.scMainAndLog.ResumeLayout(false);
            this.scTreeAndFiles.Panel1.ResumeLayout(false);
            this.scTreeAndFiles.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scTreeAndFiles)).EndInit();
            this.scTreeAndFiles.ResumeLayout(false);
            this.tcBottom.ResumeLayout(false);
            this.tpIssues.ResumeLayout(false);
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
        private System.Windows.Forms.TabPage tpIssues;
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
        private System.Windows.Forms.ToolStripMenuItem miEdit;
        private System.Windows.Forms.ToolStripMenuItem miEditFindAndReplace;
        private System.Windows.Forms.ToolStripMenuItem miEditFindAndReplaceQuickFind;
        private System.Windows.Forms.ToolStripMenuItem miEditFindAndReplaceQuickReplace;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem miEditUndo;
        private System.Windows.Forms.ToolStripMenuItem miEditRedo;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem miEditCut;
        private System.Windows.Forms.ToolStripMenuItem miEditCopy;
        private System.Windows.Forms.ToolStripMenuItem miEditPaste;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem miEditSelectAll;
        private System.Windows.Forms.ToolStripMenuItem miView;
        private System.Windows.Forms.ToolStripMenuItem miViewShowWhitespaces;
        private System.Windows.Forms.ToolStripMenuItem miEditGoTo;
        private System.Windows.Forms.ToolStripMenuItem miEditGoToSymbol;
        private System.Windows.Forms.ToolStripButton tbtnFileNew;
        private System.Windows.Forms.ListView lvIssues;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ImageList imglstIcons;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ToolStripMenuItem miHelp;
        private System.Windows.Forms.ToolStripMenuItem miHelpAbout;
    }
}