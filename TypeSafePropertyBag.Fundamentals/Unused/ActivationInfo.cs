using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag.Fundamentals.Unused
{
    public class ActivationInfo
    {
        public ActivationInfo(Type type, IList<Argument> arguments)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        }

        public Type Type { get; }
        public IList<Argument> Arguments { get; }
    }

    public class Argument
    {
        public Argument(Type type, int position) : this(type, position, null)
        {
        }

        public Argument(Type type, string name) : this(type, name, null)
        {
        }

        public Argument(Type type, int position, object value)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Position = position;
            Name = null;
            Value = value;
        }

        public Argument(Type type, string name, object value)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Position = -1;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value;
        }

        public Type Type { get; }
        public int Position { get; }
        public string Name { get; }
        public object Value { get; }
    }
}
