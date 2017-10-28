using AutoMapper;
using System;
using System.Collections.Generic;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IConfigureAMapper
    {
        bool SupportsMapFrom { get; }
        IGetInitialMapperConfig MapperConfigStarter { get; }
        IList<IMapperConfigurationStep> ConfigurationSteps { get; }

        void Add(IMapperConfigurationStep step);
        void Clear();

        IConfigurationProvider ConfigurationProvider { get; }
    }
}
