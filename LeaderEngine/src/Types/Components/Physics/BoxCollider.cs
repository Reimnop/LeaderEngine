using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using System.Text;

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
                UpdateShape(transform.Scale);
            }
        }

        private Vector3 _scale = Vector3.One;

        public override void Start()
        {
            var box = new Box(_scale.X * transform.Scale.X, _scale.Y * transform.Scale.Y, _scale.Z * transform.Scale.Z);
            ShapeIndex = PhysicsController.Simulation.Shapes.Add(box);

            Shape = box;

            transform.OnScaleChange += UpdateShape;
            transform.OnPositionChange += UpdateShape;
        }

        private void UpdateShape(Vector3 obj)
        {
            PhysicsController.Simulation.Shapes.Remove(ShapeIndex);

            var box = new Box(_scale.X * transform.Scale.X, _scale.Y * transform.Scale.Y, _scale.Z * transform.Scale.Z);
            ShapeIndex = PhysicsController.Simulation.Shapes.Add(box);

            Shape = box;
        }

        public override void OnRemove()
        {
            transform.OnScaleChange -= UpdateShape;
            transform.OnPositionChange -= UpdateShape;

            PhysicsController.Simulation.Shapes.Remove(ShapeIndex);
        }
    }
}
