/*
 $Id: SyncClient.cs 255 2010-03-17 16:08:53Z gclin009 $
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
    /// <summary>
    /// TODO...
    /// </summary>
    public static class SyncClient
    {

        /// <summary>
        /// Gets a new ProfileManager instance.
        /// </summary>
        /// <param name="storagePath">Location where all profiles are saved to or loaded from.</param>
        /// <returns></returns>
        public static ProfileManager GetProfileManager(string storagePath)
        {
            // Returns any objects derived from ProfileManager abstract class.
            return new SQLiteProfileManager(storagePath);
        }    
      

        /// <summary>
        /// Gets a new MetaDataProvider instance.
        /// </summary>
        /// <param name="storagePath">Location where all metadata are saved to or loaded from.</param>
        /// <param name="rootPath">To be combined with relative paths to give absolute paths of files.</param>
        /// <returns></returns>
        public static MetaDataProvider GetMetaDataProvider(string storagePath, string rootPath)
        {
            return new SQLiteMetaDataProvider(storagePath, rootPath);
        }


        /// <summary>
        /// Gets a new SyncSourceProvider instance.
        /// </summary>
        /// <param name="storagePath">Location where all SyncSource information are saved to or loaded from.</param>
        /// <returns></returns>
        public static SyncSourceProvider GetSyncSourceProvider(string storagePath)
        {
            return new SQLiteSyncSourceProvider(storagePath);
        }


        /// <summary>
        /// Gets a new SyncActionsProvider instance.
        /// </summary>
        /// <param name="storagePath">Location where all SyncActions are saved to or loaded from.</param>
        /// <returns></returns>
        public static SQLiteSyncActionsProvider GetSyncActionsProvider(string storagePath)
        {
            return new SQLiteSyncActionsProvider(storagePath);
        }       

        
    }
}
