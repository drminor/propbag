﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport
{
    public class SimpleMapperConfigurationBuilder<TSource, TDestination>
        : SimpleMapperConfigurationBuilderGenBase, IBuildMapperConfigurations<TSource, TDestination> where TDestination : class, IPropBag
    {
        public SimpleMapperConfigurationBuilder() : base(null) 
        {
        }

        public SimpleMapperConfigurationBuilder(IMapperConfigurationStepGen configStarter) : base(configStarter)
        {

        }

        public IConfigurationProvider GetNewBaseConfiguration(
            IConfigureAMapper<TSource, TDestination> configs,
            IPropBagMapperKey<TSource, TDestination> mapRequest)
        {
            // Reset all settings but configStarter
            base.Configs = configs;
            base.Action = null;

            // The base will produce a single compositeAction from the list of actions
            // in configs.
            Action<IMapperConfigurationExpression> compositeGenAction = base.Action;

            IConfigurationProvider result = GetNewBaseConfiguration(compositeGenAction, configs.FinalConfigStep.ConfigurationStep, mapRequest);

            return result;
        }

        public IConfigurationProvider GetNewBaseConfiguration(
            IConfigureAMapper<TSource, TDestination> configs,
            IPropBagMapperKey<TSource, TDestination> mapRequest,
            IMapperConfigurationStepGen configStarter)
        {
            // Reset all to force new evaluation of configs.
            base.Configs = configs;
            base.ConfigStarter = configStarter;
            base.Action = null;

            // The base will produce a single compositeAction from the list of actions
            // in configs.
            Action<IMapperConfigurationExpression> compositeGenAction = base.Action;

            IConfigurationProvider result = GetNewBaseConfiguration(compositeGenAction, configs.FinalConfigStep.ConfigurationStep, mapRequest);

            return result;
        }

        private IConfigurationProvider GetNewBaseConfiguration(
            Action<IMapperConfigurationExpression> compositeGenAction,
            Action<IPropBagMapperKey<TSource, TDestination>, IMapperConfigurationExpression> finalAction,
            IPropBagMapperKey<TSource, TDestination> mapRequest)
        {
            // Wrap up the mapRequestValue in a standard Action<IMapperConfigurationExpression> action.
            Action<IMapperConfigurationExpression> wrappedAction = GetWrappedAction(finalAction, mapRequest);
            compositeGenAction += wrappedAction;

            IConfigurationProvider result = new MapperConfiguration(compositeGenAction);
            return result;
        }


        private Action<IMapperConfigurationExpression> GetWrappedAction(
            Action<IPropBagMapperKey<TSource, TDestination>, IMapperConfigurationExpression> finalAction,
            IPropBagMapperKey<TSource, TDestination> mapRequest)
        {
            return OurAction;

            // Nested Method, wraps the mapRequest value for use later, when this
            // action is called.
            void OurAction(IMapperConfigurationExpression exp)
            {
                finalAction(mapRequest, exp);
            }
        }

    }
}