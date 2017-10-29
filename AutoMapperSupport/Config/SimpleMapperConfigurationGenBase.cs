﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport.Config
{
    public abstract class SimpleMapperConfigurationGenBase : IConfigureAMapperGen
    {
        public virtual bool SupportsMapFrom => true;

        protected IBuildMapperConfigurationsGen MapperConfigBuilderGen { get; }

        public IList<IMapperConfigurationStepGen> ConfigurationSteps { get; }

        public SimpleMapperConfigurationGenBase(IBuildMapperConfigurationsGen mapperConfigBuilderGen)
        {
            MapperConfigBuilderGen = mapperConfigBuilderGen;
            ConfigurationSteps = new List<IMapperConfigurationStepGen>();
        }

        // TODO: Make this use the singleton pattern -- once the base has been provided,
        // and the steps performed, there is no reason to re-evaluate.
        public IConfigurationProvider GetConfigurationProviderGen()
        {
            IConfigurationProvider mapperProfile = MapperConfigBuilderGen.GetNewBaseConfiguration(this);
            return mapperProfile;
        }

        public void Add(IMapperConfigurationStepGen step)
        {
            ConfigurationSteps.Add(step);
        }

        public void Clear()
        {
            ConfigurationSteps.Clear();
        }
    }
}
