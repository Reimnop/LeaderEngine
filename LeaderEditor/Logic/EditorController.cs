#define dumb_scene_load

using ImGuiNET;
using ImGuizmoNET;
using LeaderEngine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace LeaderEditor
{
    public class EditorController : Component
    {
        public static EditorController Main;
        public static EditorRenderer ERenderer;

        private Entity editorCamera;

        private CameraMove cm;

        private void Start()
        {
            if (Main == null)
                Main = this;

            ERenderer = (EditorRenderer)Engine.Renderer;

            //init editor gui
            ImGuiController.RegisterImGui(ImGuiRenderer);

            BaseEntity.AddComponent<GridRenderer>();
            BaseEntity.AddComponent<SceneHierachy>();
            BaseEntity.AddComponent<Inspector>();
            BaseEntity.AddComponent<AssetManager>();

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

            foreach (var entity in DataManager.CurrentScene.SceneRootEntities)
                CheckMeshRayHitRecursively(entity);

            SceneHierachy.SelectedEntity = rayHit;

            void CheckMeshRayHitRecursively(Entity entity)
            {
                //get mesh
                var mr = entity.GetComponent<MeshRenderer>();

                if (mr == null)
                    goto CheckChildren;

                if (mr.Mesh == null)
                    goto CheckChildren;

                if (EditorRaycast.MeshRaycast(mr.Mesh, ray, entity.Transform.GlobalTransform, out float dist))
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

        private OPERATION operation = OPERATION.TRANSLATE;

        private void ImGuiRenderer()
        {
            //dockspace
            ImGui.DockSpaceOverViewport();

            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
#if DEBUG
                    if (ImGui.BeginMenu("Debug"))
                    {
                        if (ImGui.MenuItem("Save Assets"))
                        {
                            DataManager.SaveGameAssets("game-assets.ldrassets");
                        }

                        if (ImGui.MenuItem("Load Assets"))
                        {
                            DataManager.LoadGameAssets("game-assets.ldrassets");
                        }

#if dumb_scene_load
                        if (ImGui.MenuItem("Reload Scene"))
                        {
                            using (var ms = new System.IO.MemoryStream())
                            {
                                var writer = new System.IO.BinaryWriter(ms);
                                DataManager.CurrentScene.Serialize(writer);

                                ms.Seek(0, System.IO.SeekOrigin.Begin);

                                var reader = new System.IO.BinaryReader(ms);
                                Scene s = Scene.Deserialize(reader);
                            }
                        }
#endif

                        ImGui.EndMenu();
                    }
#endif

                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }

            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new System.Numerics.Vector2(160.0f, 90.0f));
            if (ImGui.Begin("Viewport"))
            {
                cm.Focus = ImGui.IsWindowFocused();

                var cPos = ImGui.GetCursorScreenPos();
                var vSize = ImGui.GetContentRegionAvail();

                ERenderer.ViewportSize = new Vector2i((int)vSize.X, (int)vSize.Y);

                //display framebuffer texture on window
                ImGui.GetWindowDrawList().AddImage(
                    (IntPtr)ERenderer.Framebuffer.GetTexture(FramebufferAttachment.ColorAttachment0),
                    cPos,
                    cPos + vSize,
                    new System.Numerics.Vector2(0.0f, 1.0f),
                    new System.Numerics.Vector2(1.0f, 0.0f));

                //gizmos
                ImGui.PushClipRect(cPos, cPos + vSize, false);
                if (SceneHierachy.SelectedEntity != null)
                {
                    var entity = SceneHierachy.SelectedEntity;

                    ImGuizmo.SetOrthographic(false);
                    ImGuizmo.SetDrawlist();

                    ImGuizmo.SetRect(cPos.X, cPos.Y, vSize.X, vSize.Y);

                    Camera.Main.CalculateViewProjection(out var view, out var projection);

                    var transform = entity.Transform.GlobalTransform;
                    ImGuizmo.Manipulate(ref view.Row0.X, ref projection.Row0.X, operation, MODE.LOCAL, ref transform.Row0.X);

                    if (ImGuizmo.IsUsing())
                    {
                        var parentGlobal = entity.Parent != null ? entity.Parent.Transform.GlobalTransform : Matrix4.Identity;
                        var local = transform * parentGlobal.Inverted();

                        entity.Transform.Position = local.ExtractTranslation();
                        entity.Transform.Rotation = local.ExtractRotation();
                        entity.Transform.Scale = local.ExtractScale();
                    }
                }

                ImGui.SetCursorScreenPos(cPos + new System.Numerics.Vector2(4.0f));

                ImGui.SetNextItemWidth(240.0f);
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
                    ray.Origin = Camera.Main.BaseTransform.GlobalTransform.ExtractTranslation(); //get cam position

                    var mPos = (ImGui.GetMousePos() - cPos) / vSize; //get mouse pos in [0, 1]
                    Vector2 mousePos = new Vector2(mPos.X, mPos.Y) * new Vector2(2.0f) - new Vector2(1.0f); //convert to [-1, 1]

                    Camera.Main.CalculateViewProjection(out var view, out var proj); //get view and proj

                    Vector4 unprojected = new Vector4(mousePos.X, -mousePos.Y, 1.0f, 1.0f) * Matrix4.Invert(view * proj); //what the fuck?
                    ray.Direction = Vector3.Normalize(unprojected.Xyz);

                    MouseSelect(ray);
                }

                ImGui.End();
            }
            ImGui.PopStyleVar();

            if (ImGui.Begin("Debug"))
            {
                ImGui.Text($"Frametime: {MathF.Floor(Time.UnscaledDeltaTime * 100000.0f) / 100.0f}ms");
                ImGui.Text($"Framerate: {MathF.Floor(1.0f / Time.UnscaledDeltaTime)}");

#if DEBUG
                ImGui.DragFloat("Exposure", ref ((ForwardRenderer)Engine.Renderer).Exposure, 0.1f);
#endif

                ImGui.End();
            }
        }
    }
}
