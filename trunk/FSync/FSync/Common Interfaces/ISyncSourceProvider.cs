using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    /// <summary>
    /// Defines common behavior of classes implement this interface
    /// Instance of this class is in charge of managing sync source records
    /// </summary>
    public interface ISyncSourceProvider
    {
        void Insert(SyncSource source);
        IList<SyncSource> Load();
        void Update(SyncSource source);
        void Delete(SyncSource source);        
    }
}
