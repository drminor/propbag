using System;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt32;
    using PropIdType = UInt32;
    using PropNameType = String;

    // TODO: Since the maximum number of properties per object may be less than 2^^32, then the 
    // maximum number of objects may be more than 2 ^^ 32 -- which takes a Int64, not a UInt32

    public class SimpleCompKeyMan : ICKeyMan<SimpleExKey, CompositeKeyType, ObjectIdType, PropIdType, PropNameType>
    {
        private SimpleLevel2KeyMan Level2KeyMan { get; }
        public int MaxPropsPerObject { get; }

        int _botFieldLen;
        int _shift;
        CompositeKeyType _botMask;
        CompositeKeyType _topMask;

        public SimpleCompKeyMan(SimpleLevel2KeyMan level2KeyMan)
        {
            Level2KeyMan = level2KeyMan ?? throw new ArgumentNullException($"{nameof(level2KeyMan)}.");
            MaxPropsPerObject = level2KeyMan.MaxPropsPerObject;

            double numBitsForProps = Math.Log(MaxPropsPerObject, 2);

            if( (int)numBitsForProps - numBitsForProps > 0.5)
            {
                throw new ArgumentException("The maxPropsPerObject must be an even power of two. For example: 256, 512, 1024, etc.");
            }

            int numberOfBitsInCKey = (int)Math.Log(CompositeKeyType.MaxValue, 2);

            double topRange =  numberOfBitsInCKey - 2; // Must leave room for at least 4 objects.

            if (4 > numBitsForProps || numBitsForProps > topRange)
            {
                throw new ArgumentException($"maxPropsPerObject must be between 4 and {topRange}, inclusive.", nameof(level2KeyMan.MaxPropsPerObject));
            }

            int numberOfTopBits = (int)Math.Round((double)numberOfBitsInCKey - numBitsForProps, 0);
            MaxObjectsPerAppDomain = (long) Math.Pow(2, numberOfTopBits);

            _shift = numberOfTopBits;
            _botFieldLen = numberOfBitsInCKey - numberOfTopBits;
            _botMask = ((CompositeKeyType)1 << _botFieldLen) - 1;
            _topMask = ((CompositeKeyType)1 << numberOfTopBits) - 1;
        }

        public long MaxObjectsPerAppDomain { get; }

        // Join and split exploded key from L1 and L2
        //ExKeyT Join(L1T top, L2T bot);
        public SimpleExKey Join(ObjectIdType top, PropIdType bot)
        {
            CompositeKeyType cKey = JoinComp(top, bot);
            SimpleExKey result = new SimpleExKey(cKey, null, top, bot);
            return result;
        }

        //L1T Split(ExKeyT exKey, out L2T bot);
        public ObjectIdType Split(SimpleExKey exKey, out PropIdType bot)
        {
            throw new NotImplementedException();
        }

        // Join and split exploded key from L1 and L2Raw.
        //ExKeyT Join(L1T top, L2TRaw bot);
        public SimpleExKey Join(ObjectIdType top, PropNameType rawBot)
        {
            PropIdType bot = Level2KeyMan.FromRaw(rawBot);
            CompositeKeyType cKey = JoinComp(top, bot);

            SimpleExKey result = new SimpleExKey(cKey, null, top, bot);
            return result;
        }

        //L1T Split(ExKeyT exKey, out L2TRaw bot);
        public ObjectIdType Split(SimpleExKey exKey, out PropNameType rawBot)
        {
            throw new NotImplementedException();
        }

        // Try version of Join
        //bool TryJoin(L1T top, L2TRaw rawBot, out ExKeyT exKey);
        public bool TryJoin(ObjectIdType top, PropNameType rawBot, out SimpleExKey exKey)
        {
            if(TryJoinComp(top, rawBot, out CompositeKeyType cKey, out PropIdType bot))
            {
                exKey = new SimpleExKey(cKey, null, top, bot);
                return true;
            }
            else
            {
                exKey = new SimpleExKey();
                return false;
            }
        }

        // Try version of Join Comp
        public bool TryJoinComp(ObjectIdType top, PropNameType rawBot, out CompositeKeyType cKey, out PropIdType bot)
        {
            if (Level2KeyMan.TryGetFromRaw(rawBot, out bot))
            {
                cKey = JoinComp(top, Level2KeyMan.FromRaw(rawBot));
                return true;
            }
            else
            {
                cKey = 0;
                return false;
            }
        }

        // Create exploded key from composite key.
        //ExKeyT Split(CompT cKey);
        public SimpleExKey Split(CompositeKeyType cKey)
        {
            uint top = SplitComp(cKey, out PropIdType bot);
            return new SimpleExKey(cKey, null, top, bot);
        }


        // Join and split composite key from L1 and L2.
        //CompT JoinComp(L1T top, L2T bot);
        public CompositeKeyType JoinComp(ObjectIdType top, PropIdType bot)
        {
            CompositeKeyType result = top;
            result = result << _botFieldLen;
            result += bot;
            return result;
        }

        //L1T SplitComp(CompT cKey, out L2T bot);
        public ObjectIdType SplitComp(CompositeKeyType cKey, out PropIdType bot)
        {
            bot = (PropIdType)(cKey & _botMask);

            ObjectIdType result = (ObjectIdType)((cKey >> _botFieldLen) & _topMask);
            return result;
        }

        // Join and split composite key from L1 and L2Raw.
        //CompT JoinComp(L1T top, L2TRaw rawBot);
        public CompositeKeyType JoinComp(ObjectIdType top, PropNameType rawBot)
        {
            throw new NotImplementedException();
        }

        //L1T SplitComp(CompT cKey, out L2TRaw rawBot);
        public ObjectIdType SplitComp(CompositeKeyType cKey, out PropNameType rawBot)
        {
            ObjectIdType result = SplitComp(_topMask, out PropIdType bot);
            rawBot = Level2KeyMan.FromCooked(bot);

            return result;
        }

    }
}
