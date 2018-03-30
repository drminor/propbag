﻿using AutoMapper;
using DRM.TypeSafePropertyBag;
using System;
using System.Reflection;
using System.Threading;

namespace DRM.PropBag.AutoMapperSupport
{
    using PropModelType = IPropModel<String>;

    public class SimpleAutoMapperProvider : IAutoMapperService, IDisposable
    {
        #region Private Members

        private readonly IMapTypeDefinitionProvider _mapTypeDefinitionProvider;
        private readonly ICacheAutoMappers _autoMapperCache;
        private readonly IAutoMapperBuilderProvider _autoMapperBuilderProvider;

        #endregion

        #region Constructors

        // Disallow the parameterless constructor.
        private SimpleAutoMapperProvider()
        {
            throw new NotSupportedException("Use of the paremeterless constructor for SimpleAutoMapperProvider is not supported.");
        }

        public SimpleAutoMapperProvider
        (
            IMapTypeDefinitionProvider mapTypeDefinitionProvider,
            IAutoMapperBuilderProvider autoMapperBuilderProvider,
            ICacheAutoMappers autoMapperCache)
        {
            _mapTypeDefinitionProvider = mapTypeDefinitionProvider ?? throw new ArgumentNullException(nameof(mapTypeDefinitionProvider));
            _autoMapperBuilderProvider = autoMapperBuilderProvider ?? throw new ArgumentNullException(nameof(autoMapperBuilderProvider));
            _autoMapperCache = autoMapperCache ?? throw new ArgumentNullException(nameof(autoMapperCache));
        }

        #endregion

        #region Public Methods

        // Gen Submit
        public IAutoMapperRequestKeyGen SubmitRawAutoMapperRequest(PropModelType propModel, Type sourceType, string configPackageName)
        {
            Type typeToCreate = propModel.NewEmittedType ?? propModel.TypeToWrap;
            AutoMapperReqSubDelegate mapperRequestSubmitter = GetAutoMapperReqSubDelegate(sourceType, typeToCreate);
            IAutoMapperRequestKeyGen result = mapperRequestSubmitter(propModel, configPackageName, this);

            return result;
        }

        // Typed Submit Raw Auto
        // TODO: Consider adding a method that takes a IConfigureAMapper instead of a configPackageName.
        public IAutoMapperRequestKey<TSource, TDestination> SubmitRawAutoMapperRequest<TSource, TDestination>
        (
            PropModelType propModel,
            string configPackageName,
            IHaveAMapperConfigurationStep configStarterForThisRequest = null,
            IPropFactory propFactory = null
        )
            where TDestination : class, IPropBag
        {
            // TODO: check to make sure that the "configStarterForThisRequest" value is being sent to the correct place.
            // TODO: Consider making the caller supply a IBuildMapperConfigurations "service."

            // Create a Configuration Builder for this request.
            IBuildMapperConfigurations<TSource, TDestination> propBagMapperConfigurationBuilder
                = new SimpleMapperConfigurationBuilder<TSource, TDestination>(configStarter: configStarterForThisRequest);

            // Create a MapperBuilder for this request.
            IBuildAutoMapper<TSource, TDestination> autoMapperBuilder
                = _autoMapperBuilderProvider.GetAutoMapperBuilder<TSource, TDestination>(propBagMapperConfigurationBuilder, this);

            // Lookup the package name and return a mapping configuration.
            IConfigureAMapper<TSource, TDestination> mappingConfiguration
                = GetMappingConfiguration<TSource, TDestination>(configPackageName);

            IMapTypeDefinition<TSource> srcMapTypeDef
                = _mapTypeDefinitionProvider.GetTypeDescription<TSource>(propModel, propFactory: propFactory, className: null);

            IMapTypeDefinition<TDestination> dstMapTypeDef
                = _mapTypeDefinitionProvider.GetTypeDescription<TDestination>(propModel, propFactory: propFactory, className: null);

            // Create the mapper request.
            IAutoMapperRequestKey<TSource, TDestination> typedMapperRequest
                = new AutoMapperRequestKey<TSource, TDestination>
                (
                    autoMapperBuilder: autoMapperBuilder,
                    mappingConfiguration: mappingConfiguration,
                    sourceMapTypeDef: srcMapTypeDef,
                    destinationMapTypeDef: dstMapTypeDef
                );

            IAutoMapperRequestKeyGen newMapRequest_raw = RegisterRawAutoMapperRequest(typedMapperRequest);
            return (IAutoMapperRequestKey<TSource, TDestination>)newMapRequest_raw;
        }

        // Typed Get Mapper (Simple wrapper around our pass through method.)
        public IMapper GetRawAutoMapper<TSource, TDestination>(IAutoMapperRequestKey<TSource, TDestination> mapperRequest)
            where TDestination : class, IPropBag
        {
            return _autoMapperCache.GetRawAutoMapper(mapperRequest);
        }

        #endregion

        #region Pass-through calls to the Raw Auto MappersCache

        public IAutoMapperRequestKeyGen RegisterRawAutoMapperRequest(IAutoMapperRequestKeyGen mapperRequest)
        {
            return _autoMapperCache.RegisterRawAutoMapperRequest(mapperRequest);
        }

        public IMapper GetRawAutoMapper(IAutoMapperRequestKeyGen mapperRequest)
        {
            return _autoMapperCache.GetRawAutoMapper(mapperRequest);
        }

        public long ClearRawAutoMappersCache()
        {
            return _autoMapperCache.ClearRawAutoMappersCache();
        }

        #endregion

        #region Private Methods

        private IConfigureAMapper<TSource, TDestination> GetMappingConfiguration<TSource, TDestination>(string configPackageName) where TDestination : class, IPropBag
        {
            switch (configPackageName.ToLower())
            {
                case "extra_members":
                    {
                        return new ConfigPackage_ExtraMembers().GetTheMapperConfig<TSource, TDestination>();
                    }
                case "emit_proxy":
                    {
                        return new ConfigPackage_EmitProxy().GetTheMapperConfig<TSource, TDestination>();
                    }
                default:
                    {
                        throw new InvalidOperationException($"The configPackageName: {configPackageName} is not recognized.");
                    }
            }
        }

        #endregion

        #region Generic Method Support

        internal delegate IAutoMapperRequestKeyGen AutoMapperReqSubDelegate
            (PropModelType propModel, string configPackageName, IAutoMapperService autoMapperService);

        static AutoMapperReqSubDelegate GetAutoMapperReqSubDelegate(Type sourceType, Type destinationType)
        {
            MethodInfo TypedRegisterRawAutoMapperRequest_MI = 
                GenericMethodTemplates.RawAutoMapperReqSubmitter_MI.MakeGenericMethod(sourceType, destinationType);

            AutoMapperReqSubDelegate result = (AutoMapperReqSubDelegate)Delegate.CreateDelegate(typeof(AutoMapperReqSubDelegate), TypedRegisterRawAutoMapperRequest_MI);
            return result;
        }

        /// <summary>
        /// Used to create Delegates when the type of the value is not known at run time.
        /// </summary>
        static private Type GMT_TYPE = typeof(GenericMethodTemplates);

        // Method Templates for Property Bag
        internal static class GenericMethodTemplates
        {
            static Lazy<MethodInfo> rawAutoMapperReqSubmitter_Lazy_MI;
            public static MethodInfo RawAutoMapperReqSubmitter_MI { get { return rawAutoMapperReqSubmitter_Lazy_MI.Value; } }

            static GenericMethodTemplates()
            {
                rawAutoMapperReqSubmitter_Lazy_MI = new Lazy<MethodInfo>(() =>
                    GMT_TYPE.GetMethod("SubmitRawAutoMapperRequest", BindingFlags.Static | BindingFlags.NonPublic),
                    LazyThreadSafetyMode.PublicationOnly);
            }

            // The Typed method for RawAutoMappers
            static IAutoMapperRequestKey<TSource, TDestination> SubmitRawAutoMapperRequest<TSource, TDestination>
            (
                PropModelType propModel,
                string configPackageName,
                IAutoMapperService autoMapperProvider
            )
            where TDestination : class, IPropBag
            {
                IAutoMapperRequestKey<TSource, TDestination> result
                    = autoMapperProvider.SubmitRawAutoMapperRequest<TSource, TDestination>
                        (
                        propModel: propModel,
                        configPackageName: configPackageName,
                        configStarterForThisRequest: null,
                        propFactory: null
                        );

                return result;
            }
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    ClearRawAutoMappersCache();

                    if(_mapTypeDefinitionProvider is IDisposable disable1)
                    {
                        disable1.Dispose();
                    }

                    if (_autoMapperCache is IDisposable disable2)
                    {
                        disable2.Dispose();
                    }

                    if (_autoMapperBuilderProvider is IDisposable disable3)
                    {
                        disable3.Dispose();
                    }
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