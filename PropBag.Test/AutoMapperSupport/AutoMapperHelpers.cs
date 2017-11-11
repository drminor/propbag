using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace PropBagLib.Tests.AutoMapperSupport
{
    public class AutoMapperHelpers
    {
        public SimpleAutoMapperProvider InitializeAutoMappers(IPropModelProvider propModelProvider)
        {
            IPropBagMapperBuilderProvider propBagMapperBuilderProvider
                = new SimplePropBagMapperBuilderProvider
                (
                    wrapperTypeCreator: null,
                    viewModelActivator: null
                );

            IMapTypeDefinitionProvider mapTypeDefinitionProvider = new SimpleMapTypeDefinitionProvider();

            ICachePropBagMappers mappersCachingService = new SimplePropBagMapperCache();

            SimpleAutoMapperProvider autoMapperProvider = new SimpleAutoMapperProvider
                (
                mapTypeDefinitionProvider: mapTypeDefinitionProvider,
                mappersCachingService: mappersCachingService,
                mapperBuilderProvider: propBagMapperBuilderProvider,
                propModelProvider: propModelProvider
                );

            return autoMapperProvider;
        }

        //IPropModelProvider _propModelProvider_V1;
        //public IPropModelProvider PropModelProvider_V1
        //{
        //    get
        //    {
        //        if(_propModelProvider_V1 == null)
        //        {
        //            _propModelProvider_V1 = new PropModelProvider();
        //        }
        //        return _propModelProvider_V1;
        //    }
        //}

        private SimpleLevel2KeyMan _level2KeyManager;
        private SimpleCompKeyMan _compKeyManager;
        private SimpleObjectIdDictionary<IPropGen> _theStore;

        SimplePropStoreAccessServiceProvider<IPropBag, IPropGen> PropStoreAccessServiceProvider { get; set; }

        IPropFactory _propFactory_V1;
        public IPropFactory PropFactory_V1
        {
            get
            {
                if(_propFactory_V1 == null)
                {
                    _theStore = ProvisonTheStore(out _level2KeyManager, out _compKeyManager);

                    PropStoreAccessServiceProvider = new SimplePropStoreAccessServiceProvider<IPropBag, IPropGen>
                        (_theStore, _compKeyManager, _level2KeyManager);

                    _propFactory_V1 = new PropFactory
                        (
                        propStoreAccessServiceProvider: PropStoreAccessServiceProvider,
                        typeResolver: GetTypeFromName,
                        valueConverter: null
                        );
                }
                return _propFactory_V1;
            }
        }

        SimpleAutoMapperProvider _autoMapperProvider_V1;
        public SimpleAutoMapperProvider GetAutoMapperSetup_V1() 
        {
            if(_autoMapperProvider_V1 == null)
            {
                //IPropModelProvider propModelProvider = PropModelProvider_V1;

                IPropFactory propFactory = PropFactory_V1;
                _autoMapperProvider_V1 = new AutoMapperHelpers().InitializeAutoMappers(propModelProvider: null);
            }
            return _autoMapperProvider_V1;
        }

        public Type GetTypeFromName(string typeName)
        {
            Type result;
            try
            {
                result = Type.GetType(typeName);
            }
            catch (System.Exception e)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", e);
            }

            if (result == null)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.");
            }

            return result;
        }

        private static SimpleObjectIdDictionary<IPropGen> ProvisonTheStore(out SimpleLevel2KeyMan level2KeyMan, out SimpleCompKeyMan compKeyManager)
        {
            level2KeyMan = new SimpleLevel2KeyMan(MAX_NUMBER_OF_PROPERTIES);
            compKeyManager = new SimpleCompKeyMan(level2KeyMan);

            SimpleObjectIdDictionary<IPropGen> result = new SimpleObjectIdDictionary<IPropGen>(compKeyManager, level2KeyMan);
            return result;
        }

        // Maximum number of PropertyIds for any one given Object.
        private const int LOG_BASE2_MAX_PROPERTIES = 16;
        public static readonly int MAX_NUMBER_OF_PROPERTIES = (int)Math.Pow(2, LOG_BASE2_MAX_PROPERTIES); //65536;

    }
}
