/*
 $Id$
 */
/*FileInfoComparer class implements IEqualityComparer   
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace OneSync.Files
{
    /// <summary>
    /// Implement IEqualityComparer interface
    /// </summary>
    public class FileInfoComparer : IEqualityComparer<FileInfo>
    {
        /// <summary>
        ///Path to source folder
        /// </summary>
        private string baseSource;
        /// <summary>
        /// Path to destination folder
        /// </summary>
        private string baseDestination;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseSource">Path to the source folder</param>
        /// <param name="baseDestination">Path to the destination folder</param>
        /// 

        public FileInfoComparer(string baseSource, string baseDestination)
        {
            this.baseSource = baseSource;
            this.baseDestination = baseDestination; 
        }

        /// <summary>
        /// Two compared objects are equal if they have same hash code values
        /// </summary>
        /// <param name="fileInfo1">FileInfo</param>
        /// <param name="fileInfo2">FileInfo</param>
        /// <returns>bool</returns>
        public bool Equals(FileInfo fileInfo1, FileInfo fileInfo2)
        {         
            return GetHashCode(fileInfo1) == GetHashCode(fileInfo2);                 
        }
        /// <summary>
        /// Get HashCode of a fileinfo object
        /// </summary>
        /// <param name="fi">FileInfo</param>
        /// <returns>int</returns>
        /// 
       
        public int GetHashCode(FileInfo fi)
        {
            string relativePath = "";
            if (fi.FullName.Contains(baseSource)) relativePath = fi.FullName.Substring(baseSource.Length, fi.FullName.Length - baseSource.Length);
            else if (fi.FullName.Contains(baseDestination)) relativePath = fi.FullName.Substring(baseDestination.Length, fi.FullName.Length - baseDestination.Length);
            string hash = FileUtils.GetFileHash(fi.FullName) + relativePath;
            return hash.GetHashCode();
        }        
    }
}
