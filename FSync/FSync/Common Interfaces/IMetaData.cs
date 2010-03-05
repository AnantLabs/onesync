using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{ 
    /// <summary>
    /// IMetaData implement IList interface
    /// maintain a list of MetaDataItem
    /// </summary>
    public interface IMetaData       
    {
        IList<IMetaDataItem> MetaDataItems
        {
            set;
            get;
        }
    }
}
