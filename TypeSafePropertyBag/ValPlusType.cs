using DRM.TypeSafePropertyBag;
using System;

namespace DRM.TypeSafePropertyBag
{
    public struct ValPlusType : IEquatable<ValPlusType>
    {
        public object Value { get; set; }
        public Type Type { get; set; }

        public ValPlusType(object value, Type type)
        {
            Value = value;
            Type = type;
        }

        public ValPlusType(Tuple<object, Type> tuple)
        {
            Value = tuple.Item1;
            Type = tuple.Item2;
        }

        public Tuple<object, Type> Tuple
        {
            get
            {
                return new Tuple<object, Type>(Value, Type);
            }
        }

        //public bool Equals(ValPlusType other)
        //{
        //    // TODO: Can we do better than the default Type.Equals implementation.
        //    return other.Value.Equals(Value) && other.Type.Equals(Type);
        //}

        //// override object.Equals
        //public override bool Equals(object obj)
        //{
        //    if (obj == null || obj.GetType() != typeof(ValPlusType))
        //    {
        //        return false;
        //    }

        //    return this.Equals((ValPlusType)obj);
        //}

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return GenerateHash.CustomHash(Value.GetHashCode(), Type.GetHashCode());
        }

        public override bool Equals(object other) => other is ValPlusType && Equals((ValPlusType)other);

        /// <summary>
        /// Uses the implementation of Equals provided by the object.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ValPlusType other) => Value.Equals(other.Value) && Type == other.Type; 

        public static bool operator ==(ValPlusType left, ValPlusType right) => left.Equals(right);

        public static bool operator !=(ValPlusType left, ValPlusType right) => !left.Equals(right);

    }
}
