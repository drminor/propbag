
using System;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt32;
    using PropIdType = UInt32;

    using ObjectRefType = WeakReference<IPropBag>;

    public struct SimpleExKey : IExplodedKey<CompositeKeyType, ObjectIdType, PropIdType>, IEquatable<SimpleExKey>
    {
        public CompositeKeyType CKey { get; }
        public ObjectIdType Level1Key { get; }
        public PropIdType Level2Key { get; }

        public ObjectRefType _wrToPropBag { get; }

        public SimpleExKey(CompositeKeyType cKey, ObjectRefType accessToken, ObjectIdType level1Key, PropIdType level2Key) : this()
        {
            CKey = cKey;
            Level1Key = level1Key;
            Level2Key = level2Key;
            _wrToPropBag = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        }

        public override bool Equals(object obj)
        {
            return obj is SimpleExKey && Equals((SimpleExKey)obj);
        }

        public bool Equals(SimpleExKey other)
        {
            return CKey == other.CKey;
        }

        public override int GetHashCode()
        {
            //var hashCode = 1252689209;
            //hashCode = hashCode * -1521134295 + base.GetHashCode();
            //hashCode = hashCode * -1521134295 + CKey.GetHashCode();

            var hashCode = CKey.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"CompKey: O:{Level1Key} -- P:{Level2Key}";
        }

        public static bool operator ==(SimpleExKey key1, SimpleExKey key2)
        {
            return key1.Equals(key2);
        }

        public static bool operator !=(SimpleExKey key1, SimpleExKey key2)
        {
            return !(key1 == key2);
        }
    }
}
