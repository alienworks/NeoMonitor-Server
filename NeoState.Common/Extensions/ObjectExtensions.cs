using System;
using System.Reflection;

namespace NeoState.Common
{
    /// <summary>
    /// Is this necessary?
    /// </summary>
    public static class ObjectExtensions
    {
        public static T GetFieldValue<T>(this object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = instance.GetType().GetField(fieldName, bindFlags);
            return (T)field.GetValue(instance);
        }

        public static T GetInstanceField<T>(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return (T)field.GetValue(instance);
        }
    }
}