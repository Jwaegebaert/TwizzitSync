using System;
using System.ComponentModel;
using System.Reflection;

namespace TwizzitSync.Extensions;

internal static class EnumExtensions
{
    /// <summary>
    /// Get the description of an enum value.
    /// </summary>
    /// <param name="value">Enum value.</param>
    /// <returns>Description of the enum value.</returns>
    public static string GetDescription(this Enum value)
    {
        FieldInfo field = value.GetType().GetField(value.ToString());
        DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();

        return attribute == null ? value.ToString() : attribute.Description;
    }

    /// <summary>
    /// Get the enum value from its description.
    /// </summary>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <param name="description">Description of the enum value.</param>
    /// <param name="defaultValue">Default value.</param>
    /// <returns>Enum value.</returns>
    public static T GetEnumValue<T>(string description) where T : struct, Enum
    {
        if (string.IsNullOrEmpty(description))
            return default;

        foreach (var field in typeof(T).GetFields())
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute && attribute.Description == description || field.Name == description)
                return (T)field.GetValue(null);

        return default;
    }
}