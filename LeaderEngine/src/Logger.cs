using System;
using System.Collections.Generic;
using System.Text;

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
