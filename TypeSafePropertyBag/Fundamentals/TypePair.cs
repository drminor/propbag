using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    public struct TypePair : IEquatable<TypePair>
    {
        private int _hashCode;
        public readonly Type SourceType;
        public readonly Type DestinationType;

        public readonly Type[] TypeArguments;

        public TypePair(Type sourceType, Type destinationType) : this()
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            TypeArguments = new Type[] { sourceType, destinationType };

            _hashCode = ComputeHashCode();
        }

        public static TypePair Create<TSource>(TSource source, Type sourceType, Type destinationType)
        {
            if (source != null)
            {
                sourceType = source.GetType();
            }
            return new TypePair(sourceType, destinationType);
        }

        public static TypePair Create<TSource, TDestination>(TSource source, TDestination destination, Type sourceType, Type destinationType)
        {
            if (source != null)
            {
                sourceType = source.GetType();
            }
            if (destination != null)
            {
                destinationType = destination.GetType();
            }
            return new TypePair(sourceType, destinationType);
        }

        private int ComputeHashCode()
        {
            var hashCode = -502369705;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(SourceType);
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(DestinationType);
            return hashCode;
        }

        public bool Equals(TypePair other) => SourceType == other.SourceType && DestinationType == other.DestinationType;

        public override bool Equals(object other) => other is TypePair && Equals((TypePair)other);

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public static bool operator ==(TypePair left, TypePair right) => left.Equals(right);
        public static bool operator !=(TypePair left, TypePair right) => !left.Equals(right);
    }
}
