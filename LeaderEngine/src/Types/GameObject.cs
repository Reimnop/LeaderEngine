using System;
using System.Collections.Generic;

namespace LeaderEngine
{
    public enum RenderHint
    {
        World,
        Transparent,
        Gui
    }

    public class GameObject : IDisposable
    {
        public string Name;
        public RenderHint RenderHint;
        public bool ActiveSelf { private set; get; }
        public Transform transform { private set; get; }

        private List<Component> components = new List<Component>();
        private List<EditorComponent> editorComponents = new List<EditorComponent>();

        public GameObject(string name, RenderHint renderHint = RenderHint.World)
        {
            Name = name;

            RenderHint = renderHint;

            List<GameObject> listToAdd = null;

            switch (renderHint)
            {
                case RenderHint.World:
                    listToAdd = Application.main.WorldGameObjects;
                    break;
                case RenderHint.Transparent:
                    listToAdd = Application.main.WorldGameObjects_Transparent;
                    break;
                case RenderHint.Gui:
                    listToAdd = Application.main.GuiGameObjects;
                    break;
            }

            Application.main.ExecuteNextUpdate(() =>
            {
                listToAdd.Add(this);
            });

            Init();
        }

        ~GameObject()
        {
            ThreadManager.ExecuteOnMainThread(() => Dispose());
        }

        private void Init()
        {
            transform = AddComponent<Transform>();
            SetActive(true);
        }

        public void Update()
        {
            if (!ActiveSelf)
                return;

            if (!Application.main.EditorMode)
            {
                Component[] comps = components.ToArray();

                for (int i = 0; i < comps.Length; i++)
                {
                    Component co = comps[i];

                    if (co == null)
                        continue;

                    if (co.Enabled)
                        co.Update();
                }
            }

            EditorComponent[] eComps = editorComponents.ToArray();

            for (int i = 0; i < eComps.Length; i++)
            {
                EditorComponent co = eComps[i];

                if (co == null)
                    continue;

                if (co.Enabled)
                    co.EditorUpdate();
            }
        }

        public void LateUpdate()
        {
            if (!ActiveSelf)
                return;

            if (!Application.main.EditorMode)
            {
                Component[] comps = components.ToArray();

                for (int i = 0; i < comps.Length; i++)
                {
                    Component co = comps[i];

                    if (co == null)
                        continue;

                    if (co.Enabled)
                        co.LateUpdate();
                }
            }

            transform.UpdateTransform();
        }

        public void Render()
        {
            if (!ActiveSelf)
                return;

            Component[] comps = components.ToArray();

            for (int i = 0; i < comps.Length; i++)
            {
                Component co = comps[i];

                if (co == null)
                    continue;

                if (co.Enabled)
                    co.OnRender();
            }
        }

        public void RenderGui()
        {
            if (!ActiveSelf)
                return;

            Component[] comps = components.ToArray();

            for (int i = 0; i < comps.Length; i++)
            {
                Component co = comps[i];

                if (co == null)
                    continue;

                if (co.Enabled)
                    co.OnRenderGui();
            }
        }

        public void SetActive(bool active)
        {
            ActiveSelf = active;
        }

        public T AddComponent<T>(params object[] args) where T : Component
        {
            if (typeof(T) == typeof(Transform) && transform != null)
                return null;

            var comp = (T)Activator.CreateInstance(typeof(T), args);
            components.Add(comp);
            comp.gameObject = this;

            if (typeof(EditorComponent).IsAssignableFrom(typeof(T)))
            {
                EditorComponent eComp = (EditorComponent)(object)comp;
                editorComponents.Add(eComp);
                eComp.EditorStart();
            }

            if (!Application.main.EditorMode)
                comp.Start();

            return comp;
        }

        public Component AddComponent(Component component)
        {
            if (component.GetType() == typeof(Transform) && transform != null)
                return null;

            components.Add(component);
            component.gameObject = this;

            if (typeof(EditorComponent).IsAssignableFrom(component.GetType()))
            {
                EditorComponent eComp = (EditorComponent)component;
                editorComponents.Add(eComp);
                eComp.EditorStart();
            }

            if (!Application.main.EditorMode)
                component.Start();

            return component;
        }

        public void AddComponents(Component[] components)
        {
            foreach (var co in components)
                if (co.GetType() == typeof(Transform))
                    return;

            this.components.AddRange(components);

            foreach (var co in components)
            {
                co.gameObject = this;

                if (typeof(EditorComponent).IsAssignableFrom(co.GetType()))
                {
                    EditorComponent eComp = (EditorComponent)co;
                    editorComponents.Add(eComp);
                    eComp.EditorStart();
                }

                if (!Application.main.EditorMode)
                    co.Start();
            }
        }

        public T GetComponent<T>() where T : Component
        {
            return (T)components.Find(x => x.GetType() == typeof(T) || x.GetType().IsSubclassOf(typeof(T)));
        }

        public List<Component> GetAllComponents()
        {
            return components;
        }

        public void RemoveComponent<T>() where T : Component
        {
            if (typeof(T) == typeof(Transform))
                return;

            Component comp = components.Find(x => x.GetType() == typeof(T));

            if (typeof(EditorComponent).IsAssignableFrom(comp.GetType()))
            {
                EditorComponent eComp = (EditorComponent)comp;
                editorComponents.Add(eComp);
                eComp.EditorRemove();
            }

            if (!Application.main.EditorMode)
                comp.OnRemove();

            components.Remove(comp);
        }

        public void RemoveComponent(Component component)
        {
            if (component.GetType() == typeof(Transform))
                return;

            if (typeof(EditorComponent).IsAssignableFrom(component.GetType()))
            {
                EditorComponent eComp = (EditorComponent)component;
                editorComponents.Add(eComp);
                eComp.EditorRemove();
            }

            if (!Application.main.EditorMode)
                component.OnRemove();

            components.Remove(component);
        }

        public void StartAll()
        {
            components.ForEach(x => x.Start());
        }

        public void RemoveAll()
        {
            components.ForEach(x => x.OnRemove());
        }

        public void Destroy()
        {
            Application.main.ExecuteNextUpdate(() =>
            {
                Dispose();
            });
        }

        public void Dispose()
        {
            Application.main.WorldGameObjects.Remove(this);
            Cleanup();

            GC.SuppressFinalize(this);
        }

        private void Cleanup()
        {
            components.ForEach(co => co.OnRemove());
        }
    }
}
