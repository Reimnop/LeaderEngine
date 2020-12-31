using BepuPhysics;
using BepuPhysics.Collidables;

namespace LeaderEngine
{
    public class Staticbody : Component
    {
        private Collider collider;

        private StaticHandle handle;

        public override void Start()
        {
            collider = gameObject.GetComponent<Collider>();

            handle = PhysicsController.Simulation.Statics.Add(new StaticDescription(
                new System.Numerics.Vector3(transform.Position.X, transform.Position.Y, transform.Position.Z),
                new CollidableDescription(collider.ShapeIndex, 0.01f)));
        }

        public override void OnRemove()
        {
            PhysicsController.Simulation.Statics.Remove(handle);
        }
    }
}
