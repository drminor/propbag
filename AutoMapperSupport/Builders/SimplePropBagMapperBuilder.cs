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

        // Create a new PropBagMapper
        public IPropBagMapper<TSource, TDestination> GeneratePropBagMapper
            (
            IPropBagMapperKey<TSource, TDestination> mapperRequestKey,
            ViewModelFactoryInterface viewModelFactory
            )
        {
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

        // Create a new AutoMapper (IMapper)
        public IMapper GenerateRawAutoMapper(IPropBagMapperKey<TSource, TDestination> mapperRequestKey)
        {
            CheckTypeToCreate(typeof(TSource), mapperRequestKey.SourceTypeDef.TargetType);
            CheckTypeToCreate(typeof(TDestination), mapperRequestKey.DestinationTypeDef.TargetType);

            IConfigurationProvider configProvider = _mapperConfigurationBuilder.GetNewConfiguration(mapperRequestKey);

            IMapper theMapper = configProvider.CreateMapper();

            return theMapper;
        }

        public Func<IPropBagMapperKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> GenPropBagMapperCreator => GeneratePropBagMapperGen;

        public Func<IPropBagMapperKeyGen, IMapper> GenRawAutoMapperCreator => GenerateRawAutoMapperGen;

        #endregion

        #region Private Methods 

        private IPropBagMapperGen GeneratePropBagMapperGen(IPropBagMapperKeyGen mapRequestGen, ViewModelFactoryInterface viewModelFactory)
        {
            IPropBagMapperKey<TSource, TDestination> mapRequestTyped = mapRequestGen as IPropBagMapperKey<TSource, TDestination>;

            if(mapRequestTyped == null)
            {
                throw new InvalidOperationException($"{nameof(mapRequestGen)} does not implement the correct typed {nameof(IPropBagMapperKey<TSource, TDestination>)} interface.");
            }

            return GeneratePropBagMapper(mapRequestTyped, viewModelFactory);
        }

        private IMapper GenerateRawAutoMapperGen(IPropBagMapperKeyGen mapRequestGen)
        {
            IPropBagMapperKey<TSource, TDestination> mapRequestTyped = mapRequestGen as IPropBagMapperKey<TSource, TDestination>;

            if (mapRequestTyped == null)
            {
                throw new InvalidOperationException($"{nameof(mapRequestGen)} does not implement the correct typed {nameof(IPropBagMapperKey<TSource, TDestination>)} interface.");
            }

            return GenerateRawAutoMapper(mapRequestTyped);
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
