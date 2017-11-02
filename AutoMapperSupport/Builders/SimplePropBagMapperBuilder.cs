using AutoMapper;
using DRM.PropBag.ControlModel;
using DRM.ViewModelTools;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public class SimplePropBagMapperBuilder<TSource, TDestination> : IBuildPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        private IBuildMapperConfigurations<TSource, TDestination> MapperConfigurationBuilder { get; }
        private IViewModelActivator ViewModelActivator { get; }
        private ICreateWrapperType WrapperTypeCreator { get; }

        public SimplePropBagMapperBuilder(
            IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder,
            ICreateWrapperType wrapperTypeCreator,
            IViewModelActivator viewModelActivator)
        {
            MapperConfigurationBuilder = mapperConfigurationBuilder;
            ViewModelActivator = viewModelActivator;
            WrapperTypeCreator = wrapperTypeCreator;
        }

        public Func<IPropBagMapperKeyGen, IPropBagMapperGen> GenMapperCreator => GenerateMapperGen;

        public IPropBagMapper<TSource, TDestination> GenerateMapper(IPropBagMapperKey<TSource, TDestination> mapRequest)
        {
            //TODO: See if we can support both Source and Destination PropBag-Based types.
            if (mapRequest.MappingConfiguration.RequiresWrappperTypeEmitServices)
            {
                // Create the Proxy/Wrapper type if it does not already exist.
                PropModel propModel = mapRequest.DestinationTypeDef.PropModel;

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
                = new SimplePropBagMapper<TSource, TDestination>(mapRequest, theMapper, ViewModelActivator);

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
