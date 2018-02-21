using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.Caches;
using DRM.PropBag.ViewModelTools;
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
        private MemConsumptionTracker _mct; // This is only used to measure the amount of memory used during EstablishContext.

        #region Settings

        private bool _trackerEnabledState = false;
        private const int MAX_PROPS_PER_OBJECT = 65536;
        private string _resourceFolderPath = @"C:\DEV\VS2013Projects\PubPropBag\PropBagLib.Tests.PropBagTemplates\ProbBagTemplates";

        private string[] _pbTemplateFilenames = new string[]
        {
                "MainWindowVM.xaml",
                "PersonCollectionVM.xaml",
                "PersonEditorVM.xaml",
                "PersonVM.xaml"
        };

        private string[] _mapperRequestFilenames = new string[]
        {
                "MapperConf_Both.xaml",
        };

        string _configPackageNameSuffix = "Emit_Proxy";

        #endregion

        #region Getter/Setters for the Settings

        protected virtual bool TrackMemConsumptionUsedToEstablishContext
        {
            get
            {
                return _trackerEnabledState;
            }
            set
            {
                _trackerEnabledState = value;
            }
        }

        public virtual int MaxPropsPerObject
        {
            get
            {
                return MAX_PROPS_PER_OBJECT;
            }
            protected set
            {
                throw new NotSupportedException("These tests do not support changing the value of MaxPropsPerObject.");
            }
        }

        protected virtual string ResourceFolderPath
        {
            get
            {
                return _resourceFolderPath;
            }
            set
            {
                _resourceFolderPath = value;
            }
        }


        protected virtual string[] PBTemplateFilenames
        {
            get
            {
                return _pbTemplateFilenames;
            }
            set
            {
                _pbTemplateFilenames = value;
            }
        }

        protected virtual string[] MapperRequestFilenames
        {
            get
            {
                return _mapperRequestFilenames;
            }
            set
            {
                _mapperRequestFilenames = value;
            }
        }

        protected virtual string ConfigPackageNameSuffix
        {
            get
            {
                return _configPackageNameSuffix;
            }
            set
            {
                _configPackageNameSuffix = value;
            }
        }

        #endregion

        #region Service Properties

        protected SimplePropStoreProxy _theStore { get; set; }
        protected PSAccessServiceCreatorInterface PropStoreAccessService_Factory { get; set; }
        protected IPropFactory DefaultPropFactory { get; set; }

        protected IProvidePropModels PropModelProvider { get; set; }
        protected ViewModelHelper ViewModelHelper { get; set; }
        protected IProvideAutoMappers AutoMapperProvider { get; set; }

        protected string DataDirPath { get; set; }

        #endregion

        #region Constructor

        public PropBagVMTestBase()
        {

        }

        #endregion

        #region Specification Implementation

        protected override Action EstablishContext()
        {
            _mct = new MemConsumptionTracker
                (
                "PSMT",
                "Starting MemTracker for PropStoreServices, AutoMapperSupport and related.",
                TrackMemConsumptionUsedToEstablishContext
                );

            ITypeDescBasedTConverterCache typeDescBasedTConverterCache = new TypeDescBasedTConverterCache();
            _mct.MeasureAndReport("After new TypeDescBasedTConverterCache");

            IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates));
            _mct.MeasureAndReport("After new SimpleDelegateCacheProvider");

            IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();
            _mct.MeasureAndReport("After new SimpleHandlerDispatchDelegateCacheProvider");

            // Property Store
            _theStore = new SimplePropStoreProxy(MaxPropsPerObject, handlerDispatchDelegateCacheProvider);

            // PropStoreAccessService Factory.
            PropStoreAccessService_Factory = _theStore.PropStoreAccessServiceFactory;

            // ViewModelActivator.
            IViewModelActivator vmActivator = new SimpleViewModelActivator();
            _mct.MeasureAndReport("After new SimpleViewModelActivator");

            // AutoMapper Services
            AutoMapperProvider = GetAutoMapperProvider(vmActivator, PropStoreAccessService_Factory);
            _mct.MeasureAndReport("After GetAutoMapperProvider");

            IConvertValues valueConverter = new PropFactoryValueConverter(typeDescBasedTConverterCache);

            // Default PropFactory
            DefaultPropFactory = BuildDefaultPropFactory
                (
                valueConverter,
                delegateCacheProvider//,
                                     //AutoMapperProvider
                );
            _mct.MeasureAndReport("After BuildDefaultPropFactory");

            // The Factory used to build PropFactories.
            IPropFactoryFactory propFactoryFactory = BuildThePropFactoryFactory
                (
                valueConverter,
                delegateCacheProvider
                );

            // PropModel Provider
            RemotePropModelProvider remotePropModelProvider = GetPropModelProvider(propFactoryFactory, ConfigPackageNameSuffix);
            PropModelProvider = remotePropModelProvider;
            _mct.MeasureAndReport("After GetPropModelProvider");

            // Load the PropBag and Mapper Templates
            LoadPropModelsAndMappers(remotePropModelProvider, ResourceFolderPath, PBTemplateFilenames, MapperRequestFilenames);
            _mct.MeasureAndReport("After LoadPropModelsAndMappers");

            // Create the ViewModelHelper
            ViewModelHelper = new ViewModelHelper(PropModelProvider, vmActivator, PropStoreAccessService_Factory, AutoMapperProvider);
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

        #endregion

        #region Methods Used to Establish the Context

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

        protected virtual RemotePropModelProvider GetPropModelProvider
            (
            IPropFactoryFactory propFactoryFactory,
            string configPackageNameSuffix
            )
        {
            ResourceDictionaryProvider rdProvider = new ResourceDictionaryProvider();

            PropBagTemplateParser pbtParser = new PropBagTemplateParser();

            RemotePropModelProvider propModelProvider = new RemotePropModelProvider(rdProvider, pbtParser, propFactoryFactory, configPackageNameSuffix);
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

        protected virtual ResourceDictionary GetResources(string resourceFolderPath, string[] filenames)
        {
            ResourceDictionaryProvider resourceDictionaryProvider = new ResourceDictionaryProvider();
            ResourceDictionary resources = resourceDictionaryProvider.LoadUsingSTA(resourceFolderPath, filenames);
            resourceDictionaryProvider = null;

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

        #endregion
    }
}
