using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;

namespace PropBagLib.Tests.AutoMapperSupport
{
    public class AutoMapperHelpers
    {
        public AutoMapperProvider InitializeAutoMappers(IPropModelProvider propModelProvider)
        {
            IPropBagMapperBuilderProvider propBagMapperBuilderProvider
                = new SimplePropBagMapperBuilderProvider
                (
                    wrapperTypeCreator: null,
                    viewModelActivator: null
                );

            IMapTypeDefinitionProvider mapTypeDefinitionProvider = new SimpleMapTypeDefinitionProvider();

            ICachePropBagMappers mappersCachingService = new SimplePropBagMapperCache();

            AutoMapperProvider autoMapperProvider = new AutoMapperProvider
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

        IPropFactory _propFactory_V1;
        public IPropFactory PropFactory_V1
        {
            get
            {
                if(_propFactory_V1 == null)
                {
                    _propFactory_V1 = new PropFactory(false, GetTypeFromName, null);
                }
                return _propFactory_V1;
            }
        }

        AutoMapperProvider _autoMapperProvider_V1;
        public AutoMapperProvider GetAutoMapperSetup_V1() 
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

    }
}
