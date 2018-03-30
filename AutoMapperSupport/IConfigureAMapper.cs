using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IConfigureAMapper<TSource, TDestination> : IConfigureAMapperGen where TDestination : class, IPropBag
    {
        ICreateMappingExpressions<TSource, TDestination> FinalConfigActionProvider { get; }
        Func<TDestination, TSource> SourceConstructor { get; }
        Func<TSource, TDestination> DestinationConstructor { get; }
    }

    public interface IConfigureAMapperGen
    {
        bool SupportsMapFrom { get; }
        bool RequiresWrappperTypeEmitServices { get; }
        IReadOnlyCollection<IHaveAMapperConfigurationStep> ConfigurationSteps { get; }

        //IConfigurationProvider GetConfigurationProviderGen();
    }
}
