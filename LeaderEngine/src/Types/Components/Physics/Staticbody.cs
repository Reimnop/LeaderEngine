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
                new System.Numerics.Vector3(transform.LocalPosition.X, transform.LocalPosition.Y, transform.LocalPosition.Z),
                new CollidableDescription(collider.ShapeIndex, 0.01f)));

            transform.OnPositionChange += OnPositionChange;
        }

        private void OnPositionChange(OpenTK.Mathematics.Vector3 obj)
        {
            PhysicsController.Simulation.Statics.ApplyDescription(handle,
                new StaticDescription(
                    new System.Numerics.Vector3(transform.LocalPosition.X, transform.LocalPosition.Y, transform.LocalPosition.Z),
                    new CollidableDescription(collider.ShapeIndex, 0.01f))
                );
        }

        public override void OnRemove()
        {
            transform.OnPositionChange -= OnPositionChange;
            PhysicsController.Simulation.Statics.Remove(handle);
        }
    }
}
