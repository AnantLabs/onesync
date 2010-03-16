/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class Configuration
    {
        public static string METADATA_RELATIVE_PATH = @"\data.md";
        public static string DIRTY_FOLDER_RELATIVE_PATH = @"\files";


        public const string DATABASE_NAME = "data.md";

        /***************
         * Table Names
         ***************/
        public const string TBL_ACTION = "ACTION_TABLE";
        public const string TBL_SYNCSOURCE_INFO = "SYNCSOURCE_INFO_TABLE";
        public const string TBL_METADATA = "METADATA_TABLE";
        public const string TBL_PROFILE = "PROFILE";


        /***************
         * Column Names
         ***************/
        // TBL_DATASOURCE_INFO Columns
        public const string COL_SOURCE_ABSOLUTE_PATH = "SOURCE_ABSOLUTE_PATH";
        public const string COL_SOURCE_ID = "SOURCE_ID";

        // TBL_ACTION Columns
        public const string COL_ACTION_ID = "ACTION_ID";
        public const string COL_CHANGE_IN = "CHANGE_IN";
        public const string COL_ACTION_TYPE = "ACTION_TYPE";
        public const string COL_OLD_RELATIVE_PATH = "OLD_RELATIVE_PATH";
        public const string COL_NEW_RELATIVE_PATH = "NEW_RELATIVE_PATH";
        public const string COL_OLD_HASH = "OLD_HASH";
        public const string COL_NEW_HASH = "NEW_HASH";

        // TBL_METADATA Columns
        public const string COL_RELATIVE_PATH = "RELATIVE_PATH";
        public const string COL_HASH_CODE = "HASH_CODE";
        public const string COL_LAST_MODIFIED_TIME = "LAST_MODIFIED_TIME";
        public const string COL_NTFS_ID1 = "NTFS_ID1";
        public const string COL_NTFS_ID2 = "NTFS_ID2";

        // TBL_PROFILE Columns
        public const string COL_PROFILE_ID = "PROFILE_ID";
        public const string COL_PROFILE_NAME = "PROFILE_NAME";
        public const string COL_METADATA_SOURCE_LOCATION = "METADATA_SOURCE_LOCATION";
        public const string COL_SYNC_SOURCE_ID = "SYNC_SOURCE_ID";
        public const string COL_SOURCE_ABS_PATH = "SOURCE_ABSOLUTE_PATH";
        //public const string COL_SOURCE_ID = "SOURCE_ID";
    }
}
