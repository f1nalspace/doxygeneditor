namespace TSP.DoxygenEditor.FilterControls
{
    partial class FilterBarControl
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilterBarControl));
            this.panFilterEdit = new System.Windows.Forms.Panel();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.lblFilter = new System.Windows.Forms.Label();
            this.btnClear = new System.Windows.Forms.Button();
            this.imglstButtons = new System.Windows.Forms.ImageList(this.components);
            this.btnToggleVisibility = new System.Windows.Forms.Button();
            this.panFilterEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // panFilterEdit
            // 
            this.panFilterEdit.Controls.Add(this.tbFilter);
            this.panFilterEdit.Controls.Add(this.lblFilter);
            this.panFilterEdit.Controls.Add(this.btnClear);
            this.panFilterEdit.Controls.Add(this.btnToggleVisibility);
            this.panFilterEdit.Dock = System.Windows.Forms.DockStyle.Top;
            this.panFilterEdit.Location = new System.Drawing.Point(0, 0);
            this.panFilterEdit.Name = "panFilterEdit";
            this.panFilterEdit.Padding = new System.Windows.Forms.Padding(2);
            this.panFilterEdit.Size = new System.Drawing.Size(616, 28);
            this.panFilterEdit.TabIndex = 4;
            // 
            // tbFilter
            // 
            this.tbFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbFilter.Location = new System.Drawing.Point(45, 2);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(517, 22);
            this.tbFilter.TabIndex = 5;
            this.tbFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbFilter_KeyDown);
            // 
            // lblFilter
            // 
            this.lblFilter.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblFilter.Location = new System.Drawing.Point(2, 2);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Size = new System.Drawing.Size(43, 24);
            this.lblFilter.TabIndex = 4;
            this.lblFilter.Text = "Filter:";
            this.lblFilter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnClear
            // 
            this.btnClear.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClear.ImageKey = "DeleteFilter_16x.png";
            this.btnClear.ImageList = this.imglstButtons;
            this.btnClear.Location = new System.Drawing.Point(562, 2);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(26, 24);
            this.btnClear.TabIndex = 7;
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // imglstButtons
            // 
            this.imglstButtons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imglstButtons.ImageStream")));
            this.imglstButtons.TransparentColor = System.Drawing.Color.Transparent;
            this.imglstButtons.Images.SetKeyName(0, "CollapseArrow_16x.png");
            this.imglstButtons.Images.SetKeyName(1, "ExpandArrow_16x.png");
            this.imglstButtons.Images.SetKeyName(2, "DeleteFilter_16x.png");
            this.imglstButtons.Images.SetKeyName(3, "FilterTextbox_16x.png");
            // 
            // btnToggleVisibility
            // 
            this.btnToggleVisibility.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnToggleVisibility.ImageKey = "CollapseArrow_16x.png";
            this.btnToggleVisibility.ImageList = this.imglstButtons;
            this.btnToggleVisibility.Location = new System.Drawing.Point(588, 2);
            this.btnToggleVisibility.Name = "btnToggleVisibility";
            this.btnToggleVisibility.Size = new System.Drawing.Size(26, 24);
            this.btnToggleVisibility.TabIndex = 6;
            this.btnToggleVisibility.UseVisualStyleBackColor = true;
            this.btnToggleVisibility.Click += new System.EventHandler(this.btnToggleVisibility_Click);
            // 
            // FilterBarControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panFilterEdit);
            this.Name = "FilterBarControl";
            this.Size = new System.Drawing.Size(616, 28);
            this.panFilterEdit.ResumeLayout(false);
            this.panFilterEdit.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panFilterEdit;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnToggleVisibility;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.Label lblFilter;
        private System.Windows.Forms.ImageList imglstButtons;
    }
}
