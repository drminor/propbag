using AutoMapper;
using DRM.ViewModelTools;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public class SimplePropBagMapperBuilder<TSource, TDestination> : IBuildPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        private IConfigureAMapper<TSource, TDestination> MapperConfiguration { get; }

        private IViewModelActivator VmActivator { get; }

        public SimplePropBagMapperBuilder(IConfigureAMapper<TSource, TDestination> mapperConfiguration,
            IViewModelActivator vmActivator)
        {
            MapperConfiguration = mapperConfiguration;
            VmActivator = vmActivator;
        }

        public Func<IPropBagMapperKeyGen, IPropBagMapperGen> GenMapperCreator => GenerateMapperGen;

        public IPropBagMapper<TSource, TDestination> GenerateMapper(IPropBagMapperKey<TSource, TDestination> mapRequest)
        {
            IConfigurationProvider configProvider = MapperConfiguration.GetConfigurationProvider(mapRequest);

            IMapper autoMapper = configProvider.CreateMapper();

            IPropBagMapper <TSource, TDestination> result 
                = new SimplePropBagMapper<TSource, TDestination>(mapRequest, autoMapper, VmActivator);

            return result;
        }

        private IPropBagMapperGen GenerateMapperGen(IPropBagMapperKeyGen mapRequestGen)
        {
            return (IPropBagMapperGen)GenerateMapper((IPropBagMapperKey<TSource, TDestination>)mapRequestGen);
        }
    }
}
