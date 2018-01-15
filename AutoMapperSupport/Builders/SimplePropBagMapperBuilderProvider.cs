using DRM.TypeWrapper;
using DRM.TypeWrapper.TypeDesc;
using DRM.ViewModelTools;
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
            ViewModelActivator = viewModelActivator ?? new SimpleViewModelActivator();
            _storeAccessCreator = storeAccessCreator ?? throw new ArgumentNullException(nameof(storeAccessCreator));

        }

        public IBuildPropBagMapper<TSource, TDestination> GetPropBagMapperBuilder<TSource, TDestination>
            (
            IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder
            ) where TDestination: class, IPropBag
        {
            IBuildPropBagMapper<TSource, TDestination> result
                = new SimplePropBagMapperBuilder<TSource, TDestination>
                (
                    mapperConfigurationBuilder: mapperConfigurationBuilder,
                    wrapperTypeCreator: WrapperTypeCreator,
                    viewModelActivator: ViewModelActivator,
                    storeAccessCreator: _storeAccessCreator
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
    }
}
