using AutoMapper;
using Swhp.AutoMapperSupport;
using System;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    /// <summary>
    /// For use with EmitProxy. EmitProxy produces "real" properties,
    /// so no custom AutoMapper support is required.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    public class EmitProxyConfigFinalStep<TSource, TDestination>
        : IMapperConfigurationExpressionProvider<TSource, TDestination> //where TDestination : class, IPropBag
    {
        public bool RequiresProxyType => true;

        public Action<IAutoMapperRequestKey<TSource, TDestination>,IMapperConfigurationExpression> ActionStep
        {
            get
            {
                return BuildEmitProxyConfig;
            }
        }

        public void BuildEmitProxyConfig(IAutoMapperRequestKey<TSource, TDestination> mapRequest, IMapperConfigurationExpression cfg)
        {
            //PropModelType propModel = mapRequest.DestinationTypeDef.PropModel;

            Func<TDestination, TSource> regularInstanceCreator = mapRequest.MappingConfiguration.SourceConstructor;

            //Type newWrapperType = mapRequest.DestinationTypeDef.NewEmittedType;

            Type newWrapperType = mapRequest.DestinationTypeDef.RunTimeType;


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

