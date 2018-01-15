using AutoMapper;
using DRM.PropBag.ControlModel;
using DRM.ViewModelTools;
using System;

using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class SimplePropBagMapperBuilder<TSource, TDestination> : IBuildPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        private IBuildMapperConfigurations<TSource, TDestination> MapperConfigurationBuilder { get; }
        private IViewModelActivator ViewModelActivator { get; }
        private PSAccessServiceCreatorInterface _storeAccessCreator;
        private ICreateWrapperTypes WrapperTypeCreator { get; }

        public SimplePropBagMapperBuilder(
            IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder,
            ICreateWrapperTypes wrapperTypeCreator,
            IViewModelActivator viewModelActivator,
            PSAccessServiceCreatorInterface storeAccessCreator
            )
        {
            MapperConfigurationBuilder = mapperConfigurationBuilder;
            ViewModelActivator = viewModelActivator;
            _storeAccessCreator = storeAccessCreator;
            WrapperTypeCreator = wrapperTypeCreator;
        }

        public Func<IPropBagMapperKeyGen, IPropBagMapperGen> GenMapperCreator => GenerateMapperGen;

        public IPropBagMapper<TSource, TDestination> GenerateMapper(IPropBagMapperKey<TSource, TDestination> mapRequest)
        {

            if (mapRequest.MappingConfiguration.RequiresWrappperTypeEmitServices)
            {
                // TODO: Is this really the responsibility of the PropBagMapperBuilder,
                // or can we hand this off to the IBuildMapperConfigurations interface?
                // Create the Proxy/Wrapper type if it does not already exist.
                IPropModel propModel = mapRequest.DestinationTypeDef.PropModel;

                // TODO: Can we avoid setting the NewWrapperType on the existing instance
                // of the mapRequest?
                Type newWrapperType = WrapperTypeCreator.GetWrapperType<TDestination>(propModel) as Type;
                mapRequest.DestinationTypeDef.NewWrapperType = newWrapperType;
            }

            IConfigurationProvider configProvider = MapperConfigurationBuilder.GetNewConfiguration
                (
                /*mapRequest.MappingConfiguration, */
                mapRequest: mapRequest/*, configStarter: null*/
                );

            IMapper theMapper = configProvider.CreateMapper();

            IPropBagMapper <TSource, TDestination> result 
                = new SimplePropBagMapper<TSource, TDestination>(mapRequest, theMapper, ViewModelActivator, _storeAccessCreator);

            return result;
        }

        public bool Validate(IPropBagMapperKey<TSource, TDestination> mapperRequestKey)
        {
            if(mapperRequestKey.MappingConfiguration.RequiresWrappperTypeEmitServices)
            {
                if (mapperRequestKey.SourceTypeDef.IsPropBag) throw new ApplicationException
                        ("The first type, TSource, is expected to be a regular, i.e., non-propbag-based type.");

                if (!mapperRequestKey.DestinationTypeDef.IsPropBag) throw new ApplicationException
                        ("The second type, TDestination, is expected to be a propbag-based type.");
            } 

            return true;
        }

        private IPropBagMapperGen GenerateMapperGen(IPropBagMapperKeyGen mapRequestGen)
        {
            return (IPropBagMapperGen)GenerateMapper((IPropBagMapperKey<TSource, TDestination>)mapRequestGen);
        }
    }
}
