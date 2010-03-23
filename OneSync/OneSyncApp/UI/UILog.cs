using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace OneSync.UI
{
    class UILog
    {
        private ObservableCollection<UILogEntry> _LogsEntries = new ObservableCollection<UILogEntry>();


        //The enum of status in log.
        //The status of the process:
        //  0 - Completely processed;
        //  1 - Conflict.
        public enum Status
        {
            Completed = 0,
            Conflict
        };
        
        public ObservableCollection<UILogEntry> LogEntries
        {
            get { return _LogsEntries; }
        }

        /// <summary>
        /// Once a file in the source/storage directory is processed, a new log will be added.
        /// </summary>
        /// <param name="fileName">The name of the processed file.</param>
        /// <param name="status">The status of the process: 0 - Completely processed; 1 - Conflict.</param>
        /// <param name="message">A detailed message.</param>
        public void Add(string fileName, Status status, string message)
        {
            UILogEntry entry = new UILogEntry(fileName);

            switch (status)
            {
                case (Status.Completed):
                    entry.ImageSrc = "Resource/completed_icon.gif";
                    entry.Status = "Completely processed";
                    break;
                case (Status.Conflict):
                    entry.ImageSrc = "Resource/conflicting_icon.gif";
                    entry.Status = "Conflict";
                    break;
                default: //Will be improved in V1.0.
                    entry.ImageSrc = "Resource/completed_icon.gif";
                    entry.Status = "Completely processed";
                    break;
            }

            entry.Message = message;

            //Add a new log entry to the log collection.
            _LogsEntries.Add(entry);
        }
    }

    // Private class to represent a log entry
    public class UILogEntry
    {
        public UILogEntry(string filename)
        {
            this.FileName = filename;
        }

        public string ImageSrc { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }

}
