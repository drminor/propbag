using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    public class SimpleLevel2KeyMan : IL2KeyMan<uint, string>
    {
        Dictionary<string, uint> _rawDict;
        Dictionary<uint, string> _cookedDict;

        public SimpleLevel2KeyMan()
        {
            _rawDict = new Dictionary<string, uint>();
            _cookedDict = new Dictionary<uint, string>();
        }

        public string FromCooked(uint bot)
        {
            string result = _cookedDict.Values.FirstOrDefault();
            return result;
        }

        public bool TryGetFromCooked(uint bot, out string rawBot)
        {
            if (_cookedDict.TryGetValue(bot, out rawBot))
            {
                return true;
            }
            else
            {
                rawBot = null;
                return false;
            }
        }

        public uint FromRaw(string rawBot)
        {
            uint result = _rawDict[rawBot];
            return result;
        }

        public bool TryGetFromRaw(string rawBot, out uint bot)
        {
            if(_rawDict.TryGetValue(rawBot, out bot))
            {
                return true;
            }
            else
            {
                bot = 0;
                return false;
            }
        }

        public uint Add(string rawBot)
        {
            uint cookedVal = NextCookedVal;
            _rawDict.Add(rawBot, cookedVal);
            _cookedDict.Add(cookedVal, rawBot);
            return cookedVal;
        }

        private long m_Counter = 0;
        public uint NextCookedVal
        {
            get
            {
                long temp = System.Threading.Interlocked.Increment(ref m_Counter);
                if (temp > uint.MaxValue) throw new InvalidOperationException("The Level2 Key Manager has run out of Ids.");
                return (uint)temp;
            }
        }
    }
}
