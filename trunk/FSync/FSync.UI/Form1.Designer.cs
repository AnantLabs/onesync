namespace OneSync.UI
{
    partial class Form1
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
            this.buttonCreate = new System.Windows.Forms.Button();
            this.textSyncSource = new System.Windows.Forms.TextBox();
            this.textMdSource = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textProfileName = new System.Windows.Forms.TextBox();
            this.buttonCreateSchema = new System.Windows.Forms.Button();
            this.buttonLoadProfile = new System.Windows.Forms.Button();
            this.buttonSync = new System.Windows.Forms.Button();
            this.buttonInsertMd = new System.Windows.Forms.Button();
            this.buttonLoadMd = new System.Windows.Forms.Button();
            this.buttonLoadActions = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonCreate
            // 
            this.buttonCreate.Location = new System.Drawing.Point(12, 129);
            this.buttonCreate.Name = "buttonCreate";
            this.buttonCreate.Size = new System.Drawing.Size(116, 32);
            this.buttonCreate.TabIndex = 0;
            this.buttonCreate.Text = "Create Profile";
            this.buttonCreate.UseVisualStyleBackColor = true;
            this.buttonCreate.Click += new System.EventHandler(this.buttonCreate_Click);
            // 
            // textSyncSource
            // 
            this.textSyncSource.Location = new System.Drawing.Point(158, 57);
            this.textSyncSource.Name = "textSyncSource";
            this.textSyncSource.Size = new System.Drawing.Size(240, 22);
            this.textSyncSource.TabIndex = 1;
            this.textSyncSource.Text = "C:\\Users\\chockablock\\Desktop\\source";
            // 
            // textMdSource
            // 
            this.textMdSource.Location = new System.Drawing.Point(158, 85);
            this.textMdSource.Name = "textMdSource";
            this.textMdSource.Size = new System.Drawing.Size(240, 22);
            this.textMdSource.TabIndex = 2;
            this.textMdSource.Text = "C:\\Users\\chockablock\\Desktop\\Sync Folder";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Sync source";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(116, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Metadata Source";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "Profile Name";
            // 
            // textProfileName
            // 
            this.textProfileName.Location = new System.Drawing.Point(158, 30);
            this.textProfileName.Name = "textProfileName";
            this.textProfileName.Size = new System.Drawing.Size(240, 22);
            this.textProfileName.TabIndex = 6;
            // 
            // buttonCreateSchema
            // 
            this.buttonCreateSchema.Location = new System.Drawing.Point(134, 129);
            this.buttonCreateSchema.Name = "buttonCreateSchema";
            this.buttonCreateSchema.Size = new System.Drawing.Size(124, 32);
            this.buttonCreateSchema.TabIndex = 7;
            this.buttonCreateSchema.Text = "Create Schema";
            this.buttonCreateSchema.UseVisualStyleBackColor = true;
            this.buttonCreateSchema.Click += new System.EventHandler(this.buttonCreateSchema_Click);
            // 
            // buttonLoadProfile
            // 
            this.buttonLoadProfile.Location = new System.Drawing.Point(264, 129);
            this.buttonLoadProfile.Name = "buttonLoadProfile";
            this.buttonLoadProfile.Size = new System.Drawing.Size(105, 32);
            this.buttonLoadProfile.TabIndex = 8;
            this.buttonLoadProfile.Text = "Load Profiles";
            this.buttonLoadProfile.UseVisualStyleBackColor = true;
            this.buttonLoadProfile.Click += new System.EventHandler(this.buttonLoadProfile_Click);
            // 
            // buttonSync
            // 
            this.buttonSync.Location = new System.Drawing.Point(12, 176);
            this.buttonSync.Name = "buttonSync";
            this.buttonSync.Size = new System.Drawing.Size(116, 32);
            this.buttonSync.TabIndex = 9;
            this.buttonSync.Text = "Sync";
            this.buttonSync.UseVisualStyleBackColor = true;
            this.buttonSync.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // buttonInsertMd
            // 
            this.buttonInsertMd.Location = new System.Drawing.Point(134, 176);
            this.buttonInsertMd.Name = "buttonInsertMd";
            this.buttonInsertMd.Size = new System.Drawing.Size(124, 32);
            this.buttonInsertMd.TabIndex = 10;
            this.buttonInsertMd.Text = "Insert MD";
            this.buttonInsertMd.UseVisualStyleBackColor = true;
            this.buttonInsertMd.Click += new System.EventHandler(this.buttonInsertMd_Click);
            // 
            // buttonLoadMd
            // 
            this.buttonLoadMd.Location = new System.Drawing.Point(264, 176);
            this.buttonLoadMd.Name = "buttonLoadMd";
            this.buttonLoadMd.Size = new System.Drawing.Size(105, 32);
            this.buttonLoadMd.TabIndex = 11;
            this.buttonLoadMd.Text = "Load MD";
            this.buttonLoadMd.UseVisualStyleBackColor = true;
            this.buttonLoadMd.Click += new System.EventHandler(this.buttonLoadMd_Click);
            // 
            // buttonLoadActions
            // 
            this.buttonLoadActions.Location = new System.Drawing.Point(12, 223);
            this.buttonLoadActions.Name = "buttonLoadActions";
            this.buttonLoadActions.Size = new System.Drawing.Size(116, 32);
            this.buttonLoadActions.TabIndex = 12;
            this.buttonLoadActions.Text = "Load Actions";
            this.buttonLoadActions.UseVisualStyleBackColor = true;
            this.buttonLoadActions.Click += new System.EventHandler(this.buttonLoadActions_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 285);
            this.Controls.Add(this.buttonLoadActions);
            this.Controls.Add(this.buttonLoadMd);
            this.Controls.Add(this.buttonInsertMd);
            this.Controls.Add(this.buttonSync);
            this.Controls.Add(this.buttonLoadProfile);
            this.Controls.Add(this.buttonCreateSchema);
            this.Controls.Add(this.textProfileName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textMdSource);
            this.Controls.Add(this.textSyncSource);
            this.Controls.Add(this.buttonCreate);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCreate;
        private System.Windows.Forms.TextBox textSyncSource;
        private System.Windows.Forms.TextBox textMdSource;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textProfileName;
        private System.Windows.Forms.Button buttonCreateSchema;
        private System.Windows.Forms.Button buttonLoadProfile;
        private System.Windows.Forms.Button buttonSync;
        private System.Windows.Forms.Button buttonInsertMd;
        private System.Windows.Forms.Button buttonLoadMd;
        private System.Windows.Forms.Button buttonLoadActions;
    }
}

