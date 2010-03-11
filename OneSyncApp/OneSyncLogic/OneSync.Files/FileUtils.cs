/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.InteropServices;

namespace OneSync.Files
{
    /// <summary>
    /// Provide some file utilities
    /// </summary>
    public class FileUtils
    {

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
        /// Print information of any given fileinfo object
        /// </summary>
        /// <param name="fileInfo"></param>
        public static void PrintFileInfo(FileInfo fileInfo)
        {
            Console.WriteLine("Creation time: " + fileInfo.CreationTime);
            Console.WriteLine("Directory: " + fileInfo.Directory);
            Console.WriteLine("Directory name: " + fileInfo.DirectoryName);
            Console.WriteLine("Full name: " + fileInfo.FullName);
            Console.WriteLine("Length: " + fileInfo.Length);
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
        public static void Copy(string source, string destination)
        {
            //extract the directory lead to destination
            string directory = destination.Substring(0, destination.LastIndexOf('\\'));
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            //overwriten on exist
            File.Copy(source, destination, true);
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


    }
}
