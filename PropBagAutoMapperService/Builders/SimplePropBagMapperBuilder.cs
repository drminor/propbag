using AutoMapper;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    using PropModelType = IPropModel<String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public class SimplePropBagMapperBuilder<TSource, TDestination> : IPropBagMapperBuilder<TSource, TDestination> where TDestination : class, IPropBag
    {
        #region Private Properties

        private readonly IPropBagMapperService _propBagMapperService;

        #endregion

        #region Constructor

        public SimplePropBagMapperBuilder(IPropBagMapperService propBagMapperService)
        {
            _propBagMapperService = propBagMapperService;
        }

        #endregion

        #region Public Members

        // Create a new PropBagMapper
        public IPropBagMapper<TSource, TDestination> BuildPropBagMapper
            (
            IPropBagMapperRequestKey<TSource, TDestination> mapperRequestKey,
            ViewModelFactoryInterface viewModelFactory
            )
        {
            CheckTypeToCreate("source", typeof(TSource), mapperRequestKey.SourceTypeDef.TargetType);
            CheckTypeToCreate("destination", typeof(TDestination), mapperRequestKey.DestinationTypeDef.TargetType);

            PropModelType propModel = mapperRequestKey.PropModel;
            IMapper theMapper = mapperRequestKey.AutoMapper;

            IPropBagMapper<TSource, TDestination> result
                = new SimplePropBagMapper<TSource, TDestination>
                (
                propModel,
                theMapper,
                viewModelFactory,
                _propBagMapperService,
                mapperRequestKey
                );

            return result;
        }

        public Func<IPropBagMapperRequestKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> PropBagMapperBuilderGen
            => BuildPropBagMapperGen;

        #endregion

        #region Private Methods 

        private IPropBagMapperGen BuildPropBagMapperGen
            (
            IPropBagMapperRequestKeyGen mapRequestGen,
            ViewModelFactoryInterface viewModelFactory
            )
        {
            IPropBagMapperRequestKey<TSource, TDestination> mapRequestTyped
                = mapRequestGen as IPropBagMapperRequestKey<TSource, TDestination>;

            if(mapRequestTyped == null)
            {
                throw new InvalidOperationException($"{nameof(mapRequestGen)} does not implement the correct typed {nameof(IPropBagMapperRequestKey<TSource, TDestination>)} interface.");
            }

            return BuildPropBagMapper(mapRequestTyped, viewModelFactory);
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
