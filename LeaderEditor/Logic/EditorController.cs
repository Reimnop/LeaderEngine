using ImGuiNET;
using ImGuizmoNET;
using LeaderEngine;
using OpenTK.Mathematics;
using System;

namespace LeaderEditor
{
    public class EditorController : Component
    {
        public static EditorController Main;
        public static EditorRenderer Renderer;

        private Entity editorCamera;

        private ProjectManager pc;
        private CameraMove cm;

        private OPERATION operation = OPERATION.TRANSLATE;

        private void Start()
        {
            if (Main == null)
                Main = this;

            Renderer = (EditorRenderer)Engine.Renderer;

            //init editor gui
            ImGuiController.OnImGui += OnImGui;

            BaseEntity.AddComponent<GridRenderer>();
            BaseEntity.AddComponent<SceneHierachy>();
            BaseEntity.AddComponent<Inspector>();
            BaseEntity.AddComponent<AssetManager>();

            pc = BaseEntity.AddComponent<ProjectManager>();

            editorCamera = new Entity("EditorCamera");
            editorCamera.AddComponent<Camera>();
            cm = editorCamera.AddComponent<CameraMove>();

            editorCamera.Unlist();
        }

        //mouse select
        private void MouseSelect(Ray ray)
        {
            Entity rayHit = null;
            float t = float.PositiveInfinity;

            foreach (var entity in DataManager.CurrentScene.SceneEntities)
                CheckMeshRayHitRecursively(entity);

            SceneHierachy.SelectedEntity = rayHit;

            void CheckMeshRayHitRecursively(Entity entity)
            {
                if (!entity.Active)
                    return;

                //get mesh
                var mr = entity.GetComponent<MeshRenderer>();

                if (mr == null)
                    goto CheckChildren;

                if (mr.Mesh == null)
                    goto CheckChildren;

                if (EditorRaycast.MeshRaycast(mr.Mesh, ray, entity.Transform.GlobalModelMatrix, out float dist))
                {
                    if (t > dist)
                    {
                        t = dist;
                        rayHit = entity;
                    }
                }

                CheckChildren:
                foreach (var child in entity.Children)
                    CheckMeshRayHitRecursively(child);
            }
        }

        private void OnImGui()
        {
            //dockspace
            ImGui.DockSpaceOverViewport();

            if (ImGui.BeginMainMenuBar())
            {
                pc.DrawFileMenu();

                ImGui.EndMainMenuBar();
            }

            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new System.Numerics.Vector2(160f, 90f));
            if (ImGui.Begin("Viewport"))
            {
                cm.Focus = ImGui.IsWindowFocused();

                var cPos = ImGui.GetCursorScreenPos();
                var vSize = ImGui.GetContentRegionAvail();

                Renderer.ViewportSize = new Vector2i((int)vSize.X, (int)vSize.Y);

                //display framebuffer texture on window
                ImGui.GetWindowDrawList().AddImage(
                    (IntPtr)Renderer.FramebufferTexture,
                    cPos,
                    cPos + vSize,
                    new System.Numerics.Vector2(0f, 1f),
                    new System.Numerics.Vector2(1f, 0f));

                //gizmos
                ImGui.PushClipRect(cPos, cPos + vSize, false);
                if (SceneHierachy.SelectedEntity != null)
                {
                    var entity = SceneHierachy.SelectedEntity;

                    ImGuizmo.SetOrthographic(false);
                    ImGuizmo.SetDrawlist();

                    ImGuizmo.SetRect(cPos.X, cPos.Y, vSize.X, vSize.Y);

                    Camera.Main.CalculateViewProjection(out var view, out var projection);

                    Matrix4 transform = entity.Transform.GlobalModelMatrix;
                    ImGuizmo.Manipulate(ref view.Row0.X, ref projection.Row0.X, operation, MODE.LOCAL, ref transform.Row0.X);

                    if (ImGuizmo.IsUsing())
                    {
                        Matrix4 parentGlobal = entity.Parent != null ? entity.Parent.Transform.GlobalModelMatrix : Matrix4.Identity;
                        Matrix4 local = transform * parentGlobal.Inverted();

                        entity.Transform.Position = local.ExtractTranslation();
                        entity.Transform.Rotation = local.ExtractRotation();
                        entity.Transform.Scale = local.ExtractScale();
                    }
                }

                ImGui.SetCursorScreenPos(cPos + new System.Numerics.Vector2(4f));

                ImGui.SetNextItemWidth(240f);
                if (ImGui.BeginCombo("Gizmo", operation == OPERATION.TRANSLATE ? "Translate" : (operation == OPERATION.ROTATE ? "Rotate" : "Scale")))
                {
                    if (ImGui.Selectable("Translate", operation == OPERATION.TRANSLATE))
                        operation = OPERATION.TRANSLATE;
                    if (ImGui.Selectable("Rotate", operation == OPERATION.ROTATE))
                        operation = OPERATION.ROTATE;
                    if (ImGui.Selectable("Scale", operation == OPERATION.SCALE))
                        operation = OPERATION.SCALE;
                }
                ImGui.PopClipRect();

                //mouse select with raycast
                if (ImGui.IsWindowHovered() && ImGui.IsWindowFocused() && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGuizmo.IsUsing())
                {
                    Ray ray;
                    ray.Origin = Camera.Main.BaseTransform.GlobalModelMatrix.ExtractTranslation(); //get cam position

                    var mPos = (ImGui.GetMousePos() - cPos) / vSize; //get mouse pos in [0, 1]
                    Vector2 mousePos = new Vector2(mPos.X, mPos.Y) * new Vector2(2f) - new Vector2(1f); //convert to [-1, 1]

                    Camera.Main.CalculateViewProjection(out var view, out var proj); //get view and proj

                    Vector4 unprojected = new Vector4(mousePos.X, -mousePos.Y, 1f, 1f) * Matrix4.Invert(view * proj); //what the fuck?
                    ray.Direction = Vector3.Normalize(unprojected.Xyz);

                    MouseSelect(ray);
                }

                ImGui.End();
            }
            ImGui.PopStyleVar();

            if (ImGui.Begin("Debug"))
            {
                ImGui.Text($"Frametime: {MathF.Floor(Time.UnscaledDeltaTime * 100000f) / 100f}ms");
                ImGui.Text($"Framerate: {MathF.Floor(1f / Time.UnscaledDeltaTime)}");

#if DEBUG
                ImGui.DragFloat("Exposure", ref ((ForwardRenderer)Engine.Renderer).Exposure, 0.1f);
#endif

                ImGui.End();
            }
        }
    }
}
