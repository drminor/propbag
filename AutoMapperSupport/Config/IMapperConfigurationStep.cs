using AutoMapper;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IMapperConfigurationStep
    {
        Action<IMapperConfigurationExpression> ConfigurationStep { get; }
    }
}
