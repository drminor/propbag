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

namespace PropBagTestApp.Infra
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public static class PropStoreServicesForThisApp
    {
        private readonly static SimplePropStoreAccessServiceProviderProxy _theStore;

        public static int MAX_NUMBER_OF_PROPERTIES = 65536;
        //public static PSServiceSingletonProviderInterface PropStoreServices { get; }

        public static IPropFactory DefaultPropFactory { get; }

        public static IProvidePropModels PropModelProvider { get; }
        public static ViewModelHelper ViewModelHelper { get; }
        public static IProvideAutoMappers AutoMapperProvider { get; }

        public static string ConfigPackageNameSuffix { get; set; }

        //static PropStoreServicesForThisApp()
        //{
        //    PropStoreServices = BuildPropStoreService(MAX_NUMBER_OF_PROPERTIES);

        //    IPropBagTemplateProvider propBagTemplateProvider = new PropBagTemplateProvider(Application.Current.Resources, null);
        //    IMapperRequestProvider mapperRequestProvider = new MapperRequestProvider(Application.Current.Resources, null);

        //    IViewModelActivator vmActivator = new SimpleViewModelActivator();

        //    AutoMapperProvider = GetAutoMapperProvider(vmActivator, PropStoreServices.PropStoreEntryPoint);

        //    IPropFactory defaultPropFactory = BuildDefaultPropFactory(PropStoreServices, AutoMapperProvider);

        //    IPropFactoryFactory propFactoryFactory = BuildThePropFactoryFactory(PropStoreServices);

        //    IParsePropBagTemplates propBagTemplateParser = new PropBagTemplateParser();

        //    PropModelProvider = new SimplePropModelProvider
        //        (
        //        propBagTemplateProvider,
        //        mapperRequestProvider,
        //        propBagTemplateParser,
        //        //vmActivator,
        //        //PropStoreServices.PropStoreEntryPoint,
        //        propFactoryFactory,
        //        defaultPropFactory
        //        );

        //    ViewModelHelper = new ViewModelHelper(PropModelProvider, vmActivator, PropStoreServices.PropStoreEntryPoint);
        //}

        static PropStoreServicesForThisApp()
        {
            // Create the 3 core cha
            ITypeDescBasedTConverterCache typeDescBasedTConverterCache = new TypeDescBasedTConverterCache();
            IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates));
            IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();

            _theStore = new SimplePropStoreAccessServiceProviderProxy(MAX_NUMBER_OF_PROPERTIES, handlerDispatchDelegateCacheProvider);

            // Get a reference to the PropStoreAccessService Factory.
            PSAccessServiceCreatorInterface psAccessServiceFactory = _theStore.PropStoreAccessServiceFactory;

            IViewModelActivator vmActivator = new SimpleViewModelActivator();

            AutoMapperProvider = GetAutoMapperProvider(vmActivator, psAccessServiceFactory);

            IConvertValues valueConverter = new PropFactoryValueConverter(typeDescBasedTConverterCache);

            // Default PropFactory
            IPropFactory defaultPropFactory = BuildDefaultPropFactory
                (
                valueConverter,
                delegateCacheProvider,
                AutoMapperProvider
                );

            // The Factory used to build PropFactories.
            IPropFactoryFactory propFactoryFactory = BuildThePropFactoryFactory
                (
                valueConverter,
                delegateCacheProvider
                );

            PropModelProvider = GetPropModelProvider(propFactoryFactory, DefaultPropFactory, ConfigPackageNameSuffix);

            ViewModelHelper = new ViewModelHelper(PropModelProvider, vmActivator, psAccessServiceFactory, AutoMapperProvider);
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
            IProvideDelegateCaches delegateCacheProvider,
            IProvideAutoMappers autoMapperProvider
            )
        {
            ResolveTypeDelegate typeResolver = null;

            IPropFactory result = new WPFPropFactory
                (
                delegateCacheProvider: delegateCacheProvider,
                valueConverter: valueConverter,
                typeResolver: typeResolver,
                autoMapperProvider: autoMapperProvider
                );

            return result;
        }

        //private static PSServiceSingletonProviderInterface BuildPropStoreService(int maxNumberOfProperties)
        //{
        //    PSServiceSingletonProviderInterface result;

        //    ITypeDescBasedTConverterCache typeDescBasedTConverterCache = new TypeDescBasedTConverterCache();

        //    IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider
        //        (
        //        typeof(PropBag),
        //        typeof(APFGenericMethodTemplates)
        //        );

        //    IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();

        //    using (SimplePropStoreAccessServiceProviderProxy epCreator = new SimplePropStoreAccessServiceProviderProxy())
        //    {
        //        PSAccessServiceCreatorInterface propStoreEntryPoint = epCreator.GetPropStoreEntryPoint(maxNumberOfProperties, handlerDispatchDelegateCacheProvider);

        //        result = new PropStoreServices
        //            (typeDescBasedTConverterCache,
        //            delegateCacheProvider,
        //            handlerDispatchDelegateCacheProvider,
        //            propStoreEntryPoint);
        //    }

        //    return result;
        //}

        //private static PSServiceSingletonProviderInterface BuildPropStoreService(int maxNumberOfProperties)
        //{
        //    PSServiceSingletonProviderInterface result;

        //    ITypeDescBasedTConverterCache typeDescBasedTConverterCache = new TypeDescBasedTConverterCache();

        //    IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates));

        //    IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();

        //    result = new PropStoreServices_NotUsed
        //        (
        //        typeDescBasedTConverterCache,
        //        delegateCacheProvider,
        //        handlerDispatchDelegateCacheProvider
        //        );


        //    return result;
        //}

        //private static IPropFactory BuildDefaultPropFactory
        //    (
        //    PSServiceSingletonProviderInterface propStoreServices,
        //    IProvideAutoMappers autoMapperProvider
        //    )
        //{
        //    IConvertValues valueConverter = new PropFactoryValueConverter(propStoreServices.TypeDescBasedTConverterCache);
        //    ResolveTypeDelegate typeResolver = null;

        //    IPropFactory result = new WPFPropFactory
        //        (
        //        delegateCacheProvider: propStoreServices.DelegateCacheProvider,
        //        valueConverter: valueConverter,
        //        typeResolver: typeResolver,
        //        autoMapperProvider: autoMapperProvider
        //        );

        //    return result;
        //}

        private static IProvidePropModels GetPropModelProvider
            (
            //IViewModelActivator vmActivator,
            //PSServiceSingletonProviderInterface propStoreServices,
            IPropFactoryFactory propFactoryFactory,
            IPropFactory defaultPropFactory,
            string configPackageNameSuffix
            )
        {
            IPropBagTemplateProvider propBagTemplateProvider = new PropBagTemplateProvider(Application.Current.Resources, null);

            IMapperRequestProvider mapperRequestProvider = new MapperRequestProvider(Application.Current.Resources, configPackageNameSuffix);


            IParsePropBagTemplates propBagTemplateParser = new PropBagTemplateParser();

            IProvidePropModels propModelProvider = new SimplePropModelProvider
                (
                propBagTemplateProvider,
                mapperRequestProvider,
                propBagTemplateParser,
                //vmActivator,
                //psAccessServiceFactory,
                propFactoryFactory,
                defaultPropFactory
                );

            return propModelProvider;
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
                mapperBuilderProvider: propBagMapperBuilderProvider/*, propModelProvider: null*/
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
