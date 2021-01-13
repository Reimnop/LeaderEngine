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

    public class Entity : IDisposable
    {
        public string Name;
        public RenderHint RenderHint;
        public bool ActiveSelf { private set; get; }
        public Transform Transform { private set; get; }

        public string Tag = "Default";

        public Entity Parent
        {
            get => _parent;
            set
            {
                _parent?.Children.Remove(this);
                value?.Children.Add(this);
                _parent = value;
            }
        }
        private Entity _parent;

        public List<Entity> Children { private set; get; } = new List<Entity>();

        private List<Component> components = new List<Component>();
        private List<EditorComponent> editorComponents = new List<EditorComponent>();

        public Entity(string name, RenderHint renderHint = RenderHint.World)
        {
            Name = name;

            RenderHint = renderHint;

            List<Entity> listToAdd = null;

            switch (renderHint)
            {
                case RenderHint.World:
                    listToAdd = Application.Main.WorldEntities;
                    break;
                case RenderHint.Transparent:
                    listToAdd = Application.Main.WorldEntities_Transparent;
                    break;
                case RenderHint.Gui:
                    listToAdd = Application.Main.GuiEntities;
                    break;
            }

            listToAdd.Add(this);

            Init();
        }

        ~Entity()
        {
            ThreadManager.ExecuteOnMainThread(() => Dispose());
        }

        private void Init()
        {
            Transform = AddComponent<Transform>();
            SetActive(true);
        }

        public void Update()
        {
            if (!ActiveSelf)
                return;

            if (!Application.Main.EditorMode)
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

            UpdateLocal();
        }

        public void LateUpdate()
        {
            if (!ActiveSelf)
                return;

            if (!Application.Main.EditorMode)
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

            Transform.UpdateTransform();

            LateUpdateLocal();
        }

        private void UpdateLocal()
        {
            Entity[] _children = Children.ToArray();

            for (int i = 0; i < _children.Length; i++)
            {
                _children[i]?.Update();
            }
        }

        private void LateUpdateLocal()
        {
            Entity[] _children = Children.ToArray();

            for (int i = 0; i < _children.Length; i++)
            {
                _children[i]?.LateUpdate();
            }
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
            if (typeof(T) == typeof(Transform) && Transform != null)
                return null;

            var comp = (T)Activator.CreateInstance(typeof(T), args);
            components.Add(comp);
            comp.BaseEntity = this;

            if (typeof(EditorComponent).IsAssignableFrom(typeof(T)))
            {
                EditorComponent eComp = (EditorComponent)(object)comp;
                editorComponents.Add(eComp);
                eComp.EditorStart();
            }

            if (!Application.Main.EditorMode)
                comp.Start();

            return comp;
        }

        public Component AddComponent(Component component)
        {
            if (component.GetType() == typeof(Transform) && Transform != null)
                return null;

            components.Add(component);
            component.BaseEntity = this;

            if (typeof(EditorComponent).IsAssignableFrom(component.GetType()))
            {
                EditorComponent eComp = (EditorComponent)component;
                editorComponents.Add(eComp);
                eComp.EditorStart();
            }

            if (!Application.Main.EditorMode)
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
                co.BaseEntity = this;

                if (typeof(EditorComponent).IsAssignableFrom(co.GetType()))
                {
                    EditorComponent eComp = (EditorComponent)co;
                    editorComponents.Add(eComp);
                    eComp.EditorStart();
                }

                if (!Application.Main.EditorMode)
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

            if (!Application.Main.EditorMode)
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

            if (!Application.Main.EditorMode)
                component.OnRemove();

            components.Remove(component);
        }

        public void ReplaceTransform(Transform transform)
        {
            transform.BaseEntity = this;

            components.Remove(this.Transform);
            components.Insert(0, transform);

            this.Transform = transform;
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
            Dispose();
        }

        public void Dispose()
        {
            Entity[] _children = Children.ToArray();
            for (int i = 0; i < _children.Length; i++)
                _children[i].Parent = null;

            Application.Main.WorldEntities.Remove(this);
            Parent = null;
            Cleanup();

            GC.SuppressFinalize(this);
        }

        private void Cleanup()
        {
            components.ForEach(co => co.OnRemove());
            editorComponents.ForEach(co => co.EditorRemove());
        }
    }
}
