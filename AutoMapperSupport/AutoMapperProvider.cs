using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using DRM.TypeWrapper;
using DRM.ViewModelTools;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public class AutoMapperProvider : ICachePropBagMappers
    {
        #region Private Members

        IPropModelProvider _propModelProvider { get; }

        string NO_PBT_CONVERSION_SERVICE_MSG = $"The {nameof(ViewModelActivatorStandard)} has no PropModelProvider." +
            $"All calls must provide a PropModel.";

        IMapTypeDefinitionProvider _mapTypeDefinitionProvider { get; }

        IViewModelActivator _standardActivator;
        IViewModelActivator _emitProxyActivator;

        ICachePropBagMappers _mappersCachingService { get; }

        IPropFactory _defaultPropFactory { get; }

        #endregion

        #region Constructors

        private AutoMapperProvider() { } // Disallow the parameterless constructor.

        public AutoMapperProvider(IPropModelProvider propModelProvider,
            IMapTypeDefinitionProvider mapTypeDefinitionProvider,
            IViewModelActivator standardActivator,
            IViewModelActivator emitProxyActivator,
            ICachePropBagMappers mappersCachingService,
            IPropFactory defaultPropFactory)
        {
            _propModelProvider = propModelProvider ?? throw new ArgumentNullException(nameof(propModelProvider));
            _mapTypeDefinitionProvider = mapTypeDefinitionProvider ?? throw new ArgumentNullException(nameof(mapTypeDefinitionProvider));
            _standardActivator = standardActivator ?? throw new ArgumentNullException(nameof(standardActivator));
            _emitProxyActivator = emitProxyActivator ?? throw new ArgumentNullException(nameof(emitProxyActivator));
            _mappersCachingService = mappersCachingService ?? throw new ArgumentNullException(nameof(mappersCachingService));
            _defaultPropFactory = defaultPropFactory;

            if (_defaultPropFactory == null)
            {
                System.Diagnostics.Debug.WriteLine($"{nameof(AutoMapperProvider)} was not given a default IPropFactory.");
            }
        }

        #endregion

        #region Public Properties
        public bool HasPbTConversionService => (_propModelProvider != null);
        #endregion

        #region Public Methods

        public IPropBagMapperKey<TSource, TDestination> RegisterMapperRequest<TSource, TDestination>
            (
            string resourceKey,
            Type typeToWrap,
            IPropFactory propFactory
            ) where TDestination : class, IPropBag
        {
            PropModel propModel = GetPropModel(resourceKey);

            IPropBagMapperKey<TSource, TDestination> typedMapperRequest = CreateTypedMapperRequest<TSource, TDestination>
                (
                propModel,
                typeToWrap,
                propFactory ?? _defaultPropFactory ?? throw new InvalidOperationException("No PropFactory was provided and no Default PropFactory was specified upon construction.")
                );

            this.RegisterMapperRequest(typedMapperRequest);

            return typedMapperRequest;
        }

        public IPropBagMapper<TSource,TDestination> GetMapper<TSource, TDestination>(IPropBagMapperKey<TSource, TDestination> mapRequest) where TDestination : class, IPropBag
        {
            return (IPropBagMapper<TSource, TDestination>)_mappersCachingService.GetMapper(mapRequest);
        }

        public void RegisterMapperRequest(IPropBagMapperKeyGen mapRequest)
        {
            _mappersCachingService.RegisterMapperRequest(mapRequest);
        }

        public IPropBagMapperGen GetMapper(IPropBagMapperKeyGen mapRequest)
        {
            return _mappersCachingService.GetMapper(mapRequest);
        }

        #endregion

        #region Private Methods

        private IPropBagMapperKey<TSource, TDestination> CreateTypedMapperRequest<TSource, TDestination>
            (
            PropModel propModel,
            Type typeToWrap,
            IPropFactory propFactory
            ) where TDestination : class, IPropBag
        {
            #region Mapper Configuration Work
            IMapperConfigurationStepGen configStarter = new MapperConfigStarter_Default();


            IBuildMapperConfigurations<TSource, TDestination> configBuilder
                = new SimpleMapperConfigurationBuilder<TSource, TDestination>
                (); //(configStarter); // Uses the same configStarter for all builds.

            IConfigureAMapper<TSource, TDestination> mappingConf
                = new SimpleMapperConfiguration<TSource, TDestination>
                (configBuilder, configStarter) // Uses the configStarter for just this build.
                {
                    FinalConfigAction = new StandardConfigFinalStep<TSource, TDestination>().ConfigurationStep,
                };

            PropBagMapperBuilder<TSource, TDestination> mapperBuilder
                = new PropBagMapperBuilder<TSource, TDestination>
                (mapperConfiguration: mappingConf, vmActivator: _standardActivator);
            #endregion

            IMapTypeDefinition<TSource> sourceMapTypeDef = _mapTypeDefinitionProvider.GetTypeDescription<TSource>
                (propModel, propFactory, typeToWrap, null);

            IMapTypeDefinition<TDestination> destinationMapTypeDef = _mapTypeDefinitionProvider.GetTypeDescription<TDestination>
                (propModel, propFactory, typeToWrap, null);

            IPropBagMapperKey<TSource, TDestination> result = new PropBagMapperKey<TSource, TDestination>
                (propBagMapperBuilder: mapperBuilder,
                //mappingConfiguration: mappingConf,
                sourceMapTypeDef: sourceMapTypeDef,
                destinationMapTypeDef: destinationMapTypeDef,
                sourceConstructor: null,
                destinationConstructor: null);

            return result;
        }

        private PropModel GetPropModel(string resourceKey)
        {
            if (!HasPbTConversionService)
            {
                throw new InvalidOperationException(NO_PBT_CONVERSION_SERVICE_MSG);
            }

            PropModel propModel = _propModelProvider.GetPropModel(resourceKey);
            return propModel;
        }

        #endregion

        #region GET SIZE

        private void GetSizeX()
        {
            long StopBytes = 0;
            object myFoo;

            long StartBytes = System.GC.GetTotalMemory(true);
            myFoo = new object();
            StopBytes = System.GC.GetTotalMemory(true);

            string result = "Size is " + ((long)(StopBytes - StartBytes)).ToString();

            GC.KeepAlive(myFoo); // This ensure a reference to object keeps object in memory
        }

        private void GetSizeY()
        {

            //long size = 0;
            //object o = new object();
            //using (Stream s = new MemoryStream())
            //{
            //    BinaryFormatter formatter = new BinaryFormatter();
            //    formatter.Serialize(s, o);
            //    size = s.Length;
            //}
        }

        #endregion

        // TODO: Consider supporting finding the PBT requied for a mapper by class name.
        //public IPropBagMapper<TSource, TDestination> GetMapper<TSource, TDestination>(string instanceKey)
        //{
        //    IPropBagMapper<TSource, TDestination> result = null;

        //    Type dtViewModelType = typeof(TDestination);

        //    PropModel propModel = _propModelProvider.GetPropModel(instanceKey);

        //    // Extra Members
        //    if (_mappingStrategy == PropBagMappingStrategyEnum.ExtraMembers)
        //    {
        //        // TODO!: DO WE NEED THIS?
        //        TypeDescription typeDescription = _typeDescriptionProvider.BuildTypeDesc(propModel,dtViewModelType);

        //        if (typeDescription == null)
        //        {
        //            System.Diagnostics.Debug.WriteLine($"Could not build a TypeDescription from the PropMode with instance key = {instanceKey}.");
        //            return result;
        //        }

        //        Type rtViewModelType = _propBagProxyBuilder.BuildVmProxyClass(typeDescription);
        //        // END -- DO WE NEED THIS?

        //        //Type rtViewModelType = typeof(TDestination); //typeof(ReferenceBindViewModelPB);

        //        IPropBagMapperKey<TSource, TDestination> mapperRequest
        //            = new PropBagMapperKey<TSource, TDestination>(propModel, rtViewModelType, _mappingStrategy);
        //    }

        //    // Emit Proxy
        //    else if (_mappingStrategy == PropBagMappingStrategyEnum.EmitProxy)
        //    {
        //        TypeDescription typeDescription = _typeDescriptionProvider.BuildTypeDesc(propModel, dtViewModelType);

        //        if (typeDescription == null)
        //        {
        //            System.Diagnostics.Debug.WriteLine($"Could not build a TypeDescription from the PropMode with instance key = {instanceKey}.");
        //            return result;
        //        }

        //        Type rtViewModelType = _propBagProxyBuilder.BuildVmProxyClass(typeDescription);

        //        IPropBagMapperKey<TSource, TDestination> mapperRequest
        //            = new PropBagMapperKey<TSource, TDestination>(propModel, rtViewModelType, _mappingStrategy);

        //        _mappersCachingService.Register(mapperRequest);
        //        result = (IPropBagMapper<TSource, TDestination>)_mappersCachingService.GetMapperToUse(mapperRequest);
        //    }

        //    // Emit Wrapper
        //    //else if(mappingStrategy == PropBagMappingStrategyEnum.EmitWrapper)
        //    //{

        //    //}
        //    else
        //    {
        //        throw new InvalidOperationException($"The mapping strategy: {_mappingStrategy} is not recognized or is not supported.");
        //    }

        //    if (result == null)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Could not get an AutoMapper for <{typeof(TSource)},{typeof(TDestination)}>");
        //    }
        //    return result;
        //}
    }
}
