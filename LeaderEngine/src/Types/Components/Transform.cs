using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class Transform : Component
    {
        public Vector3 Position;
        public Quaternion Rotation 
        { 
            get 
            {
                return Quaternion.FromEulerAngles(new Vector3(
                    MathHelper.DegreesToRadians(RotationEuler.X),
                    MathHelper.DegreesToRadians(RotationEuler.Y),
                    MathHelper.DegreesToRadians(RotationEuler.Z))
                    );
            }
            set
            {
                Vector3 tempEuler;
                Quaternion.ToEulerAngles(value, out tempEuler);
                RotationEuler = new Vector3(
                    MathHelper.RadiansToDegrees(tempEuler.X),
                    MathHelper.RadiansToDegrees(tempEuler.Y),
                    MathHelper.RadiansToDegrees(tempEuler.X)
                    );
            }
        }
        public Vector3 RotationEuler;
        public Vector3 Scale = Vector3.One;

        public Vector3 Forward { get { return (Matrix4.CreateFromQuaternion(Rotation) * -Vector4.UnitZ).Xyz; } }
        public Vector3 Right { get { return (Matrix4.CreateFromQuaternion(Rotation) * Vector4.UnitX).Xyz; } }
        public Vector3 Up { get { return (Matrix4.CreateFromQuaternion(Rotation) * Vector4.UnitY).Xyz; } }
    }
}
