using System;
using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;
using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;
    public interface IBuildPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        // GeneratePropBagMapper does exist on each concreate implementation, but is not needed, so its been commented out.
        //IPropBagMapper<TSource, TDestination> GeneratePropBagMapper(IPropBagMapperKey<TSource, TDestination> mapperRequestKey, ViewModelFactoryInterface viewModelFactory);

        Func<IPropBagMapperKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> GenMapperCreator { get; }

        Func<IPropBagMapperKeyGen, IMapper> RawAutoMapperCreator { get; }
    }
}
