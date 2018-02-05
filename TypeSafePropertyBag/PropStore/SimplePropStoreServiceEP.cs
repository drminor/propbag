using System;

namespace DRM.TypeSafePropertyBag
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;
    using PSAccessServiceProviderInterface = IProvidePropStoreAccessService<UInt32, String>;
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;

    public class SimplePropStoreServiceEP : PSAccessServiceCreatorInterface, IDisposable
    {
        #region Private Members

        PSAccessServiceProviderInterface _propStoreAccessServiceProvider;

        #endregion

        #region Public Properties

        public int MaxPropsPerObject => _propStoreAccessServiceProvider.MaxPropsPerObject;
        public long MaxObjectsPerAppDomain => _propStoreAccessServiceProvider.MaxObjectsPerAppDomain;

        #endregion

        #region Constructor

        public SimplePropStoreServiceEP(int maxPropsPerObject, IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider)
        {
            _propStoreAccessServiceProvider = new SimplePropStoreAccessServiceProvider(maxPropsPerObject, handlerDispatchDelegateCacheProvider);
        }

        #endregion

        #region PropStoreAccessService Creation

        public PSAccessServiceInterface CreatePropStoreService(IPropBag propBag)
        {
            PSAccessServiceInterface result = _propStoreAccessServiceProvider.CreatePropStoreService(propBag);
            return result;
        }

        public object FixPropItemSet(IPropBag propBag)
        {
            object propItemSet_Handle = _propStoreAccessServiceProvider.FixPropItemSet(propBag);
            return propItemSet_Handle;
        }

        public bool TryOpenPropItemSet(IPropBag propBag, out object propItemSet_Handle)
        {
            bool result = _propStoreAccessServiceProvider.TryOpenPropItemSet(propBag, out propItemSet_Handle);
            return result;
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
                    if(_propStoreAccessServiceProvider != null)
                    {
                        _propStoreAccessServiceProvider.Dispose();
                    }
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

        #region Diagnostics

        public int AccessCounter => _propStoreAccessServiceProvider.AccessCounter;


        public void IncAccess()
        {
            _propStoreAccessServiceProvider.IncAccess();
        }

        public void ResetAccessCounter()
        {
            _propStoreAccessServiceProvider.ResetAccessCounter();
        }

        public int TotalNumberOfAccessServicesCreated => _propStoreAccessServiceProvider.TotalNumberOfAccessServicesCreated;

        public int NumberOfRootPropBagsInPlay => _propStoreAccessServiceProvider.NumberOfRootPropBagsInPlay;

        #endregion
    }
}
