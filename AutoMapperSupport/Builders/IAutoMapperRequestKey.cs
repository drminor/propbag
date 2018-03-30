using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IAutoMapperRequestKey<TSource, TDestination> : IAutoMapperRequestKeyGen //where TDestination: class, IPropBag
    {
        IMapTypeDefinition<TSource> SourceTypeDef { get; }
        IMapTypeDefinition<TDestination> DestinationTypeDef { get; }

        //Func<TDestination, TSource> SourceConstructor { get; }
        //Func<TSource, TDestination> DestinationConstructor { get; }

        IConfigureAMapper<TSource, TDestination> MappingConfiguration { get; }
    }
}
