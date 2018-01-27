﻿using AutoMapper;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class SimplePropBagMapper<TSource, TDestination> : AbstractPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        public SimplePropBagMapper
            (
                IPropBagMapperKey<TSource, TDestination> mapRequest,
                IMapper mapper,
                IViewModelActivator vmActivator,
                PSAccessServiceCreatorInterface storeAccessCreator
            )
            : base
            (
                mapRequest,
                mapper,
                vmActivator,
                storeAccessCreator
            )
        {
        }
    }
}
