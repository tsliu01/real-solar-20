namespace Hdg
{
    using System;
    using UnityEditor;

    public class rdtGuiArrayField
    {
        public static Array Draw(string label, Array arr, ref bool foldout)
        {
            Array destinationArray = (Array) arr.Clone();
            EditorGUILayout.BeginVertical();
            foldout = rdtGuiFoldout.Draw(foldout, label, true);
            if (foldout)
            {
                EditorGUI.indentLevel += 2;
                int length = arr.Length;
                int num2 = EditorGUILayout.IntField("Size", length);
                if (length != num2)
                {
                    destinationArray = Array.CreateInstance(destinationArray.GetType().GetElementType(), num2);
                    Array.Copy(arr, destinationArray, Math.Min(num2, length));
                }
                for (int i = 0; i < num2; i++)
                {
                    bool flag = false;
                    object obj2 = rdtGuiProperty.Draw("Element " + i, destinationArray.GetValue(i), ref flag);
                    destinationArray.SetValue(obj2, i);
                }
                EditorGUI.indentLevel -= 2;
            }
            EditorGUILayout.EndVertical();
            return destinationArray;
        }
    }
}

