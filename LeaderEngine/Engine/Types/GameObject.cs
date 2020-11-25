using System;
using System.Collections.Generic;

namespace LeaderEngine
{
    public class GameObject : IDisposable
    {
        private List<Component> components = new List<Component>();

        public void Update()
        {
            components.ForEach(co => co.Update());
            components.ForEach(co => co.LateUpdate());
        }

        public void Render()
        {
            /*if (!Active)
                return;

            if (VertArray == null || Shader == null)
                return;

            Shader.Use();
            VertArray.Use();*/

            
        }

        public void OnClosing()
        {
            components.ForEach(co => co.OnClosing());
            components.Clear();
            Dispose();
        }

        public void Dispose()
        {
            components.ForEach(co => co.OnRemove());
        }
    }
}
