﻿using AutoMapper;
using System;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface ICreateMappingExpressions<TSource, TDestination> where TDestination : class, IPropBag
    {
        Action<IPropBagMapperKey<TSource, TDestination>, IMapperConfigurationExpression> ActionStep { get; }

        bool RequiresProxyType { get; }
    }
}
