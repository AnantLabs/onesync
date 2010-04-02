/*
 $Id$
 */
/*
 * Last edited by Thuat
 * Last modified time: 27/03/2010
 * Changes:
 *  +Integrate with KtmIntegration which provides ntfs transaction.
 *  +Files/folders permission check
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security.AccessControl;
using System.Security.Permissions;

namespace OneSync.Files
{
    /// <summary>
    /// Provide some file utilities
    /// </summary>
    public class FileUtils
    {
        private const int MEGABYTES_TO_BYTES_FACTOR = 1024 * 1024;
        private const int STREAM_LIMIT = 50 * MEGABYTES_TO_BYTES_FACTOR;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetFileInformationByHandle(IntPtr hFile,
           out BY_HANDLE_FILE_INFORMATION lpFileInformation);

        /* MSDN Reference:
         * http://msdn.microsoft.com/en-us/library/aa363788(VS.85).aspx
         * http://msdn.microsoft.com/en-us/library/aa364952(VS.85).aspx
         * */

        struct BY_HANDLE_FILE_INFORMATION
        {
            public uint FileAttributes;
            public Int64 CreationTime;
            public Int64 LastAccessTime;
            public Int64 LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        }

        /// <summary>
        /// Compute the hash of content of any given file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileHash(string path)
        {          
            string hashString = "";
            MD5 md5Hasher = new MD5CryptoServiceProvider();
            using (Stream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] hashBytes = md5Hasher.ComputeHash(fileStream);
                foreach (byte b in hashBytes) hashString += String.Format("{0:x2}", b);
            }          
            return hashString;
        }

        /// <summary>
        /// Delete a file given absolute path
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        public static bool Delete(string absolutePath, bool forceToDelete)
        {
            if (IsOpen(absolutePath))
                throw new FileInUseException("File " + absolutePath + " is in use by another process");
            if (IsFileReadOnly(absolutePath) && forceToDelete && File.Exists(absolutePath))
            {
                FileInfo fileInfo = new FileInfo(absolutePath);
                fileInfo.IsReadOnly = !forceToDelete;
            }                
            try
            {
                File.Delete(absolutePath);
                return true;
            }
            catch (Exception) { return false; }            
        }

        /// <summary>
        /// Delete a file given absolute path
        /// If the folder contains this file is empty after the file is deleted, the folder is deleted as well.
        /// </summary>
        /// <param name="absolutePath"></param>
        public static bool DeleteFileAndFolderIfEmpty(string baseFolder, string absolutePath, bool forceToDelete)
        {               
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(new FileInfo(absolutePath).Directory.FullName);
                if ( File.Exists (absolutePath))  Delete(absolutePath, forceToDelete);
                try
                {
                    //delete empty folder recursively 
                    DeleteEmptyFolderRecursively(baseFolder, dirInfo);
                }                
                catch (Exception){}
                return true;
            }
            catch (Exception)
            { return false; }            
        }

        /// <summary>
        /// Recursively delete empty folders
        /// </summary>
        /// <param name="baseFolder"></param>
        /// <param name="dir"></param>
        private static void DeleteEmptyFolderRecursively(string baseFolder, DirectoryInfo dir)
        {           
                if ( dir.GetFiles().Length  == 0
                && dir.GetDirectories().Length == 0
                && !dir.FullName.Equals(baseFolder))
            {
                Console.WriteLine("Empty");
                dir.Delete();
                DeleteEmptyFolderRecursively(baseFolder, dir.Parent);
            }
        }

        /// <summary>
        /// Get list of files in a folder 
        /// exclude hidden files
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        public static FileInfo[] GetFilesExcludeHidden(string absolutePath)
        {
            return (new DirectoryInfo(absolutePath).GetFiles("*.*", SearchOption.AllDirectories)
                .Where(f => (f.Attributes & FileAttributes.Hidden) !=  FileAttributes.Hidden)).ToArray<FileInfo>();           
        }

        /// <summary>
        /// Get list of sub folders under a directory, hidden folders are excluded
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        public static DirectoryInfo[] GetDirectoriesExcludeHidden(string absolutePath)
        {
            return (new DirectoryInfo(absolutePath).GetDirectories("*.*", SearchOption.AllDirectories)
                .Where(f => (f.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)).ToArray<DirectoryInfo>();
        }

        private static string GetDoubleHash(string path)
        {
            long fileSize = new FileInfo(path).Length;
            int numberOfThreads = Convert.ToInt32((fileSize % STREAM_LIMIT) + 1);
            int startIndex = 0;
            for (int x = 0; x < numberOfThreads && startIndex <= fileSize; x++)
            {
                //PartialFileStream stream = new PartialFileStream(path, FileMode.Open, startIndex, STREAM_LIMIT - 1);


                startIndex += STREAM_LIMIT - 1;
            }
            return "";
        }

        public static void PartialHash(Stream stream)
        {
            StringBuilder builder = new StringBuilder();
            MD5 md5Hasher = new MD5CryptoServiceProvider();
            byte[] hashBytes = md5Hasher.ComputeHash(stream);
            foreach (byte b in hashBytes) builder.Append(String.Format("{0:x2}", b));
        }
        
        /// <summary>
        /// Get the relative path given the base directory and the full path to a file
        /// </summary>
        /// <param name="baseDirectory"></param>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static string GetRelativePath(string baseDirectory, string fullPath)
        {
            if (fullPath.Contains(baseDirectory)) return fullPath.Substring(baseDirectory.Length, fullPath.Length - baseDirectory.Length);
            else return "";
        }

        /// <summary>
        /// copy one file to another location
        /// source and destination is the full path
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <exception cref="System.ComponentModel.Win32Exception"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="FileInUseException"></exception>
        public static bool Copy(string source, string destination, bool forceToCopy)
        {
            if (!File.Exists(source)) throw new FileNotFoundException(source);
            if (IsOpen(source)) throw new FileInUseException("File " + source + " is being opened");
            if (IsOpen(source) || IsOpen(destination)) throw new FileInUseException("File is currently open");                

            //extract the directory lead to destination
            string directory = destination.Substring(0, destination.LastIndexOf('\\'));
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);            
            //overwriten on exist            
            try
            {
                FileInfo info = new FileInfo (destination);
                if (File.Exists(destination) && info.IsReadOnly) { info.IsReadOnly =!forceToCopy; }
                File.Copy(source, destination, true);
                return true;
            }
            catch (Exception){return false;}
        }


        public static bool Move(string oldPath, string newPath, bool forceToRename)
        {
            if (!File.Exists(oldPath)) throw new FileNotFoundException(oldPath);
            if (IsOpen(oldPath)) throw new FileInUseException("File " + oldPath + " is being opened");

            //extract the directory lead to destination
            string directory = newPath.Substring(0, newPath.LastIndexOf('\\'));
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            //overwriten on exist            
            try
            {
                FileInfo info = new FileInfo(newPath);
                if (info.Exists && info.IsReadOnly) { info.IsReadOnly = !forceToRename; }
                File.Delete(newPath);
                File.Move(oldPath, newPath);
                return true;
            }
            catch (Exception) { return false; }
        }

        /// <summary>
        /// Used in conflict resolution. Conflict file will be renamed and copied/moved over
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static bool DuplicateRename(string source, string destination, bool keepOriginal)
        {
            int lastSlashIndex = destination.LastIndexOf('\\');
            int lastDotIndex = destination.LastIndexOf('.');
            string fileName = "";
            string directory = "";
            string extension = "";
            directory = destination.Substring(0, lastSlashIndex);
            if (lastSlashIndex > 0 && lastDotIndex > 0)
            {
                fileName = destination.Substring(lastSlashIndex + 1, lastDotIndex - lastSlashIndex - 1);
                extension = destination.Substring(lastDotIndex, destination.Length - lastDotIndex);
            }
            else
            {
                fileName = destination.Substring(lastSlashIndex + 1, destination.Length - 1 - lastSlashIndex);
                extension = destination.Substring(lastSlashIndex + 1, destination.Length - lastSlashIndex - 1 - fileName.Length);
            }

            fileName += "[conflicted-copy-" + string.Format("{0:yyyy-MM-dd-hh-mm-ss}", DateTime.Now) + "]";
            destination = directory + "\\" + fileName + extension;
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);                       

            if (keepOriginal)
                return Copy(source, destination, true);
            else
                return Move(source, destination, false);
        }

        /// <summary>
        /// Returns unique identifier that is associated with the file specified.
        /// Renaming a file in the FAT file system can also change the file ID,
        /// but only if the new file name is longer than the old one.
        /// In the NTFS file system, a file keeps the same file ID until it is deleted.
        /// File path specified is assumed to be valid and file exist.
        /// </summary>
        /// <param name="filePath">Path to file.</param>
        public static UInt64 GetFileUniqueID(string filePath)
        {
            BY_HANDLE_FILE_INFORMATION info;
            UInt64 uid = 0;

            // Assume that file path is valid and file exists.
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                GetFileInformationByHandle(fs.SafeFileHandle.DangerousGetHandle(), out info);
            }

            uid = info.FileSizeHigh << 32;
            uid &= info.FileSizeLow;

            return uid;
        }

        /// <summary>
        /// Check whether a file with absolute path is read only
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        public static bool IsFileReadOnly(string absolutePath)
        {
            return ((File.GetAttributes(absolutePath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) ? true : false;
        }
        
        /// <summary>
        /// Check whether a file with absolute path is hidden
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        public static bool IsHidden(string absolutePath)
        {            
            return ((File.GetAttributes(absolutePath) & FileAttributes.Hidden) == FileAttributes.Hidden) ? true : false;
        }

        /// <summary>
        /// Test whether a file with absolute path is open/in use
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        public static bool IsOpen(string absolutePath)
        {
            if (!File.Exists(absolutePath)) return false;
            FileStream fs  = null;
            try
            {
                fs = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);
                return false;
            }
            catch (Exception)
            {return true;}
            finally
            {if (fs != null) fs.Dispose();}            
        }

        public static bool MoveFolder(string sourceDir, string destinationDir)
        {
            try
            {
                FileInfo[] fileInfos = new DirectoryInfo(sourceDir).GetFiles("*.*", SearchOption.AllDirectories);
                foreach (FileInfo info in fileInfos)
                {
                    string absolutePathInDestination = info.FullName.Replace(sourceDir, destinationDir);
                    try
                    {
                        Copy(info.FullName, absolutePathInDestination, false);
                    }
                    catch (Exception) { }                    
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }            
        }
    }
}
