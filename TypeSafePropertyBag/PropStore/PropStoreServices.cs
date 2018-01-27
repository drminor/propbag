using System;

namespace DRM.TypeSafePropertyBag
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;
    using PSServiceSingletonProviderInterface = IProvidePropStoreServiceSingletons<UInt32, String>;

    public class PropStoreServices : PSServiceSingletonProviderInterface
    {
        public PropStoreServices(ITypeDescBasedTConverterCache typeDescBasedTConverterCache, IProvideDelegateCaches delegateCacheProvider, IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider, PSAccessServiceCreatorInterface propStoreEntryPoint)
        {
            TypeDescBasedTConverterCache = typeDescBasedTConverterCache ?? throw new ArgumentNullException(nameof(typeDescBasedTConverterCache));
            DelegateCacheProvider = delegateCacheProvider ?? throw new ArgumentNullException(nameof(delegateCacheProvider));
            HandlerDispatchDelegateCacheProvider = handlerDispatchDelegateCacheProvider ?? throw new ArgumentNullException(nameof(handlerDispatchDelegateCacheProvider));
            PropStoreEntryPoint = propStoreEntryPoint ?? throw new ArgumentNullException(nameof(propStoreEntryPoint));
        }

        public ITypeDescBasedTConverterCache TypeDescBasedTConverterCache { get; }
        public IProvideDelegateCaches DelegateCacheProvider { get; }
        public IProvideHandlerDispatchDelegateCaches HandlerDispatchDelegateCacheProvider { get; }
        public PSAccessServiceCreatorInterface PropStoreEntryPoint { get; }
    }
}
