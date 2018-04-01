
using System.Collections.Generic;

namespace Swhp.AutoMapperSupport
{
    public abstract class AbstractMapperConfigurationGen : IConfigureAMapperGen
    {
        //protected IBuildMapperConfigurationsGen MapperConfigBuilderGen { get; }

        public IHaveAMapperConfigurationStep ConfigStarter { get; }

        protected List<IHaveAMapperConfigurationStep> _ourConfigurationSteps;
        public IReadOnlyCollection<IHaveAMapperConfigurationStep> ConfigurationSteps { get; }

        public virtual bool SupportsMapFrom { get; }
        public bool RequiresWrappperTypeEmitServices { get; }

        public AbstractMapperConfigurationGen
            (
            //mapperConfigBuilderGen,
            List<IHaveAMapperConfigurationStep> configSteps,
            IHaveAMapperConfigurationStep configStarter,
            bool requiresWrapperTypeEmitServices,
            bool supportsMapFrom
            )
        {
            //MapperConfigBuilderGen = mapperConfigBuilderGen;
            ConfigStarter = configStarter;

            _ourConfigurationSteps = MakeACopy(configSteps);
            ConfigurationSteps = _ourConfigurationSteps.AsReadOnly();

            SupportsMapFrom = supportsMapFrom;
            RequiresWrappperTypeEmitServices = requiresWrapperTypeEmitServices;
        }

        //// TODO: Make this use the singleton pattern -- once the base has been provided,
        //// and the steps performed, there is no reason to re-evaluate.
        //public IConfigurationProvider GetConfigurationProviderGen()
        //{
        //    IConfigurationProvider mapperProfile = MapperConfigBuilderGen.GetNewConfiguration(this);
        //    return mapperProfile;
        //}

        private List<IHaveAMapperConfigurationStep> MakeACopy(List<IHaveAMapperConfigurationStep> source)
        {
            List<IHaveAMapperConfigurationStep> result = new List<IHaveAMapperConfigurationStep>(source);
            return result;
        }

        //public void Add(IHaveAMapperConfigurationStep step)
        //{
        //    ConfigurationSteps.Add(step);
        //}

        //public void Clear()
        //{
        //    ConfigurationSteps.Clear();
        //}
    }
}

