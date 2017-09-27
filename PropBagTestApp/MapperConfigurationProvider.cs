using DRM.PropBag.AutoMapperSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration;

namespace PropBagTestApp
{
    public class MapperConfigurationProvider : MapperConfigurationProviderBase
    {
        protected override MapperConfiguration BuildBaseConfigAction(MapperConfigurationExpression cfg)
        {
            return new MapperConfiguration(cfg);
        }
    }
}
