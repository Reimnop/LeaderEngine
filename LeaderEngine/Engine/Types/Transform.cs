using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEngine
{
    public class Transform : Component
    {
        public Vector3 position;
        public Quaternion rotation = Quaternion.FromEulerAngles(0.0f, 0.0f, 2.0f);
        public Vector3 scale = Vector3.One;
    }
}
