/*
 $Id: Configuration.cs 644 2010-04-15 03:46:01Z thuatvng $
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
        public const string TBL_DATASOURCE_INFO = "DATASOURCE_INFO_TABLE";
        public const string TBL_METADATA = "METADATA_TABLE";
        public const string TLB_FOLDERMETADATA = "FOLDER_TABLE";

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

        // TLB_FOLDERMETADATA columns
        public const string COL_FOLDER_RELATIVE_PATH = "RELATIVE_PATH";
        public const string COL_IS_FOLDER_EMPTY = "IS_FOLDER_EMPTY";
    }
}
