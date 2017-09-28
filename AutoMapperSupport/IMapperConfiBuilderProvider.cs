using AutoMapper;
using AutoMapper.Configuration;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IMapperConfiBuilderProvider
    {
        Action<MapperConfigurationExpression> ResetMemberConfigAction { get; }

        Func<MapperConfigurationExpression, MapperConfiguration> BaseConfigBuilder { get; }
    }
}
