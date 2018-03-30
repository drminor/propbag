using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public interface IPropBagMapperKey<TSource, TDestination> : IPropBagMapperKeyGen where TDestination: class, IPropBag
    {
        IAutoMapperRequestKey<TSource, TDestination> AutoMapperRequestKey { get; }

        IMapTypeDefinition<TSource> SourceTypeDef { get; }
        IMapTypeDefinition<TDestination> DestinationTypeDef { get; }

        //Func<TDestination, TSource> SourceConstructor { get; }
        //Func<TSource, TDestination> DestinationConstructor { get; }

        IConfigureAMapper<TSource, TDestination> MappingConfiguration { get; }

        IPropBagMapper<TSource, TDestination> GeneratePropBagMapper
        (
            IPropBagMapperKey<TSource, TDestination> mapperRequestKey,
            ViewModelFactoryInterface viewModelFactory
        );

    }
}
