using AutoMapper;
using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropBagMapper<TSource, TDestination>
        : IPropBagMapperGen
    {
        TDestination MapToDestination(TSource s);
        TDestination MapToDestination(TSource s, TDestination d);

        TSource MapToSource(TDestination d);
        TSource MapToSource(TDestination d, TSource s);

        IEnumerable<TDestination> MapToDestination(IEnumerable<TSource> listOfSources);
        IEnumerable<TSource> MapToSource(IEnumerable<TDestination> listOfDestinations);

        TDestination GetNewDestination();

        Type TargetRunTimeType { get; }
    }

    public interface IPropBagMapperGen : IDisposable
    {
        Type SourceType { get; }
        Type DestinationType { get; }

        bool SupportsMapFrom { get; }
        //IMapperConfigurationExpression Configure(IMapperConfigurationExpression cfg);
        IMapper Mapper { get; }
    }

}
