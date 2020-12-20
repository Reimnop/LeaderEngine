using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class DirectionalLight : Component
    {
        public override void Start()
        {
            LightingController.DirectionalLight = this;
        }

        public void GenViewProject(out Matrix4 view, out Matrix4 proj)
        {
            proj = Matrix4.CreateOrthographic(10.0f, 10.0f, 1.0f, 7.5f);

            view = Matrix4.CreateTranslation(-gameObject.Transform.Position) *
                Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(new Vector3(
                    MathHelper.DegreesToRadians(gameObject.Transform.RotationEuler.X),
                    MathHelper.DegreesToRadians(gameObject.Transform.RotationEuler.Y),
                    MathHelper.DegreesToRadians(gameObject.Transform.RotationEuler.Z))
                    ));
        }

        public override void OnRemove()
        {
            LightingController.DirectionalLight = null;
        }
    }
}
