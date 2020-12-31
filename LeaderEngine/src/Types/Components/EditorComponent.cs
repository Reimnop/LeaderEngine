using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEngine
{
    public class EditorComponent : Component
    {
        public virtual void EditorStart() { return; }
        public virtual void EditorUpdate() { return; }
        public virtual void EditorRemove() { return; }
    }
}
