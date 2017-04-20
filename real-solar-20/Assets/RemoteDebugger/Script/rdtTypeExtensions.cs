namespace Hdg
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public static class rdtTypeExtensions
    {
        private static List<FieldInfo> s_fields = new List<FieldInfo>(0x100);

        public static List<FieldInfo> GetAllFields(this System.Type t)
        {
            s_fields.Clear();
            GetAllFieldsImp(t);
            return s_fields;
        }

        private static void GetAllFieldsImp(System.Type t)
        {
            if (t != null)
            {
                BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                FieldInfo[] fields = t.GetFields(bindingAttr);
                s_fields.AddRange(fields);
                GetAllFieldsImp(t.BaseType);
            }
        }

        public static FieldInfo GetFieldInHierarchy(this System.Type t, string name)
        {
            if (t == null)
            {
                return null;
            }
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            FieldInfo field = t.GetField(name, bindingAttr);
            if (field == null)
            {
                field = GetFieldInHierarchy(t.BaseType, name);
            }
            return field;
        }

        public static bool IsReference(this System.Type type)
        {
            return ((!type.IsValueType && (type != typeof(string))) && !type.IsArray);
        }

        public static bool IsUserStruct(this System.Type type)
        {
            return ((!type.IsPrimitive && !type.IsEnum) && type.IsValueType);
        }
    }
}

