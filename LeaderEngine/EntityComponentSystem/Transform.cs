﻿using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class Transform
    {
        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                RecalculateLocal();
            }
        }
        public Vector3 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                RecalculateLocal();
            }
        }
        public Quaternion Rotation
        {
            get => _rotation;
            set
            {
                //check equal
                if (_rotation == value)
                    return;

                _rotation = value;
                Quaternion.ToEulerAngles(value, out Vector3 euler);

                //convert to degrees
                _eulerAngles = new Vector3(
                    MathHelper.RadiansToDegrees(euler.X),
                    MathHelper.RadiansToDegrees(euler.Y),
                    MathHelper.RadiansToDegrees(euler.Z));

                RecalculateLocal();
            }
        }
        public Vector3 EulerAngles
        {
            get => _eulerAngles;
            set
            {
                if (_eulerAngles == value)
                    return;

                _eulerAngles = value;

                //convert to radians
                Vector3 radEuler = new Vector3(
                    MathHelper.DegreesToRadians(value.X),
                    MathHelper.DegreesToRadians(value.Y),
                    MathHelper.DegreesToRadians(value.Z));

                Quaternion.FromEulerAngles(radEuler, out _rotation);

                RecalculateLocal();
            }
        }

        public Matrix4 LocalModelMatrix => _localModelMatrix;
        public Matrix4 GlobalModelMatrix
        {
            get
            {
                Matrix4 parentMat = Matrix4.Identity;
                if (baseEntity.Parent != null)
                    parentMat = baseEntity.Parent.Transform.GlobalModelMatrix;

                return _localModelMatrix * parentMat;
            }
        }

        private Vector3 _position = Vector3.Zero;
        private Vector3 _scale = Vector3.One;
        private Quaternion _rotation = Quaternion.Identity;
        private Vector3 _eulerAngles = Vector3.Zero;

        private Matrix4 _localModelMatrix = Matrix4.Identity;

        //direction vectors
        public Vector3 Forward => Vector3.Transform(-Vector3.UnitZ, Quaternion.Conjugate(GlobalModelMatrix.ExtractRotation()));
        public Vector3 Right => Vector3.Transform(Vector3.UnitX, Quaternion.Conjugate(GlobalModelMatrix.ExtractRotation()));
        public Vector3 Up => Vector3.Transform(Vector3.UnitY, Quaternion.Conjugate(GlobalModelMatrix.ExtractRotation()));

        private Entity baseEntity;

        internal Transform(Entity baseEntity)
        {
            this.baseEntity = baseEntity;
        }

        private void RecalculateLocal()
        {
            _localModelMatrix = 
                Matrix4.CreateScale(_scale)
                * Matrix4.CreateFromQuaternion(_rotation)
                * Matrix4.CreateTranslation(_position);
        }
    }
}
