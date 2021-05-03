using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LeaderEngine
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class VertexAttrib : Attribute
    {
        public VertexAttribPointerType PointerType;
        public int Location;
        public int Size;
        public bool Normalized;

        public VertexAttrib(VertexAttribPointerType pointerType, int location, int size, bool normalized)
        {
            if (location == 0)
                throw new Exception("Location cannot be 0!");

            PointerType = pointerType;
            Location = location;
            Normalized = normalized;
            Size = size;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexData
    {
        [VertexAttrib(VertexAttribPointerType.Float, 1, 3, false)]
        public Vector3 Normal;

        [VertexAttrib(VertexAttribPointerType.Float, 2, 3, false)]
        public Vector3 Color;

        [VertexAttrib(VertexAttribPointerType.Float, 3, 2, false)]
        public Vector2 UV;
    }

    public sealed class Mesh : IDisposable
    {
        private struct VertexAttribData
        {
            public VertexAttribPointerType PointerType;
            public int Location;
            public int Size;
            public int Offset;
            public bool Normalized;
        }

        public readonly string Name;
        public readonly string ID;

        //vertex data
        public Vector3[] Vertices { private set; get; }
        public uint[] Indices { private set; get; }

        private int VAO, VBO0, VBO1, EBO;
        private VertexAttribData[] vertexAttribs;

        private Type perVertexDataType;

        public PrimitiveType PrimitiveType { private set; get; }
        public DrawElementsType DrawElementsType { private set; get; }

        public int VerticesCount => Vertices.Length;
        public int IndicesCount => Indices.Length;

        public Mesh(string name, string id = null)
        {
            Name = name;

            //generate buffers
            VAO = GL.GenVertexArray();
            VBO0 = GL.GenBuffer();
            VBO1 = GL.GenBuffer();
            EBO = GL.GenBuffer();

            ID = id ?? RNG.GetRandomID();

            DataManager.Meshes.Add(ID, this);
        }

        internal void Unlist()
        {
            DataManager.Meshes.Remove(ID);
        }

        public void LoadMesh(
            Vector3[] vertices, uint[] indices, 
            PrimitiveType primitiveType = PrimitiveType.Triangles, 
            DrawElementsType drawElementsType = DrawElementsType.UnsignedInt)
        {
            PrimitiveType = primitiveType;
            DrawElementsType = drawElementsType;

            Vertices = vertices;
            Indices = indices;

            GL.BindVertexArray(VAO);

            //upload vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO0);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Vector3.SizeInBytes, vertices, BufferUsageHint.StaticDraw);

            //upload element buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            //vertex attrib
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.ObjectLabel(ObjectLabelIdentifier.VertexArray, VAO, Name.Length + 4, Name + "-VAO");
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, VBO0, Name.Length + 5, Name + "-VBO0");
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, EBO, Name.Length + 4, Name + "-EBO");
        }

        public void SetPerVertexData<T>(T[] data) where T : struct
        {
            perVertexDataType = typeof(T);

            //upload buffer to gpu
            int vertexSize = Unsafe.SizeOf<T>();

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO1);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * vertexSize, data, BufferUsageHint.StaticDraw);

            //vertex attribs
            FieldInfo[] fields = typeof(T).GetFields();

            vertexAttribs = new VertexAttribData[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                var attribs = fields[i].GetCustomAttributes(typeof(VertexAttrib));
                VertexAttrib attrib = attribs.Count() > 0 ? (VertexAttrib)attribs.First() : throw new ArgumentNullException();

                int offset = Marshal.OffsetOf<T>(fields[i].Name).ToInt32();

                GL.VertexAttribPointer(attrib.Location, attrib.Size, attrib.PointerType, attrib.Normalized, vertexSize, offset);
                GL.EnableVertexAttribArray(attrib.Location);

                //store vertex attrib
                vertexAttribs[i] = new VertexAttribData
                {
                    PointerType = attrib.PointerType,
                    Location = attrib.Location,
                    Size = attrib.Size,
                    Offset = offset,
                    Normalized = attrib.Normalized
                };
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, VBO1, Name.Length + 5, Name + "-VBO1");
        }

        public void Use()
        {
            GL.BindVertexArray(VAO);
        }

        internal void Serialize(BinaryWriter writer)
        {
            //write name
            writer.Write(Name);
            //write id
            writer.Write(ID);
            //write prim type
            writer.Write((int)PrimitiveType);
            //write draw elem type
            writer.Write((int)DrawElementsType);

            //get buffers
            byte[] vertexBuffer = Helper.StructArrayToByteArray(Vertices);
            byte[] elementBuffer = Helper.StructArrayToByteArray(Indices);

            //write buffers
            writer.Write(VerticesCount);
            writer.Write(vertexBuffer);

            writer.Write(IndicesCount);
            writer.Write(elementBuffer);

            //write per vertex data
            bool hasPerVertexData = perVertexDataType != null;
            writer.Write(hasPerVertexData);

            if (hasPerVertexData)
            {
                byte[] perVertexBuffer = new byte[VerticesCount * Marshal.SizeOf(perVertexDataType)];
                GL.GetNamedBufferSubData(VBO1, IntPtr.Zero, perVertexBuffer.Length, perVertexBuffer);

                writer.Write(perVertexDataType.AssemblyQualifiedName);
                writer.Write(perVertexBuffer);
            }
        }

        public static Mesh Deserialize(BinaryReader reader)
        {
            //read name
            string name = reader.ReadString();
            //read id
            string id = reader.ReadString();
            //read prim type
            var primType = (PrimitiveType)reader.ReadInt32();
            //read draw elem type
            var drawElemType = (DrawElementsType)reader.ReadInt32();

            //read buffers
            int verticesCount = reader.ReadInt32();
            Vector3[] vertices = Helper.ByteArrayToStructArray<Vector3>(reader.ReadBytes(verticesCount * Vector3.SizeInBytes));

            int indicesCount = reader.ReadInt32();
            uint[] indices = Helper.ByteArrayToStructArray<uint>(reader.ReadBytes(indicesCount * sizeof(uint)));

            bool hasPerVertexData = reader.ReadBoolean();
            dynamic perVertexData = null;

            if (hasPerVertexData)
            {
                Type perVertexType = Type.GetType(reader.ReadString());
                int size = Marshal.SizeOf(perVertexType);

                MethodInfo m = 
                    typeof(Helper)
                    .GetMethod("ByteArrayToStructArray", BindingFlags.Static | BindingFlags.Public)
                    .MakeGenericMethod(perVertexType);

                perVertexData = m.Invoke(null, new object[] { reader.ReadBytes(verticesCount * size) });
            }

            dynamic mesh = new Mesh(name, id);
            mesh.LoadMesh(vertices, indices, primType, drawElemType);

            if (hasPerVertexData)
                mesh.SetPerVertexData(perVertexData);

            return mesh;
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(VAO);
            GL.DeleteBuffer(VBO0);
            GL.DeleteBuffer(VBO1);
            GL.DeleteBuffer(EBO);

            DataManager.Meshes.Remove(ID);
        }
    }
}