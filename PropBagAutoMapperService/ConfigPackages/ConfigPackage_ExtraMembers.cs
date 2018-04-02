using Swhp.AutoMapperSupport;
using System.Collections.Generic;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    public class ConfigPackage_ExtraMembers : IProvideAMapperConfiguration
    {
        public const string EXTRA_MEMBERS_PROP_BAG_PACKAGE_NAME = "Extra_Members";

        public IConfigureAMapper<TSource, TDestination> GetTheMapperConfig<TSource, TDestination>()
        {
            List<IHaveAMapperConfigurationStep> configSteps = new List<IHaveAMapperConfigurationStep>
            {
                new MapperConfigStarter_Default(),
                new ExtraMembersConfigInitialStep()
            };

            IConfigureAMapper<TSource, TDestination> result = new SimpleMapperConfiguration<TSource, TDestination>
            (
                EXTRA_MEMBERS_PROP_BAG_PACKAGE_NAME,
                configSteps: configSteps,
                finalConfigActionProvider: new ExtraMembersConfigFinalStep<TSource, TDestination>(),
                sourceConstructor: null,
                destinationConstructor: null,
                configStarter: null,
                requiresWrappperTypeEmitServices: false,
                supportsMapFrom: true
            );
                
            return result;
        }
    }
}
