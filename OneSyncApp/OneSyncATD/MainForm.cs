using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace OneSyncATD
{
    public partial class MainForm : Form
    {
        String testPath;
        String testFolderPath;
        String testRightFolder;
        String testLeftFolder;
        String testInterFolder;

        public MainForm()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            TestCaseReading readCases = new TestCaseReading();
            List<TestCase> listTestCases = readCases.readTestCases(testPath);
            TestExecution execTests = new TestExecution(testRightFolder, testInterFolder, testLeftFolder);
            execTests.executeTests(listTestCases);
            TestResultWriting writeResults = new TestResultWriting();
            writeResults.writeResult(testFolderPath, listTestCases);
            MessageBox.Show("Test Cases finish running", "OneSync", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); 
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                filePath.Text = openFile.FileName;
                testPath = openFile.FileName;
                testFolderPath = Path.GetDirectoryName(testPath);
            }
        }

        private void filePath_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                filePath.Text = openFile.FileName;
                testPath = openFile.FileName;
                testFolderPath = Path.GetDirectoryName(testPath);
            }
        }

        private void rightFolderButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog openFolder = new FolderBrowserDialog();
            if (openFolder.ShowDialog() == DialogResult.OK)
            {
                rightFolderText.Text = openFolder.SelectedPath;
                testRightFolder = openFolder.SelectedPath;
            }
        }

        private void leftFolderButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog openFolder = new FolderBrowserDialog();
            if (openFolder.ShowDialog() == DialogResult.OK)
            {
                leftFolderText.Text = openFolder.SelectedPath;
                testLeftFolder = openFolder.SelectedPath;
            }
        }

        private void rightFolderText_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog openFolder = new FolderBrowserDialog();
            if (openFolder.ShowDialog() == DialogResult.OK)
            {
                rightFolderText.Text = openFolder.SelectedPath;
                testRightFolder = openFolder.SelectedPath;
            }
        }

        private void leftFolderText_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog openFolder = new FolderBrowserDialog();
            if (openFolder.ShowDialog() == DialogResult.OK)
            {
                leftFolderText.Text = openFolder.SelectedPath;
                testLeftFolder = openFolder.SelectedPath;
            }
        }

        private void interButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog openFolder = new FolderBrowserDialog();
            if (openFolder.ShowDialog() == DialogResult.OK)
            {
                interFolderText.Text = openFolder.SelectedPath;
                testInterFolder = openFolder.SelectedPath;
            }
        }

        private void interFolderText_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog openFolder = new FolderBrowserDialog();
            if (openFolder.ShowDialog() == DialogResult.OK)
            {
                interFolderText.Text = openFolder.SelectedPath;
                testInterFolder = openFolder.SelectedPath;
            }
        }
    }
}
