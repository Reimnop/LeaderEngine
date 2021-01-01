using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEngine
{
    public class AudioListener : Component
    {
        public override void Start()
        {
            AL.Listener(ALListener3f.Position, ref transform.Position);
            transform.OnPositionChange += OnPositionChange;
        }

        private void OnPositionChange(Vector3 obj)
        {
            AL.Listener(ALListener3f.Position, ref obj);
        }

        public override void OnRemove()
        {
            transform.OnPositionChange -= OnPositionChange;
        }
    }
}
