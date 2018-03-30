using System;
using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public interface IBuildPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        Func<IPropBagMapperKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> GenPropBagMapperCreator { get; }

        IPropBagMapper<TSource, TDestination> GeneratePropBagMapper(IPropBagMapperKey<TSource, TDestination> mapperRequestKey, ViewModelFactoryInterface viewModelFactory);
    }
}
