using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using DRM.ViewModelTools;
using System;
using System.Collections.Generic;

namespace DRM.PropBag.AutoMapperSupport
{
    public abstract class AbstractConfigPackage : ICreateMapperRequests
    {
        public virtual IPropBagMapperKey<TSource, TDestination> CreateMapperRequest<TSource, TDestination>
            (
            PropModel propModel,
            Type typeToWrap,
            IHaveAMapperConfigurationStep configStarterForThisRequest,
            IPropFactory propFactory = null
            ) where TDestination : class, IPropBag
        {
            if (propModel == null) throw new ArgumentNullException($"{nameof(propModel)}");
            //if (typeToWrap == null) throw new ArgumentNullException($"{nameof(typeToWrap)}");
            //if (propFactory == null) throw new ArgumentNullException($"{nameof(propFactory)}");

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
                (
                    configBuilder,
                    GetFinalConfigAction<TSource, TDestination>(),
                    this.RequiresWrappperTypeEmitServices, // Let the Mapping Configuration know if we require a ProxType to be created.
                    new List<IHaveAMapperConfigurationStep>(), // This will be fixed soon.
                    configStarterForThisRequest // The configStarter for just this build.
                );


            IViewModelActivator viewModelActivator = GetViewModelActivator();

            ICreateWrapperType wrapperTypeCreator = GetWrapperTypeCreator();


            IBuildPropBagMapper<TSource, TDestination> mapperBuilder
                = new SimplePropBagMapperBuilder<TSource, TDestination>
                (mapperConfiguration: mappingConf, wrapperTypeCreator: wrapperTypeCreator, viewModelActivator: viewModelActivator);

            #endregion

            //IMapTypeDefinitionProvider mapTypeDefinitionProvider = GetMapTypeDefinitionProvider();

            IMapTypeDefinition<TSource> sourceMapTypeDef = mapTypeDefinitionProvider.GetTypeDescription<TSource>
                (propModel, typeToWrap, null, propFactory);

            IMapTypeDefinition<TDestination> destinationMapTypeDef = mapTypeDefinitionProvider.GetTypeDescription<TDestination>
                (propModel, typeToWrap, null, propFactory);

            IPropBagMapperKey<TSource, TDestination> result = new PropBagMapperKey<TSource, TDestination>
                (
                propBagMapperBuilder: mapperBuilder,
                mappingConfiguration: mappingConf,
                sourceMapTypeDef: sourceMapTypeDef,
                destinationMapTypeDef: destinationMapTypeDef,
                sourceConstructor: null,
                destinationConstructor: null
                );

            return result;
        }

        public abstract IHaveAMapperConfigurationStep GetConfigStarter();

        //public abstract IViewModelActivator GetViewModelActivator();

        //public abstract ICreateWrapperType GetWrapperTypeCreator();

        public abstract ICreateMappingExpressions<TSource, TDestination>
            GetFinalConfigAction<TSource, TDestination>() where TDestination : class, IPropBag;

        public abstract bool RequiresWrappperTypeEmitServices { get; }

        public virtual IMapTypeDefinitionProvider GetMapTypeDefinitionProvider()
        {
            return new SimpleMapTypeDefinitionProvider();
        }

    }
}
