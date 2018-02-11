using System;

namespace DRM.TypeSafePropertyBag
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;
    using PSAccessServiceProviderInterface = IProvidePropStoreAccessService<UInt32, String>;

    public class PropStoreServiceCreatorFactory : IDisposable
    {
        #region Constructor

        public PropStoreServiceCreatorFactory()
        {
        }

        #endregion

        #region Public Methods

        public PSAccessServiceCreatorInterface GetPropStoreEntryPoint(int maxPropsPerObject, IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider)
        {
            PSAccessServiceProviderInterface propStoreAccessServiceProvider = new SimplePropStoreAccessServiceProvider(maxPropsPerObject, handlerDispatchDelegateCacheProvider);

            PSAccessServiceCreatorInterface result = propStoreAccessServiceProvider;
            return result;
        }

        #endregion

        #region IDisposable Support

        // IDisposable is being implemented here so that callers can use the using syntax as in
        // using(x = new PropStoreServiceCreatorFactory()
        //{     PSAccessServiceCreatorInterface propStoreEntryPoint = epCreator.GetPropStoreEntryPoint(maxNumberOfProperties, handlerDispatchDelegateCacheProvider); ...}

        protected virtual void Dispose(bool disposing)
        {
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
