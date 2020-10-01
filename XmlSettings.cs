using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml;

namespace MPT_Modules.Utilities {
    public class XmlManager {
        #region Private Fields
        /// <summary>XML formatted Dictionary.</summary>
        private readonly Dictionary<string, Dictionary<string, string>> settings;
        #endregion
        #region Public Properties
        /// <summary>Base directory the settings are written/read.</summary>
        public string FileDirectory { get; set; }
        /// <summary>Name the file will be given when written/read.</summary>
        public string FileName { get; set; }

        /// <summary>Name of the root XML header.</summary>
        public string RootHeaderName { get; set; }
        #endregion

        /// <summary>Create a new XmlManager to serialize an XML settings file.</summary>
        /// <param name="directory">Path to save the XML file to.</param>
        /// <param name="fileName">Name of the XML file.</param>
        /// <param name="isAppRelative">Whether the directory is relative to the application directory.</param>
        public XmlManager(string directory, string fileName, string rootHeaderName, bool isAppRelative = false) {
            FileDirectory = (isAppRelative) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, new DirectoryInfo(directory).Name) : directory;
            FileName = new DirectoryInfo(fileName).Name;
            RootHeaderName = rootHeaderName;
            settings = new Dictionary<string, Dictionary<string, string>>();
        }

        /// <summary>Checks if a particular property in a section exists.</summary>
        /// <param name="section">Name of the section to check for.</param>
        /// <param name="propertyName">Name of the proeprty to check for.</param>
        /// <returns>True if the property in section exists, else false.</returns>
        public bool Exists(string section, string propertyName) {
            Dictionary<string, string> outSection = null;
            settings.TryGetValue(section, out outSection);
            return outSection?.ContainsKey(propertyName) == true;
        }

        /// <summary>Write a value to a property in a section.</summary>
        /// <typeparam name="T">Valid types: Bool, Int32, Single, Double or String.</typeparam>
        /// <param name="section">Section name to write the new property to.</param>
        /// <param name="propertyName">Name of the property to create.</param>
        /// <param name="value">Value to give the property.</param>
        /// <exception cref="NotImplementedException">Throw on invalid type.</exception>
        public void Write<T>(string section, string propertyName, T value) where T : IComparable, IConvertible, IEquatable<T>, IComparable<T> {
            if (!settings.ContainsKey(section))
                settings.Add(section, new Dictionary<string, string>());

            Dictionary<string, string> outSection;
            settings.TryGetValue(section, out outSection);

            if (Exists(section, propertyName))
                outSection.Remove(propertyName);

            switch (Type.GetTypeCode(typeof(T))) {
                case TypeCode.Boolean:
                case TypeCode.Int32:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.String:
                    outSection.Add(propertyName, value.ToString());
                    break;
                default:
                    throw new NotImplementedException("Type constraint not implemented exception.");
            }
        }

        /// <summary></summary>
        /// <typeparam name="T">Valid types: Bool, Int32, Single, Double or String.</typeparam>
        /// <param name="section">Section name to read the property from.</param>
        /// <param name="propertyName">Name of the property to read.</param>
        /// <param name="value">Out reference to write the value to.</param>
        /// <exception cref="NotImplementedException">Throw on invalid type.</exception>
        /// <returns>True if property can be read, else false.</returns>
        public bool Read<T>(string section, string propertyName, out T value) where T : IComparable, IConvertible, IEquatable<T>, IComparable<T> {
            bool hasProperty = false;
            value = default(T);

            if (Exists(section, propertyName)) {
                Dictionary<string, string> outSection;
                settings.TryGetValue(section, out outSection);
                string property = string.Empty;
                outSection?.TryGetValue(propertyName, out property);

                switch (Type.GetTypeCode(typeof(T))) {
                    case TypeCode.Boolean:
                        value = (T)(object)Convert.ToBoolean(property);
                        break;
                    case TypeCode.Int32:
                        value = (T)(object)Convert.ToInt32(property);
                        break;
                    case TypeCode.Single:
                        value = (T)(object)Convert.ToSingle(property);
                        break;
                    case TypeCode.Double:
                        value = (T)(object)Convert.ToDouble(property);
                        break;
                    case TypeCode.String:
                        value = (T)(object)property;
                        break;
                    default:
                        throw new NotImplementedException("Type constraint not implemented exception.");
                }

                hasProperty = true;
            }

            return hasProperty;
        }

        /// <summary>Writes the XML data to the file at the provided directory.</summary>
        public void Serialize() {
            XmlDocument document = new XmlDocument();
            XmlNode sectionNode = document.CreateElement(RootHeaderName, "");
            document.AppendChild(sectionNode);

            foreach (var section in settings) {
                XmlNode node = document.CreateElement(section.Key, document.DocumentElement.NamespaceURI);

                foreach(var pair in section.Value) {
                    XmlElement property = document.CreateElement(pair.Key, document.DocumentElement.NamespaceURI);
                    property.InnerText = pair.Value;
                    node.AppendChild(property);
                }

                sectionNode.AppendChild(node);
            }

            Directory.CreateDirectory(FileDirectory);
            document.Save(Path.Combine(FileDirectory, FileName + ".xml"));
        }

        /// <summary>Reads the XML data from the file at the provided directory.</summary>
        public void Deserialize() {
            string filePath = Path.Combine(FileDirectory, FileName + ".xml");

            if (File.Exists(filePath)) {
                settings.Clear();
            } else return;

            XmlDocument document = new XmlDocument();
            document.Load(filePath);
            XmlNode sectionNode = document.DocumentElement;

            if (sectionNode.Name != RootHeaderName)
                throw new XmlException("XML does not contain '" + RootHeaderName + "' XML root element.");

            foreach (XmlNode section in sectionNode) {
                Dictionary<string, string> deserializedSection = new Dictionary<string, string>();
                settings.Add(section.Name, deserializedSection);

                foreach (XmlNode property in section)
                    deserializedSection.Add(property.Name, property.InnerText);
            }
        }
    }
}
