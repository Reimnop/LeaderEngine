using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LeaderEngine
{
    public class FntParser : IDisposable
    {
        public readonly FntNode RootNode;

        private Stream stream;

        public FntParser(string path)
        {
            RootNode = new FntNode(Path.GetFileNameWithoutExtension(path), string.Empty);
            stream = new FileStream(path, FileMode.Open);

            FntNode currentNode = null;
            while (NextToken(out string token))
            {
                if (!token.Contains('='))
                {
                    if (currentNode != null)
                    {
                        RootNode.AddChildren(currentNode);
                    }
                    currentNode = new FntNode(token, string.Empty);
                    continue;
                }

                string[] splittedToken = token.Split('=');

                string name = splittedToken[0];
                string value = splittedToken[1];

                if (value[0] == '"' && value[value.Length - 1] == '"')
                {
                    value = value.Substring(1, value.Length - 2);
                }

                currentNode.AddChildren(new FntNode(name, value));
            }

            RootNode.AddChildren(currentNode);
        }

        private bool NextToken(out string token)
        {
            token = string.Empty;

            if (stream.Position >= stream.Length)
                return false; //end of stream

            bool inQuotes = false;

            bool tokenStarted = false;

            StringBuilder stringBuilder = new StringBuilder();

            while (stream.Position < stream.Length)
            {
                char c = (char)stream.ReadByte();

                if (!tokenStarted && (c == ' ' || c == '\r' || c == '\n'))
                    continue;

                if (!tokenStarted && c != ' ' && c != '\r' && c != '\n')
                    tokenStarted = true;

                if (tokenStarted)
                {
                    if (c == ' ' && !inQuotes)
                        break;

                    if (c == '\r' || c == '\n')
                        break;

                    if (c == '"')
                        inQuotes = !inQuotes;

                    stringBuilder.Append(c);
                }
            }

            token = stringBuilder.ToString();
            return true;
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}
