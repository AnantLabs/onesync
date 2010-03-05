using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class MetaDataSource
    {
        private string path = "";
        
        public MetaDataSource(string path)
        {
            this.path = path;            
        }
        public string Path
        {
            get
            {
                return path;
            }
        }

        public string DirtyFolderPath
        {
            get { return this.path + @"\files"; }
        }
    }
}
