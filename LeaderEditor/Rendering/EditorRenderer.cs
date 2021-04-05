using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;
using LeaderEngine;

namespace LeaderEditor
{
    public class EditorRenderer : GLRenderer
    {
        private Dictionary<DrawType, List<GLDrawData>> drawLists = new Dictionary<DrawType, List<GLDrawData>>()
        {
            { DrawType.Opaque, new List<GLDrawData>() },
            { DrawType.Transparent, new List<GLDrawData>() },
            { DrawType.GUI, new List<GLDrawData>() }
        };

        public static ImGuiController ImGuiController { get; } = new ImGuiController();

        public override void Init()
        {
            ImGuiController.Init();
            ImGuiController.RegisterImGui(imgui);

            Logger.Log("Renderer initialized.");
        }

        private void imgui()
        {
            ImGuiNET.ImGui.ShowDemoWindow();
        }

        public override void PushDrawData(DrawType drawType, GLDrawData drawData)
        {
            drawLists[drawType].Add(drawData);
        }

        public override void Update()
        {
            ImGuiController.Update(Time.DeltaTime);
        }

        public override void Render()
        {
            if (Camera.Main == null)
                return;

            //set matrices
            DataManager.CurrentScene.SceneEntities.ForEach(en => en.Transform.CalculateModelMatrixRecursively());

            Camera.Main.CalculateViewProjection(out Matrix4 view, out Matrix4 projection);

            WorldView = view;
            WorldProjection = projection;

            //call all render funcs
            DataManager.CurrentScene.SceneEntities.ForEach(en => en.Render());

            //render opaque
            GL.Enable(EnableCap.DepthTest);

            var opDrawList = drawLists[DrawType.Opaque];

            opDrawList.ForEach(drawData =>
            {
                Mesh mesh = drawData.Mesh;
                Material mat = drawData.Material;

                if (mesh == null || mat == null)
                    return;

                mesh.Use();
                mat.Use();

                GL.DrawElements(PrimitiveType.Triangles, mesh.IndicesCount, DrawElementsType.UnsignedInt, 0);
            });

            ClearDrawList();
            
            ImGuiController.RenderImGui();
        }

        private void ClearDrawList()
        {
            foreach (var kvp in drawLists)
                kvp.Value.Clear();
        }
    }
}
