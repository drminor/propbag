using System;
using System.Collections.Concurrent;

namespace DRM.TypeSafePropertyBag
{
    internal class ViewManagerProviderCollection : IDisposable
    {
        #region Private Members

        ConcurrentDictionary<IViewManagerProviderKey, IProvideACViewManager> _dict;

        #endregion

        #region Constructor

        public ViewManagerProviderCollection()
        {
            // TODO: Provide Expected Currency Levels.
            _dict = new ConcurrentDictionary<IViewManagerProviderKey, IProvideACViewManager>();
        }

        #endregion

        #region Public Members

        public int Count => _dict.Count;

        public IProvideACViewManager GetOrAdd(IViewManagerProviderKey viewManagerProviderKey, Func<IViewManagerProviderKey, IProvideACViewManager> vFactory)
        {
            IProvideACViewManager result = _dict.GetOrAdd(viewManagerProviderKey, vFactory);
            return result;
        }

        public bool TryGetValue(IViewManagerProviderKey viewManagerProviderKey, out IProvideACViewManager cViewManagerProvider)
        {
            if(_dict.TryGetValue(viewManagerProviderKey, out cViewManagerProvider))
            {
                return true;
            }
            else
            {
                cViewManagerProvider = null;
                return false;
            }
        }

        public IProvideACViewManager this[IViewManagerProviderKey viewManagerProviderKey]
        {
            get
            {
                IProvideACViewManager result = _dict[viewManagerProviderKey];
                return result;
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
                    // Dispose managed state (managed objects).
                    foreach(IProvideACViewManager cViewManagerProvider in _dict.Values)
                    {
                        cViewManagerProvider.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.

                // Set large fields to null.
                _dict.Clear();
                _dict = null;

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
