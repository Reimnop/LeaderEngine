using System;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public abstract class GLRenderer
    {
        private Vector2i _viewPortSize = new Vector2i(1600, 900);
        public Vector2i ViewportSize
        {
            get => _viewPortSize;
            set
            {
                _viewPortSize.X = Math.Max(1, value.X);
                _viewPortSize.Y = Math.Max(1, value.Y);
            }
        }

        public abstract void Init();
        public abstract void Update();
        public abstract void QueueCommandsShadowMap(CommandBuffer commandBuffer);
        public abstract void QueueCommandsOpaque(CommandBuffer commandBuffer);
        public abstract void QueueCommandsTransparent(CommandBuffer commandBuffer);
        public abstract void QueueCommandsGUI(CommandBuffer commandBuffer);
        public abstract void Render();
    }
}
