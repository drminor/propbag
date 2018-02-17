using System;

namespace DRM.TypeSafePropertyBag
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;
    using PSAccessServiceProviderInterface = IProvidePropStoreAccessService<UInt32, String>;

    /// <summary>
    /// This class is used to create the main PropStore and return a PropStoreService Creator.
    /// It is necessary to use this class since the PSAccessServiceProviderInterface is internal to the TypeSafePropertyBag assembly.
    /// </summary>
    public class SimplePropStoreProxy : IDisposable
    {
        #region Private Members

        private readonly PSAccessServiceProviderInterface _theStore;

        #endregion

        #region Constructor

        public SimplePropStoreProxy(int maxPropsPerObject, IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider)
        {
            _theStore = new SimplePropStoreAccessServiceProvider(maxPropsPerObject, handlerDispatchDelegateCacheProvider);
        }

        // Disallow the use of the parameterless contructor
        private SimplePropStoreProxy()
        {
        }

        #endregion

        #region Public Properties

        public PSAccessServiceCreatorInterface PropStoreAccessServiceFactory
        {
            get
            {
                return _theStore;
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
                    _theStore.Dispose();
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
