using System;
using System.Collections.Concurrent;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    using PropIdType = UInt32;
    using PSAccessServiceInternalInterface = IPropStoreAccessServiceInternal<UInt32, String>;

    internal class ViewManagerCollection : IDisposable
    {
        #region Private Members

        ConcurrentDictionary<PropIdType, IManageCViews> _dict;

        #endregion

        #region Constructor

        public ViewManagerCollection()
        {
            // TODO: Provide Expected Currency Levels.
            _dict = new ConcurrentDictionary<PropIdType, IManageCViews>();
        }

        #endregion

        #region Public Members

        public int Count => _dict.Count;

        public IManageCViews GetOrAdd(PropIdType propId, Func<PropIdType, IManageCViews> vFactory)
        {
            IManageCViews result = _dict.GetOrAdd(propId, vFactory);
            return result;
        }

        public bool TryGetValue(PropIdType propId, out IManageCViews cViewManager)
        {
            if(_dict.TryGetValue(propId, out cViewManager))
            {
                return true;
            }
            else
            {
                cViewManager = null;
                return false;
            }
        }

        public IManageCViews this[PropIdType propId]
        {
            get
            {
                IManageCViews result = _dict[propId];
                return result;
            }
        }

        #endregion

        #region IDisposable Support

        private void ClearCollectionViewManagers()
        {
            foreach(IManageCViews cViewManager in _dict.Values)
            {
                if(cViewManager is IDisposable disable)
                {
                    disable.Dispose();
                }
            }
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    ClearCollectionViewManagers();
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
