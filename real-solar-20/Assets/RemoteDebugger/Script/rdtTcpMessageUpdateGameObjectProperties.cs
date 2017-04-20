namespace Hdg
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct rdtTcpMessageUpdateGameObjectProperties : rdtTcpMessage
    {
        public int m_instanceId;
        public bool m_enabled;
        public string m_tag;
        public int m_layer;
    }
}

