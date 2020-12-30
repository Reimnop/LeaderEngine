using System;
using System.Collections.Generic;
using System.Text;
using BepuPhysics;
using BepuPhysics.Collidables;

namespace LeaderEngine
{
    public class Rigidbody : Component
    {
        private Collider collider;

        private BodyHandle handle;

        public override void Start()
        {
            collider = gameObject.GetComponent<Collider>();
            collider.Shape.ComputeInertia(1.0f, out var bodyInertia);

            handle = PhysicsController.Simulation.Bodies.Add(BodyDescription.CreateDynamic(
                new System.Numerics.Vector3(transform.Position.X, transform.Position.Y, transform.Position.Z),
                bodyInertia,
                new CollidableDescription(collider.ShapeIndex, 0.01f),
                new BodyActivityDescription(0.05f)));

            PhysicsController.OnPhysicsUpdate += OnPhysicsUpdate;
        }

        private void OnPhysicsUpdate(Simulation sim)
        {
            BodyReference body = sim.Bodies.GetBodyReference(handle);

            transform.Position = new OpenTK.Mathematics.Vector3(body.Pose.Position.X, body.Pose.Position.Y, body.Pose.Position.Z);
            transform.Rotation = new OpenTK.Mathematics.Quaternion(body.Pose.Orientation.X, body.Pose.Orientation.Y, body.Pose.Orientation.Z, body.Pose.Orientation.W);
        }

        public override void OnRemove()
        {
            PhysicsController.OnPhysicsUpdate -= OnPhysicsUpdate;

            PhysicsController.Simulation.Bodies.Remove(handle);
        }
    }
}
