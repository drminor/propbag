﻿using DRM.TypeSafePropertyBag;
using Swhp.AutoMapperSupport;
using System;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    using PropModelType = IPropModel<String>;

    public interface IPropBagMapperService : ICachePropBagMappers
    {
        // Typed Submit 
        IPropBagMapperRequestKey<TSource, TDestination> SubmitPropBagMapperRequest<TSource, TDestination>
        (
            PropModelType propModel,
            string configPackageName,
            IHaveAMapperConfigurationStep configStarterForThisRequest = null,
            IPropFactory propFactory = null
        )
        where TDestination : class, IPropBag;

        //Typed Submit with a Mapping Configuration (IConfigureAMapper)
        IPropBagMapperRequestKey<TSource, TDestination> SubmitPropBagMapperRequest<TSource, TDestination>
        (
            PropModelType propModel,
            IConfigureAMapper<TSource, TDestination> mappingConfiguration,
            IHaveAMapperConfigurationStep configStarterForThisRequest = null,
            IPropFactory propFactory = null
        )
        where TDestination : class, IPropBag;

        // Gen Submit 
        IPropBagMapperRequestKeyGen SubmitPropBagMapperRequest
        (
            PropModelType propModel,
            Type sourceType,
            string configPackageName
        );

        // The following two methods are provided by ICachePropBagMappers

        ////    Typed Get Mapper
        //      IPropBagMapper<TSource, TDestination> GetPropBagMapper<TSource, TDestination>
        //      (
        //          IPropBagMapperKey<TSource, TDestination> mapperRequest
        //      )
        //      where TDestination : class, IPropBag;

        ////    Gen Get Mapper
        //      IPropBagMapperGen GetPropBagMapper(IPropBagMapperKeyGen mapperRequest);
    }
}
