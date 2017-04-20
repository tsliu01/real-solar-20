namespace Hdg
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    [Serializable]
    public class rdtSerializerContainerArray : rdtSerializerInterface
    {
        public List<object> m_objects = new List<object>();
        public System.Type m_type;

        public object Deserialize(rdtSerializerRegistry registry)
        {
            Array array = Array.CreateInstance(this.m_type, this.m_objects.Count);
            for (int i = 0; i < this.m_objects.Count; i++)
            {
                array.SetValue(registry.Deserialize(this.m_objects[i]), i);
            }
            return array;
        }

        public static object Serialize(object objIn, rdtSerializerRegistry registry)
        {
            System.Type elementType = objIn.GetType().GetElementType();
            if (elementType.IsSerializable && (elementType != typeof(object)))
            {
                return objIn;
            }
            ICollection is2 = (ICollection) objIn;
            rdtSerializerContainerArray array = new rdtSerializerContainerArray();
            foreach (object obj2 in is2)
            {
                array.m_objects.Add(registry.Serialize(obj2));
            }
            array.m_type = elementType;
            return array;
        }
    }
}

