using System;

namespace LeaderEngine
{
    public static class Logger
    {
        public static void Log(object msg)
        {
            Console.WriteLine("[LEADERENGINE] " + msg.ToString());
        }
    }
}
