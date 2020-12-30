using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LeaderEngine
{
    public class BoxColliderStatic : Component
    {
        public Vector3 Scale = Vector3.One;

        public TypedIndex shapeIndex;
        private Box boxShape;

        public override void Start()
        {
            boxShape = new Box(Scale.X * transform.Scale.X, Scale.Y * transform.Scale.Y, Scale.Z * transform.Scale.Z);

            shapeIndex = PhysicsController.Simulation.Shapes.Add(boxShape);

            PhysicsController.Simulation.Statics.Add(new StaticDescription(
                new Vector3(transform.Position.X, transform.Position.Y, transform.Position.Z),
                new CollidableDescription(shapeIndex, 0.1f)));
        }
    }
}
