using DRM.ViewModelTools;

namespace DRM.PropBag.AutoMapperSupport
{
    public class ConfigPackage_ExtraMembers : AbstractConfigPackage
    {
        public override IHaveAMapperConfigurationStep GetConfigStarter()
        {
            return new MapperConfigStarter_Default();
        }

        //public override IViewModelActivator GetViewModelActivator()
        //{
        //    return new SimlpleViewModelActivator();
        //}

        //public override ICreateWrapperType GetWrapperTypeCreator()
        //{
        //    return null;
        //}

        public override ICreateMappingExpressions<TSource, TDestination> GetFinalConfigAction<TSource, TDestination>()
        {
            return new ExtraMembersConfigFinalStep<TSource, TDestination>();
        }

        public override bool RequiresWrappperTypeEmitServices => false;

    }
}
