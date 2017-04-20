namespace Hdg
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization.Formatters.Binary;

    public class rdtClientEnumerateServers
    {
        [CompilerGenerated]
        private bool k__BackingField;
        private IPEndPoint m_endPoint;
        private object m_lock = new object();
        private List<rdtServerAddress> m_servers = new List<rdtServerAddress>();
        private UdpClient m_udpHello;

        public rdtClientEnumerateServers()
        {
            this.StartHelloListening();
        }

        private void OnReceiveHelloCallback(IAsyncResult result)
        {
            try
            {
                rdtUdpMessageHello hello;
                using (MemoryStream stream = new MemoryStream(this.m_udpHello.EndReceive(result, ref this.m_endPoint)))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    hello = formatter.Deserialize(stream) as rdtUdpMessageHello;
                }
                if (hello != null)
                {
                    lock (this.m_lock)
                    {
                        rdtServerAddress item = new rdtServerAddress(this.m_endPoint.Address, hello.m_deviceName, hello.m_deviceType, hello.m_devicePlatform, hello.m_serverVersion);
                        int index = this.m_servers.IndexOf(item);
                        if (index >= 0)
                        {
                            this.m_servers[index].m_timer = 0.0;
                        }
                        else
                        {
                            rdtDebug.Debug(this, string.Concat(new object[] { "Found a new server ", item.IPAddress, " called ", item.FormattedName, " timestamp=", DateTime.Now.TimeOfDay.TotalSeconds.ToString() }), new object[0]);
                            rdtDebug.Debug(this, "Server has version " + item.m_serverVersion, new object[0]);
                            this.m_servers.Add(item);
                        }
                        goto Label_0160;
                    }
                }
                rdtDebug.Error(this, "Ignoring invalid message", new object[0]);
            Label_0160:
                this.m_udpHello.BeginReceive(new AsyncCallback(this.OnReceiveHelloCallback), null);
            }
            catch (ObjectDisposedException)
            {
                rdtDebug.Debug(this, "Hello listener disposed", new object[0]);
                this.Stop();
            }
        }

        public void Reset()
        {
            lock (this.m_lock)
            {
                this.m_servers.Clear();
            }
        }

        private void StartHelloListening()
        {
            rdtDebug.Debug(this, "Starting hello listener", new object[0]);
            this.Stopped = false;
            try
            {
                IPEndPoint localEP = new IPEndPoint(IPAddress.Any, rdtSettings.CLIENT_ID_PORT);
                this.m_udpHello = new UdpClient(localEP);
                this.m_udpHello.BeginReceive(new AsyncCallback(this.OnReceiveHelloCallback), null);
            }
            catch (Exception exception)
            {
                rdtDebug.Error(this, exception, "Exception", new object[0]);
                this.Stop();
            }
        }

        public void Stop()
        {
            if (this.m_udpHello != null)
            {
                this.m_udpHello.Close();
                this.m_udpHello = null;
            }
            this.Stopped = true;
        }

        public void Update(double delta)
        {
            lock (this.m_lock)
            {
                List<rdtServerAddress> list = new List<rdtServerAddress>();
                foreach (rdtServerAddress address in this.m_servers)
                {
                    address.m_timer += delta;
                    if (address.m_timer > (rdtSettings.BROADCAST_TIME * 2f))
                    {
                        list.Add(address);
                    }
                }
                foreach (rdtServerAddress address2 in list)
                {
                    rdtDebug.Debug(this, "Removing " + address2.FormattedName + " because we haven't heard from it  timestamp=" + DateTime.Now.TimeOfDay.TotalSeconds.ToString(), new object[0]);
                    this.m_servers.Remove(address2);
                }
            }
        }

        public List<rdtServerAddress> Servers
        {
            get
            {
                lock (this.m_lock)
                {
                    return new List<rdtServerAddress>(this.m_servers);
                }
            }
        }

        public bool Stopped
        {
            [CompilerGenerated]
            get
            {
                return this.k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.k__BackingField = value;
            }
        }
    }
}

