namespace Hdg
{
    using System;
    using UnityEditor;
    using UnityEngine;

    public class rdtGuiFoldout
    {
        public static bool Draw(bool foldout, string content, bool toggleOnLabelClick)
        {
            return Draw(foldout, new GUIContent(content), toggleOnLabelClick, EditorStyles.foldout);
        }

        public static bool Draw(bool foldout, string content, bool toggleOnLabelClick, GUIStyle style)
        {
            return Draw(foldout, new GUIContent(content), toggleOnLabelClick, style);
        }

        public static bool Draw(bool foldout, GUIContent content, bool toggleOnLabelClick, GUIStyle style)
        {
            return EditorGUI.Foldout(GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, style), foldout, content, toggleOnLabelClick, style);
        }
    }
}

