/*
 $Id$
 */
/*
 * 1. If a file exists in both source and destination locations, by default robocopy copies the file
 * only if the two versions have different timestamps or different size.
 * 
 * 
 * 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;


namespace OneSync.Files.XCopy
{
    public class XCopy: ICopy
    {
        #region declaration
        //mirror directory tree. Equivalent to /E and /PURGE
        private const string MIRROR_MODE = " /MIR";

        private const string COPY_IN_RESTARTABLE_MODE = " /Z";

        /// <summary>
        /// Copy folder structures and files contained recursively
        /// </summary>
        private const string COPY_RECURSIVELY = " /E";

        /*The use of /COPY:[D][A][T][S][O][U] specifies exactly what to copy
         * D: Data, A: Atributes, T: Timestamps, S: NTFS Security,O: NTFS ownership
         * U: Auditing information.
         */
        private const string COPY = " /COPY";
        
        //Equlivalent to /COPY:DATSOU
        //Default copy is /COPY:DAT
        private const string COPYAll = " /COPYALL";

        //copy nothing. Useful with /PURGE
        private const string NOCOPY = " /NOCOPY";

        //copy files in backup mode
        //must have backup and restore file priviledges to copy files
        private const string COPY_FILES_IN_BACKUP_MODE = " /B";

        //copy files in restartable mode for greater resiliency
        //automatically switch to backup mode if the restartable copy fails with an "Access Denied" error.
        //must have backup and restore file priviledges to copy files
        private const string COPY_FILES_IN_RESTARTABLE_MODE = " /ZB";

        //monitor the source directory tree for changes and copy changes when they occur.
        //specify mininum number of changes that must occurs before running again
        private const string MIN_CHANGES = " /MON";

        //specify mininum number of time that must occurs before running again 
        private const string MIN_INTERVAL = " /MOT";



        private const string SKIP_JUNCTION = " /XJ";
        private const string EXCLUDE_FILES_LAD_OLDER_THAN = " /MAXLAD";
        private const string EXCLUDE_FILES_LAD_NEWER_THAN = " /MINLAD";
       

        //Copy NTFS security information. Source and destination volumes must both be NTFS.
        //Equlivalent to /COPY:DATS.
        private const string SECURITY = " /SEC";

        //move files to destination and delete at sources after moving. 
        private const string MOV_FILES = " /MOV";

        //move files and directories to destination and delete at source after moving.
        private const string MOVE = " /MOVE";

        //delete files and directories that no longer exists in the source
        private const string PURGE = " /PURGE";

        //A+:{R|A|S|H|N|T}. Set attributes for the copied files
        //R:readonly, S; System, N: Not content indexed, A: Archive, H: Hidden, T: Temporary
        private const string SET_FILES_ATTRIBUTES = " /A+:";

        ///A-:{R|A|S|H|N|T}. Turn off the attributes of the copied files
        private const string REMOVE_FILES_ATTRIBUTES = " /A-:";

        //Create a directory tree structure containing zero-length files only
        //no file data is copied.
        private const string CREATE = " /CREATE";

        ///Include files with attributes
        /// IA:{R|A|S|H|C|N|E|T|O}
        private const string INCLUDE_FILES_WITH_ATTRIBUTES = " /IA:";

        ///XA:{R|A|S|H|C|N|E|T|O}
        ///Exclude files with attributes 
        private const string EXCLUDE_FILES_WITH_ATTRIBUTES = " /XA:";

        private const string COPY_ARCHIVE_ONLY = " /A";
        /// <summary>
        /// copy files with the archive attribute and turn off the attribute at source
        /// </summary>
        private const string COPY_RESET_ARCHIVE_AT_SOURCE = " /M";
	
        /// <summary>
        /// Exclude files with specified names, paths, or wildcards characters
        /// </summary>
        private const string EXCLUDE_FILES = " /XF";

        /// <summary>
        /// Exclude directories with specified names or wildcards characters
        /// </summary>

        private const string EXCLUDE_DIRECTORIES = " /XD";

        /// <summary>
        /// Exclude files tagged as "Changed"       
        /// </summary>

        private const string EXCLUDE_CHANGED_FILES = " /XC";

        /// <summary>
        /// Exclude files tagged as newer
        /// </summary>
        /// 
        private const string EXCLUDE_NEWER_FILES = " /XN";

        /// <summary>
        /// Exclude files tagged as older
        /// </summary>
        /// 
        private const string EXCLUDED_OLDER_FILES = " /XO";

        /// <summary>
        /// Exclude files and directories tagged as extra
        /// </summary>
        private const string EXCLUDE_EXTRA = " /XX";


        /// <summary>
        /// Exclude files and directories tagged as "Lonely"
        /// </summary>
        private const string EXCLUDE_LONELY = " /XL";

        /// <summary>
        /// exclude files tagged as "Same"
        /// </summary>
        private const string INCLUDE_FILES_SAME = " /IS";

        /// <summary>
        /// tweaked file is defined to be one that exists in both the source and destination
        /// with identical size and timestamp but different attribute settings.
        /// </summary>
        
        private const string INCLUDES_TWEAKED_FILES = " /IT";

        /// <summary>
        /// exclude files larger than n bytes
        /// </summary>
        private const string EXCLUDE_FILES_MAX_BYTES = " /MAX:";

        /// <summary>
        /// exclude files smaller than n bytes
        /// </summary>
        private const string EXCLUDE_FILES_MIN_BYTES = " /MIN:";


        /// <summary>
        /// Exclude files with last modified date older than n days or specified date 
        /// </summary>
        private const string EXCLUDE_FILES_MAXAGE = " /MAXAGE:";

        /// <summary>
        /// Exclude files with last modified date newer than n days or spcific date
        /// </summary>
        /// 
        private const string EXCLUDE_FILES_MINAGE = " /MINAGE:";

        /// <summary>
        /// Exclude files with last access dates newer than n days or specific date
        /// </summary>
        /// 
        private const string EXCLUDE_FILES_MINLAD = " /MINLAD:";

        /// <summary>
        /// Exclude files with last access dates older than n days or specific date
        /// </summary>
        /// 
        private const string EXCLUDE_FILES_MAXLAD = " /MAXLAD:";

        /// <summary>
        /// Number of retries on failed copy. Default is 1 million
        /// </summary>
        /// 
        private const string RETRY = " /R:";

        /// <summary>
        /// wait time between retries. Default is 30 seconds
        /// </summary>
        /// 
        private const string WAIT_INTERVAL = " /W:";


        /// <summary>
        /// List without copying, deleting or applying time stamp to any file.
        /// </summary>
        /// 
        private const string LIST = " /L";

        private string source, destination;

        #endregion declaration

        IFilter filter;
        public XCopy()
        {
        }

        public XCopy(string source, string destination, XFilter filter)
        {
            this.source = source;
            this.destination = destination;
            this.filter = filter;
        }


        public string Source
        {
            get
            {
                return this.source;
            }
            set
            {
                this.source = value;
            }
        }

        public string Destination
        {
            get
            {
                return this.destination;
            }
            set
            {
                this.destination = value;
            }
        }
        public IFilter Filter
        {
            set
            {
                filter = value;
            }
            get
            {
                return filter;
            }
        }

        public void Copy()
        {
            Process copyProcess = new Process();
            copyProcess.StartInfo.FileName = "robocopy.exe";
            copyProcess.StartInfo.Arguments = BuildCommand();
            copyProcess.StartInfo.CreateNoWindow = true;
            copyProcess.StartInfo.UseShellExecute = false;            
            copyProcess.Start();
        }

        private string BuildCommand()
        {
            string command =  this.source + " " + this.destination + " *.*" + COPY_RECURSIVELY ;

            return command;
        }

    }
}
