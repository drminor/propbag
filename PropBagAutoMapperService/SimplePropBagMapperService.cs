using AutoMapper;
using DRM.TypeSafePropertyBag;
using Swhp.AutoMapperSupport;
using System;
using System.Reflection;
using System.Threading;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    using PropModelType = IPropModel<String>;

    public class SimplePropBagMapperService : IPropBagMapperService, IDisposable
    {
        public const int PROP_BAG_MAPPER_CONFIG_DETAIL_EXTENSION_SOURCE_ID = 1;

        #region Private Members

        private readonly IMapTypeDefinitionProvider _mapTypeDefinitionProvider;
        private readonly IPropBagMapperBuilderProvider _mapperBuilderProvider;

        private readonly ICachePropBagMappers _propBagMappersCache;
        private readonly IAutoMapperService _autoMapperService;

        #endregion

        #region Constructors

        // Disallow the parameterless constructor.
        private SimplePropBagMapperService()
        {
            throw new NotSupportedException("Use of the paremeterless constructor for SimpleAutoMapperProvider is not supported.");
        }

        public SimplePropBagMapperService
        (
            IMapTypeDefinitionProvider mapTypeDefinitionProvider,
            IPropBagMapperBuilderProvider mapperBuilderProvider,
            ICachePropBagMappers mappersCachingService,
            IAutoMapperService autoMapperService
            )
        {
            _mapTypeDefinitionProvider = mapTypeDefinitionProvider ?? throw new ArgumentNullException(nameof(mapTypeDefinitionProvider));
            _mapperBuilderProvider = mapperBuilderProvider ?? throw new ArgumentNullException(nameof(mapperBuilderProvider));
            _propBagMappersCache = mappersCachingService ?? throw new ArgumentNullException(nameof(mappersCachingService));
            _autoMapperService = autoMapperService ?? throw new ArgumentNullException(nameof(autoMapperService));
        }

        #endregion

        #region Public Methods

        // Gen Submit
        public IPropBagMapperRequestKeyGen SubmitPropBagMapperRequest
            (
            PropModelType propModel,
            Type sourceType,
            string configPackageName
            )
        {
            Type typeToCreate = propModel.RunTimeType;
            PropBagMapperReqSubDelegate mapperRequestSubmitter = GetPropBagMapperReqSubDelegate(sourceType, typeToCreate);
            IPropBagMapperRequestKeyGen result = mapperRequestSubmitter(propModel, configPackageName, this);

            return result;
        }

        // Typed Submit with ConfigPackageName
        public IPropBagMapperRequestKey<TSource, TDestination> SubmitPropBagMapperRequest<TSource, TDestination>
            (
            PropModelType propModel,
            string configPackageName,
            IHaveAMapperConfigurationStep configStarterForThisRequest = null,
            IPropFactory propFactory = null
            )
        where TDestination : class, IPropBag
        {
            // Lookup the package name and return a mapping configuration.
            IConfigureAMapper<TSource, TDestination> mappingConfiguration
                = GetTheMappingConfiguration<TSource, TDestination>(configPackageName);

            var result = SubmitPropBagMapperRequest<TSource, TDestination>(propModel, mappingConfiguration, configStarterForThisRequest, propFactory);

            return result;
        }

        // Typed Submit with Mapping Configuration.
        public IPropBagMapperRequestKey<TSource, TDestination> SubmitPropBagMapperRequest<TSource, TDestination>
            (
            PropModelType propModel,
            IConfigureAMapper<TSource, TDestination> mappingConfiguration,
            IHaveAMapperConfigurationStep configStarterForThisRequest,
            IPropFactory propFactory = null
            )
        where TDestination : class, IPropBag
        {
            // Use the AutoMapperService to register a 'raw' request.
            IAutoMapperRequestKey<TSource, TDestination> autoMapperRequestKey =
                this.SubmitRawAutoMapperRequest<TSource, TDestination>(propModel, mappingConfiguration, configStarterForThisRequest);

            // Create a MapperBuilder for this request.
            IPropBagMapperBuilder<TSource, TDestination> propBagMapperBuilder
                = BuildTheAutoMapperBuilder<TSource, TDestination>(/*configStarterForThisRequest*/);

            // Create the PropBag Mapper Request.
            IPropBagMapperRequestKey<TSource, TDestination> typedMapperRequest
                = new PropBagMapperRequestKey<TSource, TDestination>(propBagMapperBuilder, autoMapperRequestKey, propModel);

            //// Store the request 
            //IPropBagMapperKeyGen newMapRequest = RegisterPropBagMapperRequest(typedMapperRequest);
            //return (IPropBagMapperKey<TSource, TDestination>)newMapRequest;

            // We could store the request in the PropBagMapper Cache, but it would not be used.
            return typedMapperRequest;
        }

        private IPropBagMapperBuilder<TSource, TDestination> BuildTheAutoMapperBuilder<TSource, TDestination>()
            where TDestination : class, IPropBag
        {
            // Create a MapperBuilder for this request.
            IPropBagMapperBuilder<TSource, TDestination> propBagMapperBuilder
                = _mapperBuilderProvider.GetPropBagMapperBuilder<TSource, TDestination>(this);

            return propBagMapperBuilder;
        }

        // Typed Get PropBag Mapper
        public IPropBagMapper<TSource, TDestination> GetPropBagMapper<TSource, TDestination>
            (
            IPropBagMapperRequestKey<TSource, TDestination> mapperRequest
            )
            where TDestination : class, IPropBag
        {
            // TODO: Consider simply using the 'Gen' version on the PropBagMappers Cache
            // it not that much less type safe and not that much faster -- however it does add some complexity.
            //IPropBagMapper<TSource, TDestination> result = GetPropBagMapper(propBagMapperRequestKey);

            // --- LOOK AT ME ----

            // Fetch the raw AutoMapper, if not already retrieved.
            if (mapperRequest.AutoMapper == null)
            {
                mapperRequest.AutoMapper =
                    GetRawAutoMapper<TSource, TDestination>(mapperRequest.AutoMapperRequestKey);
            }

            IPropBagMapper<TSource, TDestination> result = _propBagMappersCache.GetPropBagMapper<TSource, TDestination>(mapperRequest);
            return result;
        }

        #endregion

        #region Pass-through calls to the PropBag MappersCache

        public IPropBagMapperRequestKeyGen RegisterPropBagMapperRequest(IPropBagMapperRequestKeyGen propBagMapperRequestKey)
        {
            return _propBagMappersCache.RegisterPropBagMapperRequest(propBagMapperRequestKey);
        }

        // --- LOOK AT ME ----
        public IPropBagMapperGen GetPropBagMapper(IPropBagMapperRequestKeyGen mapperRequest)
        {
            // Fetch the raw AutoMapper, if not already retrieved.
            if (mapperRequest.AutoMapper == null)
            {
                mapperRequest.AutoMapper = GetRawAutoMapperGen(mapperRequest.AutoMapperRequestKeyGen);
            }

            return _propBagMappersCache.GetPropBagMapper(mapperRequest);
        }

        public long ClearPropBagMappersCache()
        {
            return _propBagMappersCache.ClearPropBagMappersCache();
        }

        #endregion

        #region Methods using the AutoMapper Service

        // Submit a request to queue up the creation of a Raw AutoMapper, and receive the RequestKey.
        private IAutoMapperRequestKey<TSource, TDestination> SubmitRawAutoMapperRequest<TSource, TDestination>
            (
            PropModelType propModel,
            IConfigureAMapper<TSource, TDestination> mappingConfiguration,
            IHaveAMapperConfigurationStep configStarterForThisRequest
            )
            where TDestination : class, IPropBag
        {
            IMapTypeDefinition srcMapTypeDef
                = _mapTypeDefinitionProvider.GetTypeDescription(typeof(TSource), uniqueRef: propModel, uniqueToken: null);

            IMapTypeDefinition dstMapTypeDef
                = _mapTypeDefinitionProvider.GetTypeDescription(typeof(TDestination), uniqueRef: propModel, uniqueToken: null);

            CheckRunTimeType(propModel, typeof(TDestination));

            IAutoMapperConfigDetails pbMapperConfigDetails
                = new PropBagMapperConfigDetails
                (
                PROP_BAG_MAPPER_CONFIG_DETAIL_EXTENSION_SOURCE_ID,
                mappingConfiguration.PackageName,
                propModel
                );

            IAutoMapperRequestKey<TSource, TDestination> autoMapperRequestKey
                = _autoMapperService.SubmitRawAutoMapperRequest<TSource, TDestination>
                (
                    srcMapTypeDef,
                    dstMapTypeDef,
                    pbMapperConfigDetails,
                    mappingConfiguration,
                    configStarterForThisRequest
                );

            return autoMapperRequestKey;
        }

        private void CheckRunTimeType(PropModelType propModel, Type destinationType)
        {
            if (propModel != null)
            {
                if (propModel.RunTimeType == null)
                {
                    throw new InvalidOperationException("The PropModel's RunTimeType is null.");
                }

                if (propModel.RunTimeType != destinationType)
                {
                    //System.Diagnostics.Debug.WriteLine($"Warning: The type parameter T: ({typeof(T)}) does not equal the " +
                    //    $"PropModel's RunTimeType: ({propModel.RunTimeType}).");

                    throw new InvalidOperationException($"The type parameter T: ({destinationType}) does not equal the " +
                        $"PropModel's RunTimeType: ({propModel.RunTimeType}).");
                }
            }
        }

        // From AutoMapperRequestKey (Typed)
        private IMapper GetRawAutoMapper<TSource, TDestination>
            (
            IAutoMapperRequestKey<TSource, TDestination> rawAutoMappeRequestKey
            )
            where TDestination : class, IPropBag
        {
            //if (rawAutoMappeRequestKey.AutoMapper == null)
            //{
            //    IMapper autoMapper = _autoMapperService.GetRawAutoMapper<TSource, TDestination>(rawAutoMappeRequestKey);
            //    rawAutoMappeRequestKey.AutoMapper = autoMapper;
            //}

            //return rawAutoMappeRequestKey.AutoMapper;

            IMapper result = _autoMapperService.GetRawAutoMapper<TSource, TDestination>(rawAutoMappeRequestKey);
            return result;
        }

        // From AutoMapperRequestKey (Gen)
        private IMapper GetRawAutoMapperGen(IAutoMapperRequestKeyGen rawAutoMappeRequestKeyGen)
        {
            //if (rawAutoMappeRequestKeyGen.AutoMapper == null)
            //{
            //    IMapper autoMapper = _autoMapperService.GetAutoMapper(rawAutoMappeRequestKeyGen);
            //    rawAutoMappeRequestKeyGen.AutoMapper = autoMapper;
            //}

            //return rawAutoMappeRequestKeyGen.AutoMapper;

            IMapper result = _autoMapperService.GetAutoMapper(rawAutoMappeRequestKeyGen);
            return result;
        }

        #endregion

        #region Private Methods

        private IConfigureAMapper<TSource, TDestination> GetTheMappingConfiguration<TSource, TDestination>(string configPackageName) where TDestination : class, IPropBag
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

        #endregion

        #region Generic Method Support

        internal delegate IPropBagMapperRequestKeyGen PropBagMapperReqSubDelegate
            (PropModelType propModel, string configPackageName, IPropBagMapperService propBagMapperService);

        static PropBagMapperReqSubDelegate GetPropBagMapperReqSubDelegate(Type sourceType, Type destinationType)
        {
            MethodInfo TypedRegisterMapperRequest_MI = 
                GenericMethodTemplates.PropBagMapperReqSubmitter_MI.MakeGenericMethod(sourceType, destinationType);

            PropBagMapperReqSubDelegate result = (PropBagMapperReqSubDelegate)Delegate.CreateDelegate(typeof(PropBagMapperReqSubDelegate), TypedRegisterMapperRequest_MI);
            return result;
        }

        /// <summary>
        /// Used to create Delegates when the type of the value is not known at run time.
        /// </summary>
        static private Type GMT_TYPE = typeof(GenericMethodTemplates);

        // Method Templates for Property Bag
        internal static class GenericMethodTemplates
        {
            static Lazy<MethodInfo> propBagMapperReqSubmitter_Lazy_MI;
            public static MethodInfo PropBagMapperReqSubmitter_MI { get { return propBagMapperReqSubmitter_Lazy_MI.Value; } }

            static GenericMethodTemplates()
            {
                propBagMapperReqSubmitter_Lazy_MI = new Lazy<MethodInfo>(() =>
                    GMT_TYPE.GetMethod("SubmitPropBagMapperRequest", BindingFlags.Static | BindingFlags.NonPublic),
                    LazyThreadSafetyMode.PublicationOnly);
            }

            // The Typed Method for PropBagMappers
            static IPropBagMapperRequestKey<TSource, TDestination> SubmitPropBagMapperRequest<TSource, TDestination>
            (
                PropModelType propModel,
                string configPackageName,
                IPropBagMapperService propBagMapperService
            )
            where TDestination : class, IPropBag
            {
                IPropBagMapperRequestKey<TSource, TDestination> result
                    = propBagMapperService.SubmitPropBagMapperRequest<TSource, TDestination>
                    (
                        propModel: propModel,
                        configPackageName: configPackageName,
                        configStarterForThisRequest: null,
                        propFactory: null
                        );

                return result;
            }
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    ClearPropBagMappersCache();

                    if (_mapTypeDefinitionProvider is IDisposable disable1)
                    {
                        disable1.Dispose();
                    }

                    if (_propBagMappersCache is IDisposable disable2)
                    {
                        disable2.Dispose();
                    }

                    if (_mapperBuilderProvider is IDisposable disable3)
                    {
                        disable3.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Temp() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
