namespace Hdg
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct rdtTcpMessageComponents : rdtTcpMessage
    {
        public List<Component> m_components;
        public int m_layer;
        public string m_tag;
        public bool m_enabled;
        public int m_instanceId;
        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Component
        {
            public bool m_canBeDisabled;
            public bool m_enabled;
            public string m_name;
            public string m_assemblyName;
            public int m_instanceId;
            public List<rdtTcpMessageComponents.Property> m_properties;
            public override string ToString()
            {
                return string.Format("Component {0}", this.m_name);
            }
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Property
        {
            public string m_name;
            public object m_value;
            public Type m_type;
            public override string ToString()
            {
                return string.Format("{0} = {1}", this.m_name, this.m_value);
            }

            public rdtTcpMessageComponents.Property Clone()
            {
                rdtTcpMessageComponents.Property property = new rdtTcpMessageComponents.Property();
                property.m_name = this.m_name;
                property.m_type = this.m_type;
                return property;
            }

            public void Deserialise(rdtSerializerRegistry registry)
            {
                List<rdtTcpMessageComponents.Property> list = this.m_value as List<rdtTcpMessageComponents.Property>;
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].Deserialise(registry);
                    }
                }
                else
                {
                    this.m_value = registry.Deserialize(this.m_value);
                }
            }
            public enum Type
            {
                Field,
                Property,
                Method
            }
        }
    }
}

