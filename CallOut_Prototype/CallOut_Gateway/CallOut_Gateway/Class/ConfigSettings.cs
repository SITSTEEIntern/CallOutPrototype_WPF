using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

namespace CallOut_Gateway.Class
{
    public class ConfigSettings
    {

        private static string NodePath = "//system.serviceModel//client//endpoint";

        public ConfigSettings() { }

        public static string GetEndpointAddress()
        {
            return ConfigSettings.loadConfigDocument().SelectSingleNode(NodePath).Attributes["address"].Value;
        }

        public void SaveEndpointAddress(string endpointAddress, string endpointName)
        {
            // load config document for current assembly
            XmlDocument doc = loadConfigDocument();
            
            // retrieve appSettings node
            XmlNodeList nodes = doc.SelectNodes(NodePath);

            foreach (XmlNode node in nodes)
            {
                if (node == null)
                    throw new InvalidOperationException("Error. Could not find endpoint node in config file.");

                //If this isnt the node I want to change, look at the next one
                //Change this string to the name attribute of the node you want to change
                if (node.Attributes["name"].Value != endpointName)
                {
                    continue;
                }

                try
                {
                    // select the 'add' element that contains the key
                    //XmlElement elem = (XmlElement)node.SelectSingleNode(string.Format("//add[@key='{0}']", key));
                    node.Attributes["address"].Value = endpointAddress;

                    doc.Save(getConfigFilePath());

                    break;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public static void SaveEndpointAddress(string endpointAddress, string ConfigPath, string endpointName)
        {
            // load config document for current assembly
            XmlDocument doc = loadConfigDocument(ConfigPath);

            // retrieve appSettings node
            XmlNodeList nodes = doc.SelectNodes(NodePath);

            foreach (XmlNode node in nodes)
            {
                if (node == null)
                    throw new InvalidOperationException("Error. Could not find endpoint node in config file.");

                //If this isnt the node I want to change, look at the next one
                if (node.Attributes["name"].Value != endpointName)
                {
                    continue;
                }

                try
                {
                    // select the 'add' element that contains the key
                    //XmlElement elem = (XmlElement)node.SelectSingleNode(string.Format("//add[@key='{0}']", key));
                    node.Attributes["address"].Value = endpointAddress;

                    doc.Save(ConfigPath);

                    break;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public static XmlDocument loadConfigDocument()
        {
            XmlDocument doc = null;
            try
            {
                doc = new XmlDocument();
                doc.Load(getConfigFilePath());
                return doc;
            }
            catch (System.IO.FileNotFoundException e)
            {
                throw new Exception("No configuration file found.", e);
            }
        }

        public static XmlDocument loadConfigDocument(string Path)
        {
            XmlDocument doc = null;
            try
            {
                doc = new XmlDocument();
                doc.Load(Path);
                return doc;
            }
            catch (System.IO.FileNotFoundException e)
            {
                throw new Exception("No configuration file found.", e);
            }
        }

        private static string getConfigFilePath()
        {
            //Need to check is to edit to which config file.
            //Debug.WriteLine(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile.ToString()); //give back vshost
            return AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
        }
    }
}