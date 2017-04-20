namespace Hdg
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    internal class rdtServersMenu
    {
        private Action<rdtServerAddress> m_onSelected;
        private List<rdtServerAddress> m_servers = new List<rdtServerAddress>();

        public rdtServersMenu(Action<rdtServerAddress> onSelected)
        {
            this.m_onSelected = onSelected;
        }

        private void OnContextMenu(object userdata)
        {
            this.m_onSelected(userdata as rdtServerAddress);
        }

        public void Show(rdtServerAddress currentServer)
        {
            GUIContent content = new GUIContent("Active Player");
            Rect position = GUILayoutUtility.GetRect(content, EditorStyles.toolbarDropDown);
            GUI.Label(position, content, EditorStyles.toolbarDropDown);
            Event current = Event.current;
            if ((current.isMouse && (current.type == EventType.MouseDown)) && position.Contains(current.mousePosition))
            {
                current.Use();
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("None"), currentServer == null, new GenericMenu.MenuFunction2(this.OnContextMenu), null);
                for (int i = 0; i < this.m_servers.Count; i++)
                {
                    rdtServerAddress userData = this.m_servers[i];
                    menu.AddItem(new GUIContent(userData.ToString()), userData.Equals(currentServer), new GenericMenu.MenuFunction2(this.OnContextMenu), userData);
                }
                menu.DropDown(position);
            }
        }

        public List<rdtServerAddress> Servers
        {
            set
            {
                this.m_servers = value;
                this.m_servers.Sort();
            }
        }
    }
}

