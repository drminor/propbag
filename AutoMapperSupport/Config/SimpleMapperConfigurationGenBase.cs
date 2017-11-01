using AutoMapper;
using System.Collections.Generic;

namespace DRM.PropBag.AutoMapperSupport
{
    public abstract class SimpleMapperConfigurationGenBase : IConfigureAMapperGen
    {
        public virtual bool SupportsMapFrom => true;

        protected IBuildMapperConfigurationsGen MapperConfigBuilderGen { get; }

        public IList<IHaveAMapperConfigurationStep> ConfigurationSteps { get; }

        public SimpleMapperConfigurationGenBase(IBuildMapperConfigurationsGen mapperConfigBuilderGen)
        {
            MapperConfigBuilderGen = mapperConfigBuilderGen;
            ConfigurationSteps = new List<IHaveAMapperConfigurationStep>();
        }

        // TODO: Make this use the singleton pattern -- once the base has been provided,
        // and the steps performed, there is no reason to re-evaluate.
        public IConfigurationProvider GetConfigurationProviderGen()
        {
            IConfigurationProvider mapperProfile = MapperConfigBuilderGen.GetNewConfiguration(this);
            return mapperProfile;
        }

        public void Add(IHaveAMapperConfigurationStep step)
        {
            ConfigurationSteps.Add(step);
        }

        public void Clear()
        {
            ConfigurationSteps.Clear();
        }
    }
}

