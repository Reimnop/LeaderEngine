using Newtonsoft.Json.Linq;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEditor.Data
{
    public static class TypeChanger
    {
        private static Dictionary<Type, Func<object, object>> typeChangeFuncs = new Dictionary<Type, Func<object, object>>()
        {
            { typeof(Vector4), ConvertToVector4 },
            { typeof(Vector3), ConvertToVector3 },
        };

        public static object ConvertType(Type dest, object data)
        {
            if (typeChangeFuncs.ContainsKey(dest))
                return typeChangeFuncs[dest](data);
            else 
                return Convert.ChangeType(data, dest);
        }

        private static object ConvertToVector4(object arg)
        {
            JArray array = (JArray)arg;
            return new Vector4(array[0].Value<float>(), array[1].Value<float>(), array[2].Value<float>(), array[3].Value<float>());
        }

        private static object ConvertToVector3(object arg)
        {
            JArray array = (JArray)arg;
            return new Vector3(array[0].Value<float>(), array[1].Value<float>(), array[2].Value<float>());
        }
    }
}
