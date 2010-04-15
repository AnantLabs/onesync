/*
 $Id: ProfileNameExistsException.cs 595 2010-04-14 06:47:14Z deskohet $
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class SyncJobNameExistException:ApplicationException
    {
        // SyncSource associated with Sync Job conflicted name.
        private SyncSource s;

        public SyncJobNameExistException(string message):base(message)
        {
        }

        public SyncJobNameExistException() :base(){ }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s">SyncSource associated with Sync Job conflicted name.</param>
        public SyncJobNameExistException(SyncSource s) : base() 
        {
            this.s = s;
        }

        public SyncSource SyncSource
        {
            get { return s; }
        }
    }
}
