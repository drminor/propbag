using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using DRM.ViewModelTools;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public class AutoMapperProvider : ICachePropBagMappers
    {
        #region Private Members

        string NO_PROPMODEL_LOOKUP_SERVICES = $"The {nameof(SimpleViewModelActivator)} has no PropModelProvider." +
            $"All calls must provide a PropModel.";

        IMapTypeDefinitionProvider MapTypeDefinitionProvider { get; }
        ICachePropBagMappers MappersCachingService { get; }
        IPropBagMapperBuilderProvider MapperBuilderProvider { get; }
        IPropModelProvider PropModelProvider { get; }

        #endregion

        #region Constructors

        private AutoMapperProvider() { } // Disallow the parameterless constructor.

        public AutoMapperProvider
            (
            IMapTypeDefinitionProvider mapTypeDefinitionProvider,
            ICachePropBagMappers mappersCachingService,
            IPropBagMapperBuilderProvider mapperBuilderProvider,
            IPropModelProvider propModelProvider = null
            )
        {
            MapTypeDefinitionProvider = mapTypeDefinitionProvider ?? throw new ArgumentNullException(nameof(mapTypeDefinitionProvider));
            MappersCachingService = mappersCachingService ?? throw new ArgumentNullException(nameof(mappersCachingService));
            MapperBuilderProvider = mapperBuilderProvider ?? throw new ArgumentNullException(nameof(mapperBuilderProvider));
            PropModelProvider = propModelProvider;

            if (!HasPropModelLookupService) System.Diagnostics.Debug.WriteLine(NO_PROPMODEL_LOOKUP_SERVICES);
        }

        #endregion

        #region Public Properties
        public bool HasPropModelLookupService => (PropModelProvider != null);
        #endregion

        #region Public Methods
        // TODO: Consider supporting the ability to provide a IConfigureAMapper instead of a configPackageName.

        public IPropBagMapperKey<TSource, TDestination> RegisterMapperRequest<TSource, TDestination>
            (
            string resourceKey,
            Type typeToWrap,
            string configPackageName,
            IHaveAMapperConfigurationStep configStarterForThisRequest = null,
            IPropFactory propFactory = null
            ) where TDestination : class, IPropBag
        {
            PropModel propModel = GetPropModel(resourceKey);

            IPropBagMapperKey<TSource, TDestination> typedMapperRequest =
                RegisterMapperRequest<TSource, TDestination>
                (
                    propModel,
                    typeToWrap,
                    configPackageName,
                    configStarterForThisRequest,
                    propFactory);

            return typedMapperRequest;
        }

        public IPropBagMapperKey<TSource, TDestination> RegisterMapperRequest<TSource, TDestination>
            (
            PropModel propModel,
            Type typeToWrap,
            string configPackageName,
            IHaveAMapperConfigurationStep configStarterForThisRequest = null,
            IPropFactory propFactory = null
            ) where TDestination : class, IPropBag
        {
            // TODO: check to make sure that the "configStarterForThisRequest" value is being sent to the correct place.

            // TODO: Consider making the caller supply a IBuildMapperConfigurations "service."
            // Create a Configuration Builder for this request.
            IBuildMapperConfigurations<TSource, TDestination> propBagMapperConfigurationBuilder
                = new SimpleMapperConfigurationBuilder<TSource, TDestination>(configStarter: configStarterForThisRequest);

            // Create a MapperBuilder for this request.
            IBuildPropBagMapper<TSource, TDestination> propBagMapperBuilder
                = MapperBuilderProvider.GetPropBagMapperBuilder<TSource, TDestination>(propBagMapperConfigurationBuilder);

            // Lookup the package name and return a mapping configuration.
            IConfigureAMapper<TSource, TDestination> mappingConfiguration
                = GetMappingConfiguration<TSource, TDestination>(configPackageName);

            IMapTypeDefinition<TSource> srcMapTypeDef
                = MapTypeDefinitionProvider.GetTypeDescription<TSource>(propModel, typeToWrap, className: null, propFactory: propFactory);

            IMapTypeDefinition<TDestination> dstMapTypeDef
                = MapTypeDefinitionProvider.GetTypeDescription<TDestination>(propModel, typeToWrap, className: null, propFactory: propFactory);


            // Create the mapper request.
            IPropBagMapperKey<TSource, TDestination> typedMapperRequest
                = new PropBagMapperKey<TSource, TDestination>
                (
                    propBagMapperBuilder: propBagMapperBuilder,
                    mappingConfiguration: mappingConfiguration,
                    sourceMapTypeDef: srcMapTypeDef,
                    destinationMapTypeDef: dstMapTypeDef
                );

            IPropBagMapperKeyGen newMapRequest = this.RegisterMapperRequest(typedMapperRequest);

            return (IPropBagMapperKey<TSource, TDestination>) newMapRequest;
        }

        #endregion

        #region Pass-through calls to the MappersCachingService

        public IPropBagMapper<TSource,TDestination> GetMapper<TSource, TDestination>(IPropBagMapperKey<TSource, TDestination> mapperRequest) where TDestination : class, IPropBag
        {
            return (IPropBagMapper<TSource, TDestination>)MappersCachingService.GetMapper(mapperRequest);
        }

        public IPropBagMapperKeyGen RegisterMapperRequest(IPropBagMapperKeyGen mapperRequest)
        {
            return MappersCachingService.RegisterMapperRequest(mapperRequest);
        }

        public IPropBagMapperGen GetMapper(IPropBagMapperKeyGen mapperRequest)
        {
            return MappersCachingService.GetMapper(mapperRequest);
        }

        #endregion

        #region Private Methods

        private IConfigureAMapper<TSource, TDestination> GetMappingConfiguration<TSource, TDestination>(string configPackageName) where TDestination : class, IPropBag
        {
            switch (configPackageName.ToLower())
            {
                case "extra_members":
                    {
                        return new ConfigPackage_ExtraMembers().GetTheMapperConfig<TSource, TDestination>();
                    }
                case "emit_proxy":
                    {
                        return new ConfigPackage_EmitProxy().GetTheMapperConfig<TSource, TDestination>();
                    }
                default:
                    {
                        throw new InvalidOperationException($"The configPackageName: {configPackageName} is not recognized.");
                    }
            }
        }

        private PropModel GetPropModel(string resourceKey, IPropFactory propFactory = null)
        {
            if (!HasPropModelLookupService)
            {
                throw new InvalidOperationException(NO_PROPMODEL_LOOKUP_SERVICES);
            }

            PropModel propModel = PropModelProvider.GetPropModel(resourceKey, propFactory);
            return propModel;
        }

        //private IPropBagMapperKey<TSource, TDestination> CreateTypedMapperRequest<TSource, TDestination>
        //    (
        //    PropModel propModel,
        //    Type typeToWrap,
        //    ICreateMapperRequests requestCreator,
        //    IHaveAMapperConfigurationStep configStarterForThisRequest
        //    ) where TDestination : class, IPropBag
        //{
        //    return requestCreator.CreateMapperRequest<TSource, TDestination>
        //        (
        //        propModel,
        //        typeToWrap,
        //        configStarterForThisRequest
        //        );
        //}

        #endregion

        #region GET SIZE

        private void GetSizeX()
        {
            long StopBytes = 0;
            object myFoo;

            long StartBytes = System.GC.GetTotalMemory(true);
            myFoo = new object();
            StopBytes = System.GC.GetTotalMemory(true);

            string result = "Size is " + ((long)(StopBytes - StartBytes)).ToString();

            GC.KeepAlive(myFoo); // This ensure a reference to object keeps object in memory
        }

        private void GetSizeY()
        {

            //long size = 0;
            //object o = new object();
            //using (Stream s = new MemoryStream())
            //{
            //    BinaryFormatter formatter = new BinaryFormatter();
            //    formatter.Serialize(s, o);
            //    size = s.Length;
            //}
        }

        #endregion
    }
}
