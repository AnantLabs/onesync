/*
 $Id: SyncJobManager.cs 66 2010-03-10 07:48:55Z gclin009 $
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{

    /// <summary>
    /// Class to manage SyncJob.
    /// </summary>
    public abstract class SyncJobManager
    {
        // Path to where SyncJobs are saved.
        protected string _jobsStoragePath;

        
        /// <summary>
        /// Creates a SyncJob manager.
        /// </summary>
        /// <param name="storagePath">Location where all SyncJobs are saved to or loaded from.</param>
        public SyncJobManager(string storagePath)
        {
            _jobsStoragePath = storagePath;
        }


        /// <summary>
        /// Load all SyncJobs that is saved.
        /// </summary>
        /// <returns></returns>
        public abstract IList<SyncJob> LoadAllJobs();


        /// <summary>
        /// Load SyncJob with specified name.
        /// </summary>
        /// <param name="jobId">Name of SyncJob</param>
        /// <returns>SyncJob with specified name. null if SyncJob not found.</returns>
        public abstract SyncJob Load(string jobName);

        /// <summary>
        /// Create a new SyncJob. Newly created SyncJob will be saved
        /// </summary>
        /// <param name="jobName">Name of SyncJob.</param>
        /// <param name="absoluteSyncPath">Absolute path to folder which is to be synchronized.</param>
        /// <param name="absoluteIntermediatePath">Absolute path to intermediary storage location used for synchronization.</param>
        /// <returns>Newly create SyncJob. null if SyncJob cannot be created.</returns>
        public abstract SyncJob CreateSyncJob(string jobName, string absoluteSyncPath, string absoluteIntermediatePath);


        /// <summary>
        /// Update a SyncJob requires update 2 tables at the same time, 
        /// If one update on a table fails, the total update action must fail too.
        /// </summary>
        /// <param name="job"></param>
        public abstract bool Update(SyncJob job);


        /// <summary>
        /// Delete a SyncJob requires delete data from 2 tables SYNCSOURCE_INFO and SYNCJOB
        /// If deletion action on one table fails, the total action must fail too.
        /// </summary>
        /// <param name="job"></param>
        /// <returns>true if successful.</returns>
        public abstract bool Delete(SyncJob job);


        /// <summary>
        /// Add new SyncJob
        /// </summary>
        /// <param name="job">SyncJob to be added</param>
        /// <returns>true if SyncJob is added successfully.</returns>
        /// <exception cref="SyncJobNameExistException">SyncJob with same name already exists.</exception>
        public abstract bool Add(SyncJob job);
        
        public abstract void CreateSchema();
        #region Public Properties

        /// <summary>
        /// Location where all SyncJobs are saved to or loaded from 
        /// </summary>
        public string StoragePath
        {
            get { return _jobsStoragePath; }
        }

        #endregion


        


      
    }
}
