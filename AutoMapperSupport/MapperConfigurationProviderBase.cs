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
        public Func<Action<IMapperConfigurationExpression>, IConfigurationProvider> BaseConfigBuilder
        {
            get
            {
                return BuildBaseConfigAction;
            }
        }

        protected virtual IConfigurationProvider BuildBaseConfigAction(Action<IMapperConfigurationExpression> cfgAction)
        {
            // Create a new mapping configration using the supplied Action which acts 
            // on an object of a type that implements the IMapperConfiguationExpression interface.
            IConfigurationProvider result = new MapperConfiguration(cfgAction);
            return result;
        }

        public Action<IMapperConfigurationExpression> UseDefaultConfigurationAction
        {
            get
            {
                return UseDefaultConfiguration;
            }
        }

        protected virtual void UseDefaultConfiguration(IMapperConfigurationExpression cfg)
        {
            // No nothing.
        }

        public Action<IMapperConfigurationExpression> ResetMemberConfigAction
        {
            get { return ResetMemberConfiguration; }
        }

        protected virtual void ResetMemberConfiguration(IMapperConfigurationExpression cfg)
        {
            // This will reset the new MapperConfig so that there are no default settings.
            cfg.AddMemberConfiguration();
        }
    }
}
