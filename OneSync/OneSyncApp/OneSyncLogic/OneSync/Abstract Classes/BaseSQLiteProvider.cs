/*
 $Id: BaseSQLiteProvider.cs 66 2010-03-10 07:48:55Z gclin009 $
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace OneSync.Synchronization
{
    /// <summary>
    /// Base provider class 
    /// All the other provider will extend this class
    /// </summary>
    public class BaseSQLiteProvider
    {
        //path to meta data folder
        protected string baseFolder = "";
        //name of database file
        protected string DATABASE_NAME = @"\data.md";

        public BaseSQLiteProvider()
        {
        }

        public BaseSQLiteProvider(string baseFolder)
        {
            Base = baseFolder;
        }
        /// <summary>
        /// Get the base class of the provider
        /// </summary>
        public string Base
        {
            set
            {
                this.baseFolder = value;
            }
            get
            {
                return this.baseFolder;
            }

        }
        //1 connection string for all providers 
        # region ConnectionString
        public string ConnectionString
        {
            get
            {
                return string.Format("Version=3,uri=file:{0}", Base + DATABASE_NAME);
            }            
        }
        # endregion ConnectionString

        public string DatabaseName
        {
            get
            {
                return this.DATABASE_NAME;
            }
        }
    }
}
