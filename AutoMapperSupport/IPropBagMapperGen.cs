using AutoMapper;
using System;

using DRM.PropBag.ControlModel;


namespace DRM.PropBag.AutoMapperSupport
{
    public interface IPropBagMapper<TSource, TDestination>
        : IPropBagMapperGen
    {
        Type SourceType { get; }
        Type DestinationType { get; }

        TDestination MapFrom(TSource s);
        TDestination MapFrom(TSource s, TDestination d);

        TSource MapTo(TDestination d);
        TSource MapTo(TDestination d, TSource s);
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
        bool UseCustom { get; }
        IMapTypeDefinitionGen SourceTypeGenDef { get; }
        IMapTypeDefinitionGen DestinationTypeGenDef { get; }

        Func<IPropBagMapperKeyGen, IPropBagMapperGen> CreateMapper { get; }
    }

    public interface IMapTypeDefinition<T> : IMapTypeDefinitionGen
    {
    }

    public interface IMapTypeDefinitionGen
    {
        Type Type { get; }

        bool IsPropBag { get; }
        PropModel PropModel { get; }
        Type BaseType { get; }
    }

}
