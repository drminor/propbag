using AutoMapper;
using AutoMapper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IMapperConfigurationProvider
    {
        Action<IMapperConfigurationExpression> DefaultConfig { get; }

        Action<IMapperConfigurationExpression> EmptyConfig { get; }

        Func<MapperConfigurationExpression, MapperConfiguration> BaseConfigBuilder { get; }
    }
}
