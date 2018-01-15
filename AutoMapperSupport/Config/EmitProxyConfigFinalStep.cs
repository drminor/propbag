using AutoMapper;
using DRM.PropBag.ControlModel;
using System;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    /// <summary>
    /// For use with EmitProxy. EmitProxy produces "real" properties,
    /// so no custom AutoMapper support is required.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    public class EmitProxyConfigFinalStep<TSource, TDestination>
        : ICreateMappingExpressions<TSource, TDestination> where TDestination : class, IPropBag
    {
        public bool RequiresProxyType => true;

        public Action<IPropBagMapperKey<TSource, TDestination>,IMapperConfigurationExpression> ActionStep
        {
            get
            {
                return BuildEmitProxyConfig;
            }
        }

        public void BuildEmitProxyConfig(IPropBagMapperKey<TSource, TDestination> mapRequest, IMapperConfigurationExpression cfg)
        {
            IPropModel propModel = mapRequest.DestinationTypeDef.PropModel;

            Func<TDestination, TSource> regularInstanceCreator = mapRequest.MappingConfiguration.SourceConstructor;

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

