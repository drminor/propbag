using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using DRM.ViewModelTools;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public abstract class AbstractConfigPackage : ICreateMapperRequests
    {
        public virtual IPropBagMapperKey<TSource, TDestination> CreateMapperRequest<TSource, TDestination> 
            (
            PropModel propModel,
            Type typeToWrap,
            IPropFactory propFactory,
            IHaveAMapperConfigurationStep configStarterForThisRequest
            ) where TDestination : class, IPropBag
        {
            if (propModel == null) throw new ArgumentNullException($"{nameof(propModel)}");
            if (typeToWrap == null) throw new ArgumentNullException($"{nameof(typeToWrap)}");
            if (propFactory == null) throw new ArgumentNullException($"{nameof(propFactory)}");

            #region Mapper Configuration Work

            IHaveAMapperConfigurationStep configStarterForAllBuilds = GetConfigStarter();

            if(configStarterForAllBuilds != null && configStarterForThisRequest != null)
            {
                System.Diagnostics.Debug.WriteLine("A ConfigStarter has been specified for all builds and for just this build; using both.");
            }

            IBuildMapperConfigurations<TSource, TDestination> configBuilder
                = new SimpleMapperConfigurationBuilder<TSource, TDestination>
                (configStarterForAllBuilds); // The configStarter for all builds.

            IConfigureAMapper<TSource, TDestination> mappingConf
                = new SimpleMapperConfiguration<TSource, TDestination>
                (configBuilder, configStarterForThisRequest) // The configStarter for just this build.
                {
                    FinalConfigAction = GetFinalConfigAction<TSource, TDestination>().ActionStep
                };

            IViewModelActivator viewModelActivator = GetViewModelActivator();

            SimplePropBagMapperBuilder<TSource, TDestination> mapperBuilder
                = new SimplePropBagMapperBuilder<TSource, TDestination>
                (mapperConfiguration: mappingConf, vmActivator: viewModelActivator);

            #endregion

            IMapTypeDefinitionProvider mapTypeDefinitionProvider = GetMapTypeDefinitionProvider();

            IMapTypeDefinition<TSource> sourceMapTypeDef = mapTypeDefinitionProvider.GetTypeDescription<TSource>
                (propModel, propFactory, typeToWrap, null);

            IMapTypeDefinition<TDestination> destinationMapTypeDef = mapTypeDefinitionProvider.GetTypeDescription<TDestination>
                (propModel, propFactory, typeToWrap, null);

            IPropBagMapperKey<TSource, TDestination> result = new PropBagMapperKey<TSource, TDestination>
                (
                propBagMapperBuilder: mapperBuilder,
                //mappingConfiguration: mappingConf,
                sourceMapTypeDef: sourceMapTypeDef,
                destinationMapTypeDef: destinationMapTypeDef,
                sourceConstructor: null,
                destinationConstructor: null
                );

            return result;
        }

        public abstract IHaveAMapperConfigurationStep GetConfigStarter();

        public abstract IViewModelActivator GetViewModelActivator();

        public abstract IMapperConfigurationFinalAction<TSource, TDestination>
            GetFinalConfigAction<TSource, TDestination>() where TDestination : class, IPropBag;

        public virtual IMapTypeDefinitionProvider GetMapTypeDefinitionProvider()
        {
            return new SimpleMapTypeDefinitionProvider();
        }

    }
}
