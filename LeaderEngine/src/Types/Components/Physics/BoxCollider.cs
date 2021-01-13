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
                UpdateShape(Transform.Scale);
            }
        }

        private Vector3 _scale = Vector3.One;

        public override void Start()
        {
            var box = new Box(_scale.X * Transform.Scale.X, _scale.Y * Transform.Scale.Y, _scale.Z * Transform.Scale.Z);
            ShapeIndex = PhysicsController.Simulation.Shapes.Add(box);

            Shape = box;

            Transform.OnScaleChange += UpdateShape;
            Transform.OnPositionChange += UpdateShape;
        }

        private void UpdateShape(Vector3 obj)
        {
            PhysicsController.Simulation.Shapes.Remove(ShapeIndex);

            var box = new Box(_scale.X * Transform.Scale.X, _scale.Y * Transform.Scale.Y, _scale.Z * Transform.Scale.Z);
            ShapeIndex = PhysicsController.Simulation.Shapes.Add(box);

            Shape = box;
        }

        public override void OnRemove()
        {
            Transform.OnScaleChange -= UpdateShape;
            Transform.OnPositionChange -= UpdateShape;

            PhysicsController.Simulation.Shapes.Remove(ShapeIndex);
        }
    }
}
