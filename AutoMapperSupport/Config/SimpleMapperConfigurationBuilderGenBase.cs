using AutoMapper;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public class SimpleMapperConfigurationBuilderGenBase : IBuildMapperConfigurationsGen
    {
        protected IMapperConfigurationStepGen ConfigStarter { get; set; }

        protected IConfigureAMapperGen Configs { get; set; }

        private Action<IMapperConfigurationExpression> _action;
        protected Action<IMapperConfigurationExpression> Action
        {
            get
            {
                if(_action == null)
                {
                    _action = ConfigStarter?.ConfigurationStep ?? NoOpAction;

                    foreach (IMapperConfigurationStepGen step in Configs.ConfigurationSteps)
                    {
                        _action += step.ConfigurationStep;
                    }
                }
                return _action;
            }
            set
            {
                _action = value;
            }
        }

        #region Constructors

        public SimpleMapperConfigurationBuilderGenBase() : this(null)
        {
        }

        public SimpleMapperConfigurationBuilderGenBase(IMapperConfigurationStepGen configStarter)
        {
            ConfigStarter = configStarter;
            _action = null;
            Configs = null;
        }

        #endregion

        public IConfigurationProvider GetNewConfiguration(IConfigureAMapperGen configs)
        {
            // Keep the existing ConfigStarter setting.
            Configs = configs;
            _action = null;

            IConfigurationProvider result = new MapperConfiguration(Action);

            return result;
        }

        public IConfigurationProvider GetNewConfiguration(IConfigureAMapperGen configs, IMapperConfigurationStepGen configStarter)
        {
            Configs = configs;
            ConfigStarter = configStarter;
            _action = null;

            IConfigurationProvider result = new MapperConfiguration(Action);

            return result;
        }

        private void NoOpAction(IMapperConfigurationExpression cfg)
        {
            // Do nothing.
        }

    }
}
