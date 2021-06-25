using ImGuiNET;
using LeaderEngine;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace LeaderEditor
{
    public class ProjectManager : Component
    {
        private bool isWindowOpen = false;

        private void Start()
        {
            ImGuiController.OnImGui += OnImGui;
        }

        private void Update()
        {
            if (Input.GetKeyCombo(Keys.LeftControl, Keys.O))
            {
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    fbd.ShowDialog();

                    if (!string.IsNullOrEmpty(fbd.SelectedPath))
                    {
                        OpenProject(fbd.SelectedPath);
                    }
                }
            }

            if (Input.GetKeyCombo(Keys.LeftControl, Keys.S))
            {
                if (!string.IsNullOrEmpty(Project.Path))
                {
                    SaveProject();
                }
                else
                {
                    SaveProjectAs();
                }
            }

            if (Input.GetKeyCombo(Keys.LeftControl, Keys.LeftShift, Keys.S))
            {
                SaveProjectAs();
            }
        }

        private void OnImGui()
        {
            if (isWindowOpen)
            {
                if (ImGui.Begin("Project Manager", ref isWindowOpen))
                {
                    ImGui.End();
                }
            }
        }

        public void DrawFileMenu()
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Open", "Ctrl+O"))
                {
                    using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                    {
                        fbd.ShowDialog();

                        if (!string.IsNullOrEmpty(fbd.SelectedPath))
                        {
                            OpenProject(fbd.SelectedPath);
                        }
                    }
                }

                if (ImGui.MenuItem("Save", "Ctrl+S"))
                {
                    if (!string.IsNullOrEmpty(Project.Path))
                    {
                        SaveProject();
                    }
                    else
                    {
                        SaveProjectAs();
                    }
                }

                if (ImGui.MenuItem("Save As", "Ctrl+Shift+S"))
                {
                    SaveProjectAs();
                }

                if (ImGui.MenuItem("Open Project Manager"))
                {
                    isWindowOpen = true;
                }

                ImGui.EndMenu();
            }
        }

        private void OpenProject(string selectedPath)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(selectedPath, "project-config.xml"));

            Project.Path = selectedPath;

            XmlElement projectRoot = (XmlElement)xmlDocument.GetElementsByTagName("Project")[0];
            Project.Name = projectRoot.GetAttribute("Name");

            XmlElement assets = (XmlElement)projectRoot.GetElementsByTagName("Assets")[0];
            Project.AssetsPackage = assets.GetAttribute("Path");

            XmlElement scenes = (XmlElement)projectRoot.GetElementsByTagName("Scenes")[0];
            Project.CurrentSceneIndex = int.Parse(scenes.GetAttribute("CurrentIndex"));

            XmlNodeList scenesList = scenes.GetElementsByTagName("scene");
            foreach (XmlElement sceneElement in scenesList)
            {
                Project.SceneFiles.Add(sceneElement.GetAttribute("Path"));
            }

            //load assets
            LeaderEngine.AssetManager.LoadAssetsFromFile(Path.Combine(Project.Path, Project.AssetsPackage));
            string scenesDir = Path.Combine(Project.Path, "scenes");
            DataManager.LoadSceneFromFile(Path.Combine(scenesDir, Project.SceneFiles[Project.CurrentSceneIndex]));
        }

        private void SaveProjectAs()
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.ShowDialog();

                if (!string.IsNullOrEmpty(fbd.SelectedPath))
                {
                    Project.Path = fbd.SelectedPath;
                    SaveProject();
                }
            }
        }

        private void SaveProject()
        {
            //save assets
            LeaderEngine.AssetManager.SaveAssetsToFile(Path.Combine(Project.Path, Project.AssetsPackage));
            string scenesDir = Path.Combine(Project.Path, "scenes");
            Directory.CreateDirectory(scenesDir);
            DataManager.SaveSceneToFile(Path.Combine(scenesDir, Project.SceneFiles[Project.CurrentSceneIndex]));

            //save xml
            XmlDocument xmlDocument = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDocument.AppendChild(xmlDeclaration);

            //project root and name
            XmlElement projectRoot = xmlDocument.CreateElement("Project");
            projectRoot.SetAttribute("Name", Project.Name);
            xmlDocument.AppendChild(projectRoot);

            //assets package path
            XmlElement assetPackage = xmlDocument.CreateElement("Assets");
            assetPackage.SetAttribute("Path", Project.AssetsPackage);
            projectRoot.AppendChild(assetPackage);

            //scenes
            XmlElement scenes = xmlDocument.CreateElement("Scenes");
            scenes.SetAttribute("CurrentIndex", Project.CurrentSceneIndex.ToString());

            foreach (string scenePath in Project.SceneFiles)
            {
                XmlElement scene = xmlDocument.CreateElement("Scene");
                scene.SetAttribute("Path", scenePath);
                scenes.AppendChild(scene);
            }

            projectRoot.AppendChild(scenes);

            xmlDocument.Save(Path.Combine(Project.Path, "project-config.xml"));
        }
    }
}
