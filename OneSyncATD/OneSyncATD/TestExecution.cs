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
                }
                catch (Exception exception)
                {
                    //throw exception;
                    oneCase.testActual = "false";
                }
                finally
                {
                    if (oneCase.testActual.Equals(oneCase.testExpected))
                    {
                        oneCase.testPassed = true;
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
                    oneCase.testActual = "false";
                }
                else
                {
                    oneCase.testActual = "true";
                }

                sWatch.Stop();            

            }
            else if (oneCase.testMethod.Equals("copyfrom"))
            {
                try
                {
                    String sourceFolder = enumFolder(oneCase.testParameters);
                    String targetFolder = enumAnoFolder(oneCase.testParameters);
                    DirectoryInfo diSource = new DirectoryInfo(sourceFolder);
                    DirectoryInfo diTarget = new DirectoryInfo(targetFolder);
                    copyAll(diSource, diTarget);
                    oneCase.testActual = "true";
                }
                catch(Exception exception)
                {
                    oneCase.testActual = "false";
                }

            }
            else if (oneCase.testMethod.Equals("clearall"))
            {
                DirectoryInfo diRight = new DirectoryInfo(rightFolder);
                DirectoryInfo diInter = new DirectoryInfo(interFolder);
                DirectoryInfo diLeft = new DirectoryInfo(leftFolder);

                foreach (Profile profileItem in (SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).LoadAllProfiles()))
                {
                    SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).Delete(profileItem);
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
            else if (oneCase.testMethod.Equals("createprofile")) 
            {
                String[] comParameters = oneCase.testParameters.Split(',');
                Boolean passRFlag = false;
                Boolean passLFlag = false;

                if (SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).ProfileExists(comParameters[0]))
                {
                    passRFlag = true;
                    oneCase.testComment = oneCase.testComment + "//Profiles--" + comParameters[0] + "is created before.//";
                }
                else
                {
                    SyncClient.Initialize(System.Windows.Forms.Application.StartupPath, comParameters[0], rightFolder, interFolder);
                }

                if (SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).ProfileExists(comParameters[1]))
                {
                    passLFlag = true;
                    oneCase.testComment = oneCase.testComment + "//Profiles--" + comParameters[1] + "is created before.//";
                }
                else
                {
                    SyncClient.Initialize(System.Windows.Forms.Application.StartupPath, comParameters[1], leftFolder, interFolder);
                }

                if (SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).ProfileExists(comParameters[0]))
                {
                    passRFlag = true;
                }
                if (SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).ProfileExists(comParameters[1]))
                {
                    passLFlag = true;
                }
                if (passRFlag && passLFlag)
                {
                    oneCase.testActual = "true";
                }
                else
                {
                    oneCase.testActual = "false";
                    oneCase.testComment = oneCase.testComment + "//Profiles are not created successfully.//";
                }
            }
            else if (oneCase.testMethod.Equals("rightsyncfirst"))
            {
                String[] comParameters = oneCase.testParameters.Split(',');
                foreach (Profile profileItem in SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).LoadAllProfiles())
                {
                    if (profileItem.Name.Equals(comParameters[0]) && profileItem.SyncSource.Path.Equals(rightFolder))
                    {
                        FileSyncAgent currentAgent = new OneSync.Synchronization.FileSyncAgent(profileItem);
                        SyncPreviewResult previewResult = currentAgent.PreviewSync();
                        currentAgent.Synchronize(previewResult);
                        break;                        
                    }
                }

                foreach (Profile profileItem in SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).LoadAllProfiles())
                {
                    if (profileItem.Name.Equals(comParameters[1]) && profileItem.SyncSource.Path.Equals(leftFolder))
                    {
                        FileSyncAgent currentAgent = new OneSync.Synchronization.FileSyncAgent(profileItem);
                        SyncPreviewResult previewResult = currentAgent.PreviewSync();
                        currentAgent.Synchronize(previewResult);
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
                    oneCase.testComment = oneCase.testComment + "//Both right and left directories are not balanced with each other.//";
                }
                
            }
            else if (oneCase.testMethod.Equals("leftsyncfirst"))
            {
                String[] comParameters = oneCase.testParameters.Split(',');
                foreach (Profile profileItem in SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).LoadAllProfiles())
                {
                    if (profileItem.Name.Equals(comParameters[0]) && profileItem.SyncSource.Path.Equals(rightFolder))
                    {
                        FileSyncAgent currentAgent = new OneSync.Synchronization.FileSyncAgent(profileItem);
                        SyncPreviewResult previewResult = currentAgent.PreviewSync();
                        currentAgent.Synchronize(previewResult);
                        break;
                    }
                }

                foreach (Profile profileItem in SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).LoadAllProfiles())
                {
                    if (profileItem.Name.Equals(comParameters[1]) && profileItem.SyncSource.Path.Equals(leftFolder))
                    {
                        FileSyncAgent currentAgent = new OneSync.Synchronization.FileSyncAgent(profileItem);
                        SyncPreviewResult previewResult = currentAgent.PreviewSync();
                        currentAgent.Synchronize(previewResult);
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
                    oneCase.testComment = oneCase.testComment + "//Both right and left directories are not balanced with each other.//"; 
                }

            }
            else if (oneCase.testMethod.Equals("updateprofile")) //id:updateprofile:oldprofilename,newprofilename,sync source is right or left folder:comment
            {
                String[] comParameters = oneCase.testParameters.Split(',');
                String oldProName = comParameters[0];
                String newProName = comParameters[1];
                String syncSource = comParameters[2];
                syncSource = enumFolder(syncSource);
                Profile currentProfile = null;
                //Find out the current profile.
                foreach (Profile profileItem in (SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).LoadAllProfiles()))
                {
                    if (profileItem.Name.Equals(oldProName) && profileItem.SyncSource.Path.Equals(syncSource))
                    {
                        currentProfile = profileItem;
                        break;
                    }
                }
                currentProfile.Name = newProName;
                SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).Update(currentProfile);

                foreach (Profile profileItem in (SyncClient.GetProfileManager(System.Windows.Forms.Application.StartupPath).LoadAllProfiles()))
                {
                    if (profileItem.Name.Equals(oldProName) && profileItem.SyncSource.Path.Equals(syncSource))
                    {
                        oneCase.testActual = "false";
                        oneCase.testComment = oneCase.testComment + " //Old sync job name is found for current sync source.//";
                        break;
                    }
                    if (profileItem.Name.Equals(newProName) && profileItem.SyncSource.Path.Equals(syncSource))
                    {
                        oneCase.testActual = "true";
                    }
                }
            }
            else if (oneCase.testMethod.Equals("deletefiles")) //id:deletefiles:right or left,number of files to be deleted:true or false:comment
            {
                String[] comParameters = oneCase.testParameters.Split(',');
                int fileCount = 0;
                int fileMax = int.Parse(comParameters[1]);
                String[] fileNames = new String[fileMax];
                DirectoryInfo dirInfo;

                if (enumFolder(comParameters[0]).Equals("right"))
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
                        fileCount++;
                        if (fileCount >= fileMax)
                        {
                            oneCase.testComment = oneCase.testComment + "//" + fileNames[fileCount - 1] + " is removed//";
                            oneCase.testActual = "true";
                            break;
                        }
                    }
                }
            }
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
                        dirInf.Delete();
                        foldCount++;
                        if (foldCount >= foldMax)
                        {
                            oneCase.testComment = oneCase.testComment + "//" + foldNames[foldCount - 1] + " is removed//";
                            oneCase.testActual = "true";
                            break;
                        }
                    }
                }
            }
            else
            {
                oneCase.testActual = "Invalid command.";
            }
        } 

        public void copyAll(DirectoryInfo sourceFolder, DirectoryInfo targetFolder)
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

        public Boolean compareAll(DirectoryInfo sourceFolder, DirectoryInfo targetFolder)
        {
            Boolean continueFlag = false;
            Boolean resultFlag = false;
            Boolean dirFlag = false;
            foreach (FileInfo sourceInf in sourceFolder.GetFiles())
            {
                resultFlag = false;
                foreach (FileInfo targetInf in targetFolder.GetFiles())
                {
                    if (sourceInf.Length == targetInf.Length)
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
                    foreach (DirectoryInfo diTargetSubDir in targetFolder.GetDirectories())
                    {
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
    }
}
