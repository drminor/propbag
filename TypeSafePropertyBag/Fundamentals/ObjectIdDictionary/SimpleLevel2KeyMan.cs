using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    using PropIdType = UInt32;
    using PropNameType = String;

    public class SimpleLevel2KeyMan : IL2KeyMan<PropIdType, PropNameType>
    {
        #region Private Members

        Dictionary<PropNameType, PropIdType> _rawDict;
        Dictionary<PropIdType, PropNameType> _cookedDict;

        readonly object _sync;

        #endregion

        #region Constructor

        public SimpleLevel2KeyMan(int maxPropsPerObject)
        {
            _rawDict = new Dictionary<PropNameType, PropIdType>();
            _cookedDict = new Dictionary<PropIdType, PropNameType>();
            MaxPropsPerObject = maxPropsPerObject;

            _sync = new object();
        }

        #endregion

        #region Public Members

        public int MaxPropsPerObject { get; }

        public PropNameType FromCooked(PropIdType bot)
        {
            PropNameType result = _cookedDict.Values.FirstOrDefault();
            return result;
        }

        public bool TryGetFromCooked(PropIdType bot, out PropNameType rawBot)
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

        public PropIdType FromRaw(PropNameType rawBot)
        {
            PropIdType result = _rawDict[rawBot];
            return result;
        }

        public bool TryGetFromRaw(PropNameType rawBot, out PropIdType bot)
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

        public PropIdType Add(PropNameType rawBot)
        {
            PropIdType cookedVal = NextCookedVal;
            _rawDict.Add(rawBot, cookedVal);
            _cookedDict.Add(cookedVal, rawBot);
            return cookedVal;
        }

        public PropIdType GetOrAdd(PropNameType rawBot)
        {
            lock (_sync)
            {
                if (_rawDict.TryGetValue(rawBot, out PropIdType existingCookedVal))
                {
                    return existingCookedVal;
                }
                else
                {
                    PropIdType cookedVal = NextCookedVal;
                    _rawDict.Add(rawBot, cookedVal);
                    _cookedDict.Add(cookedVal, rawBot);
                    return cookedVal;
                }
            }
        }

        #endregion

        #region Private Methods

        private long m_Counter = 0;
        private uint NextCookedVal
        {
            get
            {
                long temp = System.Threading.Interlocked.Increment(ref m_Counter);
                if (temp > MaxPropsPerObject) throw new InvalidOperationException("The SimpleLevel2Key Manager has run out of property ids.");
                return (uint)temp;
            }
        }

        #endregion
    }
}
