/*
 $Id: IntermediaryStorage.cs 66 2010-03-10 07:48:55Z gclin009 $
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    /// <summary>
    /// Encapsulates information about intermediary storage used
    /// for synchronization such as its absolute path as well as
    /// where the dirty files are stored.
    /// </summary>
    public class IntermediaryStorage
    {
        private string path = "";
        
        public IntermediaryStorage(string path)
        {
            this.path = path;            
        }
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
            }
        }

        public string DirtyFolderPath
        {
            get { return this.path + @"\files"; }
        }
    }
}
