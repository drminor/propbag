using AutoMapper;
using System;

namespace Swhp.AutoMapperSupport
{
    // TODO: replace this class with a method, or two in the SimpleMapperConfigurationBuilder class.

    public class SimpleMapperConfigurationBuilderGenBase /*: IBuildMapperConfigurationsGen*/
    {
        protected IHaveAMapperConfigurationStep ConfigStarter { get; set; }

        protected IConfigureAMapperGen Configs { get; set; }

        private Action<IMapperConfigurationExpression> _action;
        protected Action<IMapperConfigurationExpression> Action
        {
            get
            {
                if(_action == null)
                {
                    _action = ConfigStarter?.ConfigurationStep ?? NoOpAction;

                    foreach (IHaveAMapperConfigurationStep step in Configs.ConfigurationSteps)
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

        public SimpleMapperConfigurationBuilderGenBase(IHaveAMapperConfigurationStep configStarter)
        {
            ConfigStarter = configStarter;
            _action = null;
            Configs = null;
        }

        #endregion

        //public IConfigurationProvider GetNewConfiguration(IConfigureAMapperGen configs)
        //{
        //    // Keep the existing ConfigStarter setting.
        //    Configs = configs;
        //    _action = null;

        //    IConfigurationProvider result = new MapperConfiguration(Action);

        //    return result;
        //}

        //public IConfigurationProvider GetNewConfiguration(IConfigureAMapperGen configs, IHaveAMapperConfigurationStep configStarter)
        //{
        //    Configs = configs;
        //    ConfigStarter = configStarter;
        //    _action = null;

        //    IConfigurationProvider result = new MapperConfiguration(Action);

        //    return result;
        //}

        private void NoOpAction(IMapperConfigurationExpression cfg)
        {
            // Do nothing.
        }

    }
}
