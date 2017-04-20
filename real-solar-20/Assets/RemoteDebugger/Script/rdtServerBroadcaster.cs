namespace Hdg
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    using UnityEngine;

    internal class rdtServerBroadcaster
    {
        private rdtUdpMessageHello m_message = new rdtUdpMessageHello();
        private bool m_run;
        private Thread m_thread;

        public rdtServerBroadcaster()
        {
            this.m_message.m_deviceName = SystemInfo.deviceName;
            this.m_message.m_deviceType = SystemInfo.deviceType.ToString();
            this.m_message.m_devicePlatform = Application.platform.ToString();
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Version version = executingAssembly.GetName().Version;
            object[] customAttributes = executingAssembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
            string informationalVersion = "";
            if (customAttributes.Length > 0)
            {
                informationalVersion = ((AssemblyInformationalVersionAttribute) customAttributes[0]).InformationalVersion;
            }
            object[] objArray2 = executingAssembly.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
            string configuration = "";
            if (objArray2.Length > 0)
            {
                configuration = ((AssemblyConfigurationAttribute) objArray2[0]).Configuration;
            }
            this.m_message.m_serverVersion = string.Format("{0}.{1}.{2} {3} {4}", new object[] { version.Major, version.Minor, version.Build, informationalVersion, configuration });
            this.m_run = true;
            this.m_thread = new Thread(new ThreadStart(this.ThreadFunc));
            this.m_thread.Name = "rdtServerBroadcaster";
            this.m_thread.Start();
        }

        public void Stop()
        {
            rdtDebug.Debug("Stopping server broadcaster thread", new object[0]);
            this.m_run = false;
            this.m_thread.Join();
        }

        private void ThreadFunc()
        {
            while (this.m_run)
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, rdtSettings.CLIENT_ID_PORT);
                try
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        new BinaryFormatter().Serialize(stream, this.m_message);
                        byte[] dgram = stream.ToArray();
                        UdpClient client = new UdpClient();
                        client.EnableBroadcast = true;
                        client.Connect(endPoint);
                        client.Send(dgram, dgram.Length);
                        client.Close();
                    }
                }
                catch (SocketException exception)
                {
                    rdtDebug.Error(this, "Couldn't broadcast hello message!", new object[] { exception });
                    break;
                }
                catch (Exception exception2)
                {
                    rdtDebug.Error(this, "Error broadcasting hello message", new object[] { exception2 });
                }
                Thread.Sleep((int) (rdtSettings.BROADCAST_TIME * 0x3e8));
            }
            rdtDebug.Debug(this, "Finishing broadcast thread", new object[0]);
        }
    }
}

