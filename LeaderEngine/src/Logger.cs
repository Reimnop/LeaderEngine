using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LeaderEngine
{
    public static class Logger
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Log(object msg)
        {
            Console.WriteLine($"[{Assembly.GetCallingAssembly().GetName().Name}] " + msg.ToString());
        }
    }
}
