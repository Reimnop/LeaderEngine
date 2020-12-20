﻿using System;
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
        public Transform Transform { private set; get; }

        private List<Component> components = new List<Component>();

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
            Transform = AddComponent<Transform>();
            SetActive(true);
        }

        public void Update()
        {
            if (!ActiveSelf)
                return;

            components.ForEach(co => {
                if (co.Enabled)
                    co.Update();
            });
        }

        public void LateUpdate()
        {
            if (!ActiveSelf)
                return;

            components.ForEach(co => {
                if (co.Enabled)
                    co.LateUpdate();
            });
        }

        public void Render()
        {
            if (!ActiveSelf)
                return;

            components.ForEach(co => {
                if (co.Enabled)
                    co.OnRender();
            });
        }

        public void RenderGui()
        {
            if (!ActiveSelf)
                return;

            components.ForEach(co => {
                if (co.Enabled)
                    co.OnRenderGui();
            });
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
            comp.gameObject = this;
            comp.Start();

            return comp;
        }

        public Component AddComponent(Component component)
        {
            if (component.GetType() == typeof(Transform) && Transform != null)
                return null;

            Application.main.ExecuteNextUpdate(() =>
            {
                components.Add(component);
                component.gameObject = this;
                component.Start();
            });

            return component;
        }

        public void AddComponents(Component[] components)
        {
            foreach (var co in components)
                if (co.GetType() == typeof(Transform))
                    return;

            Application.main.ExecuteNextUpdate(() =>
            {
                this.components.AddRange(components);

                foreach (var co in components)
                {
                    co.gameObject = this;
                    co.Start();
                }
            });
        }

        public T GetComponent<T>() where T : Component
        {
            return (T)components.Find(x => x.GetType() == typeof(T));
        }

        public List<Component> GetAllComponents()
        {
            return components;
        }

        public void RemoveComponent<T>() where T : Component
        {
            if (typeof(T) == typeof(Transform))
                return;

            Application.main.ExecuteNextUpdate(() => components.Remove(components.Find(x => x.GetType() == typeof(T))));
        }

        public void RemoveComponent(Component component)
        {
            if (component.GetType() == typeof(Transform))
                return;

            Application.main.ExecuteNextUpdate(() => {
                component.OnRemove();
                components.Remove(component);
            });
        }

        public void OnClosing()
        {
            Cleanup();
            components.ForEach(co => co.OnClosing());
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