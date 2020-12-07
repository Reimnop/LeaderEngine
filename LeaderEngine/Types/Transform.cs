using OpenTK.Mathematics;
using ImGuiNET;

namespace LeaderEngine
{
    public class Transform : Component
    {
        public Vector3 position;
        public Quaternion rotation 
        { 
            get 
            {
                return Quaternion.FromEulerAngles(new Vector3(
                    MathHelper.DegreesToRadians(rotationEuler.X),
                    MathHelper.DegreesToRadians(rotationEuler.Y),
                    MathHelper.DegreesToRadians(rotationEuler.Z))
                    );
            }
            set
            {
                Vector3 tempEuler;
                Quaternion.ToEulerAngles(value, out tempEuler);
                rotationEuler = new Vector3(
                    MathHelper.RadiansToDegrees(tempEuler.X),
                    MathHelper.RadiansToDegrees(tempEuler.Y),
                    MathHelper.RadiansToDegrees(tempEuler.X)
                    );
            }
        }
        public Vector3 rotationEuler;
        public Vector3 scale = Vector3.One;

        public Vector3 forward { get { return Vector3.Normalize(rotation * -Vector3.UnitZ); } }
        public Vector3 right { get { return Vector3.Normalize(rotation * Vector3.UnitX); } }
        public Vector3 up { get { return Vector3.Normalize(rotation * Vector3.UnitY); } }
    }
}
