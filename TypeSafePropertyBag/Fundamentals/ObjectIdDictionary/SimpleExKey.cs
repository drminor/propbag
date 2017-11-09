
using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag.Fundamentals.ObjectIdDictionary
{
    public struct SimpleExKey : IExplodedKey<ulong, uint, uint>, IEquatable<SimpleExKey>
    {
        public ulong CKey { get; }
        public uint Level1Key { get; }
        public uint Level2Key { get; }

        public SimpleExKey(ulong cKey, uint level1Key, uint level2Key) : this()
        {
            CKey = cKey;
            Level1Key = level1Key;
            Level2Key = level2Key;
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
            var hashCode = 1252689209;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + CKey.GetHashCode();
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
