using System;
using System.Collections.Generic;

namespace LeaderEngine
{
    public class Entity
    {
        public string Name;
        public string Tag;

        public readonly Transform Transform;

        private Entity _parent;
        public Entity Parent
        {
            get => _parent;
            set
            {
                //check equal
                if (value == _parent)
                    return;

                //remove from old parent
                _parent?.Children.Remove(this);
                //add to new parent
                _parent = value;
                _parent?.Children.Add(this);
            }
        }

        public bool Active = true;

        internal List<Entity> Children { get; } = new List<Entity>();
        private List<Component> components = new List<Component>();

        internal List<IRenderer> Renderers { get; } = new List<IRenderer>();
        internal List<IShadowMapRenderer> ShadowMapRenderers { get; } = new List<IShadowMapRenderer>();

        public bool Renderable => Renderers.Count > 0;

        private Scene scene;

        public Entity(string name, string tag = null, Entity parent = null, Scene scene = null)
        {
            Name = name;
            Tag = tag ?? string.Empty;

            Transform = new Transform(this);

            this.scene = scene ?? DataManager.CurrentScene;

            if (parent != null)
            {
                _parent = parent;
                parent.Children.Add(this);
            }

            this.scene.SceneEntities.Add(this);
        }

        internal void Unlist()
        {
            scene.SceneEntities.Remove(this);
            scene = null;

            DataManager.UnlistedEntities.Add(this);
        }

        internal void Update()
        {
            if (!Active)
                return;

            foreach (var component in components)
            {
                if (component.Enabled)
                {
                    component.UpdateMethod?.Invoke();
                }
            }
        }

        internal void InitRenderResources()
        {
            Transform.CacheResoures();
        }

        internal void Render(in RenderData renderData)
        {
            if (!Active)
                return;

            foreach (var renderer in Renderers)
                renderer.Render(renderData);
        }

        internal void RenderShadowMap(in LightData lightData)
        {
            if (!Active)
                return;

            foreach (var renderer in ShadowMapRenderers)
                renderer.RenderShadowMap(lightData);
        }

        public void Destroy()
        {
            _parent?.Children.Remove(this);

            RemoveFromEntityList();

            while (Children.Count > 0)
                Children[0].Destroy();
        }

        private void RemoveFromEntityList()
        {
            if (scene != null)
                scene.SceneEntities.Remove(this);
            else
                DataManager.UnlistedEntities.Remove(this);
        }

        #region ComponentGettersSetters
        public T GetComponent<T>() where T : Component
        {
            return (T)components.Find(c => typeof(T).IsAssignableFrom(c.GetType()));
        }
        public Component[] GetComponents<T>() where T : Component
        {
            return components.FindAll(c => typeof(T).IsAssignableFrom(c.GetType())).ToArray();
        }
        public void AddComponent(Component component) //basic
        {
            components.Add(component);

            if (typeof(IRenderer).IsAssignableFrom(component.GetType()))
                Renderers.Add((IRenderer)component);

            if (typeof(IShadowMapRenderer).IsAssignableFrom(component.GetType()))
                ShadowMapRenderers.Add((IShadowMapRenderer)component);

            component.BaseEntity = this;
            component.StartMethod?.Invoke();
        }
        public T AddComponent<T>(params object[] args) where T : Component
        {
            T comp = (T)Activator.CreateInstance(typeof(T), args);
            AddComponent(comp);
            return comp;
        }
        private void RemoveComponentAt(int index) //basic
        {
            var component = components[index];

            component.RemoveMethod?.Invoke();

            if (typeof(IRenderer).IsAssignableFrom(component.GetType()))
                Renderers.Remove((IRenderer)component);

            if (typeof(IShadowMapRenderer).IsAssignableFrom(component.GetType()))
                ShadowMapRenderers.Remove((IShadowMapRenderer)component);

            components.RemoveAt(index);
        }
        public void RemoveComponent(Component component)
        {
            RemoveComponentAt(components.IndexOf(component));
        }
        public void RemoveComponent<T>() where T : Component
        {
            RemoveComponentAt(components.FindIndex(c => typeof(T).IsAssignableFrom(c.GetType())));
        }
        #endregion
    }
}
