/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class SyncSourcesNumberExceededException:ApplicationException
    {
        public SyncSourcesNumberExceededException(string message) : base(message) { }      
        public SyncSourcesNumberExceededException() : base() { }
    }
}
