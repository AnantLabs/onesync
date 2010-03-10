using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;

namespace OneSync
{
    /// <summary>
    /// Structure for describing a log activity
    /// </summary>
    public struct LogActivity
    {
        public string sourceFilePath;
        public string storageFilePath;
        public string status;
        public string logMessage;
        public DateTime startTime;
        public DateTime endTime;
    }

    /// <summary>
    /// Provides useful methods to record down activities after synchronization as well as 
    /// to manage other log-related actions
    /// Only the methods addToLog and returnLogReportPath are to be called from outside
    /// The rest of the methods are for internal use only
    /// </summary>
    public class Log
    {
        public const string parentDirectoryName = "OneSync";
        public const string syncDirAndLogFilePairsLocation = "logpairs.dat";

        public Log()
        {
            // Do nothing since the class functions are intended to be static
        }

        private static string getAppDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        private static bool checkDirectory()
        {
            bool success = true;
            string path = string.Concat(getAppDir(), @"\", parentDirectoryName);

            try
            {
                // Determine whether the directory exists
                // if not, create the directory
                if (!(Directory.Exists(path))) Directory.CreateDirectory(path);
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        private static bool storeSyncDirAndLogFilePairs(Hashtable pairs)
        {
            bool success = true;

            if (checkDirectory())
            {
                string filePath = string.Concat(getAppDir(), @"\",
                    parentDirectoryName, @"\", syncDirAndLogFilePairsLocation);

                try
                {
                    Stream stream = File.Open(filePath, FileMode.Create);
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, pairs);
                    stream.Close();
                }
                catch (IOException)
                {
                    success = false;
                }
            }
            else
            {
                success = false;
            }

            return success;
        }

        private static Hashtable loadSyncDirAndLogFilePairs()
        {
            string filePath = string.Concat(getAppDir(), @"\",
                    parentDirectoryName, @"\", syncDirAndLogFilePairsLocation);

            Stream stream = File.Open(filePath, FileMode.Open);
            BinaryFormatter bin = new BinaryFormatter();
            Hashtable pairs = (Hashtable)bin.Deserialize(stream);
            stream.Close();
            return pairs;
        }

        /// <summary>
        /// Add log records to the log file after each sync operation.
        /// More parameters will be added as required.
        /// </summary>
        /// <param name="syncDirPath">Path of source directory.</param>
        /// <param name="storageDirPath">Path of storage directory. Put "DropBox" if the operation is online sync.</param>
        /// <param name="logActivity">List of LogActivity structs.</param>
        /// <param name="syncDirection">Indicates the direction of synchronization.</param>
        /// <param name="numOfFilesProcessed">Number of files that have been processed in the synchronization.</param>
        /// <returns></returns>
        public static bool addToLog(String syncDirPath, String storageDirPath, List<LogActivity> logActivity,
            String syncDirection, int numOfFilesProcessed)
        {
            bool success = true;
            bool fileNotFound = false;
            Hashtable pairs = null;

            if (checkDirectory())
            {
                try
                {
                    pairs = loadSyncDirAndLogFilePairs();
                }
                catch (FileNotFoundException)
                {
                    fileNotFound = true;
                }
                catch (Exception)
                {
                    success = false;
                }

                if (success)
                {
                    if (fileNotFound) pairs = new Hashtable();

                    string logFilePath;

                    // Check if the pair already exists
                    if (pairs.ContainsKey(syncDirPath))
                    {
                        string logFileName = (String)pairs[syncDirPath];
                        logFilePath = String.Concat(getAppDir(), @"\", parentDirectoryName, @"\", logFileName);
                    }
                    else
                    {
                        string logFileName = string.Format("{0}{1}", DateTime.Now.Ticks, ".xml");
                        logFilePath = String.Concat(getAppDir(), @"\", parentDirectoryName, @"\", logFileName);

                        // Add to the pairs
                        pairs.Add(syncDirPath, logFileName);
                    }

                    try
                    {
                        // Create the document if file not exists
                        if (!(File.Exists(logFilePath)))
                        {
                            XmlTextWriter xmlWriter = new XmlTextWriter(logFilePath, System.Text.Encoding.UTF8);
                            xmlWriter.Formatting = Formatting.Indented;
                            xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
                            xmlWriter.WriteStartElement("syncdirectory");
                            xmlWriter.WriteAttributeString("description", "Synchronization directory");
                            xmlWriter.WriteAttributeString("value", syncDirPath);
                            xmlWriter.WriteEndElement();
                            xmlWriter.Close();
                        }

                        // Load the document
                        XmlDocument xmlLogDoc = new XmlDocument();
                        xmlLogDoc.Load(logFilePath);

                        // Get the root element
                        XmlNode root = xmlLogDoc.DocumentElement;

                        // Construct the child nodes for the sync session
                        XmlElement syncSessionNode = xmlLogDoc.CreateElement("syncsession");
                        XmlElement storageDirPathNode = xmlLogDoc.CreateElement("storagedirectory");
                        storageDirPathNode.SetAttribute("description", "Storage directory");
                        storageDirPathNode.InnerText = storageDirPath;
                        XmlElement syncDirectionNode = xmlLogDoc.CreateElement("syncdirection");
                        syncDirectionNode.SetAttribute("description", "Synchronization direction");
                        syncDirectionNode.InnerText = syncDirection;
                        XmlElement numOfFilesProcessedNode = xmlLogDoc.CreateElement("numberoffilesprocessed");
                        numOfFilesProcessedNode.SetAttribute("description", "Number of files processed");
                        numOfFilesProcessedNode.InnerText = numOfFilesProcessed.ToString();
                        XmlElement dateTimeNode = xmlLogDoc.CreateElement("syncdate");
                        dateTimeNode.SetAttribute("description", "Date");
                        dateTimeNode.InnerText = DateTime.Now.Date.ToString();

                        root.AppendChild(syncSessionNode);
                        syncSessionNode.AppendChild(storageDirPathNode);
                        syncSessionNode.AppendChild(syncDirectionNode);
                        syncSessionNode.AppendChild(numOfFilesProcessedNode);
                        syncSessionNode.AppendChild(dateTimeNode);

                        foreach (LogActivity la in logActivity)
                        {
                            XmlElement syncRecordNode = xmlLogDoc.CreateElement("syncrecord");
                            XmlElement sourceFilePathNode = xmlLogDoc.CreateElement("sourcepath");
                            XmlElement storageFilePathNode = xmlLogDoc.CreateElement("storagepath");
                            XmlElement statusNode = xmlLogDoc.CreateElement("status");
                            XmlElement logMessageNode = xmlLogDoc.CreateElement("logmessage");
                            XmlElement startTimeNode = xmlLogDoc.CreateElement("starttime");
                            XmlElement endTimeNode = xmlLogDoc.CreateElement("endtime");

                            sourceFilePathNode.InnerText = la.sourceFilePath;
                            storageFilePathNode.InnerText = la.storageFilePath;
                            statusNode.InnerText = la.status;
                            logMessageNode.InnerText = la.logMessage;
                            startTimeNode.InnerText = la.startTime.TimeOfDay.ToString();
                            endTimeNode.InnerText = la.endTime.TimeOfDay.ToString();

                            syncRecordNode.AppendChild(sourceFilePathNode);
                            syncRecordNode.AppendChild(storageFilePathNode);
                            syncRecordNode.AppendChild(statusNode);
                            syncRecordNode.AppendChild(logMessageNode);
                            syncRecordNode.AppendChild(startTimeNode);
                            syncRecordNode.AppendChild(endTimeNode);
                            syncSessionNode.AppendChild(syncRecordNode);
                        }



                        xmlLogDoc.Save(logFilePath);

                        storeSyncDirAndLogFilePairs(pairs);
                    }
                    catch (Exception)
                    {
                        success = false;
                    }
                }
            }
            else
            {
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Extracts log records from the log file, process the records to convert 
        /// them to HTML format and pass the resulting information to default browser.
        /// </summary>
        /// <param name="syncDirPath">Path of source directory.</param>
        /// <param name="showLatestOnly">True will show only the records for the latest sync session.</param>
        /// <returns></returns>
        public static string returnLogReportPath(String syncDirPath, bool showLatestOnly)
        {
            string htmlfile;

            try
            {
                string log = "<html><body style='FONT-FAMILY: arial'>";
                Hashtable pairs = loadSyncDirAndLogFilePairs();

                if (pairs.ContainsKey(syncDirPath))
                {
                    string logFileName = (String)pairs[syncDirPath];
                    string logFilePath = String.Concat(getAppDir(), @"\", parentDirectoryName, @"\", logFileName);

                    XmlDocument doc = new XmlDocument();
                    doc.Load(logFilePath);

                    // Folder directory first
                    XmlElement root = doc.DocumentElement;
                    log += "<table width=800><tr><td width='30%'>" + root.GetAttribute("description") + "</td>";
                    log += "<td>: " + root.GetAttribute("value") + "</td></tr></table></br></br>";

                    XmlNodeList syncSessionsNodesList = root.SelectNodes("syncsession");

                    if (showLatestOnly)
                    {
                        log += returnProcessXML(syncSessionsNodesList[syncSessionsNodesList.Count - 1]);
                    }
                    else
                    {
                        foreach (XmlNode syncSessionNode in syncSessionsNodesList)
                        {
                            log += returnProcessXML(syncSessionNode);
                        }
                    }
                }

                log += "</body></html>";

                htmlfile = string.Concat(getAppDir(), @"\", parentDirectoryName, @"\", "logreport.html");
                FileStream fs = new FileStream(htmlfile, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(log);
                sw.Close();
                fs.Close();
            }
            catch (Exception e)
            {
                htmlfile = e.ToString();
            }

            return htmlfile;
        }

        private static string returnProcessXML(XmlNode syncSessionNode)
        {
            string log = "";

            log += "<table width=800><tr><td width='30%'>" + syncSessionNode["storagedirectory"].GetAttribute("description") + "</td>";
            log += "<td>: " + syncSessionNode["storagedirectory"].InnerText + "</td></tr>";
            log += "<tr><td width='30%'>" + syncSessionNode["syncdirection"].GetAttribute("description") + "</td>";
            log += "<td>: " + syncSessionNode["syncdirection"].InnerText + "</td></td>";
            log += "<tr><td width='30%'>" + syncSessionNode["numberoffilesprocessed"].GetAttribute("description") + "</td>";
            log += "<td>: " + syncSessionNode["numberoffilesprocessed"].InnerText + "</td></td>";
            log += "<tr><td width='30%'>" + syncSessionNode["syncdate"].GetAttribute("description") + "</td>";
            log += "<td>: " + syncSessionNode["syncdate"].InnerText + "</td></td>";
            log += "</table>";

            XmlNodeList syncRecordsNodesList = syncSessionNode.SelectNodes("syncrecord");

            log += "<table width=800><tr><th>Source</th><th>Storage</th><th>Status</th><th>Message</th><th>Start Time</th><th>End Time</th></tr>";

            foreach (XmlNode syncRecordNode in syncRecordsNodesList)
            {
                log += "<tr><td>" + syncRecordNode["sourcepath"].InnerText + "</td>";
                log += "<td>" + syncRecordNode["storagepath"].InnerText + "</td>";
                log += "<td>" + syncRecordNode["status"].InnerText + "</td>";
                log += "<td>" + syncRecordNode["logmessage"].InnerText + "</td>";
                log += "<td>" + syncRecordNode["starttime"].InnerText + "</td>";
                log += "<td>" + syncRecordNode["endtime"].InnerText + "</td></tr>";
            }

            log += "</table>";
            log += "</br></br>";

            return log;
        }
    }
}
