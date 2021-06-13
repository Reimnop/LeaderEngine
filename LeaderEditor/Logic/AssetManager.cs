using ImGuiNET;
using LeaderEngine;
using System.Numerics;
using System.Windows.Forms;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace LeaderEditor
{
    public class AssetManager : Component
    {
        public static Prefab SelectedPrefab;
        public static Mesh SelectedMesh;
        public static Texture SelectedTexture;
        public static Material SelectedMaterial;
        public static AudioClip SelectedClip;
        public static Cubemap SelectedCubemap;

        private void Start()
        {
            //register ImGui
            ImGuiController.RegisterImGui(ImGuiRenderer);
        }

        private void ImGuiRenderer()
        {
            if (ImGui.Begin("Asset Manager"))
            {
                if (Input.GetKeyDown(Keys.I) && ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows))
                    SelectedPrefab?.Instantiate();
            }
        }
    }
}
