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
        private IBuildMapperConfigurations<TSource, TDestination> _mapperConfigurationBuilder { get; }

        private IViewModelActivator _viewModelActivator { get; }
        private PSAccessServiceCreatorInterface _storeAccessCreator;
        private IProvideAutoMappers _autoMapperService;

        private ICreateWrapperTypes _wrapperTypeCreator { get; }

        public SimplePropBagMapperBuilder
            (
            IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder,
            ICreateWrapperTypes wrapperTypeCreator,
            IViewModelActivator viewModelActivator,
            PSAccessServiceCreatorInterface storeAccessCreator,
            IProvideAutoMappers autoMapperService
            )
        {
            _mapperConfigurationBuilder = mapperConfigurationBuilder;
            _viewModelActivator = viewModelActivator;
            _storeAccessCreator = storeAccessCreator;
            _autoMapperService = autoMapperService;
            _wrapperTypeCreator = wrapperTypeCreator;
        }

        public Func<IPropBagMapperKeyGen, IPropBagMapperGen> GenMapperCreator => GenerateMapperGen;

        private IPropBagMapperGen GenerateMapperGen(IPropBagMapperKeyGen mapRequestGen)
        {
            return (IPropBagMapperGen)GenerateMapper((IPropBagMapperKey<TSource, TDestination>)mapRequestGen);
        }

        public IPropBagMapper<TSource, TDestination> GenerateMapper(IPropBagMapperKey<TSource, TDestination> mapRequest)
        {
            // TODO: MM_Work: Remove this code, the newWrapperType should have been created by this point.

            //if (mapRequest.MappingConfiguration.RequiresWrappperTypeEmitServices)
            //{
            //    // TODO: Is this really the responsibility of the PropBagMapperBuilder,
            //    // or can we hand this off to the IBuildMapperConfigurations interface?

            //    // Create the Proxy/Wrapper type if it does not already exist.
            //    PropModelType propModel = mapRequest.DestinationTypeDef.PropModel;

            //    // TODO: Can we avoid setting the NewWrapperType on the existing instance of the mapRequest?

            //    //Type newWrapperType = WrapperTypeCreator.GetWrapperType<TDestination>(propModel) as Type;

            //    //CheckTypeToCreate(typeof(TDestination), propModel.TypeToCreate);
            //    Type newWrapperType = _wrapperTypeCreator.GetWrapperType(propModel, propModel.TypeToCreate) as Type;

            //    mapRequest.DestinationTypeDef.NewWrapperType = newWrapperType;
            //}

            //mapRequest.DestinationTypeDef.NewWrapperType = mapRequest.DestinationTypeDef.PropModel.NewEmittedType;

            IConfigurationProvider configProvider = _mapperConfigurationBuilder.GetNewConfiguration(mapRequest);

            IMapper theMapper = configProvider.CreateMapper();

            IPropBagMapper <TSource, TDestination> result = new SimplePropBagMapper<TSource, TDestination>
                (
                mapRequest,
                theMapper,
                _viewModelActivator,
                _storeAccessCreator,
                _autoMapperService
                );

            return result;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckTypeToCreate(Type typeParameter, Type typeFromPropModel)
        {
            if(typeParameter != typeFromPropModel)
            {
                throw new InvalidOperationException($"The type parameter: {typeParameter} does not match the PropModel's TypeToCreate: {typeFromPropModel}.");
            }
        }

        public bool Validate(IPropBagMapperKey<TSource, TDestination> mapperRequestKey)
        {
            if(mapperRequestKey.MappingConfiguration.RequiresWrappperTypeEmitServices)
            {
                if (mapperRequestKey.SourceTypeDef.IsPropBag)
                    throw new ApplicationException("The first type, TSource, is expected to be a regular, i.e., non-propbag-based type.");

                if (!mapperRequestKey.DestinationTypeDef.IsPropBag)
                    throw new ApplicationException("The second type, TDestination, is expected to be a propbag-based type.");
            } 

            return true;
        }

    }
}
