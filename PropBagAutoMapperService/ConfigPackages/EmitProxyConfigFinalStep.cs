using AutoMapper;
using DRM.PropBag.AutoMapperSupport;
using System;

namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    /// For use with EmitProxy. EmitProxy produces "real" properties,
    /// so no custom AutoMapper support is required.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    public class EmitProxyConfigFinalStep<TSource, TDestination>
        : ICreateMappingExpressions<TSource, TDestination> //where TDestination : class, IPropBag
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

            Type newWrapperType = mapRequest.DestinationTypeDef.NewEmittedType;

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

