﻿using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.Caches;
using DRM.PropBagWPF;
using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;
using System;

namespace PropBagLib.Tests.AutoMapperSupport
{
    using PropIdType = UInt32;
    using PropNameType = String;

    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class AutoMapperHelpers : IDisposable
    {
        // Maximum number of PropertyIds for any one given Object.
        private const int LOG_BASE2_MAX_PROPERTIES = 16;
        public static readonly int MAX_NUMBER_OF_PROPERTIES = (int)Math.Pow(2, LOG_BASE2_MAX_PROPERTIES); //65536;

        public IProvideAutoMappers InitializeAutoMappers(PSAccessServiceCreatorInterface storeAccessCreator)
        {
            IViewModelActivator vmActivator = new SimpleViewModelActivator();

            IPropBagMapperBuilderProvider propBagMapperBuilderProvider
                = new SimplePropBagMapperBuilderProvider
                (
                    wrapperTypesCreator: null,
                    viewModelActivator: vmActivator,
                    storeAccessCreator: storeAccessCreator
                );

            IMapTypeDefinitionProvider mapTypeDefinitionProvider = new SimpleMapTypeDefinitionProvider();

            ICachePropBagMappers mappersCachingService = new SimplePropBagMapperCache();

            SimpleAutoMapperProvider autoMapperProvider = new SimpleAutoMapperProvider
                (
                mapTypeDefinitionProvider: mapTypeDefinitionProvider,
                mappersCachingService: mappersCachingService,
                mapperBuilderProvider: propBagMapperBuilderProvider,
                propModelProvider: null
                );

            return autoMapperProvider;
        }

        PSAccessServiceCreatorInterface _storeAccessCreator;
        public PSAccessServiceCreatorInterface StoreAccessCreator
        {
            get
            {
                if (_storeAccessCreator == null)
                {
                    IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();

                    _storeAccessCreator = new SimplePropStoreServiceEP(
                        MAX_NUMBER_OF_PROPERTIES,
                        handlerDispatchDelegateCacheProvider);
                }
                return _storeAccessCreator;
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
                IPropFactory propFactory = PropFactory_V1;

                _autoMapperProvider_V1 = new AutoMapperHelpers().InitializeAutoMappers(storeAccessCreator: StoreAccessCreator);
            }
            return _autoMapperProvider_V1;
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
                    this._autoMapperProvider_V1 = null;
                    this._propFactory_V1 = null;
                    this._storeAccessCreator = null;
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
