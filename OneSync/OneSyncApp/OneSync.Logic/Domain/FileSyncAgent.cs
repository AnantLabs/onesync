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

namespace OneSync.Synchronization
{
    #region Declare delegates
    public delegate void SyncCompletedHandler(object sender, SyncCompletedEventArgs eventArgs);
    public delegate void SyncCancelledHandler(object sender, SyncCancelledEventArgs eventArgs);
    public delegate void SyncStatusChangedHandler(object sender, SyncStatusChangedEventArgs args);
    public delegate void SyncProgressChangedHandler(object sender, SyncProgressChangedEventArgs args);
    public delegate void SyncFileChangedHandler(object sender, SyncFileChangedEventArgs args);
    #endregion Declare delegates


    /// <summary>
    /// 
    /// </summary>    
    public class FileSyncAgent : BaseSyncAgent
    {
        #region Declare events
        public event SyncCompletedHandler SyncCompleted;
        public event SyncStatusChangedHandler StatusChanged;
        public event SyncProgressChangedHandler ProgressChanged;
        public event SyncFileChangedHandler SyncFileChanged;
        #endregion declare events

        private List<SyncAction> actions = new List<SyncAction>();
        private SyncJob _job;


        #region Constructors
        //default constructor
        public FileSyncAgent()
            : base()
        {
        }

        public FileSyncAgent(SyncJob job)
            : base(job)
        {
            this._job = job;
        }
        #endregion Constructors

        /// <summary>
        /// This method apply patches and generating new patches
        /// </summary>
        /// <param name="preview"></param>
        public void Synchronize(SyncPreviewResult preview)
        {
            if (StatusChanged != null) StatusChanged(this, new SyncStatusChangedEventArgs("Applying patch"));
            Apply(preview);
            SyncEmptyFolders();
            if (StatusChanged != null) StatusChanged(this, new SyncStatusChangedEventArgs("Generating patch"));
            Generate();
            if (SyncCompleted != null) SyncCompleted(this, new SyncCompletedEventArgs());
        }

        /// <summary>
        /// Generate the sync preview that being applied to the folder
        /// </summary>
        /// <returns></returns>
        public SyncPreviewResult GenerateSyncPreview()
        {
            if (StatusChanged != null)
                StatusChanged(this, new SyncStatusChangedEventArgs("Preparing to sync"));

            SyncActionsProvider actProvider = SyncClient.GetSyncActionsProvider(this._job.IntermediaryStorage.Path);
            actions = (List<SyncAction>)actProvider.Load(_job.SyncSource.ID, SourceOption.SOURCE_ID_NOT_EQUALS);

            //Generate current folder's metadata
            FileMetaData currentItems = MetaDataProvider.GenerateFileMetadata(_job.SyncSource.Path, _job.SyncSource.ID, false);

            //Load current folder metadata from database
            MetaDataProvider mdProvider = SyncClient.GetMetaDataProvider(_job.IntermediaryStorage.Path, _job.SyncSource.ID);
            FileMetaData oldCurrentItems = mdProvider.LoadFileMetadata(_job.SyncSource.ID, SourceOption.SOURCE_ID_EQUALS);

            //Load the other folder metadata from database
            FileMetaData oldOtherItems = mdProvider.LoadFileMetadata(_job.SyncSource.ID, SourceOption.SOURCE_ID_NOT_EQUALS);
            return new SyncPreviewResult(actions, currentItems, oldCurrentItems, oldOtherItems);

        }

        /// <summary>
        /// Carry out the sync actions
        /// The previewResult contains 3 categories of items (copy over, delete, create new)
        /// </summary>
        /// <returns></returns>
        public SyncResult Apply(SyncPreviewResult previewResult)
        {
            // Logging
            List<LogActivity> applyActivities = new List<LogActivity>();
            DateTime starttime = DateTime.Now;
            SyncResult syncResult = new SyncResult();

            int totalWorkItems = previewResult.ItemsToCopyOver.Count +
                previewResult.ConflictItems.Count + previewResult.ItemsToDelete.Count;
            if (totalWorkItems == 0)
                if (ProgressChanged != null) ProgressChanged(this, new SyncProgressChangedEventArgs(100));
            int workItem = 0;

            #region items to copy over
            foreach (SyncAction action in previewResult.ItemsToCopyOver)
            {
                if (ProgressChanged != null)
                {
                    workItem++;
                    ProgressChanged(this, new SyncProgressChangedEventArgs((workItem * 100 / totalWorkItems)));
                }
                if (action.Skip)
                {
                    syncResult.Skipped.Add(action);
                    applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), "SKIPPED"));
                    //add log                   
                    continue;
                }
                if (SyncFileChanged != null) SyncFileChanged(this, new SyncFileChangedEventArgs(ChangeType.NEWLY_CREATED, action.RelativeFilePath));
                try
                {
                    try { SyncExecutor.CopyToSyncFolderAndUpdateActionTable((CreateAction)action, _job); }
                    catch (Exception) { applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), "FAIL")); }
                    syncResult.Ok.Add(action);
                    applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), "SUCCESS"));
                }
                catch (Exception) { Console.WriteLine("Logging error"); }
            }
            #endregion items to copy over

            #region items to delete
            foreach (SyncAction action in previewResult.ItemsToDelete)
            {
                if (ProgressChanged != null)
                {
                    workItem++;
                    ProgressChanged(this, new SyncProgressChangedEventArgs((workItem * 100 / totalWorkItems)));
                }
                if (action.Skip)
                {
                    syncResult.Skipped.Add(action);
                    applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), "SKIPPED"));
                    //add log                    
                    continue;
                }
                if (SyncFileChanged != null) SyncFileChanged(this, new SyncFileChangedEventArgs(ChangeType.DELETED, action.RelativeFilePath));
                try
                {
                    try
                    { SyncExecutor.DeleteInSyncFolderAndUpdateActionTable((DeleteAction)action, _job); }
                    catch (Exception)
                    {
                        syncResult.Errors.Add(action);
                        applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), "FAIL"));
                    }
                    syncResult.Ok.Add(action);
                    applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), "SUCCESS"));
                }
                catch (Exception) { Console.WriteLine("Logging error "); }
            }
            #endregion items to delete

            #region conflicted items
            foreach (SyncAction action in previewResult.ConflictItems)
            {
                if (ProgressChanged != null)
                {
                    workItem++;
                    ProgressChanged(this, new SyncProgressChangedEventArgs((workItem * 100 / totalWorkItems)));
                }
                if (action.Skip)
                {
                    syncResult.Skipped.Add(action);
                    applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), action.ConflictResolution.ToString()));
                    continue;
                }
                else if (action.ConflictResolution == ConflictResolution.DUPLICATE_RENAME)
                {
                    if (SyncFileChanged != null) SyncFileChanged(this, new SyncFileChangedEventArgs(ChangeType.NEWLY_CREATED, action.RelativeFilePath));
                    try
                    {
                        try
                        { SyncExecutor.DuplicateRenameToSyncFolderAndUpdateActionTable((CreateAction)action, _job); }
                        catch (Exception)
                        {
                            syncResult.Errors.Add(action);
                            applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), action.ConflictResolution.ToString() + "_FAIL"));
                        }
                        syncResult.Ok.Add(action);
                        applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), action.ConflictResolution.ToString() + "_SUCCESS"));
                    }
                    catch (Exception)
                    { Console.WriteLine("Logging error"); }
                }
                else if (action.ConflictResolution == ConflictResolution.OVERWRITE)
                {
                    if (action.Skip)
                    {
                        syncResult.Skipped.Add(action);
                        applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), action.ConflictResolution.ToString()));
                        continue;
                    }
                    if (SyncFileChanged != null) SyncFileChanged(this, new SyncFileChangedEventArgs(ChangeType.MODIFIED, action.RelativeFilePath));
                    try
                    {
                        try
                        { SyncExecutor.CopyToSyncFolderAndUpdateActionTable((CreateAction)action, _job); }
                        catch (Exception)
                        {
                            syncResult.Errors.Add(action);
                            applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), action.ConflictResolution.ToString() + "_FAIL"));
                        }
                        syncResult.Ok.Add(action);
                        applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), action.ConflictResolution.ToString() + "_SUCCESS"));
                    }
                    catch (Exception)
                    { Console.WriteLine("Logging error"); }
                }
            }

            #endregion conflicted items




            // Add to log
            Log.AddToLog(_job.SyncSource.Path, _job.IntermediaryStorage.Path,
                    _job.Name, applyActivities, Log.From, applyActivities.Count, starttime, DateTime.Now);

            return syncResult;
            // TODO:
            // Update metadata after patch is applied
            // Delete all dirty files
        }

        /// <summary>
        /// Generate patches 
        /// </summary>
        private void Generate()
        {
            //read metadata of the current folder in file system 
            Metadata currentItems = MetaDataProvider.Generate(_job.SyncSource.Path, _job.SyncSource.ID, false);
            Metadata metadataInIStorage = MetaDataProvider.Generate(_job.IntermediaryStorage.DirtyFolderPath, "", false);


            //read metadata of the current folder stored in the database
            MetaDataProvider mdProvider = SyncClient.GetMetaDataProvider(_job.IntermediaryStorage.Path, _job.SyncSource.Path);
            Metadata storedItems = mdProvider.Load(_job.SyncSource.ID, SourceOption.SOURCE_ID_EQUALS);

            //Update metadata 
            mdProvider.Update(storedItems, currentItems);

            storedItems = mdProvider.Load(_job.SyncSource.ID, SourceOption.SOURCE_ID_NOT_EQUALS);
            FileMetaDataComparer actionGenerator = new FileMetaDataComparer(currentItems.FileMetadata, storedItems.FileMetadata);

            //generate list of sync actions by comparing 2 metadata

            IList<SyncAction> newActions = actionGenerator.Generate();

            //delete actions of previous sync
            SyncActionsProvider actProvider = SyncClient.GetSyncActionsProvider(_job.IntermediaryStorage.Path);
            actProvider.Delete(_job.SyncSource.ID, SourceOption.SOURCE_ID_EQUALS);

            // Logging
            List<LogActivity> generateActivities = new List<LogActivity>();
            int count = 0;
            DateTime starttime = DateTime.Now;

            int totalWorkItems = newActions.Count;
            if (totalWorkItems == 0)
                if (ProgressChanged != null) ProgressChanged(this, new SyncProgressChangedEventArgs(100));
            int workItem = 0;

            foreach (SyncAction action in newActions)
            {
                if (ProgressChanged != null)
                {
                    workItem++;
                    ProgressChanged(this, new SyncProgressChangedEventArgs((workItem * 100 / totalWorkItems)));
                }
                try
                {
                    FileMetaDataItem item = new FileMetaDataItem("", "", action.RelativeFilePath, action.FileHash, DateTime.Now, 0, 0);
                    #region newly created action
                    if (SyncFileChanged != null) SyncFileChanged(this, new SyncFileChangedEventArgs(ChangeType.NEWLY_CREATED, action.RelativeFilePath));
                    if (action.ChangeType == ChangeType.NEWLY_CREATED && !metadataInIStorage.FileMetadata.MetaDataItems.Contains(item, new FileMetaDataItemComparer()))
                    {
                        SyncExecutor.CopyToDirtyFolderAndUpdateActionTable(action, _job);
                    }
                    #endregion newly created action

                    #region delete action
                    else if (action.ChangeType == ChangeType.DELETED || action.ChangeType == ChangeType.RENAMED)
                    {
                        if (SyncFileChanged != null) SyncFileChanged(this, new SyncFileChangedEventArgs(ChangeType.DELETED, action.RelativeFilePath));
                        actProvider.Add(action);
                        if (ProgressChanged != null)
                        {
                            workItem++;
                            ProgressChanged(this, new SyncProgressChangedEventArgs((workItem * 100 / totalWorkItems)));
                        }
                    }
                    #endregion delete action

                    generateActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), "SUCCESS"));
                }
                catch (Exception)
                {
                    generateActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), "FAIL"));
                }
                count++;
            }

            // Add to log
            Log.AddToLog(_job.SyncSource.Path, _job.IntermediaryStorage.Path,
                _job.Name, generateActivities, Log.To, generateActivities.Count, starttime, DateTime.Now);
        }

        /// <summary>
        /// Gets the SyncJob associated with this FileSyncAgent
        /// </summary>
        public SyncJob SyncJob
        {
            get { return _job; }
        }

        private void SyncEmptyFolders()
        {
            //read metadata of the current folder in file system 
            FolderMetadata currentItems = MetaDataProvider.GenerateFolderMetadata(_job.SyncSource.Path, _job.SyncSource.ID, false);

            //read metadata of the current folder stored in the database
            MetaDataProvider mdProvider = SyncClient.GetMetaDataProvider(_job.IntermediaryStorage.Path, _job.SyncSource.Path);

            FolderMetadata oldCurrentItems = mdProvider.LoadFolderMetadata(_job.SyncSource.ID, SourceOption.SOURCE_ID_EQUALS);
            FolderMetadata otherItems = mdProvider.LoadFolderMetadata( _job.SyncSource.ID, SourceOption.SOURCE_ID_NOT_EQUALS);

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
    }
}
