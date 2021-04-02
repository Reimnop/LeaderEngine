using System;
using System.Linq;
using System.Reflection;

namespace LeaderEngine
{
    public class Component
    {
        public Entity BaseEntity { get; internal set; }

        internal Action StartMethod;
        internal Action UpdateMethod;
        internal Action LateUpdateMethod;
        internal Action RemoveMethod;

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
