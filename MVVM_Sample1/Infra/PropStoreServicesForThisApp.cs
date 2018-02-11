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
    using PSServiceSingletonProviderInterface = IProvidePropStoreServiceSingletons<UInt32, String>;

    public static class PropStoreServicesForThisApp 
    {
        public static int MAX_NUMBER_OF_PROPERTIES = 65536;
        public static PSServiceSingletonProviderInterface PropStoreServices { get; }

        public static IPropFactory DefaultPropFactory { get; }

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
                    IViewModelActivator vmActivator = new SimpleViewModelActivator();

                    PropModelProvider = GetPropModelProvider(vmActivator, ConfigPackageNameSuffix);

                    ViewModelHelper = new ViewModelHelper(PropModelProvider, vmActivator, PropStoreServices.PropStoreEntryPoint);

                    // Remove any AutoMapper that may have been previously created.
                    AutoMapperProvider.Clear();
                }
            }
        }

        static MemConsumptionTracker _mct = new MemConsumptionTracker();


        static PropStoreServicesForThisApp() 
        {
                _mct.Measure("Begin PropStoreServicesForThisApp");

            Person p = new Person();
                _mct.MeasureAndReport("New Person");

            PropStoreServices = BuildPropStoreService(MAX_NUMBER_OF_PROPERTIES);
                _mct.MeasureAndReport("After BuildPropStoreService");

            IViewModelActivator vmActivator = new SimpleViewModelActivator();
                _mct.MeasureAndReport("After new SimpleViewModelActivator");

            AutoMapperProvider = GetAutoMapperProvider(vmActivator, PropStoreServices.PropStoreEntryPoint);
                _mct.MeasureAndReport("After GetAutoMapperProvider");

            DefaultPropFactory = BuildDefaultPropFactory(PropStoreServices, AutoMapperProvider);
                _mct.MeasureAndReport("After new BuildDefaultPropFactory");

            //IPropBagTemplateProvider propBagTemplateProvider = new PropBagTemplateProvider(Application.Current.Resources, null);
            //_mct.MeasureAndReport("After new PropBagTemplateProvider");

            //IMapperRequestProvider mapperRequestProvider = new MapperRequestProvider(Application.Current.Resources, ConfigPackageNameSuffix);
            //_mct.MeasureAndReport("After new MapperRequestProvider");


            //PropModelProvider = new PropModelProvider(propBagTemplateProvider, mapperRequestProvider, DefaultPropFactory, vmActivator, PropStoreServices.PropStoreEntryPoint);
            //_mct.MeasureAndReport("After new PropModelProvider");

            PropModelProvider = GetPropModelProvider(vmActivator, ConfigPackageNameSuffix);

            ViewModelHelper = new ViewModelHelper(PropModelProvider, vmActivator, PropStoreServices.PropStoreEntryPoint);
                _mct.MeasureAndReport("After new ViewModelHelper");
        }

        private static IProvidePropModels GetPropModelProvider(IViewModelActivator vmActivator, string configPackageNameSuffix)
        {
            IPropBagTemplateProvider propBagTemplateProvider = new PropBagTemplateProvider(Application.Current.Resources, null);
            _mct.MeasureAndReport("After new PropBagTemplateProvider");

            IMapperRequestProvider mapperRequestProvider = new MapperRequestProvider(Application.Current.Resources, ConfigPackageNameSuffix);
            _mct.MeasureAndReport("After new MapperRequestProvider");

            IProvidePropModels propModelProvider = new PropModelProvider(propBagTemplateProvider, mapperRequestProvider, DefaultPropFactory, vmActivator, PropStoreServices.PropStoreEntryPoint);
            _mct.MeasureAndReport("After new PropModelProvider");
            return propModelProvider;
        }


        private static PSServiceSingletonProviderInterface BuildPropStoreService(int maxNumberOfProperties)
        {
            PSServiceSingletonProviderInterface result;

            ITypeDescBasedTConverterCache typeDescBasedTConverterCache = new TypeDescBasedTConverterCache();
            _mct.MeasureAndReport("After new TypeDescBasedTConverterCache");

            IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates));
            _mct.MeasureAndReport("After new SimpleDelegateCacheProvider");

            IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();
            _mct.MeasureAndReport("After new SimpleHandlerDispatchDelegateCacheProvider");

            using (PropStoreServiceCreatorFactory epCreator = new PropStoreServiceCreatorFactory())
            {
                PSAccessServiceCreatorInterface propStoreEntryPoint = epCreator.GetPropStoreEntryPoint(maxNumberOfProperties, handlerDispatchDelegateCacheProvider);

                result = new PropStoreServices
                    (typeDescBasedTConverterCache,
                    delegateCacheProvider,
                    handlerDispatchDelegateCacheProvider,
                    propStoreEntryPoint);
            }

            //    PSAccessServiceCreatorInterface propStoreEntryPoint = new SimplePropStoreServiceEP(maxNumberOfProperties, handlerDispatchDelegateCacheProvider);
            //_mct.MeasureAndReport("After new SimplePropStoreServiceEP");

            _mct.MeasureAndReport("After New PropStoreServices");

            return result;
        }

        private static IPropFactory BuildDefaultPropFactory(PSServiceSingletonProviderInterface propStoreServices, IProvideAutoMappers autoMapperProvider)
        {
            IConvertValues valueConverter = new PropFactoryValueConverter(propStoreServices.TypeDescBasedTConverterCache);
            ResolveTypeDelegate typeResolver = null;
            IPropFactory result = new WPFPropFactory
                (delegateCacheProvider: propStoreServices.DelegateCacheProvider,
                valueConverter: valueConverter,
                typeResolver: typeResolver,
                autoMapperProvider: autoMapperProvider);

            return result;
        }

        private static IProvideAutoMappers GetAutoMapperProvider(IViewModelActivator viewModelActivator, PSAccessServiceCreatorInterface storeAccessCreator)
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

            // TODO: Remove the dependency on IProvidePropModels. (See TODO note in SimpleAutoMapperProvider.)
            SimpleAutoMapperProvider autoMapperProvider = new SimpleAutoMapperProvider
                (
                mapTypeDefinitionProvider: mapTypeDefinitionProvider,
                mappersCachingService: mappersCachingService,
                mapperBuilderProvider: propBagMapperBuilderProvider,
                propModelProvider: null
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
