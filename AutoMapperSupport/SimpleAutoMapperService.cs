using AutoMapper;
using System;
using System.Reflection;
using System.Threading;

namespace Swhp.AutoMapperSupport
{
    public class SimpleAutoMapperService : IAutoMapperService, IDisposable
    {
        #region Private Members

        private readonly IAutoMapperCache _autoMapperCache;
        private readonly IAutoMapperBuilderProvider _autoMapperBuilderProvider;
        private IMapperConfigurationLookupService _mapperConfigurationLookupService { get; set; }

        #endregion

        #region Constructors

        public SimpleAutoMapperService
            (
            IAutoMapperBuilderProvider autoMapperBuilderProvider,
            IAutoMapperCache autoMapperCache,
            IMapperConfigurationLookupService mapperConfigurationLookupService
            )
        {
            _autoMapperBuilderProvider = autoMapperBuilderProvider ?? throw new ArgumentNullException(nameof(autoMapperBuilderProvider));
            _autoMapperCache = autoMapperCache ?? throw new ArgumentNullException(nameof(autoMapperCache));

            _mapperConfigurationLookupService = mapperConfigurationLookupService;
        }

        // Disallow the parameterless constructor.
        private SimpleAutoMapperService()
        {
            throw new NotSupportedException("Use of the paremeterless constructor for SimpleAutoMapperProvider is not supported.");
        }

        #endregion

        #region Public Methods

        // Gen Submit
        public IAutoMapperRequestKeyGen SubmitRawAutoMapperRequest
            (
            Type sourceType,
            Type destinationType,
            string configPackageName,
            IHaveAMapperConfigurationStep configStarterForThisRequest
            )
        {
            AutoMapperReqSubDelegate mapperRequestSubmitter = GetAutoMapperReqSubDelegate(sourceType, destinationType);
            IAutoMapperRequestKeyGen result = mapperRequestSubmitter(sourceType, destinationType, configPackageName, configStarterForThisRequest, this);

            return result;
        }

        public IAutoMapperRequestKey<TSource, TDestination> SubmitRawAutoMapperRequest<TSource, TDestination>
            (
            IMapTypeDefinition srcMapTypeDef,
            IMapTypeDefinition dstMapTypeDef,
            string configPackageName,
            IHaveAMapperConfigurationStep configStarterForThisRequest
            )
        {
            if(_mapperConfigurationLookupService == null)
            {
                throw new InvalidOperationException("The AutoMapperService has no MapperConfigurationLookup Service.");
            }

            // Lookup the package name and return a mapping configuration.
            IConfigureAMapper<TSource, TDestination> mappingConfiguration
                = _mapperConfigurationLookupService.GetTheMapperConfig<TSource, TDestination>(configPackageName);

            IAutoMapperRequestKey<TSource, TDestination> result
                = SubmitRawAutoMapperRequest
                (
                    srcMapTypeDef,
                    dstMapTypeDef,
                    mappingConfiguration,
                    configStarterForThisRequest
                    );

            return result;
        }

        // Typed Submit Raw Auto
        public IAutoMapperRequestKey<TSource, TDestination> SubmitRawAutoMapperRequest<TSource, TDestination>
            (
            IMapTypeDefinition srcMapTypeDef,
            IMapTypeDefinition dstMapTypeDef,
            IConfigureAMapper<TSource, TDestination> mappingConfiguration,
            IHaveAMapperConfigurationStep configStarterForThisRequest
            )
        {
            // Create a MapperBuilder for this request.
            IAutoMapperBuilder<TSource, TDestination> autoMapperBuilder
                = BuildTheAutoMapperBuilder<TSource, TDestination>(configStarterForThisRequest);

            // Create the mapper request.
            IAutoMapperRequestKey<TSource, TDestination> autoMapperRequestKey
                = new AutoMapperRequestKey<TSource, TDestination>
                (
                    autoMapperBuilder: autoMapperBuilder,
                    mappingConfiguration: mappingConfiguration,
                    sourceMapTypeDef: srcMapTypeDef,
                    destinationMapTypeDef: dstMapTypeDef
                );

            IAutoMapperRequestKeyGen response_AutoMapperRequestKey
                = RegisterRawAutoMapperRequest(autoMapperRequestKey);

            return (IAutoMapperRequestKey<TSource, TDestination>)response_AutoMapperRequestKey;
        }

        // Typed Get Mapper (Simple wrapper around our pass through method.)
        public IMapper GetRawAutoMapper<TSource, TDestination>
            (
            IAutoMapperRequestKey<TSource, TDestination> autoMapperRequestKey
            )
        {
            return _autoMapperCache.GetRawAutoMapper(autoMapperRequestKey);
        }

        private IAutoMapperBuilder<TSource, TDestination> BuildTheAutoMapperBuilder<TSource, TDestination>(IHaveAMapperConfigurationStep configStarterForThisRequest)
        {
            // TODO: check to make sure that the "configStarterForThisRequest" value is being sent to the correct place.

            // TODO: Make the method: GetAutoMapperBuilder on IAutoMapperBuilderProvider accept a
            // configStarterForThisRequest parameter.
            // Then create a new interface: IBuildMapperConfigurations_Provider
            // an implementation of this interface can be injected into the IBuildAutoMapper instance
            // so that the AutoMapperBuilder can create the ConfigurationBuilder for this request
            // and give it the configStarterForThisRequest value.

            // Create a Configuration Builder for this request.
            IMapperConfigurationBuilder<TSource, TDestination> propBagMapperConfigurationBuilder
                = new SimpleMapperConfigurationBuilder<TSource, TDestination>(configStarter: configStarterForThisRequest);

            // Create a MapperBuilder for this request.
            IAutoMapperBuilder<TSource, TDestination> autoMapperBuilder
                = _autoMapperBuilderProvider.GetAutoMapperBuilder<TSource, TDestination>
                (
                    propBagMapperConfigurationBuilder,
                    this
                );

            return autoMapperBuilder;
        }

        #endregion

        #region Pass-through calls to the Raw Auto MappersCache

        public IAutoMapperRequestKeyGen RegisterRawAutoMapperRequest(IAutoMapperRequestKeyGen autoMapperRequestKey)
        {
            return _autoMapperCache.RegisterRawAutoMapperRequest(autoMapperRequestKey);
        }

        public IMapper GetRawAutoMapper(IAutoMapperRequestKeyGen autoMapperRequestKey)
        {
            return _autoMapperCache.GetRawAutoMapper(autoMapperRequestKey);
        }

        public long ClearRawAutoMappersCache()
        {
            return _autoMapperCache.ClearRawAutoMappersCache();
        }

        #endregion

        #region Generic Method Support

        internal delegate IAutoMapperRequestKeyGen AutoMapperReqSubDelegate
            (
             Type sourceType,
             Type destinationType,
             string configPackageName,
             IHaveAMapperConfigurationStep configStarterForThisRequest,
             IAutoMapperService autoMapperService
            );

        static AutoMapperReqSubDelegate GetAutoMapperReqSubDelegate(Type sourceType, Type destinationType)
        {
            MethodInfo TypedRegisterRawAutoMapperRequest_MI = 
                GenericMethodTemplates.RawAutoMapperReqSubmitter_MI.MakeGenericMethod(sourceType, destinationType);

            AutoMapperReqSubDelegate result =
                (AutoMapperReqSubDelegate)Delegate.CreateDelegate(typeof(AutoMapperReqSubDelegate), TypedRegisterRawAutoMapperRequest_MI);
            return result;
        }

        /// <summary>
        /// Used to create Delegates when the type of the value is not known at run time.
        /// </summary>
        static private Type GMT_TYPE = typeof(GenericMethodTemplates);

        // Method Templates for Property Bag
        internal static class GenericMethodTemplates
        {
            static Lazy<MethodInfo> rawAutoMapperReqSubmitter_Lazy_MI;
            public static MethodInfo RawAutoMapperReqSubmitter_MI { get { return rawAutoMapperReqSubmitter_Lazy_MI.Value; } }

            static GenericMethodTemplates()
            {
                rawAutoMapperReqSubmitter_Lazy_MI = new Lazy<MethodInfo>(() =>
                    GMT_TYPE.GetMethod("SubmitRawAutoMapperRequest", BindingFlags.Static | BindingFlags.NonPublic),
                    LazyThreadSafetyMode.PublicationOnly);
            }

            // The Typed method for RawAutoMappers
            static IAutoMapperRequestKey<TSource, TDestination> SubmitRawAutoMapperRequest<TSource, TDestination>
                (
                Type sourceType,
                Type destinationType,
                string configPackageName,
                IHaveAMapperConfigurationStep configStarterForThisRequest,
                IAutoMapperService autoMapperProvider
                )
            {
                IMapTypeDefinition srcMapTypeDef = new MapTypeDefinition(typeof(TSource));
                IMapTypeDefinition dstMapTypeDef = new MapTypeDefinition(typeof(TDestination));

                IAutoMapperRequestKey<TSource, TDestination> result
                    = autoMapperProvider.SubmitRawAutoMapperRequest<TSource, TDestination>
                    (
                    srcMapTypeDef: srcMapTypeDef,
                    dstMapTypeDef: dstMapTypeDef,
                    configPackageName: configPackageName,
                    configStarterForThisRequest: configStarterForThisRequest
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
                    ClearRawAutoMappersCache();

                    if (_autoMapperCache is IDisposable disable1)
                    {
                        disable1.Dispose();
                    }

                    if (_autoMapperBuilderProvider is IDisposable disable2)
                    {
                        disable2.Dispose();
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
