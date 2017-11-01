using AutoMapper;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IHaveAMapperConfigurationStep
    {
        Action<IMapperConfigurationExpression> ConfigurationStep { get; }
    }
}
