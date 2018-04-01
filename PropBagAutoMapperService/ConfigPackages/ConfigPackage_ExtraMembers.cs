using Swhp.AutoMapperSupport;
using System.Collections.Generic;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    public class ConfigPackage_ExtraMembers : IProvideAMapperConfiguration
    {
        public IConfigureAMapper<TSource, TDestination> GetTheMapperConfig<TSource, TDestination>() //where TDestination : class, IPropBag
        {
            List<IHaveAMapperConfigurationStep> configSteps = new List<IHaveAMapperConfigurationStep>
            {
                new MapperConfigStarter_Default(),
                new ExtraMembersConfigInitialStep()
            };

            IConfigureAMapper<TSource, TDestination> result = new SimpleMapperConfigTyped<TSource, TDestination>
            (
                configSteps: configSteps,
                finalConfigActionProvider: new ExtraMembersConfigFinalStep<TSource, TDestination>(),
                sourceConstructor: null,
                destinationConstructor: null,
                configStarter: null,
                requiresWrappperTypeEmitServices: false
            );
                
            return result;
        }
    }
}
