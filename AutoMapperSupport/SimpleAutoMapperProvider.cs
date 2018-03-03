using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;
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

        IMapTypeDefinitionProvider MapTypeDefinitionProvider { get; }
        ICachePropBagMappers MappersCachingService { get; }
        IPropBagMapperBuilderProvider MapperBuilderProvider { get; }

        #endregion

        #region Constructors

        private SimpleAutoMapperProvider() { } // Disallow the parameterless constructor.

        public SimpleAutoMapperProvider
            (
            IMapTypeDefinitionProvider mapTypeDefinitionProvider,
            ICachePropBagMappers mappersCachingService,
            IPropBagMapperBuilderProvider mapperBuilderProvider
            )
        {
            MapTypeDefinitionProvider = mapTypeDefinitionProvider ?? throw new ArgumentNullException(nameof(mapTypeDefinitionProvider));
            MappersCachingService = mappersCachingService ?? throw new ArgumentNullException(nameof(mappersCachingService));
            MapperBuilderProvider = mapperBuilderProvider ?? throw new ArgumentNullException(nameof(mapperBuilderProvider));
        }

        #endregion

        #region Public Methods

        public IPropBagMapperKeyGen RegisterMapperRequest(PropModelType propModel, Type sourceType, string configPackageName)
        {
            Type targetType = propModel.TargetType;

            RegisterMapperRequestDelegate submitMapperRequest = GetTheRegisterMapperRequestDelegate(sourceType, targetType);
            IPropBagMapperKeyGen result = submitMapperRequest(propModel, targetType, configPackageName, this);

            return result;
        }

        // TODO: Consider adding a method that takes a IConfigureAMapper instead of a configPackageName.
        public IPropBagMapperKey<TSource, TDestination> RegisterMapperRequest<TSource, TDestination>
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
                = MapperBuilderProvider.GetPropBagMapperBuilder<TSource, TDestination>(propBagMapperConfigurationBuilder, this);

            // Lookup the package name and return a mapping configuration.
            IConfigureAMapper<TSource, TDestination> mappingConfiguration
                = GetMappingConfiguration<TSource, TDestination>(configPackageName);

            IMapTypeDefinition<TSource> srcMapTypeDef
                = MapTypeDefinitionProvider.GetTypeDescription<TSource>(propModel, targetType, propFactory: propFactory, className: null);

            IMapTypeDefinition<TDestination> dstMapTypeDef
                = MapTypeDefinitionProvider.GetTypeDescription<TDestination>(propModel, targetType, propFactory: propFactory, className: null);


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

        public long ClearEmittedTypeCache()
        {
            return MapperBuilderProvider.ClearTypeCache();
        }

        public long ClearMappersCache()
        {
            return MappersCachingService.ClearMappersCache();
        }

        public void ClearCaches()
        {
            MappersCachingService.ClearMappersCache();
            MapperBuilderProvider.ClearTypeCache();
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

        static RegisterMapperRequestDelegate GetTheRegisterMapperRequestDelegate(Type sourceType, Type destinationType)
        {
            MethodInfo TypedRegisterMapperRequest_MI = GenericMethodTemplates.GenRegisterMapperRequest_MI.MakeGenericMethod(sourceType, destinationType);
            RegisterMapperRequestDelegate result = (RegisterMapperRequestDelegate)Delegate.CreateDelegate(typeof(RegisterMapperRequestDelegate), TypedRegisterMapperRequest_MI);

            return result;
        }

        internal delegate IPropBagMapperKeyGen RegisterMapperRequestDelegate
            (PropModelType propModel, Type targetType, string configPackageName, IProvideAutoMappers autoMapperProvider);

        /// <summary>
        /// Used to create Delegates when the type of the value is not known at run time.
        /// </summary>
        static private Type GMT_TYPE = typeof(GenericMethodTemplates);

        // Method Templates for Property Bag
        internal static class GenericMethodTemplates
        {
            static Lazy<MethodInfo> theSingleGenRegisterMapperRequest_MI;
            public static MethodInfo GenRegisterMapperRequest_MI { get { return theSingleGenRegisterMapperRequest_MI.Value; } }

            static GenericMethodTemplates()
            {
                theSingleGenRegisterMapperRequest_MI = new Lazy<MethodInfo>(() =>
                    GMT_TYPE.GetMethod("RegisterMapperRequest", BindingFlags.Static | BindingFlags.NonPublic),
                    LazyThreadSafetyMode.PublicationOnly);
            }

            // The Typed Method
            static IPropBagMapperKey<TSource, TDestination> RegisterMapperRequest<TSource, TDestination>
                (PropModelType propModel, Type targetType, string configPackageName, IProvideAutoMappers autoMapperProvider) where TDestination : class, IPropBag
            {

                IPropBagMapperKey<TSource, TDestination> result
                    = autoMapperProvider.RegisterMapperRequest<TSource, TDestination>
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

                    if(MapTypeDefinitionProvider is IDisposable disable)
                    {
                        disable.Dispose();
                    }

                    if(MappersCachingService is IDisposable disable2)
                    {
                        disable2.Dispose();
                    }


                    if(MapperBuilderProvider is IDisposable disable3)
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
