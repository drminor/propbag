﻿using AutoMapper;
using System;
using System.Collections.Generic;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IConfigureAMapper<TSource, TDestination> : IConfigureAMapperGen where TDestination : class, IPropBag
    {
        IMapperConfigurationStep<TSource, TDestination> FinalConfigStep { get; }

        IConfigurationProvider GetConfigurationProvider(IPropBagMapperKey<TSource, TDestination> propBagMapperKey);
    }

    public interface IConfigureAMapperGen
    {
        bool SupportsMapFrom { get; }
        IList<IMapperConfigurationStepGen> ConfigurationSteps { get; }

        void Add(IMapperConfigurationStepGen step);
        void Clear();

        IConfigurationProvider GetConfigurationProviderGen();
    }
}
