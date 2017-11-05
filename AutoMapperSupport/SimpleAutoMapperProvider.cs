using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using DRM.ViewModelTools;
using System;
using System.Reflection;
using System.Threading;

namespace DRM.PropBag.AutoMapperSupport
{
    public class SimpleAutoMapperProvider : ICachePropBagMappers, IProvideAutoMappers
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

        private SimpleAutoMapperProvider() { } // Disallow the parameterless constructor.

        public SimpleAutoMapperProvider
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

        public IPropBagMapperKeyGen RegisterMapperRequest(MapperRequest mr)
        {
            PropModel propModel = PropModelProvider.GetPropModel(mr.PropModelResourceKey);
            Type targetType = propModel.TargetType;

            RegisterMapperRequestDelegate x = GetTheRegisterMapperRequestDelegate(mr.SourceType, targetType);
            IPropBagMapperKeyGen result = x(propModel, targetType, mr.ConfigPackageName, this);

            return result;
        }

        public IPropBagMapperKey<TSource, TDestination> RegisterMapperRequest<TSource, TDestination>
            (
            string resourceKey,
            Type targetType,
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
                    targetType,
                    configPackageName,
                    configStarterForThisRequest,
                    propFactory);

            return typedMapperRequest;
        }

        // TODO: Consider adding a method that takes a IConfigureAMapper instead of a configPackageName.
        public IPropBagMapperKey<TSource, TDestination> RegisterMapperRequest<TSource, TDestination>
            (
            PropModel propModel,
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
                = MapperBuilderProvider.GetPropBagMapperBuilder<TSource, TDestination>(propBagMapperConfigurationBuilder);

            // Lookup the package name and return a mapping configuration.
            IConfigureAMapper<TSource, TDestination> mappingConfiguration
                = GetMappingConfiguration<TSource, TDestination>(configPackageName);

            IMapTypeDefinition<TSource> srcMapTypeDef
                = MapTypeDefinitionProvider.GetTypeDescription<TSource>(propModel, targetType, className: null, propFactory: propFactory);

            IMapTypeDefinition<TDestination> dstMapTypeDef
                = MapTypeDefinitionProvider.GetTypeDescription<TDestination>(propModel, targetType, className: null, propFactory: propFactory);


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

        #endregion

        #region Generic Method Support

        static RegisterMapperRequestDelegate GetTheRegisterMapperRequestDelegate(Type sourceType, Type destinationType)
        {
            MethodInfo TypedRegisterMapperRequest_MI = GenericMethodTemplates.GenRegisterMapperRequest_MI.MakeGenericMethod(sourceType, destinationType);
            RegisterMapperRequestDelegate result = (RegisterMapperRequestDelegate)Delegate.CreateDelegate(typeof(RegisterMapperRequestDelegate), TypedRegisterMapperRequest_MI);

            return result;
        }

        internal delegate IPropBagMapperKeyGen RegisterMapperRequestDelegate
            (PropModel propModel, Type targetType, string configPackageName, IProvideAutoMappers autoMapperProvider);

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
                (PropModel propModel, Type targetType, string configPackageName, IProvideAutoMappers autoMapperProvider) where TDestination : class, IPropBag
            {

                IPropBagMapperKey<TSource, TDestination> result
                    = autoMapperProvider.RegisterMapperRequest<TSource, TDestination>
                    (
                        propModel: propModel,
                        targetType: targetType,
                        configPackageName: configPackageName,
                        configStarterForThisRequest: null,
                        propFactory: null
                        );

                return result;
            }
        }

        #endregion
    }
}
