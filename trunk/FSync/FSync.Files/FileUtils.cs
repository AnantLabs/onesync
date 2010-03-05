using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace OneSync.Files
{
    /// <summary>
    /// Provide some file utilities
    /// </summary>
    public class FileUtils
    {
        /// <summary>
        /// Compute the hash of content of any given file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileHash(string path)
        {
            string hashString = "";
            MD5 md5Hasher = new MD5CryptoServiceProvider();
            using (Stream fileStream = new FileStream (path, FileMode.Open,FileAccess.Read))
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
            Console.WriteLine("Creation time: " + fileInfo.CreationTime );
            Console.WriteLine("Directory: " + fileInfo.Directory );
            Console.WriteLine("Directory name: " + fileInfo.DirectoryName ); 
            Console.WriteLine("Full name: " +fileInfo.FullName);
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
            string directory = destination.Substring (0, destination.LastIndexOf('\\'));
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            //overwriten on exist
            File.Copy(source, destination, true);
        }
    }
}
