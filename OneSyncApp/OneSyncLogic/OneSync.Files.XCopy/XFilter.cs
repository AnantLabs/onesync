/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Files.XCopy
{
    public class XFilter: IFilter
    {
        public XFilter()
        {
        }
       #region IFilter Members

        public bool CopyAll
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool NoCopy
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool CopyNonEmptyDirectories
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool CopyAllDirectories
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool RestartableMode
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool BackupMode
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void SetCopyFlags(string flags)
        {
            throw new NotImplementedException();
        }

        public bool SecurityEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool MovEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

       public bool MoveEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool PurgeEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool MirrorEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool ListFilesOnly
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void SetAttributesForCopiedFiles(string attributes)
        {
            throw new NotImplementedException();
        }

        public void RemoveAttributesForCopiedFiles(string attributes)
        {
            throw new NotImplementedException();
        }

        public void IncludeFilesWithAttributes(string attributes)
        {
            throw new NotImplementedException();
        }

        public void ExcludeFilesWithAttributes(string attributes)
        {
            throw new NotImplementedException();
        }

        public void ExcludeFilesGreaterThan(int bytes)
        {
            throw new NotImplementedException();
        }

        public void ExcludeFilesSmallerThan(int bytes)
        {
            throw new NotImplementedException();
        }

        public void ExcludeFilesLADOlderThan(int nDays)
        {
            throw new NotImplementedException();
        }

        public void ExcludeFilesLADOlderThan(string day)
        {
            throw new NotImplementedException();
        }

        public void ExcludeFilesLADNewerThan(int nDays)
        {
            throw new NotImplementedException();
        }

        public void ExcludeFilesLADNewerThan(string day)
        {
            throw new NotImplementedException();
        }

        public void ExcludeFilesLMDOlderThan(int nDays)
        {
            throw new NotImplementedException();
        }

        public void ExcludeFilesLMDOlderThan(string day)
        {
            throw new NotImplementedException();
        }

        public void ExcludeFilesLMDNewerThan(int nDays)
        {
            throw new NotImplementedException();
        }

        public void ExcludeFilesLMDNewerThan(string day)
        {
            throw new NotImplementedException();
        }

        public void SetMaxRetry(int times)
        {
            throw new NotImplementedException();
        }

        public void SetMaxWaitInterval(int seconds)
        {
            throw new NotImplementedException();
        }

        #endregion

        
    }
}
