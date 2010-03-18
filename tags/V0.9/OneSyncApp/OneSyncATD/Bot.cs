using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OneSyncATD
{

    public class Bot
    {
        private delegate void TraverseDirectoryCallback(FileSystemInfo fi);
        
        public event UpdateHandler Update;
        public string RootFolder { get; set; }
        public RandomGenerator.NameType type { get; set; }
        public DateTime CreationTime { get; set; }


        // Constructors
        public Bot(string rootFolder) :
            this(rootFolder, DateTime.Now, RandomGenerator.NameType.Letters)
        {
        }

        public Bot(string rootFolder, DateTime creationTime,  RandomGenerator.NameType type)
        {
            this.RootFolder = rootFolder;
            this.CreationTime = creationTime;
            this.type = type;
        }

        protected virtual void OnUpdate(string msg, int progress)
        {
            if (Update != null) Update(this, new UpdateEventArgs(msg, progress));
        }

        /// <summary>
        /// Create a random directory structure with specified maximum depth.
        /// Each directory will have random number of subdirectories not more than specified limit.
        /// </summary>
        /// <param name="depth">A number greater or equals to 0.</param>
        /// <param name="subDirLimit">Maximum number of subdirectories each folder can have.</param>
        /// <param name="type">Type of directory name</param>
        public void CreateDirectoryStructure(int depth, int subDirLimit)
        {
            if (depth <= 0) return;

            DirectoryInfo di = GetRootDirectoryInfo();

            OnUpdate("Creating directory strucure...", 0);

            // Create directory structure randomly.
            // Note: Structure might not have secified depth due to 'randomness'
            CreateDirectoryStructure(di, depth, subDirLimit);

            // Create directory with specified depth
            for (int i = 0; i < depth; i++)
            {
                string newDirName = RandomGenerator.GetDirectoryName(type);
                di = di.CreateSubdirectory(newDirName);
                di.CreationTime = this.CreationTime;
            }
        }

        /// <summary>
        /// Create files randomly inside each subdirectories with random filesize.
        /// </summary>
        /// <param name="count">Average files to be created in each directory.</param>
        /// <param name="minSize">Minimum file size.</param>
        /// <param name="maxSize">Maximum file size.</param>
        /// <param name="type">Type of filename.</param>
        public void DropFiles(int count, int minSize, int maxSize)
        {
            if (maxSize < minSize) throw new ArgumentException("maxSize must not be less than minSize");
            if (minSize < 0) throw new ArgumentException("minSize must not be less than 0");

            DirectoryInfo di = new DirectoryInfo(RootFolder);
            if (!di.Exists) di.Create();

            // Create file randomly in each folder
            TraverseDirectoryTree(this.RootFolder, true, (fi) =>
            {
                int fileCount = RandomGenerator.random.Next(0, count + 1);

                OnUpdate(string.Format("Creating {0} files in {1}", fileCount, fi.FullName), 0);

                for (int i = 0; i < fileCount; i++)
                {
                    string filename = RandomGenerator.GetFilename(type);
                    string filepath = fi.FullName + '\\' + filename;
                    int size = RandomGenerator.random.Next(minSize, maxSize + 1);

                    Utility.CreateFile(filepath, size);
                    FileInfo f = new FileInfo(filepath);
                    f.CreationTime = this.CreationTime;
                }
            });

        }
        
        /// <summary>
        /// Modifies attributes of files randomly with specified probability.
        /// </summary>
        /// <param name="readOnly">Set to true to make some files as read-only.</param>
        /// <param name="system">Set to true to make some files as system file.</param>
        /// <param name="lastModifiedTime">Change last modified time. Use null to ignore.</param>
        /// <param name="probability">A number between 1 and 100 inclusive.</param>
        public void ModifyFiles(bool delete, bool readOnly, bool system, DateTime? lastModifiedTime, int probability)
        {
            if (probability < 0 || probability > 100)
                throw new ArgumentException("probability must be between 1 and 100 inclusive.");

            DirectoryInfo di = GetRootDirectoryInfo();

            // Create file randomly in each folder
            TraverseDirectoryTree(this.RootFolder, false, (fi) =>
            {
                // Ignore directories
                if (fi.GetType() == typeof(FileInfo))
                {
                    if (delete && RandomGenerator.random.Next(1, 101) <= probability)
                    {
                        fi.Attributes = 0;
                        fi.Delete();
                        OnUpdate(string.Format("Deleted {0}.", fi.FullName), 0);
                        return;
                    }

                    if (readOnly && RandomGenerator.random.Next(1, 101) <= probability)
                    {
                        fi.Attributes |= FileAttributes.ReadOnly;
                        OnUpdate(string.Format("Changing {0} to read-only.", fi.FullName), 0);
                    }

                    if (system && RandomGenerator.random.Next(1, 101) <= probability)
                    {
                        fi.Attributes |= FileAttributes.System;
                        OnUpdate(string.Format("Changing {0} to system file.", fi.FullName), 0);
                    }

                    if (lastModifiedTime != null && RandomGenerator.random.Next(1, 101) <= probability)
                    {
                        // Clear all file attributes before modifying time
                        FileAttributes attr = fi.Attributes;
                        fi.Attributes = 0;
                        fi.LastWriteTime = (DateTime)lastModifiedTime;

                        // Restore back file attributes
                        fi.Attributes = attr;

                        OnUpdate(string.Format("Changing {0} last modified time to {1}", fi.FullName, lastModifiedTime.ToString()), 0);
                    }
                }

            });

            OnUpdate("Done.", 0);
        }

        /// <summary>
        /// Clear all file attributes.
        /// </summary>
        /// <param name="lastModifiedTime">Last Modified time of all files. Use null to ignore.</param>
        public void ResetFiles(DateTime lastModifiedTime)
        {
            DirectoryInfo di = GetRootDirectoryInfo();

            // Create file randomly in each folder
            TraverseDirectoryTree(this.RootFolder, false, (fi) =>
            {
                fi.Attributes = 0;

                if (lastModifiedTime != null)
                    fi.LastWriteTime = lastModifiedTime;

            });
        }

        #region Helper Methods

        private DirectoryInfo GetRootDirectoryInfo()
        {
            DirectoryInfo di = new DirectoryInfo(RootFolder);
            if (!di.Exists) di.Create();

            return di;
        }

        private static void TraverseDirectoryTree(string path, bool dirOnly, TraverseDirectoryCallback callback)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            callback(di);
            TraverseDirectoryTree(di, dirOnly, callback);
        }

        private static void TraverseDirectoryTree(DirectoryInfo root, bool dirOnly, TraverseDirectoryCallback callback)
        {
            // Traverse directories recursively
            foreach (DirectoryInfo di in root.GetDirectories())
            {
                try
                {
                    callback(di);
                    TraverseDirectoryTree(di, dirOnly, callback);
                }
                catch (UnauthorizedAccessException ex)
                {
                    // 
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }

            if (dirOnly) return;

            foreach (FileInfo fi in root.GetFiles())
                callback(fi);

        }

        private void CreateDirectoryStructure(DirectoryInfo di, int depth, int subDirLimit)
        {
            if (depth <= 0) return;

            // Create random number of subdirectoriesfolder with specified levels
            int num = RandomGenerator.random.Next(0, subDirLimit + 1);
            for (int i = 0; i < num; i++)
            {
                string newDirName = RandomGenerator.GetDirectoryName(type);
                DirectoryInfo newFolder = di.CreateSubdirectory(newDirName);

                // Create subfolders recursively
                this.CreateDirectoryStructure(newFolder, depth - 1, subDirLimit);
            }
        }

        #endregion

        
    }
}
