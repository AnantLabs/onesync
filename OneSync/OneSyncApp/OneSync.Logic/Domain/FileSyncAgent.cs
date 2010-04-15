/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OneSync.Files;

namespace OneSync.Synchronization
{
    #region Delegates for event handler
    public delegate void SyncCompletedHandler(object sender, SyncCompletedEventArgs eventArgs);
    public delegate void SyncCancelledHandler(object sender, SyncCancelledEventArgs eventArgs);
    public delegate void SyncStatusChangedHandler(object sender, SyncStatusChangedEventArgs args);
    public delegate void SyncProgressChangedHandler(object sender, SyncProgressChangedEventArgs args);
    public delegate void SyncFileChangedHandler(object sender, SyncFileChangedEventArgs args);
    

    #endregion

    /// <summary>
    /// Delegates that can be used to update status message.
    /// </summary>
    public delegate void StatusCallbackDelegate(string statusMsg);


    /// <summary>
    /// 
    /// </summary>    
    public class FileSyncAgent
    {
        #region Events
        public event SyncCompletedHandler SyncCompleted;
        public event SyncStatusChangedHandler StatusChanged;
        public event SyncProgressChangedHandler ProgressChanged;
        public event SyncFileChangedHandler SyncFileChanged;
        
        #endregion Events

        private SyncJob _job;

        /* To keep track of progress */
        private int totalProgress = 0, currentProgress = 0;

        /* Providers */
        SyncActionsProvider actProvider;
        MetaDataProvider mdProvider;

        private List<LogActivity> log = new List<LogActivity>();

        private delegate void ExecuteActionsCallback(SyncAction action);

        // Constructor
        public FileSyncAgent(SyncJob job)
        {
            this._job = job;

            // Instantiates providers
            actProvider = SyncClient.GetSyncActionsProvider(this._job.IntermediaryStorage.Path);
            mdProvider = SyncClient.GetMetaDataProvider(_job.IntermediaryStorage.Path, _job.SyncSource.ID);
        }

        /// <summary>
        /// Generate the sync preview containing actions to be executed.
        /// The returned SyncPreviewResult can then be passed to Synchronize method
        /// as an argument for the actions to be executed.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public SyncPreviewResult GenerateSyncPreview(StatusCallbackDelegate statusCallback)
        {
            OnStatusChanged(new SyncStatusChangedEventArgs("Preparing to sync"));

            // Load actions to be executed.
            IList<SyncAction> actions = actProvider.Load(_job.SyncSource.ID, SourceOption.SOURCE_ID_NOT_EQUALS);

            // Generate current folder's metadata
            FileMetaData currentItems = MetaDataProvider.GenerateFileMetadata(_job.SyncSource.Path, _job.SyncSource.ID, false, statusCallback);

            // Load current folder metadata from database
            FileMetaData oldCurrentItems = mdProvider.LoadFileMetadata(_job.SyncSource.ID, SourceOption.SOURCE_ID_EQUALS);

            // Load the other folder metadata from database
            FileMetaData oldOtherItems = mdProvider.LoadFileMetadata(_job.SyncSource.ID, SourceOption.SOURCE_ID_NOT_EQUALS);

            return new SyncPreviewResult(actions, currentItems, oldCurrentItems, oldOtherItems);
        }

        /// <summary>
        /// Executes synchronization based on the SyncPreviewRe;sult as well
        /// as generate patch for other folder.
        /// </summary>
        /// <param name="preview">SyncPreviewResult to be executed.</param>
        public void Synchronize(SyncPreviewResult preview)
        {
            try
            {
                OnStatusChanged(new SyncStatusChangedEventArgs("Applying patch"));
                ApplyPatch(preview);
                SyncEmptyFolders();
                OnStatusChanged(new SyncStatusChangedEventArgs("Generating patch"));
                GeneratePatch();
                OnSyncCompleted(new SyncCompletedEventArgs());
            }
            catch (OutOfDiskSpaceException)
            {
                throw;
            }
        }

        /// <summary>
        /// Execute actions of the sync preview result. The actions to be executed
        /// file copy, deletion or rename
        /// </summary>
        private void ApplyPatch(SyncPreviewResult previewResult)
        {
            DateTime startTime = DateTime.Now;
            
            // Create a new list of log
            log.Clear();

            totalProgress = previewResult.ItemsCount;
            if (totalProgress == 0)
            {
                OnProgressChanged(new SyncProgressChangedEventArgs(1, 1));
                return;
            }

            currentProgress = 0;
            try
            {
                ExecuteCreateActions(previewResult.ItemsToCopyOver);
                ExecuteDeleteActions(previewResult.ItemsToDelete);
                ExecuteRenameActions(previewResult.ItemsToRename);
                ExecuteConflictActions(previewResult.ConflictItems);
            }
            catch (Exception){}
            finally
            {
                WriteLog(startTime, Log.From);
            }
        }

        /// <summary>
        /// Generate patches 
        /// </summary>
        private void GeneratePatch()
        {
            // Delete previously saved actions
            actProvider.Delete(_job.SyncSource.ID, SourceOption.SOURCE_ID_EQUALS);

            // Generate new actions
            IList<SyncAction> newActions = GenerateActions();
            
            if (newActions.Count == 0)
            {
                OnProgressChanged(new SyncProgressChangedEventArgs(1, 1));
                return;
            }

            // Clear log
            log.Clear();
            DateTime startTime = DateTime.Now;

            /*
            try
            {
                FileMetaData mdDirtyFolder =
                    MetaDataProvider.GenerateFileMetadata(_job.IntermediaryStorage.DirtyFolderPath, "", false, false);
                DeleteRedundantDirtyFiles(newActions, mdDirtyFolder.MetaDataItems, _job.IntermediaryStorage.DirtyFolderPath);
            }
            catch (Exception){}
             */

            try
            {
                SaveActionsAndDirtyFiles(newActions);
            }
            catch (OutOfDiskSpaceException)
            {
                throw;
            }
            finally
            {
                WriteLog(startTime, Log.To);
            }
            //catch (OutOfDiskSpaceException) { throw; }
            
            
        }

        private void DeleteRedundantDirtyFiles(IList<SyncAction> actions, IList<FileMetaDataItem> items, string baseFolder)
        {
            FileMetadataSyncActionsComparer comparer = new FileMetadataSyncActionsComparer(actions, items);
            List<FileMetaDataItem> mdOnly = comparer.InMetaDataOnly;
            foreach (var item in mdOnly)
            {
                string absolute = baseFolder + item.RelativePath;
                Files.FileUtils.DeleteFileAndFolderIfEmpty(baseFolder, absolute, true);        
            }
        }

        private void SaveActionsAndDirtyFiles(IList<SyncAction> actions)
        {
            FileMetaData fileMetaData = MetaDataProvider.GenerateFileMetadata(_job.IntermediaryStorage.DirtyFolderPath,
                                                                              "", false,true);
            int totalProgress = actions.Count;
            int currProgress = 0;

            foreach (SyncAction a in actions)
            {
                if (!Validator.SyncJobParamsValidated(_job.Name, _job.IntermediaryStorage.Path, _job.SyncSource.Path))
                    return;

                try
                {
                    OnProgressChanged(new SyncProgressChangedEventArgs(++currProgress, totalProgress));
                    OnSyncFileChanged(new SyncFileChangedEventArgs(a.ChangeType, a.RelativeFilePath));

                    FileMetaDataItem item = new FileMetaDataItem("", a.RelativeFilePath, a.FileHash, DateTime.Now, 0, 0);

                    if (a.ChangeType == ChangeType.NEWLY_CREATED && !fileMetaData.MetaDataItems.Contains(item, new FileMetaDataItemComparer()))
                        SyncExecutor.CopyToDirtyFolderAndUpdateActionTable(a, _job);
                    else
                        actProvider.Add(a);

                    log.Add(new LogActivity(a.RelativeFilePath, a.ChangeType.ToString(), "SUCCESS"));
                }
                catch (OutOfDiskSpaceException)
                {
                    log.Add(new LogActivity(a.RelativeFilePath, a.ChangeType.ToString(),"FAIL"));
                    throw;
                }
                catch (Exception)
                {
                    log.Add(new LogActivity(a.RelativeFilePath, a.ChangeType.ToString(), "FAIL"));
                    throw;
                }
            }
        }

        private void ExecuteCreateActions(IList<SyncAction> copyList)
        {
            ExecuteActions(copyList, a =>
            {
                SyncExecutor.CopyToSyncFolderAndUpdateActionTable((CreateAction)a, _job);
            });
        }

        private void ExecuteDeleteActions(IList<SyncAction> deleteList)
        {
            ExecuteActions(deleteList, a =>
            {
                SyncExecutor.DeleteInSyncFolderAndUpdateActionTable((DeleteAction)a, _job);
                string absolutePath = _job.IntermediaryStorage.DirtyFolderPath + a.RelativeFilePath;
                Files.FileUtils.DeleteFileAndFolderIfEmpty(_job.IntermediaryStorage.DirtyFolderPath, absolutePath, true);
            });
        }

        private void ExecuteRenameActions(IList<SyncAction> renameList)
        {
            ExecuteActions(renameList, a =>
            {
                SyncExecutor.RenameInSyncFolderAndUpdateActionTable((RenameAction)a, _job);
            });
        }

        private void ExecuteConflictActions(IList<SyncAction> conflictList)
        {
            ExecuteActions(conflictList, a =>
            {
                if (a.ChangeType == ChangeType.NEWLY_CREATED)
                {
                    if (a.ConflictResolution == ConflictResolution.DUPLICATE_RENAME)
                        SyncExecutor.DuplicateRenameInSyncFolderAndUpdateActionTable((CreateAction)a, _job);
                    else if (a.ConflictResolution == ConflictResolution.OVERWRITE)
                        SyncExecutor.CopyToSyncFolderAndUpdateActionTable((CreateAction)a, _job);
                }
                else if (a.ChangeType == ChangeType.RENAMED)
                {
                    if (a.ConflictResolution == ConflictResolution.DUPLICATE_RENAME)
                        SyncExecutor.ConflictRenameAndUpdateActionTable((RenameAction)a, _job, true);
                    else if (a.ConflictResolution == ConflictResolution.OVERWRITE)
                        SyncExecutor.ConflictRenameAndUpdateActionTable((RenameAction)a, _job, false);
                }
                else if (a.ChangeType == ChangeType.DELETED)
                {
                    SyncExecutor.DeleteFromActionTable((DeleteAction)a, _job);
                }
            });
        }

        private void ExecuteActions(IList<SyncAction> actionsList, ExecuteActionsCallback exe)
        {
            foreach (SyncAction action in actionsList)
            {
                currentProgress++;
                OnProgressChanged(new SyncProgressChangedEventArgs(currentProgress, totalProgress));
                OnSyncFileChanged(new SyncFileChangedEventArgs(action.ChangeType, action.RelativeFilePath));

                if (action.Skip)
                {                    
                    log.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), "SKIPPED"));
                    continue;
                }

                string logStatus = "";
                try
                {
                    if (exe != null) exe(action);
                    logStatus = "SUCCESS";
                }
                catch (Exception)
                {
                    logStatus = "FAIL";
                        
                }
                finally
                {
                    if (action.ConflictResolution != ConflictResolution.NONE)
                        logStatus = logStatus.Insert(0, action.ConflictResolution.ToString() + "_");
                    log.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), logStatus));
                }
            }
        }

        private Metadata UpdateSyncSourceMetadata()
        {
            //read metadata of the current folder stored in the database
            Metadata mdCurrentOld = mdProvider.Load(_job.SyncSource.ID, SourceOption.SOURCE_ID_EQUALS);

            //read metadata of the current folder in file system 
            Metadata mdCurrent = MetaDataProvider.Generate(_job.SyncSource.Path, _job.SyncSource.ID, false, false,true);

            //Update metadata 
            mdProvider.Update(mdCurrentOld, mdCurrent);

            return mdCurrent;
        }

        /// <summary>
        /// Generates SyncActions to be executed by other PC by comparing the metadata 
        /// of current SyncSource and the metadata of other PC.
        /// </summary>
        private IList<SyncAction> GenerateActions()
        {
            //Metadata mdCurrent = UpdateSyncSourceMetadata();
            //Metadata mdOther = mdProvider.Load(_job.SyncSource.ID, SourceOption.SOURCE_ID_NOT_EQUALS);
            FileMetaData mdCurrent = MetaDataProvider.GenerateFileMetadata(
                _job.SyncSource.Path, _job.SyncSource.ID, false, false);

            FileMetaData mdOldCurrent = mdProvider.LoadFileMetadata(_job.SyncSource.ID, SourceOption.SOURCE_ID_EQUALS);

            mdProvider.UpdateFileMetadata(mdOldCurrent, mdCurrent);

            FileMetaData mdOther = mdProvider.LoadFileMetadata(_job.SyncSource.ID, SourceOption.SOURCE_ID_NOT_EQUALS);
            //generate list of sync actions by comparing 2 metadata
            var differences = new FileMetaDataComparer(mdCurrent, mdOther);

            return actProvider.Generate(mdCurrent.SourceId,
                                        differences.LeftOnly,
                                        differences.RightOnly,
                                        differences.BothModified);
        }

        private void WriteLog(DateTime startTime, string direction)
        {
            Log.AddToLog(_job.SyncSource.Path, _job.IntermediaryStorage.Path,
                            _job.Name, log, direction, log.Count, startTime, DateTime.Now);
        }

        private void SyncEmptyFolders()
        {
            //read metadata of the current folder in file system 
            FolderMetadata currentItems = MetaDataProvider.GenerateFolderMetadata(_job.SyncSource.Path, _job.SyncSource.ID, false,false,true);

            //read the folder metadata of current folder stored in database
            FolderMetadata oldCurrentItems = mdProvider.LoadFolderMetadata(_job.SyncSource.ID, SourceOption.SOURCE_ID_EQUALS);

            //read folder metadata of the other source stored in the database
            FolderMetadata otherItems = mdProvider.LoadFolderMetadata(_job.SyncSource.ID, SourceOption.SOURCE_ID_NOT_EQUALS);

            //get the difference between current folder metadata with its previous state (from previous sync session).
            FolderMetadataComparer comparer1 = new FolderMetadataComparer(oldCurrentItems, currentItems);

            //get the difference between current folder metadata with the other folder's previous metadata
            FolderMetadataComparer compare2 = new FolderMetadataComparer(currentItems, otherItems);


            FolderMetadataComparer comparer3 = new FolderMetadataComparer(oldCurrentItems.FolderMetadataItems, compare2.LeftOnly);
            FolderMetadataComparer comparer4 = new FolderMetadataComparer(compare2.RightOnly, comparer1.LeftOnly);

            List<FolderMetadataItem> temps = comparer3.Both.ToList();
            
            temps.Sort(new FolderMetadataItemComparer());
            temps.Reverse();

            foreach (FolderMetadataItem item in temps)
                SyncExecutor.DeleteFolder(this._job.SyncSource.Path, item.RelativePath, true);

            foreach (FolderMetadataItem item in comparer4.LeftOnly)
                if (item.IsEmpty == 1) SyncExecutor.CreateFolder(this._job.SyncSource.Path, item.RelativePath);

            currentItems = MetaDataProvider.GenerateFolderMetadata(_job.SyncSource.Path, _job.SyncSource.ID, false,
                                                                   false, true);
            mdProvider.UpdateFolderMetadata(oldCurrentItems, currentItems, false);
        }

        #region Event Raising
        protected virtual void OnSyncCompleted(SyncCompletedEventArgs e)
        {
            if (SyncCompleted != null) SyncCompleted(this, e);
        }

        protected virtual void OnStatusChanged(SyncStatusChangedEventArgs e)
        {
            if (StatusChanged != null) StatusChanged(this, e);
        }

        protected virtual void OnProgressChanged(SyncProgressChangedEventArgs e)
        {
            if (ProgressChanged != null) ProgressChanged(this, e);
        }

        protected virtual void OnSyncFileChanged(SyncFileChangedEventArgs e)
        {
            if (SyncFileChanged != null) SyncFileChanged(this, e);
        }
        #endregion
    }
}