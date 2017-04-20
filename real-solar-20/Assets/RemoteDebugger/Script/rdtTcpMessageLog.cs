namespace Hdg
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct rdtTcpMessageLog : rdtTcpMessage
    {
        public string m_message;
        public string m_stackTrace;
        public LogType m_logType;
        public rdtTcpMessageLog(string message, string stackTrace, LogType logType)
        {
            this.m_message = message;
            this.m_stackTrace = stackTrace;
            this.m_logType = logType;
        }

        public override string ToString()
        {
            string message = this.m_message;
            if (!string.IsNullOrEmpty(this.m_stackTrace))
            {
                message = message + "\n" + this.m_stackTrace;
            }
            return message;
        }
    }
}

