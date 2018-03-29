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
        private readonly IAutoMapperService _autoMapperService;

        #endregion

        #region Constructor

        public SimplePropBagMapperBuilder
            (
            IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder,
            IAutoMapperService autoMapperService
            )
        {
            _mapperConfigurationBuilder = mapperConfigurationBuilder;
            _autoMapperService = autoMapperService;
        }

        #endregion

        #region Public Members

        public IPropBagMapper<TSource, TDestination> GeneratePropBagMapper
            (
            IPropBagMapperKey<TSource, TDestination> mapperRequestKey,
            ViewModelFactoryInterface viewModelFactory
            )
        {
            //Type TargetRunTimeType = mapperRequestKey.DestinationTypeDef.NewEmittedType ?? mapperRequestKey.DestinationTypeDef.TargetType;

            CheckTypeToCreate(typeof(TSource), mapperRequestKey.SourceTypeDef.TargetType);
            CheckTypeToCreate(typeof(TDestination), mapperRequestKey.DestinationTypeDef.TargetType);

            IMapper theMapper = mapperRequestKey.AutoMapper;

            IPropBagMapper<TSource, TDestination> result = new SimplePropBagMapper<TSource, TDestination>
                (
                mapperRequestKey,
                theMapper,
                viewModelFactory,
                _autoMapperService
                );

            return result;
        }

        public IMapper GenerateRawAutoMapperTyped(IPropBagMapperKey<TSource, TDestination> mapperRequestKey)
        {
            //Type TargetRunTimeType = mapperRequestKey.DestinationTypeDef.NewEmittedType ?? mapperRequestKey.DestinationTypeDef.TargetType;
            Type TargetRunTimeType = mapperRequestKey.DestinationTypeDef.RunTimeType;

            CheckTypeToCreate(typeof(TDestination), mapperRequestKey.DestinationTypeDef.TargetType);

            IConfigurationProvider configProvider = _mapperConfigurationBuilder.GetNewConfiguration(mapperRequestKey);

            IMapper theMapper = configProvider.CreateMapper();

            return theMapper;
        }

        public Func<IPropBagMapperKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> GenMapperCreator => GenerateMapperGen;

        public Func<IPropBagMapperKeyGen, IMapper> RawAutoMapperCreator => GenerateRawAutoMapper;

        #endregion

        #region Private Methods 

        private IPropBagMapperGen GenerateMapperGen(IPropBagMapperKeyGen mapRequestGen, ViewModelFactoryInterface viewModelFactory)
        {
            IPropBagMapperKey<TSource, TDestination> mapRequestTyped = mapRequestGen as IPropBagMapperKey<TSource, TDestination>;

            if(mapRequestTyped == null)
            {
                throw new InvalidOperationException($"{nameof(mapRequestGen)} does not implement the correct typed {nameof(IPropBagMapperKey<TSource, TDestination>)} interface.");
            }

            return GeneratePropBagMapper(mapRequestTyped, viewModelFactory);
        }

        private IMapper GenerateRawAutoMapper(IPropBagMapperKeyGen mapRequestGen)
        {
            IPropBagMapperKey<TSource, TDestination> mapRequestTyped = mapRequestGen as IPropBagMapperKey<TSource, TDestination>;

            if (mapRequestTyped == null)
            {
                throw new InvalidOperationException($"{nameof(mapRequestGen)} does not implement the correct typed {nameof(IPropBagMapperKey<TSource, TDestination>)} interface.");
            }

            return GenerateRawAutoMapperTyped(mapRequestTyped);
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
