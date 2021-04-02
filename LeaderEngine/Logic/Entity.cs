using System;
using System.Collections.Generic;

namespace LeaderEngine
{
    public class Entity
    {
        public string Name;

        public List<Entity> Children { get; } = new List<Entity>();

        private List<Component> components { get; } = new List<Component>();

        public Entity(string name)
        {
            Name = name;
        }

        internal void Update()
        {
            components.ForEach(x => x.UpdateMethod?.Invoke());

            Children.ForEach(child => child.Update());
        }

        #region ComponentGettersSetters
        public T GetComponent<T>() where T : Component
        {
            return (T)components.Find(c => typeof(T).IsAssignableFrom(c.GetType()));
        }
        public void AddComponent(Component component) //basic
        {
            components.Add(component);
            
            component.StartMethod?.Invoke();
        }
        public Component AddComponent<T>(params object[] args) where T : Component
        {
            Component comp = (T)Activator.CreateInstance(typeof(T), args);
            AddComponent(comp);
            return comp;
        }
        private void RemoveComponentAt(int index) //basic
        {
            components[index].RemoveMethod?.Invoke();

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
