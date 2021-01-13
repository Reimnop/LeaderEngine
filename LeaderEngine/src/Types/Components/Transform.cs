using OpenTK.Mathematics;
using System;

namespace LeaderEngine
{
    public class Transform : Component
    {
        public Vector3 Position
        {
            get
            {
                Matrix4 model = ModelMatrix;
                return new Vector3(model.M41, model.M42, model.M43);
            }
        }
        public Vector3 LocalPosition = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 RotationEuler = Vector3.Zero;
        public Vector3 Scale = Vector3.One;

        public Matrix4 ModelMatrix
        {
            get
            {
                Matrix4 model;
                if (BaseEntity.Parent == null)
                    model = Matrix4.CreateScale(Scale)
                        * Matrix4.CreateFromQuaternion(Rotation)
                        * Matrix4.CreateTranslation(LocalPosition + RenderingGlobals.GlobalPosition);
                else
                    model = Matrix4.CreateScale(Scale)
                        * Matrix4.CreateFromQuaternion(Rotation)
                        * Matrix4.CreateTranslation(LocalPosition)
                        * BaseEntity.Parent.Transform.ModelMatrix;
                return model;
            }
        }

        public event Action<Vector3> OnPositionChange;
        public event Action<Quaternion, Vector3> OnRotationChange;
        public event Action<Vector3> OnScaleChange;

        public Vector3 Forward => Vector3.Transform(-Vector3.UnitZ, Quaternion.Conjugate(Rotation));
        public Vector3 Right => Vector3.Transform(Vector3.UnitX, Quaternion.Conjugate(Rotation));
        public Vector3 Up => Vector3.Transform(Vector3.UnitY, Quaternion.Conjugate(Rotation));

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
            if (LocalPosition != lastPosition)
                OnPositionChange?.Invoke(LocalPosition);

            lastPosition = LocalPosition;
        }

        private void UpdateScale()
        {
            if (Scale != lastScale)
                OnScaleChange?.Invoke(Scale);

            lastScale = Scale;
        }
    }
}
