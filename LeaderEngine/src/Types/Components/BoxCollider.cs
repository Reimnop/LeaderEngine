using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LeaderEngine
{
    public class BoxCollider : Component
    {
        public Vector3 Scale = Vector3.One;

        public TypedIndex shapeIndex;
        private Box boxShape;

        private BodyHandle handle;

        public override void Start()
        {
            boxShape = new Box(Scale.X * transform.Scale.X, Scale.Y * transform.Scale.Y, Scale.Z * transform.Scale.Z);
            boxShape.ComputeInertia(1.0f, out var boxInertia);

            shapeIndex = PhysicsController.Simulation.Shapes.Add(boxShape);

            handle = PhysicsController.Simulation.Bodies.Add(BodyDescription.CreateDynamic(new Vector3(
                            transform.Position.X, transform.Position.Y, transform.Position.Z),
                            boxInertia,
                            new CollidableDescription(shapeIndex, 0.1f),
                            new BodyActivityDescription(0.01f)));

            PhysicsController.OnPhysicsUpdate += OnPhysicsUpdate;
        }

        private void OnPhysicsUpdate(Simulation sim)
        {
            var bodyRef = sim.Bodies.GetBodyReference(handle);

            transform.Position = new OpenTK.Mathematics.Vector3(bodyRef.Pose.Position.X, bodyRef.Pose.Position.Y, bodyRef.Pose.Position.Z);
            transform.Rotation = new OpenTK.Mathematics.Quaternion(bodyRef.Pose.Orientation.X, bodyRef.Pose.Orientation.Y, bodyRef.Pose.Orientation.Z, bodyRef.Pose.Orientation.W);
        }

        public override void OnRemove()
        {
            PhysicsController.OnPhysicsUpdate -= OnPhysicsUpdate;

            PhysicsController.Simulation.Bodies.Remove(handle);
            PhysicsController.Simulation.Shapes.Remove(shapeIndex);
        }
    }
}
