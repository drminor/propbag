using AutoMapper;
using DRM.PropBag.ControlModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DRM.PropBag.AutoMapperSupport
{
    public class ExtraMembersConfigFinalStep<TSource, TDestination>
        : IMapperConfigurationStep<TSource, TDestination> where TDestination : class, IPropBag
    {
        public Action<IPropBagMapperKey<TSource, TDestination>, IMapperConfigurationExpression> ConfigurationStep
        {
            get
            {
                return BuildExtraMemberConfig;
            }
        }

        public void BuildExtraMemberConfig(IPropBagMapperKey<TSource, TDestination> mapRequest, IMapperConfigurationExpression cfg)
        {
            PropModel propModel = mapRequest.DestinationTypeDef.PropModel;

            IEnumerable<MemberInfo> extraMembers = new ExtraMembersProvider().GetExtraMembers(propModel);

            Func<TDestination, TSource> regularInstanceCreator = mapRequest.SourceConstructor;

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

