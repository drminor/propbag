using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;
    using PropNameType = String;

    internal class SimpleLevel2KeyMan : IL2KeyMan<PropIdType, PropNameType>
    {
        #region Private Members

        internal Dictionary<PropNameType, PropIdType> _rawDict;
        internal Dictionary<PropIdType, PropNameType> _cookedDict;

        readonly object _sync;

        #endregion

        #region Constructor

        public SimpleLevel2KeyMan(int maxPropsPerObject)
        {
            MaxPropsPerObject = maxPropsPerObject;

            _sync = new object();
            _rawDict = new Dictionary<PropNameType, PropIdType>();
            _cookedDict = new Dictionary<PropIdType, PropNameType>();
        }

        //public SimpleLevel2KeyMan(IL2KeyMan<PropIdType, PropNameType> sourceToCopy)
        //{
        //    MaxPropsPerObject = sourceToCopy.MaxPropsPerObject;

        //    _sync = new object();

        //    _rawDict = new Dictionary<PropNameType, PropIdType>(sourceToCopy._rawDict);
        //    _cookedDict = new Dictionary<PropIdType, PropNameType>(sourceToCopy._cookedDict);
        //}

        internal SimpleLevel2KeyMan(int maxPropsPerObject, Dictionary<PropNameType, PropIdType> rawDict, Dictionary<PropIdType, PropNameType> cookedDict)
        {
            MaxPropsPerObject = maxPropsPerObject;

            _sync = new object();

            _rawDict = rawDict;
            _cookedDict = cookedDict;
        }

        #endregion

        public object Clone()
        {
            SimpleLevel2KeyMan result = new SimpleLevel2KeyMan(this.MaxPropsPerObject, this._rawDict, this._cookedDict);
            return result;
        }

        #region Public Members

        public int MaxPropsPerObject { get; }

        public PropNameType FromCooked(PropIdType bot)
        {
            PropNameType result = _cookedDict[bot];
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

        public int PropertyCount => _rawDict.Count;

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

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _rawDict.Clear();
                    _cookedDict.Clear();
                    _rawDict = null;
                    _cookedDict = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Temp() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
