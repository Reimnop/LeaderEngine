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
            collider = BaseEntity.GetComponent<Collider>();

            handle = PhysicsController.Simulation.Statics.Add(new StaticDescription(
                new System.Numerics.Vector3(BaseTransform.LocalPosition.X, BaseTransform.LocalPosition.Y, BaseTransform.LocalPosition.Z),
                new CollidableDescription(collider.ShapeIndex, 0.01f)));

            BaseTransform.OnPositionChange += OnPositionChange;
        }

        private void OnPositionChange(OpenTK.Mathematics.Vector3 obj)
        {
            PhysicsController.Simulation.Statics.ApplyDescription(handle,
                new StaticDescription(
                    new System.Numerics.Vector3(BaseTransform.LocalPosition.X, BaseTransform.LocalPosition.Y, BaseTransform.LocalPosition.Z),
                    new CollidableDescription(collider.ShapeIndex, 0.01f))
                );
        }

        public override void OnRemove()
        {
            BaseTransform.OnPositionChange -= OnPositionChange;
            PhysicsController.Simulation.Statics.Remove(handle);
        }
    }
}
