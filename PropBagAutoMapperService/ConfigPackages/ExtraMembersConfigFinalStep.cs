﻿using AutoMapper;
using DRM.PropBag.AutoMapperSupport;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    using PropModelType = IPropModel<String>;

    public class ExtraMembersConfigFinalStep<TSource, TDestination>
        : ICreateMappingExpressions<TSource, TDestination> //where TDestination : class, IPropBag
    {
        public bool RequiresProxyType => false;

        public Action<IAutoMapperRequestKey<TSource, TDestination>, IMapperConfigurationExpression> ActionStep
        {
            get
            {
                return BuildExtraMemberConfig;
            }
        }

        public void BuildExtraMemberConfig(IAutoMapperRequestKey<TSource, TDestination> mapRequest, IMapperConfigurationExpression cfg)
        {
            PropModelType propModel = mapRequest.DestinationTypeDef.PropModel as PropModelType;

            // TODO: Create an interface for the ExtraMembersProvider and then create
            // property so that this value can be set after construction, but before calling BuildExtraMemberConfig.
            IEnumerable<MemberInfo> extraMembers = new ExtraMembersProvider().GetExtraMembers(propModel);

            Func<TDestination, TSource> regularInstanceCreator = mapRequest.MappingConfiguration.SourceConstructor;

            //cfg.IncludeExtraMembersForType(typeof(Destination), extraMembers);

            cfg
                .CreateMap<TSource, TDestination>()
                //.RegisterExtraMembers(cfg)
                .AddExtraDestintionMembers(extraMembers)
            //.ForMember("Item1", opt => opt.Condition(srcA => srcA.Item1 != "AA"))
            ;

            //Func<Destination, bool> cond = (s => "x" != (string)s.GetIt("Item1", typeof(string)));

            cfg
                .CreateMap<TDestination, TSource>()
                .AddExtraSourceMembers(extraMembers)
            //.ForMember("Item1", opt => opt.Condition(cond))
            ;
        }
    }
}
