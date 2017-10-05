using AutoMapper;
using System;
using System.Collections.Generic;

namespace DRM.PropBag.AutoMapperSupport
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

    }

    public interface IPropBagMapperGen
    {
        Type SourceType { get; }
        Type DestinationType { get; }

        bool SupportsMapFrom { get; }
        IMapperConfigurationExpression Configure(IMapperConfigurationExpression cfg);
        IMapper Mapper { get; set; }
    }

}
