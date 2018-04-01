using AutoMapper;
using System;

namespace Swhp.AutoMapperSupport
{
    public interface IHaveAMapperConfigurationStep
    {
        Action<IMapperConfigurationExpression> ConfigurationStep { get; }
    }
}
