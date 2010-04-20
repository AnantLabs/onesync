//Coded by Nguyen van Thuat
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Synchronization
{
    class FileMetadataSyncActionsComparer
    {
        private List<FileMetaDataItem> onlyInMetadata = new List<FileMetaDataItem>();
        
        public FileMetadataSyncActionsComparer (IList<SyncAction> actions, IList<FileMetaDataItem> metaDataItems)
        {
            Compare(actions, metaDataItems);
        }

        private void Compare (IList<SyncAction> actions, IList<FileMetaDataItem> metaDataItems)
        {
            IList<FileMetaDataItem> dummyItems = new List<FileMetaDataItem>();
            foreach (var action in actions)
            {
                    dummyItems.Add(new FileMetaDataItem("", action.RelativeFilePath, action.FileHash, DateTime.Now,
                                                    0, 0));
            }

            IEnumerable<FileMetaDataItem> inMDOnly = from mdItem in metaDataItems
                                                         where
                                                             !dummyItems.Contains(mdItem, new FileMetaDataItemComparer())
                                                         select mdItem;

            onlyInMetadata.Clear();
            onlyInMetadata.AddRange(inMDOnly.ToList());
        }

        public List<FileMetaDataItem> InMetaDataOnly
        {
            get { return onlyInMetadata; }
        }
    }
}
