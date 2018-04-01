using AutoMapper;
using System;

namespace Swhp.AutoMapperSupport
{
    public class SimpleAutoMapperBuilder<TSource, TDestination> : IBuildAutoMapper<TSource, TDestination> //where TDestination : class, IPropBag
    {
        #region Private Properties

        private readonly IBuildMapperConfigurations<TSource, TDestination> _mapperConfigurationBuilder;
        private readonly IAutoMapperService _autoMapperService;

        #endregion

        #region Constructor

        public SimpleAutoMapperBuilder
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

        // Create a new AutoMapper (IMapper)
        public IMapper GenerateRawAutoMapper(IAutoMapperRequestKey<TSource, TDestination> mapperRequestKey)
        {
            CheckTypeToCreate(typeof(TSource), mapperRequestKey.SourceTypeDef.TargetType);
            CheckTypeToCreate(typeof(TDestination), mapperRequestKey.DestinationTypeDef.TargetType);

            IConfigurationProvider configProvider = _mapperConfigurationBuilder.GetNewConfiguration(mapperRequestKey);

            IMapper theMapper = configProvider.CreateMapper();

            return theMapper;
        }

        public Func<IAutoMapperRequestKeyGen, IMapper> GenRawAutoMapperCreator => GenerateRawAutoMapperGen;

        #endregion

        #region Private Methods 

        private IMapper GenerateRawAutoMapperGen(IAutoMapperRequestKeyGen mapRequestGen)
        {
            IAutoMapperRequestKey<TSource, TDestination> mapRequestTyped = mapRequestGen as IAutoMapperRequestKey<TSource, TDestination>;

            if (mapRequestTyped == null)
            {
                throw new InvalidOperationException($"{nameof(mapRequestGen)} does not implement the correct typed {nameof(IAutoMapperRequestKey<TSource, TDestination>)} interface.");
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
