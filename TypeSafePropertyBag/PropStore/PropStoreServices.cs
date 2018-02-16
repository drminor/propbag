using System;

namespace DRM.TypeSafePropertyBag
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;
    using PSServiceSingletonProviderInterface = IProvidePropStoreServiceSingletons<UInt32, String>;

    public class PropStoreServices_NotUsed : PSServiceSingletonProviderInterface, IDisposable
    {
        public PropStoreServices_NotUsed(ITypeDescBasedTConverterCache typeDescBasedTConverterCache, IProvideDelegateCaches delegateCacheProvider, IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider/*, PSAccessServiceCreatorInterface propStoreEntryPoint*/)
        {
            TypeDescBasedTConverterCache = typeDescBasedTConverterCache ?? throw new ArgumentNullException(nameof(typeDescBasedTConverterCache));
            DelegateCacheProvider = delegateCacheProvider ?? throw new ArgumentNullException(nameof(delegateCacheProvider));
            HandlerDispatchDelegateCacheProvider = handlerDispatchDelegateCacheProvider ?? throw new ArgumentNullException(nameof(handlerDispatchDelegateCacheProvider));
            //PropStoreEntryPoint = propStoreEntryPoint ?? throw new ArgumentNullException(nameof(propStoreEntryPoint));
        }

        public ITypeDescBasedTConverterCache TypeDescBasedTConverterCache { get; }
        public IProvideDelegateCaches DelegateCacheProvider { get; }
        public IProvideHandlerDispatchDelegateCaches HandlerDispatchDelegateCacheProvider { get; }
        //public PSAccessServiceCreatorInterface PropStoreEntryPoint { get; }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    if(TypeDescBasedTConverterCache is IDisposable disable)
                    {
                        disable.Dispose();
                    }

                    if(DelegateCacheProvider is IDisposable disable2)
                    {
                        disable2.Dispose();
                    }

                    if(HandlerDispatchDelegateCacheProvider is IDisposable disable3)
                    {
                        disable3.Dispose();
                    }

                    //if(PropStoreEntryPoint is IDisposable disable4)
                    //{
                    //    disable4.Dispose();
                    //}
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
