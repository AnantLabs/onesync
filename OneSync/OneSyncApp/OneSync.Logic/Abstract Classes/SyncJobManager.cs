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


        // TODO: delete job requires only the profile id?
        /// <summary>
        /// Delete a profile requires delete data from 2 tables SYNCSOURCE_INFO and PROFILE
        /// If deletion action on one table fails, the total action must fail too.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns>true if successful.</returns>
        public abstract bool Delete(SyncJob job);


        /// <summary>
        /// Add new SyncJob
        /// </summary>
        /// <param name="profile">SyncJob to be added</param>
        /// <returns>true if SyncJob is added successfully.</returns>
        /// <exception cref="ProfileNameExistException">SyncJob with same name already exists.</exception>
        public abstract bool Add(SyncJob job);

        /// <summary>
        /// Determines whether a profile with specified name already exists.
        /// </summary>
        /// <param name="jobName"></param>
        /// <returns>true if SyncJob with specified name already exists.</returns>
        public abstract bool SyncJobExists(string jobName);

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


        #region Static Helper Methods
        
        /// <summary>
        /// Returns SyncJobs that is associated with specified path as its synchronization source.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="jobs"></param>
        /// <returns></returns>
        public static IList<SyncJob> FindByDataSource(string path, IList<SyncJob> jobs)
        {
            IEnumerable<SyncJob> results = from j in jobs
                                           where j.SyncSource.Path.Equals(path)
                                           select j;
            return results.ToList();
        }


        /// <summary>
        /// Returns SyncJobs with specified id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="profiles"></param>
        /// <returns></returns>
        public static SyncJob FindBySyncJobId(string id, IList<SyncJob> jobs)
        {
            // TODO: Sufficient to stop search when profile is found?
            IEnumerable<SyncJob> results = from j in jobs
                                           where j.ID.Equals(id)
                                           select j;
            IList<SyncJob> pList = results.ToList();
            if (pList.Count == 1) return pList[0];
            return null;
        }

        #endregion


        


      
    }
}
