﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DRM.ViewModelTools;

namespace DRM.PropBag.AutoMapperSupport
{
    public class SimplePropBagMapper<TSource, TDestination> : AbstractPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        public SimplePropBagMapper(IPropBagMapperKey<TSource, TDestination> mapRequest,
            IMapper mapper, IViewModelActivator<TDestination> vmActivator)
            : base(mapRequest, mapper, vmActivator)
        {
        }
    }
}