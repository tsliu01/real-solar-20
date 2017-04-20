namespace Hdg
{
    using System;
    using System.Net;

    [Serializable]
    public class rdtServerAddress : IComparable<rdtServerAddress>
    {
        private byte[] m_address;
        public string m_clientIP = "";
        public string m_deviceName;
        public string m_devicePlatform;
        public string m_deviceType;
        private string m_formattedName;
        public string m_serverVersion;
        public double m_timer;

        public rdtServerAddress(System.Net.IPAddress address, string name, string type, string platform, string serverVersion)
        {
            this.m_serverVersion = serverVersion;
            this.m_address = address.GetAddressBytes();
            this.m_deviceName = name ?? "";
            this.m_deviceType = type ?? "";
            this.m_devicePlatform = platform ?? "";
            this.m_timer = 0.0;
            bool flag = this.m_deviceName.Equals("<unknown>");
            this.m_formattedName = flag ? "" : (this.m_deviceName + " ");
            if (!string.IsNullOrEmpty(this.m_devicePlatform))
            {
                if (!flag)
                {
                    this.m_formattedName = this.m_formattedName + "- ";
                }
                this.m_formattedName = this.m_formattedName + this.m_devicePlatform + " ";
            }
            this.m_formattedName = this.m_formattedName + string.Format("({0}@{1})", this.m_deviceType, address.ToString());
        }

        public int CompareTo(rdtServerAddress other)
        {
            return this.FormattedName.CompareTo(other.FormattedName);
        }

        public override bool Equals(object o)
        {
            if (o == null)
            {
                return false;
            }
            rdtServerAddress address = o as rdtServerAddress;
            return ((((address.m_deviceName == this.m_deviceName) && (address.m_deviceType == this.m_deviceType)) && (address.m_devicePlatform == this.m_devicePlatform)) && address.IPAddress.Equals(this.IPAddress));
        }

        public override int GetHashCode()
        {
            int num = 13;
            if (this.m_deviceName != null)
            {
                num = (num * 7) + this.m_deviceName.GetHashCode();
            }
            if (this.m_deviceType != null)
            {
                num = (num * 7) + this.m_deviceType.GetHashCode();
            }
            if (this.m_devicePlatform != null)
            {
                num = (num * 7) + this.m_devicePlatform.GetHashCode();
            }
            return ((num * 7) + this.m_address.GetHashCode());
        }

        public override string ToString()
        {
            return this.FormattedName;
        }

        public string FormattedName
        {
            get
            {
                return this.m_formattedName;
            }
        }

        public System.Net.IPAddress IPAddress
        {
            get
            {
                if ((this.m_address != null) && (this.m_address.Length > 0))
                {
                    return new System.Net.IPAddress(this.m_address);
                }
                return null;
            }
        }
    }
}

