using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEditor.Logic
{
    public static class Extensions
    {
        public static System.Numerics.Vector3 ToSystemVector3(this OpenTK.Mathematics.Vector3 vector3)
        {
            return new System.Numerics.Vector3(vector3.X, vector3.Y, vector3.Z);
        }

        public static OpenTK.Mathematics.Vector3 ToOTKVector3(this System.Numerics.Vector3 vector3)
        {
            return new OpenTK.Mathematics.Vector3(vector3.X, vector3.Y, vector3.Z);
        }
    }
}
