using AutoMapper;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IPropBagMapper<TSource, TDestination>
        : IPropBagMapperGen
    {
        Type SourceType { get; }
        Type DestinationType { get; }

        TDestination MapToDestination(TSource s);
        TDestination MapToDestination(TSource s, TDestination d);

        TSource MapToSource(TDestination d);
        TSource MapToSource(TDestination d, TSource s);
    }

    public interface IPropBagMapperGen
    {
        bool SupportsMapFrom { get; }
        IMapperConfigurationExpression Configure(IMapperConfigurationExpression cfg);
        IMapper Mapper { get; set; }
    }

    public interface IPropBagMapperKey<TSource, TDestination> : IPropBagMapperKeyGen
    {
        IMapTypeDefinition<TSource> SourceTypeDef { get; }
        IMapTypeDefinition<TDestination> DestinationTypeDef { get; }
        Func<TDestination, TSource> ConstructSourceFunc { get; }
        Func<TSource, TDestination> ConstructDestinationFunc { get; }
    }

    public interface IPropBagMapperKeyGen
    {
        PropBagMappingStrategyEnum MappingStrategy { get; }
        IMapTypeDefinitionGen SourceTypeGenDef { get; }
        IMapTypeDefinitionGen DestinationTypeGenDef { get; }

        Func<IPropBagMapperKeyGen, IPropBagMapperGen> CreateMapper { get; }
    }

}
