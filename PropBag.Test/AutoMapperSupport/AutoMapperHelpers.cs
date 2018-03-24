using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.Caches;
using DRM.PropBag.TypeWrapper;
using DRM.PropBag.TypeWrapper.TypeDesc;
using DRM.PropBag.ViewModelTools;
using DRM.PropBagWPF;
using DRM.TypeSafePropertyBag;
using System;

namespace PropBagLib.Tests.AutoMapperSupport
{
    using PropModelCacheInterface = ICachePropModels<String>;

    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class AutoMapperHelpers : IDisposable
    {
        // Maximum number of PropertyIds for any one given Object.
        private const int LOG_BASE2_MAX_PROPERTIES = 16;
        public static readonly int MAX_NUMBER_OF_PROPERTIES = (int)Math.Pow(2, LOG_BASE2_MAX_PROPERTIES); //65536;

        public static string _resourceFolderPath = @"C:\DEV\VS2013Projects\PubPropBag\PropBagLib.Tests.PropBagTemplates\ProbBagTemplates";

        //public static string[] _pbTemplateFilenames = new string[]
        //    {
        //        "MainWindowVM.xaml",
        //        "PersonCollectionVM.xaml",
        //        "PersonEditorVM.xaml",
        //        "PersonVM.xaml"
        //    };

        //public static string[] _mapperRequestFilenames = new string[]
        //    {
        //        "MapperConfigs.xaml",
        //    };

        private string[] _pbTemplateFilenames = new string[]
        {
            "MainWindowVM_Emit.xaml",
            "PersonCollectionVM_Emit.xaml",
            "PersonEditorVM_Emit.xaml",

            "MainWindowVM_Extra.xaml",
            "PersonCollectionVM_Extra.xaml",
            "PersonEditorVM_Extra.xaml",

            "PersonVM.xaml"
        };

        private string[] _mapperRequestFilenames = new string[]
        {
            "MapperConfigs.xaml"
        };

        private SimplePropStoreProxy _theStore { get; set; }

        public IProvideAutoMappers InitializeAutoMappers(PSAccessServiceCreatorInterface storeAccessCreator)
        {
            IViewModelActivator vmActivator = new SimpleViewModelActivator();

            ICreateWrapperTypes simpleWrapperTypeCreator = GetWrapperTypeCreator_V1();

            IPropBagMapperBuilderProvider propBagMapperBuilderProvider
                = new SimplePropBagMapperBuilderProvider
                (
                    wrapperTypesCreator: simpleWrapperTypeCreator,
                    viewModelActivator: vmActivator,
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

        public PSAccessServiceCreatorInterface StoreAccessCreator
        {
            get
            {
                if (_theStore == null)
                {
                    IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();

                    // Create the Property Store
                    _theStore = new SimplePropStoreProxy(MAX_NUMBER_OF_PROPERTIES, handlerDispatchDelegateCacheProvider);
                }

                PSAccessServiceCreatorInterface psAccessServiceFactory = _theStore.PropStoreAccessServiceFactory;
                return psAccessServiceFactory;
            }
        }

        public IPropFactory GetNewPropFactory_V1()
        {
            _propFactory_V1 = null;
            return PropFactory_V1;
        }

        IPropFactory _propFactory_V1;
        public IPropFactory PropFactory_V1
        {
            get
            {
                if(_propFactory_V1 == null)
                {
                    IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates));

                    ITypeDescBasedTConverterCache typeDescBasedTConverterCache = new TypeDescBasedTConverterCache();
                    IConvertValues valueConverter = new PropFactoryValueConverter(typeDescBasedTConverterCache);

                    _propFactory_V1 = new WPFPropFactory
                        (
                        delegateCacheProvider: delegateCacheProvider,
                        valueConverter: valueConverter,
                        typeResolver: null
                        );
                }
                return _propFactory_V1;
            }
        }

        IPropFactory _propFactoryExt_V1;
        public IPropFactory PropFactoryExt_V1
        {
            get
            {
                if (_propFactoryExt_V1 == null)
                {
                    IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates));

                    _propFactoryExt_V1 = new PropExtStoreFactory
                        (
                         delegateCacheProvider: delegateCacheProvider,
                        valueConverter: null,
                        typeResolver: null,
                        stuff: null
                        );
                }
                return _propFactoryExt_V1;
            }
        }

        IProvideAutoMappers _autoMapperProvider_V1;
        public IProvideAutoMappers GetAutoMapperSetup_V1() 
        {
            if(_autoMapperProvider_V1 == null)
            {
                _autoMapperProvider_V1 = new AutoMapperHelpers().InitializeAutoMappers(storeAccessCreator: StoreAccessCreator);
            }
            return _autoMapperProvider_V1;
        }

        ICreateWrapperTypes _wrapperTypeCreator_V1;
        public ICreateWrapperTypes GetWrapperTypeCreator_V1()
        {
            if(_wrapperTypeCreator_V1 == null)
            {
                _wrapperTypeCreator_V1 = BuildSimpleWrapperTypeCreator();
            }
            return _wrapperTypeCreator_V1;
        }

        PropModelCacheInterface _propModelCache_V1;
        public PropModelCacheInterface GetPropModelCache_V1()
        {
            if(_propModelCache_V1 == null)
            {
                IPropFactoryFactory propFactoryFactory = GetThePropFactoryFactory();

                // PropModel Provider
                RemotePropModelProvider remotePropModelProvider = GetPropModelProvider(propFactoryFactory/*, configPackageNameSuffix: null*/);


                // Load the PropBag and Mapper Templates
                LoadPropModelsAndMappers(remotePropModelProvider, _resourceFolderPath, _pbTemplateFilenames, _mapperRequestFilenames);

                _propModelCache_V1 = new SimplePropModelCache(remotePropModelProvider);

            }
            return _propModelCache_V1;
        }

        private IPropFactoryFactory GetThePropFactoryFactory()
        {
            ITypeDescBasedTConverterCache typeDescBasedTConverterCache = new TypeDescBasedTConverterCache();

            IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates));


            IConvertValues valueConverter = new PropFactoryValueConverter(typeDescBasedTConverterCache);
            ResolveTypeDelegate typeResolver = null;

            IPropFactoryFactory result = BuildThePropFactoryFactory(valueConverter, delegateCacheProvider, typeResolver);

            return result;
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

        private RemotePropModelProvider GetPropModelProvider
            (
            IPropFactoryFactory propFactoryFactory
            //, string configPackageNameSuffix
            )
        {
            ResourceDictionaryProvider rdProvider = new ResourceDictionaryProvider();

            PropBagTemplateParser pbtParser = new PropBagTemplateParser();

            RemotePropModelProvider propModelProvider = new RemotePropModelProvider(rdProvider, pbtParser, propFactoryFactory/*, configPackageNameSuffix*/);

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

            remotePropModelProvider.LoadPropModels(resourceFolderPath, pbTemplateFilenames);
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

        //public Type GetTypeFromName(string typeName)
        //{
        //    Type result;
        //    try
        //    {
        //        result = Type.GetType(typeName);
        //    }
        //    catch (ArgumentNullException ane)
        //    {
        //        throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", ane);
        //    }
        //    catch (System.Reflection.TargetInvocationException tie)
        //    {
        //        throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", tie);
        //    }
        //    catch (ArgumentException ae)
        //    {
        //        throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", ae);
        //    }
        //    catch (TypeLoadException tle)
        //    {
        //        throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", tle);
        //    }
        //    catch (System.IO.FileLoadException fle)
        //    {
        //        throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", fle);
        //    }
        //    catch (BadImageFormatException bife)
        //    {
        //        throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", bife);
        //    }

        //    if (result == null)
        //    {
        //        throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}. (No exception was thrown, but GetType returned null.");
        //    }

        //    return result;
        //}

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if(_autoMapperProvider_V1 is IDisposable disable)
                    {
                        disable.Dispose();
                    }

                    this._propFactory_V1 = null;

                    if (_theStore != null) _theStore.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Temp() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
