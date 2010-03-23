/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.AccessControl;

namespace OneSync.Files
{
    /// <summary>
    /// 
    /// </summary>
    public class FolderComparer
    {
        private string source= "", destination="";
        IEnumerable<FileInfo> sourceFileInfos, destinationFileInfos;
        IEnumerable<DirectoryInfo> sourceDirectoryInfos, destinationDirectoryInfos;
        private FileInfoComparer fileInfoComparer;
        private DirectoryInfoComparer directoryInforComparer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public FolderComparer(string source, string destination)
        {
            if ( !(Directory.Exists (source) && Directory.Exists (destination))) throw new DirectoryNotFoundException();
            this.source = source;
            this.destination = destination;

            this.sourceDirectoryInfos = GetDirectoryInfos(this.source);
            this.destinationDirectoryInfos = GetDirectoryInfos(this.destination);
            
            this.sourceFileInfos = GetFileInfos(this.source);
            this.destinationFileInfos = GetFileInfos(this.destination);

            this.fileInfoComparer = new FileInfoComparer(source, destination);
            this.directoryInforComparer = new DirectoryInfoComparer(source, destination);
        }
         /// <summary>
         /// Get Source folder
         /// </summary>
          
        public string  Source
        {
            get { return this.source; }
        }

        /// <summary>
        /// Get destination folder
        /// </summary>
        public string Destination
        {
            get { return this.destination; }
        }

        /// <summary>
        /// Get info of a FileInfo given a path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private IEnumerable<FileInfo> GetFileInfos(string path)
        {                
            string[] fileNames = Directory.GetFiles (path, "*.*", SearchOption.AllDirectories) ;
            List<FileInfo> fileInfos = new List<FileInfo>();            
            foreach (string fileName in fileNames)
            {                
                fileInfos.Add (new FileInfo (fileName));
            }     
            return fileInfos;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsIdentical()
        {            
            return sourceFileInfos.SequenceEqual(destinationFileInfos, fileInfoComparer);
        }

        /// <summary>
        /// Return a list of common files
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileInfo> GetCommonFiles()
        {
            return sourceFileInfos.Intersect(destinationFileInfos, fileInfoComparer);
        }

        /// <summary>
        /// Return only files in source
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileInfo> GetSourceOnly()
        {      
             return sourceFileInfos.Except(destinationFileInfos, fileInfoComparer);
        }

        /// <summary>
        /// Return only files in destination
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileInfo> GetDestinationOnly()
        {
            return destinationFileInfos.Except(sourceFileInfos, fileInfoComparer );
        }
             
        /// <summary>
        /// Get info of DirectoryInfo object given a path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private IEnumerable<DirectoryInfo> GetDirectoryInfos(string path)
        {
            
            string[] folderNames = Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories);
            List<DirectoryInfo> directoryInfo = new List<DirectoryInfo>();
            foreach (string folderName in folderNames)
            {
                directoryInfo.Add(new DirectoryInfo(folderName));
            }
            return directoryInfo;
        }

        /// <summary>
        /// Get common folders
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DirectoryInfo> GetCommonFolders()
        {
            return sourceDirectoryInfos.Intersect(destinationDirectoryInfos, directoryInforComparer );
        }

        /// <summary>
        /// Get folders in source only
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DirectoryInfo> GetFoldersInSourcesOnly()
        {
            return sourceDirectoryInfos.Except(destinationDirectoryInfos, directoryInforComparer);
        }

        /// <summary>
        /// Get folders in destination only
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DirectoryInfo> GetFoldersInDestinationOnly()
        {
            return destinationDirectoryInfos.Except(sourceDirectoryInfos, directoryInforComparer);
        }
    }
}

