using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LeaderEngine
{
    public static class Logger
    {
        public static bool IgnoreInfo = false;
        public static bool IgnoreWarning = false;
        public static bool IgnoreError = false;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Log(object msg, bool forceShow = false)
        {
            if (IgnoreInfo && !forceShow)
                return;

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{Assembly.GetCallingAssembly().GetName().Name}] " + msg.ToString());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LogWarning(object msg, bool forceShow = false)
        {
            if (IgnoreWarning && !forceShow)
                return;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{Assembly.GetCallingAssembly().GetName().Name}] " + msg.ToString());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LogError(object msg, bool forceShow = false)
        {
            if (IgnoreError && !forceShow)
                return;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{Assembly.GetCallingAssembly().GetName().Name}] " + msg.ToString());
        }
    }
}