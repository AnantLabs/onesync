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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using OneSync.Synchronization;

namespace OneSync.Files
{
    /// <summary>
    /// Provide some file utilities
    /// </summary>
    public class FileUtils
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetFileInformationByHandle(IntPtr hFile,
                                                              out BY_HANDLE_FILE_INFORMATION lpFileInformation);

        /* MSDN Reference:
         * http://msdn.microsoft.com/en-us/library/aa363788(VS.85).aspx
         * http://msdn.microsoft.com/en-us/library/aa364952(VS.85).aspx
         * */

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

        public static bool FileExist (string baseFolder, string fileName)
        {
            string absolutePath = System.IO.Path.Combine(baseFolder, fileName);
            return File.Exists(absolutePath) ? true : false;
        }

        /// <summary>
        /// Delete a file given absolute path
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        public static bool Delete(string absolutePath, bool forceToDelete)
        {
            /*
            if (IsOpen(absolutePath))
                throw new FileInUseException("File " + absolutePath + " is in use by another process");
             */
            /*
            if (IsFileReadOnly(absolutePath) && forceToDelete && File.Exists(absolutePath))
            {
             */
            var fileInfo = new FileInfo(absolutePath);
            if (forceToDelete) fileInfo.IsReadOnly = !forceToDelete;

            try
            {
                File.Delete(absolutePath);
                return true;
            }
            catch (Exception)
            {return false;}
        }

        /// <summary>
        /// Delete a file given absolute path
        /// If the folder contains this file is empty after the file is deleted, the folder is deleted as well.
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <param name="baseFolder"></param>
        /// <param name="forceToDelete"></param>
        public static bool DeleteFileAndFolderIfEmpty(string baseFolder, string absolutePath, bool forceToDelete)
        {
            try
            {
                var dirInfo = new DirectoryInfo(new FileInfo(absolutePath).Directory.FullName);
                if (File.Exists(absolutePath)) Delete(absolutePath, forceToDelete);
                try
                {
                    //delete empty folder recursively 
                    DeleteEmptyFolderRecursively(baseFolder, dirInfo, forceToDelete);
                }
                catch (Exception){}
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Recursively delete empty folders
        /// </summary>
        /// <param name="baseFolder"></param>
        /// <param name="dir"></param>
        /// <param name="forceToDelete"></param>
        public static void DeleteEmptyFolderRecursively(string baseFolder, DirectoryInfo dir, bool forceToDelete)
        {
            if (IsDirectoryEmpty(dir.FullName) & !dir.FullName.Equals(baseFolder))
            {
                if (IsDirectoryReadOnly(dir.FullName) && forceToDelete)
                    dir.Attributes &= ~FileAttributes.ReadOnly;
                dir.Delete();
                DeleteEmptyFolderRecursively(baseFolder, dir.Parent, forceToDelete);
            }
        }

        /// <summary>
        /// Delete a folder given an absolute path
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <param name="forceToDelete"></param>
        public static void DeleteFolder(string absolutePath, bool forceToDelete)
        {
            if (!IsDirectoryEmpty(absolutePath)  || !Directory.Exists(absolutePath)) return;
            if (!IsDirectoryEmpty(absolutePath)  || !Directory.Exists(absolutePath)) return;
            var dirInfo = new DirectoryInfo(absolutePath);
            if (dirInfo.Exists & IsDirectoryReadOnly(absolutePath))
                dirInfo.Attributes &= ~FileAttributes.ReadOnly;
            dirInfo.Delete();
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
                .Where(f => (f.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)).ToArray();
        }

        /// <summary>
        /// Get list of sub folders under a directory, hidden folders are excluded
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        public static DirectoryInfo[] GetDirectoriesExcludeHidden(string absolutePath)
        {
            return (new DirectoryInfo(absolutePath).GetDirectories("*.*", SearchOption.AllDirectories)
                .Where(f => (f.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)).ToArray();
        }

        /// <summary>
        /// Get the relative path given the base directory and the full path to a file
        /// </summary>
        /// <param name="baseDirectory"></param>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static string GetRelativePath(string baseDirectory, string fullPath)
        {
            if (fullPath.Contains(baseDirectory))
                return fullPath.Substring(baseDirectory.Length, fullPath.Length - baseDirectory.Length);
            return "";
        }

        /// <summary>
        /// copy one file to another location
        /// source and destination is the full path
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="FileInUseException"></exception>
        public static bool Copy(string source, string destination, bool forceToCopy)
        {
            //extract the directory lead to destination
            string directory = destination.Substring(0, destination.LastIndexOf('\\'));
            
            //overwriten on exist            
            try
            {
                var info = new FileInfo(destination);
                if (!info.Directory.Exists) Directory.CreateDirectory((info.Directory.FullName));
                /*
                if (File.Exists(destination) && info.IsReadOnly)
                {
                    info.IsReadOnly = !forceToCopy;
                }*/
                File.Copy(source, destination, forceToCopy);
                return true;
            }
            catch (Exception ex)
            {
                // Check for not enough space on the disk.
                if ((uint) Marshal.GetHRForException(ex) == 0x80070070)
                    throw new OutOfDiskSpaceException();
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <param name="forceToRename"></param>
        /// <returns></returns>
        /// <exception cref="FileInUseException"></exception>
        public static bool Move(string oldPath, string newPath, bool forceToRename)
        {
            if (!Directory.Exists(oldPath))
                throw new SyncSourceException(oldPath + " is not found");

            if (!Directory.Exists(newPath))
                throw new SyncSourceException(newPath+ " is not found");

            //if (IsOpen(oldPath)) throw new FileInUseException("File " + oldPath + " is being opened");
            //extract the directory lead to destination
            string directory = newPath.Substring(0, newPath.LastIndexOf('\\'));
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            //overwriten on exist
            try
            {
                var info = new FileInfo(newPath);
                /*if (info.Exists && info.IsReadOnly)
                {
                    info.IsReadOnly = !forceToRename;
                }*/
                File.Delete(newPath);
                File.Move(oldPath, newPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Used in conflict resolution. Conflict file will be renamed and copied or moved over
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        public static bool DuplicateRename(string source, string destination)
        {
            string directory = Path.GetDirectoryName(destination);
            string fileName = Path.GetFileNameWithoutExtension(destination);
            string extension = Path.GetExtension(destination);

            fileName += "[conflicted-copy-" + string.Format("{0:yyyy-MM-dd-hh-mm-ss}", DateTime.Now) + "]";
            destination = directory + "\\" + fileName + extension;

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
            using (var fs = new FileStream(filePath, FileMode.Open))
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
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static bool IsFileReadOnly(string absolutePath)
        {
            return ((File.GetAttributes(absolutePath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                       ? true
                       : false;
        }

        /// <summary>
        /// Check whether a file with absolute path is hidden
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static bool IsFileHidden(string absolutePath)
        {
            return ((File.GetAttributes(absolutePath) & FileAttributes.Hidden) == FileAttributes.Hidden) ? true : false;
        }

        /// <summary>
        /// Check whether a directory with absolute path is hidden
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="AugmentException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        public static bool IsDirectoryHidden(string absolutePath)
        {
            var dirInfo = new DirectoryInfo(absolutePath);
            return ((dirInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) ? true : false;
        }


        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        internal static extern bool PathIsDirectoryEmpty(string pszPath);

        public static bool IsDirectoryEmpty(string absolutePath)
        {
            return PathIsDirectoryEmpty(absolutePath);
        }

        /// <summary>
        /// Check whether a directory with absolute path is read only
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="AugmentException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        public static bool IsDirectoryReadOnly(string absolutePath)
        {
            var dirInfo = new DirectoryInfo(absolutePath);
            return ((dirInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) ? true : false;
        }

        /// <summary>
        /// Test whether a file with absolute path is open/in use
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>        
        public static bool IsOpen(string absolutePath)
        {
            FileStream fs = null;
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

        /// <summary>
        /// Move contents from a folder to another one
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destinationDir"></param>
        /// <returns></returns>
        public static bool MoveFolder(string sourceDir, string destinationDir)
        {
            bool succeeded = true;
            //may throw unauthorize access here
            FileInfo[] fileInfos = new DirectoryInfo(sourceDir).GetFiles("*.*", SearchOption.AllDirectories);
            foreach (FileInfo info in fileInfos)
            {
                string absolutePathInDestination = info.FullName.Replace(sourceDir, destinationDir);
                try{ Copy(info.FullName, absolutePathInDestination, false);}
                catch (OutOfDiskSpaceException){throw;}
                catch (Exception){succeeded = false;}
            }
            return succeeded;
        }

        #region Nested type: BY_HANDLE_FILE_INFORMATION
        private struct BY_HANDLE_FILE_INFORMATION
        {
            public Int64 CreationTime;
            public uint FileAttributes;
            public uint FileIndexHigh;
            public uint FileIndexLow;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public Int64 LastAccessTime;
            public Int64 LastWriteTime;
            public uint NumberOfLinks;
            public uint VolumeSerialNumber;
        }
        #endregion
    }
}