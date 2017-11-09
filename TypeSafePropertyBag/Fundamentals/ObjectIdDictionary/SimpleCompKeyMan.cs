using System;

namespace DRM.TypeSafePropertyBag.Fundamentals.ObjectIdDictionary
{
    public class SimpleCompKeyMan : ICKeyMan<ulong, uint, uint, string>
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

        public ulong Join(uint top, uint bot)
        {
            ulong result = top;
            result = result << _botFieldLen;
            result += bot;
            return result;
        }

        public IExplodedKey<ulong, uint, uint> Join(uint top, string rawBot)
        {
            uint bot = Level2KeyMan.FromRaw(rawBot);
            ulong cKey = Join(top, bot);

            IExplodedKey<ulong, uint, uint> result = new SimpleExKey(cKey, top, bot);

            return result;
        }

        public bool TryJoin(uint top, string rawBot, out ulong cKey)
        {
            if (Level2KeyMan.TryGetFromRaw(rawBot, out uint bot))
            {
                cKey = Join(top, Level2KeyMan.FromRaw(rawBot));
                return true;
            }
            else
            {
                cKey = 0;
                return false;
            }
        }

        public IExplodedKey<ulong, uint, uint> Split(ulong cKey)
        {
            uint top = Split(cKey, out uint bot);
            return new SimpleExKey(cKey, top, bot);
        }

        public uint Split(ulong cKey, out uint bot)
        {
            bot = (uint)(cKey & _botMask);

            uint result = (uint)((cKey >> _botFieldLen) & _topMask);
            return result;
        }

        public uint Split(ulong cKey, out string rawBot)
        {
            uint result = Split(_topMask, out uint bot);
            rawBot = Level2KeyMan.FromCooked(bot);

            return result;
        }
    }
}
