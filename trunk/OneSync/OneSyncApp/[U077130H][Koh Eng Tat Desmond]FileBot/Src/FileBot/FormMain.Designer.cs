namespace CodeDroids.FileBot
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.btnBrowse = new System.Windows.Forms.Button();
            this.grp1_1 = new System.Windows.Forms.GroupBox();
            this.nudMaxSubDirs = new System.Windows.Forms.NumericUpDown();
            this.nudDepth = new System.Windows.Forms.NumericUpDown();
            this.lbl2 = new System.Windows.Forms.Label();
            this.lbl1 = new System.Windows.Forms.Label();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.optUnicode = new System.Windows.Forms.RadioButton();
            this.optLettersSymbols1 = new System.Windows.Forms.RadioButton();
            this.optLetters1 = new System.Windows.Forms.RadioButton();
            this.nudMaxSize = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.nudMinSize = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.nudAveFiles = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.lblRoot = new System.Windows.Forms.Label();
            this.fbd = new System.Windows.Forms.FolderBrowserDialog();
            this.grp2_1 = new System.Windows.Forms.GroupBox();
            this.txtLastMod = new System.Windows.Forms.TextBox();
            this.chkLastModified = new System.Windows.Forms.CheckBox();
            this.btnModify = new System.Windows.Forms.Button();
            this.chkSystem = new System.Windows.Forms.CheckBox();
            this.chkReadOnly = new System.Windows.Forms.CheckBox();
            this.chkDelete = new System.Windows.Forms.CheckBox();
            this.btnResetAttr = new System.Windows.Forms.Button();
            this.nudProb = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tab1 = new System.Windows.Forms.TabPage();
            this.grp1_3 = new System.Windows.Forms.GroupBox();
            this.grp1_2 = new System.Windows.Forms.GroupBox();
            this.txtCreateDate = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.tab2 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label12 = new System.Windows.Forms.Label();
            this.optLetters3 = new System.Windows.Forms.RadioButton();
            this.optUnicode3 = new System.Windows.Forms.RadioButton();
            this.optLettersSymbols3 = new System.Windows.Forms.RadioButton();
            this.btnAdd = new System.Windows.Forms.Button();
            this.txtCreationDate2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.nudAveFiles2 = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.nudMinSize2 = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.nudMaxSize2 = new System.Windows.Forms.NumericUpDown();
            this.tab3 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.grp3_1 = new System.Windows.Forms.GroupBox();
            this.optLetters2 = new System.Windows.Forms.RadioButton();
            this.optUnicode2 = new System.Windows.Forms.RadioButton();
            this.optLettersSymbols2 = new System.Windows.Forms.RadioButton();
            this.chkRandomize = new System.Windows.Forms.CheckBox();
            this.btnRestore = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.ofd = new System.Windows.Forms.OpenFileDialog();
            this.sfd = new System.Windows.Forms.SaveFileDialog();
            this.txtRootDir = new System.Windows.Forms.TextBox();
            this.pb = new System.Windows.Forms.ProgressBar();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statlbl = new System.Windows.Forms.ToolStripStatusLabel();
            this.grp1_1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxSubDirs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDepth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAveFiles)).BeginInit();
            this.grp2_1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudProb)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tab1.SuspendLayout();
            this.grp1_3.SuspendLayout();
            this.grp1_2.SuspendLayout();
            this.tab2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudAveFiles2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinSize2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxSize2)).BeginInit();
            this.tab3.SuspendLayout();
            this.grp3_1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(414, 10);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // grp1_1
            // 
            this.grp1_1.Controls.Add(this.nudMaxSubDirs);
            this.grp1_1.Controls.Add(this.nudDepth);
            this.grp1_1.Controls.Add(this.lbl2);
            this.grp1_1.Controls.Add(this.lbl1);
            this.grp1_1.Location = new System.Drawing.Point(6, 6);
            this.grp1_1.Name = "grp1_1";
            this.grp1_1.Size = new System.Drawing.Size(457, 56);
            this.grp1_1.TabIndex = 0;
            this.grp1_1.TabStop = false;
            this.grp1_1.Text = "Directories";
            // 
            // nudMaxSubDirs
            // 
            this.nudMaxSubDirs.Location = new System.Drawing.Point(383, 19);
            this.nudMaxSubDirs.Name = "nudMaxSubDirs";
            this.nudMaxSubDirs.Size = new System.Drawing.Size(53, 20);
            this.nudMaxSubDirs.TabIndex = 3;
            this.nudMaxSubDirs.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // nudDepth
            // 
            this.nudDepth.Location = new System.Drawing.Point(96, 19);
            this.nudDepth.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudDepth.Name = "nudDepth";
            this.nudDepth.Size = new System.Drawing.Size(53, 20);
            this.nudDepth.TabIndex = 1;
            this.nudDepth.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // lbl2
            // 
            this.lbl2.AutoSize = true;
            this.lbl2.Location = new System.Drawing.Point(164, 21);
            this.lbl2.Name = "lbl2";
            this.lbl2.Size = new System.Drawing.Size(213, 13);
            this.lbl2.TabIndex = 2;
            this.lbl2.Text = "Maximum sub-directories (within a directory):";
            // 
            // lbl1
            // 
            this.lbl1.AutoSize = true;
            this.lbl1.Location = new System.Drawing.Point(6, 21);
            this.lbl1.Name = "lbl1";
            this.lbl1.Size = new System.Drawing.Size(84, 13);
            this.lbl1.TabIndex = 0;
            this.lbl1.Text = "Directory Depth:";
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(366, 211);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(97, 38);
            this.btnGenerate.TabIndex = 3;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // optUnicode
            // 
            this.optUnicode.AutoSize = true;
            this.optUnicode.Location = new System.Drawing.Point(176, 19);
            this.optUnicode.Name = "optUnicode";
            this.optUnicode.Size = new System.Drawing.Size(119, 17);
            this.optUnicode.TabIndex = 2;
            this.optUnicode.Text = "Unicode Characters";
            this.optUnicode.UseVisualStyleBackColor = true;
            // 
            // optLettersSymbols1
            // 
            this.optLettersSymbols1.AutoSize = true;
            this.optLettersSymbols1.Location = new System.Drawing.Point(69, 19);
            this.optLettersSymbols1.Name = "optLettersSymbols1";
            this.optLettersSymbols1.Size = new System.Drawing.Size(101, 17);
            this.optLettersSymbols1.TabIndex = 1;
            this.optLettersSymbols1.Text = "Letters/Symbols";
            this.optLettersSymbols1.UseVisualStyleBackColor = true;
            // 
            // optLetters1
            // 
            this.optLetters1.AutoSize = true;
            this.optLetters1.Checked = true;
            this.optLetters1.Location = new System.Drawing.Point(6, 19);
            this.optLetters1.Name = "optLetters1";
            this.optLetters1.Size = new System.Drawing.Size(57, 17);
            this.optLetters1.TabIndex = 0;
            this.optLetters1.TabStop = true;
            this.optLetters1.Text = "Letters";
            this.optLetters1.UseVisualStyleBackColor = true;
            // 
            // nudMaxSize
            // 
            this.nudMaxSize.Location = new System.Drawing.Point(264, 54);
            this.nudMaxSize.Maximum = new decimal(new int[] {
            8000000,
            0,
            0,
            0});
            this.nudMaxSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMaxSize.Name = "nudMaxSize";
            this.nudMaxSize.Size = new System.Drawing.Size(53, 20);
            this.nudMaxSize.TabIndex = 0;
            this.nudMaxSize.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(163, 56);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(95, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Max File Size (KB):";
            // 
            // nudMinSize
            // 
            this.nudMinSize.Location = new System.Drawing.Point(104, 54);
            this.nudMinSize.Maximum = new decimal(new int[] {
            8000000,
            0,
            0,
            0});
            this.nudMinSize.Name = "nudMinSize";
            this.nudMinSize.Size = new System.Drawing.Size(53, 20);
            this.nudMinSize.TabIndex = 6;
            this.nudMinSize.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Min File Size (KB):";
            // 
            // nudAveFiles
            // 
            this.nudAveFiles.Location = new System.Drawing.Point(171, 25);
            this.nudAveFiles.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudAveFiles.Name = "nudAveFiles";
            this.nudAveFiles.Size = new System.Drawing.Size(53, 20);
            this.nudAveFiles.TabIndex = 1;
            this.nudAveFiles.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(159, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Average files (within a directory):";
            // 
            // lblRoot
            // 
            this.lblRoot.AutoSize = true;
            this.lblRoot.Location = new System.Drawing.Point(9, 15);
            this.lblRoot.Name = "lblRoot";
            this.lblRoot.Size = new System.Drawing.Size(78, 13);
            this.lblRoot.TabIndex = 0;
            this.lblRoot.Text = "Root Directory:";
            // 
            // grp2_1
            // 
            this.grp2_1.Controls.Add(this.txtLastMod);
            this.grp2_1.Controls.Add(this.chkLastModified);
            this.grp2_1.Controls.Add(this.btnModify);
            this.grp2_1.Controls.Add(this.label8);
            this.grp2_1.Controls.Add(this.label9);
            this.grp2_1.Controls.Add(this.chkSystem);
            this.grp2_1.Controls.Add(this.nudProb);
            this.grp2_1.Controls.Add(this.chkReadOnly);
            this.grp2_1.Controls.Add(this.chkDelete);
            this.grp2_1.Location = new System.Drawing.Point(6, 6);
            this.grp2_1.Name = "grp2_1";
            this.grp2_1.Size = new System.Drawing.Size(457, 81);
            this.grp2_1.TabIndex = 3;
            this.grp2_1.TabStop = false;
            this.grp2_1.Text = "Actions";
            // 
            // txtLastMod
            // 
            this.txtLastMod.Enabled = false;
            this.txtLastMod.Location = new System.Drawing.Point(170, 48);
            this.txtLastMod.Name = "txtLastMod";
            this.txtLastMod.Size = new System.Drawing.Size(146, 20);
            this.txtLastMod.TabIndex = 4;
            // 
            // chkLastModified
            // 
            this.chkLastModified.AutoSize = true;
            this.chkLastModified.Location = new System.Drawing.Point(6, 50);
            this.chkLastModified.Name = "chkLastModified";
            this.chkLastModified.Size = new System.Drawing.Size(158, 17);
            this.chkLastModified.TabIndex = 3;
            this.chkLastModified.Text = "Change Last Modified Time:";
            this.chkLastModified.UseVisualStyleBackColor = true;
            this.chkLastModified.CheckedChanged += new System.EventHandler(this.chkLastModified_CheckedChanged);
            // 
            // btnModify
            // 
            this.btnModify.Location = new System.Drawing.Point(333, 44);
            this.btnModify.Name = "btnModify";
            this.btnModify.Size = new System.Drawing.Size(118, 27);
            this.btnModify.TabIndex = 4;
            this.btnModify.Text = "Modify";
            this.btnModify.UseVisualStyleBackColor = true;
            this.btnModify.Click += new System.EventHandler(this.btnModify_Click);
            // 
            // chkSystem
            // 
            this.chkSystem.AutoSize = true;
            this.chkSystem.Location = new System.Drawing.Point(203, 19);
            this.chkSystem.Name = "chkSystem";
            this.chkSystem.Size = new System.Drawing.Size(112, 17);
            this.chkSystem.TabIndex = 2;
            this.chkSystem.Text = "Set as System File";
            this.chkSystem.UseVisualStyleBackColor = true;
            // 
            // chkReadOnly
            // 
            this.chkReadOnly.AutoSize = true;
            this.chkReadOnly.Location = new System.Drawing.Point(88, 19);
            this.chkReadOnly.Name = "chkReadOnly";
            this.chkReadOnly.Size = new System.Drawing.Size(109, 17);
            this.chkReadOnly.TabIndex = 1;
            this.chkReadOnly.Text = "Set as Read-Only";
            this.chkReadOnly.UseVisualStyleBackColor = true;
            // 
            // chkDelete
            // 
            this.chkDelete.AutoSize = true;
            this.chkDelete.Location = new System.Drawing.Point(6, 19);
            this.chkDelete.Name = "chkDelete";
            this.chkDelete.Size = new System.Drawing.Size(76, 17);
            this.chkDelete.TabIndex = 0;
            this.chkDelete.Text = "Delete File";
            this.chkDelete.UseVisualStyleBackColor = true;
            // 
            // btnResetAttr
            // 
            this.btnResetAttr.Location = new System.Drawing.Point(339, 218);
            this.btnResetAttr.Name = "btnResetAttr";
            this.btnResetAttr.Size = new System.Drawing.Size(118, 27);
            this.btnResetAttr.TabIndex = 5;
            this.btnResetAttr.Text = "Reset file attributes";
            this.btnResetAttr.UseVisualStyleBackColor = true;
            this.btnResetAttr.Click += new System.EventHandler(this.btnResetAttr_Click);
            // 
            // nudProb
            // 
            this.nudProb.Location = new System.Drawing.Point(392, 18);
            this.nudProb.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudProb.Name = "nudProb";
            this.nudProb.Size = new System.Drawing.Size(45, 20);
            this.nudProb.TabIndex = 1;
            this.nudProb.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(328, 20);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(58, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "Probability:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(437, 20);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(15, 13);
            this.label9.TabIndex = 2;
            this.label9.Text = "%";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tab1);
            this.tabControl1.Controls.Add(this.tab2);
            this.tabControl1.Controls.Add(this.tab3);
            this.tabControl1.Location = new System.Drawing.Point(12, 39);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(477, 281);
            this.tabControl1.TabIndex = 3;
            // 
            // tab1
            // 
            this.tab1.Controls.Add(this.grp1_3);
            this.tab1.Controls.Add(this.btnGenerate);
            this.tab1.Controls.Add(this.grp1_2);
            this.tab1.Controls.Add(this.grp1_1);
            this.tab1.Location = new System.Drawing.Point(4, 22);
            this.tab1.Name = "tab1";
            this.tab1.Padding = new System.Windows.Forms.Padding(3);
            this.tab1.Size = new System.Drawing.Size(469, 255);
            this.tab1.TabIndex = 0;
            this.tab1.Text = "Generate";
            this.tab1.UseVisualStyleBackColor = true;
            // 
            // grp1_3
            // 
            this.grp1_3.Controls.Add(this.optLetters1);
            this.grp1_3.Controls.Add(this.optUnicode);
            this.grp1_3.Controls.Add(this.optLettersSymbols1);
            this.grp1_3.Location = new System.Drawing.Point(6, 161);
            this.grp1_3.Name = "grp1_3";
            this.grp1_3.Size = new System.Drawing.Size(457, 44);
            this.grp1_3.TabIndex = 2;
            this.grp1_3.TabStop = false;
            this.grp1_3.Text = "Directory/File Naming";
            // 
            // grp1_2
            // 
            this.grp1_2.Controls.Add(this.txtCreateDate);
            this.grp1_2.Controls.Add(this.label11);
            this.grp1_2.Controls.Add(this.label3);
            this.grp1_2.Controls.Add(this.nudAveFiles);
            this.grp1_2.Controls.Add(this.label4);
            this.grp1_2.Controls.Add(this.nudMinSize);
            this.grp1_2.Controls.Add(this.label5);
            this.grp1_2.Controls.Add(this.nudMaxSize);
            this.grp1_2.Location = new System.Drawing.Point(6, 68);
            this.grp1_2.Name = "grp1_2";
            this.grp1_2.Size = new System.Drawing.Size(457, 87);
            this.grp1_2.TabIndex = 1;
            this.grp1_2.TabStop = false;
            this.grp1_2.Text = "Files";
            // 
            // txtCreateDate
            // 
            this.txtCreateDate.Location = new System.Drawing.Point(325, 24);
            this.txtCreateDate.Name = "txtCreateDate";
            this.txtCreateDate.Size = new System.Drawing.Size(122, 20);
            this.txtCreateDate.TabIndex = 4;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(244, 27);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(75, 13);
            this.label11.TabIndex = 3;
            this.label11.Text = "Creation Date:";
            // 
            // tab2
            // 
            this.tab2.Controls.Add(this.groupBox1);
            this.tab2.Controls.Add(this.btnResetAttr);
            this.tab2.Controls.Add(this.grp2_1);
            this.tab2.Location = new System.Drawing.Point(4, 22);
            this.tab2.Name = "tab2";
            this.tab2.Padding = new System.Windows.Forms.Padding(3);
            this.tab2.Size = new System.Drawing.Size(469, 255);
            this.tab2.TabIndex = 1;
            this.tab2.Text = "Modify";
            this.tab2.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.optLetters3);
            this.groupBox1.Controls.Add(this.optUnicode3);
            this.groupBox1.Controls.Add(this.optLettersSymbols3);
            this.groupBox1.Controls.Add(this.btnAdd);
            this.groupBox1.Controls.Add(this.txtCreationDate2);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.nudAveFiles2);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.nudMinSize2);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.nudMaxSize2);
            this.groupBox1.Location = new System.Drawing.Point(6, 93);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(457, 90);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Add Files";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 71);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(52, 13);
            this.label12.TabIndex = 20;
            this.label12.Text = "Filename:";
            // 
            // optLetters3
            // 
            this.optLetters3.AutoSize = true;
            this.optLetters3.Checked = true;
            this.optLetters3.Location = new System.Drawing.Point(64, 69);
            this.optLetters3.Name = "optLetters3";
            this.optLetters3.Size = new System.Drawing.Size(57, 17);
            this.optLetters3.TabIndex = 17;
            this.optLetters3.TabStop = true;
            this.optLetters3.Text = "Letters";
            this.optLetters3.UseVisualStyleBackColor = true;
            // 
            // optUnicode3
            // 
            this.optUnicode3.AutoSize = true;
            this.optUnicode3.Location = new System.Drawing.Point(234, 69);
            this.optUnicode3.Name = "optUnicode3";
            this.optUnicode3.Size = new System.Drawing.Size(65, 17);
            this.optUnicode3.TabIndex = 19;
            this.optUnicode3.Text = "Unicode";
            this.optUnicode3.UseVisualStyleBackColor = true;
            // 
            // optLettersSymbols3
            // 
            this.optLettersSymbols3.AutoSize = true;
            this.optLettersSymbols3.Location = new System.Drawing.Point(127, 69);
            this.optLettersSymbols3.Name = "optLettersSymbols3";
            this.optLettersSymbols3.Size = new System.Drawing.Size(101, 17);
            this.optLettersSymbols3.TabIndex = 18;
            this.optLettersSymbols3.Text = "Letters/Symbols";
            this.optLettersSymbols3.UseVisualStyleBackColor = true;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(333, 57);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(118, 27);
            this.btnAdd.TabIndex = 16;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // txtCreationDate2
            // 
            this.txtCreationDate2.Location = new System.Drawing.Point(325, 13);
            this.txtCreationDate2.Name = "txtCreationDate2";
            this.txtCreationDate2.Size = new System.Drawing.Size(122, 20);
            this.txtCreationDate2.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(244, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Creation Date:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 16);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(159, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Average files (within a directory):";
            // 
            // nudAveFiles2
            // 
            this.nudAveFiles2.Location = new System.Drawing.Point(171, 14);
            this.nudAveFiles2.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudAveFiles2.Name = "nudAveFiles2";
            this.nudAveFiles2.Size = new System.Drawing.Size(53, 20);
            this.nudAveFiles2.TabIndex = 10;
            this.nudAveFiles2.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 45);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(92, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Min File Size (KB):";
            // 
            // nudMinSize2
            // 
            this.nudMinSize2.Location = new System.Drawing.Point(104, 43);
            this.nudMinSize2.Maximum = new decimal(new int[] {
            8000000,
            0,
            0,
            0});
            this.nudMinSize2.Name = "nudMinSize2";
            this.nudMinSize2.Size = new System.Drawing.Size(53, 20);
            this.nudMinSize2.TabIndex = 14;
            this.nudMinSize2.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(163, 45);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(95, 13);
            this.label10.TabIndex = 15;
            this.label10.Text = "Max File Size (KB):";
            // 
            // nudMaxSize2
            // 
            this.nudMaxSize2.Location = new System.Drawing.Point(264, 43);
            this.nudMaxSize2.Maximum = new decimal(new int[] {
            8000000,
            0,
            0,
            0});
            this.nudMaxSize2.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMaxSize2.Name = "nudMaxSize2";
            this.nudMaxSize2.Size = new System.Drawing.Size(53, 20);
            this.nudMaxSize2.TabIndex = 9;
            this.nudMaxSize2.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // tab3
            // 
            this.tab3.Controls.Add(this.label1);
            this.tab3.Controls.Add(this.grp3_1);
            this.tab3.Controls.Add(this.chkRandomize);
            this.tab3.Controls.Add(this.btnRestore);
            this.tab3.Controls.Add(this.btnSave);
            this.tab3.Location = new System.Drawing.Point(4, 22);
            this.tab3.Name = "tab3";
            this.tab3.Padding = new System.Windows.Forms.Padding(3);
            this.tab3.Size = new System.Drawing.Size(469, 255);
            this.tab3.TabIndex = 2;
            this.tab3.Text = "Save/Restore";
            this.tab3.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(453, 39);
            this.label1.TabIndex = 4;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // grp3_1
            // 
            this.grp3_1.Controls.Add(this.optLetters2);
            this.grp3_1.Controls.Add(this.optUnicode2);
            this.grp3_1.Controls.Add(this.optLettersSymbols2);
            this.grp3_1.Location = new System.Drawing.Point(6, 190);
            this.grp3_1.Name = "grp3_1";
            this.grp3_1.Size = new System.Drawing.Size(457, 44);
            this.grp3_1.TabIndex = 3;
            this.grp3_1.TabStop = false;
            this.grp3_1.Text = "Directory/File Naming";
            this.grp3_1.Visible = false;
            // 
            // optLetters2
            // 
            this.optLetters2.AutoSize = true;
            this.optLetters2.Checked = true;
            this.optLetters2.Location = new System.Drawing.Point(6, 19);
            this.optLetters2.Name = "optLetters2";
            this.optLetters2.Size = new System.Drawing.Size(57, 17);
            this.optLetters2.TabIndex = 0;
            this.optLetters2.TabStop = true;
            this.optLetters2.Text = "Letters";
            this.optLetters2.UseVisualStyleBackColor = true;
            // 
            // optUnicode2
            // 
            this.optUnicode2.AutoSize = true;
            this.optUnicode2.Location = new System.Drawing.Point(176, 19);
            this.optUnicode2.Name = "optUnicode2";
            this.optUnicode2.Size = new System.Drawing.Size(119, 17);
            this.optUnicode2.TabIndex = 2;
            this.optUnicode2.Text = "Unicode Characters";
            this.optUnicode2.UseVisualStyleBackColor = true;
            // 
            // optLettersSymbols2
            // 
            this.optLettersSymbols2.AutoSize = true;
            this.optLettersSymbols2.Location = new System.Drawing.Point(69, 19);
            this.optLettersSymbols2.Name = "optLettersSymbols2";
            this.optLettersSymbols2.Size = new System.Drawing.Size(101, 17);
            this.optLettersSymbols2.TabIndex = 1;
            this.optLettersSymbols2.Text = "Letters/Symbols";
            this.optLettersSymbols2.UseVisualStyleBackColor = true;
            // 
            // chkRandomize
            // 
            this.chkRandomize.AutoSize = true;
            this.chkRandomize.Location = new System.Drawing.Point(14, 167);
            this.chkRandomize.Name = "chkRandomize";
            this.chkRandomize.Size = new System.Drawing.Size(264, 17);
            this.chkRandomize.TabIndex = 2;
            this.chkRandomize.Text = "Randomize directory name and filename on restore";
            this.chkRandomize.UseVisualStyleBackColor = true;
            this.chkRandomize.CheckedChanged += new System.EventHandler(this.chkRandomize_CheckedChanged);
            // 
            // btnRestore
            // 
            this.btnRestore.Location = new System.Drawing.Point(6, 107);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(457, 42);
            this.btnRestore.TabIndex = 1;
            this.btnRestore.Text = "Restore Directory Structure";
            this.btnRestore.UseVisualStyleBackColor = true;
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(6, 59);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(457, 42);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save Directory Structure";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // ofd
            // 
            this.ofd.Filter = "BIN files|*.bin";
            // 
            // sfd
            // 
            this.sfd.Filter = "BIN files|*.bin";
            // 
            // txtRootDir
            // 
            this.txtRootDir.Location = new System.Drawing.Point(93, 12);
            this.txtRootDir.Name = "txtRootDir";
            this.txtRootDir.Size = new System.Drawing.Size(315, 20);
            this.txtRootDir.TabIndex = 1;
            this.txtRootDir.Text = global::CodeDroids.FileBot.Properties.Settings.Default.RootDir;
            // 
            // pb
            // 
            this.pb.Location = new System.Drawing.Point(12, 322);
            this.pb.Name = "pb";
            this.pb.Size = new System.Drawing.Size(477, 10);
            this.pb.TabIndex = 5;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statlbl});
            this.statusStrip1.Location = new System.Drawing.Point(0, 350);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(503, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statStrip";
            // 
            // statlbl
            // 
            this.statlbl.AutoSize = false;
            this.statlbl.Name = "statlbl";
            this.statlbl.Size = new System.Drawing.Size(500, 17);
            this.statlbl.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(503, 372);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.pb);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.txtRootDir);
            this.Controls.Add(this.lblRoot);
            this.Controls.Add(this.btnBrowse);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.Text = "FileBot";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.grp1_1.ResumeLayout(false);
            this.grp1_1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxSubDirs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDepth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAveFiles)).EndInit();
            this.grp2_1.ResumeLayout(false);
            this.grp2_1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudProb)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tab1.ResumeLayout(false);
            this.grp1_3.ResumeLayout(false);
            this.grp1_3.PerformLayout();
            this.grp1_2.ResumeLayout(false);
            this.grp1_2.PerformLayout();
            this.tab2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudAveFiles2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinSize2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxSize2)).EndInit();
            this.tab3.ResumeLayout(false);
            this.tab3.PerformLayout();
            this.grp3_1.ResumeLayout(false);
            this.grp3_1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.GroupBox grp1_1;
        private System.Windows.Forms.NumericUpDown nudDepth;
        private System.Windows.Forms.Label lbl1;
        private System.Windows.Forms.NumericUpDown nudMaxSubDirs;
        private System.Windows.Forms.Label lbl2;
        private System.Windows.Forms.NumericUpDown nudAveFiles;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton optUnicode;
        private System.Windows.Forms.RadioButton optLettersSymbols1;
        private System.Windows.Forms.RadioButton optLetters1;
        private System.Windows.Forms.NumericUpDown nudMaxSize;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nudMinSize;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblRoot;
        private System.Windows.Forms.TextBox txtRootDir;
        private System.Windows.Forms.FolderBrowserDialog fbd;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.GroupBox grp2_1;
        private System.Windows.Forms.NumericUpDown nudProb;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtLastMod;
        private System.Windows.Forms.CheckBox chkLastModified;
        private System.Windows.Forms.CheckBox chkSystem;
        private System.Windows.Forms.CheckBox chkReadOnly;
        private System.Windows.Forms.CheckBox chkDelete;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tab1;
        private System.Windows.Forms.GroupBox grp1_2;
        private System.Windows.Forms.TextBox txtCreateDate;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TabPage tab2;
        private System.Windows.Forms.Button btnModify;
        private System.Windows.Forms.TabPage tab3;
        private System.Windows.Forms.OpenFileDialog ofd;
        private System.Windows.Forms.SaveFileDialog sfd;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.GroupBox grp1_3;
        private System.Windows.Forms.GroupBox grp3_1;
        private System.Windows.Forms.RadioButton optLetters2;
        private System.Windows.Forms.RadioButton optUnicode2;
        private System.Windows.Forms.RadioButton optLettersSymbols2;
        private System.Windows.Forms.CheckBox chkRandomize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnResetAttr;
        private System.Windows.Forms.ProgressBar pb;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statlbl;
        private System.Windows.Forms.TextBox txtCreationDate2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown nudAveFiles2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown nudMinSize2;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown nudMaxSize2;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.RadioButton optLetters3;
        private System.Windows.Forms.RadioButton optUnicode3;
        private System.Windows.Forms.RadioButton optLettersSymbols3;
    }
}

