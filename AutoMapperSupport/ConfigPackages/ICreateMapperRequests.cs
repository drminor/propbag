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
            IHaveAMapperConfigurationStep configStarterForThisRequest,
            IPropFactory propFactory = null
            ) where TDestination : class, IPropBag;
    }
}