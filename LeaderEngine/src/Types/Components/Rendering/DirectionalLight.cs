using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class DirectionalLight : EditorComponent
    {
        public override void EditorStart()
        {
            LightingController.DirectionalLight = this;
        }

        public void GenViewProject(out Matrix4 view, out Matrix4 proj)
        {
            proj = Matrix4.CreateOrthographic(128.0f, 128.0f, -64.0f, 64.0f);

            view = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(-new Vector3(
                MathHelper.DegreesToRadians(transform.RotationEuler.X),
                MathHelper.DegreesToRadians(transform.RotationEuler.Y),
                MathHelper.DegreesToRadians(transform.RotationEuler.Z))
                ));
        }

        public override void EditorRemove()
        {
            LightingController.DirectionalLight = null;
        }
    }
}
