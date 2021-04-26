using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class FntParser : IDisposable
    {
        private class TokenSeparator
        {
            public readonly string BaseString;
            public readonly int Length;

            public int Position;

            public TokenSeparator(string str)
            {
                BaseString = str.TrimEnd();
                Length = BaseString.Length;
            }

            public bool NextToken(out string tokenString)
            {
                tokenString = null;

                //get out!
                if (Position >= Length)
                    return false;

                //remove spaces
                StringBuilder stringBuilder = new StringBuilder();

                bool forceRead = true;
                bool canRead = false;

                do
                {
                    if (!canRead && BaseString[Position] != ' ')
                    {
                        forceRead = false;
                        canRead = true;
                    }

                    if (BaseString[Position] == '"')
                        forceRead = !forceRead;

                    if (canRead)
                        stringBuilder.Append(BaseString[Position]);

                    Position++;
                }
                while (Position < Length && (BaseString[Position] != ' ' || forceRead));

                tokenString = stringBuilder.ToString();
                return true;
            }
        }

        private struct Assignment
        {
            public string Name;
            public string Value;

            public static Assignment FromString(string str)
            {
                StringBuilder name = new StringBuilder();
                StringBuilder value = new StringBuilder();

                int pos = 0;

                //get name
                while (str[pos] != '=')
                {
                    name.Append(str[pos]);
                    pos++;
                }

                pos++;

                //get value
                while (pos < str.Length)
                {
                    if (str[pos] != '"')
                        value.Append(str[pos]);
                    pos++;
                }

                return new Assignment
                {
                    Name = name.ToString(),
                    Value = value.ToString()
                };
            }
        }

        public struct FntCharacter
        {
            public Vector2i Position;
            public Vector2i Size;
            public Vector2i Offset;
            public int Advance;
        }

        private StreamReader reader;

        public string FilePath;

        #region FontData
        public string FontName;
        public int FontSize;
        public bool IsBold;
        public bool IsItalic;

        public int PaddingTop;
        public int PaddingLeft;
        public int PaddingBottom;
        public int PaddingRight;

        public int LineHeight;
        public int Base;

        public Vector2i TextureSize;
        public Texture FontTexture;

        public int CharacterCount;

        public Dictionary<int, FntCharacter> Characters = new Dictionary<int, FntCharacter>();
        #endregion

        private static Dictionary<string, Action<FntParser, Stack<Assignment>>> parseFuncs = new Dictionary<string, Action<FntParser, Stack<Assignment>>>()
        {
            { "info", ParseInfo },
            { "common", ParseCommon },
            { "page", ParsePage },
            { "chars", ParseChars },
            { "char", ParseChar }
        };

        public FntParser(string path)
        {
            FilePath = path;

            reader = new StreamReader(File.Open(path, FileMode.Open));
            LoadFile();
        }

        private void LoadFile()
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                var tokens = new TokenSeparator(line);
                if (tokens.NextToken(out string token))
                {
                    Stack<Assignment> assignments = new Stack<Assignment>();
                    while (tokens.NextToken(out string tk))
                        assignments.Push(Assignment.FromString(tk));

                    if (parseFuncs.TryGetValue(token, out var func))
                        func.Invoke(this, assignments);
                }
            }
        }

        #region ParseFuncs
        private static void ParseInfo(FntParser parser, Stack<Assignment> assignments)
        {
            while (assignments.Count > 0)
            {
                Assignment a = assignments.Pop();

                switch (a.Name)
                {
                    case "face":
                        parser.FontName = a.Value;
                        break;
                    case "size":
                        parser.FontSize = int.Parse(a.Value);
                        break;
                    case "bold":
                        parser.IsBold = a.Value == "1";
                        break;
                    case "italic":
                        parser.IsItalic = a.Value == "1";
                        break;
                    case "padding":
                        string[] paddingStrs = a.Value.Split(',');
                        parser.PaddingTop = int.Parse(paddingStrs[0]);
                        parser.PaddingLeft = int.Parse(paddingStrs[1]);
                        parser.PaddingBottom = int.Parse(paddingStrs[2]);
                        parser.PaddingRight = int.Parse(paddingStrs[3]);
                        break;
                }
            }
        }
        private static void ParseCommon(FntParser parser, Stack<Assignment> assignments)
        {
            while (assignments.Count > 0)
            {
                Assignment a = assignments.Pop();

                switch (a.Name)
                {
                    case "lineHeight":
                        parser.LineHeight = int.Parse(a.Value);
                        break;
                    case "base":
                        parser.Base = int.Parse(a.Value);
                        break;
                    case "scaleW":
                        parser.TextureSize.X = int.Parse(a.Value);
                        break;
                    case "scaleH":
                        parser.TextureSize.Y = int.Parse(a.Value);
                        break;
                }
            }
        }
        private static void ParsePage(FntParser parser, Stack<Assignment> assignments)
        {
            while (assignments.Count > 0)
            {
                Assignment a = assignments.Pop();

                switch (a.Name)
                {
                    case "file":
                        parser.FontTexture = Texture.FromFile(parser.FontName + "-Font", Path.Combine(Path.GetDirectoryName(parser.FilePath), a.Value));
                        break;
                }
            }
        }
        private static void ParseChars(FntParser parser, Stack<Assignment> assignments)
        {
            while (assignments.Count > 0)
            {
                Assignment a = assignments.Pop();

                switch (a.Name)
                {
                    case "count":
                        parser.CharacterCount = int.Parse(a.Value);
                        break;
                }
            }
        }
        private static void ParseChar(FntParser parser, Stack<Assignment> assignments)
        {
            int id = 0;
            FntCharacter character = new FntCharacter();

            while (assignments.Count > 0)
            {
                Assignment a = assignments.Pop();

                switch (a.Name)
                {
                    case "id":
                        id = int.Parse(a.Value);
                        break;
                    case "x":
                        character.Position.X = int.Parse(a.Value);
                        break;
                    case "y":
                        character.Position.Y = int.Parse(a.Value);
                        break;
                    case "width":
                        character.Size.X = int.Parse(a.Value);
                        break;
                    case "height":
                        character.Size.Y = int.Parse(a.Value);
                        break;
                    case "xoffset":
                        character.Offset.X = int.Parse(a.Value);
                        break;
                    case "yoffset":
                        character.Offset.Y = int.Parse(a.Value);
                        break;
                    case "xadvance":
                        character.Advance = int.Parse(a.Value);
                        break;
                }
            }

            parser.Characters.Add(id, character);
        }
        #endregion

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
