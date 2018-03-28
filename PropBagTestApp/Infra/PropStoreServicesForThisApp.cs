using AutoMapper;
using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.Caches;
using DRM.PropBag.TypeWrapper;
using DRM.PropBag.TypeWrapper.TypeDesc;
using DRM.PropBag.ViewModelTools;
using DRM.PropBagControlsWPF;
using DRM.PropBagWPF;
using DRM.TypeSafePropertyBag;
using System;
using System.ComponentModel;
using System.Windows;

namespace PropBagTestApp.Infra
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;
    using PropModelCacheInterface = ICachePropModels<String>;
    using ViewModelActivatorInterface = IViewModelActivator<UInt32, String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public static class PropStoreServicesForThisApp
    {
        private readonly static SimplePropStoreProxy _theStore;

        public static int MAX_NUMBER_OF_PROPERTIES = 65536;

        public static IPropFactory DefaultPropFactory { get; }

        public static PropModelCacheInterface PropModelCache { get; set; }

        public static SimpleViewModelFactory ViewModelFactory { get; }
        public static IAutoMapperService AutoMapperService { get; }

        public static ICreateWrapperTypes WrapperTypeCreator { get; }

        public static string ConfigPackageNameSuffix { get; set; }

        static PropStoreServicesForThisApp()
        {
            // Create the 3 core caches
            ITypeDescBasedTConverterCache typeDescBasedTConverterCache = new TypeDescBasedTConverterCache();
            IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates));
            IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();

            _theStore = new SimplePropStoreProxy(MAX_NUMBER_OF_PROPERTIES, handlerDispatchDelegateCacheProvider);

            // Get a reference to the PropStoreAccessService Factory.
            PSAccessServiceCreatorInterface psAccessServiceFactory = _theStore.PropStoreAccessServiceFactory;

            ViewModelActivatorInterface vmActivator = new SimpleViewModelActivator();

            WrapperTypeCreator = BuildSimpleWrapperTypeCreator();


            IConvertValues valueConverter = new PropFactoryValueConverter(typeDescBasedTConverterCache);

            // Default PropFactory
            DefaultPropFactory = BuildDefaultPropFactory
                (
                valueConverter,
                delegateCacheProvider
                );

            // The Factory used to build PropFactories.
            IPropFactoryFactory propFactoryFactory = BuildThePropFactoryFactory
                (
                valueConverter,
                delegateCacheProvider
                );

            IProvidePropModels propModelProvider = GetPropModelProvider(propFactoryFactory);

            PropModelCache = new SimplePropModelCache(propModelProvider);

            ViewModelFactory = new SimpleViewModelFactory(PropModelCache, vmActivator, psAccessServiceFactory, null, WrapperTypeCreator);

            AutoMapperService = GetAutoMapperProvider(ViewModelFactory);

            ViewModelFactory.AutoMapperService = AutoMapperService;
        }

        private static IPropFactoryFactory BuildThePropFactoryFactory
            (
            IConvertValues valueConverter,
            IProvideDelegateCaches delegateCacheProvider
            )
        {
            ResolveTypeDelegate typeResolver = null;

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


        private static IProvidePropModels GetPropModelProvider
            (
            IPropFactoryFactory propFactoryFactory
            )
        {
            IPropBagTemplateProvider propBagTemplateProvider = new PropBagTemplateProvider(Application.Current.Resources);

            IMapperRequestProvider mapperRequestProvider = new MapperRequestProvider(Application.Current.Resources);

            IParsePropBagTemplates propBagTemplateParser = new PropBagTemplateParser();

            IProvidePropModels propModelProvider = new SimplePropModelProvider
                (
                propBagTemplateProvider,
                mapperRequestProvider,
                propBagTemplateParser,
                propFactoryFactory
                );

            return propModelProvider;
        }

        private static IAutoMapperService GetAutoMapperProvider(ViewModelFactoryInterface viewModelFactory)
        {
            IMapTypeDefinitionProvider mapTypeDefinitionProvider = new SimpleMapTypeDefinitionProvider();

            ICachePropBagMappers mappersCachingService = new SimplePropBagMapperCache(viewModelFactory);
            ICacheAutoMappers rawAutoMapperCache = new SimpleAutoMapperCache();

            IPropBagMapperBuilderProvider propBagMapperBuilderProvider = new SimplePropBagMapperBuilderProvider();

            SimpleAutoMapperProvider autoMapperProvider = new SimpleAutoMapperProvider
                (
                mapTypeDefinitionProvider: mapTypeDefinitionProvider,
                mappersCachingService: mappersCachingService,
                rawAutoMapperCache: rawAutoMapperCache,
                mapperBuilderProvider: propBagMapperBuilderProvider
                );

            return autoMapperProvider;
        }

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

        public static IMapper GetAutoMapper<TSource, TDestination>
            (
            IMapperRequest mapperRequest,
            //ViewModelFactoryInterface viewModelFactory,
            IAutoMapperService autoMapperService,
            out IPropBagMapperKey<TSource, TDestination> propBagMapperKey
            )
            where TDestination : class, IPropBag
        {
            // This is where the PropModel is used to define the Mapper 

            // TODO: See if we can submit the request earlier; perhaps when the mapper request is created.

            Type typeToWrap = mapperRequest.PropModel.TypeToWrap;

            // Submit the Mapper Request.
            propBagMapperKey = autoMapperService.SubmitRawAutoMapperRequest<TSource, TDestination>
                (mapperRequest.PropModel/*, viewModelFactory*/, typeToWrap, mapperRequest.ConfigPackageName);

            // Get the AutoMapper mapping function associated with the mapper request just submitted.
            //IPropBagMapperGen genMapper = _autoMapperService.GetMapper(mapperKey);

            IMapper rawAutoMapper = autoMapperService.GetRawAutoMapper(propBagMapperKey);
            return rawAutoMapper;
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
