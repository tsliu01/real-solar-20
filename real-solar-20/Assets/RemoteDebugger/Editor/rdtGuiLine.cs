namespace Hdg
{
    using System;
    using UnityEditor;
    using UnityEngine;

    public static class rdtGuiLine
    {
        public static void DrawHorizontalLine()
        {
            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, new GUILayoutOption[] { GUILayout.MaxHeight(1f), GUILayout.ExpandWidth(true) });
            Color color = GUI.color;
            GUI.color = Color.white;
            Color color2 = EditorGUIUtility.isProSkin ? new Color(0.2784314f, 0.2784314f, 0.2784314f, 1f) : new Color(0.3647059f, 0.3647059f, 0.3647059f, 255f);
            EditorGUI.DrawRect(rect, color2);
            GUI.color = color;
        }
    }
}

