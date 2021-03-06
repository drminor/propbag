﻿using DRM.PropBag;
using DRM.PropBag.Caches;
using DRM.PropBag.TypeWrapper;
using DRM.PropBag.TypeWrapper.TypeDesc;
using DRM.PropBag.ViewModelTools;
using DRM.PropBagControlsWPF;
using DRM.PropBagWPF;
using DRM.TypeSafePropertyBag;
using ObjectSizeDiagnostics;
using Swhp.Tspb.PropBagAutoMapperService;
using Swhp.AutoMapperSupport;
using System;

namespace PropBagLib.Tests.SpecBasedVMTests
{
    using PropModelCacheInterface = ICachePropModels<String>;
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;
    using ViewModelActivatorInterface = IViewModelActivator<UInt32, String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public abstract class PropBagVMTestBase : Specification
    {
        private MemConsumptionTracker _mct; // This is only used to measure the amount of memory used during EstablishContext.

        #region Settings

        private bool _trackerEnabledState = false;
        private const int MAX_PROPS_PER_OBJECT = 65536;
        private string _resourceFolderPath = @"C:\DEV\VS2013Projects\PubPropBag\PropBagLib.Tests.PropBagTemplates\ProbBagTemplates";

        private string[] _pbTemplateFilenames = new string[]
        {
            "MainWindowVM_Emit.xaml",
            "PersonCollectionVM_Emit.xaml",
            "PersonEditorVM_Emit.xaml",
            "PersonVM_Emit.xaml",

            "MainWindowVM_Extra.xaml",
            "PersonCollectionVM_Extra.xaml",
            "PersonEditorVM_Extra.xaml",
            "PersonVM.xaml"
        };

        private string[] _mapperRequestFilenames = new string[]
        {
                "MapperConfigs.xaml",
        };

        string _configPackageNameSuffix = "Emit";
        //string _configPackageNameSuffix = "Extra";

        string _defaultNamespace = "MVVMApplication.ViewModel";

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

        protected virtual string DefaultNamespace
        {
            get
            {
                return _defaultNamespace;
            }
            set
            {
                _defaultNamespace = value;
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

        protected ViewModelFactoryInterface ViewModelFactory { get; set; }

        protected ICreateWrapperTypes WrapperTypeCreator { get; set; }
        protected IPropBagMapperService PropBagMapperService { get; set; }

        protected PropModelCacheInterface PropModelCache { get; set; }

        //protected IPropFactory DefaultPropFactory { get; set; }

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

            // Build the PropFactory Provider and the two Delegate Caches on which it depends.
            ITypeDescBasedTConverterCache typeDescBasedTConverterCache = new TypeDescBasedTConverterCache();
                _mct.MeasureAndReport("After new TypeDescBasedTConverterCache");

            IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates));
                _mct.MeasureAndReport("After new SimpleDelegateCacheProvider");

            IConvertValues valueConverter = new PropFactoryValueConverter(typeDescBasedTConverterCache);
            ResolveTypeDelegate typeResolver = null;

            // The Factory used to build PropFactories.
            IPropFactoryFactory propFactoryFactory = BuildThePropFactoryFactory
                (
                valueConverter,
                delegateCacheProvider,
                typeResolver
                );

            // Build the Property Store and the EventHandler (Delegate) Dispatch cache on which it depends.
            // This Property Store is a regular property of this class and not a (static) AppDomain-wide singleton. 

            IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();
                _mct.MeasureAndReport("After new SimpleHandlerDispatchDelegateCacheProvider");

            // Property Store
            _theStore = new SimplePropStoreProxy(MaxPropsPerObject, handlerDispatchDelegateCacheProvider);
propModelBuilder
            // PropStoreAccessService Factory.
            PropStoreAccessService_Factory = _theStore.PropStoreAccessServiceFactory;

            // ViewModelActivator.
            ViewModelActivatorInterface vmActivator = new SimpleViewModelActivator();
                _mct.MeasureAndReport("After new SimpleViewModelActivator");

            WrapperTypeCreator = BuildSimpleWrapperTypeCreator();
                _mct.MeasureAndReport("After GetSimpleWrapperTypeCreator");


            // PropModel Provider
            RemotePropModelBuilder remotePropModelProvider = GetPropModelProvider(propFactoryFactory/*, ConfigPackageNameSuffix*/);
            //PropModelBuilder = remotePropModelProvider;
                _mct.MeasureAndReport("After GetPropModelProvider");

            // Load the PropBag and Mapper Templates
            LoadPropModelsAndMappers(remotePropModelProvider, ResourceFolderPath, PBTemplateFilenames, MapperRequestFilenames);
                _mct.MeasureAndReport("After LoadPropModelsAndMappers");

            PropModelCache = new SimplePropModelCache(remotePropModelProvider);

            // Create the ViewModelFactory
            ViewModelFactory = new SimpleViewModelFactory(PropModelCache, vmActivator, PropStoreAccessService_Factory, null, WrapperTypeCreator);
            _mct.MeasureAndReport("After new ViewModelFactory");

            // AutoMapper Services
            PropBagMapperService = GetAutoMapperProvider(ViewModelFactory);
            _mct.MeasureAndReport("After GetAutoMapperProvider");

            ViewModelFactory.AutoMapperService = PropBagMapperService;

            //// Default PropFactory
            //DefaultPropFactory = BuildDefaultPropFactory
            //    (
            //    valueConverter,
            //    delegateCacheProvider,
            //    typeResolver
            //    );

            //_mct.MeasureAndReport("After BuildDefaultPropFactory");

            return OurCleanupRoutine;

            void OurCleanupRoutine()
            {
                _mct.CompactMeasureAndReport("Before Context Cleanup");

                // Wrapper Type Creator
                if(WrapperTypeCreator is IDisposable disable1)
                {
                    disable1.Dispose();
                }

                // AutoMapper Provider
                if (PropBagMapperService is IDisposable disable2)
                {
                    disable2.Dispose();
                }
                PropBagMapperService = null;

                // PropModel Provider
                if (PropModelCache is IDisposable disable3)
                {
                    disable3.Dispose();
                }
                PropModelCache = null;

                // ViewModel Helper
                //if (ViewModelHelper is IDisposable disable4)
                //{
                //    disable4.Dispose();
                //}
                //ViewModelHelper = null;

                // The Property Store
                _theStore.Dispose();

                // Type Converter Cache
                if (typeDescBasedTConverterCache is IDisposable disable5)
                {
                    disable5.Dispose();
                }

                // Delegate Cache Provider
                if (delegateCacheProvider is IDisposable disable6)
                {
                    disable6.Dispose();
                }

                // Event Handler Dispatcher Delegate Cache Provider
                if (handlerDispatchDelegateCacheProvider is IDisposable disable7)
                {
                    disable7.Dispose();
                }

                // PropModel Provider
                if(remotePropModelProvider is IDisposable disable8)
                {
                    disable8.Dispose();
                }

                propFactoryFactory = null;
                //DefaultPropFactory = null;

                _mct.CompactMeasureAndReport("After Context Cleanup");
            }
        }

        #endregion

        #region Methods Used to Establish the Context

        protected virtual IPropFactoryFactory BuildThePropFactoryFactory
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

        private IPropFactory BuildDefaultPropFactory
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

        protected virtual RemotePropModelBuilder GetPropModelProvider
            (
            IPropFactoryFactory propFactoryFactory
            //, string configPackageNameSuffix
            )
        {
            ResourceDictionaryProvider rdProvider = new ResourceDictionaryProvider();

            PropBagTemplateParser pbtParser = new PropBagTemplateParser();

            RemotePropModelBuilder propModelBuilder = new RemotePropModelBuilder(rdProvider, pbtParser, propFactoryFactory/*, configPackageNameSuffix*/);
            _mct.MeasureAndReport("After new PropModelBuilder");

            return propModelBuilder;
        }

        void LoadPropModelsAndMappers
            (
            RemotePropModelBuilder remotePropModelProvider,
            string resourceFolderPath,
            string[] pbTemplateFilenames,
            string[] mapperRequestFilenames
            )
        {
            remotePropModelProvider.LoadMapperRequests(resourceFolderPath, mapperRequestFilenames);
            _mct.MeasureAndReport("After LoadMapperRequests");

            remotePropModelProvider.LoadPropModels(resourceFolderPath, pbTemplateFilenames);
            _mct.MeasureAndReport("After LoadPropModels");
        }

        //protected virtual ResourceDictionary GetResources(string resourceFolderPath, string[] filenames)
        //{
        //    ResourceDictionaryProvider resourceDictionaryProvider = new ResourceDictionaryProvider();
        //    ResourceDictionary resources = resourceDictionaryProvider.LoadUsingSTA(resourceFolderPath, filenames);
        //    resourceDictionaryProvider = null;

        //    return resources;
        //}

        protected virtual IPropBagMapperService GetAutoMapperProvider(ViewModelFactoryInterface viewModelFactory)
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

        protected virtual ICreateWrapperTypes BuildSimpleWrapperTypeCreator()
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

        #endregion

        #region Helper Methods

        protected string GetFullClassName(string defaultNamespace, string className, string suffix)
        {
            string result;

            if (className.Contains("."))
            {
                result = suffix != null ? $"{className}_{suffix}" : className;
            }
            else
            {
                result = suffix != null ? $"{defaultNamespace}.{className}_{suffix}" : className;
            }
            return result;
        }

        protected string GetFullClassName(string defaultNamespace, string className)
        {
            string result;

            if (className.Contains("."))
            {
                result = className;
            }
            else
            {
                result = $"{defaultNamespace}.{className}";
            }
            return result;
        }

        #endregion
    }
}
