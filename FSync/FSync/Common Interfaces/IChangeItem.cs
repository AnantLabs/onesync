using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public enum ChangeType
    {        
        NEWLY_CREATED ,
        DELETED,
        MODIFIED,
        RENAMED,
        NO_CHANGED
    }

     
    public interface IChangeItem
    {
        /// <summary>
        /// Change Type (create, delete,...)
        /// </summary>
        ChangeType ItemChangeType
        {
            get;
        }     
    }
}
