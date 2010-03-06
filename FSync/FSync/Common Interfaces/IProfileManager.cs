using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    /// <summary>
    /// Define common behaviors for profile manager
    /// Instance of this class is in charge of managing profile records 
    /// </summary>
    public interface IProfileManager
    {
        IList<Profile> Load();

        void Update(Profile profile);

        void Delete(Profile profile);

        void Insert(Profile profile);
    }
}
