namespace OneSync.UI.FolderDiff
{
    partial class FolderDiff
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkBoxShowFolders = new System.Windows.Forms.CheckBox();
            this.checkBoxMonitor = new System.Windows.Forms.CheckBox();
            this.buttonCompare = new System.Windows.Forms.Button();
            this.buttonBrowseDestinationFolder = new System.Windows.Forms.Button();
            this.textDestinationFolder = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textSourceFolder = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonBrowseSourceFolder = new System.Windows.Forms.Button();
            this.listViewDifference = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.panel1.Controls.Add(this.checkBoxShowFolders);
            this.panel1.Controls.Add(this.checkBoxMonitor);
            this.panel1.Controls.Add(this.buttonCompare);
            this.panel1.Controls.Add(this.buttonBrowseDestinationFolder);
            this.panel1.Controls.Add(this.textDestinationFolder);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.textSourceFolder);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.buttonBrowseSourceFolder);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(949, 73);
            this.panel1.TabIndex = 0;
            // 
            // checkBoxShowFolders
            // 
            this.checkBoxShowFolders.AutoSize = true;
            this.checkBoxShowFolders.Location = new System.Drawing.Point(475, 41);
            this.checkBoxShowFolders.Name = "checkBoxShowFolders";
            this.checkBoxShowFolders.Size = new System.Drawing.Size(111, 21);
            this.checkBoxShowFolders.TabIndex = 9;
            this.checkBoxShowFolders.Text = "Show folders";
            this.checkBoxShowFolders.UseVisualStyleBackColor = true;
            this.checkBoxShowFolders.CheckedChanged += new System.EventHandler(this.checkBoxShowFolders_CheckedChanged);
            // 
            // checkBoxMonitor
            // 
            this.checkBoxMonitor.AutoSize = true;
            this.checkBoxMonitor.Checked = true;
            this.checkBoxMonitor.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMonitor.Location = new System.Drawing.Point(475, 13);
            this.checkBoxMonitor.Name = "checkBoxMonitor";
            this.checkBoxMonitor.Size = new System.Drawing.Size(77, 21);
            this.checkBoxMonitor.TabIndex = 6;
            this.checkBoxMonitor.Text = "Monitor";
            this.checkBoxMonitor.UseVisualStyleBackColor = true;
            this.checkBoxMonitor.CheckedChanged += new System.EventHandler(this.checkBoxMonitor_CheckedChanged);
            // 
            // buttonCompare
            // 
            this.buttonCompare.Location = new System.Drawing.Point(585, 10);
            this.buttonCompare.Name = "buttonCompare";
            this.buttonCompare.Size = new System.Drawing.Size(129, 51);
            this.buttonCompare.TabIndex = 5;
            this.buttonCompare.Text = "Compare";
            this.buttonCompare.UseVisualStyleBackColor = true;
            this.buttonCompare.Click += new System.EventHandler(this.buttonCompare_Click);
            // 
            // buttonBrowseDestinationFolder
            // 
            this.buttonBrowseDestinationFolder.Location = new System.Drawing.Point(380, 41);
            this.buttonBrowseDestinationFolder.Name = "buttonBrowseDestinationFolder";
            this.buttonBrowseDestinationFolder.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseDestinationFolder.TabIndex = 2;
            this.buttonBrowseDestinationFolder.Text = "Browse";
            this.buttonBrowseDestinationFolder.UseVisualStyleBackColor = true;
            this.buttonBrowseDestinationFolder.Click += new System.EventHandler(this.buttonBrowseDestinationFolder_Click);
            // 
            // textDestinationFolder
            // 
            this.textDestinationFolder.Location = new System.Drawing.Point(128, 41);
            this.textDestinationFolder.Name = "textDestinationFolder";
            this.textDestinationFolder.Size = new System.Drawing.Size(246, 22);
            this.textDestinationFolder.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Destination folder";
            // 
            // textSourceFolder
            // 
            this.textSourceFolder.Location = new System.Drawing.Point(128, 9);
            this.textSourceFolder.Name = "textSourceFolder";
            this.textSourceFolder.Size = new System.Drawing.Size(246, 22);
            this.textSourceFolder.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Source folder";
            // 
            // buttonBrowseSourceFolder
            // 
            this.buttonBrowseSourceFolder.Location = new System.Drawing.Point(380, 12);
            this.buttonBrowseSourceFolder.Name = "buttonBrowseSourceFolder";
            this.buttonBrowseSourceFolder.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseSourceFolder.TabIndex = 0;
            this.buttonBrowseSourceFolder.Text = "Browse";
            this.buttonBrowseSourceFolder.UseVisualStyleBackColor = true;
            this.buttonBrowseSourceFolder.Click += new System.EventHandler(this.buttonBrowseSourceFolder_Click);
            // 
            // listViewDifference
            // 
            this.listViewDifference.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listViewDifference.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewDifference.FullRowSelect = true;
            this.listViewDifference.Location = new System.Drawing.Point(0, 73);
            this.listViewDifference.Name = "listViewDifference";
            this.listViewDifference.Size = new System.Drawing.Size(949, 338);
            this.listViewDifference.TabIndex = 1;
            this.listViewDifference.UseCompatibleStateImageBehavior = false;
            this.listViewDifference.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Id";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "File";
            this.columnHeader2.Width = 350;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Difference";
            this.columnHeader3.Width = 350;
            // 
            // FolderDiff
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(949, 411);
            this.Controls.Add(this.listViewDifference);
            this.Controls.Add(this.panel1);
            this.Name = "FolderDiff";
            this.Text = "Folder Diff";
            this.Load += new System.EventHandler(this.FolderDiff_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView listViewDifference;
        private System.Windows.Forms.Button buttonBrowseDestinationFolder;
        private System.Windows.Forms.TextBox textDestinationFolder;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textSourceFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonBrowseSourceFolder;
        private System.Windows.Forms.Button buttonCompare;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.CheckBox checkBoxMonitor;
        private System.Windows.Forms.CheckBox checkBoxShowFolders;
    }
}

