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
    using PSServiceSingletonProviderInterface = IProvidePropStoreServiceSingletons<UInt32, String>;

    public abstract class PropBagVMTestBase : Specification
    {
        MemConsumptionTracker _mct; // This is only used to measure the amount of memory used during EstablishContext.

        protected int MaxPropsPerObject { get; private set; }
        protected PSServiceSingletonProviderInterface PropStoreServices { get; private set; }
        protected IProvidePropModels PropModelProvider { get; private set; }
        protected ViewModelHelper ViewModelHelper { get; private set; }
        protected IProvideAutoMappers AutoMapperProvider { get; private set; }

        protected string ConfigPackageNameSuffix { get; private set; }

        protected string DataDirPath { get; set; }

        protected override Action EstablishContext()
        {
            bool _memTrackerEnabledState = false;
            _mct = new MemConsumptionTracker("PSMT", "Starting MemTracker for PropStoreServices, AutoMapperSupport and related.", _memTrackerEnabledState);

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

            PropStoreServices = BuildPropStoreService(MaxPropsPerObject);
            _mct.MeasureAndReport("After BuildPropStoreService");

            IViewModelActivator vmActivator = new SimpleViewModelActivator();
            _mct.MeasureAndReport("After new SimpleViewModelActivator");

            AutoMapperProvider = GetAutoMapperProvider(vmActivator, PropStoreServices.PropStoreEntryPoint);
            _mct.MeasureAndReport("After GetAutoMapperProvider");

            IPropFactory defaultPropFactory = BuildDefaultPropFactory(PropStoreServices, AutoMapperProvider);
            _mct.MeasureAndReport("After BuildDefaultPropFactory");

            IPropFactoryFactory propFactoryFactory = BuildThePropFactoryFactory(PropStoreServices);

            RemotePropModelProvider remotePropModelProvider = GetPropModelProvider(vmActivator, PropStoreServices.PropStoreEntryPoint, propFactoryFactory, defaultPropFactory, ConfigPackageNameSuffix);
            PropModelProvider = remotePropModelProvider;
            _mct.MeasureAndReport("After GetPropModelProvider");

            LoadPropModelsAndMappers(remotePropModelProvider, resourceFolderPath, pbTemplateFilenames, mapperRequestFilenames);
            _mct.MeasureAndReport("After LoadPropModelsAndMappers");

            ViewModelHelper = new ViewModelHelper(PropModelProvider, vmActivator, PropStoreServices.PropStoreEntryPoint);
            _mct.MeasureAndReport("After new ViewModelHelper");

            return OurCleanupRoutine;

            void OurCleanupRoutine()
            {
                _mct.CompactMeasureAndReport("Before Context Cleanup");

                // PropStoreServices
                if (PropStoreServices is IDisposable disable1)
                {
                    disable1.Dispose();
                }
                PropStoreServices = null;

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

                _mct.CompactMeasureAndReport("After Context Cleanup");
            }
        }

        protected virtual IPropFactoryFactory BuildThePropFactoryFactory(PSServiceSingletonProviderInterface propStoreServices)
        {
            PropFactoryValueConverter valueConverter = new PropFactoryValueConverter(propStoreServices.TypeDescBasedTConverterCache);
            IPropFactoryFactory result = new PropFactoryFactory(propStoreServices.DelegateCacheProvider, valueConverter, typeResolver: null);
            return result;
        }

        protected virtual PSServiceSingletonProviderInterface BuildPropStoreService(int maxNumberOfProperties)
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
            _mct.MeasureAndReport("After New PropStoreServices");

            return result;
        }

        private IPropFactory BuildDefaultPropFactory
            (
            PSServiceSingletonProviderInterface propStoreServices,
            IProvideAutoMappers autoMapperProvider
            )
        {
            IProvideDelegateCaches delegateCacheProvider = PropStoreServices.DelegateCacheProvider;
            IConvertValues valueConverter = new PropFactoryValueConverter(propStoreServices.TypeDescBasedTConverterCache);
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
            IViewModelActivator vmActivator,
            PSAccessServiceCreatorInterface storeAccessCreator,
            IPropFactoryFactory propFactoryFactory,
            IPropFactory defaultPropFactory,
            string configPackageNameSuffix
            )
        {
            //IPropBagTemplateProvider propBagTemplateProvider = new PropBagTemplateProvider(resources, null);
            //_mct.MeasureAndReport("After new PropBagTemplateProvider");

            //IMapperRequestProvider mapperRequestProvider = new MapperRequestProvider(resources, ConfigPackageNameSuffix);
            //_mct.MeasureAndReport("After new MapperRequestProvider");

            //IPropFactory fallBackPropFactory = null;
            //IProvidePropModels propModelProvider = new SimplePropModelProvider(propBagTemplateProvider, mapperRequestProvider, vmActivator, PropStoreServices.PropStoreEntryPoint, fallBackPropFactory);
            //_mct.MeasureAndReport("After new PropModelProvider");

            //return propModelProvider;

            ResourceDictionaryProvider rdProvider = new ResourceDictionaryProvider();

            PropBagTemplateParser pbtParser = new PropBagTemplateParser();

            RemotePropModelProvider propModelProvider = new RemotePropModelProvider(rdProvider, pbtParser, propFactoryFactory, defaultPropFactory);
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
                //, propModelProvider: propModelProvider
                );

            return autoMapperProvider;
        }

    }
}
