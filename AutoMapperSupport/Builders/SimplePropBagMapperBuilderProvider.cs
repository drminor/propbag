using DRM.PropBag.TypeWrapper;
using DRM.PropBag.TypeWrapper.TypeDesc;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class SimplePropBagMapperBuilderProvider : IPropBagMapperBuilderProvider
    {
        private ICreateWrapperTypes WrapperTypeCreator { get; }
        private IViewModelActivator ViewModelActivator { get; }

        private PSAccessServiceCreatorInterface _storeAccessCreator;

        public SimplePropBagMapperBuilderProvider
            (
            ICreateWrapperTypes wrapperTypesCreator,
            IViewModelActivator viewModelActivator, 
            PSAccessServiceCreatorInterface storeAccessCreator
            )
        {
            WrapperTypeCreator = wrapperTypesCreator ?? GetSimpleWrapperTypeCreator();
            ViewModelActivator = viewModelActivator ?? throw new ArgumentNullException(nameof(viewModelActivator)); // new SimpleViewModelActivator();
            _storeAccessCreator = storeAccessCreator ?? throw new ArgumentNullException(nameof(storeAccessCreator));
        }

        public IBuildPropBagMapper<TSource, TDestination> GetPropBagMapperBuilder<TSource, TDestination>
            (
            IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder,
            IProvideAutoMappers autoMapperService
            )
            where TDestination: class, IPropBag
        {
            IBuildPropBagMapper<TSource, TDestination> result
                = new SimplePropBagMapperBuilder<TSource, TDestination>
                (
                    mapperConfigurationBuilder: mapperConfigurationBuilder,
                    wrapperTypeCreator: WrapperTypeCreator,
                    viewModelActivator: ViewModelActivator,
                    storeAccessCreator: _storeAccessCreator,
                    autoMapperService: autoMapperService
                );

            return result;
        }

        private ICreateWrapperTypes GetSimpleWrapperTypeCreator()
        {
            // -- Build WrapperType Caching Service
            // Used by some ViewModel Activators to emit types, i.e., modules.
            IModuleBuilderInfo moduleBuilderInfo = new SimpleModuleBuilderInfo();

            IEmitWrapperType emitWrapperType = new SimpleWrapperTypeEmitter(mbInfo: moduleBuilderInfo);

            ICacheWrapperTypes wrapperTypeCachingService = new WrapperTypeLocalCache
                (
                emitterEngine: emitWrapperType
                );

            // -- Build TypeDesc Caching Service
            // Used only by some ModuleBuilders.
            ITypeDescriptionProvider typeDescriptionProvider = new SimpleTypeDescriptionProvider();

            ICacheTypeDescriptions typeDescCachingService = new TypeDescriptionLocalCache
                (
                typeDescriptionProvider: typeDescriptionProvider
                );

            ICreateWrapperTypes result = new SimpleWrapperTypeCreator
                (
                wrapperTypeCachingService: wrapperTypeCachingService,
                typeDescCachingService: typeDescCachingService
                );

            return result;
        }

        // TODO: Note: The WrapperTypeCreator hold two caches, the result provide is only from the cache of emitted types.
        // The number of entries in the cache of TypeDescriptors is not included.
        public long ClearTypeCache()
        {
            return WrapperTypeCreator.ClearTypeCache();
        }
    }
}
