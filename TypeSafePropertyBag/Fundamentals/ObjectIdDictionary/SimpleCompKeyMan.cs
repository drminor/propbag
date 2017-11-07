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

        public ulong Join(uint top, string rawBot)
        {
            ulong result = Join(top, Level2KeyMan.FromRaw(rawBot));
            return result;
        }

        public bool TryJoin(uint top, string rawBot, out ulong comp)
        {
            if (Level2KeyMan.TryGetFromRaw(rawBot, out uint bot))
            {
                comp = Join(top, Level2KeyMan.FromRaw(rawBot));
                return true;
            }
            else
            {
                comp = 0;
                return false;
            }
        }


        public uint Split(ulong comp, out uint bot)
        {
            bot = (uint)(comp & _botMask);

            uint result = (uint)((comp >> _botFieldLen) & _topMask);
            return result;
        }

        public uint Split(ulong comp, out string rawBot)
        {
            uint result = Split(_topMask, out uint bot);
            rawBot = Level2KeyMan.FromCooked(bot);

            return result;
        }
    }
}
