# XmlSettings
Simple Xml based File Settings reader/writer. You can read and write properties into a section in the XmlSettings. Create a section or two or whatever, target the section for reading/writing, then write the properties you want to save into the sections you want them in. Then serialize to save or deserialize to load.

```C#
// XmlSettings is embedded in the System.Xml namespace.
using System.Xml;
```
```C#
// Create a new XmlSettings instance.
XmlSettings xset = new XmlSettings(appDirectory, folderName);

// Create a section and target the seciton for reading/writing.
xset.CreateSection("Application");
xset.TargetSection("Application");

// Destroy a section you don't need.
xset.DestroySection("Trash Section");

/*
  Supports Read/Write for:
    Bool, Int32, Single, Double, String.
*/
// Write to the section.
xset.Write<string>("FontSize", FontSizeProperty);
// Read from the section.
xset.Read("FontSize", out FontSizeProperty);

// Serialize--save--the XmlSettings to an XML file.
xset.Serialize();
// Deserialize--load--the XmlSettings from an XML file.
xset.Deserialize();
```
