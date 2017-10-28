using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport.Config
{
    public class SimpleMapperConfiguration : IConfigureAMapper
    {
        public bool SupportsMapFrom => true;

        public IGetInitialMapperConfig MapperConfigStarter { get; }

        public IList<IMapperConfigurationStep> ConfigurationSteps { get; }

        public SimpleMapperConfiguration(IGetInitialMapperConfig mapperConfigStarter)
        {
            MapperConfigStarter = mapperConfigStarter;
            ConfigurationSteps = new List<IMapperConfigurationStep>();
        }

        // TODO: Make this use the singleton pattern -- once the base has been provided,
        // and the steps performed, there is no reason to re-evaluate.
        public IConfigurationProvider ConfigurationProvider
        {
            get
            {
                IConfigurationProvider mapperExp = MapperConfigStarter.GetNewBaseConfiguration();
                foreach(IMapperConfigurationStep step in ConfigurationSteps)
                {
                    //step.ConfigurationStep(mapperExp.m);
                }
                return mapperExp;
            }
        }

        public void Add(IMapperConfigurationStep step)
        {
            ConfigurationSteps.Add(step);
        }

        public void Clear()
        {
            ConfigurationSteps.Clear();
        }
    }
}
