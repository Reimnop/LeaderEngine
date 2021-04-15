using System;
using OpenTK.Audio.OpenAL;
using System.Collections.Generic;
using System.Text;

namespace LeaderEngine
{
    public class AudioClip
    {
        private AudioClip()
        {

        }

        public static AudioClip FromFile(string path)
        {
            return new AudioClip();
        }
    }

    public class AudioSource
    {

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

            CheckALError("Start");

            Logger.Log("Audio initialized.");
        }

        internal static void CheckALError(string str)
        {
            ALError error = AL.GetError();
            if (error != ALError.NoError)
            {
                Console.WriteLine($"ALError at '{str}': {AL.GetErrorString(error)}");
            }
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
