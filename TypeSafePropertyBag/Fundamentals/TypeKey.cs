using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    public struct TypeKey : IEquatable<TypeKey>
    {
        int _hashCode;
        public Type Type;

        public TypeKey(Type type) : this()
        {
            Type = type;
            _hashCode = ComputeHashCode();
        }

        private int ComputeHashCode()
        {
            var hashCode = -782905235;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(Type);
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is TypeKey && Equals((TypeKey)obj);
        }

        public bool Equals(TypeKey other)
        {
            return EqualityComparer<Type>.Default.Equals(Type, other.Type);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public static bool operator ==(TypeKey key1, TypeKey key2)
        {
            return key1.Equals(key2);
        }

        public static bool operator !=(TypeKey key1, TypeKey key2)
        {
            return !(key1 == key2);
        }
    }
}
