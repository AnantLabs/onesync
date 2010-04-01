/*
 $Id: DirectoryInfoComparer.cs 66 2010-03-10 07:48:55Z gclin009 $
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OneSync.Files
{
    /// <summary>
    /// Compare info of 2 DirectoryInfo objects
    /// </summary>

    public class DirectoryInfoComparer: IEqualityComparer<DirectoryInfo>
    {
        private string baseSource = "";
        private string baseDestination = "";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseSource"></param>
        /// <param name="baseDestination"></param>
        public DirectoryInfoComparer(string baseSource, string baseDestination)
        {
            Console.WriteLine("Base source: " + baseSource );
            Console.WriteLine("Base destination: " + baseDestination );
            this.baseSource = baseSource;
            this.baseDestination = baseDestination;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="directoryInfoSource"></param>
        /// <param name="directoryInfoDestination"></param>
        /// <returns></returns>
        public bool Equals(DirectoryInfo directoryInfoSource, DirectoryInfo directoryInfoDestination)
        {
            return GetHashCode(directoryInfoSource) == GetHashCode(directoryInfoDestination);    
        }
        
        /// <summary>
        /// Get Hash Code of a given DirectoryInfo object 
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <returns>int</returns>
        public int GetHashCode(DirectoryInfo directoryInfo)
        {
            string relativePath = "";
            if (directoryInfo.FullName.Contains(baseSource)) relativePath = directoryInfo.FullName.Substring(baseSource.Length, directoryInfo.FullName.Length - baseSource.Length);
            else if (directoryInfo.FullName.Contains(baseDestination)) relativePath = directoryInfo.FullName.Substring(baseDestination.Length, directoryInfo.FullName.Length - baseDestination.Length);            
            return relativePath.GetHashCode();
        }
    }
}
