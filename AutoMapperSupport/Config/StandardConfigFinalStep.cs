﻿using AutoMapper;
using DRM.PropBag.ControlModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DRM.PropBag.AutoMapperSupport
{
    public class StandardConfigFinalStep<TSource, TDestination> : IMapperConfigurationStep<TSource, TDestination>
    {
        public Action<IPropBagMapperKey<TSource, TDestination>,IMapperConfigurationExpression> ConfigurationStep
        {
            get
            {
                return BuildStandardConfig;
            }
        }

        public void BuildStandardConfig(IPropBagMapperKey<TSource, TDestination> mapRequest, IMapperConfigurationExpression cfg)
        {
            PropModel propModel = mapRequest.DestinationTypeDef.PropModel;

            Func<TDestination, TSource> regularInstanceCreator = mapRequest.SourceConstructor;

            Type newWrapperType = mapRequest.DestinationTypeDef.NewWrapperType;

            cfg.CreateMap(typeof(TSource), newWrapperType);

            if (regularInstanceCreator != null)
            {
                cfg.CreateMap(newWrapperType, typeof(TSource)); //.ConstructUsing(RegularInstanceCreator);
            }
            else
            {
                cfg.CreateMap(newWrapperType, typeof(TSource));
            }
        }

    }
}
