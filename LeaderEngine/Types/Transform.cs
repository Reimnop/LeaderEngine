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
                return Quaternion.FromEulerAngles(rotationEuler);
            }
            set
            {
                Quaternion.ToEulerAngles(value, out rotationEuler);
            }
        }
        public Vector3 rotationEuler;
        public Vector3 scale = Vector3.One;

        public Vector3 forward { get { return Vector3.Normalize(rotation * -Vector3.UnitZ); } }
        public Vector3 right { get { return Vector3.Normalize(rotation * Vector3.UnitX); } }
        public Vector3 up { get { return Vector3.Normalize(rotation * Vector3.UnitY); } }

        public override void OnEditorGui()
        {
            System.Numerics.Vector3 posSys = position.ToSystemVector3();
            ImGui.DragFloat3("Position", ref posSys, 0.05f);
            position = posSys.ToOTKVector3();

            System.Numerics.Vector3 rotSys = rotationEuler.ToSystemVector3();
            ImGui.DragFloat3("Rotation", ref rotSys, 0.05f);
            rotationEuler = rotSys.ToOTKVector3();

            System.Numerics.Vector3 scaleSys = scale.ToSystemVector3();
            ImGui.DragFloat3("Scale", ref scaleSys, 0.05f);
            scale = scaleSys.ToOTKVector3();
        }
    }
}
