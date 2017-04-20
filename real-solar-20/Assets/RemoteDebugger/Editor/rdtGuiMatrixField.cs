namespace Hdg
{
    using System;
    using UnityEditor;
    using UnityEngine;

    public class rdtGuiMatrixField
    {
        public static Matrix4x4 Draw(string label, Matrix4x4 matrix, ref bool foldout)
        {
            Matrix4x4 matrixx = matrix;
            EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
            foldout = rdtGuiFoldout.Draw(foldout, label, true);
            if (foldout)
            {
                EditorGUI.indentLevel += 2;
                matrixx.m00 = EditorGUILayout.FloatField("E00", matrixx.m00, new GUILayoutOption[0]);
                matrixx.m01 = EditorGUILayout.FloatField("E01", matrixx.m01, new GUILayoutOption[0]);
                matrixx.m02 = EditorGUILayout.FloatField("E02", matrixx.m02, new GUILayoutOption[0]);
                matrixx.m03 = EditorGUILayout.FloatField("E03", matrixx.m03, new GUILayoutOption[0]);
                matrixx.m10 = EditorGUILayout.FloatField("E10", matrixx.m10, new GUILayoutOption[0]);
                matrixx.m11 = EditorGUILayout.FloatField("E11", matrixx.m11, new GUILayoutOption[0]);
                matrixx.m12 = EditorGUILayout.FloatField("E12", matrixx.m12, new GUILayoutOption[0]);
                matrixx.m13 = EditorGUILayout.FloatField("E13", matrixx.m13, new GUILayoutOption[0]);
                matrixx.m20 = EditorGUILayout.FloatField("E20", matrixx.m20, new GUILayoutOption[0]);
                matrixx.m21 = EditorGUILayout.FloatField("E21", matrixx.m21, new GUILayoutOption[0]);
                matrixx.m22 = EditorGUILayout.FloatField("E22", matrixx.m22, new GUILayoutOption[0]);
                matrixx.m23 = EditorGUILayout.FloatField("E23", matrixx.m23, new GUILayoutOption[0]);
                matrixx.m30 = EditorGUILayout.FloatField("E30", matrixx.m30, new GUILayoutOption[0]);
                matrixx.m31 = EditorGUILayout.FloatField("E31", matrixx.m31, new GUILayoutOption[0]);
                matrixx.m32 = EditorGUILayout.FloatField("E32", matrixx.m32, new GUILayoutOption[0]);
                matrixx.m33 = EditorGUILayout.FloatField("E33", matrixx.m33, new GUILayoutOption[0]);
                EditorGUI.indentLevel -= 2;
            }
            EditorGUILayout.EndVertical();
            return matrixx;
        }
    }
}

