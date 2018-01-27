using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    public struct ValPlusType : IEquatable<ValPlusType>
    {
        public readonly bool ValueIsDefined;
        public readonly object Value;
        public readonly Type Type;

        public ValPlusType(bool valueIsDefined, object value, Type type)
        {
            ValueIsDefined = valueIsDefined;
            Value = value;
            Type = type;
        }

        public ValPlusType(object value, Type type)
        {
            ValueIsDefined = value != null || !type.IsValueType;
            Value = value;
            Type = type;
        }

        public ValPlusType(Type type)
        {
            ValueIsDefined = false;
            Value = null;
            Type = type;
        }

        public ValPlusType(Tuple<bool, object, Type> tuple)
        {
            ValueIsDefined = tuple.Item1;
            Value = tuple.Item2;
            Type = tuple.Item3;
        }

        public Tuple<bool, object, Type> Tuple => new Tuple<bool, object, Type>(ValueIsDefined, Value, Type);

        public override bool Equals(object obj)
        {
            return obj is ValPlusType && Equals((ValPlusType)obj);
        }

        public bool Equals(ValPlusType other)
        {
            return ValueIsDefined == other.ValueIsDefined
                    && EqualityComparer<Type>.Default.Equals(Type, other.Type)
                    && EqualityComparer<object>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            var hashCode = -816317690;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + ValueIsDefined.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(Value);
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(Type);
            return hashCode;
        }

        public static bool operator ==(ValPlusType type1, ValPlusType type2)
        {
            return type1.Equals(type2);
        }

        public static bool operator !=(ValPlusType type1, ValPlusType type2)
        {
            return !(type1 == type2);
        }
    }
}
