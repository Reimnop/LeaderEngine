using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace LeaderEngine
{
    public class GameObject : IDisposable
    {
        public string Name;
        public bool ActiveSelf { private set; get; }

        public VertexArray VertexArray;
        public Shader Shader;

        private List<Component> components = new List<Component>();

        public GameObject(string name)
        {
            Name = name;
            Application.instance.GameObjects.Add(this);

            Init();
        }

        private void Init()
        {
            AddComponent<Transform>();
            SetActive(true);
        }

        public void Update()
        {
            components.ForEach(co => co.Update());
        }

        public void LateUpdate()
        {
            components.ForEach(co => co.LateUpdate());
        }

        public void Render()
        {
            if (!ActiveSelf)
                return;

            if (VertexArray == null || Shader == null)
                return;

            Shader.Use();
            VertexArray.Use();

            GL.DrawElements(PrimitiveType.Triangles, VertexArray.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
        }

        public void SetActive(bool active)
        {
            ActiveSelf = active;
        }

        public void AddComponent<T>(params object[] args) where T : Component
        {
            if (typeof(T) == typeof(Transform))
                return;

            components.Add((T)Activator.CreateInstance(typeof(T), args));
        }

        public T GetComponent<T>() where T : Component
        {
            return (T)components.Find(x => x.GetType() == typeof(T));
        }

        public void RemoveComponent<T>() where T : Component
        {
            if (typeof(T) == typeof(Transform))
                return;

            components.Remove(components.Find(x => x.GetType() == typeof(T)));
        }

        public void OnClosing()
        {
            Cleanup();
            components.ForEach(co => co.OnClosing());
        }

        public void Dispose()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            components.ForEach(co => co.OnRemove());

            VertexArray.Dispose();
            Shader.Dispose();
        }
    }
}
