using AutoMapper;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public class SimplePropBagMapper<TSource, TDestination> : AbstractPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        public SimplePropBagMapper
        (
            IPropBagMapperKey<TSource, TDestination> mapRequest,
            IMapper mapper,
            ViewModelFactoryInterface viewModelFactory,
            IAutoMapperService autoMapperService
        )
        : base
            (
            mapRequest,
            mapper,
            viewModelFactory,
            autoMapperService
            )
        {
        }

    }
}
