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
    public delegate void SyncCompletedHandler(object sender, SyncCompletedEventArgs eventArgs);
    public delegate void SyncCancelledHandler(object sender, SyncCancelledEventArgs eventArgs);
    public delegate void SyncStatusChangedHandler(object sender, SyncStatusChangedEventArgs args);
    public delegate void SyncProgressChangedHandler(object sender, SyncProgressChangedEventArgs args);
    public delegate void SyncFileChangedHandler(object sender, SyncFileChangedEventArgs args);

    public enum SyncStatus
    {
        VERIFY_PATCH,
        APPLY_PATCH,
        GENERATE_PATCH,
        UPDATE_DATA
    }

    /// <summary>
    /// 
    /// </summary>    
    public class FileSyncAgent : BaseSyncAgent
    {
        public event SyncCompletedHandler SyncCompleted;
        public event SyncStatusChangedHandler StatusChanged;
        public event SyncProgressChangedHandler ProgressChanged;
        public event SyncFileChangedHandler SyncFileChanged;
        
        private List<SyncAction> actions = new List<SyncAction>();
        private SyncJob _job;

        public FileSyncAgent()
            : base()
        {
        }

        public FileSyncAgent(SyncJob job)
            : base(job)
        {
            this._job = job;
            SyncActionsProvider actProvider = SyncClient.GetSyncActionsProvider(job.IntermediaryStorage.Path);
            actions = (List<SyncAction>)actProvider.Load(job.SyncSource.ID, SourceOption.SOURCE_ID_NOT_EQUALS);
        }

        public void Synchronize(SyncPreviewResult preview)
        {
            if (ProgressChanged != null) ProgressChanged(this, new SyncProgressChangedEventArgs(0));
            Apply(preview);
            if (ProgressChanged != null) ProgressChanged(this, new SyncProgressChangedEventArgs(50));
            Generate();
            if (ProgressChanged != null) ProgressChanged(this, new SyncProgressChangedEventArgs(100));
            if (SyncCompleted != null) SyncCompleted(this, new SyncCompletedEventArgs());
        }

        public SyncPreviewResult GenerateSyncPreview()
        {
            //Generate current folder's metadata
            FileMetaData currentItems = MetaDataProvider.Generate(profile.SyncSource.Path, profile.SyncSource.ID);

            //Load current folder metadata from database
            MetaDataProvider mdProvider = SyncClient.GetMetaDataProvider(profile.IntermediaryStorage.Path, profile.SyncSource.ID);
            FileMetaData oldCurrentItems = mdProvider.Load(profile.SyncSource.ID, SourceOption.SOURCE_ID_EQUALS);

            //Load the other folder metadata from database
            FileMetaData oldOtherItems = mdProvider.Load(profile.SyncSource.ID, SourceOption.SOURCE_ID_NOT_EQUALS);

            SyncPreviewResult result = new SyncPreviewResult(actions, currentItems, oldCurrentItems, oldOtherItems);
            
            return result;
        }

        /// <summary>
        /// Carry out the sync actions
        /// </summary>
        /// <returns></returns>
        public SyncResult Apply(SyncPreviewResult previewResult)
        {
            // Logging
            List<LogActivity> applyActivities = new List<LogActivity>();            
            DateTime starttime = DateTime.Now;

            SyncResult syncResult = new SyncResult();
            foreach (SyncAction action in previewResult.ItemsToCopyOver)
            {
                if (action.Skip)
                {
                    //add log
                    continue ;
                }
               if (SyncFileChanged != null) SyncFileChanged(this, new SyncFileChangedEventArgs(ChangeType.NEWLY_CREATED, action.RelativeFilePath));
               try
               {
                   try { SyncExecutor.CopyToSyncFolderAndUpdateActionTable((CreateAction)action, profile);}
                   catch (Exception) {applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), "FAIL"));}                   
                   syncResult.Ok.Add(action);
                   applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), "SUCCESS"));                   
                }
                catch (Exception){Console.WriteLine("Logging error");}                
            }

            foreach (SyncAction action in previewResult.ItemsToDelete)
            {
                if (action.Skip)
                {
                    //add log
                    continue;
                }
                if (SyncFileChanged != null) SyncFileChanged(this, new SyncFileChangedEventArgs(ChangeType.DELETED, action.RelativeFilePath));
                try
                {
                    try
                    {SyncExecutor.DeleteInSyncFolderAndUpdateActionTable((DeleteAction)action, profile);}
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

            foreach (SyncAction action in previewResult.ConflictItems)
            {
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
                        {SyncExecutor.DuplicateRenameToSyncFolderAndUpdateActionTable((CreateAction)action, profile);}
                        catch (Exception)
                        {syncResult.Errors.Add(action);
                            applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), action.ConflictResolution.ToString() + "_FAIL"));
                        }                        
                        syncResult.Ok.Add(action);
                        applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), action.ConflictResolution.ToString() + "_SUCCESS"));
                    }
                    catch (Exception)
                    {Console.WriteLine("Logging error");}                  
                }
                else if (action.ConflictResolution == ConflictResolution.OVERWRITE)
                {
                    if (action.Skip)
                    {
                        continue;
                    }
                    if (SyncFileChanged != null) SyncFileChanged(this, new SyncFileChangedEventArgs(ChangeType.MODIFIED, action.RelativeFilePath));
                    try
                    {
                        try
                        {SyncExecutor.CopyToSyncFolderAndUpdateActionTable((CreateAction)action, profile);}
                        catch (Exception)
                        {
                            syncResult.Errors.Add(action);
                            applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), action.ConflictResolution.ToString() + "_FAIL"));
                        }                        
                        syncResult.Ok.Add(action);
                        applyActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), action.ConflictResolution.ToString() + "_SUCCESS"));
                    }
                    catch (Exception)
                    {Console.WriteLine("Logging error");}                    
                }
            }

            // Add to log
            Log.addToLog(profile.SyncSource.Path, profile.IntermediaryStorage.Path,
                    profile.Name, applyActivities, Log.from, applyActivities.Count, starttime, DateTime.Now);

            return syncResult;
            // TODO:
            // Update metadata after patch is applied
            // Delete all dirty files
        }

        public void Generate()
        {
            //read metadata of the current folder in file system 
            FileMetaData currentItems = MetaDataProvider.Generate(profile.SyncSource.Path, profile.SyncSource.ID);

            //read metadata of the current folder stored in the database
            MetaDataProvider mdProvider = SyncClient.GetMetaDataProvider(profile.IntermediaryStorage.Path, profile.SyncSource.Path);
            FileMetaData storedItems = mdProvider.Load(profile.SyncSource.ID, SourceOption.SOURCE_ID_EQUALS);


            //Update metadata 
            mdProvider.Update(storedItems, currentItems);

            storedItems = mdProvider.Load(profile.SyncSource.ID, SourceOption.SOURCE_ID_NOT_EQUALS);
            FileMetaDataComparer actionGenerator = new FileMetaDataComparer(currentItems, storedItems);

            //generate list of sync actions by comparing 2 metadata

            IList<SyncAction> newActions = actionGenerator.Generate();

            //delete actions of previous sync
            SyncActionsProvider actProvider = SyncClient.GetSyncActionsProvider(profile.IntermediaryStorage.Path);
            actProvider.Delete(profile.SyncSource.ID, SourceOption.SOURCE_ID_EQUALS);

            // Logging
            List<LogActivity> generateActivities = new List<LogActivity>();
            int count = 0;
            DateTime starttime = DateTime.Now;

            if (Directory.Exists(profile.IntermediaryStorage.DirtyFolderPath)) Directory.Delete(profile.IntermediaryStorage.DirtyFolderPath, true);
            foreach (SyncAction action in newActions)
            {
                try
                {
                    if (action.ChangeType == ChangeType.NEWLY_CREATED)
                    {
                        SyncExecutor.CopyToDirtyFolderAndUpdateActionTable(action, profile);
                    }
                    else if (action.ChangeType == ChangeType.DELETED || action.ChangeType == ChangeType.RENAMED)
                    {                        
                        actProvider.Add(action);
                    }

                    generateActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), "SUCCESS"));
                }
                catch (Exception)
                {
                    generateActivities.Add(new LogActivity(action.RelativeFilePath, action.ChangeType.ToString(), "FAIL"));
                }

                count++;
            }

            // Add to log
            Log.addToLog(profile.SyncSource.Path, profile.IntermediaryStorage.Path,
                profile.Name, generateActivities, Log.to, count, starttime, DateTime.Now);
        }

        /// <summary>
        /// Gets the SyncJob associated with this FileSyncAgent
        /// </summary>
        public SyncJob SyncJob
        {
            get { return _job; }
        }

       
    }
}
