using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IPropBagMapperKey<TSource, TDestination> : IPropBagMapperKeyGen where TDestination: class, IPropBag
    {
        IMapTypeDefinition<TSource> SourceTypeDef { get; }
        IMapTypeDefinition<TDestination> DestinationTypeDef { get; }

        //Func<TDestination, TSource> SourceConstructor { get; }
        //Func<TSource, TDestination> DestinationConstructor { get; }

        //Func<IPropBagMapperKeyGen, IPropBagMapperGen> MapperCreator { get; }
        IConfigureAMapper<TSource, TDestination> MappingConfiguration { get; }
    }

    public interface IPropBagMapperKeyGen
    {
        IMapTypeDefinitionGen SourceTypeGenDef { get; }
        IMapTypeDefinitionGen DestinationTypeGenDef { get; }

        IPropBagMapperGen CreateMapper();
    }
}
