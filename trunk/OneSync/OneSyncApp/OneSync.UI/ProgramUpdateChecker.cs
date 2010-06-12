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
    }
}
