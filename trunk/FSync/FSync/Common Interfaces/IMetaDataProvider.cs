using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{   
    //defines a common interface for metadata provider (file, database,...)
    public interface IMetaDataProvider
    {        
        void Insert(FileMetaData metadata);
        void Update(FileMetaData metadata);
        FileMetaData Load(SyncSource source);
    }
}
