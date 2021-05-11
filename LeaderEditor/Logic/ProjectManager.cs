using LeaderEngine;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace LeaderEditor
{
    public struct AssetGroup
    {
        public string FileName;
        public string Name;
    }

    public static class Project
    {
        public static string Path = string.Empty;

        public static string Name = "Untitled Project";
        public static List<AssetGroup> Assets = new List<AssetGroup>();
        public static int CurrentAssetGroupIndex = 0;
    }

    public static class ProjectManager
    {
        public static void NewProject(string path)
        {
            Project.Path = path;
            Project.Name = "Untitled Project";
            Project.Assets.Clear();
            Project.CurrentAssetGroupIndex = 0;

            SaveProject();
        }

        public static void SaveProject()
        {
            string cfgPath = Path.Combine(Project.Path, "project-config.xml");

            using (var writer = XmlWriter.Create(cfgPath, new XmlWriterSettings() { Indent = true }))
            {
                writer.WriteStartDocument();

                //project start
                writer.WriteStartElement("Project");
                writer.WriteAttributeString("Name", Project.Name);

                writer.WriteStartElement("Assets");
                writer.WriteAttributeString("CurrentIndex", Project.CurrentAssetGroupIndex.ToString());

                foreach (var asset in Project.Assets)
                {
                    writer.WriteStartElement("Asset");
                    writer.WriteAttributeString("FileName", asset.FileName);
                    writer.WriteString(asset.Name);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();

                //project end
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        public static bool OpenProject(string path)
        {
            string cfgPath = Path.Combine(path, "project-config.xml");

            if (!File.Exists(cfgPath))
                return false;

            Project.Path = path;

            var doc = XDocument.Load(cfgPath);

            Project.Name = doc
                .Element("Project")
                .Attribute("Name")
                .Value;

            Project.CurrentAssetGroupIndex = int.Parse(doc
                .Element("Project")
                    .Element("Assets")
                    .Attribute("CurrentIndex")
                    .Value);

            Project.Assets = doc
                .Element("Project")
                    .Element("Assets")
                        .Elements("Asset")
                        .Select(atr => new AssetGroup { FileName = atr.Attribute("FileName").Value, Name = atr.Value })
                        .ToList();

            LoadAssetGroup();

            return true;
        }

        public static void LoadAssetGroup()
        {
            if (Project.CurrentAssetGroupIndex >= Project.Assets.Count)
                return;

            var ag = Project.Assets[Project.CurrentAssetGroupIndex];
            DataManager.LoadAssets(Path.Combine(Project.Path, "Assets/", ag.FileName));
        }

        public static void SaveAssetGroup(int index = -1)
        {
            if (index < 0)
                index = Project.CurrentAssetGroupIndex;

            string assetsPath = Path.Combine(Project.Path, "Assets/");

            Directory.CreateDirectory(assetsPath);

            var ag = Project.Assets[index];
            DataManager.SaveAssets(Path.Combine(assetsPath, ag.FileName));
        }
    }
}
