using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;

namespace OneSync
{
    class UIProfileStorageXML
    {
        //The path to the .xml file. It is stored in the installation path.
        string fileName = Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.LastIndexOf('\\'))
            + "\\SyncJobProfile.xml";


        public UIProfileStorageXML()
        {

        }

        public void StoreProfile(string profileName, string sourceDir, string storageDir) 
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();

                try
                {
                    xmlDoc.Load(fileName);
                }
                catch(System.IO.FileNotFoundException)
                {
                    //If file is not found, create a new xml file
                    XmlTextWriter xmlWriter = new XmlTextWriter(fileName, System.Text.Encoding.UTF8);
                    xmlWriter.Formatting = Formatting.Indented;
                    xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
                    xmlWriter.WriteStartElement("Root");
                    xmlWriter.Close();
                    xmlDoc.Load(fileName);
                }
                XmlNode root = xmlDoc.DocumentElement;
                XmlElement childNode = xmlDoc.CreateElement("profile");
                XmlElement childNodeOfChildNode = xmlDoc.CreateElement("sourceDir");
                XmlElement childNode2OfChildNode = xmlDoc.CreateElement("storageDir");
                
                root.AppendChild(childNode);
                childNode.SetAttribute("ProfileName", profileName);
                childNode.AppendChild(childNodeOfChildNode);
                childNodeOfChildNode.SetAttribute("SourceDir", sourceDir);
                childNode.AppendChild(childNode2OfChildNode);
                childNode2OfChildNode.SetAttribute("StorageDir", storageDir);

                xmlDoc.Save(fileName);
            }
            catch(Exception)
            {
                throw;
            }
        }

        public List<ProfileItem> RetrieveAllProfiles() 
        {
            List<ProfileItem> profileItemsCollection = new List<ProfileItem>();
            XmlTextReader reader = new XmlTextReader(fileName);
            int check = 0;
            string profileName = "";
            string sourceDir = "";
            string storageDir = "";
            while (reader.Read())
            {
                if(reader.NodeType == XmlNodeType.Element)
                {
                    if(reader.Name.Equals("profile"))
                    {
                        profileName = reader.GetAttribute("ProfileName");
                        check++;
                    }
                    else if (reader.Name.Equals("sourceDir"))
                    {
                        sourceDir = reader.GetAttribute("SourceDir");
                        check++;
                    }
                    else if (reader.Name.Equals("storageDir"))
                    {
                        storageDir = reader.GetAttribute("StorageDir");
                        check++;
                    }
                }

                if(check == 3)
                {
                    profileItemsCollection.Add(new ProfileItem(profileName, sourceDir, storageDir));
                    check = 0;
                }
            }

            return profileItemsCollection;
        }
    }

    class ProfileItem
    {
        public string ProfileName;
        public string SourceDir;
        public string StorageDir;
        public ProfileItem(string profileName, string sourceDir, string storageDir)
        {
            ProfileName = profileName;
            SourceDir = sourceDir;
            StorageDir = storageDir;
        }
        public override string ToString()
        {
            return ProfileName;
        }
    }
}
