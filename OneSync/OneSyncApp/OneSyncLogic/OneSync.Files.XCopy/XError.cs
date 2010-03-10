/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Files.XCopy
{
    public class XError
    {
        /// <summary>
        /// Serious error. Robocopy did not copy any files. This is either
        ///a usage error or an error due to insufficient access privileges
        ///on the source or destination directories.
        /// </summary>
        public const int SERIOUS = 0x10;

        /// <summary>
        /// Some files or directories could not be copied (copy errors
        ///occurred and the retry limit was exceeded). Check these errors
        ///further.
        /// </summary>
        public const int COPY_FAILED = 0x08;

        /// <summary>
        /// Some Mismatched files or directories were detected. Examine
        ///the output log. Housekeeping is probably necessary.
        /// </summary>
        public const int FILES_MISMATCHED = 0x04;

        /// <summary>
        /// Some Extra files or directories were detected. Examine the
        ///output log. Some housekeeping may be needed.
        /// </summary>
        private const int EXTRA_FILES_DETECTED = 0x02;

        /// <summary>
        /// One or more files were copied successfully (that is, new files
        ///have arrived).
        /// </summary>
        private const int SUCCESSFUL = 0x01;

        /// <summary>
        /// No errors occurred, and no copying was done. The source
        ///and destination directory trees are completely synchronized.
        /// </summary>
        private const int SYNCHONIZED_NO_COPY = 0x00;

    }
}
