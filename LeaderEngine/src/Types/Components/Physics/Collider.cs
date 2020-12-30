using BepuPhysics.Collidables;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEngine
{
    public class Collider : Component
    {
        public TypedIndex ShapeIndex;
        public IConvexShape Shape;
    }
}
