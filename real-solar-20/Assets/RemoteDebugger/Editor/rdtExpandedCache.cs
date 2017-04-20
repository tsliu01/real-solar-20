namespace Hdg
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [Serializable]
    public class rdtExpandedCache
    {
        private Dictionary<string, bool> m_expandedState = new Dictionary<string, bool>();

        public void Clear()
        {
            this.m_expandedState.Clear();
        }

        public bool IsExpanded(rdtTcpMessageComponents.Component component)
        {
            return this.IsExpanded(component.m_instanceId, null);
        }

        public bool IsExpanded(rdtTcpMessageComponents.Component component, rdtTcpMessageComponents.Property property)
        {
            return this.IsExpanded(component.m_instanceId, property.m_name);
        }

        public bool IsExpanded(int instanceId, [Optional, DefaultParameterValue(null)] string suffix)
        {
            string key = instanceId.ToString();
            if (!string.IsNullOrEmpty(suffix))
            {
                key = key + "." + suffix;
            }
            if (this.m_expandedState.ContainsKey(key))
            {
                return this.m_expandedState[key];
            }
            return false;
        }

        public void SetExpanded(bool expanded, rdtTcpMessageComponents.Component component)
        {
            this.SetExpanded(expanded, component.m_instanceId, null);
        }

        public void SetExpanded(bool expanded, rdtTcpMessageComponents.Component component, rdtTcpMessageComponents.Property property)
        {
            this.SetExpanded(expanded, component.m_instanceId, property.m_name);
        }

        public void SetExpanded(bool expanded, int instanceId, [Optional, DefaultParameterValue(null)] string suffix)
        {
            string str = instanceId.ToString();
            if (!string.IsNullOrEmpty(suffix))
            {
                str = str + "." + suffix;
            }
            this.m_expandedState[str] = expanded;
        }
    }
}

