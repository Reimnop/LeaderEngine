using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class Transform
    {
        public Vector3 OriginOffset = Vector3.Zero;

        public Vector3 Position = Vector3.Zero;
        public Vector3 Scale = Vector3.One;

        public Matrix4 GlobalTransform
        {
            get
            {
                //calculate the model matrix
                if (Position == Vector3.Zero && Scale == Vector3.One && Rotation == Quaternion.Identity)
                    return baseEntity.Parent != null ? baseEntity.Parent.Transform.GlobalTransform : Matrix4.Identity;

                Matrix4 res =
                    Matrix4.CreateTranslation(-OriginOffset)
                    * Matrix4.CreateScale(Scale)
                    * Matrix4.CreateFromQuaternion(internalRotation)
                    * Matrix4.CreateTranslation(Position);

                if (baseEntity.Parent != null)
                    res *= baseEntity.Parent.Transform.GlobalTransform;

                return res;
            }
        }

        public Quaternion Rotation
        {
            get => internalRotation;
            set
            {
                //check equal
                if (internalRotation == value)
                    return;

                internalRotation = value;
                Quaternion.ToEulerAngles(value, out Vector3 euler);

                //convert to degrees
                internalEulerAngles = new Vector3(
                    MathHelper.RadiansToDegrees(euler.X),
                    MathHelper.RadiansToDegrees(euler.Y),
                    MathHelper.RadiansToDegrees(euler.Z));
            }
        }
        public Vector3 EulerAngles
        {
            get => internalEulerAngles;
            set
            {
                if (internalEulerAngles == value)
                    return;

                internalEulerAngles = value;

                //convert to radians
                Vector3 radEuler = new Vector3(
                    MathHelper.DegreesToRadians(value.X),
                    MathHelper.DegreesToRadians(value.Y),
                    MathHelper.DegreesToRadians(value.Z));

                Quaternion.FromEulerAngles(radEuler, out internalRotation);
            }
        }

        private Quaternion internalRotation = Quaternion.Identity;
        private Vector3 internalEulerAngles = Vector3.Zero;

        //direction vectors
        public Vector3 Forward => Vector3.Transform(-Vector3.UnitZ, Quaternion.Conjugate(GlobalTransform.ExtractRotation()));
        public Vector3 Right => Vector3.Transform(Vector3.UnitX, Quaternion.Conjugate(GlobalTransform.ExtractRotation()));
        public Vector3 Up => Vector3.Transform(Vector3.UnitY, Quaternion.Conjugate(GlobalTransform.ExtractRotation()));

        internal Matrix4 ModelMatrix = Matrix4.Identity;

        private Entity baseEntity;

        internal Transform(Entity baseEntity)
        {
            this.baseEntity = baseEntity;
        }

        internal void CalculateModelMatrixRecursively()
        {
            //calculate the model matrix
            if (Position == Vector3.Zero && Scale == Vector3.One && Rotation == Quaternion.Identity)
            {
                ModelMatrix = baseEntity.Parent != null ? baseEntity.Parent.Transform.ModelMatrix : Matrix4.Identity;
                goto CalculateChildren;
            }
            
            Matrix4 res =
                Matrix4.CreateTranslation(-OriginOffset)
                * Matrix4.CreateScale(Scale)
                * Matrix4.CreateFromQuaternion(internalRotation)
                * Matrix4.CreateTranslation(Position);

            if (baseEntity.Parent != null)
                res *= baseEntity.Parent.Transform.ModelMatrix;

            ModelMatrix = res;

            //calculate children
        CalculateChildren:
            baseEntity.Children.ForEach(x => x.Transform.CalculateModelMatrixRecursively());
        }
    }
}
