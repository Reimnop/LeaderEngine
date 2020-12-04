using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Emit;
using System.Windows.Forms;
using ImGuiNET;
using LeaderEditor.Gui;
using LeaderEditor.Logic.LeaderCompiler;
using LeaderEngine;

using Application = LeaderEngine.Application;

namespace LeaderEditor.Logic
{
    public class MainMenuBar : Component
    {
        public override void Start()
        {
            ImGuiController.main.OnImGui += OnImGui;
        }

        private void OnImGui()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Compile", "F9"))
                    {
                        OpenFileDialog ofd = new OpenFileDialog();
                        ofd.Filter = "*.cs|*.cs";
                        ofd.Multiselect = true;

                        ofd.ShowDialog();

                        if (ofd.FileNames != null)
                        {
                            List<string> sources = new List<string>();

                            foreach (var fileName in ofd.FileNames)
                            {
                                sources.Add(File.ReadAllText(fileName));
                            }

                            EmitResult result;

                            Compiler compiler = new Compiler();
                            Type[] types = compiler.Compile(sources.ToArray(), out result);

                            if (result.Success)
                            {
                                foreach (var t in types)
                                {
                                    if (t.IsSubclassOf(typeof(Component)))
                                        Inspector.SerializeableComponents.Add(t, null);
                                }
                            }
                            else
                            {
                                foreach (var diag in result.Diagnostics)
                                    DebugConsole.main.WriteLine(diag.ToString());
                            }
                        }
                    }

                    if (ImGui.MenuItem("Exit", "Alt+F4"))
                    {
                        Application.main.Close();
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Windows"))
                {
                    ImGui.MenuItem("Console", null, ref DebugConsole.main.isOpen);
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }
        }
    }
}
