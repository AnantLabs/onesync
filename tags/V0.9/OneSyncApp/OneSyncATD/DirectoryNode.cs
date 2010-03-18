using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OneSyncATD
{
    [Serializable]
    public class DirectoryNode
    {
        public static event UpdateHandler Update;

        public DateTime Created;
        public FileAttributes Attributes;
        public List<DirectoryNode> SubDirectories = new List<DirectoryNode>();
        public List<FileNode> Files = new List<FileNode>();
        

        public string Name;

        // Constructor
        public DirectoryNode(string name, DateTime created)
        {
            this.Name = name;
            this.Created = created;
        }

        protected static void OnUpdate(string msg, int progress)
        {
            if (Update != null)
                Update(null, new UpdateEventArgs(msg, progress));
        }

        public static DirectoryNode SaveStructure(string path, DirectoryNode node)
        {
            DirectoryInfo root = new DirectoryInfo(path);


            OnUpdate(string.Format("Saving directory {0}", path), 0);

            foreach (DirectoryInfo di in root.GetDirectories())
            {
                try
                {
                    DirectoryNode dnode = new DirectoryNode(di.Name, di.CreationTime);
                    dnode.Attributes = di.Attributes;
                    node.SubDirectories.Add(dnode);
                    SaveStructure(path + '\\' + di.Name, dnode);
                }
                catch (UnauthorizedAccessException) { }
            }

            foreach (FileInfo fi in root.GetFiles())
            {
                try
                {
                    FileNode fnode = new FileNode(fi.Name, fi.Length,
                                                  fi.CreationTime, fi.LastWriteTime, fi.LastAccessTime);
                    fnode.Attributes = fi.Attributes;
                    node.Files.Add(fnode);
                }
                catch (UnauthorizedAccessException) { }

            }
            return node;
        }

        public static void RestoreStructure(DirectoryInfo destDir, DirectoryNode node,
                                            bool randomizeName, RandomGenerator.NameType type)
        {
            foreach (DirectoryNode dnode in node.SubDirectories)
            {
                OnUpdate(string.Format("Restoring directory {0}", dnode.Name), 0);

                if (randomizeName) dnode.Name = RandomGenerator.GetDirectoryName(type);

                DirectoryInfo newDir = destDir.CreateSubdirectory(dnode.Name);
                newDir.CreationTime = dnode.Created;
                newDir.Attributes = dnode.Attributes;

                RestoreStructure(newDir, dnode, randomizeName, type);
            }

            foreach (FileNode fnode in node.Files)
            {
                if (randomizeName) fnode.Name = RandomGenerator.GetFilename(RandomGenerator.NameType.Letters);

                // Create new file
                string newFilePath = destDir.FullName + '\\' + fnode.Name;
                using (FileStream fs = File.Create(newFilePath))
                    fs.SetLength((int)fnode.Size);

                // Set file attributes
                FileInfo fi = new FileInfo(newFilePath);
                fi.CreationTime = fnode.Created;
                fi.LastWriteTime = fnode.Modified;
                fi.LastAccessTime = fnode.Accessed;
                fi.Attributes = fnode.Attributes;
            }

        }
        
        
        
    }

    [Serializable]
    public class FileNode
    {
        public string Name;
        public long Size;
        public DateTime Created, Modified, Accessed;
        public FileAttributes Attributes;


        public FileNode(string name, long size,
                        DateTime created, DateTime modified, DateTime accessed)
        {
            this.Name = name;
            this.Size = size;
            this.Created = created;
            this.Modified = modified;
            this.Accessed = accessed;
        }
    }
}
