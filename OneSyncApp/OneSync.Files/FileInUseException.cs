using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Files
{
    public class FileInUseException: ApplicationException
    {
        public FileInUseException(string message)
            : base(message)
        {

        }
        public FileInUseException()
            : base()
        {
        }
    }
}
