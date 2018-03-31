using AutoMapper;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.TypeSafePropertyBag
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public class SimplePropBagMapper<TSource, TDestination> : AbstractPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        public SimplePropBagMapper
        (
            IPropBagMapperRequestKey<TSource, TDestination> mapRequest,
            IMapper mapper,
            ViewModelFactoryInterface viewModelFactory,
            IPropBagMapperService propBagMapperService
        )
        : base
            (
            mapRequest,
            mapper,
            viewModelFactory,
            propBagMapperService
            )
        {
        }

    }
}
