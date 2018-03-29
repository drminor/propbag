using AutoMapper;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;
using System.Reflection;
using System.Threading;

namespace DRM.PropBag.AutoMapperSupport
{
    using PropModelType = IPropModel<String>;

    public class SimpleAutoMapperProvider : IAutoMapperService, IDisposable
    {
        #region Private Members

        private readonly IMapTypeDefinitionProvider _mapTypeDefinitionProvider;

        private readonly ICachePropBagMappers _propBagMappersCache;
        private readonly ICacheAutoMappers _rawAutoMapperCache;

        private readonly IPropBagMapperBuilderProvider _mapperBuilderProvider;

        #endregion

        #region Constructors

        // Disallow the parameterless constructor.
        private SimpleAutoMapperProvider()
        {
            throw new NotSupportedException("Use of the paremeterless constructor for SimpleAutoMapperProvider is not supported.");
        }

        public SimpleAutoMapperProvider
            (
            IMapTypeDefinitionProvider mapTypeDefinitionProvider,
            ICachePropBagMappers mappersCachingService,
            ICacheAutoMappers rawAutoMapperCache,
            IPropBagMapperBuilderProvider mapperBuilderProvider
            )
        {
            _mapTypeDefinitionProvider = mapTypeDefinitionProvider ?? throw new ArgumentNullException(nameof(mapTypeDefinitionProvider));

            _propBagMappersCache = mappersCachingService ?? throw new ArgumentNullException(nameof(mappersCachingService));
            _rawAutoMapperCache = rawAutoMapperCache ?? throw new ArgumentNullException(nameof(rawAutoMapperCache));

            _mapperBuilderProvider = mapperBuilderProvider ?? throw new ArgumentNullException(nameof(mapperBuilderProvider));
        }

        #endregion

        #region Public Methods

        public IPropBagMapperKeyGen SubmitPropBagMapperRequest(PropModelType propModel, Type sourceType, string configPackageName)
        {
            Type typeToCreate = propModel.NewEmittedType ?? propModel.TypeToWrap;
            MapperRequestSubmitterDelegate mapperRequestSubmitter = GetPropBagMapperReqSubDelegate(sourceType, typeToCreate);
            IPropBagMapperKeyGen result = mapperRequestSubmitter(propModel, typeToCreate, configPackageName, this);

            return result;
        }

        // TODO: Consider adding a method that takes a IConfigureAMapper instead of a configPackageName.
        public IPropBagMapperKey<TSource, TDestination> SubmitPropBagMapperRequest<TSource, TDestination>
            (
            PropModelType propModel,
            Type targetType,
            string configPackageName,
            IHaveAMapperConfigurationStep configStarterForThisRequest = null,
            IPropFactory propFactory = null
            )
            where TDestination : class, IPropBag
        {
            // TODO: check to make sure that the "configStarterForThisRequest" value is being sent to the correct place.

            // TODO: Consider making the caller supply a IBuildMapperConfigurations "service."


            // Create a Configuration Builder for this request.
            IBuildMapperConfigurations<TSource, TDestination> propBagMapperConfigurationBuilder
                = new SimpleMapperConfigurationBuilder<TSource, TDestination>(configStarter: configStarterForThisRequest);

            // Create a MapperBuilder for this request.
            IBuildPropBagMapper<TSource, TDestination> propBagMapperBuilder
                = _mapperBuilderProvider.GetPropBagMapperBuilder<TSource, TDestination>(propBagMapperConfigurationBuilder, this);

            // Lookup the package name and return a mapping configuration.
            IConfigureAMapper<TSource, TDestination> mappingConfiguration
                = GetMappingConfiguration<TSource, TDestination>(configPackageName);

            IMapTypeDefinition<TSource> srcMapTypeDef
                = _mapTypeDefinitionProvider.GetTypeDescription<TSource>(propModel/*, targetType*/, propFactory: propFactory, className: null);

            IMapTypeDefinition<TDestination> dstMapTypeDef
                = _mapTypeDefinitionProvider.GetTypeDescription<TDestination>(propModel/*, targetType*/, propFactory: propFactory, className: null);


            // Create the mapper request.
            IPropBagMapperKey<TSource, TDestination> typedMapperRequest
                = new PropBagMapperKey<TSource, TDestination>
                (
                    propBagMapperBuilder: propBagMapperBuilder,
                    mappingConfiguration: mappingConfiguration,
                    sourceMapTypeDef: srcMapTypeDef,
                    destinationMapTypeDef: dstMapTypeDef
                );

            IPropBagMapperKeyGen newMapRequest = RegisterPropBagMapperRequest(typedMapperRequest);
            return (IPropBagMapperKey<TSource, TDestination>)newMapRequest;
        }

        // TODO: Is this necessary -- Can't AutoMapper requests be manipulated using only the 'gen' interfaces?
        public IPropBagMapperKeyGen SubmitRawAutoMapperRequest(PropModelType propModel, Type sourceType, string configPackageName)
        {
            Type typeToCreate = propModel.NewEmittedType ?? propModel.TypeToWrap;
            MapperRequestSubmitterDelegate mapperRequestSubmitter = GetPropBagMapperReqSubDelegate(sourceType, typeToCreate);
            IPropBagMapperKeyGen result = mapperRequestSubmitter(propModel, typeToCreate, configPackageName, this);

            return result;
        }

        // TODO: Consider adding a method that takes a IConfigureAMapper instead of a configPackageName.
        public IPropBagMapperKey<TSource, TDestination> SubmitRawAutoMapperRequest<TSource, TDestination>
            (
            PropModelType propModel,

            Type targetType,
            string configPackageName,
            IHaveAMapperConfigurationStep configStarterForThisRequest = null,
            IPropFactory propFactory = null
            )
            where TDestination : class, IPropBag
        {
            //if (!(viewModelFactory is ViewModelFactoryInterface realVMFactory))
            //{
            //    throw new InvalidOperationException("The viewModelFactory argument is not an IViewModelFactory<L2T, L2TRaw>.");
            //}

            // TODO: check to make sure that the "configStarterForThisRequest" value is being sent to the correct place.

            // TODO: Consider making the caller supply a IBuildMapperConfigurations "service."


            // Create a Configuration Builder for this request.
            IBuildMapperConfigurations<TSource, TDestination> propBagMapperConfigurationBuilder
                = new SimpleMapperConfigurationBuilder<TSource, TDestination>(configStarter: configStarterForThisRequest);

            // Create a MapperBuilder for this request.
            IBuildPropBagMapper<TSource, TDestination> propBagMapperBuilder
                = _mapperBuilderProvider.GetPropBagMapperBuilder<TSource, TDestination>(propBagMapperConfigurationBuilder, this);

            // Lookup the package name and return a mapping configuration.
            IConfigureAMapper<TSource, TDestination> mappingConfiguration
                = GetMappingConfiguration<TSource, TDestination>(configPackageName);

            IMapTypeDefinition<TSource> srcMapTypeDef
                = _mapTypeDefinitionProvider.GetTypeDescription<TSource>(propModel/*, targetType*/, propFactory: propFactory, className: null);

            IMapTypeDefinition<TDestination> dstMapTypeDef
                = _mapTypeDefinitionProvider.GetTypeDescription<TDestination>(propModel/*, targetType*/, propFactory: propFactory, className: null);


            // Create the mapper request.
            IPropBagMapperKey<TSource, TDestination> typedMapperRequest
                = new PropBagMapperKey<TSource, TDestination>
                (
                    propBagMapperBuilder: propBagMapperBuilder,
                    mappingConfiguration: mappingConfiguration,
                    sourceMapTypeDef: srcMapTypeDef,
                    destinationMapTypeDef: dstMapTypeDef
                );

            IPropBagMapperKeyGen newMapRequest_raw = RegisterRawAutoMapperRequest(typedMapperRequest);
            return (IPropBagMapperKey<TSource, TDestination>)newMapRequest_raw;
        }

        #endregion

        #region Pass-through calls to the PropBag MappersCache

        public IPropBagMapperKeyGen RegisterPropBagMapperRequest(IPropBagMapperKeyGen mapperRequest)
        {
            return _propBagMappersCache.RegisterPropBagMapperRequest(mapperRequest);
        }

        public IPropBagMapper<TSource, TDestination> GetPropBagMapper<TSource, TDestination>(IPropBagMapperKey<TSource, TDestination> mapperRequest)
            where TDestination : class, IPropBag
        {
            return (IPropBagMapper<TSource, TDestination>)_propBagMappersCache.GetPropBagMapper(mapperRequest);
        }

        public IPropBagMapperGen GetPropBagMapper(IPropBagMapperKeyGen mapperRequest)
        {
            return _propBagMappersCache.GetPropBagMapper(mapperRequest);
        }

        public long ClearPropBagMappersCache()
        {
            return _propBagMappersCache.ClearPropBagMappersCache();
        }

        #endregion

        #region Pass-through calls to the Raw Auto MappersCache

        public IPropBagMapperKeyGen RegisterRawAutoMapperRequest(IPropBagMapperKeyGen mapperRequest)
        {
            return _rawAutoMapperCache.RegisterRawAutoMapperRequest(mapperRequest);
        }

        public IMapper GetRawAutoMapper<TSource, TDestination>(IPropBagMapperKey<TSource, TDestination> mapperRequest)
            where TDestination : class, IPropBag
        {
            return _rawAutoMapperCache.GetRawAutoMapper(mapperRequest);
        }

        public IMapper GetRawAutoMapper(IPropBagMapperKeyGen mapperRequest)
        {
            return _rawAutoMapperCache.GetRawAutoMapper(mapperRequest);
        }

        public long ClearRawAutoMappersCache()
        {
            return _rawAutoMapperCache.ClearRawAutoMappersCache();
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

        #endregion

        #region Generic Method Support

        internal delegate IPropBagMapperKeyGen MapperRequestSubmitterDelegate
            (PropModelType propModel, Type targetType, string configPackageName, IAutoMapperService autoMapperService);

        static MapperRequestSubmitterDelegate GetPropBagMapperReqSubDelegate(Type sourceType, Type destinationType)
        {
            MethodInfo TypedRegisterMapperRequest_MI = GenericMethodTemplates.PropBagMapperReqSubmitter_MI.MakeGenericMethod(sourceType, destinationType);
            MapperRequestSubmitterDelegate result = (MapperRequestSubmitterDelegate)Delegate.CreateDelegate(typeof(MapperRequestSubmitterDelegate), TypedRegisterMapperRequest_MI);

            return result;
        }

        static MapperRequestSubmitterDelegate GetAutoMapperReqSubDelegate(Type sourceType, Type destinationType)
        {
            MethodInfo TypedRegisterRawAutoMapperRequest_MI = GenericMethodTemplates.RawAutoMapperReqSubmitter_MI.MakeGenericMethod(sourceType, destinationType);
            MapperRequestSubmitterDelegate result = (MapperRequestSubmitterDelegate)Delegate.CreateDelegate(typeof(MapperRequestSubmitterDelegate), TypedRegisterRawAutoMapperRequest_MI);

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

            static Lazy<MethodInfo> rawAutoMapperReqSubmitter_Lazy_MI;
            public static MethodInfo RawAutoMapperReqSubmitter_MI { get { return rawAutoMapperReqSubmitter_Lazy_MI.Value; } }


            static GenericMethodTemplates()
            {
                propBagMapperReqSubmitter_Lazy_MI = new Lazy<MethodInfo>(() =>
                    GMT_TYPE.GetMethod("SubmitPropBagMapperRequest", BindingFlags.Static | BindingFlags.NonPublic),
                    LazyThreadSafetyMode.PublicationOnly);

                rawAutoMapperReqSubmitter_Lazy_MI = new Lazy<MethodInfo>(() =>
                    GMT_TYPE.GetMethod("SubmitRawAutoMapperRequest", BindingFlags.Static | BindingFlags.NonPublic),
                    LazyThreadSafetyMode.PublicationOnly);
            }

            // The Typed Method for PropBagMappers
            static IPropBagMapperKey<TSource, TDestination> SubmitPropBagMapperRequest<TSource, TDestination>
                (
                PropModelType propModel,
                Type targetType,
                string configPackageName,
                IAutoMapperService autoMapperProvider
                )
                where TDestination : class, IPropBag
            {

                IPropBagMapperKey<TSource, TDestination> result
                    = autoMapperProvider.SubmitPropBagMapperRequest<TSource, TDestination>
                    (
                        propModel: propModel,
                        typeToWrap: targetType,
                        configPackageName: configPackageName,
                        configStarterForThisRequest: null,
                        propFactory: null
                        );

                return result;
            }


            // The Typed method for RawAutoMappers
            static IPropBagMapperKey<TSource, TDestination> SubmitRawAutoMapperRequest<TSource, TDestination>
                (
                PropModelType propModel,
                Type targetType,
                string configPackageName,
                IAutoMapperService autoMapperProvider
                )
                where TDestination : class, IPropBag
            {

                IPropBagMapperKey<TSource, TDestination> result
                    = autoMapperProvider.SubmitRawAutoMapperRequest<TSource, TDestination>
                        (
                        propModel: propModel,
                        typeToWrap: targetType,
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

                    if(_mapTypeDefinitionProvider is IDisposable disable)
                    {
                        disable.Dispose();
                    }

                    if(_propBagMappersCache is IDisposable disable2)
                    {
                        disable2.Dispose();
                    }


                    if(_mapperBuilderProvider is IDisposable disable3)
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
