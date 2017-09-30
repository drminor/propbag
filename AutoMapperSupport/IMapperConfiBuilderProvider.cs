using AutoMapper;
using AutoMapper.Configuration;
using System;

namespace DRM.PropBag.AutoMapperSupport
{   
    public interface IMapperConfiBuilderProvider
    {
        Func<Action<IMapperConfigurationExpression>, IConfigurationProvider> BaseConfigBuilder { get; }

        Action<IMapperConfigurationExpression> ResetMemberConfigAction { get; }

        Action<IMapperConfigurationExpression> UseDefaultConfigurationAction { get; }
    }
}
