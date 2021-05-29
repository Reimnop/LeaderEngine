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

                if (value == null)
                    scene.SceneRootEntities.Add(this);
                else
                    scene.SceneRootEntities.Remove(this);
            }
        }

        public bool Active = true;

        internal List<Entity> Children { get; } = new List<Entity>();
        private List<Component> components { get; } = new List<Component>();

        public List<IRenderer> Renderers { get; } = new List<IRenderer>();
        public List<IShadowMapRenderer> ShadowMapRenderers { get; } = new List<IShadowMapRenderer>();

        private Scene scene;

        public Entity(string name, string tag = null, Entity parent = null, Scene scene = null)
        {
            Name = name;
            Tag = tag == null ? string.Empty : tag;

            Transform = new Transform(this);

            this.scene = scene != null ? scene : DataManager.CurrentScene;

            if (parent == null)
            {
                this.scene.SceneRootEntities.Add(this);
            }
            else
            {
                _parent = parent;
                parent.Children.Add(this);
            }
        }

        internal void Unlist()
        {
            if (_parent == null)
            {
                scene.SceneRootEntities.Remove(this);
            }

            GlobalData.UnlistedEntities.Add(this);
        }

        internal void RecursivelyUpdate()
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

            foreach (var child in Children)
                child.RecursivelyUpdate();
        }

        internal void RecursivelyRender(in RenderData renderData)
        {
            if (!Active)
                return;

            foreach (var renderer in Renderers)
                renderer.Render(renderData);

            foreach (var child in Children)
                child.RecursivelyRender(renderData);
        }

        internal void RecursivelyRenderShadowMap(in LightData lightData)
        {
            if (!Active)
                return;

            foreach (var renderer in ShadowMapRenderers)
                renderer.RenderShadowMap(lightData);

            foreach (var child in Children)
                child.RecursivelyRenderShadowMap(lightData);
        }

        public void Destroy()
        {
            _parent?.Children.Remove(this);
            scene.SceneRootEntities.Remove(this);

            while (Children.Count > 0)
                Children[0].Destroy();
        }

        #region ComponentGettersSetters
        public T GetComponent<T>() where T : Component
        {
            return (T)components.Find(c => typeof(T).IsAssignableFrom(c.GetType()));
        }
        public List<Component> GetComponents<T>() where T : Component
        {
            return components.FindAll(c => typeof(T).IsAssignableFrom(c.GetType()));
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
