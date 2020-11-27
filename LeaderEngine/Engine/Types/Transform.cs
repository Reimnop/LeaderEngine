using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEngine
{
    public class Transform : Component
    {
        public Vector3 position;
        public Vector3 rotationEuler;
        public Vector3 scale = Vector3.One;
    }
}
