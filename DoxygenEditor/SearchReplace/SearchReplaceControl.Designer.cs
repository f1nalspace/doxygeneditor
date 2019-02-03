namespace DoxygenEditor.SearchReplace
{
    partial class SearchReplaceControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panSearch = new System.Windows.Forms.Panel();
            this.btnSearchPrev = new System.Windows.Forms.Button();
            this.btnSearchNext = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnToggleReplace = new System.Windows.Forms.Button();
            this.cbSearchText = new System.Windows.Forms.ComboBox();
            this.panReplace = new System.Windows.Forms.Panel();
            this.btnReplaceAll = new System.Windows.Forms.Button();
            this.btnReplaceNext = new System.Windows.Forms.Button();
            this.cbReplaceText = new System.Windows.Forms.ComboBox();
            this.panOptions = new System.Windows.Forms.Panel();
            this.cbWrap = new System.Windows.Forms.CheckBox();
            this.cbIsRegex = new System.Windows.Forms.CheckBox();
            this.cbMatchWords = new System.Windows.Forms.CheckBox();
            this.cbMatchCase = new System.Windows.Forms.CheckBox();
            this.panSearch.SuspendLayout();
            this.panReplace.SuspendLayout();
            this.panOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // panSearch
            // 
            this.panSearch.Controls.Add(this.btnSearchPrev);
            this.panSearch.Controls.Add(this.btnSearchNext);
            this.panSearch.Controls.Add(this.btnClose);
            this.panSearch.Controls.Add(this.btnToggleReplace);
            this.panSearch.Controls.Add(this.cbSearchText);
            this.panSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.panSearch.Location = new System.Drawing.Point(0, 0);
            this.panSearch.Name = "panSearch";
            this.panSearch.Size = new System.Drawing.Size(435, 32);
            this.panSearch.TabIndex = 20;
            // 
            // btnSearchPrev
            // 
            this.btnSearchPrev.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearchPrev.Location = new System.Drawing.Point(338, 3);
            this.btnSearchPrev.Name = "btnSearchPrev";
            this.btnSearchPrev.Size = new System.Drawing.Size(26, 24);
            this.btnSearchPrev.TabIndex = 20;
            this.btnSearchPrev.TabStop = false;
            this.btnSearchPrev.Text = "<";
            this.btnSearchPrev.UseVisualStyleBackColor = true;
            this.btnSearchPrev.Click += new System.EventHandler(this.btnSearchPrev_Click);
            // 
            // btnSearchNext
            // 
            this.btnSearchNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearchNext.Location = new System.Drawing.Point(370, 3);
            this.btnSearchNext.Name = "btnSearchNext";
            this.btnSearchNext.Size = new System.Drawing.Size(26, 24);
            this.btnSearchNext.TabIndex = 19;
            this.btnSearchNext.TabStop = false;
            this.btnSearchNext.Text = ">";
            this.btnSearchNext.UseVisualStyleBackColor = true;
            this.btnSearchNext.Click += new System.EventHandler(this.btnSearchNext_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(402, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(26, 24);
            this.btnClose.TabIndex = 18;
            this.btnClose.TabStop = false;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnToggleReplace
            // 
            this.btnToggleReplace.Location = new System.Drawing.Point(6, 3);
            this.btnToggleReplace.Name = "btnToggleReplace";
            this.btnToggleReplace.Size = new System.Drawing.Size(26, 24);
            this.btnToggleReplace.TabIndex = 17;
            this.btnToggleReplace.TabStop = false;
            this.btnToggleReplace.Text = "^";
            this.btnToggleReplace.UseVisualStyleBackColor = true;
            this.btnToggleReplace.Click += new System.EventHandler(this.btnToggleReplace_Click);
            // 
            // cbSearchText
            // 
            this.cbSearchText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSearchText.FormattingEnabled = true;
            this.cbSearchText.Location = new System.Drawing.Point(38, 3);
            this.cbSearchText.Name = "cbSearchText";
            this.cbSearchText.Size = new System.Drawing.Size(294, 24);
            this.cbSearchText.TabIndex = 16;
            this.cbSearchText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbSearchText_KeyDown);
            // 
            // panReplace
            // 
            this.panReplace.Controls.Add(this.btnReplaceAll);
            this.panReplace.Controls.Add(this.btnReplaceNext);
            this.panReplace.Controls.Add(this.cbReplaceText);
            this.panReplace.Dock = System.Windows.Forms.DockStyle.Top;
            this.panReplace.Location = new System.Drawing.Point(0, 32);
            this.panReplace.Name = "panReplace";
            this.panReplace.Size = new System.Drawing.Size(435, 31);
            this.panReplace.TabIndex = 21;
            // 
            // btnReplaceAll
            // 
            this.btnReplaceAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReplaceAll.Location = new System.Drawing.Point(370, 3);
            this.btnReplaceAll.Name = "btnReplaceAll";
            this.btnReplaceAll.Size = new System.Drawing.Size(26, 24);
            this.btnReplaceAll.TabIndex = 22;
            this.btnReplaceAll.TabStop = false;
            this.btnReplaceAll.Text = "R";
            this.btnReplaceAll.UseVisualStyleBackColor = true;
            this.btnReplaceAll.Click += new System.EventHandler(this.btnReplaceAll_Click);
            // 
            // btnReplaceNext
            // 
            this.btnReplaceNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReplaceNext.Location = new System.Drawing.Point(338, 3);
            this.btnReplaceNext.Name = "btnReplaceNext";
            this.btnReplaceNext.Size = new System.Drawing.Size(26, 24);
            this.btnReplaceNext.TabIndex = 21;
            this.btnReplaceNext.TabStop = false;
            this.btnReplaceNext.Text = "r";
            this.btnReplaceNext.UseVisualStyleBackColor = true;
            this.btnReplaceNext.Click += new System.EventHandler(this.btnReplaceNext_Click);
            // 
            // cbReplaceText
            // 
            this.cbReplaceText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbReplaceText.FormattingEnabled = true;
            this.cbReplaceText.Location = new System.Drawing.Point(38, 3);
            this.cbReplaceText.Name = "cbReplaceText";
            this.cbReplaceText.Size = new System.Drawing.Size(294, 24);
            this.cbReplaceText.TabIndex = 20;
            this.cbReplaceText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbReplaceText_KeyDown);
            // 
            // panOptions
            // 
            this.panOptions.Controls.Add(this.cbWrap);
            this.panOptions.Controls.Add(this.cbIsRegex);
            this.panOptions.Controls.Add(this.cbMatchWords);
            this.panOptions.Controls.Add(this.cbMatchCase);
            this.panOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.panOptions.Location = new System.Drawing.Point(0, 63);
            this.panOptions.Name = "panOptions";
            this.panOptions.Size = new System.Drawing.Size(435, 31);
            this.panOptions.TabIndex = 22;
            // 
            // cbWrap
            // 
            this.cbWrap.AutoSize = true;
            this.cbWrap.Location = new System.Drawing.Point(338, 6);
            this.cbWrap.Name = "cbWrap";
            this.cbWrap.Size = new System.Drawing.Size(64, 21);
            this.cbWrap.TabIndex = 22;
            this.cbWrap.Text = "Wrap";
            this.cbWrap.UseVisualStyleBackColor = true;
            // 
            // cbIsRegex
            // 
            this.cbIsRegex.AutoSize = true;
            this.cbIsRegex.Location = new System.Drawing.Point(252, 6);
            this.cbIsRegex.Name = "cbIsRegex";
            this.cbIsRegex.Size = new System.Drawing.Size(84, 21);
            this.cbIsRegex.TabIndex = 21;
            this.cbIsRegex.Text = "Is Regex";
            this.cbIsRegex.UseVisualStyleBackColor = true;
            // 
            // cbMatchWords
            // 
            this.cbMatchWords.AutoSize = true;
            this.cbMatchWords.Location = new System.Drawing.Point(146, 6);
            this.cbMatchWords.Name = "cbMatchWords";
            this.cbMatchWords.Size = new System.Drawing.Size(104, 21);
            this.cbMatchWords.TabIndex = 20;
            this.cbMatchWords.Text = "Whole word";
            this.cbMatchWords.UseVisualStyleBackColor = true;
            // 
            // cbMatchCase
            // 
            this.cbMatchCase.AutoSize = true;
            this.cbMatchCase.Location = new System.Drawing.Point(38, 6);
            this.cbMatchCase.Name = "cbMatchCase";
            this.cbMatchCase.Size = new System.Drawing.Size(102, 21);
            this.cbMatchCase.TabIndex = 19;
            this.cbMatchCase.Text = "Match case";
            this.cbMatchCase.UseVisualStyleBackColor = true;
            // 
            // SearchReplaceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panOptions);
            this.Controls.Add(this.panReplace);
            this.Controls.Add(this.panSearch);
            this.Name = "SearchReplaceControl";
            this.Size = new System.Drawing.Size(435, 95);
            this.panSearch.ResumeLayout(false);
            this.panReplace.ResumeLayout(false);
            this.panOptions.ResumeLayout(false);
            this.panOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panSearch;
        private System.Windows.Forms.Button btnSearchPrev;
        private System.Windows.Forms.Button btnSearchNext;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnToggleReplace;
        private System.Windows.Forms.ComboBox cbSearchText;
        private System.Windows.Forms.Panel panReplace;
        private System.Windows.Forms.ComboBox cbReplaceText;
        private System.Windows.Forms.Panel panOptions;
        private System.Windows.Forms.CheckBox cbIsRegex;
        private System.Windows.Forms.CheckBox cbMatchWords;
        private System.Windows.Forms.CheckBox cbMatchCase;
        private System.Windows.Forms.Button btnReplaceAll;
        private System.Windows.Forms.Button btnReplaceNext;
        private System.Windows.Forms.CheckBox cbWrap;
    }
}
