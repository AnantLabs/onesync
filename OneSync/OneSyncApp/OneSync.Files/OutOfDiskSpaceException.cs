using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Files
{

    public class OutOfDiskSpaceException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public OutOfDiskSpaceException() { }
        public OutOfDiskSpaceException(string message) : base(message) { }
        public OutOfDiskSpaceException(string message, Exception inner) : base(message, inner) { }
    }
}
