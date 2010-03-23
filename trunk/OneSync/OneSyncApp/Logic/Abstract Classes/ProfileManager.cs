/*
 $Id: IProfileManager.cs 66 2010-03-10 07:48:55Z gclin009 $
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    /* Note:
     * Original class is IProfileManager interface but changed it to
     * abstract class since ProfileManager is a more of a 'noun' more
     * than describing a behaviour
     */

    /// <summary>
    /// Class to manage Profile.
    /// </summary>
    public abstract class ProfileManager
    {
        // Path to where profiles are saved.
        protected string _profileStoragePath;

        
        /// <summary>
        /// Creates a profile manager.
        /// </summary>
        /// <param name="storagePath">Location where all profiles are saved to or loaded from.</param>
        public ProfileManager(string storagePath)
        {
            _profileStoragePath = storagePath;
        }


        /// <summary>
        /// Load all profiles that is saved.
        /// </summary>
        /// <returns></returns>
        public abstract IList<Profile> LoadAllProfiles();


        /// <summary>
        /// Load profile with specified profile id.
        /// </summary>
        /// <param name="profileId">Id of profile to load</param>
        /// <returns>Profile with specified id. null if profile not found.</returns>
        public abstract Profile Load(string profileId);

        /// <summary>
        /// Create a new profile. Newly created profile will be saved
        /// </summary>
        /// <param name="profileName">Name of profile.</param>
        /// <param name="absoluteSyncPath">Absolute path to folder which is to be synchronized.</param>
        /// <param name="absoluteIntermediatePath">Absolute path to intermediary storage location used for synchronization.</param>
        /// <returns>Newly create Profile. null if Profile cannot be created.</returns>
        public abstract Profile CreateProfile(string profileName, string absoluteSyncPath, string absoluteIntermediatePath);


        /// <summary>
        /// Update a profile requires update 2 tables at the same time, 
        /// If one update on a table fails, the total update action must fail too.
        /// </summary>
        /// <param name="profile"></param>
        public abstract bool Update(Profile profile);


        // TODO: delete profile requires only the profile id?
        /// <summary>
        /// Delete a profile requires delete data from 2 tables SYNCSOURCE_INFO and PROFILE
        /// If deletion action on one table fails, the total action must fail too.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns>true if successful.</returns>
        public abstract bool Delete(Profile profile);


        /// <summary>
        /// Add new profile
        /// </summary>
        /// <param name="profile">Profile to be added</param>
        /// <returns>true if profile is added successfully.</returns>
        /// <exception cref="ProfileNameExistException">Profile with same name already exists.</exception>
        public abstract bool Add(Profile profile);

        /// <summary>
        /// Determines whether a profile with specified name already exists.
        /// </summary>
        /// <param name="profileName"></param>
        /// <returns>true if profile with specified name already exists.</returns>
        public abstract bool ProfileExists(string profileName);

        public abstract void CreateSchema();
        #region Public Properties

        /// <summary>
        /// Location where all profiles are saved to or loaded from 
        /// </summary>
        public string StoragePath
        {
            get { return _profileStoragePath; }
        }

        #endregion


        #region Static Helper Methods
        
        /// <summary>
        /// Returns profiles that is associated with specified path as its synchronization source.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="profiles"></param>
        /// <returns></returns>
        public static IList<Profile> FindByDataSource(string path, IList<Profile> profiles)
        {
            IEnumerable<Profile> results = from profile in profiles
                                           where profile.SyncSource.Path.Equals(path)
                                           select profile;
            return results.ToList();
        }


        /// <summary>
        /// Returns profiles with specified id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="profiles"></param>
        /// <returns></returns>
        public static Profile FindByProfileId(string id, IList<Profile> profiles)
        {
            // TODO: Sufficient to stop search when profile is found?
            IEnumerable<Profile> results = from profile in profiles
                                           where profile.ID.Equals(id)
                                           select profile;
            IList<Profile> pList = results.ToList();
            if (pList.Count == 1) return pList[0];
            return null;
        }

        #endregion


        


      
    }
}
