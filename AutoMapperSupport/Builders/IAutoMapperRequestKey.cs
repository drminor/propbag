
namespace Swhp.AutoMapperSupport
{
    public interface IAutoMapperRequestKey<TSource, TDestination> : IAutoMapperRequestKeyGen
    {
        IConfigureAMapper<TSource, TDestination> MappingConfiguration { get; }

        // -----------------
        //
        //    Below are Convenience Properties that source from the MappingConfiguration
        //
        // -----------------

        //Func<TDestination, TSource> SourceConstructor { get; }
        //Func<TSource, TDestination> DestinationConstructor { get; }
    }
}
