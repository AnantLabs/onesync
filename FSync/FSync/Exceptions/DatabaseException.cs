using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class DatabaseException:Exception
    {
        public DatabaseException() : base() { }
        public DatabaseException(string message) : base(message) { }
    }
}
