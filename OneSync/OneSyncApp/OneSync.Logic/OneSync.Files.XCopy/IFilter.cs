/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Files.XCopy
{
    public interface IFilter

    {
        bool CopyAll
        {
            set;
            get;
        }

        bool NoCopy
        {
            set;
            get;
        }

        bool CopyNonEmptyDirectories
        {
            set;
            get;
        }

        bool CopyAllDirectories
        {
            get;
            set;
        }

        bool RestartableMode
        {
            get;
            set;
        }

        bool BackupMode
        {
            get;
            set;
        }

        void SetCopyFlags(string flags);
        bool SecurityEnabled
        {
            get;
            set;
        }

        bool MovEnabled
        {
            set;
            get;
        }

        bool MoveEnabled
        {
            set;
            get;
        }

        bool PurgeEnabled
        {
            set;
            get;
        }

        bool MirrorEnabled
        {
            set;    
            get;
        }

        bool ListFilesOnly
        {
            set;
            get;
        }


        void SetAttributesForCopiedFiles (string attributes);

        void RemoveAttributesForCopiedFiles (string attributes);

        void IncludeFilesWithAttributes(string attributes);

        void ExcludeFilesWithAttributes(string attributes);

        void ExcludeFilesGreaterThan(int bytes);

        void ExcludeFilesSmallerThan(int bytes);

        void ExcludeFilesLADOlderThan(int nDays);

        void ExcludeFilesLADOlderThan(string day);

        void ExcludeFilesLADNewerThan(int nDays);

        void ExcludeFilesLADNewerThan(string day);        

        void ExcludeFilesLMDOlderThan(int nDays);

        void ExcludeFilesLMDOlderThan(string day);

        void ExcludeFilesLMDNewerThan(int nDays);

        void ExcludeFilesLMDNewerThan(string day);

        void SetMaxRetry(int times);

        void SetMaxWaitInterval(int seconds);

          

    }
}
