namespace Hdg
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class rdtGuiProperty
    {
        public static object Draw(string propName, object propValue, ref bool foldout)
        {
            if (propValue == null)
            {
                return null;
            }
            System.Type type = propValue.GetType();
            object obj2 = null;
            if (type == typeof(float))
            {
                return EditorGUILayout.FloatField(propName, (float) propValue, new GUILayoutOption[0]);
            }
#if UNITY_5_3_OR_NEWER
            if (type == typeof(double))
            {
                return EditorGUILayout.DoubleField(propName, (double) propValue, new GUILayoutOption[0]);
            }
#endif
            if (type == typeof(int))
            {
                return EditorGUILayout.IntField(propName, (int) propValue, new GUILayoutOption[0]);
            }
            if (type == typeof(uint))
            {
                return (uint) EditorGUILayout.IntField(propName, (int) ((uint) propValue), new GUILayoutOption[0]);
            }
            if (type == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(propName, (Vector2) propValue, new GUILayoutOption[0]);
            }
            if (type == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(propName, (Vector3) propValue, new GUILayoutOption[0]);
            }
            if (type == typeof(Vector4))
            {
                return rdtGuiVector4Field.Draw(propName, (Vector4) propValue, ref foldout);
            }
            if (type == typeof(Matrix4x4))
            {
                return rdtGuiMatrixField.Draw(propName, (Matrix4x4) propValue, ref foldout);
            }
            if (type == typeof(bool))
            {
                return EditorGUILayout.Toggle(propName, (bool) propValue, new GUILayoutOption[0]);
            }
            if (type.IsEnum)
            {
                return EditorGUILayout.EnumPopup(propName, (Enum) propValue, new GUILayoutOption[0]);
            }
            if (type == typeof(string))
            {
                return EditorGUILayout.TextField(propName, (string) propValue, new GUILayoutOption[0]);
            }
            if (type == typeof(Color))
            {
                return EditorGUILayout.ColorField(propName, (Color) propValue, new GUILayoutOption[0]);
            }
            if (type == typeof(Color32))
            {
                return (Color32) EditorGUILayout.ColorField(propName, (Color) ((Color32) propValue), new GUILayoutOption[0]);
            }
            if (type == typeof(Quaternion))
            {
                Quaternion quaternion = (Quaternion) propValue;
                Vector4 vector = new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
                obj2 = rdtGuiVector4Field.Draw(propName, vector, ref foldout);
                vector = (Vector4) obj2;
                quaternion.x = vector.x;
                quaternion.y = vector.y;
                quaternion.z = vector.z;
                quaternion.w = vector.w;
                return quaternion;
            }
            if (type == typeof(Bounds))
            {
                return EditorGUILayout.BoundsField(propName, (Bounds) propValue, new GUILayoutOption[0]);
            }
            if (type == typeof(Rect))
            {
                return EditorGUILayout.RectField(propName, (Rect) propValue, new GUILayoutOption[0]);
            }
            if (type.IsArray)
            {
                Array arr = (Array) propValue;
                return rdtGuiArrayField.Draw(propName, arr, ref foldout);
            }
            if (type == typeof(rdtSerializerButton))
            {
                return new rdtSerializerButton(GUILayout.Button(propName, new GUILayoutOption[0]));
            }
            if (type == typeof(rdtSerializerSlider))
            {
                rdtSerializerSlider slider = (rdtSerializerSlider) propValue;
                obj2 = EditorGUILayout.Slider(propName, slider.Value, slider.LimitMin, slider.LimitMax, new GUILayoutOption[0]);
                return new rdtSerializerSlider((float) obj2, slider.LimitMin, slider.LimitMax);
            }
            rdtDebug.Debug(string.Concat(new object[] { "rdtGuiProperty: Unknown type: ", type.Name, " (name=", propName, ", value=", propValue, ")" }), new object[0]);
            return obj2;
        }

        public static bool HasFoldout(object propValue)
        {
            if (propValue == null)
            {
                return false;
            }
            System.Type type = propValue.GetType();
            if (((type != typeof(Matrix4x4)) && !type.IsArray) && ((type != typeof(Vector4)) && (type != typeof(Quaternion))))
            {
                return (type == typeof(List<rdtTcpMessageComponents.Property>));
            }
            return true;
        }
    }
}

