using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;
using LeaderEngine;

namespace LeaderEditor
{
    public class EditorRenderer : ForwardRenderer
    {
        public static ImGuiController ImGuiController { get; } = new ImGuiController();

        public override void Init()
        {
            ImGuiController.Init();

            Logger.Log("Renderer initialized.");
        }

        public override void Update()
        {
            base.Update();

            ImGuiController.Update(Time.DeltaTime);
        }

        public override void Render()
        {
            base.Render();

            ImGuiController.RenderImGui();
        }
    }
}
