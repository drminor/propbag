using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBagControlsWPF;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBagWPF
{
    using PSServiceSingletonProviderInterface = IProvidePropStoreServiceSingletons<UInt32, String>;

    public class WPFPropFactoryProvider : IProvideAPropFactoryCreator
    {
        IProvideDelegateCaches _delegateCacheProvider;
        ITypeDescBasedTConverterCache _typeDescBasedTConverterCache;

        IProvideAutoMappers _autoMapperProvider;

        public WPFPropFactoryProvider(PSServiceSingletonProviderInterface propStoreServiceSingletonsProvider, IProvideAutoMappers autoMapperProvider)
        {
            _delegateCacheProvider = propStoreServiceSingletonsProvider.DelegateCacheProvider;
            _typeDescBasedTConverterCache = propStoreServiceSingletonsProvider.TypeDescBasedTConverterCache;

            _autoMapperProvider = autoMapperProvider;
        }

        public IPropFactory GetNewPropFactory()
        {
            IProvideDelegateCaches delegateCacheProvider = _delegateCacheProvider;

            ITypeDescBasedTConverterCache typeDescBasedTConverter = _typeDescBasedTConverterCache;
            IConvertValues propFactoryValueConverter = new PropFactoryValueConverter(typeDescBasedTConverter);

            ResolveTypeDelegate typeResolver = null;

            IPropFactory result = new WPFPropFactory
                (
                delegateCacheProvider: delegateCacheProvider,
                valueConverter: propFactoryValueConverter,
                typeResolver: typeResolver,
                autoMapperProvider: _autoMapperProvider
                );

            return result;
        }

    }
}
