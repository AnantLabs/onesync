/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class MetaDataAlreadyExistsException:ApplicationException
    {
        public MetaDataAlreadyExistsException(string message)
            : base(message)
        {
        }

        public MetaDataAlreadyExistsException() : base() { }
    }
}
