
namespace Swhp.AutoMapperSupport
{
    public interface IAutoMapperRequestKey<TSource, TDestination> : IAutoMapperRequestKeyGen
    {
        IMapTypeDefinition SourceTypeDef { get; }
        IMapTypeDefinition DestinationTypeDef { get; }

        //Func<TDestination, TSource> SourceConstructor { get; }
        //Func<TSource, TDestination> DestinationConstructor { get; }

        IConfigureAMapper<TSource, TDestination> MappingConfiguration { get; }
    }
}
