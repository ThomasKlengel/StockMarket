using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace StockMarket
{
    static class XmlHelper
    {
        private const string DEFAULTPATH = "Config.xml";

        /// <summary>
        /// searches an element with the specified name, starts looking in parent, looks also in childelements 
        /// </summary>
        /// <param name="parent">the item where the search is initiated</param>
        /// <param name="ElementName">the name of the element (e.g.: MessageGroup)</param>
        /// <returns>null if there was no element found</returns>
        public static XElement GetElement(this XElement parent, string ElementName)
        {
            // check if the parent is the element to look for
            if (parent.Name == ElementName)
            {
                return parent;
            }
            // if not, check all its child elements
            if (parent.HasElements)
            {
                foreach (XElement child in parent.Elements())
                {
                    XElement element = GetElement(child, ElementName);
                    if (element != null)
                    {
                        return element;
                    }
                }
            }

            return null;
        }

        public static string GetElementValueInRoot(this XDocument doc, string elementName)
        {
            foreach (var element in doc.Root.Elements())
            {
                if (element.Name == elementName)
                    return element.Value;
            }

            return string.Empty;
        }

        /// <summary>
        /// Reads a configuration from a XML file
        /// </summary>
        /// <param name="path">The path to the XML file (Default = "Config.xml")</param>
        /// <returns></returns>
        public static List<Share> ReadConfig(string path = DEFAULTPATH)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Share>));
                TextReader reader = new StreamReader(path);

                var listeshare = serializer.Deserialize(reader);
                reader.Close();

                return listeshare as List<Share>;
            }
            catch (Exception ex)
            {
                return new List<Share>();
            }
        }

        /// <summary>
        /// Saves the application configuration to a XML file
        /// </summary>
        /// <param name="shares">The shares to save to the XML file</param>
        /// <param name="path">The path to the XML file (Default = "Config.xml")</param>
        public static void SaveConfig(List<Share> shares, string path = DEFAULTPATH)
        {
            XmlSerializer serializer =
            new XmlSerializer(typeof(List<Share>));
            TextWriter writer = new StreamWriter(path);

            serializer.Serialize(writer, shares);
            writer.Close();
        }
    }
}
