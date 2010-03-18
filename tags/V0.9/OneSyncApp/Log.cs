/*
 $Id: Log.cs 66 2010-03-10 07:48:55Z gclin009 $
 */
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
        public string file;
        public string action;
        public string status;

        public LogActivity(string file, string action, string status)
        {
            this.file = file;
            this.action = action;
            this.status = status;
        }
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
        public const string to = "To intermediate storage";
        public const string from = "From intermediate storage";

        public Log()
        {
            // Do nothing since the class functions are intended to be static
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
        public static bool addToLog(string syncDirPath, string storageDirPath, string profileName,
            List<LogActivity> logActivity, string syncDirection, int numOfFilesProcessed,
            DateTime starttime, DateTime endtime)
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

                        // Construct the must-have child nodes for the sync session
                        XmlElement syncSessionNode = xmlLogDoc.CreateElement("syncsession");
                        XmlElement profileNameNode = xmlLogDoc.CreateElement("profilename");
                        profileNameNode.SetAttribute("description", "Profile Name");
                        profileNameNode.InnerText = profileName;
                        XmlElement dateNode = xmlLogDoc.CreateElement("syncdate");
                        dateNode.SetAttribute("description", "Date");
                        dateNode.InnerText = DateTime.Now.ToShortDateString();
                        XmlElement storageDirPathNode = xmlLogDoc.CreateElement("storagedirectory");
                        storageDirPathNode.SetAttribute("description", "Storage directory");
                        storageDirPathNode.InnerText = storageDirPath;
                        XmlElement syncDirectionNode = xmlLogDoc.CreateElement("syncdirection");
                        syncDirectionNode.SetAttribute("description", "Synchronization direction");
                        syncDirectionNode.InnerText = syncDirection;
                        XmlElement numOfFilesProcessedNode = xmlLogDoc.CreateElement("numberoffilesprocessed");
                        numOfFilesProcessedNode.SetAttribute("description", "Number of files processed");
                        numOfFilesProcessedNode.InnerText = numOfFilesProcessed.ToString();

                        // Append the nodes
                        root.AppendChild(syncSessionNode);
                        syncSessionNode.AppendChild(profileNameNode);
                        syncSessionNode.AppendChild(dateNode);
                        syncSessionNode.AppendChild(storageDirPathNode);
                        syncSessionNode.AppendChild(syncDirectionNode);
                        syncSessionNode.AppendChild(numOfFilesProcessedNode);

                        if (logActivity.Count != 0)
                        {
                            XmlElement startTimeNode = xmlLogDoc.CreateElement("starttime");
                            startTimeNode.SetAttribute("description", "Start time");
                            startTimeNode.InnerText = starttime.ToShortTimeString();
                            XmlElement endTimeNode = xmlLogDoc.CreateElement("endtime");
                            endTimeNode.SetAttribute("description", "End time");
                            endTimeNode.InnerText = endtime.ToShortTimeString();

                            // Append nodes
                            syncSessionNode.AppendChild(startTimeNode);
                            syncSessionNode.AppendChild(endTimeNode);

                            foreach (LogActivity la in logActivity)
                            {
                                XmlElement syncRecordNode = xmlLogDoc.CreateElement("syncrecord");
                                XmlElement fileNode = xmlLogDoc.CreateElement("file");
                                XmlElement actionNode = xmlLogDoc.CreateElement("action");
                                XmlElement statusNode = xmlLogDoc.CreateElement("status");

                                fileNode.InnerText = la.file;
                                actionNode.InnerText = la.action;
                                statusNode.InnerText = la.status;

                                syncRecordNode.AppendChild(fileNode);
                                syncRecordNode.AppendChild(actionNode);
                                syncRecordNode.AppendChild(statusNode);

                                syncSessionNode.AppendChild(syncRecordNode);
                            }
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
                string log = "<html><body style='FONT-FAMILY: arial'>\r\n";
                log += "<style type='text/css'>\r\n";
                log += "table.t1 { background-color: #FEFEF2; }";
                log += "tr.da { background-color: #E0ECF8; }\r\n";
                log += "tr.db { background-color: #EFFBEF; }\r\n";
                log += "tr.f { background-color: #F5A9A9; }\r\n";
                log += "tr.dh th { background-color: #FCF6CF; }\r\n";
                log += "tr.d0 td { background-color: #FCF6CF; }\r\n";
                log += "tr.d1 td { background-color: #FEFEF2; }\r\n";
                log += "</style>\r\n";
                Hashtable pairs = loadSyncDirAndLogFilePairs();

                if (pairs.ContainsKey(syncDirPath))
                {
                    string logFileName = (String)pairs[syncDirPath];
                    string logFilePath = String.Concat(getAppDir(), @"\", parentDirectoryName, @"\", logFileName);

                    XmlDocument doc = new XmlDocument();
                    doc.Load(logFilePath);

                    // Folder directory first
                    XmlElement root = doc.DocumentElement;
                    log += "<table width='100%'>\r\n<tr class='da'><td width='20%'>" + root.GetAttribute("description") + "</td>\r\n";
                    log += "<td>: " + root.GetAttribute("value") + "</td></tr></table></br></br>\r\n";

                    XmlNodeList syncSessionsNodesList = root.SelectNodes("syncsession");

                    if (showLatestOnly)
                    {
                        log += returnProcessXML(syncSessionsNodesList[syncSessionsNodesList.Count - 2]);
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

        #region private methods
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

        private static string returnProcessXML(XmlNode syncSessionNode)
        {
            string log = "";

            log += "<table class='t1' width='100%'>\r\n<tr'>\r\n<td width='20%'>" + syncSessionNode["profilename"].GetAttribute("description") + "</td>\r\n";
            log += "<td>: " + syncSessionNode["profilename"].InnerText + "</td>\r\n</tr>\r\n";
            log += "<tr>\r\n<td width='20%'>" + syncSessionNode["syncdate"].GetAttribute("description") + "</td>\r\n";
            log += "<td>: " + syncSessionNode["syncdate"].InnerText + "</td>\r\n</tr>\r\n";
            log += "<tr>\r\n<td width='20%'>" + syncSessionNode["storagedirectory"].GetAttribute("description") + "</td>\r\n";
            log += "<td>: " + syncSessionNode["storagedirectory"].InnerText + "</td>\r\n</tr>\r\n";
            log += "<tr>\r\n<td width='20%'>" + syncSessionNode["syncdirection"].GetAttribute("description") + "</td>\r\n";
            log += "<td>: " + syncSessionNode["syncdirection"].InnerText + "</td>\r\n</td>\r\n";

            int numberOfFilesProcessed = Int32.Parse(syncSessionNode["numberoffilesprocessed"].InnerText);

            log += "<tr>\r\n<td width='20%'>" + syncSessionNode["numberoffilesprocessed"].GetAttribute("description") + "</td>\r\n";

            if (numberOfFilesProcessed == 0)
            {
                log += "<td>: No files were transferred</td>\r\n</tr>\r\n";
            }
            else
            {
                log += "<td>: " + syncSessionNode["numberoffilesprocessed"].InnerText + "</td>\r\n</tr>\r\n";
                log += "<tr>\r\n<td width='20%'>" + syncSessionNode["starttime"].GetAttribute("description") + "</td>\r\n";
                log += "<td>: " + syncSessionNode["starttime"].InnerText + "</td>\r\n</tr>\r\n";
                log += "<tr>\r\n<td width='20%'>" + syncSessionNode["endtime"].GetAttribute("description") + "</td>\r\n";
                log += "<td>: " + syncSessionNode["endtime"].InnerText + "</td>\r\n</tr>\r\n";

                XmlNodeList syncRecordsNodesList = syncSessionNode.SelectNodes("syncrecord");

                log += "<table width='100%' cellpadding=3>\r\n<tr class='dh' align=left><th>File</th><th>Action</th><th>Status</th></tr>\r\n";

                int count = 0;
                string altColor;
                foreach (XmlNode syncRecordNode in syncRecordsNodesList)
                {
                    string file = syncRecordNode["file"].InnerText;
                    string action = syncRecordNode["action"].InnerText;
                    string status = syncRecordNode["status"].InnerText;

                    if (status.ToUpper().IndexOf("FAIL") >= 0) altColor = "class='f'";
                    else if (count++ % 2 == 0) altColor = "class='d1'";
                    else altColor = "class='d0'";


                    log += "<tr " + altColor + ">\r\n<td width='75%'>" + file + "</td>\r\n";
                    log += "<td>" + action + "</td>\r\n";
                    log += "<td>" + status + "</td>\r\n</tr>\r\n";
                }
            }

            log += "</table>\r\n";
            log += "</br></br></br></br>";

            return log;
        }
        #endregion
    }
}
