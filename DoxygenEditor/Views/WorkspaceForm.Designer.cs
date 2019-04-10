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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("C/C++");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Parser", new System.Windows.Forms.TreeNode[] {
            treeNode1});
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("C/C++");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Validation", new System.Windows.Forms.TreeNode[] {
            treeNode3});
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Syntax-Highlighting");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Editor", new System.Windows.Forms.TreeNode[] {
            treeNode5});
            this.panControls = new System.Windows.Forms.Panel();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panTree = new System.Windows.Forms.Panel();
            this.tvOptions = new System.Windows.Forms.TreeView();
            this.panOptionsFilterTop = new System.Windows.Forms.Panel();
            this.tbOptionsFilter = new System.Windows.Forms.TextBox();
            this.panTabFill = new System.Windows.Forms.Panel();
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tpParserCpp = new System.Windows.Forms.TabPage();
            this.gbParserCppExcludedSymbols = new System.Windows.Forms.GroupBox();
            this.cbParserCppExcludeFunctionBodySymbols = new System.Windows.Forms.CheckBox();
            this.cbParserCppExcludeFunctionCallSymbols = new System.Windows.Forms.CheckBox();
            this.gbParserCppExcludedNodes = new System.Windows.Forms.GroupBox();
            this.cbParserCppSkipFunctionBodies = new System.Windows.Forms.CheckBox();
            this.tpValidationCpp = new System.Windows.Forms.TabPage();
            this.tpEditorSyntaxHighlighting = new System.Windows.Forms.TabPage();
            this.panOptionsTitleTop = new System.Windows.Forms.Panel();
            this.lblOptionsTitle = new System.Windows.Forms.Label();
            this.gbValidationCppExcludedTypes = new System.Windows.Forms.GroupBox();
            this.cbValidationCppExcludePreprocessorMatch = new System.Windows.Forms.CheckBox();
            this.cbValidationCppExcludePreprocessorUsage = new System.Windows.Forms.CheckBox();
            this.gbValidationCppDocumentation = new System.Windows.Forms.GroupBox();
            this.cbValidationCppRequireDoxygenReference = new System.Windows.Forms.CheckBox();
            this.panControls.SuspendLayout();
            this.panTree.SuspendLayout();
            this.panOptionsFilterTop.SuspendLayout();
            this.panTabFill.SuspendLayout();
            this.tcMain.SuspendLayout();
            this.tpParserCpp.SuspendLayout();
            this.gbParserCppExcludedSymbols.SuspendLayout();
            this.gbParserCppExcludedNodes.SuspendLayout();
            this.tpValidationCpp.SuspendLayout();
            this.panOptionsTitleTop.SuspendLayout();
            this.gbValidationCppExcludedTypes.SuspendLayout();
            this.gbValidationCppDocumentation.SuspendLayout();
            this.SuspendLayout();
            // 
            // panControls
            // 
            this.panControls.Controls.Add(this.btnOk);
            this.panControls.Controls.Add(this.btnCancel);
            this.panControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panControls.Location = new System.Drawing.Point(0, 391);
            this.panControls.Margin = new System.Windows.Forms.Padding(4);
            this.panControls.Name = "panControls";
            this.panControls.Size = new System.Drawing.Size(629, 47);
            this.panControls.TabIndex = 0;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(439, 7);
            this.btnOk.Margin = new System.Windows.Forms.Padding(4);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(88, 30);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(533, 7);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 30);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // panTree
            // 
            this.panTree.Controls.Add(this.tvOptions);
            this.panTree.Controls.Add(this.panOptionsFilterTop);
            this.panTree.Dock = System.Windows.Forms.DockStyle.Left;
            this.panTree.Location = new System.Drawing.Point(0, 0);
            this.panTree.Margin = new System.Windows.Forms.Padding(0);
            this.panTree.Name = "panTree";
            this.panTree.Padding = new System.Windows.Forms.Padding(5, 6, 0, 0);
            this.panTree.Size = new System.Drawing.Size(175, 391);
            this.panTree.TabIndex = 2;
            // 
            // tvOptions
            // 
            this.tvOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tvOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvOptions.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tvOptions.HideSelection = false;
            this.tvOptions.Location = new System.Drawing.Point(5, 36);
            this.tvOptions.Name = "tvOptions";
            treeNode1.Name = "nodeParserCpp";
            treeNode1.Tag = "";
            treeNode1.Text = "C/C++";
            treeNode2.Name = "nodeParser";
            treeNode2.Tag = "";
            treeNode2.Text = "Parser";
            treeNode3.Name = "nodeValidationCpp";
            treeNode3.Text = "C/C++";
            treeNode4.Name = "nodeValidation";
            treeNode4.Text = "Validation";
            treeNode5.Name = "nodeEditorSyntaxHighlighting";
            treeNode5.Text = "Syntax-Highlighting";
            treeNode6.Name = "nodeEditor";
            treeNode6.Text = "Editor";
            this.tvOptions.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode2,
            treeNode4,
            treeNode6});
            this.tvOptions.Size = new System.Drawing.Size(170, 355);
            this.tvOptions.TabIndex = 1;
            this.tvOptions.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvOptions_AfterSelect);
            // 
            // panOptionsFilterTop
            // 
            this.panOptionsFilterTop.Controls.Add(this.tbOptionsFilter);
            this.panOptionsFilterTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panOptionsFilterTop.Location = new System.Drawing.Point(5, 6);
            this.panOptionsFilterTop.Name = "panOptionsFilterTop";
            this.panOptionsFilterTop.Size = new System.Drawing.Size(170, 30);
            this.panOptionsFilterTop.TabIndex = 0;
            // 
            // tbOptionsFilter
            // 
            this.tbOptionsFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbOptionsFilter.Location = new System.Drawing.Point(0, 0);
            this.tbOptionsFilter.Name = "tbOptionsFilter";
            this.tbOptionsFilter.Size = new System.Drawing.Size(170, 25);
            this.tbOptionsFilter.TabIndex = 0;
            // 
            // panTabFill
            // 
            this.panTabFill.Controls.Add(this.tcMain);
            this.panTabFill.Controls.Add(this.panOptionsTitleTop);
            this.panTabFill.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panTabFill.Location = new System.Drawing.Point(175, 0);
            this.panTabFill.Name = "panTabFill";
            this.panTabFill.Size = new System.Drawing.Size(454, 391);
            this.panTabFill.TabIndex = 3;
            // 
            // tcMain
            // 
            this.tcMain.Controls.Add(this.tpParserCpp);
            this.tcMain.Controls.Add(this.tpValidationCpp);
            this.tcMain.Controls.Add(this.tpEditorSyntaxHighlighting);
            this.tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcMain.ItemSize = new System.Drawing.Size(91, 25);
            this.tcMain.Location = new System.Drawing.Point(0, 24);
            this.tcMain.Margin = new System.Windows.Forms.Padding(0);
            this.tcMain.Name = "tcMain";
            this.tcMain.Padding = new System.Drawing.Point(0, 0);
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(454, 367);
            this.tcMain.TabIndex = 1;
            this.tcMain.SelectedIndexChanged += new System.EventHandler(this.tcMain_SelectedIndexChanged);
            // 
            // tpParserCpp
            // 
            this.tpParserCpp.Controls.Add(this.gbParserCppExcludedSymbols);
            this.tpParserCpp.Controls.Add(this.gbParserCppExcludedNodes);
            this.tpParserCpp.Location = new System.Drawing.Point(4, 29);
            this.tpParserCpp.Margin = new System.Windows.Forms.Padding(0);
            this.tpParserCpp.Name = "tpParserCpp";
            this.tpParserCpp.Size = new System.Drawing.Size(446, 334);
            this.tpParserCpp.TabIndex = 0;
            this.tpParserCpp.Text = "Parser\\C/C++";
            this.tpParserCpp.UseVisualStyleBackColor = true;
            // 
            // gbParserCppExcludedSymbols
            // 
            this.gbParserCppExcludedSymbols.AutoSize = true;
            this.gbParserCppExcludedSymbols.Controls.Add(this.cbParserCppExcludeFunctionBodySymbols);
            this.gbParserCppExcludedSymbols.Controls.Add(this.cbParserCppExcludeFunctionCallSymbols);
            this.gbParserCppExcludedSymbols.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbParserCppExcludedSymbols.Location = new System.Drawing.Point(0, 53);
            this.gbParserCppExcludedSymbols.Margin = new System.Windows.Forms.Padding(0);
            this.gbParserCppExcludedSymbols.Name = "gbParserCppExcludedSymbols";
            this.gbParserCppExcludedSymbols.Padding = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.gbParserCppExcludedSymbols.Size = new System.Drawing.Size(446, 76);
            this.gbParserCppExcludedSymbols.TabIndex = 1;
            this.gbParserCppExcludedSymbols.TabStop = false;
            this.gbParserCppExcludedSymbols.Text = "Excluded Symbols";
            // 
            // cbParserCppExcludeFunctionBodySymbols
            // 
            this.cbParserCppExcludeFunctionBodySymbols.AutoSize = true;
            this.cbParserCppExcludeFunctionBodySymbols.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbParserCppExcludeFunctionBodySymbols.Location = new System.Drawing.Point(5, 47);
            this.cbParserCppExcludeFunctionBodySymbols.Name = "cbParserCppExcludeFunctionBodySymbols";
            this.cbParserCppExcludeFunctionBodySymbols.Size = new System.Drawing.Size(436, 23);
            this.cbParserCppExcludeFunctionBodySymbols.TabIndex = 1;
            this.cbParserCppExcludeFunctionBodySymbols.Text = "Function Bodies";
            this.cbParserCppExcludeFunctionBodySymbols.UseVisualStyleBackColor = true;
            // 
            // cbParserCppExcludeFunctionCallSymbols
            // 
            this.cbParserCppExcludeFunctionCallSymbols.AutoSize = true;
            this.cbParserCppExcludeFunctionCallSymbols.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbParserCppExcludeFunctionCallSymbols.Location = new System.Drawing.Point(5, 24);
            this.cbParserCppExcludeFunctionCallSymbols.Name = "cbParserCppExcludeFunctionCallSymbols";
            this.cbParserCppExcludeFunctionCallSymbols.Size = new System.Drawing.Size(436, 23);
            this.cbParserCppExcludeFunctionCallSymbols.TabIndex = 0;
            this.cbParserCppExcludeFunctionCallSymbols.Text = "Function Calls";
            this.cbParserCppExcludeFunctionCallSymbols.UseVisualStyleBackColor = true;
            // 
            // gbParserCppExcludedNodes
            // 
            this.gbParserCppExcludedNodes.AutoSize = true;
            this.gbParserCppExcludedNodes.Controls.Add(this.cbParserCppSkipFunctionBodies);
            this.gbParserCppExcludedNodes.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbParserCppExcludedNodes.Location = new System.Drawing.Point(0, 0);
            this.gbParserCppExcludedNodes.Margin = new System.Windows.Forms.Padding(0);
            this.gbParserCppExcludedNodes.Name = "gbParserCppExcludedNodes";
            this.gbParserCppExcludedNodes.Padding = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.gbParserCppExcludedNodes.Size = new System.Drawing.Size(446, 53);
            this.gbParserCppExcludedNodes.TabIndex = 0;
            this.gbParserCppExcludedNodes.TabStop = false;
            this.gbParserCppExcludedNodes.Text = "Excluded Types";
            // 
            // cbParserCppSkipFunctionBodies
            // 
            this.cbParserCppSkipFunctionBodies.AutoSize = true;
            this.cbParserCppSkipFunctionBodies.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbParserCppSkipFunctionBodies.Location = new System.Drawing.Point(5, 24);
            this.cbParserCppSkipFunctionBodies.Name = "cbParserCppSkipFunctionBodies";
            this.cbParserCppSkipFunctionBodies.Size = new System.Drawing.Size(436, 23);
            this.cbParserCppSkipFunctionBodies.TabIndex = 0;
            this.cbParserCppSkipFunctionBodies.Text = "Function Bodies";
            this.cbParserCppSkipFunctionBodies.UseVisualStyleBackColor = true;
            // 
            // tpValidationCpp
            // 
            this.tpValidationCpp.Controls.Add(this.gbValidationCppDocumentation);
            this.tpValidationCpp.Controls.Add(this.gbValidationCppExcludedTypes);
            this.tpValidationCpp.Location = new System.Drawing.Point(4, 29);
            this.tpValidationCpp.Margin = new System.Windows.Forms.Padding(0);
            this.tpValidationCpp.Name = "tpValidationCpp";
            this.tpValidationCpp.Size = new System.Drawing.Size(446, 334);
            this.tpValidationCpp.TabIndex = 1;
            this.tpValidationCpp.Text = "Validation\\C/C++";
            this.tpValidationCpp.UseVisualStyleBackColor = true;
            // 
            // tpEditorSyntaxHighlighting
            // 
            this.tpEditorSyntaxHighlighting.Location = new System.Drawing.Point(4, 29);
            this.tpEditorSyntaxHighlighting.Margin = new System.Windows.Forms.Padding(0);
            this.tpEditorSyntaxHighlighting.Name = "tpEditorSyntaxHighlighting";
            this.tpEditorSyntaxHighlighting.Size = new System.Drawing.Size(446, 334);
            this.tpEditorSyntaxHighlighting.TabIndex = 2;
            this.tpEditorSyntaxHighlighting.Text = "Editor\\Syntax-Highlighting";
            this.tpEditorSyntaxHighlighting.UseVisualStyleBackColor = true;
            // 
            // panOptionsTitleTop
            // 
            this.panOptionsTitleTop.Controls.Add(this.lblOptionsTitle);
            this.panOptionsTitleTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panOptionsTitleTop.Location = new System.Drawing.Point(0, 0);
            this.panOptionsTitleTop.Name = "panOptionsTitleTop";
            this.panOptionsTitleTop.Size = new System.Drawing.Size(454, 24);
            this.panOptionsTitleTop.TabIndex = 2;
            // 
            // lblOptionsTitle
            // 
            this.lblOptionsTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOptionsTitle.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOptionsTitle.Location = new System.Drawing.Point(0, 0);
            this.lblOptionsTitle.Name = "lblOptionsTitle";
            this.lblOptionsTitle.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.lblOptionsTitle.Size = new System.Drawing.Size(454, 24);
            this.lblOptionsTitle.TabIndex = 0;
            this.lblOptionsTitle.Text = "[Options Title]";
            // 
            // gbValidationCppExcludedTypes
            // 
            this.gbValidationCppExcludedTypes.AutoSize = true;
            this.gbValidationCppExcludedTypes.Controls.Add(this.cbValidationCppExcludePreprocessorUsage);
            this.gbValidationCppExcludedTypes.Controls.Add(this.cbValidationCppExcludePreprocessorMatch);
            this.gbValidationCppExcludedTypes.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbValidationCppExcludedTypes.Location = new System.Drawing.Point(0, 0);
            this.gbValidationCppExcludedTypes.Margin = new System.Windows.Forms.Padding(0);
            this.gbValidationCppExcludedTypes.Name = "gbValidationCppExcludedTypes";
            this.gbValidationCppExcludedTypes.Padding = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.gbValidationCppExcludedTypes.Size = new System.Drawing.Size(446, 76);
            this.gbValidationCppExcludedTypes.TabIndex = 1;
            this.gbValidationCppExcludedTypes.TabStop = false;
            this.gbValidationCppExcludedTypes.Text = "Excluded Types";
            // 
            // cbValidationCppExcludePreprocessorMatch
            // 
            this.cbValidationCppExcludePreprocessorMatch.AutoSize = true;
            this.cbValidationCppExcludePreprocessorMatch.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbValidationCppExcludePreprocessorMatch.Location = new System.Drawing.Point(5, 24);
            this.cbValidationCppExcludePreprocessorMatch.Name = "cbValidationCppExcludePreprocessorMatch";
            this.cbValidationCppExcludePreprocessorMatch.Size = new System.Drawing.Size(436, 23);
            this.cbValidationCppExcludePreprocessorMatch.TabIndex = 0;
            this.cbValidationCppExcludePreprocessorMatch.Text = "Preprocessor Match";
            this.cbValidationCppExcludePreprocessorMatch.UseVisualStyleBackColor = true;
            // 
            // cbValidationCppExcludePreprocessorUsage
            // 
            this.cbValidationCppExcludePreprocessorUsage.AutoSize = true;
            this.cbValidationCppExcludePreprocessorUsage.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbValidationCppExcludePreprocessorUsage.Location = new System.Drawing.Point(5, 47);
            this.cbValidationCppExcludePreprocessorUsage.Name = "cbValidationCppExcludePreprocessorUsage";
            this.cbValidationCppExcludePreprocessorUsage.Size = new System.Drawing.Size(436, 23);
            this.cbValidationCppExcludePreprocessorUsage.TabIndex = 1;
            this.cbValidationCppExcludePreprocessorUsage.Text = "Preprocessor Usage";
            this.cbValidationCppExcludePreprocessorUsage.UseVisualStyleBackColor = true;
            // 
            // gbValidationCppDocumentation
            // 
            this.gbValidationCppDocumentation.AutoSize = true;
            this.gbValidationCppDocumentation.Controls.Add(this.cbValidationCppRequireDoxygenReference);
            this.gbValidationCppDocumentation.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbValidationCppDocumentation.Location = new System.Drawing.Point(0, 76);
            this.gbValidationCppDocumentation.Margin = new System.Windows.Forms.Padding(0);
            this.gbValidationCppDocumentation.Name = "gbValidationCppDocumentation";
            this.gbValidationCppDocumentation.Padding = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.gbValidationCppDocumentation.Size = new System.Drawing.Size(446, 53);
            this.gbValidationCppDocumentation.TabIndex = 2;
            this.gbValidationCppDocumentation.TabStop = false;
            this.gbValidationCppDocumentation.Text = "Documentation";
            // 
            // cbValidationCppRequireDoxygenReference
            // 
            this.cbValidationCppRequireDoxygenReference.AutoSize = true;
            this.cbValidationCppRequireDoxygenReference.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbValidationCppRequireDoxygenReference.Location = new System.Drawing.Point(5, 24);
            this.cbValidationCppRequireDoxygenReference.Name = "cbValidationCppRequireDoxygenReference";
            this.cbValidationCppRequireDoxygenReference.Size = new System.Drawing.Size(436, 23);
            this.cbValidationCppRequireDoxygenReference.TabIndex = 0;
            this.cbValidationCppRequireDoxygenReference.Text = "Require Doxygen Reference (@see @ref)";
            this.cbValidationCppRequireDoxygenReference.UseVisualStyleBackColor = true;
            // 
            // WorkspaceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(629, 438);
            this.Controls.Add(this.panTabFill);
            this.Controls.Add(this.panTree);
            this.Controls.Add(this.panControls);
            this.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WorkspaceForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Workspace Configuration";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.WorkspaceForm_FormClosed);
            this.panControls.ResumeLayout(false);
            this.panTree.ResumeLayout(false);
            this.panOptionsFilterTop.ResumeLayout(false);
            this.panOptionsFilterTop.PerformLayout();
            this.panTabFill.ResumeLayout(false);
            this.tcMain.ResumeLayout(false);
            this.tpParserCpp.ResumeLayout(false);
            this.tpParserCpp.PerformLayout();
            this.gbParserCppExcludedSymbols.ResumeLayout(false);
            this.gbParserCppExcludedSymbols.PerformLayout();
            this.gbParserCppExcludedNodes.ResumeLayout(false);
            this.gbParserCppExcludedNodes.PerformLayout();
            this.tpValidationCpp.ResumeLayout(false);
            this.tpValidationCpp.PerformLayout();
            this.panOptionsTitleTop.ResumeLayout(false);
            this.gbValidationCppExcludedTypes.ResumeLayout(false);
            this.gbValidationCppExcludedTypes.PerformLayout();
            this.gbValidationCppDocumentation.ResumeLayout(false);
            this.gbValidationCppDocumentation.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panControls;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Panel panTree;
        private System.Windows.Forms.Panel panOptionsFilterTop;
        private System.Windows.Forms.TreeView tvOptions;
        private System.Windows.Forms.TextBox tbOptionsFilter;
        private System.Windows.Forms.Panel panTabFill;
        private System.Windows.Forms.TabControl tcMain;
        private System.Windows.Forms.TabPage tpParserCpp;
        private System.Windows.Forms.GroupBox gbParserCppExcludedSymbols;
        private System.Windows.Forms.CheckBox cbParserCppExcludeFunctionBodySymbols;
        private System.Windows.Forms.CheckBox cbParserCppExcludeFunctionCallSymbols;
        private System.Windows.Forms.GroupBox gbParserCppExcludedNodes;
        private System.Windows.Forms.CheckBox cbParserCppSkipFunctionBodies;
        private System.Windows.Forms.TabPage tpValidationCpp;
        private System.Windows.Forms.TabPage tpEditorSyntaxHighlighting;
        private System.Windows.Forms.Panel panOptionsTitleTop;
        private System.Windows.Forms.Label lblOptionsTitle;
        private System.Windows.Forms.GroupBox gbValidationCppExcludedTypes;
        private System.Windows.Forms.CheckBox cbValidationCppExcludePreprocessorMatch;
        private System.Windows.Forms.CheckBox cbValidationCppExcludePreprocessorUsage;
        private System.Windows.Forms.GroupBox gbValidationCppDocumentation;
        private System.Windows.Forms.CheckBox cbValidationCppRequireDoxygenReference;
    }
}