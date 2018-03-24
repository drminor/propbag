using DRM.PropBag.TypeWrapper;
using DRM.TypeSafePropertyBag;
using System;
using System.Reflection;
using System.Threading;

namespace DRM.PropBag.AutoMapperSupport
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    public class SimpleAutoMapperProvider : IProvideAutoMappers, IDisposable
    {
        #region Private Members

        private readonly IMapTypeDefinitionProvider _mapTypeDefinitionProvider;
        private readonly ICachePropBagMappers _mappersCachingService;
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
            IPropBagMapperBuilderProvider mapperBuilderProvider
            )
        {
            _mapTypeDefinitionProvider = mapTypeDefinitionProvider ?? throw new ArgumentNullException(nameof(mapTypeDefinitionProvider));
            _mappersCachingService = mappersCachingService ?? throw new ArgumentNullException(nameof(mappersCachingService));
            _mapperBuilderProvider = mapperBuilderProvider ?? throw new ArgumentNullException(nameof(mapperBuilderProvider));
        }

        #endregion

        //#region Public Properties

        //public ICreateWrapperTypes WrapperTypeCreator => _mapperBuilderProvider.WrapperTypeCreator;

        //#endregion

        #region Public Methods

        public IPropBagMapperKeyGen SubmitMapperRequest(PropModelType propModel, Type sourceType, string configPackageName)
        {
            Type typeToCreate = propModel.NewEmittedType ?? propModel.TypeToCreate;

            MapperRequestSubmitterDelegate mapperRequestSubmitter = GetTheMapperRequestSubmitterDelegate(sourceType, typeToCreate);

            IPropBagMapperKeyGen result = mapperRequestSubmitter(propModel, typeToCreate, configPackageName, this);

            return result;
        }

        // TODO: Consider adding a method that takes a IConfigureAMapper instead of a configPackageName.
        public IPropBagMapperKey<TSource, TDestination> SubmitMapperRequest<TSource, TDestination>
            (
            PropModelType propModel,
            Type targetType,
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
                = _mapperBuilderProvider.GetPropBagMapperBuilder<TSource, TDestination>(propBagMapperConfigurationBuilder, this);

            // Lookup the package name and return a mapping configuration.
            IConfigureAMapper<TSource, TDestination> mappingConfiguration
                = GetMappingConfiguration<TSource, TDestination>(configPackageName);

            IMapTypeDefinition<TSource> srcMapTypeDef
                = _mapTypeDefinitionProvider.GetTypeDescription<TSource>(propModel, targetType, propFactory: propFactory, className: null);

            IMapTypeDefinition<TDestination> dstMapTypeDef
                = _mapTypeDefinitionProvider.GetTypeDescription<TDestination>(propModel, targetType, propFactory: propFactory, className: null);


            // Create the mapper request.
            IPropBagMapperKey<TSource, TDestination> typedMapperRequest
                = new PropBagMapperKey<TSource, TDestination>
                (
                    propBagMapperBuilder: propBagMapperBuilder,
                    mappingConfiguration: mappingConfiguration,
                    sourceMapTypeDef: srcMapTypeDef,
                    destinationMapTypeDef: dstMapTypeDef
                );

            IPropBagMapperKeyGen newMapRequest = RegisterMapperRequest(typedMapperRequest);

            return (IPropBagMapperKey<TSource, TDestination>) newMapRequest;
        }

        //public long ClearEmittedTypeCache()
        //{
        //    return _mapperBuilderProvider.ClearTypeCache();
        //}

        public void ClearCaches()
        {
            ClearMappersCache();
            //ClearEmittedTypeCache();
        }

        #endregion

        #region Pass-through calls to the MappersCachingService

        public IPropBagMapper<TSource,TDestination> GetMapper<TSource, TDestination>(IPropBagMapperKey<TSource, TDestination> mapperRequest) where TDestination : class, IPropBag
        {
            return (IPropBagMapper<TSource, TDestination>)_mappersCachingService.GetMapper(mapperRequest);
        }

        public IPropBagMapperKeyGen RegisterMapperRequest(IPropBagMapperKeyGen mapperRequest)
        {
            return _mappersCachingService.RegisterMapperRequest(mapperRequest);
        }

        public IPropBagMapperGen GetMapper(IPropBagMapperKeyGen mapperRequest)
        {
            return _mappersCachingService.GetMapper(mapperRequest);
        }

        public long ClearMappersCache()
        {
            return _mappersCachingService.ClearMappersCache();
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

        static MapperRequestSubmitterDelegate GetTheMapperRequestSubmitterDelegate(Type sourceType, Type destinationType)
        {
            MethodInfo TypedRegisterMapperRequest_MI = GenericMethodTemplates.GenMapperRequestSubmitter_MI.MakeGenericMethod(sourceType, destinationType);
            MapperRequestSubmitterDelegate result = (MapperRequestSubmitterDelegate)Delegate.CreateDelegate(typeof(MapperRequestSubmitterDelegate), TypedRegisterMapperRequest_MI);

            return result;
        }

        internal delegate IPropBagMapperKeyGen MapperRequestSubmitterDelegate
            (PropModelType propModel, Type targetType, string configPackageName, IProvideAutoMappers autoMapperProvider);

        /// <summary>
        /// Used to create Delegates when the type of the value is not known at run time.
        /// </summary>
        static private Type GMT_TYPE = typeof(GenericMethodTemplates);

        // Method Templates for Property Bag
        internal static class GenericMethodTemplates
        {
            static Lazy<MethodInfo> theSingleGenMapperRequestSubmitter_MI;
            public static MethodInfo GenMapperRequestSubmitter_MI { get { return theSingleGenMapperRequestSubmitter_MI.Value; } }

            static GenericMethodTemplates()
            {
                theSingleGenMapperRequestSubmitter_MI = new Lazy<MethodInfo>(() =>
                    GMT_TYPE.GetMethod("SubmitMapperRequest", BindingFlags.Static | BindingFlags.NonPublic),
                    LazyThreadSafetyMode.PublicationOnly);
            }

            // The Typed Method
            static IPropBagMapperKey<TSource, TDestination> SubmitMapperRequest<TSource, TDestination>
                (PropModelType propModel, Type targetType, string configPackageName, IProvideAutoMappers autoMapperProvider) where TDestination : class, IPropBag
            {

                IPropBagMapperKey<TSource, TDestination> result
                    = autoMapperProvider.SubmitMapperRequest<TSource, TDestination>
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
                    ClearCaches();

                    if(_mapTypeDefinitionProvider is IDisposable disable)
                    {
                        disable.Dispose();
                    }

                    if(_mappersCachingService is IDisposable disable2)
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
