/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Community.CsharpSqlite.SQLiteClient;
using Community.CsharpSqlite;
using System.IO;

namespace OneSync.Synchronization
{
    public class ProfileProcess
    {      
        public void CreateProfile(string absolutePathToProfileFolder, string profileName, string absoluteSyncPath, string absoluteIntermediatePath )
        {

        }

        public void UpdateProfile(string absolutePathToProfileFolder, Profile profile)
        {
            SQLiteProfileManager profileManager = new SQLiteProfileManager(absolutePathToProfileFolder);
            profileManager.Update(profile);
        }

        public void DeleteProfile(string absolutePathToProfileFolder, string profileId)
        {
            SQLiteProfileManager profileManager = new SQLiteProfileManager(absolutePathToProfileFolder);
            Profile profile = profileManager.Load(profileId);
            if (profileId != null) profileManager.Delete(profile);
        }

        public IList<Profile> GetProfiles(string absolutePathToProfileFolder)
        {
            SQLiteProfileManager profileManager = new SQLiteProfileManager(absolutePathToProfileFolder);
            return profileManager.Load();
        }

        public Profile GetProfileById(string absolutePathToProfileFolder, string id)
        {
            SQLiteProfileManager profileManager = new SQLiteProfileManager(absolutePathToProfileFolder);
            return profileManager.Load(id);
        }


        
    }
}
