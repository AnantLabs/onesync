/*
 $Id: ProfileNameExistsException.cs 306 2010-03-23 10:34:17Z deskohet $
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    public class ProfileNameExistException:ApplicationException
    {
        // SyncSource associated with Sync Job conflicted name.
        private SyncSource s;

        public ProfileNameExistException(string message):base(message)
        {
        }

        public ProfileNameExistException() :base(){ }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s">SyncSource associated with Sync Job conflicted name.</param>
        public ProfileNameExistException(SyncSource s) : base() 
        {
            this.s = s;
        }

        public SyncSource SyncSource
        {
            get { return s; }
        }
    }
}
