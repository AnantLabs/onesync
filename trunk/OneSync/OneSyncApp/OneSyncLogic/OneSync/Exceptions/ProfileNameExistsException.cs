using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class ProfileNameExistException:ApplicationException
    {
        public ProfileNameExistException(string message):base(message)
        {
        }
        public ProfileNameExistException() :base(){ }
    }
}
