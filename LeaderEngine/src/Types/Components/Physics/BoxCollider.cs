using BepuPhysics.Collidables;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class BoxCollider : Collider
    {
        public Vector3 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                UpdateShape(BaseTransform.Scale);
            }
        }

        private Vector3 _scale = Vector3.One;

        public override void Start()
        {
            var box = new Box(_scale.X * BaseTransform.Scale.X, _scale.Y * BaseTransform.Scale.Y, _scale.Z * BaseTransform.Scale.Z);
            ShapeIndex = PhysicsController.Simulation.Shapes.Add(box);

            Shape = box;

            BaseTransform.OnScaleChange += UpdateShape;
            BaseTransform.OnPositionChange += UpdateShape;
        }

        private void UpdateShape(Vector3 obj)
        {
            PhysicsController.Simulation.Shapes.Remove(ShapeIndex);

            var box = new Box(_scale.X * BaseTransform.Scale.X, _scale.Y * BaseTransform.Scale.Y, _scale.Z * BaseTransform.Scale.Z);
            ShapeIndex = PhysicsController.Simulation.Shapes.Add(box);

            Shape = box;
        }

        public override void OnRemove()
        {
            BaseTransform.OnScaleChange -= UpdateShape;
            BaseTransform.OnPositionChange -= UpdateShape;

            PhysicsController.Simulation.Shapes.Remove(ShapeIndex);
        }
    }
}
