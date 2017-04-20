namespace Hdg
{
    using System;
    using UnityEditor;
    using UnityEngine;

    public class rdtGuiSplit
    {
        private float m_minimumSize;
        private EditorWindow m_parentWindow;
        private bool m_resize;
        private float m_resizeInitPos;
        private float m_rightMargin;
        private float m_separatorPosition = 150f;
        private GUIStyle m_style;

        public rdtGuiSplit(float initPos, float rightMargin, EditorWindow parentWindow)
        {
            this.m_separatorPosition = initPos;
            this.m_minimumSize = initPos;
            this.m_rightMargin = rightMargin;
            this.m_parentWindow = parentWindow;
        }

        public void Draw()
        {
            Event current = Event.current;
            if (this.m_style == null)
            {
                this.m_style = new GUIStyle(GUIStyle.none);
                this.m_style.normal.background = EditorGUIUtility.whiteTexture;
            }
            EditorGUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.Width(4f) });
            GUILayout.Label("", this.m_style, new GUILayoutOption[] { GUILayout.MaxHeight(this.m_parentWindow.position.height) });
            EditorGUILayout.EndVertical();
            Rect lastRect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(lastRect, MouseCursor.ResizeHorizontal);
            if ((current.type == EventType.MouseDown) && lastRect.Contains(current.mousePosition))
            {
                this.m_resizeInitPos = current.mousePosition.x;
                this.m_resize = true;
                current.Use();
            }
            else if (this.m_resize && ((current.type == EventType.MouseUp) || (current.rawType == EventType.MouseUp)))
            {
                this.m_resize = false;
                current.Use();
            }
            if (this.m_resize)
            {
                float num = current.mousePosition.x - this.m_resizeInitPos;
                float width = this.m_parentWindow.position.width;
                if (width > this.m_rightMargin)
                {
                    width -= this.m_rightMargin;
                }
                this.m_separatorPosition = Mathf.Clamp(this.m_separatorPosition + num, this.m_minimumSize, width);
                this.m_resizeInitPos = Mathf.Clamp(current.mousePosition.x, this.m_minimumSize, width);
                this.m_parentWindow.Repaint();
            }
        }

        public float SeparatorPosition
        {
            get
            {
                return this.m_separatorPosition;
            }
        }
    }
}

