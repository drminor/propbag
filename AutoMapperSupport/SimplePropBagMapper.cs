using AutoMapper;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public class SimplePropBagMapper<TSource, TDestination> : AbstractPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        //public SimplePropBagMapper
        //    (
        //        IPropBagMapperKey<TSource, TDestination> mapRequest,
        //        IMapper mapper,
        //        ViewModelActivatorInterface vmActivator,
        //        PSAccessServiceCreatorInterface storeAccessCreator,
        //        IProvideAutoMappers autoMapperService,
        //        ICreateWrapperTypes wrapperTypeCreator
        //    )
        //    : base
        //    (
        //        mapRequest,
        //        mapper,
        //        vmActivator,
        //        storeAccessCreator,
        //        autoMapperService,
        //        wrapperTypeCreator
        //    )
        //{
        //}

        public SimplePropBagMapper
        (
            IPropBagMapperKey<TSource, TDestination> mapRequest,
            IMapper mapper,
            ViewModelFactoryInterface viewModelFactory
        )
        : base
        (
            mapRequest,
            mapper,
            viewModelFactory
        )
        {
        }

    }
}
