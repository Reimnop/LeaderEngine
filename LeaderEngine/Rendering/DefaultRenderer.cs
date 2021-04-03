using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEngine.Rendering
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
            throw new NotImplementedException();
        }

        public override void PushDrawData(DrawType drawType, GLDrawData drawData)
        {
            drawList[drawType].Add(drawData);
        }

        public override void Render()
        {



            ClearDrawList();
        }

        private void ClearDrawList()
        {
            foreach (var kvp in drawList)
                kvp.Value.Clear();
        }
    }
}
