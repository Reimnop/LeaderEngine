using System;
using System.IO;
using OpenTK.Audio.OpenAL;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    static class AudioLoader
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

    public class AudioClip : IDisposable
    {
        public readonly string Name;

        private int handle;

        private AudioClip(string name, ALFormat format, byte[] data, int size, int rate)
        {
            Name = name;

            handle = AL.GenBuffer();
            AL.BufferData(handle, format, ref data[0], size, rate);

            DataManager.AudioClips.Add(this);
        }

        public static AudioClip FromFile(string name, string path)
        {
            byte[] data = AudioLoader.LoadWave(File.Open(path, FileMode.Open), out int channels, out int bits, out int rate);
            return new AudioClip(name, AudioLoader.GetSoundFormat(channels, bits), data, data.Length - data.Length % (bits / 8 * channels), rate);
        }

        public int GetHandle()
        {
            return handle;
        }

        public void Dispose()
        {
            AL.DeleteBuffer(handle);

            DataManager.AudioClips.Remove(this);
        }
    }

    public class AudioSource : IDisposable
    {
        private int handle;

        private float _gain = 1.0f;
        private float _pitch = 1.0f;
        private bool _loop = true;
        private Vector3 _position;

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
        public Vector3 Position
        {
            get => _position;
            set
            {
                if (_position == value)
                    return;

                _position = value;
                AL.Source(handle, ALSource3f.Position, ref value);
            }
        }

        public bool Playing { get; private set; }

        private AudioClip _audioClip;

        public AudioClip Clip
        {
            get => _audioClip;
            set
            {
                if (Playing)
                {
                    Logger.LogError("Cannot set clip while playing!");
                    return;
                }

                if (_audioClip == value)
                    return;

                _audioClip = value;
                AL.Source(handle, ALSourcei.Buffer, value.GetHandle());
            }
        }

        public AudioSource()
        {
            handle = AL.GenSource();
            AL.Source(handle, ALSourcef.Gain, _gain);
            AL.Source(handle, ALSourcef.Pitch, _pitch);
            AL.Source(handle, ALSourceb.Looping, _loop);
            AL.Source(handle, ALSource3f.Position, ref _position);
        }

        public void Play()
        {
            Playing = true;
            AL.SourcePlay(handle);
        }

        public void Stop()
        {
            Playing = false;
            AL.SourceStop(handle);
        }

        public void Dispose()
        {
            AL.SourceStop(handle);
            AL.DeleteSource(handle);
        }
    }

    public static class AudioManager
    {
        private static Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();

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

        public static AudioSource GetAudioSource(string id)
        {
            if (audioSources.TryGetValue(id, out AudioSource source))
                return source;

            AudioSource audioSource = new AudioSource();
            audioSources.Add(id, audioSource);
            return audioSource;
        }
    }
}
