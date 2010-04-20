//Coded by Nguyen van Thuat
/*
 $Id$
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
        public IntermediaryStorage(string path)
        {
            this.Path = path;            
        }
        
        public string Path { get; set; }

        public string DirtyFolderPath
        {
            get { return this.Path + @"\files"; }
        }
    }
}
