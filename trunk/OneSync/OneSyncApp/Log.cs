//Coded by Naing Tayza Htoon
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
using System.Resources;

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
        public static ResourceManager m_ResourceManager = new ResourceManager(Properties.Settings.Default.LanguageResx,
                                    System.Reflection.Assembly.GetExecutingAssembly());

        private const string ParentDirectoryName = "Log";
        private const string SyncDirAndLogFilePairsLocation = "logpairs.dat";
        private static string XslFileName = m_ResourceManager.GetString("log_xslFileName");
        private const string ExceptionLogFileName = "exceptionlog.txt";
        private const long MaxLogFileSize = 100000000; // Roughly 10MB
        public static string To = m_ResourceManager.GetString("log_toIntermediateStorage");
        public static string From = m_ResourceManager.GetString("log_fromIntermediateStorage");

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
        public static bool AddToLog(string syncDirPath, string storageDirPath, string jobName,
            ICollection<LogActivity> logActivity, string syncDirection, int numOfFilesProcessed,
            DateTime startTime, DateTime endTime)
        {
            bool success = true;

            if (CheckDirectory())
            {
                Hashtable pairs = LoadSyncDirAndLogFilePairs();

                if (pairs == null) pairs = new Hashtable();

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

                    // Check log file size is more than max
                    bool moreThanMax = false;
                    moreThanMax = checkLogFileSize(logFilePath);

                    // Save log details
                    SaveLogDetails(logFilePath, moreThanMax, jobName, storageDirPath, syncDirection, numOfFilesProcessed, startTime, endTime, logActivity);

                    StoreSyncDirAndLogFilePairs(pairs);
                }
                catch (Exception e)
                {
                    success = false;
                    LogException(e);
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

            Hashtable pairs = LoadSyncDirAndLogFilePairs();

            if ((pairs != null) && (pairs.ContainsKey(syncDirPath)))
            {
                string logFileName = (String)pairs[syncDirPath];
                logFilePath = String.Concat(GetAppDir(), @"\", ParentDirectoryName, @"\", logFileName);
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

            string logFileName = "";
            if (pairs != null) logFileName = (string)pairs[syncDirPath];
            string logFilePath = String.Concat(GetAppDir(), @"\", ParentDirectoryName, @"\", logFileName);

            // convert to url for all browsers, otherwise only compatible with IE
            string logFilePathURL = System.Web.HttpUtility.UrlPathEncode(logFilePath);

            // replace \\ with / and append file:/// at the beginning
            logFilePathURL = "file:///" + Regex.Replace(logFilePathURL, "\\\\", "/");

            try { Process.Start(GetDefaultBrowser(), logFilePathURL); }
            catch (Exception) { }
        }

        /// <summary>
        /// Delete log file
        /// </summary>
        /// <param name="synDirPath">Path of source directory.</param>
        /// <returns>Success or fail</returns>
        public static bool ClearLog(String syncDirPath)
        {
            Boolean success = true;
            Hashtable pairs = LoadSyncDirAndLogFilePairs();

            string logFileName = "";
            if (pairs != null) logFileName = (string)pairs[syncDirPath];
            string logFilePath = String.Concat(GetAppDir(), @"\", ParentDirectoryName, @"\", logFileName);

            try
            {
                File.Delete(logFilePath);
            }
            catch (Exception e)
            {
                success = false;
                LogException(e);
            }

            return success;
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
                    // Clear exception log if its size exceeds 10MB
                    if (checkLogFileSize(logFilePath)) File.Delete(logFilePath);

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
            return System.Windows.Forms.Application.StartupPath;
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
            Hashtable pairs = null;
            Stream stream = null;

            try
            {
                string filePath = string.Concat(GetAppDir(), @"\", ParentDirectoryName, @"\", SyncDirAndLogFilePairsLocation);
                stream = File.Open(filePath, FileMode.Open);
                BinaryFormatter bin = new BinaryFormatter();
                pairs = (Hashtable)bin.Deserialize(stream);
            }
            catch (FileNotFoundException)
            {
            }
            catch (Exception e)
            {
                LogException(e);
            }
            finally
            {
                if (stream != null) stream.Close();
            }

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

        private static void SaveLogDetails(string logFilePath, bool moreThanMax, string jobName, string storageDirPath,
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
            XmlElement jobNameNode = xmlLogDoc.CreateElement("profilename");
            // profileNameNode.InnerText = RemoveTags(profileName);
            jobNameNode.InnerText = jobName;
            XmlElement dateNode = xmlLogDoc.CreateElement("syncdate");
            dateNode.InnerText = DateTime.Now.ToShortDateString();
            XmlElement storageDirPathNode = xmlLogDoc.CreateElement("storagedirectory");
            storageDirPathNode.InnerText = storageDirPath;
            XmlElement syncDirectionNode = xmlLogDoc.CreateElement("syncdirection");
            syncDirectionNode.InnerText = syncDirection;
            XmlElement numOfFilesProcessedNode = xmlLogDoc.CreateElement("numberoffilesprocessed");
            numOfFilesProcessedNode.InnerText = numOfFilesProcessed.ToString();

            // Delete the first two nodes (syncSessionNode) if the file size exceeds the threshold
            if (moreThanMax)
            {
                root.RemoveChild(root.LastChild);
                if(root.LastChild != null) root.RemoveChild(root.LastChild);
            }

            // Prepend the node; the last log will be displayed first
            root.PrependChild(syncSessionNode);
            syncSessionNode.AppendChild(jobNameNode);
            syncSessionNode.AppendChild(dateNode);
            syncSessionNode.AppendChild(storageDirPathNode);
            syncSessionNode.AppendChild(syncDirectionNode);
            syncSessionNode.AppendChild(numOfFilesProcessedNode);

            if (logActivity.Count != 0)
            {
                XmlElement startTimeNode = xmlLogDoc.CreateElement("starttime");
                startTimeNode.InnerText = startTime.ToLongTimeString();
                XmlElement endTimeNode = xmlLogDoc.CreateElement("endtime");
                endTimeNode.InnerText = endTime.ToLongTimeString();

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
            FileStream fs = null;
            StreamWriter sw = null;

            try
            {
                string s = Properties.Resources.xsl;
                fs = new FileStream(filePath, FileMode.Create);
                sw = new StreamWriter(fs);
                sw.Write(s);
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

        private static bool checkLogFileSize(string logFilePath)
        {
            try
            {
                FileInfo fi = new FileInfo(logFilePath);
                return (fi.Length > MaxLogFileSize);
            }
            catch(Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
