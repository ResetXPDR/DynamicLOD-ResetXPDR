﻿using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace DynamicLOD_ResetEdition
{
    public class ConfigurationFile
    {
        private Dictionary<string, string> appSettings = new();
        private XmlDocument xmlDoc = new();

        public string this[string key]
        {
            get => GetSetting(key);
            set => SetSetting(key, value);
        }

        public void LoadConfiguration()
        {
            xmlDoc = new();
            xmlDoc.LoadXml(File.ReadAllText(App.ConfigFile));

            XmlNode xmlSettings = xmlDoc.ChildNodes[1];
            appSettings.Clear();
            foreach(XmlNode child in xmlSettings.ChildNodes)
                appSettings.Add(child.Attributes["key"].Value, child.Attributes["value"].Value);
        }

        public void SaveConfiguration()
        {
            foreach (XmlNode child in xmlDoc.ChildNodes[1])
                child.Attributes["value"].Value = appSettings[child.Attributes["key"].Value];

            xmlDoc.Save(App.ConfigFile);
        }
        public bool SettingExists(string key)
        {
            if (appSettings.ContainsKey(key)) return true;
            else return false;
        }

        public string GetSetting(string key, string defaultValue = "")
        {
            if (appSettings.ContainsKey(key))
                return appSettings[key];
            else
            {
                XmlNode newNode = xmlDoc.CreateElement("add");

                XmlAttribute attribute = xmlDoc.CreateAttribute("key");
                attribute.Value = key;
                newNode.Attributes.Append(attribute);

                attribute = xmlDoc.CreateAttribute("value");
                attribute.Value = defaultValue;
                newNode.Attributes.Append(attribute);

                xmlDoc.ChildNodes[1].AppendChild(newNode);
                appSettings.Add(key, defaultValue);
                SaveConfiguration();

                return defaultValue;
            }
        }

        public void RemoveSetting(string key)
        {
            if (appSettings.ContainsKey(key))
            {
                XmlNode nodeToRemove = xmlDoc.SelectSingleNode($"//add[@key='{key}']");
                if (nodeToRemove != null)
                {
                    nodeToRemove.ParentNode.RemoveChild(nodeToRemove);
                    appSettings.Remove(key);
                    SaveConfiguration();
                }
            }
        }

        public void SetSetting(string key, string value)
        {
            if (appSettings.ContainsKey(key))
            {
                appSettings[key] = value;
                SaveConfiguration();
            }
        }
    }
}
