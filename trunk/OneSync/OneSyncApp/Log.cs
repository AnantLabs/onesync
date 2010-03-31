/*
 $Id$
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.Win32;

namespace OneSync
{
    /// <summary>
    /// Structure for describing a log activity
    /// </summary>
    public struct LogActivity
    {
        private string file, action, status;

        public LogActivity(string file, string action, string status)
        {
            this.file = file;
            this.action = action;
            this.status = status;
        }

        public string File
        {
            get
            {
                return file;
            }
            set
            {
                file = value;
            }
        }

        public string Action
        {
            get
            {
                return action;
            }
            set
            {
                action = value;
            }
        }

        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
            }
        }
    }

    /// <summary>
    /// Provides useful methods to record down activities after synchronization as well as 
    /// to manage other log-related actions
    /// </summary>
    public class Log
    {
        private const string ParentDirectoryName = "OneSync";
        private const string SyncDirAndLogFilePairsLocation = "logpairs.dat";
        private const string XslFileName = "onesynclog.xsl";
        private const string ExceptionLogFileName = "exceptionlog.txt";

        public const string To = "To intermediate storage";
        public const string From = "From intermediate storage";

        public Log()
        {
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
        public static bool AddToLog(string syncDirPath, string storageDirPath, string profileName,
            ICollection<LogActivity> logActivity, string syncDirection, int numOfFilesProcessed,
            DateTime startTime, DateTime endTime)
        {
            bool success = true;
            bool fileNotFound = false;
            Hashtable pairs = null;

            if (CheckDirectory())
            {
                try
                {
                    pairs = LoadSyncDirAndLogFilePairs();
                }
                catch (FileNotFoundException)
                {
                    fileNotFound = true;
                }
                catch (Exception e)
                {
                    success = false;
                    LogException(e);
                }

                if (success)
                {
                    if (fileNotFound) pairs = new Hashtable();

                    string logFilePath;

                    // Check if the pair already exists
                    if (pairs.ContainsKey(syncDirPath))
                    {
                        string logFileName = (String)pairs[syncDirPath];
                        logFilePath = String.Concat(GetAppDir(), @"\", ParentDirectoryName, @"\", logFileName);
                    }
                    else
                    {
                        string logFileName = string.Format("{0}{1}", DateTime.Now.Ticks, ".xml");
                        logFilePath = String.Concat(GetAppDir(), @"\", ParentDirectoryName, @"\", logFileName);

                        // Add to the pairs
                        pairs.Add(syncDirPath, logFileName);
                    }

                    try
                    {
                        // Create the document if file not exists
                        if (!(File.Exists(logFilePath)))
                        {
                            CreateXMLDocument(logFilePath, syncDirPath);
                        }

                        // Save log details
                        SaveLogDetails(logFilePath, profileName, storageDirPath, syncDirection,
                            numOfFilesProcessed, startTime, endTime, logActivity);

                        StoreSyncDirAndLogFilePairs(pairs);
                    }
                    catch (Exception e)
                    {
                        success = false;
                        LogException(e);
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
        /// Returns the path of log file given the sync source directory
        /// </summary>
        /// <param name="syncDirPath">Path of source directory.</param>
        /// <returns>Log file path</returns>
        public static string ReturnLogReportPath(string syncDirPath)
        {
            string logFilePath = "";

            try
            {
                Hashtable pairs = LoadSyncDirAndLogFilePairs();

                if (pairs.ContainsKey(syncDirPath))
                {
                    string logFileName = (String)pairs[syncDirPath];
                    logFilePath = String.Concat(GetAppDir(), @"\", ParentDirectoryName, @"\", logFileName);
                }
            }
            catch (Exception e)
            {
                LogException(e);
            }

            return logFilePath;
        }

        /// <summary>
        /// Open log report using browser
        /// </summary>
        /// <param name="syncDirPath">Path of source directory.</param>
        public static void ShowLog(String syncDirPath)
        {
            Hashtable pairs = LoadSyncDirAndLogFilePairs();
            string logFileName = (string)pairs[syncDirPath];
            string logFilePath = String.Concat(GetAppDir(), @"\", ParentDirectoryName, @"\", logFileName);

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = GetDefaultBrowser();
            proc.StartInfo.Arguments = logFilePath;
            proc.Start();
        }

        /// <summary>
        /// Log the exception to the file
        /// </summary>
        /// <param name="e">Exception occurred</param>
        public static void LogException(Exception e)
        {
            if (CheckDirectory())
            {
                string logFilePath = String.Concat(GetAppDir(), @"\", ParentDirectoryName, @"\", ExceptionLogFileName);
                FileStream fs = null;
                StreamWriter sw = null;

                try
                {
                    if (File.Exists(logFilePath)) // Append to existing exception log file
                    {
                        fs = new FileStream(logFilePath, FileMode.Append);
                        sw = new StreamWriter(fs);
                        sw.WriteLine(FormatExcetpionLog(e));
                    }
                    else // Create a new exception log file
                    {
                        fs = new FileStream(logFilePath, FileMode.Create);
                        sw = new StreamWriter(fs);
                        sw.WriteLine(FormatExcetpionLog(e));
                    }
                }
                catch (Exception)
                {
                    // nested log?
                }
                finally
                {
                    sw.Close();
                    fs.Close();
                }
            }
        }

        #region private methods
        private static string GetAppDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        private static bool CheckDirectory()
        {
            bool success = true;
            string path = string.Concat(GetAppDir(), @"\", ParentDirectoryName);

            try
            {
                // Determine whether the directory exists
                // if not, create the directory
                if (!(Directory.Exists(path))) Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                success = false;
                LogException(e);
            }

            return success;
        }

        private static bool StoreSyncDirAndLogFilePairs(Hashtable pairs)
        {
            bool success = true;

            if (CheckDirectory())
            {
                // serialize
                string filePath = string.Concat(GetAppDir(), @"\",
                    ParentDirectoryName, @"\", SyncDirAndLogFilePairsLocation);

                Stream stream = null;

                try
                {
                    stream = File.Open(filePath, FileMode.Create);
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, pairs);
                }
                catch (IOException ioe)
                {
                    success = false;
                    LogException(ioe);
                }
                finally
                {
                    stream.Close();
                }

                // create xls if not exists
                string xlsFilePath = string.Concat(GetAppDir(), @"\", ParentDirectoryName, @"\", XslFileName);
                if (!File.Exists(xlsFilePath)) success = CreateXSL(xlsFilePath);
            }
            else
            {
                success = false;
            }

            return success;
        }

        private static Hashtable LoadSyncDirAndLogFilePairs()
        {
            string filePath = string.Concat(GetAppDir(), @"\",
                    ParentDirectoryName, @"\", SyncDirAndLogFilePairsLocation);

            Stream stream = File.Open(filePath, FileMode.Open);
            BinaryFormatter bin = new BinaryFormatter();
            Hashtable pairs = (Hashtable)bin.Deserialize(stream);
            stream.Close();
            return pairs;
        }

        private static void CreateXMLDocument(string logFilePath, string syncDirPath)
        {
            XmlTextWriter xmlWriter = new XmlTextWriter(logFilePath, System.Text.Encoding.UTF8);
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            xmlWriter.WriteProcessingInstruction("xml-stylesheet", "type='text/xsl' href='" + XslFileName + "'");
            xmlWriter.WriteStartElement("syncdirectory");
            xmlWriter.WriteAttributeString("value", syncDirPath);
            xmlWriter.WriteEndElement();
            xmlWriter.Close();
        }

        private static void SaveLogDetails(string logFilePath, string profileName, string storageDirPath,
            string syncDirection, int numOfFilesProcessed, DateTime startTime, DateTime endTime,
            ICollection<LogActivity> logActivity)
        {
            // Load the document
            XmlDocument xmlLogDoc = new XmlDocument();
            xmlLogDoc.Load(logFilePath);

            // Get the root element
            XmlNode root = xmlLogDoc.DocumentElement;

            // Construct the must-have child nodes for the sync session
            XmlElement syncSessionNode = xmlLogDoc.CreateElement("syncsession");
            XmlElement profileNameNode = xmlLogDoc.CreateElement("profilename");
            profileNameNode.InnerText = RemoveTags(profileName);
            XmlElement dateNode = xmlLogDoc.CreateElement("syncdate");
            dateNode.InnerText = DateTime.Now.ToShortDateString();
            XmlElement storageDirPathNode = xmlLogDoc.CreateElement("storagedirectory");
            storageDirPathNode.InnerText = storageDirPath;
            XmlElement syncDirectionNode = xmlLogDoc.CreateElement("syncdirection");
            syncDirectionNode.InnerText = syncDirection;
            XmlElement numOfFilesProcessedNode = xmlLogDoc.CreateElement("numberoffilesprocessed");
            numOfFilesProcessedNode.InnerText = numOfFilesProcessed.ToString();

            // Prepend the node; the last log will be displayed first
            root.PrependChild(syncSessionNode);
            syncSessionNode.AppendChild(profileNameNode);
            syncSessionNode.AppendChild(dateNode);
            syncSessionNode.AppendChild(storageDirPathNode);
            syncSessionNode.AppendChild(syncDirectionNode);
            syncSessionNode.AppendChild(numOfFilesProcessedNode);

            if (logActivity.Count != 0)
            {
                XmlElement startTimeNode = xmlLogDoc.CreateElement("starttime");
                startTimeNode.InnerText = startTime.ToShortTimeString();
                XmlElement endTimeNode = xmlLogDoc.CreateElement("endtime");
                endTimeNode.InnerText = endTime.ToShortTimeString();

                // Append nodes
                syncSessionNode.AppendChild(startTimeNode);
                syncSessionNode.AppendChild(endTimeNode);

                foreach (LogActivity la in logActivity)
                {
                    XmlElement syncRecordNode = xmlLogDoc.CreateElement("syncrecord");
                    XmlElement fileNode = xmlLogDoc.CreateElement("file");
                    XmlElement actionNode = xmlLogDoc.CreateElement("action");
                    XmlElement statusNode = xmlLogDoc.CreateElement("status");

                    fileNode.InnerText = la.File;
                    actionNode.InnerText = la.Action;
                    statusNode.InnerText = la.Status;

                    syncRecordNode.AppendChild(fileNode);
                    syncRecordNode.AppendChild(actionNode);
                    syncRecordNode.AppendChild(statusNode);

                    syncSessionNode.AppendChild(syncRecordNode);
                }
            }

            xmlLogDoc.Save(logFilePath);
        }

        private static bool CreateXSL(string filePath)
        {
            bool success = true;
            StringBuilder sb = new StringBuilder();
            FileStream fs = null;
            StreamWriter sw = null;

            try
            {
                sb.Append("<?xml version='1.0' encoding='ISO-8859-1'?>\r\n");
                sb.Append("<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\r\n");
                sb.Append("<xsl:template match='/'>\r\n");
                sb.Append("<html>\r\n");
                sb.Append("<body style='FONT-FAMILY: arial'>\r\n");
                sb.Append("<style type='text/css'>\r\n");
                sb.Append("table.t1 { background-color: #FEFEF2; }tr.da { background-color: #E0ECF8; }\r\n");
                sb.Append("tr.db { background-color: #EFFBEF; }\r\n");
                sb.Append("tr.f { background-color: #F5A9A9; }\r\n");
                sb.Append("tr.dh th { background-color: #FCF6CF; }\r\n");
                sb.Append("tr.d0 td { background-color: #FCF6CF; }\r\n");
                sb.Append("tr.d1 td { background-color: #FEFEF2; }\r\n");
                sb.Append("</style>\r\n");
                sb.Append("<table width='100%'>\r\n");
                sb.Append("<tr class='da'><td width='20%'>Synchronization directory</td>\r\n");
                sb.Append("<td>: <xsl:value-of select='syncdirectory/@value'/></td></tr>\r\n");
                sb.Append("</table>\r\n");
                sb.Append("<br></br><br></br>\r\n");
                sb.Append("<xsl:for-each select='syncdirectory/syncsession'>\r\n");
                sb.Append("<table class='t1' width='100%'>\r\n");
                sb.Append("<tr><td width='20%'>Profile Name</td><td>: <xsl:value-of select='profilename'/></td></tr>\r\n");
                sb.Append("<tr><td width='20%'>Date</td><td>: <xsl:value-of select='syncdate'/></td></tr>\r\n");
                sb.Append("<tr><td width='20%'>Storage directory</td><td>: <xsl:value-of select='storagedirectory'/></td></tr>\r\n");
                sb.Append("<tr><td width='20%'>Synchronization direction</td><td>: <xsl:value-of select='syncdirection'/></td></tr>\r\n");
                sb.Append("<tr><td width='20%'>Number of files processed</td><td>: <xsl:value-of select='numberoffilesprocessed'/></td></tr>\r\n");
                sb.Append("<xsl:if test='syncrecord'>\r\n");
                sb.Append("<tr><td width='20%'>Start time</td><td>: <xsl:value-of select='starttime'/></td></tr>\r\n");
                sb.Append("<tr><td width='20%'>End time</td><td>: <xsl:value-of select='endtime'/></td></tr>\r\n");
                sb.Append("<tr>\r\n");
                sb.Append("<td colspan='2'>\r\n");
                sb.Append("<table width='100%' cellpadding='3'>\r\n");
                sb.Append("<tr class='dh' align='left'><th width='75%'>File</th><th>Action</th><th>Status</th></tr>\r\n");
                sb.Append("<xsl:for-each select='syncrecord'>\r\n");
                sb.Append("<xsl:choose>\r\n");
                sb.Append("<xsl:when test=\"status='FAIL'\">\r\n");
                sb.Append("<tr class='f'>\r\n");
                sb.Append("<td width='75%'><xsl:value-of select='file'/></td>\r\n");
                sb.Append("<td width='75%'><xsl:value-of select='action'/></td>\r\n");
                sb.Append("<td width='75%'><xsl:value-of select='status'/></td>\r\n");
                sb.Append("</tr>\r\n");
                sb.Append("</xsl:when>\r\n");
                sb.Append("<xsl:otherwise>\r\n");
                sb.Append("<xsl:choose>\r\n");
                sb.Append("<xsl:when test='(position() mod 2 = 1)'>\r\n");
                sb.Append("<tr class='d1'>\r\n");
                sb.Append("<td width='75%'><xsl:value-of select='file'/></td>\r\n");
                sb.Append("<td width='75%'><xsl:value-of select='action'/></td>\r\n");
                sb.Append("<td width='75%'><xsl:value-of select='status'/></td>\r\n");
                sb.Append("</tr>\r\n");
                sb.Append("</xsl:when>\r\n");
                sb.Append("<xsl:otherwise>\r\n");
                sb.Append("<tr class='d0'>\r\n");
                sb.Append("<td width='75%'><xsl:value-of select='file'/></td>\r\n");
                sb.Append("<td width='75%'><xsl:value-of select='action'/></td>\r\n");
                sb.Append("<td width='75%'><xsl:value-of select='status'/></td>\r\n");
                sb.Append("</tr>\r\n");
                sb.Append("</xsl:otherwise>\r\n");
                sb.Append("</xsl:choose>\r\n");
                sb.Append("</xsl:otherwise>\r\n");
                sb.Append("</xsl:choose>\r\n");
                sb.Append("</xsl:for-each>\r\n");
                sb.Append("</table>\r\n");
                sb.Append("</td>\r\n");
                sb.Append("</tr>\r\n");
                sb.Append("</xsl:if>\r\n");
                sb.Append("</table>\r\n");
                sb.Append("<br></br><br></br><br></br><br></br>\r\n");
                sb.Append("</xsl:for-each>\r\n");
                sb.Append("</body>\r\n");
                sb.Append("</html>\r\n");
                sb.Append("</xsl:template>\r\n");
                sb.Append("</xsl:stylesheet>\r\n");

                fs = new FileStream(filePath, FileMode.Create);
                sw = new StreamWriter(fs);
                sw.Write(sb);
            }
            catch (Exception e)
            {
                success = false;
                LogException(e);
            }
            finally
            {
                sw.Close();
                fs.Close();
            }

            return success;
        }

        private static string RemoveTags(String s)
        {
            s = Regex.Replace(s, "<", "&lt;");
            s = Regex.Replace(s, ">", "&gt;");
            return s;
        }

        /// <summary>
        /// Returns the default system browser
        /// source: http://ryanfarley.com/blog/archive/2004/05/16/649.aspx
        /// </summary>
        /// <returns>Default browser name e.g. iexplorer.exe</returns>
        private static string GetDefaultBrowser()
        {
            string browser = string.Empty;
            RegistryKey key = null;
            try
            {
                key = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false);

                //trim off quotes
                browser = key.GetValue(null).ToString().ToLower().Replace("\"", "");
                if (!browser.EndsWith("exe"))
                {
                    //get rid of everything after the ".exe"
                    browser = browser.Substring(0, browser.LastIndexOf(".exe") + 4);
                }
            }
            finally
            {
                if (key != null) key.Close();
            }
            return browser;
        }

        private static string FormatExcetpionLog(Exception e)
        {
            StackTrace st = new StackTrace(e, true);
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("Date\t\t: {0:F}\r\n", DateTime.Now.ToString()));
            sb.Append(String.Format("File\t\t: {0}\r\n", st.GetFrame(st.FrameCount - 1).GetFileName()));
            sb.Append(String.Format("Message\t\t: {0}\r\n", e.Message));
            sb.Append(String.Format("Details\t\t:\r\n{0}\r\n\r\n\r\n", e.ToString()));
            return sb.ToString();
        }

        #endregion
    }
}
