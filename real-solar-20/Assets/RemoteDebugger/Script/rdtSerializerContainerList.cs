namespace Hdg
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    [Serializable]
    public class rdtSerializerContainerList : rdtSerializerInterface
    {
        public List<object> m_objects = new List<object>();
        public System.Type m_type;

        public object Deserialize(rdtSerializerRegistry registry)
        {
            IList list = (IList) typeof(List<>).MakeGenericType(new System.Type[] { this.m_type }).GetConstructor(System.Type.EmptyTypes).Invoke(null);
            foreach (object obj2 in this.m_objects)
            {
                object obj3 = registry.Deserialize(obj2);
                list.Add(obj3);
            }
            return list;
        }

        public static object Serialize(object objIn, rdtSerializerRegistry registry)
        {
            System.Type type2 = objIn.GetType().GetGenericArguments()[0];
            if (type2.IsSerializable && (type2 != typeof(object)))
            {
                return objIn;
            }
            ICollection is2 = (ICollection) objIn;
            rdtSerializerContainerList list = new rdtSerializerContainerList();
            foreach (object obj2 in is2)
            {
                list.m_objects.Add(registry.Serialize(obj2));
            }
            list.m_type = type2;
            return list;
        }
    }
}

