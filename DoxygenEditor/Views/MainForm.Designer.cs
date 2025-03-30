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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            mainMenuStrip = new System.Windows.Forms.MenuStrip();
            miFile = new System.Windows.Forms.ToolStripMenuItem();
            miFileNew = new System.Windows.Forms.ToolStripMenuItem();
            miFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            miFileSave = new System.Windows.Forms.ToolStripMenuItem();
            miFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            miFileSaveAll = new System.Windows.Forms.ToolStripMenuItem();
            miFileClose = new System.Windows.Forms.ToolStripMenuItem();
            miFileCloseAll = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
            miFileRefresh = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            miFileRecentFiles = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            miFileExit = new System.Windows.Forms.ToolStripMenuItem();
            miEdit = new System.Windows.Forms.ToolStripMenuItem();
            miEditGoTo = new System.Windows.Forms.ToolStripMenuItem();
            miEditGoToSymbol = new System.Windows.Forms.ToolStripMenuItem();
            miEditFindAndReplace = new System.Windows.Forms.ToolStripMenuItem();
            miEditFindAndReplaceQuickFind = new System.Windows.Forms.ToolStripMenuItem();
            miEditFindAndReplaceQuickReplace = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            miEditUndo = new System.Windows.Forms.ToolStripMenuItem();
            miEditRedo = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            miEditCut = new System.Windows.Forms.ToolStripMenuItem();
            miEditCopy = new System.Windows.Forms.ToolStripMenuItem();
            miEditPaste = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            miEditSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            miView = new System.Windows.Forms.ToolStripMenuItem();
            miViewShowWhitespaces = new System.Windows.Forms.ToolStripMenuItem();
            miWorkspace = new System.Windows.Forms.ToolStripMenuItem();
            miWorkspaceNew = new System.Windows.Forms.ToolStripMenuItem();
            mitWorkspaceLoad = new System.Windows.Forms.ToolStripMenuItem();
            miWorkspaceConfiguration = new System.Windows.Forms.ToolStripMenuItem();
            miBuild = new System.Windows.Forms.ToolStripMenuItem();
            miBuildDocumentation = new System.Windows.Forms.ToolStripMenuItem();
            toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            miToolsParseAPIPrototypes = new System.Windows.Forms.ToolStripMenuItem();
            miHelp = new System.Windows.Forms.ToolStripMenuItem();
            miHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            tsMain = new System.Windows.Forms.ToolStrip();
            tbtnFileNew = new System.Windows.Forms.ToolStripButton();
            tbtnFileOpen = new System.Windows.Forms.ToolStripButton();
            tbtnFileSave = new System.Windows.Forms.ToolStripButton();
            tbtnFileSaveAll = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            tbtnFileRefresh = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            tbtnEditUndo = new System.Windows.Forms.ToolStripButton();
            tbtnEditRedo = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            tbtnBuildDocumentation = new System.Windows.Forms.ToolStripButton();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            tsslblParseStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            scMainAndLog = new System.Windows.Forms.SplitContainer();
            scTreeAndFiles = new System.Windows.Forms.SplitContainer();
            tvTree = new System.Windows.Forms.TreeView();
            panTreeTop = new System.Windows.Forms.Panel();
            tcFiles = new System.Windows.Forms.TabControl();
            tcBottom = new System.Windows.Forms.TabControl();
            tpDoxygenIssues = new System.Windows.Forms.TabPage();
            tpCppIssues = new System.Windows.Forms.TabPage();
            tpPerformance = new System.Windows.Forms.TabPage();
            lvPerformance = new System.Windows.Forms.ListView();
            columnHeader7 = new System.Windows.Forms.ColumnHeader();
            columnHeader8 = new System.Windows.Forms.ColumnHeader();
            columnHeader9 = new System.Windows.Forms.ColumnHeader();
            columnHeader10 = new System.Windows.Forms.ColumnHeader();
            columnHeader11 = new System.Windows.Forms.ColumnHeader();
            columnHeader12 = new System.Windows.Forms.ColumnHeader();
            columnHeader13 = new System.Windows.Forms.ColumnHeader();
            columnHeader14 = new System.Windows.Forms.ColumnHeader();
            columnHeader15 = new System.Windows.Forms.ColumnHeader();
            columnHeader16 = new System.Windows.Forms.ColumnHeader();
            columnHeader17 = new System.Windows.Forms.ColumnHeader();
            imglstIcons = new System.Windows.Forms.ImageList(components);
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            columnHeader2 = new System.Windows.Forms.ColumnHeader();
            columnHeader3 = new System.Windows.Forms.ColumnHeader();
            columnHeader4 = new System.Windows.Forms.ColumnHeader();
            columnHeader5 = new System.Windows.Forms.ColumnHeader();
            columnHeader6 = new System.Windows.Forms.ColumnHeader();
            dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
            cmsTabActions = new System.Windows.Forms.ContextMenuStrip(components);
            miCurrentTabSave = new System.Windows.Forms.ToolStripMenuItem();
            miCurrentTabClose = new System.Windows.Forms.ToolStripMenuItem();
            miCurrentTabCloseAll = new System.Windows.Forms.ToolStripMenuItem();
            miCurrentTabCloseAllButThis = new System.Windows.Forms.ToolStripMenuItem();
            dlgSaveFile = new System.Windows.Forms.SaveFileDialog();
            dlgOpenWorkspace = new System.Windows.Forms.OpenFileDialog();
            dlgSaveWorkspace = new System.Windows.Forms.SaveFileDialog();
            mainMenuStrip.SuspendLayout();
            tsMain.SuspendLayout();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)scMainAndLog).BeginInit();
            scMainAndLog.Panel1.SuspendLayout();
            scMainAndLog.Panel2.SuspendLayout();
            scMainAndLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)scTreeAndFiles).BeginInit();
            scTreeAndFiles.Panel1.SuspendLayout();
            scTreeAndFiles.Panel2.SuspendLayout();
            scTreeAndFiles.SuspendLayout();
            tcBottom.SuspendLayout();
            tpPerformance.SuspendLayout();
            cmsTabActions.SuspendLayout();
            SuspendLayout();
            // 
            // mainMenuStrip
            // 
            mainMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { miFile, miEdit, miView, miWorkspace, miBuild, toolsToolStripMenuItem, miHelp });
            mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            mainMenuStrip.Name = "mainMenuStrip";
            mainMenuStrip.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            mainMenuStrip.Size = new System.Drawing.Size(858, 24);
            mainMenuStrip.TabIndex = 0;
            mainMenuStrip.Text = "menuStrip1";
            // 
            // miFile
            // 
            miFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { miFileNew, miFileOpen, miFileSave, miFileSaveAs, miFileSaveAll, miFileClose, miFileCloseAll, toolStripMenuItem7, miFileRefresh, toolStripMenuItem1, miFileRecentFiles, toolStripMenuItem5, miFileExit });
            miFile.Name = "miFile";
            miFile.Size = new System.Drawing.Size(37, 20);
            miFile.Text = "File";
            // 
            // miFileNew
            // 
            miFileNew.Image = Properties.Resources.NewFile_16x;
            miFileNew.Name = "miFileNew";
            miFileNew.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N;
            miFileNew.Size = new System.Drawing.Size(193, 30);
            miFileNew.Text = "New";
            miFileNew.Click += MenuActionFileNew;
            // 
            // miFileOpen
            // 
            miFileOpen.Image = Properties.Resources.OpenFile_16x;
            miFileOpen.Name = "miFileOpen";
            miFileOpen.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            miFileOpen.Size = new System.Drawing.Size(193, 30);
            miFileOpen.Text = "Open...";
            miFileOpen.Click += MenuActionFileOpen;
            // 
            // miFileSave
            // 
            miFileSave.Image = Properties.Resources.Save_16x;
            miFileSave.Name = "miFileSave";
            miFileSave.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            miFileSave.Size = new System.Drawing.Size(193, 30);
            miFileSave.Text = "Save";
            miFileSave.Click += MenuActionFileSave;
            // 
            // miFileSaveAs
            // 
            miFileSaveAs.Image = Properties.Resources.SaveAs_16x;
            miFileSaveAs.Name = "miFileSaveAs";
            miFileSaveAs.Size = new System.Drawing.Size(193, 30);
            miFileSaveAs.Text = "Save as...";
            miFileSaveAs.Click += MenuActionFileSaveAs;
            // 
            // miFileSaveAll
            // 
            miFileSaveAll.Image = Properties.Resources.SaveAll_16x;
            miFileSaveAll.Name = "miFileSaveAll";
            miFileSaveAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.S;
            miFileSaveAll.Size = new System.Drawing.Size(193, 30);
            miFileSaveAll.Text = "Save all";
            miFileSaveAll.Click += MenuActionFileSaveAll;
            // 
            // miFileClose
            // 
            miFileClose.Image = Properties.Resources.CloseDocument_16x;
            miFileClose.Name = "miFileClose";
            miFileClose.Size = new System.Drawing.Size(193, 30);
            miFileClose.Text = "Close";
            miFileClose.Click += MenuActionFileClose;
            // 
            // miFileCloseAll
            // 
            miFileCloseAll.Image = Properties.Resources.CloseGroup_16x;
            miFileCloseAll.Name = "miFileCloseAll";
            miFileCloseAll.Size = new System.Drawing.Size(193, 30);
            miFileCloseAll.Text = "Close all";
            miFileCloseAll.Click += MenuActionFileCloseAll;
            // 
            // toolStripMenuItem7
            // 
            toolStripMenuItem7.Name = "toolStripMenuItem7";
            toolStripMenuItem7.Size = new System.Drawing.Size(190, 6);
            // 
            // miFileRefresh
            // 
            miFileRefresh.Image = Properties.Resources.Refresh_16x;
            miFileRefresh.Name = "miFileRefresh";
            miFileRefresh.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R;
            miFileRefresh.Size = new System.Drawing.Size(193, 30);
            miFileRefresh.Text = "Refresh";
            miFileRefresh.Click += MenuActionFileRefresh;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(190, 6);
            // 
            // miFileRecentFiles
            // 
            miFileRecentFiles.Name = "miFileRecentFiles";
            miFileRecentFiles.Size = new System.Drawing.Size(193, 30);
            miFileRecentFiles.Text = "Recent Files";
            // 
            // toolStripMenuItem5
            // 
            toolStripMenuItem5.Name = "toolStripMenuItem5";
            toolStripMenuItem5.Size = new System.Drawing.Size(190, 6);
            // 
            // miFileExit
            // 
            miFileExit.Image = Properties.Resources.Exit_16x;
            miFileExit.Name = "miFileExit";
            miFileExit.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4;
            miFileExit.Size = new System.Drawing.Size(193, 30);
            miFileExit.Text = "Exit";
            miFileExit.Click += MenuActionFileExit;
            // 
            // miEdit
            // 
            miEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { miEditGoTo, miEditFindAndReplace, toolStripMenuItem2, miEditUndo, miEditRedo, toolStripMenuItem3, miEditCut, miEditCopy, miEditPaste, toolStripMenuItem4, miEditSelectAll });
            miEdit.Name = "miEdit";
            miEdit.Size = new System.Drawing.Size(39, 20);
            miEdit.Text = "Edit";
            // 
            // miEditGoTo
            // 
            miEditGoTo.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { miEditGoToSymbol });
            miEditGoTo.Name = "miEditGoTo";
            miEditGoTo.Size = new System.Drawing.Size(174, 30);
            miEditGoTo.Text = "Go To";
            // 
            // miEditGoToSymbol
            // 
            miEditGoToSymbol.Name = "miEditGoToSymbol";
            miEditGoToSymbol.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G;
            miEditGoToSymbol.Size = new System.Drawing.Size(165, 22);
            miEditGoToSymbol.Text = "Symbol...";
            miEditGoToSymbol.Click += MenuActionEditGoToSymbol;
            // 
            // miEditFindAndReplace
            // 
            miEditFindAndReplace.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { miEditFindAndReplaceQuickFind, miEditFindAndReplaceQuickReplace });
            miEditFindAndReplace.Name = "miEditFindAndReplace";
            miEditFindAndReplace.Size = new System.Drawing.Size(174, 30);
            miEditFindAndReplace.Text = "Find And Replace";
            // 
            // miEditFindAndReplaceQuickFind
            // 
            miEditFindAndReplaceQuickFind.Image = Properties.Resources.QuickFind_16x;
            miEditFindAndReplaceQuickFind.Name = "miEditFindAndReplaceQuickFind";
            miEditFindAndReplaceQuickFind.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F;
            miEditFindAndReplaceQuickFind.Size = new System.Drawing.Size(200, 30);
            miEditFindAndReplaceQuickFind.Text = "Quick Find";
            miEditFindAndReplaceQuickFind.Click += MenuActionEditSearchAndReplaceQuickSearch;
            // 
            // miEditFindAndReplaceQuickReplace
            // 
            miEditFindAndReplaceQuickReplace.Image = Properties.Resources.QuickReplace_16x;
            miEditFindAndReplaceQuickReplace.Name = "miEditFindAndReplaceQuickReplace";
            miEditFindAndReplaceQuickReplace.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H;
            miEditFindAndReplaceQuickReplace.Size = new System.Drawing.Size(200, 30);
            miEditFindAndReplaceQuickReplace.Text = "Quick Replace";
            miEditFindAndReplaceQuickReplace.Click += MenuActionEditSearchAndReplaceQuickReplace;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new System.Drawing.Size(171, 6);
            // 
            // miEditUndo
            // 
            miEditUndo.Image = Properties.Resources.Undo_16x;
            miEditUndo.Name = "miEditUndo";
            miEditUndo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z;
            miEditUndo.Size = new System.Drawing.Size(174, 30);
            miEditUndo.Text = "Undo";
            miEditUndo.Click += MenuActionEditUndo;
            // 
            // miEditRedo
            // 
            miEditRedo.Image = Properties.Resources.Redo_16x;
            miEditRedo.Name = "miEditRedo";
            miEditRedo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y;
            miEditRedo.Size = new System.Drawing.Size(174, 30);
            miEditRedo.Text = "Redo";
            miEditRedo.Click += MenuActionEditRedo;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new System.Drawing.Size(171, 6);
            // 
            // miEditCut
            // 
            miEditCut.Image = Properties.Resources.Cut_16x;
            miEditCut.Name = "miEditCut";
            miEditCut.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
            miEditCut.Size = new System.Drawing.Size(174, 30);
            miEditCut.Text = "Cut";
            miEditCut.Click += MenuActionEditCut;
            // 
            // miEditCopy
            // 
            miEditCopy.Image = Properties.Resources.Copy_16x;
            miEditCopy.Name = "miEditCopy";
            miEditCopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            miEditCopy.Size = new System.Drawing.Size(174, 30);
            miEditCopy.Text = "Copy";
            miEditCopy.Click += MenuActionEditCopy;
            // 
            // miEditPaste
            // 
            miEditPaste.Image = Properties.Resources.Paste_16x;
            miEditPaste.Name = "miEditPaste";
            miEditPaste.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            miEditPaste.Size = new System.Drawing.Size(174, 30);
            miEditPaste.Text = "Paste";
            miEditPaste.Click += MenuActionEditPaste;
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new System.Drawing.Size(171, 6);
            // 
            // miEditSelectAll
            // 
            miEditSelectAll.Image = Properties.Resources.SelectAll_16x;
            miEditSelectAll.Name = "miEditSelectAll";
            miEditSelectAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            miEditSelectAll.Size = new System.Drawing.Size(174, 30);
            miEditSelectAll.Text = "Select All";
            miEditSelectAll.Click += MenuActionEditSelectAll;
            // 
            // miView
            // 
            miView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { miViewShowWhitespaces });
            miView.Name = "miView";
            miView.Size = new System.Drawing.Size(44, 20);
            miView.Text = "View";
            // 
            // miViewShowWhitespaces
            // 
            miViewShowWhitespaces.Checked = true;
            miViewShowWhitespaces.CheckState = System.Windows.Forms.CheckState.Checked;
            miViewShowWhitespaces.Name = "miViewShowWhitespaces";
            miViewShowWhitespaces.Size = new System.Drawing.Size(172, 22);
            miViewShowWhitespaces.Text = "Show Whitespaces";
            miViewShowWhitespaces.Click += MenuActionViewShowWhitespaces;
            // 
            // miWorkspace
            // 
            miWorkspace.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { miWorkspaceNew, mitWorkspaceLoad, miWorkspaceConfiguration });
            miWorkspace.Name = "miWorkspace";
            miWorkspace.Size = new System.Drawing.Size(77, 20);
            miWorkspace.Text = "Workspace";
            // 
            // miWorkspaceNew
            // 
            miWorkspaceNew.Name = "miWorkspaceNew";
            miWorkspaceNew.Size = new System.Drawing.Size(157, 22);
            miWorkspaceNew.Text = "New...";
            miWorkspaceNew.Click += miWorkspaceNew_Click;
            // 
            // mitWorkspaceLoad
            // 
            mitWorkspaceLoad.Name = "mitWorkspaceLoad";
            mitWorkspaceLoad.Size = new System.Drawing.Size(157, 22);
            mitWorkspaceLoad.Text = "Load...";
            mitWorkspaceLoad.Click += mitWorkspaceLoad_Click;
            // 
            // miWorkspaceConfiguration
            // 
            miWorkspaceConfiguration.Name = "miWorkspaceConfiguration";
            miWorkspaceConfiguration.Size = new System.Drawing.Size(157, 22);
            miWorkspaceConfiguration.Text = "Configuration...";
            miWorkspaceConfiguration.Click += miWorkspaceConfiguration_Click;
            // 
            // miBuild
            // 
            miBuild.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { miBuildDocumentation });
            miBuild.Name = "miBuild";
            miBuild.Size = new System.Drawing.Size(46, 20);
            miBuild.Text = "Build";
            // 
            // miBuildDocumentation
            // 
            miBuildDocumentation.Image = Properties.Resources.BuildDefinition_16x;
            miBuildDocumentation.Name = "miBuildDocumentation";
            miBuildDocumentation.Size = new System.Drawing.Size(204, 30);
            miBuildDocumentation.Text = "Build Documentation...";
            miBuildDocumentation.Click += BuildDocumentationClick;
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { miToolsParseAPIPrototypes });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            toolsToolStripMenuItem.Text = "Tools";
            // 
            // miToolsParseAPIPrototypes
            // 
            miToolsParseAPIPrototypes.Name = "miToolsParseAPIPrototypes";
            miToolsParseAPIPrototypes.Size = new System.Drawing.Size(183, 22);
            miToolsParseAPIPrototypes.Text = "Parse API Prototypes";
            miToolsParseAPIPrototypes.Click += MenuActionToolsParseAPIPrototypes;
            // 
            // miHelp
            // 
            miHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { miHelpAbout });
            miHelp.Name = "miHelp";
            miHelp.Size = new System.Drawing.Size(44, 20);
            miHelp.Text = "Help";
            // 
            // miHelpAbout
            // 
            miHelpAbout.Name = "miHelpAbout";
            miHelpAbout.Size = new System.Drawing.Size(180, 22);
            miHelpAbout.Text = "About...";
            miHelpAbout.Click += MenuActionHelpAbout;
            // 
            // tsMain
            // 
            tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            tsMain.ImageScalingSize = new System.Drawing.Size(24, 24);
            tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tbtnFileNew, tbtnFileOpen, tbtnFileSave, tbtnFileSaveAll, toolStripSeparator1, tbtnFileRefresh, toolStripSeparator2, tbtnEditUndo, tbtnEditRedo, toolStripSeparator3, tbtnBuildDocumentation });
            tsMain.Location = new System.Drawing.Point(0, 24);
            tsMain.Name = "tsMain";
            tsMain.Size = new System.Drawing.Size(858, 31);
            tsMain.TabIndex = 1;
            tsMain.Text = "toolStrip1";
            // 
            // tbtnFileNew
            // 
            tbtnFileNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbtnFileNew.Image = Properties.Resources.NewFile_16x;
            tbtnFileNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbtnFileNew.Name = "tbtnFileNew";
            tbtnFileNew.Size = new System.Drawing.Size(28, 28);
            tbtnFileNew.Text = "New file";
            tbtnFileNew.Click += MenuActionFileNew;
            // 
            // tbtnFileOpen
            // 
            tbtnFileOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbtnFileOpen.Image = Properties.Resources.OpenFile_16x;
            tbtnFileOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbtnFileOpen.Name = "tbtnFileOpen";
            tbtnFileOpen.Size = new System.Drawing.Size(28, 28);
            tbtnFileOpen.Text = "Open file...";
            tbtnFileOpen.Click += MenuActionFileOpen;
            // 
            // tbtnFileSave
            // 
            tbtnFileSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbtnFileSave.Image = Properties.Resources.Save_16x;
            tbtnFileSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbtnFileSave.Name = "tbtnFileSave";
            tbtnFileSave.Size = new System.Drawing.Size(28, 28);
            tbtnFileSave.Text = "Save file";
            tbtnFileSave.Click += MenuActionFileSave;
            // 
            // tbtnFileSaveAll
            // 
            tbtnFileSaveAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbtnFileSaveAll.Image = Properties.Resources.SaveAll_16x;
            tbtnFileSaveAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbtnFileSaveAll.Name = "tbtnFileSaveAll";
            tbtnFileSaveAll.Size = new System.Drawing.Size(28, 28);
            tbtnFileSaveAll.Text = "Save all";
            tbtnFileSaveAll.Click += MenuActionFileSaveAll;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
            // 
            // tbtnFileRefresh
            // 
            tbtnFileRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbtnFileRefresh.Image = Properties.Resources.Refresh_16x;
            tbtnFileRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbtnFileRefresh.Name = "tbtnFileRefresh";
            tbtnFileRefresh.Size = new System.Drawing.Size(28, 28);
            tbtnFileRefresh.Text = "Refresh file";
            tbtnFileRefresh.Click += MenuActionFileRefresh;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 31);
            // 
            // tbtnEditUndo
            // 
            tbtnEditUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbtnEditUndo.Image = Properties.Resources.Undo_16x;
            tbtnEditUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbtnEditUndo.Name = "tbtnEditUndo";
            tbtnEditUndo.Size = new System.Drawing.Size(28, 28);
            tbtnEditUndo.Text = "Undo";
            // 
            // tbtnEditRedo
            // 
            tbtnEditRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbtnEditRedo.Image = Properties.Resources.Redo_16x;
            tbtnEditRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbtnEditRedo.Name = "tbtnEditRedo";
            tbtnEditRedo.Size = new System.Drawing.Size(28, 28);
            tbtnEditRedo.Text = "Redo";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 31);
            // 
            // tbtnBuildDocumentation
            // 
            tbtnBuildDocumentation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbtnBuildDocumentation.Image = Properties.Resources.BuildDefinition_16x;
            tbtnBuildDocumentation.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbtnBuildDocumentation.Name = "tbtnBuildDocumentation";
            tbtnBuildDocumentation.Size = new System.Drawing.Size(28, 28);
            tbtnBuildDocumentation.Text = "Build Documentation...";
            tbtnBuildDocumentation.Click += BuildDocumentationClick;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsslblParseStatusLabel });
            statusStrip1.Location = new System.Drawing.Point(0, 565);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            statusStrip1.Size = new System.Drawing.Size(858, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // tsslblParseStatusLabel
            // 
            tsslblParseStatusLabel.Name = "tsslblParseStatusLabel";
            tsslblParseStatusLabel.Size = new System.Drawing.Size(47, 17);
            tsslblParseStatusLabel.Text = "[Status]";
            // 
            // scMainAndLog
            // 
            scMainAndLog.Dock = System.Windows.Forms.DockStyle.Fill;
            scMainAndLog.Location = new System.Drawing.Point(0, 55);
            scMainAndLog.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            scMainAndLog.Name = "scMainAndLog";
            scMainAndLog.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scMainAndLog.Panel1
            // 
            scMainAndLog.Panel1.Controls.Add(scTreeAndFiles);
            // 
            // scMainAndLog.Panel2
            // 
            scMainAndLog.Panel2.Controls.Add(tcBottom);
            scMainAndLog.Size = new System.Drawing.Size(858, 510);
            scMainAndLog.SplitterDistance = 329;
            scMainAndLog.SplitterWidth = 2;
            scMainAndLog.TabIndex = 3;
            // 
            // scTreeAndFiles
            // 
            scTreeAndFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            scTreeAndFiles.Location = new System.Drawing.Point(0, 0);
            scTreeAndFiles.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            scTreeAndFiles.Name = "scTreeAndFiles";
            // 
            // scTreeAndFiles.Panel1
            // 
            scTreeAndFiles.Panel1.Controls.Add(tvTree);
            scTreeAndFiles.Panel1.Controls.Add(panTreeTop);
            // 
            // scTreeAndFiles.Panel2
            // 
            scTreeAndFiles.Panel2.Controls.Add(tcFiles);
            scTreeAndFiles.Size = new System.Drawing.Size(858, 329);
            scTreeAndFiles.SplitterDistance = 280;
            scTreeAndFiles.TabIndex = 0;
            // 
            // tvTree
            // 
            tvTree.Dock = System.Windows.Forms.DockStyle.Fill;
            tvTree.HideSelection = false;
            tvTree.Location = new System.Drawing.Point(0, 26);
            tvTree.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            tvTree.Name = "tvTree";
            tvTree.Size = new System.Drawing.Size(280, 303);
            tvTree.TabIndex = 1;
            tvTree.DoubleClick += tvTree_DoubleClick;
            // 
            // panTreeTop
            // 
            panTreeTop.Dock = System.Windows.Forms.DockStyle.Top;
            panTreeTop.Location = new System.Drawing.Point(0, 0);
            panTreeTop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            panTreeTop.Name = "panTreeTop";
            panTreeTop.Size = new System.Drawing.Size(280, 26);
            panTreeTop.TabIndex = 2;
            // 
            // tcFiles
            // 
            tcFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            tcFiles.HotTrack = true;
            tcFiles.Location = new System.Drawing.Point(0, 0);
            tcFiles.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            tcFiles.Name = "tcFiles";
            tcFiles.SelectedIndex = 0;
            tcFiles.Size = new System.Drawing.Size(574, 329);
            tcFiles.TabIndex = 0;
            tcFiles.SelectedIndexChanged += tcFiles_SelectedIndexChanged;
            tcFiles.MouseClick += tcFiles_MouseClick;
            // 
            // tcBottom
            // 
            tcBottom.Controls.Add(tpDoxygenIssues);
            tcBottom.Controls.Add(tpCppIssues);
            tcBottom.Controls.Add(tpPerformance);
            tcBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            tcBottom.HotTrack = true;
            tcBottom.Location = new System.Drawing.Point(0, 0);
            tcBottom.Margin = new System.Windows.Forms.Padding(0);
            tcBottom.Name = "tcBottom";
            tcBottom.SelectedIndex = 0;
            tcBottom.Size = new System.Drawing.Size(858, 179);
            tcBottom.TabIndex = 0;
            // 
            // tpDoxygenIssues
            // 
            tpDoxygenIssues.Location = new System.Drawing.Point(4, 26);
            tpDoxygenIssues.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            tpDoxygenIssues.Name = "tpDoxygenIssues";
            tpDoxygenIssues.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            tpDoxygenIssues.Size = new System.Drawing.Size(850, 149);
            tpDoxygenIssues.TabIndex = 2;
            tpDoxygenIssues.Text = "Doxygen Issues";
            tpDoxygenIssues.UseVisualStyleBackColor = true;
            // 
            // tpCppIssues
            // 
            tpCppIssues.Location = new System.Drawing.Point(4, 24);
            tpCppIssues.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            tpCppIssues.Name = "tpCppIssues";
            tpCppIssues.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            tpCppIssues.Size = new System.Drawing.Size(850, 151);
            tpCppIssues.TabIndex = 0;
            tpCppIssues.Text = "C/C++ Issues";
            tpCppIssues.UseVisualStyleBackColor = true;
            // 
            // tpPerformance
            // 
            tpPerformance.Controls.Add(lvPerformance);
            tpPerformance.Location = new System.Drawing.Point(4, 24);
            tpPerformance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            tpPerformance.Name = "tpPerformance";
            tpPerformance.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            tpPerformance.Size = new System.Drawing.Size(850, 151);
            tpPerformance.TabIndex = 1;
            tpPerformance.Text = "Performance";
            tpPerformance.UseVisualStyleBackColor = true;
            // 
            // lvPerformance
            // 
            lvPerformance.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader7, columnHeader8, columnHeader9, columnHeader10, columnHeader11 });
            lvPerformance.Dock = System.Windows.Forms.DockStyle.Fill;
            lvPerformance.FullRowSelect = true;
            lvPerformance.Location = new System.Drawing.Point(4, 5);
            lvPerformance.Margin = new System.Windows.Forms.Padding(0);
            lvPerformance.MultiSelect = false;
            lvPerformance.Name = "lvPerformance";
            lvPerformance.Size = new System.Drawing.Size(842, 141);
            lvPerformance.TabIndex = 1;
            lvPerformance.UseCompatibleStateImageBehavior = false;
            lvPerformance.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader7
            // 
            columnHeader7.Text = "Id";
            columnHeader7.Width = 150;
            // 
            // columnHeader8
            // 
            columnHeader8.Text = "Input";
            columnHeader8.Width = 100;
            // 
            // columnHeader9
            // 
            columnHeader9.Text = "Output";
            columnHeader9.Width = 100;
            // 
            // columnHeader10
            // 
            columnHeader10.Text = "What";
            columnHeader10.Width = 100;
            // 
            // columnHeader11
            // 
            columnHeader11.Text = "Duration in ms";
            columnHeader11.Width = 150;
            // 
            // columnHeader12
            // 
            columnHeader12.Text = "Message";
            columnHeader12.Width = 200;
            // 
            // columnHeader13
            // 
            columnHeader13.Text = "Symbol";
            columnHeader13.Width = 200;
            // 
            // columnHeader14
            // 
            columnHeader14.Text = "Type";
            columnHeader14.Width = 100;
            // 
            // columnHeader15
            // 
            columnHeader15.Text = "Group";
            columnHeader15.Width = 100;
            // 
            // columnHeader16
            // 
            columnHeader16.Text = "Line";
            columnHeader16.Width = 100;
            // 
            // columnHeader17
            // 
            columnHeader17.Text = "File";
            columnHeader17.Width = 150;
            // 
            // imglstIcons
            // 
            imglstIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imglstIcons.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imglstIcons.ImageStream");
            imglstIcons.TransparentColor = System.Drawing.Color.Transparent;
            imglstIcons.Images.SetKeyName(0, "StatusCriticalError_16x.png");
            imglstIcons.Images.SetKeyName(1, "StatusWarning_16x.png");
            imglstIcons.Images.SetKeyName(2, "StatusInformation_16x.png");
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Message";
            columnHeader1.Width = 200;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Symbol";
            columnHeader2.Width = 200;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Type";
            columnHeader3.Width = 100;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Group";
            columnHeader4.Width = 100;
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "Line";
            columnHeader5.Width = 100;
            // 
            // columnHeader6
            // 
            columnHeader6.Text = "File";
            columnHeader6.Width = 150;
            // 
            // dlgOpenFile
            // 
            dlgOpenFile.AddExtension = false;
            dlgOpenFile.FilterIndex = 0;
            dlgOpenFile.Multiselect = true;
            dlgOpenFile.Title = "Open file";
            // 
            // cmsTabActions
            // 
            cmsTabActions.ImageScalingSize = new System.Drawing.Size(24, 24);
            cmsTabActions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { miCurrentTabSave, miCurrentTabClose, miCurrentTabCloseAll, miCurrentTabCloseAllButThis });
            cmsTabActions.Name = "cmsTabActions";
            cmsTabActions.Size = new System.Drawing.Size(162, 92);
            // 
            // miCurrentTabSave
            // 
            miCurrentTabSave.Name = "miCurrentTabSave";
            miCurrentTabSave.Size = new System.Drawing.Size(161, 22);
            miCurrentTabSave.Text = "Save";
            miCurrentTabSave.Click += MenuActionFileSave;
            // 
            // miCurrentTabClose
            // 
            miCurrentTabClose.Name = "miCurrentTabClose";
            miCurrentTabClose.Size = new System.Drawing.Size(161, 22);
            miCurrentTabClose.Text = "Close";
            miCurrentTabClose.Click += MenuActionFileClose;
            // 
            // miCurrentTabCloseAll
            // 
            miCurrentTabCloseAll.Name = "miCurrentTabCloseAll";
            miCurrentTabCloseAll.Size = new System.Drawing.Size(161, 22);
            miCurrentTabCloseAll.Text = "Close all";
            miCurrentTabCloseAll.Click += MenuActionFileCloseAll;
            // 
            // miCurrentTabCloseAllButThis
            // 
            miCurrentTabCloseAllButThis.Name = "miCurrentTabCloseAllButThis";
            miCurrentTabCloseAllButThis.Size = new System.Drawing.Size(161, 22);
            miCurrentTabCloseAllButThis.Text = "Close all but this";
            miCurrentTabCloseAllButThis.Click += MenuActionFileCloseAllButThis;
            // 
            // dlgSaveFile
            // 
            dlgSaveFile.DefaultExt = "docs";
            dlgSaveFile.FilterIndex = 0;
            dlgSaveFile.Title = "Save file";
            // 
            // dlgOpenWorkspace
            // 
            dlgOpenWorkspace.DefaultExt = "doxyedit";
            dlgOpenWorkspace.Filter = "Workspace files (*.doxyedit)|*.doxyedit";
            // 
            // dlgSaveWorkspace
            // 
            dlgSaveWorkspace.DefaultExt = "doxyedit";
            dlgSaveWorkspace.Filter = "Workspace files (*.doxyedit)|*.doxyedit";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(858, 587);
            Controls.Add(scMainAndLog);
            Controls.Add(statusStrip1);
            Controls.Add(tsMain);
            Controls.Add(mainMenuStrip);
            Font = new System.Drawing.Font("Segoe UI", 9.75F);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = mainMenuStrip;
            Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Doxygen Editor";
            FormClosing += MainForm_FormClosing;
            FormClosed += MainForm_FormClosed;
            Load += MainForm_Load;
            mainMenuStrip.ResumeLayout(false);
            mainMenuStrip.PerformLayout();
            tsMain.ResumeLayout(false);
            tsMain.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            scMainAndLog.Panel1.ResumeLayout(false);
            scMainAndLog.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)scMainAndLog).EndInit();
            scMainAndLog.ResumeLayout(false);
            scTreeAndFiles.Panel1.ResumeLayout(false);
            scTreeAndFiles.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)scTreeAndFiles).EndInit();
            scTreeAndFiles.ResumeLayout(false);
            tcBottom.ResumeLayout(false);
            tpPerformance.ResumeLayout(false);
            cmsTabActions.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

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
        private System.Windows.Forms.ToolStripMenuItem miWorkspaceNew;
        private System.Windows.Forms.ToolStripMenuItem mitWorkspaceLoad;
        private System.Windows.Forms.OpenFileDialog dlgOpenWorkspace;
        private System.Windows.Forms.SaveFileDialog dlgSaveWorkspace;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem miBuild;
        private System.Windows.Forms.ToolStripMenuItem miBuildDocumentation;
        private System.Windows.Forms.ToolStripButton tbtnBuildDocumentation;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem miToolsParseAPIPrototypes;
    }
}