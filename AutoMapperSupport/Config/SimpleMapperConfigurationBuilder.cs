using AutoMapper;
using System;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public class SimpleMapperConfigurationBuilder<TSource, TDestination>
        : SimpleMapperConfigurationBuilderGenBase, IBuildMapperConfigurations<TSource, TDestination> where TDestination : class, IPropBag
    {
        public SimpleMapperConfigurationBuilder()
            : base(null) 
        {
        }

        public SimpleMapperConfigurationBuilder(IHaveAMapperConfigurationStep configStarter)
            : base(configStarter)
        {

        }

        //public IConfigurationProvider GetNewConfiguration(
        //    IConfigureAMapper<TSource, TDestination> configs,
        //    IPropBagMapperKey<TSource, TDestination> mapRequest)
        //{
        //    // Reset all settings but configStarter
        //    base.Configs = configs;
        //    base.Action = null;

        //    // The base will produce a single compositeAction from the list of actions
        //    // in configs.
        //    Action<IMapperConfigurationExpression> compositeGenAction = base.Action;

        //    IConfigurationProvider result = GetNewConfiguration(compositeGenAction, configs.FinalConfigActionProvider.ActionStep, mapRequest);

        //    return result;
        //}

        public IConfigurationProvider GetNewConfiguration
            (
            //IConfigureAMapper<TSource, TDestination> configs,
            IPropBagMapperKey<TSource, TDestination> mapRequest/*,  IHaveAMapperConfigurationStep configStarter*/
            )
        {
            // Reset all to force new evaluation of configs.
            base.Configs = mapRequest.MappingConfiguration; // configs;
            base.ConfigStarter = null; // configStarter;
            base.Action = null;

            // The base will produce a single compositeAction from the list of actions
            // in configs.
            Action<IMapperConfigurationExpression> compositeGenAction = base.Action;

            IConfigurationProvider result = GetNewConfiguration(compositeGenAction/*, mapRequest.MappingConfiguration.FinalConfigActionProvider.ActionStep*/, mapRequest);

            return result;
        }

        private IConfigurationProvider GetNewConfiguration(
            Action<IMapperConfigurationExpression> compositeGenAction,
            //Action<IPropBagMapperKey<TSource, TDestination>, IMapperConfigurationExpression> finalAction,
            IPropBagMapperKey<TSource, TDestination> mapRequest)
        {
            // Wrap up the mapRequestValue in a standard Action<IMapperConfigurationExpression> action.
            Action<IMapperConfigurationExpression> wrappedAction = GetWrappedAction(/*finalAction, */mapRequest);
            compositeGenAction += wrappedAction;

            IConfigurationProvider result = new MapperConfiguration(compositeGenAction);
            return result;
        }

        // TODO: Figure out a way to avoid wrapping the mapRequest in the new action.
        // can we have the caller provide this when making the call?
        private Action<IMapperConfigurationExpression> GetWrappedAction(
            //Action<IPropBagMapperKey<TSource, TDestination>, IMapperConfigurationExpression> finalAction,
            IPropBagMapperKey<TSource, TDestination> mapRequest)
        {
            return OurAction;

            // Nested Method, wraps the mapRequest value for use later, when this
            // action is called.
            void OurAction(IMapperConfigurationExpression exp)
            {
                Action<IPropBagMapperKey<TSource, TDestination>, IMapperConfigurationExpression> finalAction
                    = mapRequest.MappingConfiguration.FinalConfigActionProvider.ActionStep;

                finalAction(mapRequest, exp);
            }
        }

    }
}
