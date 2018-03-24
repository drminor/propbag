using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.Caches;
using DRM.PropBagControlsWPF;

using DRM.PropBagWPF;

using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;
using System;
using System.ComponentModel;
using System.Windows;
using ObjectSizeDiagnostics;
using MVVM_Sample1.Model;
using DRM.PropBag.TypeWrapper;
using DRM.PropBag.TypeWrapper.TypeDesc;

namespace MVVM_Sample1.Infra
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;
    using PropModelCacheInterface = ICachePropModels<String>;
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public static class PropStoreServicesForThisApp 
    {
        public static int MAX_NUMBER_OF_PROPERTIES = 65536;

        private static MemConsumptionTracker _mct = new MemConsumptionTracker();
        private readonly static SimplePropStoreProxy _theStore;

        public readonly static IProvideHandlerDispatchDelegateCaches HandlerDispatchDelegateCacheProvider;

        private static IPropFactoryFactory _propFactoryFactory { get; }
        private static IViewModelActivator _vmActivator { get; }
        //private static IPropFactory _defaultPropFactory { get; }

        //public static IProvidePropModels PropModelProvider { get; private set; }
        public static PropModelCacheInterface PropModelCache { get; private set; }

        public static ViewModelHelper ViewModelHelper { get; private set; }
        public static ICreateWrapperTypes WrapperTypeCreator { get; }
        public static IProvideAutoMappers AutoMapperProvider { get; }

        //static string _configPackageNameSuffix;
        //public static string ConfigPackageNameSuffix
        //{
        //    get
        //    {
        //        return _configPackageNameSuffix;
        //    }

        //    set
        //    {
        //        if(value != _configPackageNameSuffix)
        //        {
        //            _configPackageNameSuffix = value;
        //            IProvidePropModels propModelProvider = GetPropModelProvider(_propFactoryFactory, ConfigPackageNameSuffix);

        //            if(PropModelCache != null)
        //            {
        //                PropModelCache.Clear();
        //            }

        //            PropModelCache = new SimplePropModelCache(propModelProvider);

        //            ViewModelHelper = new ViewModelHelper(PropModelCache, _vmActivator, _theStore.PropStoreAccessServiceFactory, AutoMapperProvider);

        //            // Remove any AutoMapper that may have been previously created.
        //            AutoMapperProvider.ClearMappersCache();

        //            // TODO: Consider also clearing the cache of emitted Types.
        //            //AutoMapperProvider.ClearEmittedTypeCache();
        //        }
        //    }
        //}

        public static string ConfigPackageNameSuffix { get; set; }


        static PropStoreServicesForThisApp() 
        {
                _mct.Measure("Begin PropStoreServicesForThisApp");

            Person p = new Person();
                _mct.MeasureAndReport("New Person");

            ITypeDescBasedTConverterCache typeDescBasedTConverterCache = new TypeDescBasedTConverterCache();
            _mct.MeasureAndReport("After new TypeDescBasedTConverterCache");

            IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates));
            _mct.MeasureAndReport("After new SimpleDelegateCacheProvider");

            HandlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();
            _mct.MeasureAndReport("After new SimpleHandlerDispatchDelegateCacheProvider");

            // This creates the Global, Shared, Property Store.
            _theStore = new SimplePropStoreProxy(MAX_NUMBER_OF_PROPERTIES, HandlerDispatchDelegateCacheProvider);

            // Get a reference to the PropStoreAccessService Factory.
            PSAccessServiceCreatorInterface psAccessServiceFactory = _theStore.PropStoreAccessServiceFactory;

            _vmActivator = new SimpleViewModelActivator();
                _mct.MeasureAndReport("After new SimpleViewModelActivator");

            IConvertValues valueConverter = new PropFactoryValueConverter(typeDescBasedTConverterCache);
            ResolveTypeDelegate typeResolver = null;

            _propFactoryFactory = BuildThePropFactoryFactory(valueConverter, delegateCacheProvider, typeResolver);

            IProvidePropModels propModelProvider = GetPropModelProvider(_propFactoryFactory/*, ConfigPackageNameSuffix*/);

            PropModelCache = new SimplePropModelCache(propModelProvider);

            WrapperTypeCreator = GetSimpleWrapperTypeCreator();
                _mct.MeasureAndReport("After GetSimpleWrapperTypeCreator");

            AutoMapperProvider = GetAutoMapperProvider(WrapperTypeCreator, _vmActivator, psAccessServiceFactory);
                _mct.MeasureAndReport("After GetAutoMapperProvider");

            ViewModelHelper = new ViewModelHelper(PropModelCache, _vmActivator, psAccessServiceFactory, AutoMapperProvider, WrapperTypeCreator);
                _mct.MeasureAndReport("After new ViewModelHelper");

            //_defaultPropFactory = BuildDefaultPropFactory(valueConverter, delegateCacheProvider, typeResolver);
            //_mct.MeasureAndReport("After new BuildDefaultPropFactory");
        }

        private static IProvidePropModels GetPropModelProvider
            (
            IPropFactoryFactory propFactoryFactory
            //, string configPackageNameSuffix
            )
        {
            IPropBagTemplateProvider propBagTemplateProvider = new PropBagTemplateProvider(Application.Current.Resources);
                _mct.MeasureAndReport("After new PropBagTemplateProvider");

            IMapperRequestProvider mapperRequestProvider = new MapperRequestProvider(Application.Current.Resources);
                _mct.MeasureAndReport("After new MapperRequestProvider");

            IParsePropBagTemplates propBagTemplateParser = new PropBagTemplateParser();

            IProvidePropModels propModelProvider = new SimplePropModelProvider
                (
                propBagTemplateProvider,
                mapperRequestProvider,
                propBagTemplateParser,
                propFactoryFactory
                //, configPackageNameSuffix
                );

                _mct.MeasureAndReport("After new PropModelProvider");

            return propModelProvider;
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
            IProvideDelegateCaches delegateCacheProvider,
            ResolveTypeDelegate typeResolver
            )
        {
            IPropFactory result = new WPFPropFactory
                (
                delegateCacheProvider: delegateCacheProvider,
                valueConverter: valueConverter,
                typeResolver: typeResolver
                );

            return result;
        }

        private static IProvideAutoMappers GetAutoMapperProvider
            (
            ICreateWrapperTypes wrapperTypesCreator,
            IViewModelActivator viewModelActivator,
            PSAccessServiceCreatorInterface storeAccessCreator
            )
        {
            IPropBagMapperBuilderProvider propBagMapperBuilderProvider = new SimplePropBagMapperBuilderProvider
                (
                wrapperTypesCreator: wrapperTypesCreator,
                viewModelActivator: viewModelActivator,
                storeAccessCreator: storeAccessCreator
                );

            IMapTypeDefinitionProvider mapTypeDefinitionProvider = new SimpleMapTypeDefinitionProvider();

            ICachePropBagMappers mappersCachingService = new SimplePropBagMapperCache();

            SimpleAutoMapperProvider autoMapperProvider = new SimpleAutoMapperProvider
                (
                mapTypeDefinitionProvider: mapTypeDefinitionProvider,
                mappersCachingService: mappersCachingService,
                mapperBuilderProvider: propBagMapperBuilderProvider
                );

            return autoMapperProvider;
        }

        private static ICreateWrapperTypes GetSimpleWrapperTypeCreator()
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
