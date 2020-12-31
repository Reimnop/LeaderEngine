using BepuPhysics.Collidables;

namespace LeaderEngine
{
    public class Collider : Component
    {
        public TypedIndex ShapeIndex;
        public IConvexShape Shape;
    }
}
