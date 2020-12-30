using ImGuiNET;
using LeaderEditor.Gui;
using LeaderEngine;

namespace LeaderEditor
{
    public class EditorController : Component
    {
        public enum EditorMode
        {
            Editor,
            Play
        }

        private static EditorMode _mode;
        public static EditorMode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
                UpdateMode();
            }
        }

        private GameObject ImGuiController;
        private GameObject editorCamera;

        public override void Start()
        {
            //add gui controller
            ImGuiController = new GameObject("Controller");
            ImGuiController.AddComponent<ImGuiController>().OnImGui += OnImGui;

            //camera
            editorCamera = new GameObject("EditorCamera");
            editorCamera.AddComponent<EditorCamera>();
            editorCamera.transform.Position = new OpenTK.Mathematics.Vector3(0.0f, 1.0f, 2.0f);

            //add all components
            gameObject.AddComponents(
                new Component[] {
                    new MainMenuBar(),
                    new Viewport(),
                    new SceneHierachy(),
                    new Inspector(),
                    new DebugConsole(),
                    new DebugWindow()
                });
        }

        private void OnImGui()
        {
            //the dockspace
            ImGui.DockSpaceOverViewport();
        }

        private static void UpdateMode()
        {
            switch (_mode)
            {
                case EditorMode.Editor:
                    SetupEditorMode();
                    break;
                case EditorMode.Play:
                    SetupPlayMode();
                    break;
            }
        }

        private static void SetupEditorMode()
        {
            DebugConsole.Log("Entering Editor Mode");

            EditorCamera.main.Enabled = true;
            if (Camera.main != null)
                Camera.main.Enabled = false;
            RenderingGlobals.RenderingEnabled = true;
            Application.main.EditorMode = true;
        }

        private static void SetupPlayMode()
        {
            DebugConsole.Log("Entering Play Mode");

            EditorCamera.main.Enabled = false;
            if (Camera.main != null)
                Camera.main.Enabled = true;
            else RenderingGlobals.RenderingEnabled = false;
            Application.main.EditorMode = false;
        }
    }
}
