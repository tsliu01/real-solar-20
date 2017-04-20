namespace Hdg
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct rdtTcpMessageUpdateComponentProperties : rdtTcpMessage
    {
        public int m_gameObjectInstanceId;
        public int m_componentInstanceId;
        public string m_componentName;
        public bool m_enabled;
        public List<rdtTcpMessageComponents.Property> m_properties;
    }
}

