using AutoMapper;
using AutoMapper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.AutoMapperSupport
{
    public abstract class MapperConfigurationProviderBase : IMapperConfigurationProvider
    {

        public Func<MapperConfigurationExpression, MapperConfiguration> BaseConfigBuilder
        {
            get { return BuildBaseConfigAction; }
        }

        protected virtual MapperConfiguration BuildBaseConfigAction(MapperConfigurationExpression cfg)
        {
            MapperConfiguration result = new MapperConfiguration(DefaultConfig);

            // Perform any actions here that is required by this application.
            return result;
        }

        public Action<IMapperConfigurationExpression> DefaultConfig
        {
            get { return GetDefautConfigAction; }
        }

        public Action<IMapperConfigurationExpression> EmptyConfig
        {
            get { return GetEmptyConfigAction; }
        }

        protected virtual void GetDefautConfigAction(IMapperConfigurationExpression cfg)
        {
            // Do nothing. This will allow the brand new MapperConfig to remain at it's default state.
        }

        protected virtual void GetEmptyConfigAction(IMapperConfigurationExpression cfg)
        {
            // This will reset the new MapperConfig so that there are no default settings.
            cfg.AddMemberConfiguration();
        }
    }
}
