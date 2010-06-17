using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;

namespace OneSync.UI
{
    class ProgramUpdateChecker
    {
        public readonly string XmlUrl;
        public readonly string NodeRootElement;
        public const string NODE_LATEST_VERSION = "version";
        public const string NODE_URL = "url";

        private string _updateUrl;

        public ProgramUpdateChecker()
        {
            XmlUrl = "http://onesync.googlecode.com/svn/trunk/OneSync/OneSyncApp/OneSync.UI/Resource/OneSyncLatestVersion.xml";
            NodeRootElement = "onesync";
        }

        /// <summary>
        /// Retrieves and compares the version stored on the server against the current running version.
        /// </summary>
        /// <returns>Returns the download path of the new version if new update is found; empty string if no newer version is found or if there is an error.</returns>
        public string GetNewVersion()
        {
            WebRequest request = WebRequest.Create(XmlUrl);
            request.Timeout = 3000;
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();

            Version newVersion = null;
            XmlDocument xDoc = new XmlDocument();

            xDoc.Load(new XmlTextReader(XmlUrl));
            newVersion = new Version(xDoc.GetElementsByTagName("version")[0].InnerText);
            _updateUrl = xDoc.GetElementsByTagName("url")[0].InnerText;

            if (newVersion == null || string.IsNullOrEmpty(_updateUrl))
                return "";

            Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (curVersion.CompareTo(newVersion) < 0)
                return _updateUrl; // Newer version found
            else
                return ""; // Unable to find any updates
        }

        /// <summary>
        /// Retrieves the required version of .NET Framework to make sure OneSync can run successfully.
        /// </summary>
        /// <returns>Returns an array containing the name of the .NET Framework and its download link.</returns>
        public string[] GetRequiredDotNetInfo()
        {
            WebRequest request = WebRequest.Create(XmlUrl);
            request.Timeout = 3000;
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();

            String[] requiredDotNetInfo = new string[2];
            XmlDocument xDoc = new XmlDocument();

            xDoc.Load(new XmlTextReader(XmlUrl));
            requiredDotNetInfo[0] = xDoc.GetElementsByTagName("suitable_dot_net_framework")[0].InnerText;
            requiredDotNetInfo[1] = xDoc.GetElementsByTagName("url_dot_net_framework")[0].InnerText;

            if (string.IsNullOrEmpty(requiredDotNetInfo[0]) || string.IsNullOrEmpty(requiredDotNetInfo[1]))
                return null;

            return requiredDotNetInfo;
        }
    }
}
