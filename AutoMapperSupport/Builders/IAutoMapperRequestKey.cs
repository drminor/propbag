using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IAutoMapperRequestKey<TSource, TDestination> : IAutoMapperRequestKeyGen //where TDestination: class, IPropBag
    {
        IMapTypeDefinition SourceTypeDef { get; }
        IMapTypeDefinition DestinationTypeDef { get; }

        //Func<TDestination, TSource> SourceConstructor { get; }
        //Func<TSource, TDestination> DestinationConstructor { get; }

        IConfigureAMapper<TSource, TDestination> MappingConfiguration { get; }
    }
}
