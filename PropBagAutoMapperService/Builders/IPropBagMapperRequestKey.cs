using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public interface IPropBagMapperRequestKey<TSource, TDestination> : IPropBagMapperRequestKeyGen where TDestination: class, IPropBag
    {
        IAutoMapperRequestKey<TSource, TDestination> AutoMapperRequestKey { get; }

        //IMapTypeDefinition SourceTypeDef { get; }
        //IMapTypeDefinition DestinationTypeDef { get; }

        //Func<TDestination, TSource> SourceConstructor { get; }
        //Func<TSource, TDestination> DestinationConstructor { get; }

        IConfigureAMapper<TSource, TDestination> MappingConfiguration { get; }

        IPropBagMapper<TSource, TDestination> GeneratePropBagMapper
        (
            IPropBagMapperRequestKey<TSource, TDestination> mapperRequestKey,
            ViewModelFactoryInterface viewModelFactory
        );

    }
}
