using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OneSync.Synchronization;

namespace OneSyncATD
{
    class TestExecution
    {
        String leftFolder;
        String rightFolder;
        String interFolder;
        String listSFolders;
        String listTFolders;

        public TestExecution(String rightFolPath, String interFolPath, String leftFolPath)
        {
            rightFolder = rightFolPath;
            interFolder = interFolPath;
            leftFolder = leftFolPath;
        }

        public List<TestCase> executeTests(List<TestCase> listCases)
        {
            foreach (TestCase oneCase in listCases) 
            {
                try
                {
                    oneCase.testActual = "false";
                    runCommand(oneCase);
                    if (oneCase.testActual.Equals(oneCase.testExpected))
                    {
                        oneCase.testPassed = true;
                    }
                    else
                    {
                        oneCase.testComment = oneCase.testComment + "Source Folders: " + listSFolders + "Source Files: " + listSFolders + "Target Folders: " + listTFolders + "Target Files: " + listTFiles;
                    }
                }
                catch (Exception exception)
                {
                    //throw exception;
                    oneCase.testActual = "false";
                    oneCase.testComment = oneCase.testComment + "//" + exception.Message + "//";
                }
                finally
                {
                    if (oneCase.testActual.Equals(oneCase.testExpected))
                    {
                        oneCase.testPassed = true;
                    }
                    else
                    {
                        DirectoryInfo diRight = new DirectoryInfo(rightFolder);
                        DirectoryInfo diLeft = new DirectoryInfo(leftFolder);

                        String newRight = diRight.Parent.FullName + "\\right" + oneCase.testID;
                        Directory.CreateDirectory(newRight);
                        DirectoryInfo diNewRight = new DirectoryInfo(newRight);
                        copyAll(diRight, diNewRight);

                        String newLeft = diLeft.Parent.FullName + "\\left" + oneCase.testID;
                        Directory.CreateDirectory(newLeft);
                        DirectoryInfo diNewLeft = new DirectoryInfo(newLeft);
                        copyAll(diLeft, diNewLeft);
                    }
                }
            }            
            return listCases;
        }

        /**
         *createfolders : targetfolder, folnametype, folderdepth, maxsubdir
         *generatefiles : targetfolder, filenametype, numberoffiles, fileminsize, filemaxsize           
         **/

        private void runCommand(TestCase oneCase)
        {
            #region createfolders id:createfolders:right or left,unicode or letters or letterssymbols,folderdepth,maxsubdir:true or false:comment
            if (oneCase.testMethod.Equals("createfolders"))
            {
                String[] comParameters = oneCase.testParameters.Split(',');
                RandomGenerator.NameType nameType;
                nameType = detectNameType(comParameters[1]); //comParameters[0] is type of random folder names

                Bot fileBot = new Bot(enumFolder(comParameters[0]), DateTime.Now, nameType); 
                fileBot.CreateDirectoryStructure(int.Parse(comParameters[2]), int.Parse(comParameters[3])); //comParameters[1] is depth of folder while comParameters[2] is max sub folders

                //Check whether folders created successfully or not
                System.Diagnostics.Stopwatch sWatch = new System.Diagnostics.Stopwatch();
                sWatch.Start();

                string[] checkDirs = System.IO.Directory.GetDirectories(enumFolder(comParameters[0]));

                if (checkDirs.Length == 0 && int.Parse(comParameters[2]) > 0)
                {
                    oneCase.testActual = "false";
                }
                else
                {
                    oneCase.testActual = "true";
                }

                sWatch.Stop();
                #endregion
            #region generatefiles id:generatefiles:right or left,unicode or letters or letterssymbols,numberoffiles,fileminsize,filemaxsize:true or false:comment
            } else if (oneCase.testMethod.Equals("generatefiles"))
            {
                String[] comParameters = oneCase.testParameters.Split(',');
                RandomGenerator.NameType nameType;
                nameType = detectNameType(comParameters[1]); //comParameters[0] is type of random file names

                Bot fileBot = new Bot(enumFolder(comParameters[0]), DateTime.Now, nameType);
                fileBot.DropFiles(int.Parse(comParameters[2]), int.Parse(comParameters[3]), int.Parse(comParameters[4])); //comParameters[1] is number of files per folder, comParameters[2] is min size of a file, comParameters[3] is max size of a file

                System.Diagnostics.Stopwatch sWatch = new System.Diagnostics.Stopwatch();
                sWatch.Start();

                string[] checkFiles = System.IO.Directory.GetFiles(enumFolder(comParameters[0]));

                if (checkFiles.Length == 0 && System.IO.Directory.GetDirectories(enumFolder(comParameters[0])).Length == 0)
                {
                    oneCase.testActual = "false, no files is generated.";
                }
                else
                {
                    oneCase.testActual = "true";
                }

                sWatch.Stop();
            }
            #endregion
            #region clearall reset everything to initial
            else if (oneCase.testMethod.Equals("clearall"))
            {
                DirectoryInfo diRight = new DirectoryInfo(rightFolder);
                DirectoryInfo diInter = new DirectoryInfo(interFolder);
                DirectoryInfo diLeft = new DirectoryInfo(leftFolder);

                foreach (SyncJob jobItem in (SyncClient.GetSyncJobManager(System.Windows.Forms.Application.StartupPath).LoadAllJobs()))
                {
                    SyncClient.GetSyncJobManager(System.Windows.Forms.Application.StartupPath).Delete(jobItem);
                }

                foreach (FileInfo fileInf in diRight.GetFiles())
                {
                    fileInf.Delete();
                }
                foreach (DirectoryInfo subDirectory in diRight.GetDirectories())
                {
                    subDirectory.Delete(true);
                }
                foreach (FileInfo fileInf in diInter.GetFiles())
                {
                    fileInf.Delete();
                }
                foreach (DirectoryInfo subDirectory in diInter.GetDirectories())
                {
                    subDirectory.Delete(true);
                }
                foreach (FileInfo fileInf in diLeft.GetFiles())
                {
                    fileInf.Delete();
                }
                foreach (DirectoryInfo subDirectory in diLeft.GetDirectories())
                {
                    subDirectory.Delete(true);
                }
                System.Diagnostics.Stopwatch sWatch = new System.Diagnostics.Stopwatch();
                sWatch.Start();

                string[] checkFiles = System.IO.Directory.GetFiles(rightFolder);
                string[] checkDirs = System.IO.Directory.GetDirectories(rightFolder);

                if (checkDirs.Length == 0)
                {
                    oneCase.testActual = "true";
                }
                else
                {
                    oneCase.testActual = "false";
                }
                if (checkFiles.Length == 0)
                {
                    oneCase.testActual = "true";
                }
                else
                {
                    oneCase.testActual = "false";
                }

                checkFiles = System.IO.Directory.GetFiles(interFolder);
                checkDirs = System.IO.Directory.GetDirectories(interFolder);

                if (checkDirs.Length == 0)
                {
                    oneCase.testActual = "true";
                }
                else
                {
                    oneCase.testActual = "false";
                }
                if (checkFiles.Length == 0)
                {
                    oneCase.testActual = "true";
                }
                else
                {
                    oneCase.testActual = "false";
                }

                checkFiles = System.IO.Directory.GetFiles(leftFolder);
                checkDirs = System.IO.Directory.GetDirectories(leftFolder);

                if (checkDirs.Length == 0)
                {
                    oneCase.testActual = "true";
                }
                else
                {
                    oneCase.testActual = "false";
                }
                if (checkFiles.Length == 0)
                {
                    oneCase.testActual = "true";
                }
                else
                {
                    oneCase.testActual = "false";
                }

                sWatch.Stop();
            }
            #endregion
            #region createprofile id:createprofile:syncjob1,syncjob2:true or false:comment
            else if (oneCase.testMethod.Equals("createprofile")) 
            {
                String[] comParameters = oneCase.testParameters.Split(',');
                Boolean passRFlag = false;
                Boolean passLFlag = false;
                SyncJobManager jobManager = SyncClient.GetSyncJobManager(System.Windows.Forms.Application.StartupPath);

                SyncJob rightSyncjob = jobManager.CreateSyncJob(comParameters[0], rightFolder, interFolder);
                SyncJob leftSyncjob = jobManager.CreateSyncJob(comParameters[1], leftFolder, interFolder);

                foreach (SyncJob jobItem in jobManager.LoadAllJobs())
                {
                    if (jobItem.Name.Equals(comParameters[0]) && jobItem.SyncSource.Path.Equals(rightFolder)) 
                    {
                        passRFlag = true;
                    }
                    if (jobItem.Name.Equals(comParameters[1]) && jobItem.SyncSource.Path.Equals(leftFolder))
                    {
                        passLFlag = true;
                    }
                }

                if (passRFlag && passLFlag)
                {
                    oneCase.testActual = "true";
                }
                else
                {
                    oneCase.testActual = "false, Sync Jobs are not created successfully.";
                }
            }
            #endregion
            #region rightsyncfirst id:rightsyncfirst:syncjob1,syncjob2:true or false:comment
            else if (oneCase.testMethod.Equals("rightsyncfirst"))
            {
                String[] comParameters = oneCase.testParameters.Split(',');
                foreach (SyncJob jobItem in SyncClient.GetSyncJobManager(System.Windows.Forms.Application.StartupPath).LoadAllJobs())
                {
                    if (jobItem.Name.Equals(comParameters[0]) && jobItem.SyncSource.Path.Equals(rightFolder))
                    {
                        FileSyncAgent currentAgent = new OneSync.Synchronization.FileSyncAgent(jobItem);
                        SyncPreviewResult previewResult = currentAgent.GenerateSyncPreview(null);
                        currentAgent.Synchronize(previewResult);
                        //currentAgent.Synchronize(previewResult);
                        break;                        
                    }
                }

                foreach (SyncJob jobItem in SyncClient.GetSyncJobManager(System.Windows.Forms.Application.StartupPath).LoadAllJobs())
                {
                    if (jobItem.Name.Equals(comParameters[1]) && jobItem.SyncSource.Path.Equals(leftFolder))
                    {
                        FileSyncAgent currentAgent = new OneSync.Synchronization.FileSyncAgent(jobItem);
                        SyncPreviewResult previewResult = currentAgent.GenerateSyncPreview(null);
                        currentAgent.Synchronize(previewResult);
                        //SyncPreviewResult previewResult = currentAgent.PreviewSync();
                        //currentAgent.Synchronize(previewResult);
                        break;
                    }
                }

                DirectoryInfo diRight = new DirectoryInfo(rightFolder);
                DirectoryInfo diLeft = new DirectoryInfo(leftFolder);
                if (compareAll(diRight, diLeft))
                {
                    oneCase.testActual = "true";
                }
                else
                {
                    oneCase.testActual = "false";
                }
            }
            #endregion
            #region righthalfsync id:righthalfsync:syncjob:true or false:comment
            else if (oneCase.testMethod.Equals("righthalfsync"))
            {                
                foreach (SyncJob jobItem in SyncClient.GetSyncJobManager(System.Windows.Forms.Application.StartupPath).LoadAllJobs())
                {
                    if (jobItem.Name.Equals(oneCase.testParameters) && jobItem.SyncSource.Path.Equals(rightFolder))
                    {
                        FileSyncAgent currentAgent = new OneSync.Synchronization.FileSyncAgent(jobItem);
                        SyncPreviewResult previewResult = currentAgent.GenerateSyncPreview(null);
                        currentAgent.Synchronize(previewResult);
                        //currentAgent.Synchronize(previewResult);
                        break;
                    }
                }
                oneCase.testActual = "true";
            }
            #endregion
            #region leftsyncfirst id:leftsyncfirst:syncjob1,syncjob2,true or false:comment
            else if (oneCase.testMethod.Equals("leftsyncfirst"))
            {
                String[] comParameters = oneCase.testParameters.Split(',');
                foreach (SyncJob jobItem in SyncClient.GetSyncJobManager(System.Windows.Forms.Application.StartupPath).LoadAllJobs())
                {
                    if (jobItem.Name.Equals(comParameters[1]) && jobItem.SyncSource.Path.Equals(leftFolder))
                    {
                        FileSyncAgent currentAgent = new OneSync.Synchronization.FileSyncAgent(jobItem);
                        SyncPreviewResult previewResult = currentAgent.GenerateSyncPreview(null);
                        currentAgent.Synchronize(previewResult);
                        //SyncPreviewResult previewResult = currentAgent.PreviewSync();
                        //currentAgent.Synchronize(previewResult);
                        break;
                    }
                }

                foreach (SyncJob jobItem in SyncClient.GetSyncJobManager(System.Windows.Forms.Application.StartupPath).LoadAllJobs())
                {
                    if (jobItem.Name.Equals(comParameters[0]) && jobItem.SyncSource.Path.Equals(rightFolder))
                    {
                        FileSyncAgent currentAgent = new OneSync.Synchronization.FileSyncAgent(jobItem);
                        SyncPreviewResult previewResult = currentAgent.GenerateSyncPreview(null);
                        currentAgent.Synchronize(previewResult);
                        //SyncPreviewResult previewResult = currentAgent.PreviewSync();
                        //currentAgent.Synchronize(previewResult);
                        break;
                    }
                }

                DirectoryInfo diRight = new DirectoryInfo(rightFolder);
                DirectoryInfo diLeft = new DirectoryInfo(leftFolder);
                if (compareAll(diLeft, diRight))
                {
                    oneCase.testActual = "true";
                }
                else
                {
                    oneCase.testActual = "false";
                }
            }
            #endregion
            #region lefthalfsync id:lefthalfsync:syncjob:true or false:comment
            else if (oneCase.testMethod.Equals("lefthalfsync"))
            {
                String[] comParameters = oneCase.testParameters.Split(',');

                foreach (SyncJob jobItem in SyncClient.GetSyncJobManager(System.Windows.Forms.Application.StartupPath).LoadAllJobs())
                {
                    if (jobItem.Name.Equals(oneCase.testParameters) && jobItem.SyncSource.Path.Equals(leftFolder))
                    {
                        FileSyncAgent currentAgent = new OneSync.Synchronization.FileSyncAgent(jobItem);
                        SyncPreviewResult previewResult = currentAgent.GenerateSyncPreview(null);
                        currentAgent.Synchronize(previewResult);
                        //SyncPreviewResult previewResult = currentAgent.PreviewSync();
                        //currentAgent.Synchronize(previewResult);
                        break;
                    }
                }
                oneCase.testActual = "true";
            }
            #endregion
            #region updateprofile id:updateprofile:oldprofilename,newprofilename,sync source is right or left folder:true or false:comment
            else if (oneCase.testMethod.Equals("updateprofile")) //id:updateprofile:oldprofilename,newprofilename,sync source is right or left folder:comment
            {
                String[] comParameters = oneCase.testParameters.Split(',');
                String oldJobName = comParameters[0];
                String newJobName = comParameters[1];
                String syncSource = comParameters[2];
                syncSource = enumFolder(syncSource);
                SyncJob currentJob = null;
                //Find out the current profile.
                foreach (SyncJob jobItem in (SyncClient.GetSyncJobManager(System.Windows.Forms.Application.StartupPath).LoadAllJobs()))
                {
                    if (jobItem.Name.Equals(oldJobName) && jobItem.SyncSource.Path.Equals(syncSource))
                    {
                        currentJob = jobItem;
                        break;
                    }
                }
                currentJob.Name = newJobName;
                SyncClient.GetSyncJobManager(System.Windows.Forms.Application.StartupPath).Update(currentJob);

                foreach (SyncJob jobItem in (SyncClient.GetSyncJobManager(System.Windows.Forms.Application.StartupPath).LoadAllJobs()))
                {
                    if (jobItem.Name.Equals(oldJobName) && jobItem.SyncSource.Path.Equals(syncSource))
                    {
                        oneCase.testActual = "false, old job name is found for current sync source";
                        break;
                    }
                    if (jobItem.Name.Equals(newJobName) && jobItem.SyncSource.Path.Equals(syncSource))
                    {
                        oneCase.testActual = "true";
                    }
                }
            }
            #endregion
            #region deletefiles id:deletefiles:right or left,number of files to be deleted:true or false:comment
            else if (oneCase.testMethod.Equals("deletefiles")) //id:deletefiles:right or left,number of files to be deleted:true or false:comment
            {
                String[] comParameters = oneCase.testParameters.Split(',');
                int fileCount = 0;
                int fileMax = int.Parse(comParameters[1]);
                String[] fileNames = new String[fileMax];
                DirectoryInfo dirInfo;

                if (comParameters[0].Equals("right"))
                {
                    dirInfo = new DirectoryInfo(rightFolder);
                }
                else
                {
                    dirInfo = new DirectoryInfo(leftFolder);
                }

                if (dirInfo.GetFiles().Length < fileMax)
                {
                    oneCase.testActual = "Number of files to be deleted is larger than the number of files in the directory. This test case is skipped";
                }
                else
                {
                    foreach (FileInfo fileInf in dirInfo.GetFiles())
                    {
                        fileNames[fileCount] = fileInf.Name;
                        fileInf.Delete();
                        oneCase.testComment = oneCase.testComment + "//" + fileInf.Name + " is removed//";
                        fileCount++;
                        if (fileCount >= fileMax)
                        {                            
                            oneCase.testActual = "true";
                            break;
                        }
                        if (File.Exists(fileInf.FullName))
                        {
                            oneCase.testActual = fileInf.Name + "is still existed in directory.";
                            break;
                        }
                        else
                        {
                            oneCase.testActual = "true";
                        }
                    }
                }
            }
            #endregion
            #region deletefolders id:deletefolders:right or left,number of folders to be deleted:true or false:comment
            else if (oneCase.testMethod.Equals("deletefolders")) //id:deletefolders:right or left,number of folders to be deleted:true or false:comment
            {
                String[] comParameters = oneCase.testParameters.Split(',');
                int foldCount = 0;
                int foldMax = int.Parse(comParameters[1]);
                String[] foldNames = new String[foldMax];
                DirectoryInfo dirInfo;

                if (enumFolder(comParameters[0]).Equals("right"))
                {
                    dirInfo = new DirectoryInfo(rightFolder);
                }
                else
                {
                    dirInfo = new DirectoryInfo(leftFolder);
                }

                if (dirInfo.GetDirectories().Length < foldMax)
                {
                    oneCase.testActual = "Number of folders to be deleted is larger than the number of folders in the directory. This test case is skipped";
                }
                else
                {
                    foreach (DirectoryInfo dirInf in dirInfo.GetDirectories())
                    {
                        foldNames[foldCount] = dirInf.Name;
                        Directory.Delete(dirInf.FullName, true);
                        oneCase.testComment = oneCase.testComment + "//" + dirInf.Name + " is removed//";
                        foldCount++;
                        if (foldCount >= foldMax)
                        {                            
                            oneCase.testActual = "true";
                            break;
                        }
                        if (Directory.Exists(dirInf.FullName))
                        {
                            oneCase.testActual = dirInf.Name + "is still existed.";
                            break;
                        }
                        else
                        {
                            oneCase.testActual = "true";
                        }
                    }
                }
            }
            #endregion
            #region generateafile id:generateafile:right or left,filename.ext,filesize:true or false:comment
            else if (oneCase.testParameters.Equals("generateafile")) //id:generateafile:right or left,filename.ext,filesize:true or false:comment
            {
                String[] comParameters = oneCase.testParameters.Split(',');
                String fileName;

                if (enumFolder(comParameters[0]).Equals("right"))
                {
                    fileName = rightFolder + comParameters[1];
                }
                else
                {
                    fileName = leftFolder + comParameters[1];
                }
                using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fileStream.SetLength(long.Parse(comParameters[2]));
                }
                if (File.Exists(fileName))
                {
                    oneCase.testActual = "true";
                }
                else
                {
                    oneCase.testActual = "false";
                    oneCase.testComment = oneCase.testComment + "//" + fileName + " is not found.//";
                }
            }
            #endregion
            #region updateafiledate id:updateafiledate:right or left,filename.ext,modifieddate(YYYY/MM/DD):true or false:comment
            else if (oneCase.testParameters.Equals("updateafiledate"))//id:updateafiledate:right or left,filename.ext,modifieddate(YYYY/MM/DD):true or false:comment
            {
                String[] comParameters = oneCase.testParameters.Split(',');
                String fileName;
                if (enumFolder(comParameters[0]).Equals("right"))
                {
                    fileName = rightFolder + comParameters[1];
                }
                else
                {
                    fileName = leftFolder + comParameters[1];
                }
    
                File.SetLastWriteTime(fileName, DateTime.Parse(comParameters[2]));
                oneCase.testActual = "true";
            }
            #endregion
            else
            {
                oneCase.testActual = "Invalid command.";
            }
        }

        #region copyAll(DirectoryInfo sourceFolder, DirectoryInfo targetFolder)
        private void copyAll(DirectoryInfo sourceFolder, DirectoryInfo targetFolder)
        {
            foreach (FileInfo fileInf in sourceFolder.GetFiles())
            {
                fileInf.CopyTo(Path.Combine(targetFolder.ToString(), fileInf.Name), true);
            }

            foreach (DirectoryInfo diSourceSubDir in sourceFolder.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = targetFolder.CreateSubdirectory(diSourceSubDir.Name);
                copyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
        #endregion

        #region compareAll(DirectoryInfo sourceFolder, DirectoryInfo targetFolder)
        private Boolean compareAll(DirectoryInfo sourceFolder, DirectoryInfo targetFolder)
        {           
            Boolean continueFlag = false;
            Boolean resultFlag = false;
            Boolean dirFlag = false;

            foreach (FileInfo sourceInf in sourceFolder.GetFiles())
            {                
                resultFlag = false;
                foreach (FileInfo targetInf in targetFolder.GetFiles())
                {
                    //listTFiles = listTFiles + "(" + targetInf.Name + ")";
                    if (sourceInf.Length == targetInf.Length && sourceInf.Name == targetInf.Name)
                    {
                        resultFlag = true;
                        break;
                    }
                }
                if (!resultFlag)
                {
                    continueFlag = false;
                    break;
                }
                continueFlag = true;
             }

             //Detect empty directory
             if (sourceFolder.GetFiles().Length == 0 && targetFolder.GetFiles().Length == 0)
             {
                resultFlag = true;
             }

             if (continueFlag || sourceFolder.GetDirectories().Length > 0)
             {
                foreach (DirectoryInfo diSourceSubDir in sourceFolder.GetDirectories())
                {
                    listSFolders = listSFolders + "{" + diSourceSubDir.Name + "}";
                    foreach (DirectoryInfo diTargetSubDir in targetFolder.GetDirectories())
                    {
                        listTFolders = listTFolders + "{" + diTargetSubDir.Name + "}";
                        if (diSourceSubDir.Name.Equals(diTargetSubDir.Name))
                        {
                            dirFlag = true;
                            resultFlag = compareAll(diSourceSubDir, diTargetSubDir);
                            break;
                        }
                    }

                    if (!dirFlag)
                    {
                        resultFlag = false;
                        break;
                    }
                }
            }         
            return resultFlag;
        }
        #endregion

        #region enumFolder(String comParameter)
        private string enumFolder(String comParameter)
        {
            if(comParameter.Equals("right"))
            {
                return rightFolder;
            }
            else if (comParameter.Equals("left"))
            {
                return leftFolder;
            }
            else
            {
                throw new Exception("Command Error");
            }
        }
        #endregion

        #region enumAnoFolder(String comParameter)
        private string enumAnoFolder(String comParameter)
        {
            if (comParameter.Equals("right"))
            {
                return leftFolder;
            }
            else if (comParameter.Equals("left"))
            {
                return rightFolder;
            }
            else
            {
                throw new Exception("Command Error");
            }
        }
        #endregion

        #region RandomGenerator
        private static RandomGenerator.NameType detectNameType(String comParameter)
        {
            RandomGenerator.NameType nameType;
            if (comParameter.Equals("letters")) 
            {
                nameType = RandomGenerator.NameType.Letters;
            }
            else if (comParameter.Equals("unicode"))
            {
                nameType = RandomGenerator.NameType.Unicode;
            }
            else
            {
                nameType = RandomGenerator.NameType.LettersAndSymbols;
            }
            return nameType;
        }
        #endregion
    }
}
