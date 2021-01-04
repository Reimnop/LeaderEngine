using OpenTK.Mathematics;
using System;

namespace LeaderEngine
{
    public class Transform : Component
    {
        public Vector3 Position = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 RotationEuler = Vector3.Zero;
        public Vector3 Scale = Vector3.One;

        public Matrix4 ModelMatrix {
            get
            {
                Matrix4 model;
                if (gameObject.Parent == null)
                    model = Matrix4.CreateScale(Scale)
                        * Matrix4.CreateFromQuaternion(Rotation)
                        * Matrix4.CreateTranslation(Position + RenderingGlobals.GlobalPosition);
                else
                    model = Matrix4.CreateScale(Scale)
                        * Matrix4.CreateFromQuaternion(Rotation)
                        * Matrix4.CreateTranslation(Position)
                        * gameObject.Parent.transform.ModelMatrix;
                return model;
            }
        }

        public event Action<Vector3> OnPositionChange;
        public event Action<Quaternion, Vector3> OnRotationChange;
        public event Action<Vector3> OnScaleChange;

        public Vector3 Forward => (Matrix4.CreateFromQuaternion(Rotation) * -Vector4.UnitZ).Xyz;
        public Vector3 Right => (Matrix4.CreateFromQuaternion(Rotation) * Vector4.UnitX).Xyz;
        public Vector3 Up => (Matrix4.CreateFromQuaternion(Rotation) * Vector4.UnitY).Xyz;

        private Vector3 lastEuler = Vector3.Zero;
        private Quaternion lastQuat = Quaternion.Identity;

        private Vector3 lastPosition = Vector3.Zero;
        private Vector3 lastScale = Vector3.One;

        public void UpdateTransform()
        {
            UpdatePosition();
            UpdateRotation();
            UpdateScale();
        }

        private void UpdateRotation()
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

                OnRotationChange?.Invoke(Rotation, RotationEuler);
            }

            if (Rotation != lastQuat)
            {
                Vector3 tempEuler = Rotation.ToEulerAngles();

                tempEuler.X = MathHelper.RadiansToDegrees(tempEuler.X);
                tempEuler.Y = MathHelper.RadiansToDegrees(tempEuler.Y);
                tempEuler.Z = MathHelper.RadiansToDegrees(tempEuler.Z);

                RotationEuler = tempEuler;
                lastEuler = tempEuler;

                OnRotationChange?.Invoke(Rotation, RotationEuler);
            }

            lastEuler = RotationEuler;
            lastQuat = Rotation;
        }

        private void UpdatePosition()
        {
            if (Position != lastPosition)
                OnPositionChange?.Invoke(Position);

            lastPosition = Position;
        }

        private void UpdateScale()
        {
            if (Scale != lastScale)
                OnScaleChange?.Invoke(Scale);

            lastScale = Scale;
        }
    }
}
