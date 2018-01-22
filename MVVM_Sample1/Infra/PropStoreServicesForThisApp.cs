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

namespace MVVMApplication.Infra
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;
    using PSServiceSingletonProviderInterface = IProvidePropStoreServiceSingletons<UInt32, String>;

    public static class PropStoreServicesForThisApp 
    {
        public static int MAX_NUMBER_OF_PROPERTIES = 65536;
        public static PSServiceSingletonProviderInterface PropStoreServices { get; }

        public static IPropFactory DefaultPropFactory { get; }

        public static IProvidePropModels PropModelProvider { get; }
        public static ViewModelHelper ViewModelHelper { get; }
        public static IProvideAutoMappers AutoMapperProvider { get; }

        public static string PackageConfigName { get; set; }

        static PropStoreServicesForThisApp() 
        {
            PropStoreServices = BuildPropStoreService(MAX_NUMBER_OF_PROPERTIES);

            IViewModelActivator vmActivator = new SimpleViewModelActivator();
            AutoMapperProvider = GetAutoMapperProvider(vmActivator, PropStoreServices.PropStoreEntryPoint);

            DefaultPropFactory = BuildDefaultPropFactory(PropStoreServices, AutoMapperProvider);

            IPropBagTemplateProvider propBagTemplateProvider = new PropBagTemplateProvider(Application.Current.Resources);
            IMapperRequestProvider mapperRequestProvider = new MapperRequestProvider(Application.Current.Resources);

            PropModelProvider = new PropModelProvider(propBagTemplateProvider, mapperRequestProvider, DefaultPropFactory, vmActivator, PropStoreServices.PropStoreEntryPoint);

            ViewModelHelper = new ViewModelHelper(PropModelProvider, vmActivator, PropStoreServices.PropStoreEntryPoint);

        }

        private static PSServiceSingletonProviderInterface BuildPropStoreService(int maxNumberOfProperties)
        {
            ITypeDescBasedTConverterCache typeDescBasedTConverterCache = new TypeDescBasedTConverterCache();
            IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates));

            IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();
            PSAccessServiceCreatorInterface propStoreEntryPoint = new SimplePropStoreServiceEP(maxNumberOfProperties, handlerDispatchDelegateCacheProvider);

            PSServiceSingletonProviderInterface result = new PropStoreServices
                (typeDescBasedTConverterCache,
                delegateCacheProvider,
                handlerDispatchDelegateCacheProvider,
                propStoreEntryPoint);

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
