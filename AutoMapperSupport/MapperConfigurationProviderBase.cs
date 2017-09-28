using AutoMapper;
using AutoMapper.Configuration;
using AutoMapper.Configuration.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.AutoMapperSupport
{
    public abstract class MapperConfigurationProviderBase : IMapperConfiBuilderProvider
    {
        /// <summary>
        /// Returns the Method named: BuildBaseConfigAction.
        /// Classes that inherit from this class should override 
        /// the BuildBaseConfigAction to determine how the Base Configuration is Built.
        /// </summary>
        public Func<MapperConfigurationExpression, MapperConfiguration> BaseConfigBuilder
        {
            get
            {
                return BuildBaseConfigAction;
            }
        }

        protected virtual MapperConfiguration BuildBaseConfigAction(MapperConfigurationExpression cfg)
        {
            // Remove all default settings, by calling AddMemberConfiguration();
            //ResetMemberConfigurationAction(cfg);

            // Create a new mapping configration using the configuration expression.
            MapperConfiguration result = new MapperConfiguration(cfg);

            // Perform any actions here that is required by this application.
            return result;
        }

        public Action<MapperConfigurationExpression> ResetMemberConfigAction
        {
            get { return ResetMemberConfiguration; }
        }

        protected virtual void ResetMemberConfiguration(MapperConfigurationExpression cfg)
        {
            // This will reset the new MapperConfig so that there are no default settings.
            cfg.AddMemberConfiguration();
        }
    }
}
