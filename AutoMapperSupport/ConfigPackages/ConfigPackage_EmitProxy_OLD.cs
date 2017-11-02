using DRM.TypeWrapper;
using DRM.TypeWrapper.TypeDesc;
using DRM.ViewModelTools;

namespace DRM.PropBag.AutoMapperSupport
{
    public class ConfigPackage_EmitProxy : AbstractConfigPackage
    {
        public override IHaveAMapperConfigurationStep GetConfigStarter()
        {
            return new MapperConfigStarter_Default();
        }

        public override IViewModelActivator GetViewModelActivator()
        {
            SimlpleViewModelActivator standardActivator = new SimlpleViewModelActivator();
            return standardActivator;
        }

        public override ICreateWrapperType GetWrapperTypeCreator()
        {
            ICreateWrapperType typeBuilder = GetSimpleWrapperTypeCreator();
            return typeBuilder;
        }

        public override ICreateMappingExpressions<TSource, TDestination> GetFinalConfigAction<TSource, TDestination>()
        {
            return new EmitProxyConfigFinalStep<TSource, TDestination>();
        }

        public override bool RequiresWrappperTypeEmitServices => true;


        // -- Build EmitProxy style of ViewModel Activator
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
