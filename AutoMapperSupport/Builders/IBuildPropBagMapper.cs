using System;
using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;
    public interface IBuildPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        IPropBagMapper<TSource, TDestination> GenerateMapper(IPropBagMapperKey<TSource, TDestination> mapperRequestKey, ViewModelFactoryInterface viewModelFactory);

        Func<IPropBagMapperKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> GenMapperCreator { get; }
    }
}
