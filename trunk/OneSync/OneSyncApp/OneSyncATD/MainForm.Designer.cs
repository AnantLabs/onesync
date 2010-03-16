namespace OneSyncATD
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
            this.label1 = new System.Windows.Forms.Label();
            this.filePath = new System.Windows.Forms.TextBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.startButton = new System.Windows.Forms.Button();
            this.leftFolderButton = new System.Windows.Forms.Button();
            this.leftFolderText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.rightFolderButton = new System.Windows.Forms.Button();
            this.rightFolderText = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.interButton = new System.Windows.Forms.Button();
            this.interFolderText = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 158);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Test Cases:";
            // 
            // filePath
            // 
            this.filePath.Location = new System.Drawing.Point(96, 155);
            this.filePath.Name = "filePath";
            this.filePath.Size = new System.Drawing.Size(309, 20);
            this.filePath.TabIndex = 1;
            this.filePath.Click += new System.EventHandler(this.filePath_Click);
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(402, 153);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(75, 23);
            this.browseButton.TabIndex = 2;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(216, 206);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 3;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // leftFolderButton
            // 
            this.leftFolderButton.Location = new System.Drawing.Point(402, 115);
            this.leftFolderButton.Name = "leftFolderButton";
            this.leftFolderButton.Size = new System.Drawing.Size(75, 23);
            this.leftFolderButton.TabIndex = 6;
            this.leftFolderButton.Text = "Browse";
            this.leftFolderButton.UseVisualStyleBackColor = true;
            this.leftFolderButton.Click += new System.EventHandler(this.leftFolderButton_Click);
            // 
            // leftFolderText
            // 
            this.leftFolderText.Location = new System.Drawing.Point(96, 117);
            this.leftFolderText.Name = "leftFolderText";
            this.leftFolderText.Size = new System.Drawing.Size(309, 20);
            this.leftFolderText.TabIndex = 5;
            this.leftFolderText.Click += new System.EventHandler(this.leftFolderText_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 120);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Left Folder:";
            // 
            // rightFolderButton
            // 
            this.rightFolderButton.Location = new System.Drawing.Point(402, 37);
            this.rightFolderButton.Name = "rightFolderButton";
            this.rightFolderButton.Size = new System.Drawing.Size(75, 23);
            this.rightFolderButton.TabIndex = 9;
            this.rightFolderButton.Text = "Browse";
            this.rightFolderButton.UseVisualStyleBackColor = true;
            this.rightFolderButton.Click += new System.EventHandler(this.rightFolderButton_Click);
            // 
            // rightFolderText
            // 
            this.rightFolderText.Location = new System.Drawing.Point(96, 39);
            this.rightFolderText.Name = "rightFolderText";
            this.rightFolderText.Size = new System.Drawing.Size(309, 20);
            this.rightFolderText.TabIndex = 8;
            this.rightFolderText.Click += new System.EventHandler(this.rightFolderText_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Right Folder: ";
            // 
            // interButton
            // 
            this.interButton.Location = new System.Drawing.Point(402, 77);
            this.interButton.Name = "interButton";
            this.interButton.Size = new System.Drawing.Size(75, 23);
            this.interButton.TabIndex = 12;
            this.interButton.Text = "Browse";
            this.interButton.UseVisualStyleBackColor = true;
            this.interButton.Click += new System.EventHandler(this.interButton_Click);
            // 
            // interFolderText
            // 
            this.interFolderText.Location = new System.Drawing.Point(96, 79);
            this.interFolderText.Name = "interFolderText";
            this.interFolderText.Size = new System.Drawing.Size(309, 20);
            this.interFolderText.TabIndex = 11;
            this.interFolderText.Click += new System.EventHandler(this.interFolderText_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(27, 82);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Intermediate:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(533, 245);
            this.Controls.Add(this.interButton);
            this.Controls.Add(this.interFolderText);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.rightFolderButton);
            this.Controls.Add(this.rightFolderText);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.leftFolderButton);
            this.Controls.Add(this.leftFolderText);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.filePath);
            this.Controls.Add(this.label1);
            this.Name = "MainForm";
            this.Text = "OneSyncATD";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox filePath;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button leftFolderButton;
        private System.Windows.Forms.TextBox leftFolderText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button rightFolderButton;
        private System.Windows.Forms.TextBox rightFolderText;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button interButton;
        private System.Windows.Forms.TextBox interFolderText;
        private System.Windows.Forms.Label label4;
    }
}

