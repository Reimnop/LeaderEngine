using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;
using System;
using System.IO;

namespace LeaderEngine
{
    internal static class AudioLoader
    {
        public static byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                int sample_rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();

                string data_signature = new string(reader.ReadChars(4));
                if (data_signature != "data")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int data_chunk_size = reader.ReadInt32();

                channels = num_channels;
                bits = bits_per_sample;
                rate = sample_rate;

                return reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }

        public static ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }
    }

    public class AudioSource : Component
    {
        private int handle;

        private float _gain = 1f;
        private float _pitch = 1f;
        private bool _loop = true;

        public float Gain
        {
            get => _gain;
            set
            {
                if (_gain == value)
                    return;

                _gain = value;
                AL.Source(handle, ALSourcef.Gain, value);
            }
        }
        public float Pitch
        {
            get => _pitch;
            set
            {
                if (_pitch == value)
                    return;

                _pitch = value;
                AL.Source(handle, ALSourcef.Pitch, value);
            }
        }
        public bool Looping
        {
            get => _loop;
            set
            {
                if (_loop == value)
                    return;

                _loop = value;
                AL.Source(handle, ALSourceb.Looping, value);
            }
        }

        public bool Playing { get; private set; }

        private AudioClip _audioClip;

        public AudioClip Clip
        {
            get => _audioClip;
            set
            {
                if (_audioClip == value)
                    return;

                if (Playing)
                {
                    Logger.LogError("Cannot set clip while playing!");
                    return;
                }

                if (value == null) 
                    return;

                _audioClip = value;
                AL.Source(handle, ALSourcei.Buffer, value.GetHandle());
            }
        }

        private void Start()
        {
            handle = AL.GenSource();
            AL.Source(handle, ALSourcef.Gain, _gain);
            AL.Source(handle, ALSourcef.Pitch, _pitch);
            AL.Source(handle, ALSourceb.Looping, _loop);
        }

        private void Update()
        {
            Vector3 pos = BaseTransform.GlobalModelMatrix.ExtractTranslation();
            AL.Source(handle, ALSource3f.Position, ref pos);
        }

        private void OnRemove()
        {
            AL.SourceStop(handle);
            AL.DeleteSource(handle);
        }

        public void Play()
        {
            Playing = true;
            AL.SourcePlay(handle);
        }

        public void Pause()
        {
            AL.SourcePause(handle);
        }

        public void Stop()
        {
            Playing = false;
            AL.SourceStop(handle);
        }

        public float GetPlayPosition()
        {
            AL.GetSource(handle, ALGetSourcei.SampleOffset, out int bPosition);
            return bPosition / (float)Clip.SampleRate;
        }

        public int GetHandle()
        {
            return handle;
        }
    }

    public class AudioListener : Component
    {
        private float[] orientation = new float[6];

        private void Update()
        {
            var pos = BaseTransform.GlobalModelMatrix.ExtractTranslation();
            AL.Listener(ALListener3f.Position, ref pos);

            var fw = BaseTransform.Forward;
            var up = BaseTransform.Up;

            orientation[0] = fw.X;
            orientation[1] = fw.Y;
            orientation[2] = fw.Z;
            orientation[3] = up.X;
            orientation[4] = up.Y;
            orientation[5] = up.Z;

            AL.Listener(ALListenerfv.Orientation, ref orientation[0]);
        }
    }

    public static class AudioManager
    {
        internal static void Init()
        {
            string deviceName = ALC.GetString(ALDevice.Null, AlcGetString.DefaultDeviceSpecifier);
            var device = ALC.OpenDevice(deviceName);
            var context = ALC.CreateContext(device, (int[])null);
            ALC.MakeContextCurrent(context);

            CheckALError();

            Logger.Log("Audio initialized.");
        }

        internal static void CheckALError()
        {
            ALError error = AL.GetError();
            if (error != ALError.NoError)
                Logger.LogError($"OpenAL: {AL.GetErrorString(error)}");
        }
    }
}
