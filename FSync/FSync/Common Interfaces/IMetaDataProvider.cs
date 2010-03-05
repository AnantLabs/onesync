using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{   
    //defines a common interface for metadata provider (file, database,...)
    public interface IMetaDataProvider
    {        
        void Insert(IMetaData metadata);
        void Update(IMetaData metadata);
        IMetaData Load(SyncSource source);        
    }
}
