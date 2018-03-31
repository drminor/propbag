using System;
using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;

namespace DRM.TypeSafePropertyBag
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public interface IBuildPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        Func<IPropBagMapperRequestKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> GenPropBagMapperCreator { get; }

        IPropBagMapper<TSource, TDestination> GeneratePropBagMapper(IPropBagMapperRequestKey<TSource, TDestination> mapperRequestKey, ViewModelFactoryInterface viewModelFactory);
    }
}
