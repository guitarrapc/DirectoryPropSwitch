using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DirectoryPropSwitch.internals
{
    internal enum EolStyle
    {
        [Label("")]
        Unknown = 0,
        [Label("\r")]
        CR = 1,
        [Label("\n")]
        LF = 2,
        [Label("\r\n")]
        CRLF = CR | LF,
        [Label("\n")]
        Unix = LF,
        [Label("\r")]
        MacOs = CR,
        [Label("\r\n")]
        Windows = CRLF,
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class LabelAttribute : Attribute
    {
        public string Value { get; private set; }

        public LabelAttribute(string value)
        {
            this.Value = value;
        }
    }

    internal static class EndOfLineStyleExtentions
    {
        public static string GetLabel(this Enum value)
        {
            return value.GetAttribute<LabelAttribute>()?.Value ?? value.ToString();
        }
    }

    internal static class EnumAttributeExtensionCore
    {
        private static class EnumAttributeCache<TAttribute> where TAttribute : Attribute
        {
            private static ConcurrentDictionary<Enum, TAttribute> body = new ConcurrentDictionary<Enum, TAttribute>();
            internal static TAttribute GetOrAdd(Enum enumKey, Func<Enum, TAttribute> valueFactory)
                => body.GetOrAdd(enumKey, valueFactory);
        }

        public static TAttribute GetAttribute<TAttribute>(this Enum enumKey) where TAttribute : Attribute 
            => EnumAttributeCache<TAttribute>.GetOrAdd(enumKey, _ => enumKey.GetAttributeCore<TAttribute>());

        private static TAttribute GetAttributeCore<TAttribute>(this Enum enumKey) where TAttribute : Attribute
        {
            var fieldInfo = enumKey.GetType().GetField(enumKey.ToString());
            var attributes = fieldInfo.GetCustomAttributes(typeof(TAttribute), false).Cast<TAttribute>();
            if (!attributes.Any())
                return null;
            return attributes.First();
        }
    }
}
