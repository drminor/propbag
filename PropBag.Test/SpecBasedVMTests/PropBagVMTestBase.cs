using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.Caches;
using DRM.PropBag.ViewModelTools;
using DRM.PropBagControlsWPF;
using DRM.PropBagWPF;
using DRM.TypeSafePropertyBag;
using ObjectSizeDiagnostics;
using System;
using System.Windows;

namespace PropBagLib.Tests.SpecBasedVMTests
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public abstract class PropBagVMTestBase : Specification
    {
        MemConsumptionTracker _mct; // This is only used to measure the amount of memory used during EstablishContext.

        protected int MaxPropsPerObject { get; private set; }

        private SimplePropStoreAccessServiceProviderProxy _theStore { get; set; }

        //protected PSServiceSingletonProviderInterface CoreDelegateCaches { get; private set; }

        protected IProvidePropModels PropModelProvider { get; private set; }
        protected ViewModelHelper ViewModelHelper { get; private set; }
        protected IProvideAutoMappers AutoMapperProvider { get; private set; }

        protected string ConfigPackageNameSuffix { get; private set; }

        protected string DataDirPath { get; set; }

        protected override Action EstablishContext()
        {
            bool _memTrackerEnabledState = false;
            _mct = new MemConsumptionTracker("PSMT", "Starting MemTracker for PropStoreServices, AutoMapperSupport and related.", _memTrackerEnabledState);

            // ***** SETTINGS BEGIN
            MaxPropsPerObject = 65536;

            string resourceFolderPath = @"C:\DEV\VS2013Projects\PubPropBag\PropBagLib.Tests.PropBagTemplates\ProbBagTemplates";
            string[] pbTemplateFilenames = new string[]
            {
                "MainWindowVM.xaml",
                "PersonCollectionVM.xaml",
                "PersonEditorVM.xaml",
                "PersonVM.xaml"
            };

            string[] mapperRequestFilenames = new string[]
            {
                "MapperConf_Both.xaml",
            };
            // ***** SETTINGS END

            // Core delegate caches.
            //CoreDelegateCaches = CreateTheDelegateCaches();
            //    _mct.MeasureAndReport("After BuildPropStoreService");

            ITypeDescBasedTConverterCache typeDescBasedTConverterCache = new TypeDescBasedTConverterCache();
            _mct.MeasureAndReport("After new TypeDescBasedTConverterCache");

            IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates));
            _mct.MeasureAndReport("After new SimpleDelegateCacheProvider");

            IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();
            _mct.MeasureAndReport("After new SimpleHandlerDispatchDelegateCacheProvider");

            // Property Store
            _theStore = new SimplePropStoreAccessServiceProviderProxy(MaxPropsPerObject, handlerDispatchDelegateCacheProvider);

            // PropStoreAccessService Factory.
            PSAccessServiceCreatorInterface psAccessServiceFactory = _theStore.PropStoreAccessServiceFactory;

            // ViewModelActivator.
            IViewModelActivator vmActivator = new SimpleViewModelActivator();
            _mct.MeasureAndReport("After new SimpleViewModelActivator");

            // AutoMapper Services
            AutoMapperProvider = GetAutoMapperProvider(vmActivator, psAccessServiceFactory);
            _mct.MeasureAndReport("After GetAutoMapperProvider");

            IConvertValues valueConverter = new PropFactoryValueConverter(typeDescBasedTConverterCache);

            // Default PropFactory
            IPropFactory defaultPropFactory = BuildDefaultPropFactory
                (
                valueConverter,
                delegateCacheProvider,
                AutoMapperProvider
                );
            _mct.MeasureAndReport("After BuildDefaultPropFactory");

            // The Factory used to build PropFactories.
            IPropFactoryFactory propFactoryFactory = BuildThePropFactoryFactory
                (
                valueConverter,
                delegateCacheProvider
                );

            // PropModel Provider
            RemotePropModelProvider remotePropModelProvider = GetPropModelProvider(propFactoryFactory, defaultPropFactory, ConfigPackageNameSuffix);
            PropModelProvider = remotePropModelProvider;
            _mct.MeasureAndReport("After GetPropModelProvider");

            // Load the PropBag and Mapper Templates
            LoadPropModelsAndMappers(remotePropModelProvider, resourceFolderPath, pbTemplateFilenames, mapperRequestFilenames);
            _mct.MeasureAndReport("After LoadPropModelsAndMappers");

            // Create the ViewModelHelper
            ViewModelHelper = new ViewModelHelper(PropModelProvider, vmActivator, psAccessServiceFactory, AutoMapperProvider);
            _mct.MeasureAndReport("After new ViewModelHelper");

            return OurCleanupRoutine;

            void OurCleanupRoutine()
            {
                _mct.CompactMeasureAndReport("Before Context Cleanup");

                // AutoMapperProvider
                if (AutoMapperProvider is IDisposable disable2)
                {
                    disable2.Dispose();
                }
                AutoMapperProvider = null;

                // PropModelProvider
                if (PropModelProvider is IDisposable disable3)
                {
                    disable3.Dispose();
                }
                PropModelProvider = null;

                // ViewModelHelper
                if (ViewModelHelper is IDisposable disable4)
                {
                    disable4.Dispose();
                }
                ViewModelHelper = null;

                _theStore.Dispose();

                if (typeDescBasedTConverterCache is IDisposable disable5)
                {
                    disable5.Dispose();
                }

                if (delegateCacheProvider is IDisposable disable6)
                {
                    disable6.Dispose();
                }

                if (handlerDispatchDelegateCacheProvider is IDisposable disable7)
                {
                    disable7.Dispose();
                }

                _mct.CompactMeasureAndReport("After Context Cleanup");
            }
        }

        protected virtual IPropFactoryFactory BuildThePropFactoryFactory
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

        private IPropFactory BuildDefaultPropFactory
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

        protected virtual RemotePropModelProvider GetPropModelProvider
            (
            IPropFactoryFactory propFactoryFactory,
            IPropFactory defaultPropFactory,
            string configPackageNameSuffix
            )
        {
            ResourceDictionaryProvider rdProvider = new ResourceDictionaryProvider();

            PropBagTemplateParser pbtParser = new PropBagTemplateParser();

            RemotePropModelProvider propModelProvider = new RemotePropModelProvider(rdProvider, pbtParser, propFactoryFactory, defaultPropFactory, configPackageNameSuffix);
            _mct.MeasureAndReport("After new PropModelProvider");

            return propModelProvider;
        }

        void LoadPropModelsAndMappers
            (
            RemotePropModelProvider remotePropModelProvider,
            string resourceFolderPath,
            string[] pbTemplateFilenames,
            string[] mapperRequestFilenames
            )
        {
            remotePropModelProvider.LoadMapperRequests(resourceFolderPath, mapperRequestFilenames);
            _mct.MeasureAndReport("After LoadMapperRequests");

            remotePropModelProvider.LoadPropModels(resourceFolderPath, pbTemplateFilenames);
            _mct.MeasureAndReport("After LoadPropModels");

            //IPropModel test = remotePropModelProvider.GetPropModel("PersonVM");
            //SimpleExKey testObject = test.TestObject;
        }

        protected virtual ResourceDictionary GetResources()
        {
            string resourceFolderPath = @"C:\DEV\VS2013Projects\PubPropBag\PropBagLib.Tests.PropBagTemplates\ProbBagTemplates";
            string[] filenames = new string[]
            {
                "MainWindowVM.xaml",
                "MapperConf_Both.xaml",
                "PersonCollectionVM.xaml",
                "PersonEditorVM.xaml",
                "PersonVM.xaml"
            };

            //string[] filenames = new string[]
            //{
            //    "MapperConf_Both.xaml",
            //};

            ResourceDictionaryProvider x = new ResourceDictionaryProvider();
            ResourceDictionary resources = x.LoadUsingSTA(resourceFolderPath, filenames);
            x = null;

            return resources;
        }

        protected virtual IProvideAutoMappers GetAutoMapperProvider
            (
            IViewModelActivator viewModelActivator,
            PSAccessServiceCreatorInterface psAccessServiceFactory
            )
        {
            // TODO: Expose the creation of wrapperTypeCreator (ICreateWrapperTypes).
            IPropBagMapperBuilderProvider propBagMapperBuilderProvider = new SimplePropBagMapperBuilderProvider
                (
                wrapperTypesCreator: null,
                viewModelActivator: viewModelActivator,
                storeAccessCreator: psAccessServiceFactory
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

    }
}
