using OpenTK.Audio.OpenAL;

namespace LeaderEngine
{
    public class AudioListener : Component
    {
        public override void Start()
        {
            var pos = Transform.Position;

            AL.Listener(ALListenerf.Gain, 1.0f);
            AL.Listener(ALListener3f.Position, ref pos);
        }
    }
}
