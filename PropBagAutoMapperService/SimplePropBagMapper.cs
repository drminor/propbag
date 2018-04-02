using AutoMapper;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    using PropModelType = IPropModel<String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public class SimplePropBagMapper<TSource, TDestination> : AbstractPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        public SimplePropBagMapper
            (
            PropModelType propModel,
            IMapper mapper,
            ViewModelFactoryInterface viewModelFactory,
            IPropBagMapperService propBagMapperService,
            IPropBagMapperRequestKey<TSource, TDestination> mapRequest
            )
            : base
            (
            propModel,
            mapper,
            viewModelFactory,
            propBagMapperService,
            mapRequest
            )
        {
        }

    }
}
