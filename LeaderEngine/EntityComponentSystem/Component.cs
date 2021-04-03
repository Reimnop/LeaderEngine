using System;
using System.Linq;
using System.Reflection;

namespace LeaderEngine
{
    public class Component
    {
        public Entity BaseEntity { get; internal set; }
        public Transform BaseTransform => BaseEntity.Transform;

        internal readonly Action StartMethod;
        internal readonly Action UpdateMethod;
        internal readonly Action LateUpdateMethod;
        internal readonly Action RemoveMethod;

        public Component()
        {
            //find methods
            MethodInfo[] methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);

            StartMethod = (Action)methods.FirstOrDefault(x => x.Name == "Start")?.CreateDelegate(typeof(Action), this);
            UpdateMethod = (Action)methods.FirstOrDefault(x => x.Name == "Update")?.CreateDelegate(typeof(Action), this);
            LateUpdateMethod = (Action)methods.FirstOrDefault(x => x.Name == "LateUpdate")?.CreateDelegate(typeof(Action), this);
            RemoveMethod = (Action)methods.FirstOrDefault(x => x.Name == "OnRemove")?.CreateDelegate(typeof(Action), this);
        }
    }
}
