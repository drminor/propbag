using System;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface ICreateMapperRequests
    {
        IPropBagMapperKey<TSource, TDestination> CreateMapperRequest<TSource, TDestination>
            (
            PropModel propModel,
            Type typeToWrap,
            IPropFactory propFactory,
            IMapperConfigurationStepGen configStarterForThisRequest
            ) where TDestination : class, IPropBag;
    }
}