using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ImGuiNET;

namespace LeaderEditor
{
    public class AssetImporterWizard : IDisposable
    {
        private static Dictionary<string, AssetImporterWizard> instances = new Dictionary<string, AssetImporterWizard>();

        public static AssetImporterWizard GetAssetImporter(string name)
        {
            if (instances.TryGetValue(name, out var value))
                return value;

            return null;
        }

        public readonly string Name;
        public string Title = "Import Asset";

        private Dictionary<string, object> values = new Dictionary<string, object>();
        private bool finished = false;

        public AssetImporterWizard(string name)
        {
            Name = name;

            instances.Add(name, this);
        }

        //returns false when finished
        public bool Begin()
        {
            if (finished)
                return false;

            return ImGui.Begin(Title, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse);
        }

        public void End()
        {
            finished = ImGui.Button("Import") || finished;

            ImGui.End();
        }

        public bool Finished()
        {
            return finished;
        }

        public string OpenFileDialog(string name, string filter)
        {
            if (!values.ContainsKey(name))
                values.Add(name, string.Empty);

            string value = (string)values[name];

            ImGui.PushID(name + "-btn");

            if (ImGui.Button("Select File"))
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = filter;

                    ofd.ShowDialog();

                    if (!string.IsNullOrEmpty(ofd.FileName))
                    {
                        value = ofd.FileName;
                    }
                }
            }

            ImGui.PopID();

            ImGui.SameLine();
            ImGui.InputText(name, ref value, 32767);

            values[name] = value;

            return value;
        }

        public void Dispose()
        {
            instances.Remove(Name);
        }
    }
}
