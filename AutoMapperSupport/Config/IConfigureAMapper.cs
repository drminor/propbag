using System;
using System.Collections.Generic;

namespace Swhp.AutoMapperSupport
{
    public interface IConfigureAMapper<TSource, TDestination> : IConfigureAMapperGen //where TDestination : class, IPropBag
    {
        IMapperConfigurationExpressionProvider<TSource, TDestination> FinalConfigActionProvider { get; }
        Func<TDestination, TSource> SourceConstructor { get; }
        Func<TSource, TDestination> DestinationConstructor { get; }
    }

    public interface IConfigureAMapperGen
    {
        string PackageName { get; }
        bool SupportsMapFrom { get; }
        bool RequiresWrappperTypeEmitServices { get; }
        IReadOnlyCollection<IHaveAMapperConfigurationStep> ConfigurationSteps { get; }
    }
}
