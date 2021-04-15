﻿using LeaderEngine;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

namespace LeaderEditor
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Engine.Init(new GameWindowSettings(), new NativeWindowSettings()
            {
                APIVersion = new Version(4, 5),
                WindowBorder = WindowBorder.Resizable,
                API = ContextAPI.OpenGL,
                Flags = ContextFlags.ForwardCompatible,
                Profile = ContextProfile.Core,
                Size = new Vector2i(1600, 900),
                Title = "LeaderEditor"
            }, InitEditor, new EditorRenderer());
        }

        private static void InitEditor()
        {
            Engine.IgnoreGLInfo = true;

            Entity editorScripts = new Entity("EditorEntity");
            editorScripts.AddComponent<EditorController>();

            AudioClip ac = AudioClip.FromFile("song", "song.wav");

            var source = AudioManager.GetAudioSource("source");
            source.Clip = ac;
            source.Play();

            Logger.Log("Editor initialized.", true);
        }
    }
}
