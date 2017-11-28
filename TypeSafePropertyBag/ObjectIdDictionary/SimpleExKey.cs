﻿using System;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;

    using ExKeyType = IExplodedKey<UInt64, UInt64, UInt32>;

    public struct SimpleExKey : ExKeyType, IEquatable<SimpleExKey>
    {
        private const int LOG_BASE2_MAX_PROPERTIES = 16;

        private static readonly int _maxPropPerObject = (int)Math.Pow(2, LOG_BASE2_MAX_PROPERTIES); //65536;

        private static long _maxObjectsPerAppDomain;
        private static int _botFieldLen;
        private static int _shift;
        private static CompositeKeyType _botMask;
        private static CompositeKeyType _topMask;

        static SimpleExKey()
        {
            int numBitsForProps = LOG_BASE2_MAX_PROPERTIES;
            int numberOfBitsInCKey = (int)Math.Log(CompositeKeyType.MaxValue, 2);
            int numberOfTopBits = numberOfBitsInCKey - numBitsForProps;

            _maxObjectsPerAppDomain = (long)Math.Pow(2, numberOfTopBits);

            _shift = numberOfTopBits;
            _botFieldLen = numBitsForProps; // numberOfBitsInCKey - numberOfTopBits;
            _botMask = ((CompositeKeyType)1 << _botFieldLen) - 1;
            _topMask = ((CompositeKeyType)1 << numberOfTopBits) - 1;
        }

        #region Constructor

        //public SimpleExKey(CompositeKeyType cKey/*, WeakReference<IPropBagInternal> accessToken, ObjectIdType level1Key, PropIdType level2Key*/) : this()
        //{
        //    CKey = cKey;
        //    Level1Key = level1Key;
        //    Level2Key = level2Key;
        //    //WR_AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        //}


        public SimpleExKey(ObjectIdType level1Key, PropIdType level2Key) : this(StaticFuse(level1Key, level2Key))
        {
        }

        public SimpleExKey(CompositeKeyType cKey) : this()
        {
            System.Diagnostics.Debug.Assert(cKey != 0, "The value 0 is reserved to indicate an empty Key. To create an empty key, use the parameterless constructor.");
            CKey = cKey;
        }

        #endregion

        #region Public Members

        public bool isEmpty => CKey == 0;
        public CompositeKeyType CKey { get; }
        public ObjectIdType Level1Key => (CKey >> _botFieldLen) & _topMask;
        public PropIdType Level2Key => (PropIdType)(CKey & _botMask);

        public long MaxObjectsPerAppDomain => _maxObjectsPerAppDomain;
        public int MaxPropsPerObject => _maxPropPerObject;

        //public object AccessToken => WR_AccessToken;
        //public WeakReference<IPropBagInternal> WR_AccessToken { get; }

        #endregion

        #region Public Methods

        public CompositeKeyType Fuse(ObjectIdType top, PropIdType bot)
        {
            CompositeKeyType result = top;
            result = result << _botFieldLen;
            result += bot;
            return result;
        }

        public ObjectIdType Explode(CompositeKeyType cKey, out PropIdType bot)
        {
            bot = (PropIdType)(cKey & _botMask);

            ObjectIdType result = (cKey >> _botFieldLen) & _topMask;
            return result;
        }

        public bool Verify(CompositeKeyType cKey, ObjectIdType top)
        {
            ObjectIdType testTop = (cKey >> _botFieldLen) & _topMask;
            return testTop == top;
        }

        public bool Verify(CompositeKeyType cKey, ObjectIdType top, PropIdType bot)
        {
            ObjectIdType testTop = Explode(cKey, out PropIdType testBot);
            return testTop == top && testBot == bot;
        }

        #endregion

        private static CompositeKeyType StaticFuse(ObjectIdType top, PropIdType bot)
        {
            CompositeKeyType result = top;
            result = result << _botFieldLen;
            result += bot;
            return result;
        }

        #region IEquatable Support and Object Overrides

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
            var hashCode = CKey.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"CompKey: O:{Level1Key} -- P:{Level2Key}";
        }

        public bool Equals(ExKeyType other)
        {
            return CKey == other.CKey;
        }

        public static bool operator ==(SimpleExKey key1, SimpleExKey key2)
        {
            return key1.Equals(key2);
        }

        public static bool operator !=(SimpleExKey key1, SimpleExKey key2)
        {
            return !(key1 == key2);
        }

        #endregion
    }
}
