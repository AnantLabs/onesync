using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class MetadataFileException:Exception 
    {
        public MetadataFileException(string message) : base(message) { }
        public MetadataFileException ():base(){}
    } 
}
