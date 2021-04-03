using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class Transform
    {
        public Vector3 OriginOffset = Vector3.Zero;

        public Vector3 Position = Vector3.Zero;
        public Vector3 Scale = Vector3.One;

        public Quaternion Rotation 
        {
            get => internalRotation;
            set
            {
                internalRotation = value;
                Quaternion.ToEulerAngles(value, out internalEulerAngles);
            }
        }
        public Vector3 EulerAngles
        {
            get => internalEulerAngles;
            set
            {
                internalEulerAngles = value;
                Quaternion.FromEulerAngles(value, out internalRotation);
            }
        }

        private Quaternion internalRotation = Quaternion.Identity;
        private Vector3 internalEulerAngles = Vector3.Zero;

        internal Matrix4 ModelMatrix = Matrix4.Identity;

        private Entity baseEntity;

        internal Transform(Entity baseEntity)
        {
            this.baseEntity = baseEntity;
        }

        internal void CalculateModelMatrixRecursively()
        {
            //calculate the model matrix;
            Matrix4 res = Matrix4.Identity;
            if (baseEntity.Parent != null)
                res = baseEntity.Parent.Transform.ModelMatrix;

            res *= Matrix4.CreateTranslation(OriginOffset)
                * Matrix4.CreateScale(Scale)
                * Matrix4.CreateFromQuaternion(internalRotation)
                * Matrix4.CreateTranslation(Position);

            ModelMatrix = res;

            //calculate on children
            baseEntity.Children.ForEach(x => x.Transform.CalculateModelMatrixRecursively());
        }
    }
}
