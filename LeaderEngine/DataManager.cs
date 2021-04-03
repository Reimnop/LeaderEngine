using System.Collections;
using System.Collections.Generic;

namespace LeaderEngine
{
    public class Scene
    {
        //list type that ensures there is always a root entity at index 0
        public class EntityCollection : IList<Entity>
        {
            private List<Entity> entityList = new List<Entity>();

            private Entity baseEntity;

            public Entity this[int index]
            {
                get => entityList[index];
                set => entityList[index] = value;
            }

            public int Count => entityList.Count;

            public bool IsReadOnly => false;

            internal EntityCollection(Entity root)
            {
                baseEntity = root;
                entityList.Add(root);
            }

            internal List<Entity> GetInternalList()
            {
                return entityList;
            }

            public void Add(Entity item)
            {
                entityList.Add(item);
            }

            public void Clear()
            {
                entityList.Clear();
                entityList.Add(baseEntity);
            }

            public bool Contains(Entity item)
            {
                return entityList.Contains(item);
            }

            public void CopyTo(Entity[] array, int arrayIndex)
            {
                entityList.CopyTo(array, arrayIndex);
            }

            public IEnumerator<Entity> GetEnumerator()
            {
                return entityList.GetEnumerator();
            }

            public int IndexOf(Entity item)
            {
                return entityList.IndexOf(item);
            }

            public void Insert(int index, Entity item)
            {
                if (index == 0)
                    throw new System.Exception("Cannot insert at index zero!");

                entityList.Insert(index, item);
            }

            public bool Remove(Entity item)
            {
                if (entityList[0] == item)
                    throw new System.Exception("Cannot remove root entity!");

                return entityList.Remove(item);    
            }

            public void RemoveAt(int index)
            {
                if (index == 0)
                    throw new System.Exception("Cannot remove root entity!");

                entityList.RemoveAt(index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return entityList.GetEnumerator();
            }
        }

        public string Name;

        public EntityCollection Entities { get; } = new EntityCollection(new Entity("Root"));
        public Entity RootEntity => Entities[0];

        public Scene(string name)
        {
            Name = name;
        }
    }

    public static class DataManager
    {
        public static Scene CurrentScene { get; private set; } = new Scene("Untitled Scene");
    }
}
