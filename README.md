# XmlSettings
Simple Xml based File Settings reader/writer. You can read and write properties into a section in the XmlSettings. Create a section or two or whatever, target the section for reading/writing, then write the properties you want to save into the sections you want them in. Then serialize to save or deserialize to load.

```C#
// XmlSettings is embedded in the System.Xml namespace.
using System.Xml;
```
```C#
// Create a new XmlSettings instance.
// directory = file path  to store the file (absolute or relative).
// folderName = name of the subflder to store the file in.
// rootHeaderName = root XML header element name.
// isAppDirectoryRelative = whether the directory is relative to the application directory.
XmlSettings xset = new XmlSettings(directory, folderName, rootHeaderName, isAppDirectoryRelative);

/*
  Supports Read/Write T for:
    Bool, Int32, Single, Double, String.
*/
// Write to the section.
xset.Write<T>(sectionName, propertyName, T property);
// Read from the section.
xset.Read<T>(sectionName, propertName, out property);

// Serialize--save--the XmlSettings to an XML file.
xset.Serialize();
// Deserialize--load--the XmlSettings from an XML file.
xset.Deserialize();
```
