using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace LeaderEngine
{
    public class ForwardRenderer : GLRenderer
    {
        private Dictionary<DrawType, List<GLDrawData>> drawLists = new Dictionary<DrawType, List<GLDrawData>>()
        {
            { DrawType.Opaque, new List<GLDrawData>() },
            { DrawType.Transparent, new List<GLDrawData>() },
            { DrawType.GUI, new List<GLDrawData>() }
        };

        public override void Init()
        {
            Logger.Log("Renderer initialized.");
        }

        public override void PushDrawData(DrawType drawType, GLDrawData drawData)
        {
            drawLists[drawType].Add(drawData);
        }

        public override void Render()
        {
            //set proper matrices
            DataManager.CurrentScene.SceneEntities.ForEach(en => en.Transform.CalculateModelMatrixRecursively());

            WorldProjection = Matrix4.CreateOrthographic(20.0f, 20.0f, -1.0f, 1.0f);

            //call all render funcs
            DataManager.CurrentScene.SceneEntities.ForEach(en => en.Render());

            //render opaque
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
        }

        private void ClearDrawList()
        {
            foreach (var kvp in drawLists)
                kvp.Value.Clear();
        }
    }
}
