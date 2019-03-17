using TSP.DoxygenEditor.FilterControls;
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
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.miFile = new System.Windows.Forms.ToolStripMenuItem();
            this.miFileNew = new System.Windows.Forms.ToolStripMenuItem();
            this.miFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.miFileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.miFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.miFileSaveAll = new System.Windows.Forms.ToolStripMenuItem();
            this.miFileClose = new System.Windows.Forms.ToolStripMenuItem();
            this.miFileCloseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
            this.miFileRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.miFileRecentFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
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
            this.miWorkspace = new System.Windows.Forms.ToolStripMenuItem();
            this.miWorkspaceConfiguration = new System.Windows.Forms.ToolStripMenuItem();
            this.miHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.miHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMain = new System.Windows.Forms.ToolStrip();
            this.tbtnFileNew = new System.Windows.Forms.ToolStripButton();
            this.tbtnFileOpen = new System.Windows.Forms.ToolStripButton();
            this.tbtnFileSave = new System.Windows.Forms.ToolStripButton();
            this.tbtnFileSaveAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tbtnFileRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tbtnEditUndo = new System.Windows.Forms.ToolStripButton();
            this.tbtnEditRedo = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsslblParseStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.scMainAndLog = new System.Windows.Forms.SplitContainer();
            this.scTreeAndFiles = new System.Windows.Forms.SplitContainer();
            this.tvTree = new System.Windows.Forms.TreeView();
            this.panTreeTop = new System.Windows.Forms.Panel();
            this.tcFiles = new System.Windows.Forms.TabControl();
            this.tcBottom = new System.Windows.Forms.TabControl();
            this.tpDoxygenIssues = new System.Windows.Forms.TabPage();
            this.tpCppIssues = new System.Windows.Forms.TabPage();
            this.tpPerformance = new System.Windows.Forms.TabPage();
            this.lvPerformance = new System.Windows.Forms.ListView();
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader14 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader15 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader17 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imglstIcons = new System.Windows.Forms.ImageList(this.components);
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.cmsTabActions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miCurrentTabSave = new System.Windows.Forms.ToolStripMenuItem();
            this.miCurrentTabClose = new System.Windows.Forms.ToolStripMenuItem();
            this.miCurrentTabCloseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.miCurrentTabCloseAllButThis = new System.Windows.Forms.ToolStripMenuItem();
            this.dlgSaveFile = new System.Windows.Forms.SaveFileDialog();
            this.tsslblIncludeParseState = new System.Windows.Forms.ToolStripStatusLabel();
            this.tspbIncludeStateProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.mainMenuStrip.SuspendLayout();
            this.tsMain.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scMainAndLog)).BeginInit();
            this.scMainAndLog.Panel1.SuspendLayout();
            this.scMainAndLog.Panel2.SuspendLayout();
            this.scMainAndLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scTreeAndFiles)).BeginInit();
            this.scTreeAndFiles.Panel1.SuspendLayout();
            this.scTreeAndFiles.Panel2.SuspendLayout();
            this.scTreeAndFiles.SuspendLayout();
            this.tcBottom.SuspendLayout();
            this.tpPerformance.SuspendLayout();
            this.cmsTabActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFile,
            this.miEdit,
            this.miView,
            this.miWorkspace,
            this.miHelp});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.mainMenuStrip.Size = new System.Drawing.Size(736, 24);
            this.mainMenuStrip.TabIndex = 0;
            this.mainMenuStrip.Text = "menuStrip1";
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
            this.toolStripMenuItem7,
            this.miFileRefresh,
            this.toolStripMenuItem1,
            this.miFileRecentFiles,
            this.toolStripMenuItem5,
            this.miFileExit});
            this.miFile.Name = "miFile";
            this.miFile.Size = new System.Drawing.Size(37, 20);
            this.miFile.Text = "File";
            // 
            // miFileNew
            // 
            this.miFileNew.Image = global::TSP.DoxygenEditor.Properties.Resources.NewFile_16x;
            this.miFileNew.Name = "miFileNew";
            this.miFileNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.miFileNew.Size = new System.Drawing.Size(185, 22);
            this.miFileNew.Text = "New";
            this.miFileNew.Click += new System.EventHandler(this.MenuActionFileNew);
            // 
            // miFileOpen
            // 
            this.miFileOpen.Image = global::TSP.DoxygenEditor.Properties.Resources.OpenFile_16x;
            this.miFileOpen.Name = "miFileOpen";
            this.miFileOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.miFileOpen.Size = new System.Drawing.Size(185, 22);
            this.miFileOpen.Text = "Open...";
            this.miFileOpen.Click += new System.EventHandler(this.MenuActionFileOpen);
            // 
            // miFileSave
            // 
            this.miFileSave.Image = global::TSP.DoxygenEditor.Properties.Resources.Save_16x;
            this.miFileSave.Name = "miFileSave";
            this.miFileSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.miFileSave.Size = new System.Drawing.Size(185, 22);
            this.miFileSave.Text = "Save";
            this.miFileSave.Click += new System.EventHandler(this.MenuActionFileSave);
            // 
            // miFileSaveAs
            // 
            this.miFileSaveAs.Image = global::TSP.DoxygenEditor.Properties.Resources.SaveAs_16x;
            this.miFileSaveAs.Name = "miFileSaveAs";
            this.miFileSaveAs.Size = new System.Drawing.Size(185, 22);
            this.miFileSaveAs.Text = "Save as...";
            this.miFileSaveAs.Click += new System.EventHandler(this.MenuActionFileSaveAs);
            // 
            // miFileSaveAll
            // 
            this.miFileSaveAll.Image = global::TSP.DoxygenEditor.Properties.Resources.SaveAll_16x;
            this.miFileSaveAll.Name = "miFileSaveAll";
            this.miFileSaveAll.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.miFileSaveAll.Size = new System.Drawing.Size(185, 22);
            this.miFileSaveAll.Text = "Save all";
            this.miFileSaveAll.Click += new System.EventHandler(this.MenuActionFileSaveAll);
            // 
            // miFileClose
            // 
            this.miFileClose.Image = global::TSP.DoxygenEditor.Properties.Resources.CloseDocument_16x;
            this.miFileClose.Name = "miFileClose";
            this.miFileClose.Size = new System.Drawing.Size(185, 22);
            this.miFileClose.Text = "Close";
            this.miFileClose.Click += new System.EventHandler(this.MenuActionFileClose);
            // 
            // miFileCloseAll
            // 
            this.miFileCloseAll.Image = global::TSP.DoxygenEditor.Properties.Resources.CloseGroup_16x;
            this.miFileCloseAll.Name = "miFileCloseAll";
            this.miFileCloseAll.Size = new System.Drawing.Size(185, 22);
            this.miFileCloseAll.Text = "Close all";
            this.miFileCloseAll.Click += new System.EventHandler(this.MenuActionFileCloseAll);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(182, 6);
            // 
            // miFileRefresh
            // 
            this.miFileRefresh.Image = global::TSP.DoxygenEditor.Properties.Resources.Refresh_16x;
            this.miFileRefresh.Name = "miFileRefresh";
            this.miFileRefresh.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.miFileRefresh.Size = new System.Drawing.Size(185, 22);
            this.miFileRefresh.Text = "Refresh";
            this.miFileRefresh.Click += new System.EventHandler(this.MenuActionFileRefresh);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(182, 6);
            // 
            // miFileRecentFiles
            // 
            this.miFileRecentFiles.Name = "miFileRecentFiles";
            this.miFileRecentFiles.Size = new System.Drawing.Size(185, 22);
            this.miFileRecentFiles.Text = "Recent Files";
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(182, 6);
            // 
            // miFileExit
            // 
            this.miFileExit.Image = global::TSP.DoxygenEditor.Properties.Resources.Exit_16x;
            this.miFileExit.Name = "miFileExit";
            this.miFileExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.miFileExit.Size = new System.Drawing.Size(185, 22);
            this.miFileExit.Text = "Exit";
            this.miFileExit.Click += new System.EventHandler(this.MenuActionFileExit);
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
            this.miEdit.Size = new System.Drawing.Size(39, 20);
            this.miEdit.Text = "Edit";
            // 
            // miEditGoTo
            // 
            this.miEditGoTo.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miEditGoToSymbol});
            this.miEditGoTo.Name = "miEditGoTo";
            this.miEditGoTo.Size = new System.Drawing.Size(166, 22);
            this.miEditGoTo.Text = "Go To";
            // 
            // miEditGoToSymbol
            // 
            this.miEditGoToSymbol.Name = "miEditGoToSymbol";
            this.miEditGoToSymbol.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.miEditGoToSymbol.Size = new System.Drawing.Size(165, 22);
            this.miEditGoToSymbol.Text = "Symbol...";
            this.miEditGoToSymbol.Click += new System.EventHandler(this.MenuActionEditGoToSymbol);
            // 
            // miEditFindAndReplace
            // 
            this.miEditFindAndReplace.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miEditFindAndReplaceQuickFind,
            this.miEditFindAndReplaceQuickReplace});
            this.miEditFindAndReplace.Name = "miEditFindAndReplace";
            this.miEditFindAndReplace.Size = new System.Drawing.Size(166, 22);
            this.miEditFindAndReplace.Text = "Find And Replace";
            // 
            // miEditFindAndReplaceQuickFind
            // 
            this.miEditFindAndReplaceQuickFind.Image = global::TSP.DoxygenEditor.Properties.Resources.QuickFind_16x;
            this.miEditFindAndReplaceQuickFind.Name = "miEditFindAndReplaceQuickFind";
            this.miEditFindAndReplaceQuickFind.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.miEditFindAndReplaceQuickFind.Size = new System.Drawing.Size(192, 22);
            this.miEditFindAndReplaceQuickFind.Text = "Quick Find";
            this.miEditFindAndReplaceQuickFind.Click += new System.EventHandler(this.MenuActionEditSearchAndReplaceQuickSearch);
            // 
            // miEditFindAndReplaceQuickReplace
            // 
            this.miEditFindAndReplaceQuickReplace.Image = global::TSP.DoxygenEditor.Properties.Resources.QuickReplace_16x;
            this.miEditFindAndReplaceQuickReplace.Name = "miEditFindAndReplaceQuickReplace";
            this.miEditFindAndReplaceQuickReplace.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.miEditFindAndReplaceQuickReplace.Size = new System.Drawing.Size(192, 22);
            this.miEditFindAndReplaceQuickReplace.Text = "Quick Replace";
            this.miEditFindAndReplaceQuickReplace.Click += new System.EventHandler(this.MenuActionEditSearchAndReplaceQuickReplace);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(163, 6);
            // 
            // miEditUndo
            // 
            this.miEditUndo.Image = global::TSP.DoxygenEditor.Properties.Resources.Undo_16x;
            this.miEditUndo.Name = "miEditUndo";
            this.miEditUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.miEditUndo.Size = new System.Drawing.Size(166, 22);
            this.miEditUndo.Text = "Undo";
            this.miEditUndo.Click += new System.EventHandler(this.MenuActionEditUndo);
            // 
            // miEditRedo
            // 
            this.miEditRedo.Image = global::TSP.DoxygenEditor.Properties.Resources.Redo_16x;
            this.miEditRedo.Name = "miEditRedo";
            this.miEditRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.miEditRedo.Size = new System.Drawing.Size(166, 22);
            this.miEditRedo.Text = "Redo";
            this.miEditRedo.Click += new System.EventHandler(this.MenuActionEditRedo);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(163, 6);
            // 
            // miEditCut
            // 
            this.miEditCut.Image = global::TSP.DoxygenEditor.Properties.Resources.Cut_16x;
            this.miEditCut.Name = "miEditCut";
            this.miEditCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.miEditCut.Size = new System.Drawing.Size(166, 22);
            this.miEditCut.Text = "Cut";
            this.miEditCut.Click += new System.EventHandler(this.MenuActionEditCut);
            // 
            // miEditCopy
            // 
            this.miEditCopy.Image = global::TSP.DoxygenEditor.Properties.Resources.Copy_16x;
            this.miEditCopy.Name = "miEditCopy";
            this.miEditCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.miEditCopy.Size = new System.Drawing.Size(166, 22);
            this.miEditCopy.Text = "Copy";
            this.miEditCopy.Click += new System.EventHandler(this.MenuActionEditCopy);
            // 
            // miEditPaste
            // 
            this.miEditPaste.Image = global::TSP.DoxygenEditor.Properties.Resources.Paste_16x;
            this.miEditPaste.Name = "miEditPaste";
            this.miEditPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.miEditPaste.Size = new System.Drawing.Size(166, 22);
            this.miEditPaste.Text = "Paste";
            this.miEditPaste.Click += new System.EventHandler(this.MenuActionEditPaste);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(163, 6);
            // 
            // miEditSelectAll
            // 
            this.miEditSelectAll.Image = global::TSP.DoxygenEditor.Properties.Resources.SelectAll_16x;
            this.miEditSelectAll.Name = "miEditSelectAll";
            this.miEditSelectAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.miEditSelectAll.Size = new System.Drawing.Size(166, 22);
            this.miEditSelectAll.Text = "Select All";
            this.miEditSelectAll.Click += new System.EventHandler(this.MenuActionEditSelectAll);
            // 
            // miView
            // 
            this.miView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miViewShowWhitespaces});
            this.miView.Name = "miView";
            this.miView.Size = new System.Drawing.Size(44, 20);
            this.miView.Text = "View";
            // 
            // miViewShowWhitespaces
            // 
            this.miViewShowWhitespaces.Checked = true;
            this.miViewShowWhitespaces.CheckState = System.Windows.Forms.CheckState.Checked;
            this.miViewShowWhitespaces.Name = "miViewShowWhitespaces";
            this.miViewShowWhitespaces.Size = new System.Drawing.Size(172, 22);
            this.miViewShowWhitespaces.Text = "Show Whitespaces";
            this.miViewShowWhitespaces.Click += new System.EventHandler(this.MenuActionViewShowWhitespaces);
            // 
            // miWorkspace
            // 
            this.miWorkspace.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miWorkspaceConfiguration});
            this.miWorkspace.Name = "miWorkspace";
            this.miWorkspace.Size = new System.Drawing.Size(77, 20);
            this.miWorkspace.Text = "Workspace";
            // 
            // miWorkspaceConfiguration
            // 
            this.miWorkspaceConfiguration.Name = "miWorkspaceConfiguration";
            this.miWorkspaceConfiguration.Size = new System.Drawing.Size(157, 22);
            this.miWorkspaceConfiguration.Text = "Configuration...";
            this.miWorkspaceConfiguration.Click += new System.EventHandler(this.miWorkspaceConfiguration_Click);
            // 
            // miHelp
            // 
            this.miHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miHelpAbout});
            this.miHelp.Name = "miHelp";
            this.miHelp.Size = new System.Drawing.Size(44, 20);
            this.miHelp.Text = "Help";
            // 
            // miHelpAbout
            // 
            this.miHelpAbout.Name = "miHelpAbout";
            this.miHelpAbout.Size = new System.Drawing.Size(116, 22);
            this.miHelpAbout.Text = "About...";
            this.miHelpAbout.Click += new System.EventHandler(this.MenuActionHelpAbout);
            // 
            // tsMain
            // 
            this.tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsMain.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbtnFileNew,
            this.tbtnFileOpen,
            this.tbtnFileSave,
            this.tbtnFileSaveAll,
            this.toolStripSeparator1,
            this.tbtnFileRefresh,
            this.toolStripSeparator2,
            this.tbtnEditUndo,
            this.tbtnEditRedo});
            this.tsMain.Location = new System.Drawing.Point(0, 24);
            this.tsMain.Name = "tsMain";
            this.tsMain.Size = new System.Drawing.Size(736, 31);
            this.tsMain.TabIndex = 1;
            this.tsMain.Text = "toolStrip1";
            // 
            // tbtnFileNew
            // 
            this.tbtnFileNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbtnFileNew.Image = global::TSP.DoxygenEditor.Properties.Resources.NewFile_16x;
            this.tbtnFileNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnFileNew.Name = "tbtnFileNew";
            this.tbtnFileNew.Size = new System.Drawing.Size(28, 28);
            this.tbtnFileNew.Text = "New file";
            this.tbtnFileNew.Click += new System.EventHandler(this.MenuActionFileNew);
            // 
            // tbtnFileOpen
            // 
            this.tbtnFileOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbtnFileOpen.Image = global::TSP.DoxygenEditor.Properties.Resources.OpenFile_16x;
            this.tbtnFileOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnFileOpen.Name = "tbtnFileOpen";
            this.tbtnFileOpen.Size = new System.Drawing.Size(28, 28);
            this.tbtnFileOpen.Text = "Open file...";
            this.tbtnFileOpen.Click += new System.EventHandler(this.MenuActionFileOpen);
            // 
            // tbtnFileSave
            // 
            this.tbtnFileSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbtnFileSave.Image = global::TSP.DoxygenEditor.Properties.Resources.Save_16x;
            this.tbtnFileSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnFileSave.Name = "tbtnFileSave";
            this.tbtnFileSave.Size = new System.Drawing.Size(28, 28);
            this.tbtnFileSave.Text = "Save file";
            this.tbtnFileSave.Click += new System.EventHandler(this.MenuActionFileSave);
            // 
            // tbtnFileSaveAll
            // 
            this.tbtnFileSaveAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbtnFileSaveAll.Image = global::TSP.DoxygenEditor.Properties.Resources.SaveAll_16x;
            this.tbtnFileSaveAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnFileSaveAll.Name = "tbtnFileSaveAll";
            this.tbtnFileSaveAll.Size = new System.Drawing.Size(28, 28);
            this.tbtnFileSaveAll.Text = "Save all";
            this.tbtnFileSaveAll.Click += new System.EventHandler(this.MenuActionFileSaveAll);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
            // 
            // tbtnFileRefresh
            // 
            this.tbtnFileRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbtnFileRefresh.Image = global::TSP.DoxygenEditor.Properties.Resources.Refresh_16x;
            this.tbtnFileRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnFileRefresh.Name = "tbtnFileRefresh";
            this.tbtnFileRefresh.Size = new System.Drawing.Size(28, 28);
            this.tbtnFileRefresh.Text = "Refresh file";
            this.tbtnFileRefresh.Click += new System.EventHandler(this.MenuActionFileRefresh);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 31);
            // 
            // tbtnEditUndo
            // 
            this.tbtnEditUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbtnEditUndo.Image = global::TSP.DoxygenEditor.Properties.Resources.Undo_16x;
            this.tbtnEditUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnEditUndo.Name = "tbtnEditUndo";
            this.tbtnEditUndo.Size = new System.Drawing.Size(28, 28);
            this.tbtnEditUndo.Text = "Undo";
            // 
            // tbtnEditRedo
            // 
            this.tbtnEditRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbtnEditRedo.Image = global::TSP.DoxygenEditor.Properties.Resources.Redo_16x;
            this.tbtnEditRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnEditRedo.Name = "tbtnEditRedo";
            this.tbtnEditRedo.Size = new System.Drawing.Size(28, 28);
            this.tbtnEditRedo.Text = "Redo";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslblParseStatusLabel,
            this.tsslblIncludeParseState,
            this.tspbIncludeStateProgress});
            this.statusStrip1.Location = new System.Drawing.Point(0, 427);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 9, 0);
            this.statusStrip1.Size = new System.Drawing.Size(736, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsslblParseStatusLabel
            // 
            this.tsslblParseStatusLabel.Name = "tsslblParseStatusLabel";
            this.tsslblParseStatusLabel.Size = new System.Drawing.Size(47, 17);
            this.tsslblParseStatusLabel.Text = "[Status]";
            // 
            // scMainAndLog
            // 
            this.scMainAndLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scMainAndLog.Location = new System.Drawing.Point(0, 55);
            this.scMainAndLog.Margin = new System.Windows.Forms.Padding(2);
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
            this.scMainAndLog.Size = new System.Drawing.Size(736, 372);
            this.scMainAndLog.SplitterDistance = 241;
            this.scMainAndLog.SplitterWidth = 2;
            this.scMainAndLog.TabIndex = 3;
            // 
            // scTreeAndFiles
            // 
            this.scTreeAndFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scTreeAndFiles.Location = new System.Drawing.Point(0, 0);
            this.scTreeAndFiles.Margin = new System.Windows.Forms.Padding(2);
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
            this.scTreeAndFiles.Size = new System.Drawing.Size(736, 241);
            this.scTreeAndFiles.SplitterDistance = 241;
            this.scTreeAndFiles.SplitterWidth = 3;
            this.scTreeAndFiles.TabIndex = 0;
            // 
            // tvTree
            // 
            this.tvTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvTree.HideSelection = false;
            this.tvTree.Location = new System.Drawing.Point(0, 20);
            this.tvTree.Margin = new System.Windows.Forms.Padding(2);
            this.tvTree.Name = "tvTree";
            this.tvTree.Size = new System.Drawing.Size(241, 221);
            this.tvTree.TabIndex = 1;
            this.tvTree.DoubleClick += new System.EventHandler(this.tvTree_DoubleClick);
            // 
            // panTreeTop
            // 
            this.panTreeTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panTreeTop.Location = new System.Drawing.Point(0, 0);
            this.panTreeTop.Margin = new System.Windows.Forms.Padding(2);
            this.panTreeTop.Name = "panTreeTop";
            this.panTreeTop.Size = new System.Drawing.Size(241, 20);
            this.panTreeTop.TabIndex = 2;
            // 
            // tcFiles
            // 
            this.tcFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcFiles.HotTrack = true;
            this.tcFiles.Location = new System.Drawing.Point(0, 0);
            this.tcFiles.Margin = new System.Windows.Forms.Padding(2);
            this.tcFiles.Name = "tcFiles";
            this.tcFiles.SelectedIndex = 0;
            this.tcFiles.Size = new System.Drawing.Size(492, 241);
            this.tcFiles.TabIndex = 0;
            this.tcFiles.SelectedIndexChanged += new System.EventHandler(this.tcFiles_SelectedIndexChanged);
            this.tcFiles.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tcFiles_MouseClick);
            // 
            // tcBottom
            // 
            this.tcBottom.Controls.Add(this.tpDoxygenIssues);
            this.tcBottom.Controls.Add(this.tpCppIssues);
            this.tcBottom.Controls.Add(this.tpPerformance);
            this.tcBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcBottom.HotTrack = true;
            this.tcBottom.Location = new System.Drawing.Point(0, 0);
            this.tcBottom.Margin = new System.Windows.Forms.Padding(0);
            this.tcBottom.Name = "tcBottom";
            this.tcBottom.SelectedIndex = 0;
            this.tcBottom.Size = new System.Drawing.Size(736, 129);
            this.tcBottom.TabIndex = 0;
            // 
            // tpDoxygenIssues
            // 
            this.tpDoxygenIssues.Location = new System.Drawing.Point(4, 22);
            this.tpDoxygenIssues.Margin = new System.Windows.Forms.Padding(2);
            this.tpDoxygenIssues.Name = "tpDoxygenIssues";
            this.tpDoxygenIssues.Padding = new System.Windows.Forms.Padding(2);
            this.tpDoxygenIssues.Size = new System.Drawing.Size(728, 103);
            this.tpDoxygenIssues.TabIndex = 2;
            this.tpDoxygenIssues.Text = "Doxygen Issues";
            this.tpDoxygenIssues.UseVisualStyleBackColor = true;
            // 
            // tpCppIssues
            // 
            this.tpCppIssues.Location = new System.Drawing.Point(4, 22);
            this.tpCppIssues.Margin = new System.Windows.Forms.Padding(2);
            this.tpCppIssues.Name = "tpCppIssues";
            this.tpCppIssues.Padding = new System.Windows.Forms.Padding(2);
            this.tpCppIssues.Size = new System.Drawing.Size(728, 103);
            this.tpCppIssues.TabIndex = 0;
            this.tpCppIssues.Text = "C/C++ Issues";
            this.tpCppIssues.UseVisualStyleBackColor = true;
            // 
            // tpPerformance
            // 
            this.tpPerformance.Controls.Add(this.lvPerformance);
            this.tpPerformance.Location = new System.Drawing.Point(4, 22);
            this.tpPerformance.Name = "tpPerformance";
            this.tpPerformance.Padding = new System.Windows.Forms.Padding(3);
            this.tpPerformance.Size = new System.Drawing.Size(728, 103);
            this.tpPerformance.TabIndex = 1;
            this.tpPerformance.Text = "Performance";
            this.tpPerformance.UseVisualStyleBackColor = true;
            // 
            // lvPerformance
            // 
            this.lvPerformance.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11});
            this.lvPerformance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvPerformance.FullRowSelect = true;
            this.lvPerformance.HideSelection = false;
            this.lvPerformance.Location = new System.Drawing.Point(3, 3);
            this.lvPerformance.Margin = new System.Windows.Forms.Padding(0);
            this.lvPerformance.MultiSelect = false;
            this.lvPerformance.Name = "lvPerformance";
            this.lvPerformance.Size = new System.Drawing.Size(722, 97);
            this.lvPerformance.TabIndex = 1;
            this.lvPerformance.UseCompatibleStateImageBehavior = false;
            this.lvPerformance.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "File";
            this.columnHeader7.Width = 200;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Input";
            this.columnHeader8.Width = 100;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Output";
            this.columnHeader9.Width = 100;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "What";
            this.columnHeader10.Width = 100;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "Duration in ms";
            this.columnHeader11.Width = 150;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "Message";
            this.columnHeader12.Width = 200;
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "Symbol";
            this.columnHeader13.Width = 200;
            // 
            // columnHeader14
            // 
            this.columnHeader14.Text = "Type";
            this.columnHeader14.Width = 100;
            // 
            // columnHeader15
            // 
            this.columnHeader15.Text = "Group";
            this.columnHeader15.Width = 100;
            // 
            // columnHeader16
            // 
            this.columnHeader16.Text = "Line";
            this.columnHeader16.Width = 100;
            // 
            // columnHeader17
            // 
            this.columnHeader17.Text = "File";
            this.columnHeader17.Width = 150;
            // 
            // imglstIcons
            // 
            this.imglstIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imglstIcons.ImageStream")));
            this.imglstIcons.TransparentColor = System.Drawing.Color.Transparent;
            this.imglstIcons.Images.SetKeyName(0, "StatusCriticalError_16x.png");
            this.imglstIcons.Images.SetKeyName(1, "StatusWarning_16x.png");
            this.imglstIcons.Images.SetKeyName(2, "StatusInformation_16x.png");
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
            this.columnHeader5.Text = "Line";
            this.columnHeader5.Width = 100;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "File";
            this.columnHeader6.Width = 150;
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
            this.cmsTabActions.Size = new System.Drawing.Size(162, 92);
            // 
            // miCurrentTabSave
            // 
            this.miCurrentTabSave.Name = "miCurrentTabSave";
            this.miCurrentTabSave.Size = new System.Drawing.Size(161, 22);
            this.miCurrentTabSave.Text = "Save";
            this.miCurrentTabSave.Click += new System.EventHandler(this.MenuActionFileSave);
            // 
            // miCurrentTabClose
            // 
            this.miCurrentTabClose.Name = "miCurrentTabClose";
            this.miCurrentTabClose.Size = new System.Drawing.Size(161, 22);
            this.miCurrentTabClose.Text = "Close";
            this.miCurrentTabClose.Click += new System.EventHandler(this.MenuActionFileClose);
            // 
            // miCurrentTabCloseAll
            // 
            this.miCurrentTabCloseAll.Name = "miCurrentTabCloseAll";
            this.miCurrentTabCloseAll.Size = new System.Drawing.Size(161, 22);
            this.miCurrentTabCloseAll.Text = "Close all";
            this.miCurrentTabCloseAll.Click += new System.EventHandler(this.MenuActionFileCloseAll);
            // 
            // miCurrentTabCloseAllButThis
            // 
            this.miCurrentTabCloseAllButThis.Name = "miCurrentTabCloseAllButThis";
            this.miCurrentTabCloseAllButThis.Size = new System.Drawing.Size(161, 22);
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
            // tsslblIncludeParseState
            // 
            this.tsslblIncludeParseState.Name = "tsslblIncludeParseState";
            this.tsslblIncludeParseState.Size = new System.Drawing.Size(80, 17);
            this.tsslblIncludeParseState.Text = "[IncludeState]";
            // 
            // tspbIncludeStateProgress
            // 
            this.tspbIncludeStateProgress.Name = "tspbIncludeStateProgress";
            this.tspbIncludeStateProgress.Size = new System.Drawing.Size(100, 16);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(736, 449);
            this.Controls.Add(this.scMainAndLog);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tsMain);
            this.Controls.Add(this.mainMenuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenuStrip;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Doxygen Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.tsMain.ResumeLayout(false);
            this.tsMain.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.scMainAndLog.Panel1.ResumeLayout(false);
            this.scMainAndLog.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scMainAndLog)).EndInit();
            this.scMainAndLog.ResumeLayout(false);
            this.scTreeAndFiles.Panel1.ResumeLayout(false);
            this.scTreeAndFiles.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scTreeAndFiles)).EndInit();
            this.scTreeAndFiles.ResumeLayout(false);
            this.tcBottom.ResumeLayout(false);
            this.tpPerformance.ResumeLayout(false);
            this.cmsTabActions.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem miFile;
        private System.Windows.Forms.ToolStripMenuItem miFileExit;
        private System.Windows.Forms.ToolStrip tsMain;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.SplitContainer scMainAndLog;
        private System.Windows.Forms.SplitContainer scTreeAndFiles;
        private System.Windows.Forms.TabControl tcBottom;
        private System.Windows.Forms.TabPage tpCppIssues;
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
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ImageList imglstIcons;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ToolStripMenuItem miHelp;
        private System.Windows.Forms.ToolStripMenuItem miHelpAbout;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem miFileRecentFiles;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.TabPage tpPerformance;
        private System.Windows.Forms.ListView lvPerformance;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ToolStripMenuItem miFileRefresh;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.TabPage tpDoxygenIssues;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.ColumnHeader columnHeader14;
        private System.Windows.Forms.ColumnHeader columnHeader15;
        private System.Windows.Forms.ColumnHeader columnHeader16;
        private System.Windows.Forms.ColumnHeader columnHeader17;
        private System.Windows.Forms.ToolStripButton tbtnFileOpen;
        private System.Windows.Forms.ToolStripButton tbtnFileSave;
        private System.Windows.Forms.ToolStripButton tbtnFileSaveAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tbtnFileRefresh;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem7;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tbtnEditUndo;
        private System.Windows.Forms.ToolStripButton tbtnEditRedo;
        private System.Windows.Forms.ToolStripStatusLabel tsslblParseStatusLabel;
        private System.Windows.Forms.ToolStripMenuItem miWorkspace;
        private System.Windows.Forms.ToolStripMenuItem miWorkspaceConfiguration;
        private System.Windows.Forms.ToolStripStatusLabel tsslblIncludeParseState;
        private System.Windows.Forms.ToolStripProgressBar tspbIncludeStateProgress;
    }
}