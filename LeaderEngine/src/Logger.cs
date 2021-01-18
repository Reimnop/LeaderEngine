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
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{Assembly.GetCallingAssembly().GetName().Name}] " + msg.ToString());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LogWarning(object msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{Assembly.GetCallingAssembly().GetName().Name}] " + msg.ToString());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LogError(object msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{Assembly.GetCallingAssembly().GetName().Name}] " + msg.ToString());
        }
    }
}
