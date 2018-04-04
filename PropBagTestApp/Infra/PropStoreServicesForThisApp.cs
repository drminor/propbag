using DRM.PropBag;
using DRM.PropBag.Caches;
using DRM.PropBag.TypeWrapper;
using DRM.PropBag.TypeWrapper.TypeDesc;
using DRM.PropBag.ViewModelTools;
using DRM.PropBagControlsWPF;
using DRM.PropBagWPF;
using DRM.TypeSafePropertyBag;
using Swhp.Tspb.PropBagAutoMapperService;
using Swhp.AutoMapperSupport;
using System;
using System.ComponentModel;
using System.Windows;


namespace PropBagTestApp.Infra
{
    using PropModelCacheInterface = ICachePropModels<String>;
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;
    using ViewModelActivatorInterface = IViewModelActivator<UInt32, String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public static class PropStoreServicesForThisApp
    {
        public static int MAX_NUMBER_OF_PROPERTIES = 65536;

        private readonly static SimplePropStoreProxy _theStore;

        public readonly static IProvideHandlerDispatchDelegateCaches HandlerDispatchDelegateCacheProvider;

        private static IPropFactoryFactory _propFactoryFactory { get; }
        private static ViewModelActivatorInterface _vmActivator { get; }
        //private static IPropFactory _defaultPropFactory { get; }

        public static PropModelCacheInterface PropModelCache { get; private set; }

        public static SimpleViewModelFactory ViewModelFactory { get; private set; }
        public static ICreateWrapperTypes WrapperTypeCreator { get; }
        public static IPropBagMapperService PropBagMapperService { get; }

        public static string ConfigPackageNameSuffix { get; set; }

        //static PropStoreServicesForThisApp()
        //{
        //    // Create the 3 core caches
        //    ITypeDescBasedTConverterCache typeDescBasedTConverterCache = new TypeDescBasedTConverterCache();
        //    IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates));
        //    IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();

        //    _theStore = new SimplePropStoreProxy(MAX_NUMBER_OF_PROPERTIES, handlerDispatchDelegateCacheProvider);

        //    // Get a reference to the PropStoreAccessService Factory.
        //    PSAccessServiceCreatorInterface psAccessServiceFactory = _theStore.PropStoreAccessServiceFactory;

        //    ViewModelActivatorInterface vmActivator = new SimpleViewModelActivator();

        //    WrapperTypeCreator = BuildSimpleWrapperTypeCreator();


        //    IConvertValues valueConverter = new PropFactoryValueConverter(typeDescBasedTConverterCache);

        //    // Default PropFactory
        //    DefaultPropFactory = BuildDefaultPropFactory
        //        (
        //        valueConverter,
        //        delegateCacheProvider
        //        );

        //    // The Factory used to build PropFactories.
        //    IPropFactoryFactory propFactoryFactory = BuildThePropFactoryFactory
        //        (
        //        valueConverter,
        //        delegateCacheProvider
        //        );

        //    IProvidePropModels propModelBuilder = GetPropModelProvider(propFactoryFactory);

        //    PropModelCache = new SimplePropModelCache(propModelBuilder);

        //    ViewModelFactory = new SimpleViewModelFactory(PropModelCache, vmActivator, psAccessServiceFactory, null, WrapperTypeCreator);

        //    PropBagMapperService = GetAutoMapperProvider(ViewModelFactory);

        //    ViewModelFactory.AutoMapperService = PropBagMapperService;
        //}

        static PropStoreServicesForThisApp()
        {
            // Build the PropFactory Provider and the two Delegate Caches on which it depends.
            ITypeDescBasedTConverterCache typeDescBasedTConverterCache = new TypeDescBasedTConverterCache();

            IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates));

            IConvertValues valueConverter = new PropFactoryValueConverter(typeDescBasedTConverterCache);
            ResolveTypeDelegate typeResolver = null;

            _propFactoryFactory = BuildThePropFactoryFactory(valueConverter, delegateCacheProvider, typeResolver);

            // Build the AppDomain-Wide Property Store and the EventHandler (Delegate) Dispatch cache on which it depends.
            HandlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();

            // This creates the global, shared, Property Store.
            _theStore = new SimplePropStoreProxy(MAX_NUMBER_OF_PROPERTIES, HandlerDispatchDelegateCacheProvider);

            // Get a reference to the PropStoreAccessService Factory that the PropertyStore provides.
            PSAccessServiceCreatorInterface psAccessServiceFactory = _theStore.PropStoreAccessServiceFactory;

            _vmActivator = new SimpleViewModelActivator();


            IPropModelBuilder propModelBuilder = GetPropModelProvider(_propFactoryFactory);

            PropModelCache = new SimplePropModelCache(propModelBuilder);

            WrapperTypeCreator = BuildSimpleWrapperTypeCreator();

            ViewModelFactory = new SimpleViewModelFactory(PropModelCache, _vmActivator, psAccessServiceFactory, null, WrapperTypeCreator);

            PropBagMapperService = GetAutoMapperProvider(ViewModelFactory);

            ViewModelFactory.AutoMapperService = PropBagMapperService;

            //_defaultPropFactory = BuildDefaultPropFactory(valueConverter, delegateCacheProvider, typeResolver);
            //_mct.MeasureAndReport("After new BuildDefaultPropFactory");
        }

        private static IPropFactoryFactory BuildThePropFactoryFactory
        (
            IConvertValues valueConverter,
            IProvideDelegateCaches delegateCacheProvider,
            ResolveTypeDelegate typeResolver
        )
        {
            IPropFactoryFactory result = new SimplePropFactoryFactory
                (
                delegateCacheProvider,
                valueConverter,
                typeResolver: typeResolver
                );

            return result;
        }

        private static IPropFactory BuildDefaultPropFactory
            (
            IConvertValues valueConverter,
            IProvideDelegateCaches delegateCacheProvider
            )
        {
            ResolveTypeDelegate typeResolver = null;

            IPropFactory result = new WPFPropFactory
                (
                delegateCacheProvider: delegateCacheProvider,
                valueConverter: valueConverter,
                typeResolver: typeResolver
                );

            return result;
        }


        private static IPropModelBuilder GetPropModelProvider
            (
            IPropFactoryFactory propFactoryFactory
            )
        {
            IPropBagTemplateBuilder propBagTemplateBuilder = new PropBagTemplateBuilder(Application.Current.Resources);

            IMapperRequestBuilder mapperRequestBuilder = new MapperRequestBuilder(Application.Current.Resources);

            IParsePropBagTemplates propBagTemplateParser = new PropBagTemplateParser();

            IPropModelBuilder propModelBuilder = new SimplePropModelBuilder
                (
                propBagTemplateBuilder,
                mapperRequestBuilder,
                propBagTemplateParser,
                propFactoryFactory
                );

            return propModelBuilder;
        }

        private static IPropBagMapperService GetAutoMapperProvider(ViewModelFactoryInterface viewModelFactory)
        {
            IAutoMapperBuilderProvider autoMapperBuilderProvider = new SimpleAutoMapperBuilderProvider();
            IAutoMapperCache rawAutoMapperCache = new SimpleAutoMapperCache();
            SimpleAutoMapperService autoMapperService = new SimpleAutoMapperService
            (
                autoMapperBuilderProvider: autoMapperBuilderProvider,
                autoMapperCache: rawAutoMapperCache,
                mapperConfigurationLookupService: null
            );

            IMapTypeDefinitionProvider mapTypeDefinitionProvider = new SimpleMapTypeDefinitionProvider();

            IPropBagMapperBuilderProvider propBagMapperBuilderProvider = new SimplePropBagMapperBuilderProvider();
            ICachePropBagMappers mappersCachingService = new SimplePropBagMapperCache(viewModelFactory);
            IPropBagMapperService propBagMapperService = new SimplePropBagMapperService
            (
                mapTypeDefinitionProvider: mapTypeDefinitionProvider,
                mapperBuilderProvider: propBagMapperBuilderProvider,
                mappersCachingService: mappersCachingService,
                autoMapperService: autoMapperService
            );

            return propBagMapperService;
        }

        //private static ICreateWrapperTypes BuildSimpleWrapperTypeCreator()
        //{
        //    // -- Build WrapperType Caching Service
        //    // Used by some ViewModel Activators to emit types, i.e., modules.
        //    IModuleBuilderInfo moduleBuilderInfo = new SimpleModuleBuilderInfo();

        //    IEmitWrapperType emitWrapperType = new SimpleWrapperTypeEmitter
        //        (
        //        mbInfo: moduleBuilderInfo
        //        );

        //    ICacheEmittedTypes wrapperTypeCachingService = new SimpleEmittedTypesCache
        //        (
        //        emitterEngine: emitWrapperType
        //        );

        //    // -- Build TypeDesc Caching Service
        //    // Used only by some ModuleBuilders.
        //    ITypeDescriptionProvider typeDescriptionProvider = new SimpleTypeDescriptionProvider();

        //    ICacheTypeDescriptions typeDescCachingService = new TypeDescriptionLocalCache
        //        (
        //        typeDescriptionProvider: typeDescriptionProvider
        //        );

        //    ICreateWrapperTypes result = new SimpleWrapperTypeCreator
        //        (
        //        wrapperTypeCachingService: wrapperTypeCachingService,
        //        typeDescCachingService: typeDescCachingService
        //        );

        //    return result;
        //}

        private static ICreateWrapperTypes BuildSimpleWrapperTypeCreator()
        {
            // -- Build WrapperType Caching Service
            // Used by some ViewModel Activators to emit types, i.e., modules.
            IModuleBuilderInfo moduleBuilderInfo = new SimpleModuleBuilderInfo();

            IEmitWrapperType emitWrapperType = new SimpleWrapperTypeEmitter
                (
                mbInfo: moduleBuilderInfo
                );

            ICacheEmittedTypes wrapperTypeCachingService = new SimpleEmittedTypesCache
                (
                emitterEngine: emitWrapperType
                );

            // -- Build TypeDesc Caching Service
            // Used only by some ModuleBuilders.
            ITypeDescriptionProvider typeDescriptionProvider = new SimpleTypeDescriptionProvider();

            ICacheTypeDescriptions typeDescCachingService = new TypeDescriptionLocalCache
                (
                typeDescriptionProvider: typeDescriptionProvider
                );

            ICreateWrapperTypes result = new SimpleWrapperTypeCreator
                (
                wrapperTypeCachingService: wrapperTypeCachingService,
                typeDescCachingService: typeDescCachingService
                );

            return result;
        }



        public static string GetResourceKeyWithSuffix(string rawKey, string suffix)
        {
            string result = suffix != null ? $"{rawKey}_{suffix}" : rawKey;
            return result;
        }


        public static IPropBagMapper<TSource, TDestination> GetAutoMapper<TSource, TDestination>
        (
            IMapperRequest mapperRequest,
            IPropBagMapperService propBagMapperService,
            out IPropBagMapperRequestKey<TSource, TDestination> propBagMapperRequestKey
        )
        where TDestination : class, IPropBag
        {
            // TODO: See if we can submit the request earlier; perhaps when the mapper request is created.
            //Type typeToWrap = mapperRequest.PropModel.TypeToWrap;

            // Submit the Mapper Request.
            propBagMapperRequestKey = propBagMapperService.SubmitPropBagMapperRequest<TSource, TDestination>
                (mapperRequest.PropModel, mapperRequest.ConfigPackageName);

            // Get the AutoMapper mapping function associated with the mapper request just submitted.
            IPropBagMapper<TSource, TDestination> propBagMapper = propBagMapperService.GetPropBagMapper<TSource, TDestination>(propBagMapperRequestKey);
            return propBagMapper;
        }

        #region InDesign Support

        public static bool InDesignMode() => _isInDesignMode.HasValue && _isInDesignMode == true;

        public static bool? _isInDesignMode;

        public static bool IsInDesignModeStatic
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
                    var prop = DesignerProperties.IsInDesignModeProperty;
                    _isInDesignMode
                        = (bool)DependencyPropertyDescriptor
                                        .FromProperty(prop, typeof(FrameworkElement))
                                        .Metadata.DefaultValue;
                }

                return _isInDesignMode.Value;
            }
        }

        #endregion
    }
}
