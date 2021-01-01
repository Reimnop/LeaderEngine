using OpenTK.Audio.OpenAL;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace LeaderEngine
{
    public class AudioSource : Component
    {
        public string Path;

        private int bufferHandle, sourceHandle;

        private static ALContext context;
        private static ALDevice device;

        public static void Init()
        {
            device = ALC.OpenDevice(null);
            context = ALC.CreateContext(device, (int[])null);

            ALC.MakeContextCurrent(context);
        }

        public override void Start()
        {
            bufferHandle = AL.GenBuffer();

            byte[] sound_data = LoadWave(File.Open(Path, FileMode.Open), out int channels, out int bits_per_sample, out int sample_rate);

            AL.BufferData(bufferHandle, GetSoundFormat(channels, bits_per_sample), ref sound_data[0], sound_data.Length, sample_rate);

            sourceHandle = AL.GenSource();
            AL.Source(sourceHandle, ALSourcef.Gain, 1.0f);
            AL.Source(sourceHandle, ALSourcef.Pitch, 1.0f);
            AL.Source(sourceHandle, ALSource3f.Position, ref transform.Position);

            AL.Source(sourceHandle, ALSourcei.Buffer, bufferHandle);
            AL.SourcePlay(sourceHandle);
        }

        public override void OnRemove()
        {
            AL.SourceStop(sourceHandle);
            AL.DeleteSource(sourceHandle);
            AL.DeleteBuffer(bufferHandle);
        }

        private static byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
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

        private static ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }
    }
}
