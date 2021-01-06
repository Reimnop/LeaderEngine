using ImGuiNET;
using LeaderEditor.Gui;
using LeaderEngine;

namespace LeaderEditor
{
    public class EditorController : EditorComponent
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

        public override void EditorStart()
        {
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

            gameObject.Tag = "Editor";

            //camera
            editorCamera = new GameObject("EditorCamera");
            editorCamera.AddComponent<EditorCamera>();
            editorCamera.transform.LocalPosition = new OpenTK.Mathematics.Vector3(0.0f, 1.0f, 2.0f);

            editorCamera.Tag = "Editor";

            //add gui controller
            ImGuiController = new GameObject("Controller");
            ImGuiController.AddComponent<ImGuiController>();

            ImGuiController.Tag = "Editor";
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

            EditorCamera.Main.Enabled = true;
            Application.Main.EditorMode = true;
        }

        private static void SetupPlayMode()
        {
            DebugConsole.Log("Entering Play Mode");

            EditorCamera.Main.Enabled = false;
            Application.Main.EditorMode = false;
        }
    }
}
