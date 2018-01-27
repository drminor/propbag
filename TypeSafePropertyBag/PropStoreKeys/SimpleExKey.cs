using System;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;

    using ExKeyType = IExplodedKey<UInt64, UInt64, UInt32>;

    public struct SimpleExKey : ExKeyType, IEquatable<SimpleExKey>
    {
        private readonly int _hashCode;

        private static readonly int LOG_BASE2_MAX_PROPERTIES = 16;

        private static readonly int _maxPropPerObject = (int)Math.Pow(2, LOG_BASE2_MAX_PROPERTIES); //65536;

        static int numBitsForProps = LOG_BASE2_MAX_PROPERTIES;
        static int numberOfBitsInCKey = (int)Math.Log(CompositeKeyType.MaxValue, 2);
        static int numberOfTopBits = numberOfBitsInCKey - numBitsForProps;

        private static long _maxObjectsPerAppDomain = (long)Math.Pow(2, numberOfTopBits);
        private static int _botFieldLen = numBitsForProps;

        private static CompositeKeyType _botMask = ((CompositeKeyType)1 << _botFieldLen) - 1;
        private static CompositeKeyType _topMask = ((CompositeKeyType)1 << numberOfTopBits) - 1;

        #region Constructor

        public SimpleExKey(ObjectIdType level1Key, PropIdType level2Key) : this(StaticFuse(level1Key, level2Key))
        {
        }

        public SimpleExKey(CompositeKeyType cKey) : this()
        {
            System.Diagnostics.Debug.Assert(cKey != 0, "The value 0 is reserved to indicate an empty Key. To create an empty key, use the parameterless constructor.");
            CKey = cKey;
            _hashCode = cKey.GetHashCode();
        }

        #endregion

        #region Public Members

        public CompositeKeyType CKey { get; }

        public bool IsEmpty => CKey == 0;

        public ObjectIdType Level1Key => (CKey >> _botFieldLen) & _topMask;
        public PropIdType Level2Key => (PropIdType)(CKey & _botMask);

        public long MaxObjectsPerAppDomain => _maxObjectsPerAppDomain;
        public int MaxPropsPerObject => _maxPropPerObject;

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
            return _hashCode;
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
