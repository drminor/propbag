using System;
using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;
using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public interface IBuildPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        Func<IPropBagMapperKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> GenPropBagMapperCreator { get; }

        Func<IPropBagMapperKeyGen, IMapper> GenRawAutoMapperCreator { get; }

        // Could easily be re-added
        //IPropBagMapper<TSource, TDestination> GeneratePropBagMapper(IPropBagMapperKey<TSource, TDestination> mapperRequestKey, ViewModelFactoryInterface viewModelFactory);

        // Could easily be re-added
        //IMapper GenerateRawAutoMapper(IPropBagMapperKey<TSource, TDestination> mapperRequestKey);
    }
}
