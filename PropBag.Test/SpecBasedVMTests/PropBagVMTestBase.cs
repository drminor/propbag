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
        MemConsumptionTracker _mct;

        protected int MaxPropsPerObject { get; private set; }
        protected PSServiceSingletonProviderInterface PropStoreServices { get; private set; }
        protected IProvidePropModels PropModelProvider { get; private set; }
        protected ViewModelHelper ViewModelHelper { get; private set; }
        protected IProvideAutoMappers AutoMapperProvider { get; private set; }
        protected string ConfigPackageNameSuffix { get; private set; }

        protected override Action EstablishContext()
        {
            bool _memTrackerEnabledState = false;
            _mct = new MemConsumptionTracker("PSMT", "Starting MemTracker for PropStoreServices, AutoMapperSupport and related.", _memTrackerEnabledState);

            MaxPropsPerObject = 65536;

            PropStoreServices = BuildPropStoreService(MaxPropsPerObject);
            _mct.MeasureAndReport("After BuildPropStoreService");

            IViewModelActivator vmActivator = new SimpleViewModelActivator();
            _mct.MeasureAndReport("After new SimpleViewModelActivator");

            AutoMapperProvider = GetAutoMapperProvider(vmActivator, PropStoreServices.PropStoreEntryPoint);
            _mct.MeasureAndReport("After GetAutoMapperProvider");

            ResourceDictionary resources = GetResources();
            _mct.MeasureAndReport("After Get [Dictionary] Resources");

            PropModelProvider = GetPropModelProvider(resources, vmActivator, ConfigPackageNameSuffix);
            _mct.MeasureAndReport("After GetPropModelProvider");

            ViewModelHelper = new ViewModelHelper(PropModelProvider, vmActivator, PropStoreServices.PropStoreEntryPoint);
            _mct.MeasureAndReport("After new ViewModelHelper");

            return OurCleanupRoutine;

            void OurCleanupRoutine()
            {
                _mct.CompactMeasureAndReport("Before Context Cleanup");

                // PropStoreServices
                if(PropStoreServices is IDisposable disable1)
                {
                    disable1.Dispose();
                }
                PropStoreServices = null;

                // AutoMapperProvider
                if(AutoMapperProvider is IDisposable disable2)
                {
                    disable2.Dispose();
                }
                AutoMapperProvider = null;

                // PropModelProvider
                if(PropModelProvider is IDisposable disable3)
                {
                    disable3.Dispose();
                }
                PropModelProvider = null;

                // ViewModelHelper
                if(ViewModelHelper is IDisposable disable4)
                {
                    disable4.Dispose();
                }
                ViewModelHelper = null;

                _mct.CompactMeasureAndReport("After Context Cleanup");
            }
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

        protected virtual IProvideAutoMappers GetAutoMapperProvider(IViewModelActivator viewModelActivator, PSAccessServiceCreatorInterface storeAccessCreator)
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

        protected virtual IProvidePropModels GetPropModelProvider(ResourceDictionary resources, IViewModelActivator vmActivator, string configPackageNameSuffix)
        {
            IPropBagTemplateProvider propBagTemplateProvider = new PropBagTemplateProvider(resources, null);
            _mct.MeasureAndReport("After new PropBagTemplateProvider");

            IMapperRequestProvider mapperRequestProvider = new MapperRequestProvider(resources, ConfigPackageNameSuffix);
            _mct.MeasureAndReport("After new MapperRequestProvider");

            IPropFactory fallBackPropFactory = null;
            IProvidePropModels propModelProvider = new SimplePropModelProvider(propBagTemplateProvider, mapperRequestProvider, fallBackPropFactory, vmActivator, PropStoreServices.PropStoreEntryPoint);
            _mct.MeasureAndReport("After new PropModelProvider");
            return propModelProvider;
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

        //private IPropFactory BuildDefaultPropFactory(PSServiceSingletonProviderInterface propStoreServices, IProvideAutoMappers autoMapperProvider)
        //{
        //    IConvertValues valueConverter = new PropFactoryValueConverter(propStoreServices.TypeDescBasedTConverterCache);
        //    ResolveTypeDelegate typeResolver = null;
        //    IPropFactory result = new WPFPropFactory
        //        (delegateCacheProvider: propStoreServices.DelegateCacheProvider,
        //        valueConverter: valueConverter,
        //        typeResolver: typeResolver,
        //        autoMapperProvider: autoMapperProvider);

        //    return result;
        //}

    }
}
