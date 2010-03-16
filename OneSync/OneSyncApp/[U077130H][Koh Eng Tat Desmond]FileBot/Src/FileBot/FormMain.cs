using System;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.IO;


namespace CodeDroids.FileBot
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            //Initialize.
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            // First-run
            if (txtRootDir.Text == "")
                txtRootDir.Text = Application.StartupPath;

            DirectoryNode.Update += UpdateInfo;

            txtLastMod.Text = DateTime.Now.ToString();
            txtCreateDate.Text = DateTime.Now.ToString();
            txtCreationDate2.Text = DateTime.Now.ToString();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (fbd.ShowDialog() == DialogResult.OK)
                txtRootDir.Text = fbd.SelectedPath;
        }


        #region Generate Tab

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            RandomGenerator.NameType type;

            if (optLetters1.Checked)
                type = RandomGenerator.NameType.Letters;
            else if
                (optLettersSymbols1.Checked) type = RandomGenerator.NameType.LettersAndSymbols;
            else
                type = RandomGenerator.NameType.Unicode;

            Bot bot = new Bot(txtRootDir.Text, DateTime.Parse(txtCreateDate.Text), type);
            bot.Update += UpdateInfo;

            DoWorkAsync((MethodInvoker)delegate
            {
                bot.CreateDirectoryStructure((int)nudDepth.Value, (int)nudMaxSubDirs.Value);
                bot.DropFiles((int)nudAveFiles.Value, (int)nudMinSize.Value, (int)nudMaxSize.Value);
                bot.Update -= UpdateInfo;
            });
        }

        #endregion

        #region Modify Tab

        private void btnModify_Click(object sender, EventArgs e)
        {
            DateTime? modTime = null;
            if (chkLastModified.Checked) modTime = DateTime.Parse(txtLastMod.Text);


            DoWorkAsync((MethodInvoker)delegate
            {
                Bot b = new Bot(txtRootDir.Text);
                b.Update += UpdateInfo;

                b.ModifyFiles(chkDelete.Checked,
                                chkReadOnly.Checked,
                                chkSystem.Checked, modTime, (int)nudProb.Value);
            });
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Get type of filename selected by user
            RandomGenerator.NameType type = RandomGenerator.NameType.Letters;

            if (optLettersSymbols3.Checked)
                type = RandomGenerator.NameType.LettersAndSymbols;
            else if (optUnicode3.Checked)
                type = RandomGenerator.NameType.Unicode;

            DoWorkAsync((MethodInvoker)delegate
            {                
                Bot b = new Bot(txtRootDir.Text, DateTime.Parse(txtCreationDate2.Text), type);
                b.DropFiles((int)nudAveFiles2.Value, (int)nudMinSize2.Value, (int)nudMaxSize2.Value);
            });
        }

        private void btnResetAttr_Click(object sender, EventArgs e)
        {
            DateTime? modTime = null;
            if (chkLastModified.Checked) modTime = DateTime.Parse(txtLastMod.Text);

            Bot b = new Bot(txtRootDir.Text);
            b.Update += UpdateInfo;

            b.ResetFiles(DateTime.Now);
            MessageBox.Show("Done.");
        }

        private void chkLastModified_CheckedChanged(object sender, EventArgs e)
        {
            txtLastMod.Enabled = chkLastModified.Checked;
        }

        #endregion

        #region Save/Restore Tab

        private void chkRandomize_CheckedChanged(object sender, EventArgs e)
        {
            grp3_1.Visible = chkRandomize.Checked;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (fbd.SelectedPath == "")
                fbd.SelectedPath = txtRootDir.Text;

            if (fbd.ShowDialog() == DialogResult.OK && sfd.ShowDialog() == DialogResult.OK)
            {
                DoWorkAsync((MethodInvoker)delegate
                {
                    DirectoryNode root = new DirectoryNode("root", DateTime.Now);
                    DirectoryNode.Update += UpdateInfo;
                    root = DirectoryNode.SaveStructure(fbd.SelectedPath, root);
                    Utility.Serialize(root, sfd.FileName);
                });
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DirectoryNode root = (DirectoryNode)Utility.Deserialize(ofd.FileName);

                    if (fbd.ShowDialog() == DialogResult.OK &&
                        MessageBox.Show("Do you want to restore directory structure to " + txtRootDir.Text + " ?",
                                        "FileBot", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        RandomGenerator.NameType type = RandomGenerator.NameType.Letters;

                        if (optLettersSymbols2.Checked)
                            type = RandomGenerator.NameType.LettersAndSymbols;
                        else if (optUnicode2.Checked)
                            type = RandomGenerator.NameType.Unicode;

                        DoWorkAsync((MethodInvoker)delegate
                        {
                            DirectoryInfo destDir = new DirectoryInfo(fbd.SelectedPath);
                            DirectoryNode.RestoreStructure(destDir, root, chkRandomize.Checked, type);
                        });
                    }

                }
                catch (SerializationException)
                {
                    MessageBox.Show("Unable to load file. Ensure the correct file is selected.");
                }
                
            }
        }

        #endregion

        private void DoWorkAsync(MethodInvoker d)
        {
            pb.Value = 0;
            pb.Style = ProgressBarStyle.Marquee;
            statlbl.Text = "";
            this.Enabled = false;

            // Invoke method asynchronously
            IAsyncResult result = d.BeginInvoke(WorkDone, d);
        }

        private void WorkDone(IAsyncResult ar)
        {
            // Get the MethodInvoker delegate that is completed
            MethodInvoker d = (MethodInvoker)ar.AsyncState;

            try
            {
                // Any exception will be thrown here
                d.EndInvoke(ar);

                // Update UI on UI thread as this method is on non-UI thread
                this.Invoke((MethodInvoker)delegate
                {
                    // Reset progress bar
                    pb.Value = 0;
                    pb.Style = ProgressBarStyle.Continuous;
                    this.Enabled = true;
                    statlbl.Text = "Done.";
                });
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show(string.Format("An error has occurred:\n{0}", ex.Message));
                });
            }
        }

        private void UpdateInfo(object sender, UpdateEventArgs e)
        {
            // Called from non-UI thread
            if (this.InvokeRequired)
                this.Invoke((MethodInvoker)delegate { UpdateInfo(sender, e); });
            else
            {
                if (e.Message != null)
                    statlbl.Text = e.Message;
                    //txtLog.AppendText(e.Message + "\r\n");

                if (e.Progress == 0)
                    pb.Style = ProgressBarStyle.Marquee;
                else if (e.Progress > 0 && e.Progress <= pb.Maximum)
                {
                    pb.Style = ProgressBarStyle.Continuous;
                    pb.Value = e.Progress;
                }
            }
        }

    }
}
