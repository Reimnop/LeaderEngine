using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class Transform : Component
    {
        public Vector3 Position = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 RotationEuler = Vector3.Zero;
        public Vector3 Scale = Vector3.One;

        public Vector3 Forward => (Matrix4.CreateFromQuaternion(Rotation) * -Vector4.UnitZ).Xyz;
        public Vector3 Right => (Matrix4.CreateFromQuaternion(Rotation) * Vector4.UnitX).Xyz;
        public Vector3 Up => (Matrix4.CreateFromQuaternion(Rotation) * Vector4.UnitY).Xyz;

        Vector3 lastEuler = Vector3.Zero;
        Quaternion lastQuat = Quaternion.Identity;

        public void UpdateRotation()
        {
            if (RotationEuler != lastEuler)
            {
                Quaternion quat = Quaternion.FromEulerAngles(
                    MathHelper.DegreesToRadians(RotationEuler.X),
                    MathHelper.DegreesToRadians(RotationEuler.Y),
                    MathHelper.DegreesToRadians(RotationEuler.Z)
                    );

                Rotation = quat;
                lastQuat = quat;
            }

            if (Rotation != lastQuat)
            {
                Vector3 tempEuler = Rotation.ToEulerAngles();

                tempEuler.X = MathHelper.RadiansToDegrees(tempEuler.X);
                tempEuler.Y = MathHelper.RadiansToDegrees(tempEuler.Y);
                tempEuler.Z = MathHelper.RadiansToDegrees(tempEuler.Z);

                RotationEuler = tempEuler;
                lastEuler = tempEuler;
            }

            lastEuler = RotationEuler;
            lastQuat = Rotation;
        }
    }
}
