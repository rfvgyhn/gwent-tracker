using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace GwentTracker
{
    public static class EnumExtensions
    {
        public static string GetDescription<T>(this T source) where T : Enum
        {
            var field = source.GetType().GetField(source.ToString());
            if (field == null)
                return string.Empty;
            
            var attributes = field.GetCustomAttributes<DescriptionAttribute>(false).ToArray();
            return attributes.Any() && !string.IsNullOrEmpty(attributes[0].Description) ? attributes[0].Description : source.ToString();
        }
    }
}