using DRM.TypeWrapper;
using DRM.TypeWrapper.TypeDesc;
using DRM.ViewModelTools;

using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public class SimplePropBagMapperBuilderProvider : IPropBagMapperBuilderProvider
    {
        //private IBuildMapperConfigurations<int, PropBag> y;
        //private IConfigureAMapper<int, PropBag> z;

        private ICreateWrapperType WrapperTypeCreator { get; }
        private IViewModelActivator ViewModelActivator { get; }

        public SimplePropBagMapperBuilderProvider(ICreateWrapperType wrapperTypeCreator, IViewModelActivator viewModelActivator)
        {
            WrapperTypeCreator = wrapperTypeCreator ?? GetSimpleWrapperTypeCreator();
            ViewModelActivator = viewModelActivator ?? new SimpleViewModelActivator();
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
                    viewModelActivator: ViewModelActivator
                );

            return result;
        }

        private ICreateWrapperType GetSimpleWrapperTypeCreator()
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

            ICreateWrapperType result = new SimpleWrapperTypeCreator
                (
                wrapperTypeCachingService: wrapperTypeCachingService,
                typeDescCachingService: typeDescCachingService//,
                //propModelProvider: null
                );

            return result;
        }
    }
}
