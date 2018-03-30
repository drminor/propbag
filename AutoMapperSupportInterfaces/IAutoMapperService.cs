using System;
using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;
using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport
{
    using PropModelType = IPropModel<String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public interface IAutoMapperService : ICacheAutoMappers
    {
        //Typed Submit 
        IAutoMapperRequestKey<TSource, TDestination> SubmitRawAutoMapperRequest<TSource, TDestination>
        (
            PropModelType propModel,
            string configPackageName,
            IHaveAMapperConfigurationStep configStarterForThisRequest = null,
            IPropFactory propFactory = null
        )
        where TDestination : class, IPropBag;

        // Typed Get Mapper
        IMapper GetRawAutoMapper<TSource, TDestination>
        (
            IAutoMapperRequestKey<TSource, TDestination> mapperRequest
        )
        where TDestination : class, IPropBag;

        // Gen Submit 
        IAutoMapperRequestKeyGen SubmitRawAutoMapperRequest
        (
            PropModelType propModel,
            Type sourceType,
            string configPackageName
        );

        // Provided by ICacheAutoMappers
        //IMapper GetRawAutoMapper(IAutoMapperRequestKeyGen mapperRequest);
    }

}