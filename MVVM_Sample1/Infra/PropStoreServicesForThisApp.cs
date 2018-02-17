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
using MVVMApplication.Model;

namespace MVVMApplication.Infra
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public static class PropStoreServicesForThisApp 
    {
        private readonly static SimplePropStoreProxy _theStore;

        public static int MAX_NUMBER_OF_PROPERTIES = 65536;

        private static IPropFactoryFactory PropFactoryFactory { get; }

        private static IPropFactory DefaultPropFactory { get; }

        public static IProvidePropModels PropModelProvider { get; private set; }

        public static ViewModelHelper ViewModelHelper { get; private set; }

        public static IProvideAutoMappers AutoMapperProvider { get; }

        static string _configPackageNameSuffix;
        public static string ConfigPackageNameSuffix
        {
            get
            {
                return _configPackageNameSuffix;
            }

            set
            {
                if(value != _configPackageNameSuffix)
                {
                    _configPackageNameSuffix = value;
                    PropModelProvider = GetPropModelProvider(PropFactoryFactory, ConfigPackageNameSuffix);

                    IViewModelActivator vmActivator = new SimpleViewModelActivator();
                    ViewModelHelper = new ViewModelHelper(PropModelProvider, vmActivator, _theStore.PropStoreAccessServiceFactory, AutoMapperProvider);

                    // Remove any AutoMapper that may have been previously created.
                    AutoMapperProvider.ClearMappersCache();

                    // TODO: Consider also clearing the cache of emitted Types.
                    //AutoMapperProvider.ClearEmittedTypeCache();
                }
            }
        }

        static MemConsumptionTracker _mct = new MemConsumptionTracker();


        static PropStoreServicesForThisApp() 
        {
                _mct.Measure("Begin PropStoreServicesForThisApp");

            Person p = new Person();
                _mct.MeasureAndReport("New Person");

            ITypeDescBasedTConverterCache typeDescBasedTConverterCache = new TypeDescBasedTConverterCache();
            _mct.MeasureAndReport("After new TypeDescBasedTConverterCache");

            IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates));
            _mct.MeasureAndReport("After new SimpleDelegateCacheProvider");

            IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();
            _mct.MeasureAndReport("After new SimpleHandlerDispatchDelegateCacheProvider");


            _theStore = new SimplePropStoreProxy(MAX_NUMBER_OF_PROPERTIES, handlerDispatchDelegateCacheProvider);

            // Get a reference to the PropStoreAccessService Factory.
            PSAccessServiceCreatorInterface psAccessServiceFactory = _theStore.PropStoreAccessServiceFactory;

            IViewModelActivator vmActivator = new SimpleViewModelActivator();
                _mct.MeasureAndReport("After new SimpleViewModelActivator");

            AutoMapperProvider = GetAutoMapperProvider(vmActivator, psAccessServiceFactory);
            _mct.MeasureAndReport("After GetAutoMapperProvider");

            IConvertValues valueConverter = new PropFactoryValueConverter(typeDescBasedTConverterCache);

            IPropFactoryFactory propFactoryFactory = BuildThePropFactoryFactory(valueConverter, delegateCacheProvider);
            PropFactoryFactory = propFactoryFactory;

            DefaultPropFactory = BuildDefaultPropFactory(valueConverter, delegateCacheProvider/*, AutoMapperProvider*/);
            _mct.MeasureAndReport("After new BuildDefaultPropFactory");

            PropModelProvider = GetPropModelProvider(propFactoryFactory, ConfigPackageNameSuffix);

            ViewModelHelper = new ViewModelHelper(PropModelProvider, vmActivator, psAccessServiceFactory, AutoMapperProvider);
            _mct.MeasureAndReport("After new ViewModelHelper");

        }

        private static IProvidePropModels GetPropModelProvider
            (
            IPropFactoryFactory propFactoryFactory,
            string configPackageNameSuffix
            )
        {
            IPropBagTemplateProvider propBagTemplateProvider = new PropBagTemplateProvider(Application.Current.Resources, null);
                _mct.MeasureAndReport("After new PropBagTemplateProvider");

            IMapperRequestProvider mapperRequestProvider = new MapperRequestProvider(Application.Current.Resources, configPackageNameSuffix);
                _mct.MeasureAndReport("After new MapperRequestProvider");


            IParsePropBagTemplates propBagTemplateParser = new PropBagTemplateParser();

            IProvidePropModels propModelProvider = new SimplePropModelProvider
                (
                propBagTemplateProvider,
                mapperRequestProvider,
                propBagTemplateParser,
                propFactoryFactory
                );

                _mct.MeasureAndReport("After new PropModelProvider");

            return propModelProvider;
        }

        private static IPropFactoryFactory BuildThePropFactoryFactory
            (
            IConvertValues valueConverter,
            IProvideDelegateCaches delegateCacheProvider
            )
        {
            ResolveTypeDelegate typeResolver = null;

            IPropFactoryFactory result = new PropFactoryFactory
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

        //private static PSServiceSingletonProviderInterface BuildPropStoreService()
        //{
        //    PSServiceSingletonProviderInterface result;

        //    ITypeDescBasedTConverterCache typeDescBasedTConverterCache = new TypeDescBasedTConverterCache();
        //    _mct.MeasureAndReport("After new TypeDescBasedTConverterCache");

        //    IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates));
        //    _mct.MeasureAndReport("After new SimpleDelegateCacheProvider");

        //    IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();
        //    _mct.MeasureAndReport("After new SimpleHandlerDispatchDelegateCacheProvider");

        //    result = new PropStoreServices_NotUsed
        //        (
        //        typeDescBasedTConverterCache,
        //        delegateCacheProvider,
        //        handlerDispatchDelegateCacheProvider
        //        );

        //    _mct.MeasureAndReport("After New PropStoreServices");

        //    return result;
        //}

        private static IProvideAutoMappers GetAutoMapperProvider
            (
            IViewModelActivator viewModelActivator,
            PSAccessServiceCreatorInterface storeAccessCreator
            )
        {
            // TODO: Expose the creation of wrapperTypeCreator (ICreateWrapperTypes).
            IPropBagMapperBuilderProvider propBagMapperBuilderProvider = new SimplePropBagMapperBuilderProvider
                (
                wrapperTypesCreator: null,
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
