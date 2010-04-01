/*
 $Id: JobDetails.cs 66 2010-03-10 07:48:55Z gclin009 $
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Files.XCopy
{
    public class JobDetails
    {
        private string source, destination;
        IFilter filter;
        public JobDetails(string source, string destination, IFilter filter)
        {
            this.source = source;
            this.destination = destination;
            this.filter = filter;
        }

        public string Source
        {
            set
            {
                this.source = value;
            }
            get
            {
                return this.source;
            }
        }

        public string Destination
        {
            set
            {
                this.destination = value;
            }
            get
            {
                return this.destination;
            }
        }

        public IFilter Filter
        {
            set
            {
                this.filter = value ;
            }
            get
            {
                return this.filter;
            }
            
        }
    }
}
