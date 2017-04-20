namespace Hdg
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct rdtTcpMessageGetComponents : rdtTcpMessage
    {
        public int m_instanceId;
    }
}

