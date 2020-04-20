using System;

namespace Throws.Net
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ThrowsAttribute : Attribute
    {
        public ThrowsAttribute(Type type) => Type = type;

        public Type Type { get; }
    }
}