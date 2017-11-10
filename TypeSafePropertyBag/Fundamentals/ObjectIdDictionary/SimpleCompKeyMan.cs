using System;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    public class SimpleCompKeyMan : ICKeyMan<SimpleExKey, ulong, uint, uint, string>
    {
        IL2KeyMan<uint, string> Level2KeyMan { get; }
        int _botFieldLen;
        int _shift;
        ulong _botMask;
        ulong _topMask;

        public SimpleCompKeyMan(IL2KeyMan<uint, string> level2KeyMan, int numberOfTopBits)
        {
            Level2KeyMan = level2KeyMan ?? throw new ArgumentNullException($"{nameof(level2KeyMan)}.");

            if (numberOfTopBits > 61 || numberOfTopBits < 2) throw new ArgumentException($"The {nameof(numberOfTopBits)} must be between 2 and 61, inclusive.");
            _shift = numberOfTopBits;
            _botFieldLen = 64 - numberOfTopBits;
            _botMask = ((ulong)1 << _botFieldLen) - 1;
            _topMask = ((ulong)1 << numberOfTopBits) - 1;

        }

        // Join and split exploded key from L1 and L2
        //ExKeyT Join(L1T top, L2T bot);
        public SimpleExKey Join(uint top, uint bot)
        {
            ulong ckey = JoinComp(top, bot);
            SimpleExKey result = new SimpleExKey(ckey, top, bot);
            return result;
        }

        //L1T Split(ExKeyT exKey, out L2T bot);
        public uint Split(SimpleExKey exKey, out uint bot)
        {
            throw new NotImplementedException();
        }

        // Join and split exploded key from L1 and L2Raw.
        //ExKeyT Join(L1T top, L2TRaw bot);
        public SimpleExKey Join(uint top, string rawBot)
        {
            uint bot = Level2KeyMan.FromRaw(rawBot);
            ulong cKey = JoinComp(top, bot);

            SimpleExKey result = new SimpleExKey(cKey, top, bot);
            return result;
        }

        //L1T Split(ExKeyT exKey, out L2TRaw bot);
        public uint Split(SimpleExKey exKey, out string rawBot)
        {
            throw new NotImplementedException();
        }

        // Try version of Join
        //bool TryJoin(L1T top, L2TRaw rawBot, out ExKeyT exKey);
        public bool TryJoin(uint top, string rawBot, out SimpleExKey exKey)
        {
            if(TryJoinComp(top, rawBot, out ulong cKey, out uint bot))
            {
                exKey = new SimpleExKey(cKey, top, bot);
                return true;
            }
            else
            {
                exKey = new SimpleExKey();
                return false;
            }
        }

        // Try version of Join Comp
        public bool TryJoinComp(uint top, string rawBot, out ulong cKey, out uint bot)
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
        public SimpleExKey Split(ulong cKey)
        {
            uint top = SplitComp(cKey, out uint bot);
            return new SimpleExKey(cKey, top, bot);
        }


        // Join and split composite key from L1 and L2.
        //CompT JoinComp(L1T top, L2T bot);
        public ulong JoinComp(uint top, uint bot)
        {
            ulong result = top;
            result = result << _botFieldLen;
            result += bot;
            return result;
        }

        //L1T SplitComp(CompT cKey, out L2T bot);
        public uint SplitComp(ulong cKey, out uint bot)
        {
            bot = (uint)(cKey & _botMask);

            uint result = (uint)((cKey >> _botFieldLen) & _topMask);
            return result;
        }

        // Join and split composite key from L1 and L2Raw.
        //CompT JoinComp(L1T top, L2TRaw rawBot);
        public ulong JoinComp(uint top, string rawBot)
        {
            throw new NotImplementedException();
        }

        //L1T SplitComp(CompT cKey, out L2TRaw rawBot);
        public uint SplitComp(ulong cKey, out string rawBot)
        {
            uint result = SplitComp(_topMask, out uint bot);
            rawBot = Level2KeyMan.FromCooked(bot);

            return result;
        }

    }
}
