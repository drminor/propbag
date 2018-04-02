using AutoMapper;
using System;

namespace Swhp.AutoMapperSupport
{
    public class SimpleAutoMapperBuilder<TSource, TDestination> : IAutoMapperBuilder<TSource, TDestination>
    {
        #region Private Properties

        private readonly IMapperConfigurationBuilder<TSource, TDestination> _mapperConfigurationBuilder;

        #endregion

        #region Constructor

        public SimpleAutoMapperBuilder
            (
            IMapperConfigurationBuilder<TSource, TDestination> mapperConfigurationBuilder
            )
        {
            _mapperConfigurationBuilder = mapperConfigurationBuilder;
        }

        #endregion

        #region Public Members

        // Create a new AutoMapper (IMapper)
        public IMapper BuildAutoMapper(IAutoMapperRequestKey<TSource, TDestination> mapperRequestKey)
        {
            CheckTypeToCreate("source", typeof(TSource), mapperRequestKey.SourceTypeDef.TargetType);
            CheckTypeToCreate("destination", typeof(TDestination), mapperRequestKey.DestinationTypeDef.TargetType);

            IConfigurationProvider configProvider = _mapperConfigurationBuilder.GetNewConfiguration(mapperRequestKey);

            IMapper result = configProvider.CreateMapper();

            return result;
        }

        public Func<IAutoMapperRequestKeyGen, IMapper> AutoMapperBuilderGen => BuildAutoMapperGen;

        #endregion

        #region Private Methods 

        private IMapper BuildAutoMapperGen(IAutoMapperRequestKeyGen mapRequestGen)
        {
            IAutoMapperRequestKey<TSource, TDestination> mapRequestTyped
                = mapRequestGen as IAutoMapperRequestKey<TSource, TDestination>;

            if (mapRequestTyped == null)
            {
                throw new InvalidOperationException($"{nameof(mapRequestGen)} does not implement the correct typed {nameof(IAutoMapperRequestKey<TSource, TDestination>)} interface.");
            }

            return BuildAutoMapper(mapRequestTyped);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckTypeToCreate(string parameterName, Type typeParameter, Type typeFromPropModel)
        {
            if (typeParameter != typeFromPropModel)
            {
                throw new InvalidOperationException($"The {parameterName} type parameter: {typeParameter} does not match the PropModel's TypeToCreate: {typeFromPropModel}.");
            }
        }

        #endregion
    }
}
