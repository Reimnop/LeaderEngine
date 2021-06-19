using OpenTK.Audio.OpenAL;
using System.IO;

namespace LeaderEngine
{
    public class AudioClip : GameAsset
    {
        public override GameAssetType AssetType => GameAssetType.AudioClip;

        public int SampleRate => _sampleRate;
        public int Size => _size;
        public int Handle => _handle;

        private int _sampleRate;
        private int _size;

        private int _handle;

        private AudioClip(string name, ALFormat format, byte[] data, int size, int rate) : base(name)
        {
            _handle = AL.GenBuffer();
            AL.BufferData(_handle, format, ref data[0], size, rate);

            _sampleRate = rate;
            _size = size;
        }

        public static AudioClip FromFile(string name, string path)
        {
            byte[] data = AudioLoader.LoadWave(File.Open(path, FileMode.Open), out int channels, out int bits, out int rate);
            return new AudioClip(name, AudioLoader.GetSoundFormat(channels, bits), data, data.Length - data.Length % (bits / 8 * channels), rate);
        }

        public int GetHandle()
        {
            return _handle;
        }

        public override void Dispose()
        {
            base.Dispose();

            AL.DeleteBuffer(_handle);
        }
    }
}
