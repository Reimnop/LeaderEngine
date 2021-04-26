using System;
using System.Text;
using System.IO;

namespace LeaderEngine
{
    public class FntParser : IDisposable
    {
        private StreamReader reader;

        public FntParser(string path)
        {
            reader = new StreamReader(File.Open(path, FileMode.Open));
        }

        public bool NextToken(out string token, out bool eol)
        {
            while (NextTokenIntern(out token, out eol))
            {
                if (string.IsNullOrWhiteSpace(token))
                    continue;

                return true;
            }

            return false;
        }

        private bool NextTokenIntern(out string token, out bool eol)
        {
            eol = false;

            StringBuilder sb = new StringBuilder();

            bool canEnd = true;

            bool eos;
            char c;
            while (Advance(reader, out c, out eos, out eol) || !canEnd)
            {
                if (c == '"')
                {
                    canEnd = !canEnd;
                    continue;
                }

                sb.Append(c);
            }

            token = sb.ToString();
            return !eos;
        }

        private bool Advance(StreamReader reader, out char c, out bool endOfStream, out bool endOfLine)
        {
            endOfStream = true;
            endOfLine = false;
            c = (char)0;

            if (reader.EndOfStream)
                return false;

            endOfStream = false;
            c = (char)reader.Read();

            if (c == '\n')
                endOfLine = true;

            return c switch
            {
                '=' => false,
                ' ' => false,
                '\n' => false,
                _ => true
            };
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
