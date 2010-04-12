/*
 $Id$
 */
/*
 *Last edited by Thuat in 29March
 * Changes: Apply method take in SyncPreviewResult as parameter
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Community.CsharpSqlite.SQLiteClient;
using Community.CsharpSqlite;
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

        private delegate void ExecuteActionsCallback(SyncAction action);
        private List<LogActivity> log;
        private SyncJob _job;

        // To keep track of sync progress
        int totalProgress = 0, currentProgress = 0;

        // Constructor
        public FileSyncAgent(SyncJob job)
        {
            this._job = job;
        }

        /// <summary>
        /// This method apply patches and generating new patches
        /// </summary>
        public void Synchronize(SyncPreviewResult preview)
        {
            OnStatusChanged(new SyncStatusChangedEventArgs("Applying patch"));
            Apply(preview);
            SyncEmptyFolders();
            OnStatusChanged(new SyncStatusChangedEventArgs("Generating patch"));
            Generate();
            OnSyncCompleted(new SyncCompletedEventArgs());
        }

        /// <summary>
        /// Generate the sync preview that contains sync actions to be executed.
        /// </summary>
        public SyncPreviewResult GenerateSyncPreview()
        {
            OnStatusChanged(new SyncStatusChangedEventArgs("Preparing to sync"));

            // Instantiates providers
            SyncActionsProvider actProvider = SyncClient.GetSyncActionsProvider(this._job.IntermediaryStorage.Path);
            MetaDataProvider mdProvider = SyncClient.GetMetaDataProvider(_job.IntermediaryStorage.Path, _job.SyncSource.ID);

            // Load actions to be executed.
            IList<SyncAction> actions = actProvider.Load(_job.SyncSource.ID, SourceOption.SOURCE_ID_NOT_EQUALS);

            // Generate current folder's metadata
            FileMetaData currentItems = MetaDataProvider.GenerateFileMetadata(_job.SyncSource.Path, _job.SyncSource.ID, false);

            // Load current folder metadata from database
            FileMetaData oldCurrentItems = mdProvider.LoadFileMetadata(_job.SyncSource.ID, SourceOption.SOURCE_ID_EQUALS);

            // Load the other folder metadata from database
            FileMetaData oldOtherItems = mdProvider.LoadFileMetadata(_job.SyncSource.ID, SourceOption.SOURCE_ID_NOT_EQUALS);

            return new SyncPreviewResult(actions, currentItems, oldCurrentItems, oldOtherItems);
        }

        /// <summary>
        /// Carry out the sync actions
        /// The previewResult contains 3 categories of items (copy over, delete, create new)
        /// </summary>
        private SyncResult Apply(SyncPreviewResult previewResult)
        {
            DateTime starTtime = DateTime.Now;
            SyncResult syncResult = new SyncResult();

            // Create a new list of log
            log = new List<LogActivity>();

            totalProgress = previewResult.ItemsCount;

            if (totalProgress == 0)
            {
                OnProgressChanged(new SyncProgressChangedEventArgs(1, 1));
                return syncResult;
            }

            currentProgress = 0;

            try
            {
                ExecuteCreateActions(previewResult.ItemsToCopyOver, syncResult);
                ExecuteDeleteActions(previewResult.ItemsToDelete, syncResult);
                ExecuteRenameActions(previewResult.ItemsToRename, syncResult);
                ExecuteConflictActions(previewResult.ConflictItems, syncResult);
            }
            catch (Exception){}
            finally
            {
                // Add to log
                Log.AddToLog(_job.SyncSource.Path, _job.IntermediaryStorage.Path,
                             _job.Name, log, Log.From, log.Count, starTtime, DateTime.Now);
            }
            return syncResult;
        }

        /// <summary>
        /// Generate patches 
        /// </summary>
        private void Generate()
        {
            IList<SyncAction> newActions = GenerateActions();

            SyncActionsProvider actProvider = SyncClient.GetSyncActionsProvider(_job.IntermediaryStorage.Path);
            DeleteSavedActions(actProvider);
            
            int totalWorkItems = newActions.Count;
            if (totalWorkItems == 0)
            {
                OnProgressChanged(new SyncProgressChangedEventArgs(1, 1));
                return;
            }

            // Logging
            List<LogActivity> generateActivities = new List<LogActivity>();
            DateTime starttime = DateTime.Now;

            int workItem = 0;

            Metadata mdStorage = MetaDataProvider.Generate(_job.IntermediaryStorage.DirtyFolderPath, "", false);
            try
            {
                foreach (SyncAction action in newActions)
                {
                    workItem++;
                    OnProgressChanged(new SyncProgressChangedEventArgs(workItem, totalWorkItems));

                    try
                    {
                        FileMetaDataItem item = new FileMetaDataItem("", "", action.RelativeFilePath, action.FileHash, DateTime.Now, 0, 0);
                        
                        OnSyncFileChanged(new SyncFileChangedEventArgs(ChangeType.NEWLY_CREATED, action.RelativeFilePath));

                        if (action.ChangeType == ChangeType.NEWLY_CREATED && !mdStorage.FileMetadata.MetaDataItems.Contains(item, new FileMetaDataItemComparer()))
                            SyncExecutor.CopyToDirtyFolderAndUpdateActionTable(action, _job);
                        else if (action.ChangeType == ChangeType.DELETED || action.ChangeType == ChangeType.RENAMED)
                        {
                            OnSyncFileChanged(new SyncFileChangedEventArgs(ChangeType.DELETED, action.RelativeFilePath));
                            actProvider.Add(action);

                            workItem++;
                            OnProgressChanged(new SyncProgressChangedEventArgs(workItem, totalWorkItems));
                        }
                        else SyncExecutor.UpdateTableAction(action, _job);

                        generateActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), "SUCCESS"));
                    }
                    catch (OutOfDiskSpaceException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        generateActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), "FAIL"));
                    }                      
                }
            }
            catch (Exception){}
            finally
            {
                Log.AddToLog(_job.SyncSource.Path, _job.IntermediaryStorage.Path,
                            _job.Name, generateActivities, Log.To, generateActivities.Count, starttime, DateTime.Now);
            }
            
        }

        private Metadata UpdateSyncSourceMetadata(MetaDataProvider mdProvider)
        {
            //read metadata of the current folder stored in the database
            Metadata mdCurrentOld = mdProvider.Load(_job.SyncSource.ID, SourceOption.SOURCE_ID_EQUALS);

            //read metadata of the current folder in file system 
            Metadata mdCurrent = MetaDataProvider.Generate(_job.SyncSource.Path, _job.SyncSource.ID, false);

            //Update metadata 
            mdProvider.Update(mdCurrentOld, mdCurrent);

            return mdCurrent;
        }

        private IList<SyncAction> GenerateActions()
        {
            MetaDataProvider mdProvider = SyncClient.GetMetaDataProvider(_job.IntermediaryStorage.Path, _job.SyncSource.Path);

            Metadata mdCurrent = UpdateSyncSourceMetadata(mdProvider);
            Metadata mdOther = mdProvider.Load(_job.SyncSource.ID, SourceOption.SOURCE_ID_NOT_EQUALS);

            //generate list of sync actions by comparing 2 metadata
            FileMetaDataComparer actionGenerator = new FileMetaDataComparer(mdCurrent.FileMetadata, mdOther.FileMetadata);
            
            return actionGenerator.Generate();
        }

        private void DeleteSavedActions(SyncActionsProvider actProvider)
        {
            //delete actions of previous sync
            actProvider.Delete(_job.SyncSource.ID, SourceOption.SOURCE_ID_EQUALS);
        }

        private void ExecuteCreateActions(IList<SyncAction> copyList, SyncResult syncResult)
        {
            ExecuteActions(copyList, syncResult, a =>
            {
                SyncExecutor.CopyToSyncFolderAndUpdateActionTable((CreateAction)a, _job);
            });
        }

        private void ExecuteDeleteActions(IList<SyncAction> deleteList, SyncResult syncResult)
        {
            ExecuteActions(deleteList, syncResult, a =>
            {
                SyncExecutor.DeleteInSyncFolderAndUpdateActionTable((DeleteAction)a, _job);
            });
        }

        private void ExecuteRenameActions(IList<SyncAction> renameList, SyncResult syncResult)
        {
            ExecuteActions(renameList, syncResult, a =>
            {
                SyncExecutor.RenameInSyncFolderAndUpdateActionTable((RenameAction)a, _job);
            });
        }

        private void ExecuteConflictActions(IList<SyncAction> conflictList, SyncResult syncResult)
        {
            ExecuteActions(conflictList, syncResult, a =>
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

        private void ExecuteActions(IList<SyncAction> actionsList, SyncResult syncResult, ExecuteActionsCallback exe)
        {
            foreach (SyncAction action in actionsList)
            {
                currentProgress++;
                OnProgressChanged(new SyncProgressChangedEventArgs(currentProgress, totalProgress));

                if (action.Skip)
                {
                    syncResult.Skipped.Add(action);
                    log.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), "SKIPPED"));
                    continue;
                }

                OnSyncFileChanged(new SyncFileChangedEventArgs(action.ChangeType, action.RelativeFilePath));

                string logStatus = "";
                try
                {
                    if (exe != null) exe(action);
                    syncResult.Ok.Add(action);
                    logStatus = "SUCCESS";
                }
                catch (Exception)
                {
                    logStatus = "FAIL";
                    syncResult.Errors.Add(action);
                }
                finally
                {
                    if (action.ConflictResolution != ConflictResolution.NONE)
                        logStatus = logStatus.Insert(0, action.ConflictResolution.ToString() + "_");
                    log.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), logStatus));
                }
            }
        }

        private void SyncEmptyFolders()
        {
            //read metadata of the current folder in file system 
            FolderMetadata currentItems = MetaDataProvider.GenerateFolderMetadata(_job.SyncSource.Path, _job.SyncSource.ID, false);

            //read metadata of the current folder stored in the database
            MetaDataProvider mdProvider = SyncClient.GetMetaDataProvider(_job.IntermediaryStorage.Path, _job.SyncSource.Path);

            FolderMetadata oldCurrentItems = mdProvider.LoadFolderMetadata(_job.SyncSource.ID, SourceOption.SOURCE_ID_EQUALS);
            FolderMetadata otherItems = mdProvider.LoadFolderMetadata(_job.SyncSource.ID, SourceOption.SOURCE_ID_NOT_EQUALS);

            FolderMetadataComparer comparer1 = new FolderMetadataComparer(oldCurrentItems, currentItems);
            FolderMetadataComparer compare2 = new FolderMetadataComparer(currentItems, otherItems);
            FolderMetadataComparer comparer3 = new FolderMetadataComparer(oldCurrentItems.FolderMetadataItems, compare2.LeftOnly);
            FolderMetadataComparer comparer4 = new FolderMetadataComparer(compare2.RightOnly, comparer1.LeftOnly);

            List<FolderMetadataItem> temps = comparer3.Both.ToList();
            temps.Sort(new FolderMetadataItemComparer());
            temps.Reverse();

            foreach (FolderMetadataItem item in temps)
                SyncExecutor.DeleteFolder(this._job.SyncSource.Path, item.RelativePath, true);

            foreach (FolderMetadataItem item in comparer4.LeftOnly)
                SyncExecutor.CreateFolder(this._job.SyncSource.Path, item.RelativePath);

            mdProvider.UpdateFolderMetadata(oldCurrentItems, currentItems);
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