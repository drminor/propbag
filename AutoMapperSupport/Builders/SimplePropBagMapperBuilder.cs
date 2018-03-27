using AutoMapper;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public class SimplePropBagMapperBuilder<TSource, TDestination> : IBuildPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        #region Private Properties

        private readonly IBuildMapperConfigurations<TSource, TDestination> _mapperConfigurationBuilder;

        //private readonly ViewModelActivatorInterface _viewModelActivator;
        //private readonly PSAccessServiceCreatorInterface _storeAccessCreator;
        //private readonly IProvideAutoMappers _autoMapperService;
        //private readonly ICreateWrapperTypes _wrapperTypeCreator;

        #endregion

        //private ICreateWrapperTypes _wrapperTypeCreator { get; }

        #region Constructor

        public SimplePropBagMapperBuilder
            (
            IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder
            //,
            //ViewModelActivatorInterface viewModelActivator,
            //PSAccessServiceCreatorInterface storeAccessCreator,
            //IProvideAutoMappers autoMapperService,
            //ICreateWrapperTypes wrapperTypeCreator
            )
        {
            _mapperConfigurationBuilder = mapperConfigurationBuilder;
            //_viewModelActivator = viewModelActivator;
            //_storeAccessCreator = storeAccessCreator;
            //_autoMapperService = autoMapperService;
            //_wrapperTypeCreator = wrapperTypeCreator;
        }

        #endregion

        #region Public Members

        public IPropBagMapper<TSource, TDestination> GenerateMapper(IPropBagMapperKey<TSource, TDestination> mapperRequestKey, ViewModelFactoryInterface viewModelFactory)
        {
            // TODO: Add 'virtual' property to IMapTypeDefinition named: TargetRunTimeType.
            Type TargetRunTimeType = mapperRequestKey.DestinationTypeDef.NewEmittedType ?? mapperRequestKey.DestinationTypeDef.TargetType;

            CheckTypeToCreate(typeof(TDestination), mapperRequestKey.DestinationTypeDef.TargetType);

            IConfigurationProvider configProvider = _mapperConfigurationBuilder.GetNewConfiguration(mapperRequestKey);

            IMapper theMapper = configProvider.CreateMapper();

            IPropBagMapper<TSource, TDestination> result = new SimplePropBagMapper<TSource, TDestination>
                (
                mapperRequestKey,
                theMapper,
                //_viewModelActivator,
                //_storeAccessCreator,
                //_autoMapperService,
                //_wrapperTypeCreator
                viewModelFactory
                );

            return result;
        }

        public Func<IPropBagMapperKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> GenMapperCreator => GenerateMapperGen;

        #endregion

        #region Private Methods 

        private IPropBagMapperGen GenerateMapperGen(IPropBagMapperKeyGen mapRequestGen, ViewModelFactoryInterface viewModelFactory)
        {
            IPropBagMapperKey<TSource, TDestination> mapRequestTyped = mapRequestGen as IPropBagMapperKey<TSource, TDestination>;

            if(mapRequestTyped == null)
            {
                throw new InvalidOperationException($"{nameof(mapRequestGen)} does not implement the correct typed {nameof(IPropBagMapperKey<TSource, TDestination>)} interface.");
            }

            return GenerateMapper(mapRequestTyped, viewModelFactory);
        }


        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckTypeToCreate(Type typeParameter, Type typeFromPropModel)
        {
            if (typeParameter != typeFromPropModel)
            {
                throw new InvalidOperationException($"The type parameter: {typeParameter} does not match the PropModel's TypeToCreate: {typeFromPropModel}.");
            }
        }

        #endregion
    }
}
