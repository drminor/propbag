using System;
using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public interface IPropBagMapperBuilder<TSource, TDestination> where TDestination : class, IPropBag
    {
        Func<IPropBagMapperRequestKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> GenPropBagMapperCreator { get; }

        IPropBagMapper<TSource, TDestination> GeneratePropBagMapper(IPropBagMapperRequestKey<TSource, TDestination> mapperRequestKey, ViewModelFactoryInterface viewModelFactory);
    }
}
