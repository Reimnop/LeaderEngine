using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEngine
{
    public class Transform : Component
    {
        public Vector3 position;
        public Quaternion rotation 
        { 
            get 
            {
                return Quaternion.FromEulerAngles(rotationEuler);
            }
            set
            {
                Quaternion.ToEulerAngles(value, out rotationEuler);
            }
        }
        public Vector3 rotationEuler;
        public Vector3 scale = Vector3.One;

        public Vector3 forward { get { return Vector3.Normalize(rotation * -Vector3.UnitZ); } }
        public Vector3 right { get { return Vector3.Normalize(rotation * Vector3.UnitX); } }
        public Vector3 up { get { return Vector3.Normalize(rotation * Vector3.UnitY); } }
    }
}
