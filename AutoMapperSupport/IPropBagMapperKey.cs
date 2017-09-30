using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.AutoMapperSupport
{
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
