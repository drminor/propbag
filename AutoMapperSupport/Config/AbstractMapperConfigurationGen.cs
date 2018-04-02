
using System.Collections.Generic;

namespace Swhp.AutoMapperSupport
{
    public abstract class AbstractMapperConfigurationGen : IConfigureAMapperGen
    {
        public string PackageName { get; }
        public IHaveAMapperConfigurationStep ConfigStarter { get; }

        protected List<IHaveAMapperConfigurationStep> _ourConfigurationSteps;
        public IReadOnlyCollection<IHaveAMapperConfigurationStep> ConfigurationSteps { get; }

        public virtual bool SupportsMapFrom { get; }
        public bool RequiresWrappperTypeEmitServices { get; }

        public AbstractMapperConfigurationGen
            (
            string packageName,
            List<IHaveAMapperConfigurationStep> configSteps,
            IHaveAMapperConfigurationStep configStarter,
            bool requiresWrapperTypeEmitServices,
            bool supportsMapFrom
            )
        {
            PackageName = packageName;
            ConfigStarter = configStarter;

            _ourConfigurationSteps = MakeACopy(configSteps);
            ConfigurationSteps = _ourConfigurationSteps.AsReadOnly();

            SupportsMapFrom = supportsMapFrom;
            RequiresWrappperTypeEmitServices = requiresWrapperTypeEmitServices;
        }

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

