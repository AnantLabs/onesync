/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Community.CsharpSqlite.SQLiteClient;
using Community.CsharpSqlite;


namespace OneSync.Synchronization
{


    public class Patch
    {


        /// <summary>
        /// Creates a new Patch.
        /// </summary>
        /// <param name="syncSource">SyncSource of other PC where patch is to be applied.</param>
        /// <param name="iStorage">Information about intermediary storage</param>
        /// <param name="actions">Actions to be contained in this patch.</param>
        public Patch(Profile profile)
        {
            //this.profile = profile;

        }



        /// <summary>
        /// Verify whether all the files contained in this patch are valid by
        /// checking whether it exists and optionally its file hash is the same.
        /// </summary>
        /// <param name="checkHash">Computes hash of files and verify it.</param>
        /// <returns>True if all the files are valid.</returns>
        /// 
        /*
        public bool Verify(bool checkHash)
        {
            // Root directory where all dirty files are stored
            string rootDir = profile.IntermediaryStorage.DirtyFolderPath;
            foreach (SyncAction a in actions)
            {
                // Skip Delete Action as there is no need to check for
                // if file exists, neither is there a need to compute hash.
                if (a is DeleteAction) continue;

                string filePath = Path.Combine(rootDir, a.RelativeFilePath);

                bool result = File.Exists(filePath);

                // Check whether file hash tally
                if (checkHash)
                    result &= Files.FileUtils.GetFileHash(filePath).Equals(a.FileHash);

                if (result == false) return false;
            }
            return true;
        }*/




    }
}
