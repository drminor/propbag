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
        private readonly IPropBagMapperService _propBagMapperService;

        #endregion

        #region Constructor

        public SimplePropBagMapperBuilder
            (
            IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder,
            IPropBagMapperService propBagMapperService
            )
        {
            _mapperConfigurationBuilder = mapperConfigurationBuilder;
            _propBagMapperService = propBagMapperService;
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
                _propBagMapperService
                );

            return result;
        }

        public Func<IPropBagMapperKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> GenPropBagMapperCreator => GeneratePropBagMapperGen;

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
