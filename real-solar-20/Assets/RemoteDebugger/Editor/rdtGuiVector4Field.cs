namespace Hdg
{
    using System;
    using UnityEditor;
    using UnityEngine;

    public class rdtGuiVector4Field
    {
        private static float Draw(string label, float value)
        {
            int num = EditorStyles.foldout.padding.left + EditorStyles.foldout.margin.left;
            EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUILayout.Space((float) (2 * num));
            EditorGUIUtility.labelWidth = 130f;
            value = EditorGUILayout.FloatField(label, value, new GUILayoutOption[0]);
            EditorGUIUtility.labelWidth = 0f;
            EditorGUILayout.EndHorizontal();
            return value;
        }

        public static Vector4 Draw(string label, Vector4 value, ref bool foldout)
        {
            EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
            foldout = rdtGuiFoldout.Draw(foldout, label, true, EditorStyles.foldout);
            if (foldout)
            {
                value.x = Draw("X", value.x);
                value.y = Draw("Y", value.y);
                value.z = Draw("Z", value.z);
                value.w = Draw("W", value.w);
            }
            EditorGUILayout.EndVertical();
            return value;
        }
    }
}

