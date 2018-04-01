using Swhp.AutoMapperSupport;
using System.Collections.Generic;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    public class ConfigPackage_EmitProxy : IProvideAMapperConfiguration
    {
        public IConfigureAMapper<TSource, TDestination> GetTheMapperConfig<TSource, TDestination>() //where TDestination : class, IPropBag
        {
            List<IHaveAMapperConfigurationStep> configSteps = new List<IHaveAMapperConfigurationStep>
            {
                new MapperConfigStarter_Default()
            };

            IConfigureAMapper<TSource, TDestination> result = new SimpleMapperConfigTyped<TSource, TDestination>
            (
                configSteps: configSteps,
                finalConfigActionProvider: new EmitProxyConfigFinalStep<TSource, TDestination>(),
                sourceConstructor: null,
                destinationConstructor: null,
                configStarter: null,
                requiresWrappperTypeEmitServices: true
            );
                
            return result;
        }
    }
}
