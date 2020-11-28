using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEngine
{
    public class Transform : Component
    {
        public Vector3 position;
        public Quaternion rotation { private set; get; }
        public Vector3 rotationEuler;
        public Vector3 scale = Vector3.One;

        public Vector3 forward { get { return rotation * new Vector3(0.0f, 0.0f, -1.0f); } }
        public Vector3 right { get { return rotation * new Vector3(1.0f, 0.0f, 0.0f); } }
        public Vector3 up { get { return rotation * new Vector3(0.0f, 1.0f, 0.0f); } }

        public override void LateUpdate()
        {
            rotation = Quaternion.FromEulerAngles(rotationEuler);
        }
    }
}
