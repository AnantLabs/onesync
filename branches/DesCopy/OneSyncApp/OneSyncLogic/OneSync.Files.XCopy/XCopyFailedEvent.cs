/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Files.XCopy
{
    public class XCopyFailedEvent: EventArgs 
    {
        private string message = "";
        private JobDetails copyJob;
        public XCopyFailedEvent(string message):base()
        { 
            this.message = message;
        }

        public JobDetails JobDetails
        {
            set
            {
                this.copyJob = value ;
            }
            get
            {
                return this.copyJob;
            }
        }

        public XCopyFailedEvent()
            : base()
        {

        }

        public string Message
        {
            get
            {
                return this.message;
            }
        }

        
    }
}
