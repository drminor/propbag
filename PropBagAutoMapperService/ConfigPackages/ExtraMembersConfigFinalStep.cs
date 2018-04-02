using AutoMapper;
using Swhp.AutoMapperSupport;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    using PropModelType = IPropModel<String>;

    public class ExtraMembersConfigFinalStep<TSource, TDestination>
        : IMapperConfigurationExpressionProvider<TSource, TDestination> //where TDestination : class, IPropBag
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
            //PropModelType propModel = mapRequest.DestinationTypeDef.UniqueRef as PropModelType;

            if (mapRequest.AutoMapperConfigDetails.ExtensionSourceId != SimplePropBagMapperService.PROP_BAG_MAPPER_CONFIG_DETAIL_EXTENSION_SOURCE_ID)
            {
                throw new InvalidOperationException("The ExtraMembersConfigFinalStep can only be used with a PropBagMapper.");
            }

            IPropBagMapperConfigDetails propBagMapperConfigDetails = mapRequest.AutoMapperConfigDetails as IPropBagMapperConfigDetails;
            if(propBagMapperConfigDetails == null)
            {
                throw new InvalidOperationException("The PropBagMapperConfigDetails is null.");
            }

            PropModelType propModel = propBagMapperConfigDetails.PropModel;


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

