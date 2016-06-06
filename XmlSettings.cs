using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace System.Xml {
    /// <summary>Creates application settings for the executing module.</summary>
    public class XmlSettings {
        #region Private Fields
        private Dictionary<string, Dictionary<string, string>> settings;
        private Dictionary<string, string> targetedSection;
        #endregion

        #region Public Properties
        /// <summary>Base directory the settings are written/read.</summary>
        public string BaseDirectory { get; set; }
        
        /// <summary>Name of the folder the settings are written/read.</summary>
        public string FolderName { get; set; }

        /// <summary>Gets the name of the targeted section.</summary>
        public string TargetedSectionName { get; private set; }
        #endregion

        #region Constructor
        /// <summary>Creates a new AppSettings object.</summary>
        public XmlSettings(string baseDirectory, string folderName) {
            BaseDirectory = baseDirectory;
            FolderName = folderName;
            settings = new Dictionary<string, Dictionary<string, string>>();
        }
        #endregion

        #region Serialization Interface
        /// <summary>Writes the current settings data to the settings file, then clears it from memory.</summary>
        public void Serialize() {
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateElement("settings", ""));
            
            foreach(var section in settings) {
                XmlElement node = document.CreateElement(section.Key, document.DocumentElement.NamespaceURI);

                foreach(var pair in section.Value) {
                    XmlElement property = document.CreateElement(pair.Key, document.DocumentElement.NamespaceURI);
                    property.InnerText = pair.Value;
                    node.AppendChild(property);
                }

                document.DocumentElement.AppendChild(node);
            }

            Directory.CreateDirectory(BaseDirectory);
            document.Save(Path.Combine(BaseDirectory, FolderName + ".xml"));

            settings.Clear();
        }

        /// <summary>Clears the current settings data and loads settings from the settings file.</summary>
        public void Deserialize() {
            string filePath = Path.Combine(BaseDirectory, FolderName + ".xml");

            if(!File.Exists(filePath))
                return;

            settings.Clear();

            XmlDocument document = new XmlDocument();
            document.Load(filePath);
            XmlNode settingsNode = document.DocumentElement;

            if(settingsNode.Name != "settings")
                throw new XmlException("XML does not contain settings XML root element.");

            foreach(XmlNode section in settingsNode) {
                Dictionary<string, string> deserializedSection = new Dictionary<string, string>();
                settings.Add(section.Name, deserializedSection);

                foreach(XmlNode property in section) {
                    deserializedSection.Add(property.Name, property.InnerText);
                }
            }
        }
        #endregion

        #region Write / Read Interface
        /// <summary>Writes the property to the settings data.</summary>
        /// <typeparam name="T">Type of the property: Bool, Int32, Single, Double or String.</typeparam>
        /// <param name="property">Name of the property to write.</param>
        /// <param name="data">Value to write to the property.</param>
        /// <returns>true if the section exists, else false.</returns>
        public bool Write<T>(string property, T data) where T : struct, IComparable, IConvertible, IEquatable<T>, IComparable<T> {
            bool? key = targetedSection?.ContainsKey(property);

            if(key != false && key != null) {
                if(targetedSection.ContainsKey(property))
                    targetedSection.Remove(property);

                switch(Type.GetTypeCode(typeof(T))) {
                    case TypeCode.Boolean:
                    case TypeCode.Int32:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.String:
                        targetedSection.Add(property, data.ToString());
                        break;
                    default:
                        throw new NotImplementedException("Type constraint not implemented exception.");
                }
            }

            return (key == null) ? false : (bool)key;
        }

        /// <summary>Reads the property from the settings data.</summary>
        /// <typeparam name="T">Type of the property: Bool, Int32, Single, Double or String.</typeparam>
        /// <param name="property">Name of the property to read.</param>
        /// <param name="result">Result of the read operation.</param>
        /// <returns>true if the property was found, else false.</returns>
        public bool Read<T>(string property, out T result) where T : struct, IComparable, IConvertible, IEquatable<T>, IComparable<T> {
            result = default(T);
            string value = string.Empty;

            bool? key = targetedSection?.TryGetValue(property, out value);

            if(key != false && key != null) {
                switch(Type.GetTypeCode(typeof(T))) {
                    case TypeCode.Boolean:
                        result = (T)(object)Convert.ToBoolean(value);
                    break;
                    case TypeCode.Int32:
                        result = (T)(object)Convert.ToInt32(value);
                    break;
                    case TypeCode.Single:
                        result = (T)(object)Convert.ToSingle(value);
                    break;
                    case TypeCode.Double:
                        result = (T)(object)Convert.ToDouble(value);
                    break;
                    case TypeCode.String:
                        result = (T)(object)value;
                    break;
                    default:
                        throw new NotImplementedException("Type constraint not implemented exception.");
                }
            }

            return (key == null)? false : (bool)key;
        }
        #endregion

        #region Section Interface
        /// <summary>Creates a new section and targets it.</summary>
        /// <param name="section"></param>
        /// <returns>true if the section was created, else false if the section already exists.</returns>
        public bool CreateSection(string section, bool target = false) {
            bool sectionExists = settings.ContainsKey(section);

            if(!sectionExists) {
                settings.Add(section, new Dictionary<string, string>());
                
                if (target)
                    TargetSection(section);
            }

            return sectionExists;
        }

        /// <summary>Removes an existing section.</summary>
        /// <param name="section"></param>
        /// <returns>true if the section was removed, else false.</returns>
        public bool DestroySection(string section) {
            bool sectionExists = settings.ContainsKey(section);

            if(sectionExists)
                settings.Remove(section);

            return sectionExists;
        }

        /// <summary>Attempts to target an existing section.</summary>
        /// <param name="section">Nameo f the section to target.</param>
        /// <returns>true if the section was targeted, else false.</returns>
        public virtual bool TargetSection(string section) {
            TargetedSectionName = section;
            return settings.TryGetValue(section, out targetedSection);
        }
        #endregion
    }
}
