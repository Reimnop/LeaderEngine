using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LeaderEngine
{
    internal static class SceneGlobals
    {
        internal static BinarySerializer[] Serializers = new BinarySerializer[]
        {
            new IntSerializer(),
            new FloatSerializer(),
            new StringSerializer(),
            new MeshSerializer(),
            new MaterialSerializer(),
            new TextureSerializer()
        };
    }

    public class Scene
    {
        public string Name;

        public List<Entity> SceneRootEntities { get; } = new List<Entity>();

        public Scene(string name)
        {
            Name = name;
        }

        internal void UpdateSceneHierachy()
        {
            for (int i = 0; i < SceneRootEntities.Count; i++)
                if (SceneRootEntities[i].Parent == null)
                    SceneRootEntities[i].RecursivelyUpdate();
        }

        #region Binary
        #region Serialize
        internal void Serialize(BinaryWriter writer)
        {
            //write name
            writer.Write(Name);

            //write entities
            writer.Write(SceneRootEntities.Count);
            foreach (var entity in SceneRootEntities)
                RecursivelySerializeEntity(writer, entity, SceneGlobals.Serializers);
        }
        private void RecursivelySerializeEntity(BinaryWriter writer, Entity entity, BinarySerializer[] serializers)
        {
            //write entity name & tag
            writer.Write(entity.Name);
            writer.Write(entity.Tag);

            //write transform
            var trans = entity.Transform;

            //position
            writer.Write(trans.Position.X);
            writer.Write(trans.Position.Y);
            writer.Write(trans.Position.Z);
            //rotation
            writer.Write(trans.Rotation.X);
            writer.Write(trans.Rotation.Y);
            writer.Write(trans.Rotation.Z);
            writer.Write(trans.Rotation.W);
            //scale
            writer.Write(trans.Scale.X);
            writer.Write(trans.Scale.Y);
            writer.Write(trans.Scale.Z);
            //origin
            writer.Write(trans.OriginOffset.X);
            writer.Write(trans.OriginOffset.Y);
            writer.Write(trans.OriginOffset.Z);

            //write components
            var components = entity.GetComponents<Component>();

            writer.Write(components.Count);
            foreach (var comp in components)
                SerializeComponent(writer, comp, serializers);

            //write children
            var children = entity.Children;

            writer.Write(children.Count);
            foreach (var child in children)
                RecursivelySerializeEntity(writer, child, serializers);
        }
        private void SerializeComponent(BinaryWriter writer, Component component, BinarySerializer[] serializers)
        {
            //write component type
            Type type = component.GetType();
            writer.Write(type.AssemblyQualifiedName);

            //write fields
            var fields = type.GetFields();

            writer.Write(fields.Length);
            foreach (var field in fields)
            {
                //get suitable serializer
                BinarySerializer serializer = serializers.FirstOrDefault(s => s.IsSerializable(field.FieldType));

                //ignore field
                if (serializer == null)
                {
                    writer.Write(true);
                    continue;
                }

                writer.Write(false);

                //write field name
                writer.Write(field.Name);

                //write field value
                serializer.SerializeObject(writer, field.GetValue(component));
            }

            //write properties
            var props = type.GetProperties();

            writer.Write(props.Length);
            foreach (var prop in props)
            {
                //get suitable serializer
                BinarySerializer serializer = serializers.FirstOrDefault(s => s.IsSerializable(prop.PropertyType));

                //ignore field
                if (serializer == null)
                {
                    writer.Write(true);
                    continue;
                }

                writer.Write(false);

                //write field name
                writer.Write(prop.Name);

                //write field value
                serializer.SerializeObject(writer, prop.GetValue(component));
            }
        }
        #endregion
        #region Deserialize
        internal static Scene Deserialize(BinaryReader reader)
        {
            //read name
            string name = reader.ReadString();

            //create scene
            Scene scene = new Scene(name);

            //read entities
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                RecursivelyDeserializeEntity(reader, scene, null, SceneGlobals.Serializers);

            return scene;
        }
        private static Entity RecursivelyDeserializeEntity(BinaryReader reader, Scene scene, Entity parent, BinarySerializer[] serializers)
        {
            //read name & tag
            string name = reader.ReadString();
            string tag = reader.ReadString();

            //create entity
            Entity entity = new Entity(name, tag, parent, scene);

            //read transform
            entity.Transform.Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            entity.Transform.Rotation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            entity.Transform.Scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            entity.Transform.OriginOffset = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            //read components
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                entity.AddComponent(DeserializeComponent(reader, serializers));
            }

            //read children
            int childCount = reader.ReadInt32();
            for (int i = 0; i < childCount; i++)
            {
                RecursivelyDeserializeEntity(reader, scene, entity, serializers);
            }

            return entity;
        }
        private static Component DeserializeComponent(BinaryReader reader, BinarySerializer[] serializers)
        {
            string typeName = reader.ReadString();
            Type type = Type.GetType(typeName);

            Component comp = (Component)Activator.CreateInstance(type);

            //read fields
            int fieldCount = reader.ReadInt32();
            for (int i = 0; i < fieldCount; i++)
            {
                //ignore
                if (reader.ReadBoolean())
                    continue;

                string name = reader.ReadString();
                var field = type.GetField(name);

                //get suitable serializer
                BinarySerializer serializer = serializers.FirstOrDefault(s => s.IsSerializable(field.FieldType));

                if (serializer == null)
                    continue;

                field.SetValue(comp, serializer.DeserializeObject(reader));
            }

            //read properties
            int propCount = reader.ReadInt32();
            for (int i = 0; i < propCount; i++)
            {
                //ignore
                if (reader.ReadBoolean())
                    continue;

                string name = reader.ReadString();
                var prop = type.GetProperty(name);

                //get suitable serializer
                BinarySerializer serializer = serializers.FirstOrDefault(s => s.IsSerializable(prop.PropertyType));

                if (serializer == null)
                    continue;

                prop.SetValue(comp, serializer.DeserializeObject(reader));
            }

            return comp;
        }
        #endregion
        #endregion
    }
}
