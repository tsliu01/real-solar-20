namespace Hdg
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct rdtTcpMessageGameObjects : rdtTcpMessage
    {
        public List<Gob> m_allGobs;
        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Gob
        {
            public bool m_enabled;
            public string m_name;
            public int m_instanceId;
            public bool m_hasParent;
            public int m_parentInstanceId;
            public override string ToString()
            {
                string name = this.m_name;
                if (rdtDebug.s_logLevel == rdtDebug.LogLevel.Debug)
                {
                    name = name + ":" + this.m_instanceId;
                }
                return name;
            }

            public override bool Equals(object obj)
            {
                rdtTcpMessageGameObjects.Gob gob = (rdtTcpMessageGameObjects.Gob) obj;
                return (this.m_instanceId == gob.m_instanceId);
            }

            public override int GetHashCode()
            {
                return this.m_instanceId;
            }
        }
    }
}

