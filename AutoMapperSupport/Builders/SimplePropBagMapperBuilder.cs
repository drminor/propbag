using AutoMapper;
using DRM.PropBag.TypeWrapper;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class SimplePropBagMapperBuilder<TSource, TDestination>
        : IBuildPropBagMapper<TSource, TDestination>
        where TDestination : class, IPropBag
    {
        #region Private Properties

        private readonly IBuildMapperConfigurations<TSource, TDestination> _mapperConfigurationBuilder;

        private readonly IViewModelActivator _viewModelActivator;
        private readonly PSAccessServiceCreatorInterface _storeAccessCreator;
        private readonly IProvideAutoMappers _autoMapperService;
        private readonly ICreateWrapperTypes _wrapperTypeCreator;

        #endregion

        //private ICreateWrapperTypes _wrapperTypeCreator { get; }

        #region Constructor

        public SimplePropBagMapperBuilder
            (
            IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder,
            IViewModelActivator viewModelActivator,
            PSAccessServiceCreatorInterface storeAccessCreator,
            IProvideAutoMappers autoMapperService,
            ICreateWrapperTypes wrapperTypeCreator
            )
        {
            _mapperConfigurationBuilder = mapperConfigurationBuilder;
            _viewModelActivator = viewModelActivator;
            _storeAccessCreator = storeAccessCreator;
            _autoMapperService = autoMapperService;
            _wrapperTypeCreator = wrapperTypeCreator;
        }

        #endregion

        #region Public Properties

        public Func<IPropBagMapperKeyGen, IPropBagMapperGen> GenMapperCreator => GenerateMapperGen;

        #endregion

        #region Public Methods

        public IPropBagMapper<TSource, TDestination> GenerateMapper(IPropBagMapperKey<TSource, TDestination> mapperRequestKey)
        {
            IConfigurationProvider configProvider = _mapperConfigurationBuilder.GetNewConfiguration(mapperRequestKey);

            IMapper theMapper = configProvider.CreateMapper();

            IPropBagMapper <TSource, TDestination> result = new SimplePropBagMapper<TSource, TDestination>
                (
                mapperRequestKey,
                theMapper,
                _viewModelActivator,
                _storeAccessCreator,
                _autoMapperService,
                _wrapperTypeCreator
                );

            return result;
        }

        #endregion

        #region Private Methods

        private IPropBagMapperGen GenerateMapperGen(IPropBagMapperKeyGen mapRequestGen)
        {
            return (IPropBagMapperGen)GenerateMapper((IPropBagMapperKey<TSource, TDestination>)mapRequestGen);
        }

        //[System.Diagnostics.Conditional("DEBUG")]
        //private void CheckTypeToCreate(Type typeParameter, Type typeFromPropModel)
        //{
        //    if (typeParameter != typeFromPropModel)
        //    {
        //        throw new InvalidOperationException($"The type parameter: {typeParameter} does not match the PropModel's TypeToCreate: {typeFromPropModel}.");
        //    }
        //}

        #endregion
    }
}
