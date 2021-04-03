using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace LeaderEngine
{
    public class DefaultRenderer : GLRenderer
    {
        private Dictionary<DrawType, List<GLDrawData>> drawList = new Dictionary<DrawType, List<GLDrawData>>()
        {
            { DrawType.Opaque, new List<GLDrawData>() },
            { DrawType.Transparent, new List<GLDrawData>() }
        };

        public override void Init()
        {
            Logger.Log("Renderer initialized.");
        }

        public override void PushDrawData(DrawType drawType, GLDrawData drawData)
        {
            drawList[drawType].Add(drawData);
        }

        public override void Render()
        {
            //call all render funcs
            DataManager.CurrentScene.SceneEntities.ForEach(en => en.Render());

            //render opaque
            var opDrawList = drawList[DrawType.Opaque];

            opDrawList.ForEach(drawData =>
            {
                Mesh mesh = drawData.Mesh;
                Shader shader = drawData.Shader;

                if (mesh == null || shader == null)
                    return;

                mesh.Use();
                shader.Use();

                GL.DrawElements(PrimitiveType.Triangles, mesh.IndicesCount, DrawElementsType.UnsignedInt, 0);
            });

            ClearDrawList();
        }

        private void ClearDrawList()
        {
            foreach (var kvp in drawList)
                kvp.Value.Clear();
        }
    }
}
