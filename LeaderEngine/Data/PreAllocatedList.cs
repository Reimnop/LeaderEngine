using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LeaderEngine
{
    public class PreAllocatedListEnum<T> : IEnumerator<T>
    {
        private PreAllocatedList<T> list;
        private int position = -1;

        T IEnumerator<T>.Current => list[position];
        public object Current => list[position];

        public PreAllocatedListEnum(PreAllocatedList<T> list)
        {
            this.list = list;
        }

        public bool MoveNext()
        {
            position++;
            return position < list.Count;
        }

        public void Reset()
        {
            position = -1;
        }

        public void Dispose()
        {
            
        }
    }

    public class PreAllocatedList<T> : IReadOnlyList<T>
    {
        private T[] internalArray;
        private int count;

        public PreAllocatedList(int capacity)
        {
            internalArray = new T[capacity];
        }

        public int Count => count;

        public T this[int i] => internalArray[i];

        public void Add(T item)
        {
            internalArray[count] = item;
            count++;
        }

        public void Clear()
        {
            count = 0;
        }

        public bool Contains(T item)
        {
            return internalArray.Contains(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new PreAllocatedListEnum<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
