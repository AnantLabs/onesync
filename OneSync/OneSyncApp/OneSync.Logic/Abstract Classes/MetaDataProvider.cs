using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;

namespace OneSync.Synchronization
{
    /// <summary>
    /// Class to manage MetaData.
    /// </summary>

    public abstract class MetaDataProvider
    {
        // Path to where MetaData are saved.
        protected string _mdStoragePath;

        // Root dir of files referenced by relative paths.
        protected string _rootPath;


        /// <summary>
        /// Creates a MetaDataProvider.
        /// </summary>
        /// <param name="storagePath">Location where all metadata are saved to or loaded from.</param>
        /// <param name="rootPath">To be combined with relative paths to give absolute paths of files.</param>
        public MetaDataProvider(string storagePath, string rootPath)
        {
            _mdStoragePath = storagePath;
            _rootPath = rootPath;
        }


        /// <summary>
        /// Loads either the MetaData with specified id or with different id.
        /// </summary>
        /// <param name="currId">Id of MetaData.</param>
        /// <param name="loadOther">false to load metadata with currId. true to load other metadata with id different from currId.</param>
        /// <returns></returns>
        public abstract Metadata Load(string currId, SourceOption option);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currId"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public abstract FileMetaData LoadFileMetadata(string currId, SourceOption option);

        public abstract FolderMetadata LoadFolderMetadata(string currId, SourceOption option);

        /// <summary>
        /// Add metadata infomation.
        /// </summary>
        /// <param name="mData">Metadata to be added.</param>
        /// <returns>true if added and saved successfully.</returns>
        public abstract bool Add(FileMetaData mData);


        /// <summary>
        /// Add metadata items information.
        /// </summary>
        /// <param name="mData">Metadata items to be added.</param>
        /// <returns>true if added and saved successfully.</returns>
        public abstract bool Add(IList<FileMetaDataItem> mData);


        /// <summary>
        /// Deletes specified FileMetaDataItems that has the same source id and relative path.
        /// </summary>
        /// <param name="items">FileMetaDataItems with same source id and relative path as this list will be deleted.</param>
        /// <returns>true if successfully deleted.</returns>
        public abstract bool Delete(IList<FileMetaDataItem> items);


        /// <summary>
        /// Update MetaData.
        /// </summary>
        /// <param name="items">List of updated metadata information.</param>
        /// <returns></returns>
        public abstract bool Update(IList<FileMetaDataItem> items);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldItems"></param>
        /// <param name="newItems"></param>
        /// <returns></returns>
        public abstract bool UpdateFolderMetadata(FolderMetadata oldItems, FolderMetadata newItems, bool storeOnlyEmptyFolder);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldItems"></param>
        /// <param name="newItems"></param>
        /// <returns></returns>
        public abstract bool UpdateFileMetadata(FileMetaData oldItems, FileMetaData newItems);

        /// <summary>
        /// Update MetaData information.
        /// </summary>
        /// <param name="oldMetadata">Outdated metadata.</param>
        /// <param name="newMetada">Updated metadata.</param>
        /// <returns>true if update successful.</returns>
        public abstract bool Update(Metadata oldMetadata, Metadata newMetada);

        /// <summary>
        /// Create default schema
        /// </summary>
        public abstract void CreateSchema();

        /// <summary>
        /// Generates MetaData of all files in specified path and its subdirectories.
        /// </summary>
        /// <param name="fromPath">Root path of files which metadata is to be generated.</param>
        /// <param name="id">Id of metadata to be generated.</param>
        /// <returns>MetaData of all files specified in root path.</returns>
        public static Metadata Generate(string fromPath, string id, bool excludeHidden, bool createIfNotExist, bool excludeNonEmpty)
        {
            if (!Directory.Exists((fromPath)) && createIfNotExist)
                Directory.CreateDirectory((fromPath));
            return new Metadata(GenerateFileMetadata(fromPath, id, excludeHidden, createIfNotExist), GenerateFolderMetadata(fromPath, id, excludeHidden, createIfNotExist,excludeNonEmpty));
        }

        private static void AddDirectorySecurity (string absolutePath)
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            SecurityIdentifier id = identity.User;
            DirectorySecurity directorySecurity = Directory.GetAccessControl(absolutePath);
            FileSystemAccessRule rule = new FileSystemAccessRule(id, FileSystemRights.Read,AccessControlType.Allow);
            directorySecurity.AddAccessRule(rule);
            Directory.SetAccessControl(absolutePath,directorySecurity);
        }

        public static FileMetaData GenerateFileMetadata(string fromPath, string id, bool excludeHidden, bool createIfNotExist)
        {
            if (!Directory.Exists((fromPath)) && createIfNotExist)
                Directory.CreateDirectory((fromPath));
            return GenerateFileMetadata(fromPath, id, excludeHidden, null);
        }

        public static FileMetaData GenerateFileMetadata(string fromPath, string id, bool excludeHidden, StatusCallbackDelegate statusCallback)
        {            
            FileMetaData fileMetadata = new FileMetaData(id, fromPath);

            DirectoryInfo di = new DirectoryInfo(fromPath);

            AddDirectorySecurity(fromPath);

            FileInfo[] files = null;

            if (statusCallback != null) statusCallback("Reading files from: " + fromPath);

            files = di.GetFiles("*.*", SearchOption.AllDirectories);
            
            if (files == null) return new FileMetaData(id, fromPath);
            IEnumerable<FileInfo> noHiddenFiles = from file in files
                                                  where excludeHidden ? !Files.FileUtils.IsFileHidden(file.FullName) : true
                                                  select file;
            files = noHiddenFiles.ToArray<FileInfo>();

            foreach (FileInfo f in files)
            {
                try
                {
                    if (statusCallback != null) statusCallback(string.Format("Processing file: {0}", f.FullName));

                    fileMetadata.MetaDataItems.Add(new FileMetaDataItem(id,
                        OneSync.Files.FileUtils.GetRelativePath(fromPath, f.FullName), Files.FileUtils.GetFileHash(f.FullName),
                        f.LastWriteTime, (uint)0, (uint)0));    
                }catch(Exception){ }
            }
            return fileMetadata;
        }

        /// <summary>
        /// Function to generate folder metadata of a given folder
        /// </summary>
        /// <param name="fromPath"></param>
        /// <param name="id"></param>
        /// <param name="excludeHidden"></param>
        /// <returns></returns>
        public static FolderMetadata GenerateFolderMetadata(string fromPath, string id, bool excludeHidden, bool createIfNotExist, bool excludeNonEmpty)
        {
            if (!Directory.Exists((fromPath)) && createIfNotExist)
                Directory.CreateDirectory((fromPath));

            FolderMetadata folderMetadata = new FolderMetadata(id, fromPath);

            DirectoryInfo di = new DirectoryInfo(fromPath);
            DirectoryInfo[] directories = null;

            AddDirectorySecurity(fromPath);

            directories = di.GetDirectories("*.*", SearchOption.AllDirectories);
            
            if (directories == null) return new FolderMetadata( id, fromPath );
            IEnumerable<DirectoryInfo> noHiddenDirectories = from dir in directories
                                                             where (
                                                             (excludeHidden?!Files.FileUtils.IsDirectoryHidden(dir.FullName):true))
                                                             select dir;
            directories = noHiddenDirectories.ToArray<DirectoryInfo>();

            foreach (DirectoryInfo dirInfo in directories)
            {
                folderMetadata.FolderMetadataItems.Add(new FolderMetadataItem(id, dirInfo.FullName, fromPath));
            }
            return folderMetadata;
        }


        #region Public Properties

        /// <summary>
        /// Location where all metadata are saved to or loaded from.
        /// </summary>
        public string StoragePath
        {
            get { return _mdStoragePath; }
        }

        /// <summary>
        /// To be combined with relative paths to give absolute paths of files.
        /// </summary>
        public string RootPath
        {
            get { return _rootPath; }
        }

        #endregion




    }
}
