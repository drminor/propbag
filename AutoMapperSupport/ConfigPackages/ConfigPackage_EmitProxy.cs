using DRM.TypeWrapper;
using DRM.TypeWrapper.TypeDesc;
using DRM.ViewModelTools;

namespace DRM.PropBag.AutoMapperSupport
{
    public class ConfigPackage_EmitProxy : AbstractConfigPackage
    {
        public override IMapperConfigurationStepGen GetConfigStarter()
        {
            return new MapperConfigStarter_Default();
        }

        public override IViewModelActivator GetViewModelActivator()
        {
            return GetEmitProxyActivator();
        }

        public override IMapperConfigurationStep<TSource, TDestination> GetFinalConfigAction<TSource, TDestination>()
        {
            return new StandardConfigFinalStep<TSource, TDestination>();
        }

        // -- Build EmitProxy style of ViewModel Activator
        private IViewModelActivator GetEmitProxyActivator()
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

            IViewModelActivator result = new ViewModelActivatorEmitProxy
                (
                wrapperTypeCachingService: wrapperTypeCachingService,
                typeDescCachingService: typeDescCachingService//,
                //propModelProvider: null
                );

            return result;
        }
    }
}
