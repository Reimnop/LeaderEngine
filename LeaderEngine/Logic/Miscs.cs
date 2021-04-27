using System;
using System.Text;

namespace LeaderEngine
{
    public static class RNG
    {
        private static Random idRng = new Random();
        public static string GetRandomID()
        {
            const int idLength = 16;
            const string characters = "0123456789abcdef";

            int charLength = characters.Length;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < idLength; i++)
            {
                sb.Append(characters[idRng.Next(charLength)]);
            }

            return sb.ToString();
        }
    }
}
