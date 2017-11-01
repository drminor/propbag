using DRM.ViewModelTools;

namespace DRM.PropBag.AutoMapperSupport
{
    public class ConfigPackage_ExtraMembers : AbstractConfigPackage
    {
        public override IHaveAMapperConfigurationStep GetConfigStarter()
        {
            return new MapperConfigStarter_Default();
        }

        public override IViewModelActivator GetViewModelActivator()
        {
            return new ViewModelActivatorStandard();
        }

        public override IMapperConfigurationFinalAction<TSource, TDestination> GetFinalConfigAction<TSource, TDestination>()
        {
            return new ExtraMembersConfigFinalStep<TSource, TDestination>();
        }

    }
}
